import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Popconfirm, Select, Space, Switch, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AdministrativeLevel, AdministrativeUnit, createAdministrativeUnit, deleteAdministrativeUnit,
  getAdministrativeUnits, updateAdministrativeUnit,
} from '../api/client';
import DetailDrawer from '../components/DetailDrawer';

const levelLabels: Record<AdministrativeLevel, string> = { 1: 'Tỉnh/Thành phố', 2: 'Quận/Huyện', 3: 'Xã/Phường' };
const levelOptions = Object.entries(levelLabels).map(([v, label]) => ({ value: Number(v), label }));

export default function AdministrativeUnitsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [detail, setDetail] = useState<AdministrativeUnit | null>(null);
  const [editing, setEditing] = useState<AdministrativeUnit | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['admin-units', page, pageSize, keyword],
    queryFn: () => getAdministrativeUnits({ page, pageSize, keyword }),
  });
  // All units (for the parent picker + name resolution).
  const { data: all } = useQuery({ queryKey: ['admin-units', 'all'], queryFn: () => getAdministrativeUnits({ page: 1, pageSize: 500 }) });
  const unitName = (id: string | null) => (id ? all?.items.find((u) => u.id === id)?.name ?? id : null);

  const invalidate = () => qc.invalidateQueries({ queryKey: ['admin-units'] });
  const create = useMutation({
    mutationFn: createAdministrativeUnit,
    onSuccess: () => { message.success('Đã tạo đơn vị hành chính'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string } & Parameters<typeof updateAdministrativeUnit>[1]) => updateAdministrativeUnit(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteAdministrativeUnit,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: AdministrativeUnit) {
    setEditing(r);
    form.setFieldsValue({ code: r.code, name: r.name, level: r.level, parentId: r.parentId, isActive: r.isActive });
    setOpen(true);
  }
  function submit(v: { code: string; name: string; level: AdministrativeLevel; parentId?: string | null; isActive?: boolean }) {
    if (editing) update.mutate({ id: editing.id, name: v.name, level: v.level, parentId: v.parentId ?? null, isActive: v.isActive ?? true });
    else create.mutate({ code: v.code, name: v.name, level: v.level, parentId: v.parentId ?? null });
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
      </Space>

      <Table<AdministrativeUnit>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 130 },
          { title: 'Tên', dataIndex: 'name' },
          { title: 'Cấp', dataIndex: 'level', width: 160, render: (l: AdministrativeLevel) => levelLabels[l] },
          { title: 'Đơn vị cha', dataIndex: 'parentId', width: 200, render: (id: string | null) => unitName(id) ?? '—' },
          { title: 'Trạng thái', dataIndex: 'isActive', width: 110,
            render: (a: boolean) => <Tag color={a ? 'green' : 'default'}>{a ? 'Hoạt động' : 'Ngưng'}</Tag> },
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

      <Modal title={editing ? 'Sửa đơn vị hành chính' : 'Thêm đơn vị hành chính'} open={open}
        onCancel={close} onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} disabled={!!editing} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="level" label="Cấp" rules={[{ required: true }]} initialValue={1}>
            <Select options={levelOptions} />
          </Form.Item>
          <Form.Item name="parentId" label="Đơn vị cha (tuỳ chọn)">
            <Select allowClear showSearch optionFilterProp="label"
              options={all?.items.filter((u) => u.id !== editing?.id).map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          {editing && (
            <Form.Item name="isActive" label="Hoạt động" valuePropName="checked"><Switch /></Form.Item>
          )}
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết đơn vị hành chính"
        items={detail ? [
          { label: 'Mã', value: detail.code },
          { label: 'Tên', value: detail.name },
          { label: 'Cấp', value: levelLabels[detail.level] },
          { label: 'Đơn vị cha', value: unitName(detail.parentId) },
          { label: 'Trạng thái', value: <Tag color={detail.isActive ? 'green' : 'default'}>{detail.isActive ? 'Hoạt động' : 'Ngưng'}</Tag> },
        ] : []} />
    </Space>
  );
}
