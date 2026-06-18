import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createReportingPeriod, getReportingPeriods, Periodicity, ReportingPeriod } from '../api/client';

const periodicityLabels: Record<Periodicity, string> = { 1: 'Hàng tháng', 2: 'Hàng quý', 3: 'Hàng năm' };
const options = Object.entries(periodicityLabels).map(([v, label]) => ({ value: Number(v), label }));

export default function ReportingPeriodsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['reporting-periods', page, pageSize, keyword],
    queryFn: () => getReportingPeriods({ page, pageSize, keyword }),
  });

  const create = useMutation({
    mutationFn: createReportingPeriod,
    onSuccess: () => { message.success('Đã tạo kỳ báo cáo'); setOpen(false); form.resetFields(); qc.invalidateQueries({ queryKey: ['reporting-periods'] }); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={() => setOpen(true)}>Thêm mới</Button>
      </Space>
      <Table<ReportingPeriod>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 150 },
          { title: 'Tên kỳ báo cáo', dataIndex: 'name' },
          { title: 'Chu kỳ', dataIndex: 'periodicity', width: 160, render: (p: Periodicity) => <Tag>{periodicityLabels[p]}</Tag> },
        ]}
      />
      <Modal title="Thêm kỳ báo cáo" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate(v)}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="periodicity" label="Chu kỳ" rules={[{ required: true }]} initialValue={1}>
            <Select options={options} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
