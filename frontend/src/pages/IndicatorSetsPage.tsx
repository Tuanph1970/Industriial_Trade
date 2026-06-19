import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Popconfirm, Select, Space, Table } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createIndicatorSet, deleteIndicatorSet, getIndicators, getIndicatorSets, IndicatorSet, updateIndicatorSet } from '../api/client';
import DetailDrawer from '../components/DetailDrawer';

export default function IndicatorSetsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [detail, setDetail] = useState<IndicatorSet | null>(null);
  const [editing, setEditing] = useState<IndicatorSet | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['indicator-sets', page, pageSize, keyword],
    queryFn: () => getIndicatorSets({ page, pageSize, keyword }),
  });
  const { data: indicators } = useQuery({ queryKey: ['indicators', 'all'], queryFn: () => getIndicators({ page: 1, pageSize: 200 }) });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['indicator-sets'] });
  const create = useMutation({
    mutationFn: createIndicatorSet,
    onSuccess: () => { message.success('Đã tạo bộ chỉ tiêu'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string; name: string; description?: string; indicatorIds: string[] }) => updateIndicatorSet(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteIndicatorSet,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: IndicatorSet) {
    setEditing(r);
    form.setFieldsValue({ code: r.code, name: r.name, description: r.description, indicatorIds: r.indicatorIds });
    setOpen(true);
  }
  function submit(v: { code: string; name: string; description?: string; indicatorIds?: string[] }) {
    if (editing) update.mutate({ id: editing.id, name: v.name, description: v.description, indicatorIds: v.indicatorIds ?? [] });
    else create.mutate({ ...v, indicatorIds: v.indicatorIds ?? [] });
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
      </Space>
      <Table<IndicatorSet>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 150 },
          { title: 'Tên bộ chỉ tiêu', dataIndex: 'name' },
          { title: 'Số chỉ tiêu', dataIndex: 'indicatorIds', width: 130, render: (ids: string[]) => ids.length },
          { title: 'Thao tác', width: 180, render: (_, r) => (
            <Space>
              <a onClick={() => setDetail(r)}>Xem</a>
              <a onClick={() => openEdit(r)}>Sửa</a>
              <Popconfirm title="Xoá?" okText="Xoá" cancelText="Huỷ" onConfirm={() => remove.mutate(r.id)}>
                <a style={{ color: '#cf1322' }}>Xoá</a>
              </Popconfirm>
            </Space>) },
        ]}
      />
      <Modal title={editing ? 'Sửa bộ chỉ tiêu' : 'Thêm bộ chỉ tiêu'} open={open} onCancel={close}
        onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} disabled={!!editing} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="description" label="Mô tả"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="indicatorIds" label="Chỉ tiêu thành viên" initialValue={[]}>
            <Select mode="multiple" allowClear showSearch optionFilterProp="label"
              options={indicators?.items.map((i) => ({ value: i.id, label: `${i.name} (${i.code})` }))} />
          </Form.Item>
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết bộ chỉ tiêu"
        items={detail ? [
          { label: 'Mã', value: detail.code },
          { label: 'Tên', value: detail.name },
          { label: 'Mô tả', value: detail.description },
          { label: 'Chỉ tiêu thành viên', value: detail.indicatorIds.length
            ? <ol style={{ margin: 0, paddingLeft: 18 }}>
                {detail.indicatorIds.map((id) => <li key={id}>{indicators?.items.find((i) => i.id === id)?.name ?? id}</li>)}
              </ol>
            : null },
          { label: 'Trạng thái', value: detail.isActive ? 'Hoạt động' : 'Ngưng' },
        ] : []} />
    </Space>
  );
}
