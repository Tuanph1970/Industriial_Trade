import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Select, Space, Table } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createReportTemplate, getIndicators, getReportTemplates, ReportTemplate } from '../api/client';

export default function ReportTemplatesPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['report-templates', page, pageSize, keyword],
    queryFn: () => getReportTemplates({ page, pageSize, keyword }),
  });
  const { data: indicators } = useQuery({ queryKey: ['indicators', 'all'], queryFn: () => getIndicators({ page: 1, pageSize: 200 }) });

  const create = useMutation({
    mutationFn: createReportTemplate,
    onSuccess: () => { message.success('Đã tạo biểu mẫu'); setOpen(false); form.resetFields(); qc.invalidateQueries({ queryKey: ['report-templates'] }); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });

  // Build template lines from the chosen indicators (label = indicator name, ordered by selection).
  const onFinish = (v: { code: string; name: string; description?: string; indicatorIds: string[] }) => {
    const lines = (v.indicatorIds ?? []).map((id, idx) => ({
      indicatorId: id,
      label: indicators?.items.find((i) => i.id === id)?.name ?? id,
      rowOrder: idx + 1,
    }));
    create.mutate({ code: v.code, name: v.name, description: v.description, lines });
  };

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={() => setOpen(true)}>Thêm mới</Button>
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
        ]}
      />
      <Modal title="Thêm biểu mẫu báo cáo" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={onFinish}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="description" label="Mô tả"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="indicatorIds" label="Các chỉ tiêu (mỗi chỉ tiêu là một dòng)" initialValue={[]}>
            <Select mode="multiple" allowClear showSearch optionFilterProp="label"
              options={indicators?.items.map((i) => ({ value: i.id, label: `${i.name} (${i.code})` }))} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
