import { useState } from 'react';
import { App as AntApp, Button, DatePicker, Form, Input, Modal, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import { createIndicator, getIndicators, Indicator, IndustrySector } from '../api/client';

const sectorLabels: Record<IndustrySector, string> = {
  1: 'Công nghiệp', 2: 'Năng lượng', 3: 'Thương mại', 4: 'Quản lý thị trường',
};
const dataTypeOptions = [
  { value: 1, label: 'Số' }, { value: 2, label: 'Văn bản' }, { value: 3, label: 'Danh mục' },
];
const sectorOptions = Object.entries(sectorLabels).map(([v, label]) => ({ value: Number(v), label }));

export default function IndicatorsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['indicators', page, pageSize, keyword],
    queryFn: () => getIndicators({ page, pageSize, keyword }),
  });

  const create = useMutation({
    mutationFn: createIndicator,
    onSuccess: () => {
      message.success('Đã tạo chỉ tiêu');
      setOpen(false);
      form.resetFields();
      qc.invalidateQueries({ queryKey: ['indicators'] });
    },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={() => setOpen(true)}>Thêm mới</Button>
      </Space>

      <Table<Indicator>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{
          current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); },
        }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 140 },
          { title: 'Tên chỉ tiêu', dataIndex: 'name' },
          { title: 'Đơn vị', dataIndex: 'unit', width: 100 },
          { title: 'Lĩnh vực', dataIndex: 'sector', width: 160, render: (s: IndustrySector) => sectorLabels[s] },
          { title: 'Phiên bản', dataIndex: 'version', width: 100 },
          {
            title: 'Trạng thái', dataIndex: 'isActive', width: 120,
            render: (a: boolean) => <Tag color={a ? 'green' : 'default'}>{a ? 'Hiệu lực' : 'Đã thu hồi'}</Tag>,
          },
        ]}
      />

      <Modal title="Thêm chỉ tiêu thống kê" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical"
          onFinish={(v) => create.mutate({ ...v, effectiveFrom: dayjs(v.effectiveFrom).format('YYYY-MM-DD') })}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} /></Form.Item>
          <Form.Item name="name" label="Tên chỉ tiêu" rules={[{ required: true }]}><Input maxLength={300} /></Form.Item>
          <Form.Item name="unit" label="Đơn vị tính"><Input maxLength={50} /></Form.Item>
          <Form.Item name="dataType" label="Kiểu dữ liệu" rules={[{ required: true }]} initialValue={1}>
            <Select options={dataTypeOptions} />
          </Form.Item>
          <Form.Item name="sector" label="Lĩnh vực" rules={[{ required: true }]} initialValue={1}>
            <Select options={sectorOptions} />
          </Form.Item>
          <Form.Item name="effectiveFrom" label="Hiệu lực từ" rules={[{ required: true }]} initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
