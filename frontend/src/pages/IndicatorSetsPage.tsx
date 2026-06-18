import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Select, Space, Table } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createIndicatorSet, getIndicators, getIndicatorSets, IndicatorSet } from '../api/client';

export default function IndicatorSetsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['indicator-sets', page, pageSize, keyword],
    queryFn: () => getIndicatorSets({ page, pageSize, keyword }),
  });
  const { data: indicators } = useQuery({ queryKey: ['indicators', 'all'], queryFn: () => getIndicators({ page: 1, pageSize: 200 }) });

  const create = useMutation({
    mutationFn: createIndicatorSet,
    onSuccess: () => { message.success('Đã tạo bộ chỉ tiêu'); setOpen(false); form.resetFields(); qc.invalidateQueries({ queryKey: ['indicator-sets'] }); },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền hoặc trùng mã)'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Input.Search placeholder="Tìm theo mã hoặc tên" allowClear style={{ width: 320 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Button type="primary" onClick={() => setOpen(true)}>Thêm mới</Button>
      </Space>
      <Table<IndicatorSet>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Mã', dataIndex: 'code', width: 150 },
          { title: 'Tên bộ chỉ tiêu', dataIndex: 'name' },
          { title: 'Số chỉ tiêu', dataIndex: 'indicatorIds', width: 130, render: (ids: string[]) => ids.length },
        ]}
      />
      <Modal title="Thêm bộ chỉ tiêu" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate(v)}>
          <Form.Item name="code" label="Mã" rules={[{ required: true }]}><Input maxLength={50} /></Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input maxLength={250} /></Form.Item>
          <Form.Item name="description" label="Mô tả"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="indicatorIds" label="Chỉ tiêu thành viên" initialValue={[]}>
            <Select mode="multiple" allowClear showSearch optionFilterProp="label"
              options={indicators?.items.map((i) => ({ value: i.id, label: `${i.name} (${i.code})` }))} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
