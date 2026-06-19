import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Popconfirm, Select, Space, Switch, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  createOrgUnit, deleteOrgUnit, getOrgUnits, OrgUnit, OrgUnitType, updateOrgUnit,
} from '../api/client';
import DetailDrawer from '../components/DetailDrawer';

const typeLabels: Record<OrgUnitType, string> = { 1: 'Sở', 2: 'Phòng', 3: 'Xã/Phường' };

export default function OrgUnitsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [detail, setDetail] = useState<OrgUnit | null>(null);
  const [editing, setEditing] = useState<OrgUnit | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['org-units', page, pageSize, keyword],
    queryFn: () => getOrgUnits({ page, pageSize, keyword }),
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['org-units'] });
  const onError = () => message.error('Thao tác thất bại (kiểm tra quyền hoặc ràng buộc dữ liệu)');

  const create = useMutation({
    mutationFn: createOrgUnit,
    onSuccess: () => { message.success('Đã tạo'); close(); invalidate(); }, onError,
  });
  const update = useMutation({
    mutationFn: (v: { id: string; name: string; isActive: boolean }) => updateOrgUnit(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); }, onError,
  });
  const remove = useMutation({
    mutationFn: deleteOrgUnit,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được (đơn vị có thể có đơn vị con)'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: OrgUnit) {
    setEditing(r);
    form.setFieldsValue({ code: r.code, name: r.name, type: r.type, isActive: r.isActive });
    setOpen(true);
  }
  function submit(v: { code: string; name: string; type: OrgUnitType; isActive: boolean }) {
    if (editing) update.mutate({ id: editing.id, name: v.name, isActive: v.isActive });
    else create.mutate({ code: v.code, name: v.name, type: v.type });
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
      </Space>

      <Table<OrgUnit>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 140 },
          { title: 'Tên', dataIndex: 'name' },
          { title: 'Loại', dataIndex: 'type', width: 120, render: (t: OrgUnitType) => typeLabels[t] },
          { title: 'Đường dẫn', dataIndex: 'path', width: 200 },
          { title: 'Trạng thái', dataIndex: 'isActive', width: 110,
            render: (a: boolean) => <Tag color={a ? 'green' : 'default'}>{a ? 'Hoạt động' : 'Ngưng'}</Tag> },
          {
            title: 'Thao tác', width: 180,
            render: (_, r) => (
              <Space>
                <a onClick={() => setDetail(r)}>Xem</a>
                <a onClick={() => openEdit(r)}>Sửa</a>
                <Popconfirm title="Xoá đơn vị này?" okText="Xoá" cancelText="Huỷ" onConfirm={() => remove.mutate(r.id)}>
                  <a style={{ color: '#cf1322' }}>Xoá</a>
                </Popconfirm>
              </Space>
            ),
          },
        ]}
      />

      <Modal title={editing ? 'Sửa cơ quan, đơn vị' : 'Thêm cơ quan, đơn vị'} open={open}
        onCancel={close} onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}>
            <Input maxLength={50} disabled={!!editing} />
          </Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="type" label="Loại" rules={[{ required: true }]}>
            <Select disabled={!!editing} options={[
              { value: 1, label: 'Sở' }, { value: 2, label: 'Phòng' }, { value: 3, label: 'Xã/Phường' },
            ]} />
          </Form.Item>
          {editing && (
            <Form.Item name="isActive" label="Hoạt động" valuePropName="checked"><Switch /></Form.Item>
          )}
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết cơ quan, đơn vị"
        items={detail ? [
          { label: 'Mã', value: detail.code },
          { label: 'Tên', value: detail.name },
          { label: 'Loại', value: typeLabels[detail.type] },
          { label: 'Đường dẫn', value: detail.path },
          { label: 'Trạng thái', value: <Tag color={detail.isActive ? 'green' : 'default'}>{detail.isActive ? 'Hoạt động' : 'Ngưng'}</Tag> },
        ] : []} />
    </Space>
  );
}
