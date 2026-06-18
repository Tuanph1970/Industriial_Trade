import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Popconfirm, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createEcommerce, deleteEcommerce, EcommerceParticipant, getEcommerce, getOrgUnits } from '../api/client';

export default function EcommercePage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['ecommerce', page, pageSize, keyword],
    queryFn: () => getEcommerce({ page, pageSize, keyword }),
  });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const create = useMutation({
    mutationFn: createEcommerce,
    onSuccess: () => {
      message.success('Đã tạo đơn vị TMĐT');
      setOpen(false); form.resetFields();
      qc.invalidateQueries({ queryKey: ['ecommerce'] });
    },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã số thuế)'),
  });
  const remove = useMutation({
    mutationFn: deleteEcommerce,
    onSuccess: () => { message.success('Đã xoá'); qc.invalidateQueries({ queryKey: ['ecommerce'] }); },
    onError: () => message.error('Không xoá được'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo MST / tên doanh nghiệp" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={() => setOpen(true)}>Thêm mới</Button>
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
          { title: 'Thao tác', width: 90, render: (_, r) => (
            <Popconfirm title="Xoá?" okText="Xoá" cancelText="Huỷ" onConfirm={() => remove.mutate(r.id)}>
              <a style={{ color: '#cf1322' }}>Xoá</a>
            </Popconfirm>) },
        ]}
      />
      <Modal title="Thêm đơn vị thương mại điện tử" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate({ ...v, platforms: v.platforms ?? [] })}>
          <Form.Item name="taxCode" label="Mã số thuế" rules={[{ required: true }]}><Input maxLength={20} /></Form.Item>
          <Form.Item name="businessName" label="Tên doanh nghiệp" rules={[{ required: true }]}><Input maxLength={300} /></Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị quản lý" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label"
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Form.Item name="platforms" label="Sàn TMĐT (gõ và Enter)">
            <Select mode="tags" tokenSeparators={[',']} placeholder="Shopee, Lazada, Tiki…" />
          </Form.Item>
          <Form.Item name="mainGoods" label="Mặt hàng chính"><Input maxLength={1000} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
