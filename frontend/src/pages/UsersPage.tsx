import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Popconfirm, Select, Space, Switch, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createUser, deleteUser, getOrgUnits, getRoles, getUsers, updateUser, UserAccount } from '../api/client';
import DetailDrawer from '../components/DetailDrawer';

export default function UsersPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [detail, setDetail] = useState<UserAccount | null>(null);
  const [editing, setEditing] = useState<UserAccount | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['users', page, pageSize, keyword],
    queryFn: () => getUsers({ page, pageSize, keyword }),
  });
  const { data: roles } = useQuery({ queryKey: ['roles', 'all'], queryFn: () => getRoles({ page: 1, pageSize: 100 }) });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['users'] });
  const create = useMutation({
    mutationFn: createUser,
    onSuccess: () => { message.success('Đã tạo người dùng'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng tên)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string } & Parameters<typeof updateUser>[1]) => updateUser(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteUser,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); form.setFieldsValue({ isActive: true }); setOpen(true); }
  function openEdit(r: UserAccount) {
    setEditing(r);
    form.setFieldsValue({ userName: r.userName, fullName: r.fullName, email: r.email,
      orgUnitId: r.orgUnitId, roleIds: r.roleIds, isActive: r.isActive });
    setOpen(true);
  }
  function submit(v: { userName: string; fullName?: string; email?: string;
    orgUnitId?: string | null; roleIds?: string[]; isActive?: boolean }) {
    if (editing) update.mutate({ id: editing.id, fullName: v.fullName, email: v.email,
      orgUnitId: v.orgUnitId ?? null, roleIds: v.roleIds ?? [], isActive: v.isActive ?? true });
    else create.mutate({ ...v, roleIds: v.roleIds ?? [] });
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo tên đăng nhập / họ tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
      </Space>

      <Table<UserAccount>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{
          current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); },
        }}
        columns={[
          { title: 'Tên đăng nhập', dataIndex: 'userName', width: 180 },
          { title: 'Họ tên', dataIndex: 'fullName' },
          { title: 'Email', dataIndex: 'email' },
          { title: 'Số vai trò', dataIndex: 'roleIds', width: 120, render: (r: string[]) => r.length },
          {
            title: 'Trạng thái', dataIndex: 'isActive', width: 120,
            render: (a: boolean) => <Tag color={a ? 'green' : 'default'}>{a ? 'Hoạt động' : 'Ngưng'}</Tag>,
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

      <Modal title={editing ? 'Sửa người dùng' : 'Thêm người dùng'} open={open} onCancel={close}
        onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="userName" label="Tên đăng nhập" rules={[{ required: true }]}><Input maxLength={100} disabled={!!editing} /></Form.Item>
          <Form.Item name="fullName" label="Họ tên"><Input maxLength={250} /></Form.Item>
          <Form.Item name="email" label="Email" rules={[{ type: 'email' }]}><Input /></Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị">
            <Select allowClear showSearch optionFilterProp="label"
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Form.Item name="roleIds" label="Vai trò" initialValue={[]}>
            <Select mode="multiple" allowClear
              options={roles?.items.map((r) => ({ value: r.id, label: r.name }))} />
          </Form.Item>
          {editing && (
            <Form.Item name="isActive" label="Trạng thái hoạt động" valuePropName="checked">
              <Switch checkedChildren="Hoạt động" unCheckedChildren="Ngưng" />
            </Form.Item>
          )}
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết người dùng"
        items={detail ? [
          { label: 'Tên đăng nhập', value: detail.userName },
          { label: 'Họ tên', value: detail.fullName },
          { label: 'Email', value: detail.email },
          { label: 'Đơn vị', value: units?.items.find((u) => u.id === detail.orgUnitId)?.name ?? detail.orgUnitId },
          { label: 'Vai trò', value: detail.roleIds.length
            ? <>{detail.roleIds.map((id) => <Tag key={id}>{roles?.items.find((r) => r.id === id)?.name ?? id}</Tag>)}</>
            : null },
          { label: 'Trạng thái', value: <Tag color={detail.isActive ? 'green' : 'default'}>{detail.isActive ? 'Hoạt động' : 'Ngưng'}</Tag> },
        ] : []} />
    </Space>
  );
}
