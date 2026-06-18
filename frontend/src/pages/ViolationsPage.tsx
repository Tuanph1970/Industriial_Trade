import { useState } from 'react';
import { App as AntApp, Button, DatePicker, Form, Input, Modal, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import {
  createViolation, getOrgUnits, getViolations, Violation, ViolationGroup, ViolationStatus,
} from '../api/client';

const groupLabels: Record<ViolationGroup, string> = {
  1: 'Hàng cấm / giả / nhái / kém chất lượng', 2: 'Vệ sinh, an toàn thực phẩm',
};
const statusLabels: Record<ViolationStatus, string> = { 1: 'Đã ghi nhận', 2: 'Đang xử lý', 3: 'Đã xử lý' };
const statusColors: Record<ViolationStatus, string> = { 1: 'default', 2: 'gold', 3: 'green' };
const groupOptions = Object.entries(groupLabels).map(([v, label]) => ({ value: Number(v), label }));

export default function ViolationsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['violations', page, pageSize, keyword],
    queryFn: () => getViolations({ page, pageSize, keyword }),
  });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });

  const create = useMutation({
    mutationFn: createViolation,
    onSuccess: () => {
      message.success('Đã tạo hồ sơ vi phạm');
      setOpen(false);
      form.resetFields();
      qc.invalidateQueries({ queryKey: ['violations'] });
    },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng số hồ sơ)'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo số hồ sơ / tên cơ sở" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={() => setOpen(true)}>Thêm hồ sơ</Button>
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
        ]}
      />

      <Modal title="Thêm hồ sơ vi phạm" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical"
          onFinish={(v) => create.mutate({ ...v, inspectedOn: dayjs(v.inspectedOn).format('YYYY-MM-DD') })}>
          <Form.Item name="caseNo" label="Số hồ sơ" rules={[{ required: true }]}><Input maxLength={50} /></Form.Item>
          <Form.Item name="group" label="Nhóm vi phạm" rules={[{ required: true }]} initialValue={1}>
            <Select options={groupOptions} />
          </Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị quản lý" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label"
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Form.Item name="businessName" label="Cơ sở kinh doanh" rules={[{ required: true }]}><Input maxLength={300} /></Form.Item>
          <Form.Item name="inspectedOn" label="Ngày kiểm tra" rules={[{ required: true }]} initialValue={dayjs()}>
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
          <Form.Item name="violationContent" label="Nội dung vi phạm" rules={[{ required: true }]}>
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
