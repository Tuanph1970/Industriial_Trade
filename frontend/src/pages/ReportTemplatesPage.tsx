import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Popconfirm, Select, Space, Table } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createReportTemplate, deleteReportTemplate, getIndicators, getReportTemplates, ReportTemplate, updateReportTemplate } from '../api/client';
import DetailDrawer from '../components/DetailDrawer';

export default function ReportTemplatesPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [detail, setDetail] = useState<ReportTemplate | null>(null);
  const [editing, setEditing] = useState<ReportTemplate | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['report-templates', page, pageSize, keyword],
    queryFn: () => getReportTemplates({ page, pageSize, keyword }),
  });
  const { data: indicators } = useQuery({ queryKey: ['indicators', 'all'], queryFn: () => getIndicators({ page: 1, pageSize: 200 }) });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['report-templates'] });
  const create = useMutation({
    mutationFn: createReportTemplate,
    onSuccess: () => { message.success('Đã tạo biểu mẫu'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string } & Parameters<typeof updateReportTemplate>[1]) => updateReportTemplate(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteReportTemplate,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: ReportTemplate) {
    setEditing(r);
    form.setFieldsValue({ code: r.code, name: r.name, description: r.description,
      indicatorIds: r.lines.map((l) => l.indicatorId) });
    setOpen(true);
  }

  // Build template lines from the chosen indicators (label = indicator name, ordered by selection).
  const onFinish = (v: { code: string; name: string; description?: string; indicatorIds: string[] }) => {
    const lines = (v.indicatorIds ?? []).map((id, idx) => ({
      indicatorId: id,
      label: indicators?.items.find((i) => i.id === id)?.name ?? id,
      rowOrder: idx + 1,
    }));
    if (editing) update.mutate({ id: editing.id, name: v.name, description: v.description, lines });
    else create.mutate({ code: v.code, name: v.name, description: v.description, lines });
  };

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
      </Space>
      <Table<ReportTemplate>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        expandable={{ expandedRowRender: (r) => <ol>{r.lines.map((l) => <li key={l.rowOrder}>{l.label}</li>)}</ol> }}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 150 },
          { title: 'Tên biểu mẫu', dataIndex: 'name' },
          { title: 'Số dòng', dataIndex: 'lines', width: 110, render: (l: unknown[]) => l.length },
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
      <Modal title={editing ? 'Sửa biểu mẫu báo cáo' : 'Thêm biểu mẫu báo cáo'} open={open} onCancel={close}
        onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={onFinish}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} disabled={!!editing} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="description" label="Mô tả"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="indicatorIds" label="Các chỉ tiêu (mỗi chỉ tiêu là một dòng)" initialValue={[]}>
            <Select mode="multiple" allowClear showSearch optionFilterProp="label"
              options={indicators?.items.map((i) => ({ value: i.id, label: `${i.name} (${i.code})` }))} />
          </Form.Item>
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết biểu mẫu báo cáo"
        items={detail ? [
          { label: 'Mã', value: detail.code },
          { label: 'Tên', value: detail.name },
          { label: 'Mô tả', value: detail.description },
          { label: 'Các dòng', value: detail.lines.length
            ? <ol style={{ margin: 0, paddingLeft: 18 }}>
                {[...detail.lines].sort((a, b) => a.rowOrder - b.rowOrder).map((l) => <li key={l.rowOrder}>{l.label}</li>)}
              </ol>
            : null },
          { label: 'Trạng thái', value: detail.isActive ? 'Hoạt động' : 'Ngưng' },
        ] : []} />
    </Space>
  );
}
