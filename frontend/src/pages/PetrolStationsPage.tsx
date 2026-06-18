import { useState } from 'react';
import { App as AntApp, Button, Form, Input, InputNumber, Modal, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createPetrolStation, getOrgUnits, getPetrolStations, PetrolStation, StationStatus } from '../api/client';

const statusLabels: Record<StationStatus, string> = { 1: 'Đang hoạt động', 2: 'Tạm dừng', 3: 'Đã đóng' };
const statusColors: Record<StationStatus, string> = { 1: 'green', 2: 'gold', 3: 'default' };

export default function PetrolStationsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['petrol', page, pageSize, keyword],
    queryFn: () => getPetrolStations({ page, pageSize, keyword }),
  });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const create = useMutation({
    mutationFn: createPetrolStation,
    onSuccess: () => {
      message.success('Đã tạo cửa hàng xăng dầu');
      setOpen(false); form.resetFields();
      qc.invalidateQueries({ queryKey: ['petrol'] });
    },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={() => setOpen(true)}>Thêm mới</Button>
      </Space>
      <Table<PetrolStation>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 130 },
          { title: 'Tên cửa hàng', dataIndex: 'name' },
          { title: 'Số GP', dataIndex: 'licenseNo', width: 140 },
          { title: 'Toạ độ', width: 180, render: (_, r) => (r.latitude != null ? `${r.latitude}, ${r.longitude}` : '—') },
          { title: 'Trạng thái', dataIndex: 'status', width: 150,
            render: (s: StationStatus) => <Tag color={statusColors[s]}>{statusLabels[s]}</Tag> },
        ]}
      />
      <Modal title="Thêm cửa hàng xăng dầu" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate(v)}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} /></Form.Item>
          <Form.Item name="name" label="Tên cửa hàng" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị quản lý" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label"
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Form.Item name="licenseNo" label="Số giấy phép"><Input maxLength={100} /></Form.Item>
          <Form.Item name="address" label="Địa chỉ"><Input maxLength={500} /></Form.Item>
          <Space>
            <Form.Item name="latitude" label="Vĩ độ"><InputNumber min={-90} max={90} step={0.0001} /></Form.Item>
            <Form.Item name="longitude" label="Kinh độ"><InputNumber min={-180} max={180} step={0.0001} /></Form.Item>
          </Space>
          <Form.Item name="status" label="Trạng thái" rules={[{ required: true }]} initialValue={1}>
            <Select options={Object.entries(statusLabels).map(([v, label]) => ({ value: Number(v), label }))} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
