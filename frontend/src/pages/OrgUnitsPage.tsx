import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createOrgUnit, getOrgUnits, OrgUnit, OrgUnitType } from '../api/client';

const typeLabels: Record<OrgUnitType, string> = { 1: 'Sở', 2: 'Phòng', 3: 'Xã/Phường' };

export default function OrgUnitsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['org-units', page, pageSize, keyword],
    queryFn: () => getOrgUnits(page, pageSize, keyword),
  });

  const create = useMutation({
    mutationFn: createOrgUnit,
    onSuccess: () => {
      message.success('Đã tạo cơ quan, đơn vị');
      setOpen(false);
      form.resetFields();
      qc.invalidateQueries({ queryKey: ['org-units'] });
    },
    onError: () => message.error('Tạo thất bại'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search
          placeholder="Tìm theo mã hoặc tên"
          allowClear
          onSearch={(v) => { setKeyword(v); setPage(1); }}
          style={{ width: 320 }}
        />
        <Button type="primary" onClick={() => setOpen(true)}>Thêm mới</Button>
      </Space>

      <Table<OrgUnit>
        rowKey="id"
        loading={isLoading}
        dataSource={data?.items}
        pagination={{
          current: page,
          pageSize,
          total: data?.totalCount,
          showSizeChanger: true,
          pageSizeOptions: [10, 20, 50],
          onChange: (p, ps) => { setPage(p); setPageSize(ps); },
        }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 140 },
          { title: 'Tên', dataIndex: 'name' },
          { title: 'Loại', dataIndex: 'type', width: 120, render: (t: OrgUnitType) => typeLabels[t] },
          { title: 'Đường dẫn', dataIndex: 'path', width: 220 },
          {
            title: 'Trạng thái', dataIndex: 'isActive', width: 120,
            render: (a: boolean) => <Tag color={a ? 'green' : 'default'}>{a ? 'Hoạt động' : 'Ngưng'}</Tag>,
          },
        ]}
      />

      <Modal
        title="Thêm cơ quan, đơn vị"
        open={open}
        onCancel={() => setOpen(false)}
        onOk={() => form.submit()}
        confirmLoading={create.isPending}
      >
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate(v)}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}>
            <Input maxLength={50} />
          </Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}>
            <Input maxLength={250} />
          </Form.Item>
          <Form.Item name="type" label="Loại" rules={[{ required: true }]}>
            <Select
              options={[
                { value: 1, label: 'Sở' },
                { value: 2, label: 'Phòng' },
                { value: 3, label: 'Xã/Phường' },
              ]}
            />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
