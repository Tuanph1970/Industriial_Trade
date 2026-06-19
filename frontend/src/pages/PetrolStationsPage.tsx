import { useCallback, useMemo, useState } from 'react';
import { App as AntApp, Button, Form, Input, InputNumber, Modal, Popconfirm, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  bulkImportPetrolStations, createPetrolStation, deletePetrolStation, getOrgUnits, getPetrolStations,
  PetrolStation, StationStatus, updatePetrolStation,
} from '../api/client';
import ImportModal, { getCell, parseEnum } from '../components/ImportModal';
import DetailDrawer from '../components/DetailDrawer';

const statusLabels: Record<StationStatus, string> = { 1: 'Đang hoạt động', 2: 'Tạm dừng', 3: 'Đã đóng' };
const statusColors: Record<StationStatus, string> = { 1: 'green', 2: 'gold', 3: 'default' };

export default function PetrolStationsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const [detail, setDetail] = useState<PetrolStation | null>(null);
  const [editing, setEditing] = useState<PetrolStation | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['petrol', page, pageSize, keyword],
    queryFn: () => getPetrolStations({ page, pageSize, keyword }),
  });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const unitByCode = useMemo(() => new Map((units?.items ?? []).map((u) => [u.code, u.id])), [units]);
  const mapRow = useCallback((cells: Record<string, string>) => {
    const errors: string[] = [];
    const code = getCell(cells, 'Mã');
    const name = getCell(cells, 'Tên');
    const unitCode = getCell(cells, 'Mã đơn vị');
    const orgUnitId = unitByCode.get(unitCode);
    const latRaw = getCell(cells, 'Vĩ độ');
    const lngRaw = getCell(cells, 'Kinh độ');
    const statusRaw = getCell(cells, 'Trạng thái');
    const status = statusRaw ? parseEnum(statusRaw, statusLabels) : 1;

    if (!code) errors.push('Thiếu mã');
    if (!name) errors.push('Thiếu tên');
    if (!orgUnitId) errors.push(`Không tìm thấy đơn vị '${unitCode}'`);
    if (status === undefined) errors.push(`Trạng thái không hợp lệ '${statusRaw}'`);

    if (errors.length) return { errors };
    return {
      errors,
      item: {
        code, name, orgUnitId,
        licenseNo: getCell(cells, 'Số giấy phép') || null,
        address: getCell(cells, 'Địa chỉ') || null,
        latitude: latRaw ? Number(latRaw) : null, longitude: lngRaw ? Number(lngRaw) : null,
        status: status as StationStatus,
      },
    };
  }, [unitByCode]);

  const invalidate = () => qc.invalidateQueries({ queryKey: ['petrol'] });
  const create = useMutation({
    mutationFn: createPetrolStation,
    onSuccess: () => { message.success('Đã tạo cửa hàng xăng dầu'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string } & Parameters<typeof updatePetrolStation>[1]) => updatePetrolStation(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deletePetrolStation,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: PetrolStation) {
    setEditing(r);
    form.setFieldsValue({ code: r.code, name: r.name, orgUnitId: r.orgUnitId, licenseNo: r.licenseNo,
      address: r.address, latitude: r.latitude, longitude: r.longitude, status: r.status });
    setOpen(true);
  }
  function submit(v: { code: string; name: string; orgUnitId: string; licenseNo?: string | null;
    address?: string | null; latitude?: number | null; longitude?: number | null; status: StationStatus }) {
    if (editing) update.mutate({ id: editing.id, name: v.name, licenseNo: v.licenseNo, address: v.address,
      latitude: v.latitude, longitude: v.longitude, status: v.status });
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
      <Table<PetrolStation>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 130 },
          { title: 'Tên cửa hàng', dataIndex: 'name' },
          { title: 'Số GP', dataIndex: 'licenseNo', width: 140 },
          { title: 'Toạ độ', width: 180, render: (_, r) => (r.latitude != null ? `${r.latitude}, ${r.longitude}` : '—') },
          { title: 'Trạng thái', dataIndex: 'status', width: 150,
            render: (s: StationStatus) => <Tag color={statusColors[s]}>{statusLabels[s]}</Tag> },
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
      <Modal title={editing ? 'Sửa cửa hàng xăng dầu' : 'Thêm cửa hàng xăng dầu'} open={open} onCancel={close}
        onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} disabled={!!editing} /></Form.Item>
          <Form.Item name="name" label="Tên cửa hàng" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị quản lý" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label" disabled={!!editing}
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Form.Item name="licenseNo" label="Số giấy phép"><Input maxLength={100} /></Form.Item>
          <Form.Item name="address" label="Địa chỉ"><Input maxLength={500} /></Form.Item>
          <Space>
            <Form.Item name="latitude" label="Vĩ độ"><InputNumber min={-90} max={90} step={0.0001} /></Form.Item>
            <Form.Item name="longitude" label="Kinh độ"><InputNumber min={-180} max={180} step={0.0001} /></Form.Item>
          </Space>
          <Form.Item name="status" label="Trạng thái" rules={[{ required: true }]} initialValue={1}>
            <Select options={Object.entries(statusLabels).map(([v, label]) => ({ value: Number(v), label }))} />
          </Form.Item>
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết cửa hàng xăng dầu"
        items={detail ? [
          { label: 'Mã', value: detail.code },
          { label: 'Tên cửa hàng', value: detail.name },
          { label: 'Đơn vị quản lý', value: units?.items.find((u) => u.id === detail.orgUnitId)?.name ?? detail.orgUnitId },
          { label: 'Số giấy phép', value: detail.licenseNo },
          { label: 'Địa chỉ', value: detail.address },
          { label: 'Toạ độ', value: detail.latitude != null ? `${detail.latitude}, ${detail.longitude}` : null },
          { label: 'Trạng thái', value: <Tag color={statusColors[detail.status]}>{statusLabels[detail.status]}</Tag> },
        ] : []} />

      <ImportModal
        open={importOpen} onClose={() => setImportOpen(false)}
        title="Nhập cửa hàng xăng dầu" templateName="mau-cua-hang-xang-dau"
        columns={[
          { header: 'Mã', required: true, example: 'XD001' },
          { header: 'Tên', required: true, example: 'CHXD số 1' },
          { header: 'Mã đơn vị', required: true, example: 'DV001' },
          { header: 'Số giấy phép', example: 'GP-123' },
          { header: 'Địa chỉ', example: 'Số 1, đường A' },
          { header: 'Vĩ độ', example: '20.65' },
          { header: 'Kinh độ', example: '106.05' },
          { header: 'Trạng thái', example: 'Đang hoạt động' },
        ]}
        mapRow={mapRow} commit={bulkImportPetrolStations} onDone={invalidate} />
    </Space>
  );
}
