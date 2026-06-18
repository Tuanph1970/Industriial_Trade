import { useState } from 'react';
import { App as AntApp, Button, DatePicker, Form, Input, InputNumber, Modal, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import { Campaign, CampaignStatus, createCampaign, getCampaigns } from '../api/client';

const statusLabels: Record<CampaignStatus, string> = { 1: 'Đang mở', 2: 'Đã đóng' };

export default function CampaignsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['campaigns', page, pageSize, keyword],
    queryFn: () => getCampaigns({ page, pageSize, keyword }),
  });

  const create = useMutation({
    mutationFn: createCampaign,
    onSuccess: () => {
      message.success('Đã tạo kỳ báo cáo');
      setOpen(false); form.resetFields();
      qc.invalidateQueries({ queryKey: ['campaigns'] });
    },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={() => setOpen(true)}>Tạo kỳ báo cáo</Button>
      </Space>
      <Table<Campaign>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 140 },
          { title: 'Tên kỳ báo cáo', dataIndex: 'name' },
          { title: 'Kỳ', width: 120, render: (_, r) => (r.periodMonth ? `${r.periodMonth}/${r.periodYear}` : `${r.periodYear}`) },
          { title: 'Hạn nộp', dataIndex: 'deadline', width: 130, render: (d: string | null) => d ?? '—' },
          { title: 'Trạng thái', dataIndex: 'status', width: 120,
            render: (s: CampaignStatus) => <Tag color={s === 1 ? 'green' : 'default'}>{statusLabels[s]}</Tag> },
        ]}
      />
      <Modal title="Tạo kỳ báo cáo" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical"
          onFinish={(v) => create.mutate({ ...v, deadline: v.deadline ? dayjs(v.deadline).format('YYYY-MM-DD') : null })}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} /></Form.Item>
          <Form.Item name="name" label="Tên kỳ báo cáo" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Space>
            <Form.Item name="periodYear" label="Năm" rules={[{ required: true }]} initialValue={2026}>
              <InputNumber min={2000} max={2100} />
            </Form.Item>
            <Form.Item name="periodMonth" label="Tháng (tuỳ chọn)"><InputNumber min={1} max={12} /></Form.Item>
          </Space>
          <Form.Item name="deadline" label="Hạn nộp"><DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
