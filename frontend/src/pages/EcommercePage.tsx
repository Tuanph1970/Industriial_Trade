import { useCallback, useMemo, useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Popconfirm, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  bulkImportEcommerce, createEcommerce, deleteEcommerce, EcommerceParticipant, getEcommerce,
  getOrgUnits, updateEcommerce,
} from '../api/client';
import ImportModal, { getCell } from '../components/ImportModal';

export default function EcommercePage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const [editing, setEditing] = useState<EcommerceParticipant | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['ecommerce', page, pageSize, keyword],
    queryFn: () => getEcommerce({ page, pageSize, keyword }),
  });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const unitByCode = useMemo(() => new Map((units?.items ?? []).map((u) => [u.code, u.id])), [units]);
  const mapRow = useCallback((cells: Record<string, string>) => {
    const errors: string[] = [];
    const taxCode = getCell(cells, 'Mã số thuế');
    const businessName = getCell(cells, 'Tên doanh nghiệp');
    const unitCode = getCell(cells, 'Mã đơn vị');
    const orgUnitId = unitByCode.get(unitCode);
    const platforms = getCell(cells, 'Sàn TMĐT').split(/[;,]/).map((p) => p.trim()).filter(Boolean);

    if (!taxCode) errors.push('Thiếu mã số thuế');
    if (!businessName) errors.push('Thiếu tên doanh nghiệp');
    if (!orgUnitId) errors.push(`Không tìm thấy đơn vị '${unitCode}'`);

    if (errors.length) return { errors };
    return {
      errors,
      item: { taxCode, businessName, orgUnitId, platforms, mainGoods: getCell(cells, 'Mặt hàng chính') || null },
    };
  }, [unitByCode]);

  const invalidate = () => qc.invalidateQueries({ queryKey: ['ecommerce'] });
  const create = useMutation({
    mutationFn: createEcommerce,
    onSuccess: () => { message.success('Đã tạo đơn vị TMĐT'); close(); invalidate(); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã số thuế)'),
  });
  const update = useMutation({
    mutationFn: (v: { id: string } & Parameters<typeof updateEcommerce>[1]) => updateEcommerce(v.id, v),
    onSuccess: () => { message.success('Đã cập nhật'); close(); invalidate(); },
    onError: () => message.error('Cập nhật thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteEcommerce,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  function close() { setOpen(false); setEditing(null); form.resetFields(); }
  function openCreate() { setEditing(null); form.resetFields(); setOpen(true); }
  function openEdit(r: EcommerceParticipant) {
    setEditing(r);
    form.setFieldsValue({ taxCode: r.taxCode, businessName: r.businessName, orgUnitId: r.orgUnitId,
      platforms: r.platforms, mainGoods: r.mainGoods });
    setOpen(true);
  }
  function submit(v: { taxCode: string; businessName: string; orgUnitId: string;
    platforms?: string[]; mainGoods?: string | null }) {
    if (editing) update.mutate({ id: editing.id, businessName: v.businessName,
      platforms: v.platforms ?? [], mainGoods: v.mainGoods });
    else create.mutate({ ...v, platforms: v.platforms ?? [] });
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo MST / tên doanh nghiệp" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={openCreate}>Thêm mới</Button>
        <Button onClick={() => setImportOpen(true)}>Nhập Excel/XML</Button>
      </Space>
      <Table<EcommerceParticipant>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã số thuế', dataIndex: 'taxCode', width: 160 },
          { title: 'Doanh nghiệp', dataIndex: 'businessName' },
          { title: 'Sàn TMĐT', dataIndex: 'platforms',
            render: (ps: string[]) => <>{ps.map((p) => <Tag key={p}>{p}</Tag>)}</> },
          { title: 'Mặt hàng chính', dataIndex: 'mainGoods' },
          { title: 'Thao tác', width: 130, render: (_, r) => (
            <Space>
              <a onClick={() => openEdit(r)}>Sửa</a>
              <Popconfirm title="Xoá?" okText="Xoá" cancelText="Huỷ" onConfirm={() => remove.mutate(r.id)}>
                <a style={{ color: '#cf1322' }}>Xoá</a>
              </Popconfirm>
            </Space>) },
        ]}
      />
      <Modal title={editing ? 'Sửa đơn vị thương mại điện tử' : 'Thêm đơn vị thương mại điện tử'} open={open} onCancel={close}
        onOk={() => form.submit()} confirmLoading={create.isPending || update.isPending}>
        <Form form={form} layout="vertical" onFinish={submit}>
          <Form.Item name="taxCode" label="Mã số thuế" rules={[{ required: true }]}><Input maxLength={20} disabled={!!editing} /></Form.Item>
          <Form.Item name="businessName" label="Tên doanh nghiệp" rules={[{ required: true }]}><Input maxLength={300} /></Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị quản lý" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label" disabled={!!editing}
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Form.Item name="platforms" label="Sàn TMĐT (gõ và Enter)">
            <Select mode="tags" tokenSeparators={[',']} placeholder="Shopee, Lazada, Tiki…" />
          </Form.Item>
          <Form.Item name="mainGoods" label="Mặt hàng chính"><Input maxLength={1000} /></Form.Item>
        </Form>
      </Modal>

      <ImportModal
        open={importOpen} onClose={() => setImportOpen(false)}
        title="Nhập đơn vị thương mại điện tử" templateName="mau-tmdt"
        columns={[
          { header: 'Mã số thuế', required: true, example: '0123456789' },
          { header: 'Tên doanh nghiệp', required: true, example: 'Công ty A' },
          { header: 'Mã đơn vị', required: true, example: 'DV001' },
          { header: 'Sàn TMĐT', example: 'Shopee; Lazada' },
          { header: 'Mặt hàng chính', example: 'Đồ gia dụng' },
        ]}
        mapRow={mapRow} commit={bulkImportEcommerce} onDone={invalidate} />
    </Space>
  );
}
