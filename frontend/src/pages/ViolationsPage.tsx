import { useCallback, useMemo, useState } from 'react';
import { App as AntApp, Button, DatePicker, Form, Input, InputNumber, Modal, Popconfirm, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import {
  bulkImportViolations, createViolation, deleteViolation, getOrgUnits, getViolations, updateViolation,
  Violation, ViolationGroup, ViolationStatus,
} from '../api/client';
import ImportModal, { getCell, parseEnum } from '../components/ImportModal';

const parseImportDate = (s: string): string | null => {
  if (/^\d{4}-\d{2}-\d{2}$/.test(s)) return s;
  const m = s.match(/^(\d{1,2})[/-](\d{1,2})[/-](\d{4})$/);
  if (m) return `${m[3]}-${m[2].padStart(2, '0')}-${m[1].padStart(2, '0')}`;
  const d = dayjs(s);
  return d.isValid() ? d.format('YYYY-MM-DD') : null;
};

const groupLabels: Record<ViolationGroup, string> = {
  1: 'Hàng cấm / giả / nhái / kém chất lượng', 2: 'Vệ sinh, an toàn thực phẩm',
};
const statusLabels: Record<ViolationStatus, string> = { 1: 'Đã ghi nhận', 2: 'Đang xử lý', 3: 'Đã xử lý' };
const statusColors: Record<ViolationStatus, string> = { 1: 'default', 2: 'gold', 3: 'green' };
const groupOptions = Object.entries(groupLabels).map(([v, label]) => ({ value: Number(v), label }));
const statusOptions = Object.entries(statusLabels).map(([v, label]) => ({ value: Number(v), label }));

export default function ViolationsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const [editing, setEditing] = useState<Violation | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['violations', page, pageSize, keyword],
    queryFn: () => getViolations({ page, pageSize, keyword }),
  });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const unitByCode = useMemo(() => new Map((units?.items ?? []).map((u) => [u.code, u.id])), [units]);
  const mapRow = useCallback((cells: Record<string, string>) => {
    const errors: string[] = [];
    const caseNo = getCell(cells, 'Số hồ sơ');
    const businessName = getCell(cells, 'Cơ sở kinh doanh');
    const unitCode = getCell(cells, 'Mã đơn vị');
    const orgUnitId = unitByCode.get(unitCode);
    const groupRaw = getCell(cells, 'Nhóm');
    const group = parseEnum(groupRaw, groupLabels);
    const dateRaw = getCell(cells, 'Ngày kiểm tra');
    const inspectedOn = dateRaw ? parseImportDate(dateRaw) : null;
    const violationContent = getCell(cells, 'Nội dung vi phạm');

    if (!caseNo) errors.push('Thiếu số hồ sơ');
    if (group === undefined) errors.push(`Nhóm không hợp lệ '${groupRaw}'`);
    if (!orgUnitId) errors.push(`Không tìm thấy đơn vị '${unitCode}'`);
    if (!businessName) errors.push('Thiếu cơ sở kinh doanh');
    if (!inspectedOn) errors.push('Ngày kiểm tra không hợp lệ');
    if (!violationContent) errors.push('Thiếu nội dung vi phạm');

    if (errors.length) return { errors };
    return {
      errors,
      item: { caseNo, group: group as ViolationGroup, orgUnitId, businessName, inspectedOn, violationContent },
    };
  }, [unitByCode]);

  const invalidate = () => qc.invalidateQueries({ queryKey: ['violations'] });
  const create = useMutation({
    mutationFn: createViolation,
    onSuccess: () => { message.success('Đã tạo hồ sơ vi phạm'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng số hồ sơ)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string } & Parameters<typeof updateViolation>[1]) => updateViolation(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteViolation,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: Violation) {
    setEditing(r);
    form.setFieldsValue({ caseNo: r.caseNo, group: r.group, orgUnitId: r.orgUnitId, businessName: r.businessName,
      inspectedOn: dayjs(r.inspectedOn), violationContent: r.violationContent,
      sanctionContent: r.sanctionContent, fineAmount: r.fineAmount, status: r.status });
    setOpen(true);
  }
  function submit(v: { caseNo: string; group: ViolationGroup; orgUnitId: string; businessName: string;
    inspectedOn: dayjs.Dayjs; violationContent: string;
    sanctionContent?: string | null; fineAmount?: number | null; status: ViolationStatus }) {
    const inspectedOn = dayjs(v.inspectedOn).format('YYYY-MM-DD');
    if (editing) update.mutate({ id: editing.id, group: v.group, businessName: v.businessName, inspectedOn,
      violationContent: v.violationContent, sanctionContent: v.sanctionContent, fineAmount: v.fineAmount, status: v.status });
    else create.mutate({ caseNo: v.caseNo, group: v.group, orgUnitId: v.orgUnitId,
      businessName: v.businessName, inspectedOn, violationContent: v.violationContent });
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo số hồ sơ / tên cơ sở" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm hồ sơ</Button>
        <Button onClick={() => setImportOpen(true)}>Nhập Excel/XML</Button>
      </Space>

      <Table<Violation>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{
          current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); },
        }}
        columns={[
          { title: 'Số hồ sơ', dataIndex: 'caseNo', width: 150 },
          { title: 'Cơ sở kinh doanh', dataIndex: 'businessName' },
          { title: 'Nhóm', dataIndex: 'group', width: 240, render: (g: ViolationGroup) => groupLabels[g] },
          { title: 'Ngày kiểm tra', dataIndex: 'inspectedOn', width: 130 },
          { title: 'Tiền phạt', dataIndex: 'fineAmount', width: 130, render: (v: number | null) => v?.toLocaleString('vi-VN') ?? '—' },
          {
            title: 'Trạng thái', dataIndex: 'status', width: 130,
            render: (s: ViolationStatus) => <Tag color={statusColors[s]}>{statusLabels[s]}</Tag>,
          },
          { title: 'Thao tác', width: 130, render: (_, r) => (
            <Space>
              <a onClick={() => openEdit(r)}>Sửa</a>
              <Popconfirm title="Xoá?" okText="Xoá" cancelText="Huỷ" onConfirm={() => remove.mutate(r.id)}>
                <a style={{ color: '#cf1322' }}>Xoá</a>
              </Popconfirm>
            </Space>) },
        ]}
      />

      <Modal title={editing ? 'Sửa hồ sơ vi phạm' : 'Thêm hồ sơ vi phạm'} open={open} onCancel={close}
        onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="caseNo" label="Số hồ sơ" rules={[{ required: true }]}><Input maxLength={50} disabled={!!editing} /></Form.Item>
          <Form.Item name="group" label="Nhóm vi phạm" rules={[{ required: true }]} initialValue={1}>
            <Select options={groupOptions} />
          </Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị quản lý" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label" disabled={!!editing}
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Form.Item name="businessName" label="Cơ sở kinh doanh" rules={[{ required: true }]}><Input maxLength={300} /></Form.Item>
          <Form.Item name="inspectedOn" label="Ngày kiểm tra" rules={[{ required: true }]} initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
          <Form.Item name="violationContent" label="Nội dung vi phạm" rules={[{ required: true }]}>
            <Input.TextArea rows={3} />
          </Form.Item>
          {editing && (
            <>
              <Form.Item name="status" label="Trạng thái" rules={[{ required: true }]}>
                <Select options={statusOptions} />
              </Form.Item>
              <Form.Item name="sanctionContent" label="Nội dung xử phạt">
                <Input.TextArea rows={2} />
              </Form.Item>
              <Form.Item name="fineAmount" label="Tiền phạt (VNĐ)">
                <InputNumber<number> min={0} step={100000} style={{ width: '100%' }}
                  formatter={(v) => `${v ?? ''}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={(v) => Number((v ?? '').replace(/,/g, ''))} />
              </Form.Item>
            </>
          )}
        </Form>
      </Modal>

      <ImportModal
        open={importOpen} onClose={() => setImportOpen(false)}
        title="Nhập hồ sơ vi phạm" templateName="mau-ho-so-vi-pham"
        columns={[
          { header: 'Số hồ sơ', required: true, example: 'VP001' },
          { header: 'Nhóm', required: true, example: 'Vệ sinh, an toàn thực phẩm' },
          { header: 'Mã đơn vị', required: true, example: 'DV001' },
          { header: 'Cơ sở kinh doanh', required: true, example: 'Hộ KD A' },
          { header: 'Ngày kiểm tra', required: true, example: '2026-06-01' },
          { header: 'Nội dung vi phạm', required: true, example: 'Không niêm yết giá' },
        ]}
        mapRow={mapRow} commit={bulkImportViolations}
        onDone={() => qc.invalidateQueries({ queryKey: ['violations'] })} />
    </Space>
  );
}
