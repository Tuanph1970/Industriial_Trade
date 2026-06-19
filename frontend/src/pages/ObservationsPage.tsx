import { useCallback, useMemo, useState } from 'react';
import { App as AntApp, Button, Form, Input, InputNumber, Modal, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  bulkImportObservations, createObservation, getIndicators, getObservations, getOrgUnits,
  observationAction, ObservationAction, ObservationActionValue, Observation, ObservationStatus,
} from '../api/client';
import ImportModal, { getCell } from '../components/ImportModal';
import DetailDrawer from '../components/DetailDrawer';

const statusLabels: Record<ObservationStatus, string> = { 1: 'Nháp', 2: 'Đã gửi', 3: 'Đã duyệt' };
const statusColors: Record<ObservationStatus, string> = { 1: 'default', 2: 'gold', 3: 'green' };

export default function ObservationsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [open, setOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const [detail, setDetail] = useState<Observation | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['observations', page, pageSize],
    queryFn: () => getObservations({ page, pageSize }),
  });
  const { data: indicators } = useQuery({ queryKey: ['indicators', 'all'], queryFn: () => getIndicators({ page: 1, pageSize: 100 }) });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const indicatorName = (id: string) => indicators?.items.find((i) => i.id === id)?.name ?? id;
  const unitName = (id: string) => units?.items.find((u) => u.id === id)?.name ?? id;

  const indicatorByCode = useMemo(() => new Map((indicators?.items ?? []).map((i) => [i.code, i.id])), [indicators]);
  const unitByCode = useMemo(() => new Map((units?.items ?? []).map((u) => [u.code, u.id])), [units]);

  const mapRow = useCallback((cells: Record<string, string>) => {
    const errors: string[] = [];
    const indicatorCode = getCell(cells, 'Mã chỉ tiêu');
    const unitCode = getCell(cells, 'Mã đơn vị');
    const indicatorId = indicatorByCode.get(indicatorCode);
    const orgUnitId = unitByCode.get(unitCode);
    const year = Number(getCell(cells, 'Năm'));
    const monthRaw = getCell(cells, 'Tháng');
    const valueRaw = getCell(cells, 'Giá trị');

    if (!indicatorId) errors.push(`Không tìm thấy chỉ tiêu '${indicatorCode}'`);
    if (!orgUnitId) errors.push(`Không tìm thấy đơn vị '${unitCode}'`);
    if (!year || year < 2000 || year > 2100) errors.push('Năm không hợp lệ');
    if (monthRaw && (Number(monthRaw) < 1 || Number(monthRaw) > 12)) errors.push('Tháng không hợp lệ');
    if (valueRaw && Number.isNaN(Number(valueRaw))) errors.push('Giá trị không phải số');

    if (errors.length) return { errors };
    return {
      errors,
      item: {
        indicatorId, orgUnitId, periodYear: year,
        periodMonth: monthRaw ? Number(monthRaw) : null,
        value: valueRaw ? Number(valueRaw) : null,
        valueText: getCell(cells, 'Giá trị (văn bản)') || null,
        source: getCell(cells, 'Nguồn') || null,
      },
    };
  }, [indicatorByCode, unitByCode]);

  const create = useMutation({
    mutationFn: createObservation,
    onSuccess: () => {
      message.success('Đã tạo số liệu');
      setOpen(false);
      form.resetFields();
      qc.invalidateQueries({ queryKey: ['observations'] });
    },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền)'),
  });
  const act = useMutation({
    mutationFn: (v: { id: string; action: ObservationActionValue }) => observationAction(v.id, v.action),
    onSuccess: () => { message.success('Đã cập nhật trạng thái'); qc.invalidateQueries({ queryKey: ['observations'] }); },
    onError: () => message.error('Thao tác thất bại (kiểm tra quyền hoặc trạng thái)'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Button type="primary" onClick={() => setOpen(true)}>Thêm số liệu</Button>
        <Button onClick={() => setImportOpen(true)}>Nhập Excel/XML</Button>
      </Space>

      <Table<Observation>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{
          current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); },
        }}
        columns={[
          { title: 'Chỉ tiêu', dataIndex: 'indicatorId', render: (id: string) => indicatorName(id) },
          { title: 'Đơn vị', dataIndex: 'orgUnitId', width: 200, render: (id: string) => unitName(id) },
          {
            title: 'Kỳ', width: 120,
            render: (_, r) => (r.periodMonth ? `${r.periodMonth}/${r.periodYear}` : `${r.periodYear}`),
          },
          { title: 'Giá trị', dataIndex: 'value', width: 140 },
          {
            title: 'Trạng thái', dataIndex: 'status', width: 130,
            render: (s: ObservationStatus) => <Tag color={statusColors[s]}>{statusLabels[s]}</Tag>,
          },
          { title: 'Thao tác', width: 240, render: (_, r) => (
            <Space>
              <a onClick={() => setDetail(r)}>Xem</a>
              {r.status === 1 && <a onClick={() => act.mutate({ id: r.id, action: ObservationAction.Submit })}>Gửi duyệt</a>}
              {r.status === 2 && <a onClick={() => act.mutate({ id: r.id, action: ObservationAction.Approve })}>Duyệt</a>}
              {r.status === 2 && <a style={{ color: '#cf1322' }} onClick={() => act.mutate({ id: r.id, action: ObservationAction.Return })}>Trả lại</a>}
            </Space>) },
        ]}
      />

      <Modal title="Thêm số liệu chỉ tiêu" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate(v)}>
          <Form.Item name="indicatorId" label="Chỉ tiêu" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label"
              options={indicators?.items.map((i) => ({ value: i.id, label: `${i.name} (${i.code})` }))} />
          </Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label"
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Space>
            <Form.Item name="periodYear" label="Năm" rules={[{ required: true }]} initialValue={2026}>
              <InputNumber min={2000} max={2100} />
            </Form.Item>
            <Form.Item name="periodMonth" label="Tháng (tuỳ chọn)"><InputNumber min={1} max={12} /></Form.Item>
          </Space>
          <Form.Item name="value" label="Giá trị"><InputNumber style={{ width: '100%' }} /></Form.Item>
          <Form.Item name="source" label="Nguồn"><Input maxLength={250} /></Form.Item>
        </Form>
      </Modal>

      <DetailDrawer open={!!detail} onClose={() => setDetail(null)} title="Chi tiết số liệu chỉ tiêu"
        items={detail ? [
          { label: 'Chỉ tiêu', value: indicatorName(detail.indicatorId) },
          { label: 'Đơn vị', value: unitName(detail.orgUnitId) },
          { label: 'Kỳ', value: detail.periodMonth ? `${detail.periodMonth}/${detail.periodYear}` : `${detail.periodYear}` },
          { label: 'Giá trị', value: detail.value },
          { label: 'Giá trị (văn bản)', value: detail.valueText },
          { label: 'Nguồn', value: detail.source },
          { label: 'Trạng thái', value: <Tag color={statusColors[detail.status]}>{statusLabels[detail.status]}</Tag> },
        ] : []} />

      <ImportModal
        open={importOpen} onClose={() => setImportOpen(false)}
        title="Nhập số liệu chỉ tiêu" templateName="mau-so-lieu-chi-tieu"
        columns={[
          { header: 'Mã chỉ tiêu', required: true, example: 'CT001' },
          { header: 'Mã đơn vị', required: true, example: 'DV001' },
          { header: 'Năm', required: true, example: '2026' },
          { header: 'Tháng', example: '6' },
          { header: 'Giá trị', example: '1234.5' },
          { header: 'Giá trị (văn bản)' },
          { header: 'Nguồn', example: 'Báo cáo quý' },
        ]}
        mapRow={mapRow} commit={bulkImportObservations}
        onDone={() => qc.invalidateQueries({ queryKey: ['observations'] })} />
    </Space>
  );
}
