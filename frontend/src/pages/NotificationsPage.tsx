import { useState } from 'react';
import { App as AntApp, Button, List, Space, Tag, Typography } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { getNotifications, markAllNotificationsRead, markNotificationRead, Notification } from '../api/client';

export default function NotificationsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const { data, isLoading } = useQuery({
    queryKey: ['notifications', page],
    queryFn: () => getNotifications({ page, pageSize }),
  });

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ['notifications'] });
    qc.invalidateQueries({ queryKey: ['notifications-unread'] });
  };

  const markOne = useMutation({ mutationFn: markNotificationRead, onSuccess: invalidate });
  const markAll = useMutation({
    mutationFn: markAllNotificationsRead,
    onSuccess: () => { message.success('Đã đánh dấu tất cả là đã đọc'); invalidate(); },
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space style={{ justifyContent: 'space-between', width: '100%' }}>
        <Typography.Title level={5} style={{ margin: 0 }}>Thông báo</Typography.Title>
        <Button onClick={() => markAll.mutate()} loading={markAll.isPending}>Đánh dấu tất cả đã đọc</Button>
      </Space>

      <List<Notification>
        loading={isLoading}
        dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, onChange: setPage }}
        renderItem={(n) => (
          <List.Item
            actions={n.isRead ? [] : [<Button key="r" type="link" onClick={() => markOne.mutate(n.id)}>Đánh dấu đã đọc</Button>]}
          >
            <List.Item.Meta
              title={<Space>{n.title}{!n.isRead && <Tag color="blue">Mới</Tag>}</Space>}
              description={
                <div>
                  <div>{n.message}</div>
                  <Typography.Text type="secondary">{new Date(n.createdAtUtc).toLocaleString('vi-VN')}</Typography.Text>
                </div>
              }
            />
          </List.Item>
        )}
      />
    </Space>
  );
}
