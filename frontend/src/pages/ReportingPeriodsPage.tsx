import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Popconfirm, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createReportingPeriod, deleteReportingPeriod, getReportingPeriods, Periodicity, ReportingPeriod, updateReportingPeriod } from '../api/client';
import DetailDrawer from '../components/DetailDrawer';

const periodicityLabels: Record<Periodicity, string> = { 1: 'Hàng tháng', 2: 'Hàng quý', 3: 'Hàng năm' };
const options = Object.entries(periodicityLabels).map(([v, label]) => ({ value: Number(v), label }));

export default function ReportingPeriodsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [detail, setDetail] = useState<ReportingPeriod | null>(null);
  const [editing, setEditing] = useState<ReportingPeriod | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['reporting-periods', page, pageSize, keyword],
    queryFn: () => getReportingPeriods({ page, pageSize, keyword }),
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['reporting-periods'] });
  const create = useMutation({
    mutationFn: createReportingPeriod,
    onSuccess: () => { message.success('Đã tạo kỳ báo cáo'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string; name: string; periodicity: Periodicity }) => updateReportingPeriod(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteReportingPeriod,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: ReportingPeriod) {
    setEditing(r);
    form.setFieldsValue({ code: r.code, name: r.name, periodicity: r.periodicity });
    setOpen(true);
  }
  function submit(v: { code: string; name: string; periodicity: Periodicity }) {
    if (editing) update.mutate({ id: editing.id, name: v.name, periodicity: v.periodicity });
    else create.mutate(v);
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
      </Space>
      <Table<ReportingPeriod>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 150 },
          { title: 'Tên kỳ báo cáo', dataIndex: 'name' },
          { title: 'Chu kỳ', dataIndex: 'periodicity', width: 160, render: (p: Periodicity) => <Tag>{periodicityLabels[p]}</Tag> },
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
      <Modal title={editing ? 'Sửa kỳ báo cáo' : 'Thêm kỳ báo cáo'} open={open} onCancel={close}
        onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} disabled={!!editing} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="periodicity" label="Chu kỳ" rules={[{ required: true }]} initialValue={1}>
            <Select options={options} />
          </Form.Item>
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết kỳ báo cáo"
        items={detail ? [
          { label: 'Mã', value: detail.code },
          { label: 'Tên kỳ báo cáo', value: detail.name },
          { label: 'Chu kỳ', value: periodicityLabels[detail.periodicity] },
          { label: 'Trạng thái', value: detail.isActive ? 'Hoạt động' : 'Ngưng' },
        ] : []} />
    </Space>
  );
}
