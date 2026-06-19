import { useState } from 'react';
import { App as AntApp, Button, Form, Input, InputNumber, Modal, Popconfirm, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Cluster, ClusterStatus, createCluster, deleteCluster, getClusters, getOrgUnits, updateCluster } from '../api/client';

const statusLabels: Record<ClusterStatus, string> = { 1: 'Quy hoạch', 2: 'Đang hoạt động', 3: 'Tạm dừng' };
const statusColors: Record<ClusterStatus, string> = { 1: 'blue', 2: 'green', 3: 'default' };

export default function ClustersPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<Cluster | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['clusters', page, pageSize, keyword],
    queryFn: () => getClusters({ page, pageSize, keyword }),
  });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['clusters'] });
  const create = useMutation({
    mutationFn: createCluster,
    onSuccess: () => { message.success('Đã tạo cụm công nghiệp'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string } & Parameters<typeof updateCluster>[1]) => updateCluster(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteCluster,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: Cluster) {
    setEditing(r);
    form.setFieldsValue({ code: r.code, name: r.name, orgUnitId: r.orgUnitId, areaHa: r.areaHa,
      latitude: r.latitude, longitude: r.longitude, status: r.status });
    setOpen(true);
  }
  function submit(v: { code: string; name: string; orgUnitId: string; areaHa?: number | null;
    latitude?: number | null; longitude?: number | null; status: ClusterStatus }) {
    if (editing) update.mutate({ id: editing.id, name: v.name, areaHa: v.areaHa,
      latitude: v.latitude, longitude: v.longitude, status: v.status });
    else create.mutate(v);
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
      </Space>

      <Table<Cluster>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{
          current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); },
        }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 140 },
          { title: 'Tên cụm', dataIndex: 'name' },
          { title: 'Diện tích (ha)', dataIndex: 'areaHa', width: 130 },
          {
            title: 'Toạ độ', width: 200,
            render: (_, r) => (r.latitude != null ? `${r.latitude}, ${r.longitude}` : '—'),
          },
          {
            title: 'Trạng thái', dataIndex: 'status', width: 150,
            render: (s: ClusterStatus) => <Tag color={statusColors[s]}>{statusLabels[s]}</Tag>,
          },
          { title: 'Thao tác', width: 130, render: (_, r) => (
            <Space>
              <a onClick={() => openEdit(r)}>Sửa</a>
              <Popconfirm title="Xoá?" okText="Xoá" cancelText="Huỷ" onConfirm={() => remove.mutate(r.id)}>
                <a style={{ color: '#cf1322' }}>Xoá</a>
              </Popconfirm>
            </Space>) },
        ]}
      />

      <Modal title={editing ? 'Sửa cụm công nghiệp' : 'Thêm cụm công nghiệp'} open={open} onCancel={close}
        onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} disabled={!!editing} /></Form.Item>
          <Form.Item name="name" label="Tên cụm" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị quản lý" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label" disabled={!!editing}
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Form.Item name="areaHa" label="Diện tích (ha)"><InputNumber min={0} style={{ width: '100%' }} /></Form.Item>
          <Space>
            <Form.Item name="latitude" label="Vĩ độ"><InputNumber min={-90} max={90} step={0.0001} /></Form.Item>
            <Form.Item name="longitude" label="Kinh độ"><InputNumber min={-180} max={180} step={0.0001} /></Form.Item>
          </Space>
          <Form.Item name="status" label="Trạng thái" rules={[{ required: true }]} initialValue={2}>
            <Select options={Object.entries(statusLabels).map(([v, label]) => ({ value: Number(v), label }))} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
