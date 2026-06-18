import { useState } from 'react';
import { Input, Space, Table, Tag, Typography } from 'antd';
import { useQuery } from '@tanstack/react-query';
import { AuditLog, getAuditLogs } from '../api/client';

export default function AuditLogsPage() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [actor, setActor] = useState('');
  const [action, setAction] = useState('');

  const { data, isLoading } = useQuery({
    queryKey: ['audit', page, pageSize, actor, action],
    queryFn: () => getAuditLogs({ page, pageSize, actor, action }),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Typography.Title level={5} style={{ margin: 0 }}>Nhật ký quản trị</Typography.Title>
      <Space>
        <Input.Search placeholder="Người dùng" allowClear style={{ width: 220 }}
          onSearch={(v) => { setActor(v); setPage(1); }} />
        <Input.Search placeholder="Hành động (vd: CreateOrgUnitCommand)" allowClear style={{ width: 320 }}
          onSearch={(v) => { setAction(v); setPage(1); }} />
      </Space>

      <Table<AuditLog>
        rowKey="id" loading={isLoading} dataSource={data?.items} size="small"
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [20, 50, 100], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        expandable={{
          expandedRowRender: (r) => (
            <pre style={{ margin: 0, whiteSpace: 'pre-wrap' }}>
              {r.payload}{r.error ? `\n\nLỗi: ${r.error}` : ''}
            </pre>
          ),
        }}
        columns={[
          { title: 'Thời gian', dataIndex: 'atUtc', width: 190, render: (v: string) => new Date(v).toLocaleString('vi-VN') },
          { title: 'Người dùng', dataIndex: 'actor', width: 160 },
          { title: 'Hành động', dataIndex: 'action' },
          { title: 'Kết quả', dataIndex: 'success', width: 110,
            render: (s: boolean) => <Tag color={s ? 'green' : 'red'}>{s ? 'Thành công' : 'Thất bại'}</Tag> },
        ]}
      />
    </Space>
  );
}
