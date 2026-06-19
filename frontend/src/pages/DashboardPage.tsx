import { Button, Card, Col, Empty, Row, Space, Statistic, Typography } from 'antd';
import { DownloadOutlined } from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import {
  Bar, BarChart, CartesianGrid, Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis,
} from 'recharts';
import {
  getCommerceByType, getDashboard, getObservationsBySector, getReportingSummary, getViolationsSummary,
} from '../api/client';
import { downloadCsv } from '../lib/csv';

const reportStateLabels: Record<number, string> = {
  1: 'Nháp', 2: 'Đã gửi', 3: 'Thẩm định', 4: 'Chờ duyệt', 5: 'Đã duyệt', 6: 'Từ chối',
};
const violationStatusLabels: Record<number, string> = { 1: 'Đã ghi nhận', 2: 'Đang xử lý', 3: 'Đã xử lý' };
const sectorLabels: Record<number, string> = { 1: 'Công nghiệp', 2: 'Năng lượng', 3: 'Thương mại', 4: 'Quản lý thị trường' };
const commerceTypeLabels: Record<number, string> = { 1: 'Chợ', 2: 'Siêu thị', 3: 'TTTM', 4: 'Cửa hàng tiện lợi' };
const PALETTE = ['#1677ff', '#52c41a', '#fa8c16', '#eb2f96', '#722ed1', '#13c2c2', '#faad14', '#f5222d'];

const csvBtn = (onClick: () => void) =>
  <Button size="small" icon={<DownloadOutlined />} onClick={onClick}>CSV</Button>;

export default function DashboardPage() {
  const { data: d } = useQuery({ queryKey: ['dashboard'], queryFn: getDashboard });
  const { data: reporting } = useQuery({ queryKey: ['reporting-summary'], queryFn: getReportingSummary });
  const { data: violations } = useQuery({ queryKey: ['violations-summary'], queryFn: getViolationsSummary });
  const { data: bySector } = useQuery({ queryKey: ['observations-by-sector'], queryFn: getObservationsBySector });
  const { data: byCommerceType } = useQuery({ queryKey: ['commerce-by-type'], queryFn: getCommerceByType });

  const card = (title: string, value: number | undefined) => (
    <Col xs={12} sm={8} md={6} lg={4} key={title}>
      <Card><Statistic title={title} value={value ?? 0} /></Card>
    </Col>
  );

  const entityData = d ? [
    { name: 'Cụm CN', value: d.clusters },
    { name: 'Xăng dầu', value: d.petrolStations },
    { name: 'Thương mại', value: d.commerceLocations },
    { name: 'TMĐT', value: d.ecommerceParticipants },
    { name: 'Vi phạm', value: d.violations },
    { name: 'Số liệu', value: d.observations },
  ] : [];

  const reportingData = (reporting ?? []).map((r) => ({ name: reportStateLabels[r.state] ?? `#${r.state}`, value: r.count }));

  const violationByStatus = Object.entries(
    (violations ?? []).reduce<Record<number, number>>((acc, v) => {
      acc[v.status] = (acc[v.status] ?? 0) + v.count;
      return acc;
    }, {}),
  ).map(([status, count]) => ({ name: violationStatusLabels[Number(status)] ?? status, value: count }));

  const sectorData = (bySector ?? []).map((r) => ({
    name: sectorLabels[r.sector] ?? `#${r.sector}`, count: r.count, total: r.totalValue,
  }));
  const commerceData = (byCommerceType ?? []).map((r) => ({
    name: commerceTypeLabels[r.type] ?? `#${r.type}`, value: r.count,
  }));

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
        <Col xs={24} lg={12}>
          <Card title="Phân bố đối tượng quản lý">
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={entityData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" /><YAxis allowDecimals={false} /><Tooltip />
                <Bar dataKey="value" name="Số lượng" radius={[4, 4, 0, 0]}>
                  {entityData.map((_, i) => <Cell key={i} fill={PALETTE[i % PALETTE.length]} />)}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Báo cáo theo trạng thái"
            extra={reportingData.length ? csvBtn(() => downloadCsv('bao-cao-theo-trang-thai.csv',
              ['Trạng thái', 'Số lượng'], reportingData.map((r) => [r.name, r.value]))) : null}>
            {reportingData.length ? (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie data={reportingData} dataKey="value" nameKey="name" outerRadius={100} label>
                    {reportingData.map((_, i) => <Cell key={i} fill={PALETTE[i % PALETTE.length]} />)}
                  </Pie>
                  <Legend /><Tooltip />
                </PieChart>
              </ResponsiveContainer>
            ) : <Empty description="Chưa có báo cáo" />}
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <Card title="Hồ sơ vi phạm theo trạng thái"
            extra={violationByStatus.length ? csvBtn(() => downloadCsv('vi-pham-theo-trang-thai.csv',
              ['Trạng thái', 'Số hồ sơ'], violationByStatus.map((r) => [r.name, r.value]))) : null}>
            {violationByStatus.length ? (
              <ResponsiveContainer width="100%" height={280}>
                <BarChart data={violationByStatus}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" /><YAxis allowDecimals={false} /><Tooltip />
                  <Bar dataKey="value" name="Số hồ sơ" fill="#f5222d" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            ) : <Empty description="Chưa có hồ sơ vi phạm" />}
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Số liệu chỉ tiêu theo lĩnh vực"
            extra={sectorData.length ? csvBtn(() => downloadCsv('so-lieu-theo-linh-vuc.csv',
              ['Lĩnh vực', 'Số bản ghi', 'Tổng giá trị'], sectorData.map((r) => [r.name, r.count, r.total]))) : null}>
            {sectorData.length ? (
              <ResponsiveContainer width="100%" height={280}>
                <BarChart data={sectorData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" /><YAxis allowDecimals={false} /><Tooltip />
                  <Bar dataKey="count" name="Số bản ghi" fill="#1677ff" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            ) : <Empty description="Chưa có số liệu" />}
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <Card title="Địa điểm thương mại theo loại"
            extra={commerceData.length ? csvBtn(() => downloadCsv('thuong-mai-theo-loai.csv',
              ['Loại', 'Số lượng'], commerceData.map((r) => [r.name, r.value]))) : null}>
            {commerceData.length ? (
              <ResponsiveContainer width="100%" height={280}>
                <PieChart>
                  <Pie data={commerceData} dataKey="value" nameKey="name" outerRadius={100} label>
                    {commerceData.map((_, i) => <Cell key={i} fill={PALETTE[i % PALETTE.length]} />)}
                  </Pie>
                  <Legend /><Tooltip />
                </PieChart>
              </ResponsiveContainer>
            ) : <Empty description="Chưa có dữ liệu" />}
          </Card>
        </Col>
      </Row>
    </Space>
  );
}
