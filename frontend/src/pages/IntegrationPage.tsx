import { useState } from 'react';
import { App as AntApp, Badge, Button, Card, Form, Input, Modal, Select, Space, Table, Tag } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  changeServiceStatus, ComponentStatus, createService, DataSharingService, getConnectionStatus,
  getServices, ServiceDirection, ServiceLifecycleAction, ServiceStatus,
} from '../api/client';

const directionLabels: Record<ServiceDirection, string> = { 1: 'Cung cấp', 2: 'Khai thác' };
const statusLabels: Record<ServiceStatus, string> = { 1: 'Đã đăng ký', 2: 'Đã công bố', 3: 'Đã thu hồi' };
const statusColors: Record<ServiceStatus, string> = { 1: 'default', 2: 'green', 3: 'red' };

export default function IntegrationPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data: services, isLoading } = useQuery({
    queryKey: ['services', page, pageSize],
    queryFn: () => getServices({ page, pageSize }),
  });
  const { data: status } = useQuery({ queryKey: ['connection-status'], queryFn: getConnectionStatus, refetchInterval: 30_000 });

  const create = useMutation({
    mutationFn: createService,
    onSuccess: () => { message.success('Đã đăng ký dịch vụ'); setOpen(false); form.resetFields(); qc.invalidateQueries({ queryKey: ['services'] }); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });
  const changeStatus = useMutation({
    mutationFn: ({ id, action }: { id: string; action: 0 | 1 }) => changeServiceStatus(id, action),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['services'] }); qc.invalidateQueries({ queryKey: ['connection-status'] }); },
    onError: () => message.error('Thao tác không hợp lệ ở trạng thái hiện tại'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Card title={
        <Space>Tình trạng kết nối (LGSP/NDXP)
          <Badge status={status?.healthy ? 'success' : 'error'} text={status?.healthy ? 'Bình thường' : 'Có sự cố'} />
        </Space>
      }>
        <Table<ComponentStatus>
          rowKey="component" size="small" pagination={false} dataSource={status?.components}
          columns={[
            { title: 'Thành phần', dataIndex: 'component' },
            { title: 'Mức', dataIndex: 'level', width: 80, render: (l: number) => (l === 1 ? 'Hệ thống' : 'Dịch vụ') },
            { title: 'Tình trạng', dataIndex: 'healthy', width: 120,
              render: (h: boolean) => <Tag color={h ? 'green' : 'red'}>{h ? 'Kết nối' : 'Mất kết nối'}</Tag> },
            { title: 'Chi tiết', dataIndex: 'detail' },
          ]}
        />
      </Card>

      <Card title="Dịch vụ chia sẻ dữ liệu" extra={<Button type="primary" onClick={() => setOpen(true)}>Đăng ký dịch vụ</Button>}>
        <Table<DataSharingService>
          rowKey="id" loading={isLoading} dataSource={services?.items}
          pagination={{ current: page, pageSize, total: services?.totalCount, showSizeChanger: true,
            pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
          columns={[
            { title: 'Mã', dataIndex: 'code', width: 130 },
            { title: 'Tên dịch vụ', dataIndex: 'name' },
            { title: 'Chiều', dataIndex: 'direction', width: 110, render: (d: ServiceDirection) => directionLabels[d] },
            { title: 'Trạng thái', dataIndex: 'status', width: 130,
              render: (s: ServiceStatus) => <Tag color={statusColors[s]}>{statusLabels[s]}</Tag> },
            {
              title: 'Thao tác', width: 180,
              render: (_, r) => (
                <Space>
                  {r.status !== 2 && r.status !== 3 &&
                    <Button size="small" loading={changeStatus.isPending}
                      onClick={() => changeStatus.mutate({ id: r.id, action: ServiceLifecycleAction.Publish })}>Công bố</Button>}
                  {r.status === 2 &&
                    <Button size="small" danger loading={changeStatus.isPending}
                      onClick={() => changeStatus.mutate({ id: r.id, action: ServiceLifecycleAction.Revoke })}>Thu hồi</Button>}
                </Space>
              ),
            },
          ]}
        />
      </Card>

      <Modal title="Đăng ký dịch vụ chia sẻ dữ liệu" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate(v)}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} /></Form.Item>
          <Form.Item name="name" label="Tên dịch vụ" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="direction" label="Chiều" rules={[{ required: true }]} initialValue={1}>
            <Select options={[{ value: 1, label: 'Cung cấp' }, { value: 2, label: 'Khai thác' }]} />
          </Form.Item>
          <Form.Item name="endpointUrl" label="Địa chỉ kết nối"><Input maxLength={500} /></Form.Item>
          <Form.Item name="description" label="Mô tả"><Input.TextArea rows={2} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
