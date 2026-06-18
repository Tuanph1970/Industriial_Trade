import { Card, Col, Row, Space, Statistic, Table, Tag, Typography } from 'antd';
import { useQuery } from '@tanstack/react-query';
import { getDashboard, getReportingSummary, getViolationsSummary, StateCount, ViolationSummaryRow } from '../api/client';

const reportStateLabels: Record<number, string> = {
  1: 'Nháp', 2: 'Đã gửi', 3: 'Đang thẩm định', 4: 'Chờ phê duyệt', 5: 'Đã duyệt', 6: 'Bị từ chối',
};
const violationGroupLabels: Record<number, string> = { 1: 'Hàng cấm/giả/nhái', 2: 'An toàn thực phẩm' };
const violationStatusLabels: Record<number, string> = { 1: 'Đã ghi nhận', 2: 'Đang xử lý', 3: 'Đã xử lý' };

export default function DashboardPage() {
  const { data: d } = useQuery({ queryKey: ['dashboard'], queryFn: getDashboard });
  const { data: reporting } = useQuery({ queryKey: ['reporting-summary'], queryFn: getReportingSummary });
  const { data: violations } = useQuery({ queryKey: ['violations-summary'], queryFn: getViolationsSummary });

  const card = (title: string, value: number | undefined) => (
    <Col xs={12} sm={8} md={6} lg={4} key={title}>
      <Card><Statistic title={title} value={value ?? 0} /></Card>
    </Col>
  );

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Typography.Title level={5} style={{ margin: 0 }}>Tổng quan ngành Công Thương</Typography.Title>

      <Row gutter={[16, 16]}>
        {card('Cụm công nghiệp', d?.clusters)}
        {card('Cửa hàng xăng dầu', d?.petrolStations)}
        {card('Địa điểm thương mại', d?.commerceLocations)}
        {card('Đơn vị TMĐT', d?.ecommerceParticipants)}
        {card('Hồ sơ vi phạm', d?.violations)}
        {card('Số liệu chỉ tiêu', d?.observations)}
        {card('Chỉ tiêu (danh mục)', d?.indicators)}
        {card('Kỳ báo cáo', d?.campaigns)}
        {card('Báo cáo', d?.submissions)}
        {card('Chờ phê duyệt', d?.pendingApproval)}
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} md={12}>
          <Card title="Báo cáo theo trạng thái">
            <Table<StateCount>
              rowKey="state" size="small" pagination={false} dataSource={reporting}
              columns={[
                { title: 'Trạng thái', dataIndex: 'state', render: (s: number) => reportStateLabels[s] ?? s },
                { title: 'Số lượng', dataIndex: 'count', align: 'right' },
              ]}
            />
          </Card>
        </Col>
        <Col xs={24} md={12}>
          <Card title="Hồ sơ vi phạm theo nhóm / trạng thái">
            <Table<ViolationSummaryRow>
              rowKey={(r) => `${r.group}-${r.status}`} size="small" pagination={false} dataSource={violations}
              columns={[
                { title: 'Nhóm', dataIndex: 'group', render: (g: number) => <Tag>{violationGroupLabels[g] ?? g}</Tag> },
                { title: 'Trạng thái', dataIndex: 'status', render: (s: number) => violationStatusLabels[s] ?? s },
                { title: 'Số HS', dataIndex: 'count', align: 'right' },
                { title: 'Tổng phạt (đ)', dataIndex: 'totalFine', align: 'right',
                  render: (v: number) => v?.toLocaleString('vi-VN') },
              ]}
            />
          </Card>
        </Col>
      </Row>
    </Space>
  );
}
