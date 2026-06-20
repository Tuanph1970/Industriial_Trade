import { lazy, Suspense } from 'react';
import { Badge, Button, Dropdown, Layout, Menu, Result, Space, Spin, Typography } from 'antd';
import {
  ApartmentOutlined, AuditOutlined, BarsOutlined, BellOutlined, CalendarOutlined, ClusterOutlined,
  ApiOutlined, DashboardOutlined, EnvironmentOutlined, FileSearchOutlined, FundOutlined, GoldOutlined,
  LogoutOutlined, SafetyCertificateOutlined, ShopOutlined, ShoppingOutlined, TeamOutlined, UserOutlined,
  WarningOutlined,
} from '@ant-design/icons';
import { Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { useQuery } from '@tanstack/react-query';
import { getUnreadCount } from './api/client';
// Routes are lazy-loaded so each page (and heavy libs like Recharts/Leaflet) ships as its own
// on-demand chunk instead of one large entry bundle.
const OrgUnitsPage = lazy(() => import('./pages/OrgUnitsPage'));
const UsersPage = lazy(() => import('./pages/UsersPage'));
const RolesPage = lazy(() => import('./pages/RolesPage'));
const IndicatorsPage = lazy(() => import('./pages/IndicatorsPage'));
const ClustersPage = lazy(() => import('./pages/ClustersPage'));
const ObservationsPage = lazy(() => import('./pages/ObservationsPage'));
const ViolationsPage = lazy(() => import('./pages/ViolationsPage'));
const PetrolStationsPage = lazy(() => import('./pages/PetrolStationsPage'));
const CommerceLocationsPage = lazy(() => import('./pages/CommerceLocationsPage'));
const EcommercePage = lazy(() => import('./pages/EcommercePage'));
const CampaignsPage = lazy(() => import('./pages/CampaignsPage'));
const SubmissionsPage = lazy(() => import('./pages/SubmissionsPage'));
const NotificationsPage = lazy(() => import('./pages/NotificationsPage'));
const DashboardPage = lazy(() => import('./pages/DashboardPage'));
const AuditLogsPage = lazy(() => import('./pages/AuditLogsPage'));
const IntegrationPage = lazy(() => import('./pages/IntegrationPage'));
const MapPage = lazy(() => import('./pages/MapPage'));
const IndicatorSetsPage = lazy(() => import('./pages/IndicatorSetsPage'));
const ReportTemplatesPage = lazy(() => import('./pages/ReportTemplatesPage'));
const ReportingPeriodsPage = lazy(() => import('./pages/ReportingPeriodsPage'));
const AdministrativeUnitsPage = lazy(() => import('./pages/AdministrativeUnitsPage'));
const ClassificationsPage = lazy(() => import('./pages/ClassificationsPage'));
const FilesPage = lazy(() => import('./pages/FilesPage'));

const { Header, Sider, Content } = Layout;

const navItems = [
  { key: 'dashboard', icon: <DashboardOutlined />, label: 'Tổng quan' },
  { key: 'map', icon: <EnvironmentOutlined />, label: 'Bản đồ' },
  { key: 'org-units', icon: <ApartmentOutlined />, label: 'Cơ quan, đơn vị' },
  { key: 'users', icon: <TeamOutlined />, label: 'Người dùng' },
  { key: 'roles', icon: <SafetyCertificateOutlined />, label: 'Vai trò' },
  {
    key: 'catalog', icon: <BarsOutlined />, label: 'Danh mục',
    children: [
      { key: 'indicators', label: 'Chỉ tiêu thống kê' },
      { key: 'indicator-sets', label: 'Bộ chỉ tiêu' },
      { key: 'report-templates', label: 'Biểu mẫu báo cáo' },
      { key: 'reporting-periods', label: 'Kỳ báo cáo (danh mục)' },
      { key: 'administrative-units', label: 'Đơn vị hành chính' },
      { key: 'classifications', label: 'Danh mục phân loại' },
    ],
  },
  { key: 'clusters', icon: <ClusterOutlined />, label: 'Cụm công nghiệp' },
  { key: 'observations', icon: <FundOutlined />, label: 'Số liệu chỉ tiêu' },
  { key: 'petrol-stations', icon: <GoldOutlined />, label: 'Cửa hàng xăng dầu' },
  { key: 'commerce-locations', icon: <ShopOutlined />, label: 'Địa điểm thương mại' },
  { key: 'ecommerce', icon: <ShoppingOutlined />, label: 'Thương mại điện tử' },
  { key: 'violations', icon: <WarningOutlined />, label: 'Hồ sơ vi phạm' },
  { key: 'campaigns', icon: <CalendarOutlined />, label: 'Kỳ báo cáo' },
  { key: 'submissions', icon: <AuditOutlined />, label: 'Báo cáo & phê duyệt' },
  { key: 'files', icon: <FileSearchOutlined />, label: 'Tài liệu, tệp tin' },
  { key: 'audit', icon: <FileSearchOutlined />, label: 'Nhật ký quản trị' },
  { key: 'integration', icon: <ApiOutlined />, label: 'Liên thông & chia sẻ' },
];

export default function App() {
  const navigate = useNavigate();
  const location = useLocation();
  const auth = useAuth();

  const { data: unreadCount } = useQuery({
    queryKey: ['notifications-unread'],
    queryFn: getUnreadCount,
    enabled: auth.isAuthenticated,
    refetchInterval: 30_000,
  });

  if (auth.isLoading) return <Spin fullscreen tip="Đang tải..." />;

  if (!auth.isAuthenticated) {
    return (
      <Result
        status="403"
        title="Yêu cầu đăng nhập"
        subTitle="Vui lòng đăng nhập bằng tài khoản ngành Công Thương."
        extra={<Button type="primary" onClick={() => void auth.signinRedirect()}>Đăng nhập</Button>}
      />
    );
  }

  const userName = auth.user?.profile.preferred_username ?? 'Người dùng';
  const selectedKey = location.pathname.split('/')[1] || 'dashboard';

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header
        style={{
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          background: '#fff', borderBottom: '1px solid #f0f0f0', paddingInline: 24,
        }}
      >
        <Typography.Title level={4} style={{ margin: 0, color: '#1677ff' }}>
          CSDL ngành Công Thương — Hưng Yên
        </Typography.Title>
        <Space size="middle">
          <Badge count={unreadCount ?? 0} size="small">
            <Button type="text" icon={<BellOutlined />} onClick={() => navigate('/notifications')} />
          </Badge>
          <Dropdown
            menu={{
              items: [{ key: 'logout', icon: <LogoutOutlined />, label: 'Đăng xuất' }],
              onClick: () => void auth.signoutRedirect(),
            }}
          >
            <Button type="text" icon={<UserOutlined />}>{userName}</Button>
          </Dropdown>
        </Space>
      </Header>
      <Layout>
        <Sider width={240} theme="light">
          <Menu
            mode="inline"
            selectedKeys={[selectedKey]}
            defaultOpenKeys={['catalog']}
            onClick={({ key }) => navigate(`/${key}`)}
            style={{ height: '100%', borderInlineEnd: 0 }}
            items={navItems}
          />
        </Sider>
        <Content style={{ padding: 24, background: '#fff' }}>
          <Suspense fallback={<Spin style={{ display: 'block', margin: '80px auto' }} tip="Đang tải..." />}>
          <Routes>
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/map" element={<MapPage />} />
            <Route path="/org-units" element={<OrgUnitsPage />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/roles" element={<RolesPage />} />
            <Route path="/indicators" element={<IndicatorsPage />} />
            <Route path="/indicator-sets" element={<IndicatorSetsPage />} />
            <Route path="/report-templates" element={<ReportTemplatesPage />} />
            <Route path="/reporting-periods" element={<ReportingPeriodsPage />} />
            <Route path="/administrative-units" element={<AdministrativeUnitsPage />} />
            <Route path="/classifications" element={<ClassificationsPage />} />
            <Route path="/clusters" element={<ClustersPage />} />
            <Route path="/observations" element={<ObservationsPage />} />
            <Route path="/petrol-stations" element={<PetrolStationsPage />} />
            <Route path="/commerce-locations" element={<CommerceLocationsPage />} />
            <Route path="/ecommerce" element={<EcommercePage />} />
            <Route path="/violations" element={<ViolationsPage />} />
            <Route path="/campaigns" element={<CampaignsPage />} />
            <Route path="/submissions" element={<SubmissionsPage />} />
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/files" element={<FilesPage />} />
            <Route path="/audit" element={<AuditLogsPage />} />
            <Route path="/integration" element={<IntegrationPage />} />
          </Routes>
          </Suspense>
        </Content>
      </Layout>
    </Layout>
  );
}
