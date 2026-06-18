import { useState } from 'react';
import { App as AntApp, Button, Form, Input, InputNumber, Modal, Popconfirm, Select, Space, Table } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CommerceLocation, CommerceLocationType, createCommerceLocation, deleteCommerceLocation, getCommerceLocations, getOrgUnits } from '../api/client';

const typeLabels: Record<CommerceLocationType, string> = {
  1: 'Chợ', 2: 'Siêu thị', 3: 'Trung tâm thương mại', 4: 'Cửa hàng tiện lợi',
};
const typeOptions = Object.entries(typeLabels).map(([v, label]) => ({ value: Number(v), label }));

export default function CommerceLocationsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['commerce', page, pageSize, keyword],
    queryFn: () => getCommerceLocations({ page, pageSize, keyword }),
  });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const create = useMutation({
    mutationFn: createCommerceLocation,
    onSuccess: () => {
      message.success('Đã tạo địa điểm thương mại');
      setOpen(false); form.resetFields();
      qc.invalidateQueries({ queryKey: ['commerce'] });
    },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });
  const remove = useMutation({
    mutationFn: deleteCommerceLocation,
    onSuccess: () => { message.success('Đã xoá'); qc.invalidateQueries({ queryKey: ['commerce'] }); },
    onError: () => message.error('Không xoá được'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={() => setOpen(true)}>Thêm mới</Button>
      </Space>
      <Table<CommerceLocation>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 130 },
          { title: 'Tên', dataIndex: 'name' },
          { title: 'Loại', dataIndex: 'type', width: 200, render: (t: CommerceLocationType) => typeLabels[t] },
          { title: 'Địa chỉ', dataIndex: 'address' },
          { title: 'Toạ độ', width: 180, render: (_, r) => (r.latitude != null ? `${r.latitude}, ${r.longitude}` : '—') },
          { title: 'Thao tác', width: 90, render: (_, r) => (
            <Popconfirm title="Xoá?" okText="Xoá" cancelText="Huỷ" onConfirm={() => remove.mutate(r.id)}>
              <a style={{ color: '#cf1322' }}>Xoá</a>
            </Popconfirm>) },
        ]}
      />
      <Modal title="Thêm địa điểm thương mại" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate(v)}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="type" label="Loại" rules={[{ required: true }]} initialValue={1}>
            <Select options={typeOptions} />
          </Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị quản lý" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label"
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Form.Item name="address" label="Địa chỉ"><Input maxLength={500} /></Form.Item>
          <Space>
            <Form.Item name="latitude" label="Vĩ độ"><InputNumber min={-90} max={90} step={0.0001} /></Form.Item>
            <Form.Item name="longitude" label="Kinh độ"><InputNumber min={-180} max={180} step={0.0001} /></Form.Item>
          </Space>
        </Form>
      </Modal>
    </Space>
  );
}
