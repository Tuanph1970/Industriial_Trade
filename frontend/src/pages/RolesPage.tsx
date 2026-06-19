import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Popconfirm, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ALL_PERMISSIONS, createRole, deleteRole, getRoles, Role, updateRole } from '../api/client';
import DetailDrawer from '../components/DetailDrawer';

export default function RolesPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [detail, setDetail] = useState<Role | null>(null);
  const [editing, setEditing] = useState<Role | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['roles', page, pageSize, keyword],
    queryFn: () => getRoles({ page, pageSize, keyword }),
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['roles'] });
  const create = useMutation({
    mutationFn: createRole,
    onSuccess: () => { message.success('Đã tạo vai trò'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string; name: string; permissions: string[] }) => updateRole(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteRole,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: Role) {
    setEditing(r);
    form.setFieldsValue({ code: r.code, name: r.name, permissions: r.permissions });
    setOpen(true);
  }
  function submit(v: { code: string; name: string; permissions: string[] }) {
    if (editing) update.mutate({ id: editing.id, name: v.name, permissions: v.permissions ?? [] });
    else create.mutate(v);
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
      </Space>

      <Table<Role>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{
          current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); },
        }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 160 },
          { title: 'Tên', dataIndex: 'name', width: 220 },
          {
            title: 'Quyền', dataIndex: 'permissions',
            render: (perms: string[]) => <>{perms.map((p) => <Tag key={p}>{p}</Tag>)}</>,
          },
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

      <Modal title={editing ? 'Sửa vai trò' : 'Thêm vai trò'} open={open} onCancel={close}
        onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} disabled={!!editing} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={150} /></Form.Item>
          <Form.Item name="permissions" label="Quyền" initialValue={[]}>
            <Select mode="multiple" allowClear options={ALL_PERMISSIONS.map((p) => ({ value: p, label: p }))} />
          </Form.Item>
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết vai trò"
        items={detail ? [
          { label: 'Mã', value: detail.code },
          { label: 'Tên', value: detail.name },
          { label: 'Quyền', value: detail.permissions.length
            ? <>{detail.permissions.map((p) => <Tag key={p}>{p}</Tag>)}</> : null },
          { label: 'Trạng thái', value: <Tag color={detail.isActive ? 'green' : 'default'}>{detail.isActive ? 'Hoạt động' : 'Ngưng'}</Tag> },
        ] : []} />
    </Space>
  );
}
