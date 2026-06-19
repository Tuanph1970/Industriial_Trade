import { useState } from 'react';
import { App as AntApp, Button, DatePicker, Form, Input, Modal, Popconfirm, Select, Space, Table } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import {
  createIndicator, deleteIndicator, getIndicators, Indicator, IndustrySector, updateIndicator,
} from '../api/client';
import DetailDrawer from '../components/DetailDrawer';

const sectorLabels: Record<IndustrySector, string> = {
  1: 'Công nghiệp', 2: 'Năng lượng', 3: 'Thương mại', 4: 'Quản lý thị trường',
};
const dataTypeOptions = [{ value: 1, label: 'Số' }, { value: 2, label: 'Văn bản' }, { value: 3, label: 'Danh mục' }];
const sectorOptions = Object.entries(sectorLabels).map(([v, label]) => ({ value: Number(v), label }));

export default function IndicatorsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [detail, setDetail] = useState<Indicator | null>(null);
  const [editing, setEditing] = useState<Indicator | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['indicators', page, pageSize, keyword],
    queryFn: () => getIndicators({ page, pageSize, keyword }),
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['indicators'] });
  const onError = () => message.error('Thao tác thất bại (kiểm tra quyền hoặc trùng mã)');

  const create = useMutation({ mutationFn: createIndicator, onSuccess: () => { message.success('Đã tạo'); close(); invalidate(); }, onError });
  const update = useMutation({
    mutationFn: (v: { id: string; name: string; unit: string; dataType: 1 | 2 | 3; sector: IndustrySector }) => updateIndicator(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); }, onError,
  });
  const remove = useMutation({ mutationFn: deleteIndicator, onSuccess: () => { message.success('Đã xoá'); invalidate(); }, onError: () => message.error('Không xoá được') });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: Indicator) {
    setEditing(r);
    form.setFieldsValue({ code: r.code, name: r.name, unit: r.unit, dataType: r.dataType, sector: r.sector });
    setOpen(true);
  }
  function submit(v: { code: string; name: string; unit: string; dataType: 1 | 2 | 3; sector: IndustrySector; effectiveFrom?: dayjs.Dayjs }) {
    if (editing) update.mutate({ id: editing.id, name: v.name, unit: v.unit, dataType: v.dataType, sector: v.sector });
    else create.mutate({ code: v.code, name: v.name, unit: v.unit, dataType: v.dataType, sector: v.sector, effectiveFrom: dayjs(v.effectiveFrom).format('YYYY-MM-DD') });
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
      </Space>

      <Table<Indicator>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 130 },
          { title: 'Tên chỉ tiêu', dataIndex: 'name' },
          { title: 'Đơn vị', dataIndex: 'unit', width: 100 },
          { title: 'Lĩnh vực', dataIndex: 'sector', width: 150, render: (s: IndustrySector) => sectorLabels[s] },
          { title: 'Phiên bản', dataIndex: 'version', width: 90 },
          {
            title: 'Thao tác', width: 180,
            render: (_, r) => (
              <Space>
                <a onClick={() => setDetail(r)}>Xem</a>
                <a onClick={() => openEdit(r)}>Sửa</a>
                <Popconfirm title="Xoá chỉ tiêu này?" okText="Xoá" cancelText="Huỷ" onConfirm={() => remove.mutate(r.id)}>
                  <a style={{ color: '#cf1322' }}>Xoá</a>
                </Popconfirm>
              </Space>
            ),
          },
        ]}
      />

      <Modal title={editing ? 'Sửa chỉ tiêu' : 'Thêm chỉ tiêu thống kê'} open={open}
        onCancel={close} onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} disabled={!!editing} /></Form.Item>
          <Form.Item name="name" label="Tên chỉ tiêu" rules={[{ required: true }]}><Input maxLength={300} /></Form.Item>
          <Form.Item name="unit" label="Đơn vị tính"><Input maxLength={50} /></Form.Item>
          <Form.Item name="dataType" label="Kiểu dữ liệu" rules={[{ required: true }]} initialValue={1}>
            <Select options={dataTypeOptions} />
          </Form.Item>
          <Form.Item name="sector" label="Lĩnh vực" rules={[{ required: true }]} initialValue={1}>
            <Select options={sectorOptions} />
          </Form.Item>
          {!editing && (
            <Form.Item name="effectiveFrom" label="Hiệu lực từ" rules={[{ required: true }]} initialValue={dayjs()}>
              <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
            </Form.Item>
          )}
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết chỉ tiêu"
        items={detail ? [
          { label: 'Mã', value: detail.code },
          { label: 'Tên chỉ tiêu', value: detail.name },
          { label: 'Đơn vị tính', value: detail.unit },
          { label: 'Kiểu dữ liệu', value: dataTypeOptions.find((o) => o.value === detail.dataType)?.label },
          { label: 'Lĩnh vực', value: sectorLabels[detail.sector] },
          { label: 'Phiên bản', value: detail.version },
          { label: 'Hiệu lực từ', value: detail.effectiveFrom },
          { label: 'Ngừng hiệu lực', value: detail.retiredAt },
          { label: 'Trạng thái', value: detail.isActive ? 'Hoạt động' : 'Ngưng' },
        ] : []} />
    </Space>
  );
}
