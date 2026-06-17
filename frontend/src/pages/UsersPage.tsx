import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createUser, getOrgUnits, getRoles, getUsers, UserAccount } from '../api/client';

export default function UsersPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['users', page, pageSize, keyword],
    queryFn: () => getUsers({ page, pageSize, keyword }),
  });
  const { data: roles } = useQuery({ queryKey: ['roles', 'all'], queryFn: () => getRoles({ page: 1, pageSize: 100 }) });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const create = useMutation({
    mutationFn: createUser,
    onSuccess: () => {
      message.success('Đã tạo người dùng');
      setOpen(false);
      form.resetFields();
      qc.invalidateQueries({ queryKey: ['users'] });
    },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng tên)'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo tên đăng nhập / họ tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={() => setOpen(true)}>Thêm mới</Button>
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
        ]}
      />

      <Modal title="Thêm người dùng" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate(v)}>
          <Form.Item name="userName" label="Tên đăng nhập" rules={[{ required: true }]}><Input maxLength={100} /></Form.Item>
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
        </Form>
      </Modal>
    </Space>
  );
}
