import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ALL_PERMISSIONS, createRole, getRoles, Role } from '../api/client';

export default function RolesPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['roles', page, pageSize, keyword],
    queryFn: () => getRoles({ page, pageSize, keyword }),
  });

  const create = useMutation({
    mutationFn: createRole,
    onSuccess: () => {
      message.success('Đã tạo vai trò');
      setOpen(false);
      form.resetFields();
      qc.invalidateQueries({ queryKey: ['roles'] });
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
        ]}
      />

      <Modal title="Thêm vai trò" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate(v)}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={150} /></Form.Item>
          <Form.Item name="permissions" label="Quyền" initialValue={[]}>
            <Select mode="multiple" allowClear options={ALL_PERMISSIONS.map((p) => ({ value: p, label: p }))} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
