import { useState } from 'react';
import { App as AntApp, Button, Form, Input, InputNumber, Modal, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  createObservation, getIndicators, getObservations, getOrgUnits, Observation, ObservationStatus,
} from '../api/client';

const statusLabels: Record<ObservationStatus, string> = { 1: 'Nháp', 2: 'Đã gửi', 3: 'Đã duyệt' };
const statusColors: Record<ObservationStatus, string> = { 1: 'default', 2: 'gold', 3: 'green' };

export default function ObservationsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['observations', page, pageSize],
    queryFn: () => getObservations({ page, pageSize }),
  });
  const { data: indicators } = useQuery({ queryKey: ['indicators', 'all'], queryFn: () => getIndicators({ page: 1, pageSize: 100 }) });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const indicatorName = (id: string) => indicators?.items.find((i) => i.id === id)?.name ?? id;
  const unitName = (id: string) => units?.items.find((u) => u.id === id)?.name ?? id;

  const create = useMutation({
    mutationFn: createObservation,
    onSuccess: () => {
      message.success('Đã tạo số liệu');
      setOpen(false);
      form.resetFields();
      qc.invalidateQueries({ queryKey: ['observations'] });
    },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền)'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Button type="primary" onClick={() => setOpen(true)}>Thêm số liệu</Button>
      </Space>

      <Table<Observation>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{
          current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); },
        }}
        columns={[
          { title: 'Chỉ tiêu', dataIndex: 'indicatorId', render: (id: string) => indicatorName(id) },
          { title: 'Đơn vị', dataIndex: 'orgUnitId', width: 200, render: (id: string) => unitName(id) },
          {
            title: 'Kỳ', width: 120,
            render: (_, r) => (r.periodMonth ? `${r.periodMonth}/${r.periodYear}` : `${r.periodYear}`),
          },
          { title: 'Giá trị', dataIndex: 'value', width: 140 },
          {
            title: 'Trạng thái', dataIndex: 'status', width: 130,
            render: (s: ObservationStatus) => <Tag color={statusColors[s]}>{statusLabels[s]}</Tag>,
          },
        ]}
      />

      <Modal title="Thêm số liệu chỉ tiêu" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate(v)}>
          <Form.Item name="indicatorId" label="Chỉ tiêu" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label"
              options={indicators?.items.map((i) => ({ value: i.id, label: `${i.name} (${i.code})` }))} />
          </Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label"
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Space>
            <Form.Item name="periodYear" label="Năm" rules={[{ required: true }]} initialValue={2026}>
              <InputNumber min={2000} max={2100} />
            </Form.Item>
            <Form.Item name="periodMonth" label="Tháng (tuỳ chọn)"><InputNumber min={1} max={12} /></Form.Item>
          </Space>
          <Form.Item name="value" label="Giá trị"><InputNumber style={{ width: '100%' }} /></Form.Item>
          <Form.Item name="source" label="Nguồn"><Input maxLength={250} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
