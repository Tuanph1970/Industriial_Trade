import { useCallback, useMemo, useState } from 'react';
import { App as AntApp, Button, Form, Input, InputNumber, Modal, Popconfirm, Select, Space, Table } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  bulkImportCommerceLocations, CommerceLocation, CommerceLocationType, createCommerceLocation,
  deleteCommerceLocation, getCommerceLocations, getOrgUnits, updateCommerceLocation,
} from '../api/client';
import ImportModal, { getCell, parseEnum } from '../components/ImportModal';
import DetailDrawer from '../components/DetailDrawer';

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
  const [importOpen, setImportOpen] = useState(false);
  const [detail, setDetail] = useState<CommerceLocation | null>(null);
  const [editing, setEditing] = useState<CommerceLocation | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['commerce', page, pageSize, keyword],
    queryFn: () => getCommerceLocations({ page, pageSize, keyword }),
  });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const unitByCode = useMemo(() => new Map((units?.items ?? []).map((u) => [u.code, u.id])), [units]);
  const mapRow = useCallback((cells: Record<string, string>) => {
    const errors: string[] = [];
    const code = getCell(cells, 'Mã');
    const name = getCell(cells, 'Tên');
    const unitCode = getCell(cells, 'Mã đơn vị');
    const orgUnitId = unitByCode.get(unitCode);
    const typeRaw = getCell(cells, 'Loại');
    const type = parseEnum(typeRaw, typeLabels);
    const latRaw = getCell(cells, 'Vĩ độ');
    const lngRaw = getCell(cells, 'Kinh độ');

    if (!code) errors.push('Thiếu mã');
    if (!name) errors.push('Thiếu tên');
    if (type === undefined) errors.push(`Loại không hợp lệ '${typeRaw}'`);
    if (!orgUnitId) errors.push(`Không tìm thấy đơn vị '${unitCode}'`);

    if (errors.length) return { errors };
    return {
      errors,
      item: {
        code, name, type: type as CommerceLocationType, orgUnitId,
        address: getCell(cells, 'Địa chỉ') || null,
        latitude: latRaw ? Number(latRaw) : null, longitude: lngRaw ? Number(lngRaw) : null,
      },
    };
  }, [unitByCode]);

  const invalidate = () => qc.invalidateQueries({ queryKey: ['commerce'] });
  const create = useMutation({
    mutationFn: createCommerceLocation,
    onSuccess: () => { message.success('Đã tạo địa điểm thương mại'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string } & Parameters<typeof updateCommerceLocation>[1]) => updateCommerceLocation(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteCommerceLocation,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: CommerceLocation) {
    setEditing(r);
    form.setFieldsValue({ code: r.code, name: r.name, type: r.type, orgUnitId: r.orgUnitId,
      address: r.address, latitude: r.latitude, longitude: r.longitude });
    setOpen(true);
  }
  function submit(v: { code: string; name: string; type: CommerceLocationType; orgUnitId: string;
    address?: string | null; latitude?: number | null; longitude?: number | null }) {
    if (editing) update.mutate({ id: editing.id, name: v.name, type: v.type, address: v.address,
      latitude: v.latitude, longitude: v.longitude });
    else create.mutate(v);
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
        <Button onClick={() => setImportOpen(true)}>Nhập Excel/XML</Button>
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
      <Modal title={editing ? 'Sửa địa điểm thương mại' : 'Thêm địa điểm thương mại'} open={open} onCancel={close}
        onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} disabled={!!editing} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="type" label="Loại" rules={[{ required: true }]} initialValue={1}>
            <Select options={typeOptions} />
          </Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị quản lý" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label" disabled={!!editing}
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Form.Item name="address" label="Địa chỉ"><Input maxLength={500} /></Form.Item>
          <Space>
            <Form.Item name="latitude" label="Vĩ độ"><InputNumber min={-90} max={90} step={0.0001} /></Form.Item>
            <Form.Item name="longitude" label="Kinh độ"><InputNumber min={-180} max={180} step={0.0001} /></Form.Item>
          </Space>
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết địa điểm thương mại"
        items={detail ? [
          { label: 'Mã', value: detail.code },
          { label: 'Tên', value: detail.name },
          { label: 'Loại', value: typeLabels[detail.type] },
          { label: 'Đơn vị quản lý', value: units?.items.find((u) => u.id === detail.orgUnitId)?.name ?? detail.orgUnitId },
          { label: 'Địa chỉ', value: detail.address },
          { label: 'Toạ độ', value: detail.latitude != null ? `${detail.latitude}, ${detail.longitude}` : null },
        ] : []} />

      <ImportModal
        open={importOpen} onClose={() => setImportOpen(false)}
        title="Nhập địa điểm thương mại" templateName="mau-dia-diem-thuong-mai"
        columns={[
          { header: 'Mã', required: true, example: 'TM001' },
          { header: 'Tên', required: true, example: 'Chợ trung tâm' },
          { header: 'Loại', required: true, example: 'Chợ' },
          { header: 'Mã đơn vị', required: true, example: 'DV001' },
          { header: 'Địa chỉ', example: 'Số 1, đường A' },
          { header: 'Vĩ độ', example: '20.65' },
          { header: 'Kinh độ', example: '106.05' },
        ]}
        mapRow={mapRow} commit={bulkImportCommerceLocations} onDone={invalidate} />
    </Space>
  );
}
