import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Popconfirm, Space, Table } from 'antd';
import { MinusCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Classification, createClassification, deleteClassification, getClassifications, updateClassification,
} from '../api/client';
import DetailDrawer from '../components/DetailDrawer';

interface ItemField { code: string; name: string; }

export default function ClassificationsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [detail, setDetail] = useState<Classification | null>(null);
  const [editing, setEditing] = useState<Classification | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['classifications', page, pageSize, keyword],
    queryFn: () => getClassifications({ page, pageSize, keyword }),
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['classifications'] });
  const create = useMutation({
    mutationFn: createClassification,
    onSuccess: () => { message.success('Đã tạo danh mục'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string } & Parameters<typeof updateClassification>[1]) => updateClassification(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteClassification,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: Classification) {
    setEditing(r);
    form.setFieldsValue({ code: r.code, name: r.name, description: r.description,
      items: [...r.items].sort((a, b) => a.sortOrder - b.sortOrder).map((i) => ({ code: i.code, name: i.name })) });
    setOpen(true);
  }
  function submit(v: { code: string; name: string; description?: string; items?: ItemField[] }) {
    const items = (v.items ?? []).map((it, idx) => ({ code: it.code, name: it.name, sortOrder: idx + 1 }));
    if (editing) update.mutate({ id: editing.id, name: v.name, description: v.description, items });
    else create.mutate({ code: v.code, name: v.name, description: v.description, items });
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
      </Space>

      <Table<Classification>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        expandable={{ expandedRowRender: (r) => <ol>{[...r.items].sort((a, b) => a.sortOrder - b.sortOrder).map((i) => <li key={i.code}>{i.code} — {i.name}</li>)}</ol> }}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 150 },
          { title: 'Tên danh mục', dataIndex: 'name' },
          { title: 'Số mục', dataIndex: 'items', width: 110, render: (it: unknown[]) => it.length },
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

      <Modal title={editing ? 'Sửa danh mục phân loại' : 'Thêm danh mục phân loại'} open={open} width={620}
        onCancel={close} onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} disabled={!!editing} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="description" label="Mô tả"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Các mục (mã + tên)">
            <Form.List name="items">
              {(fields, { add, remove }) => (
                <Space direction="vertical" style={{ width: '100%' }}>
                  {fields.map(({ key, name, ...rest }) => (
                    <Space key={key} align="baseline" style={{ display: 'flex' }}>
                      <Form.Item {...rest} name={[name, 'code']} rules={[{ required: true, message: 'Mã' }]} style={{ marginBottom: 0 }}>
                        <Input placeholder="Mã" style={{ width: 140 }} maxLength={50} />
                      </Form.Item>
                      <Form.Item {...rest} name={[name, 'name']} rules={[{ required: true, message: 'Tên' }]} style={{ marginBottom: 0 }}>
                        <Input placeholder="Tên" style={{ width: 300 }} maxLength={250} />
                      </Form.Item>
                      <MinusCircleOutlined onClick={() => remove(name)} />
                    </Space>
                  ))}
                  <Button type="dashed" onClick={() => add()} block icon={<PlusOutlined />}>Thêm mục</Button>
                </Space>
              )}
            </Form.List>
          </Form.Item>
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết danh mục phân loại"
        items={detail ? [
          { label: 'Mã', value: detail.code },
          { label: 'Tên', value: detail.name },
          { label: 'Mô tả', value: detail.description },
          { label: 'Các mục', value: detail.items.length
            ? <ol style={{ margin: 0, paddingLeft: 18 }}>
                {[...detail.items].sort((a, b) => a.sortOrder - b.sortOrder).map((i) => <li key={i.code}>{i.code} — {i.name}</li>)}
              </ol>
            : null },
          { label: 'Trạng thái', value: detail.isActive ? 'Hoạt động' : 'Ngưng' },
        ] : []} />
    </Space>
  );
}
