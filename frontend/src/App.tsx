import { Badge, Button, Dropdown, Layout, Menu, Result, Space, Spin, Typography } from 'antd';
import {
  ApartmentOutlined, AuditOutlined, BarsOutlined, BellOutlined, CalendarOutlined, ClusterOutlined,
  ApiOutlined, DashboardOutlined, FileSearchOutlined, FundOutlined, GoldOutlined, LogoutOutlined,
  SafetyCertificateOutlined, ShopOutlined, ShoppingOutlined, TeamOutlined, UserOutlined, WarningOutlined,
} from '@ant-design/icons';
import { Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { useQuery } from '@tanstack/react-query';
import { getUnreadCount } from './api/client';
import OrgUnitsPage from './pages/OrgUnitsPage';
import UsersPage from './pages/UsersPage';
import RolesPage from './pages/RolesPage';
import IndicatorsPage from './pages/IndicatorsPage';
import ClustersPage from './pages/ClustersPage';
import ObservationsPage from './pages/ObservationsPage';
import ViolationsPage from './pages/ViolationsPage';
import PetrolStationsPage from './pages/PetrolStationsPage';
import CommerceLocationsPage from './pages/CommerceLocationsPage';
import EcommercePage from './pages/EcommercePage';
import CampaignsPage from './pages/CampaignsPage';
import SubmissionsPage from './pages/SubmissionsPage';
import NotificationsPage from './pages/NotificationsPage';
import DashboardPage from './pages/DashboardPage';
import AuditLogsPage from './pages/AuditLogsPage';
import IntegrationPage from './pages/IntegrationPage';

const { Header, Sider, Content } = Layout;

const navItems = [
  { key: 'dashboard', icon: <DashboardOutlined />, label: 'Tổng quan' },
  { key: 'org-units', icon: <ApartmentOutlined />, label: 'Cơ quan, đơn vị' },
  { key: 'users', icon: <TeamOutlined />, label: 'Người dùng' },
  { key: 'roles', icon: <SafetyCertificateOutlined />, label: 'Vai trò' },
  { key: 'indicators', icon: <BarsOutlined />, label: 'Chỉ tiêu thống kê' },
  { key: 'clusters', icon: <ClusterOutlined />, label: 'Cụm công nghiệp' },
  { key: 'observations', icon: <FundOutlined />, label: 'Số liệu chỉ tiêu' },
  { key: 'petrol-stations', icon: <GoldOutlined />, label: 'Cửa hàng xăng dầu' },
  { key: 'commerce-locations', icon: <ShopOutlined />, label: 'Địa điểm thương mại' },
  { key: 'ecommerce', icon: <ShoppingOutlined />, label: 'Thương mại điện tử' },
  { key: 'violations', icon: <WarningOutlined />, label: 'Hồ sơ vi phạm' },
  { key: 'campaigns', icon: <CalendarOutlined />, label: 'Kỳ báo cáo' },
  { key: 'submissions', icon: <AuditOutlined />, label: 'Báo cáo & phê duyệt' },
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
            onClick={({ key }) => navigate(`/${key}`)}
            style={{ height: '100%', borderInlineEnd: 0 }}
            items={navItems}
          />
        </Sider>
        <Content style={{ padding: 24, background: '#fff' }}>
          <Routes>
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/org-units" element={<OrgUnitsPage />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/roles" element={<RolesPage />} />
            <Route path="/indicators" element={<IndicatorsPage />} />
            <Route path="/clusters" element={<ClustersPage />} />
            <Route path="/observations" element={<ObservationsPage />} />
            <Route path="/petrol-stations" element={<PetrolStationsPage />} />
            <Route path="/commerce-locations" element={<CommerceLocationsPage />} />
            <Route path="/ecommerce" element={<EcommercePage />} />
            <Route path="/violations" element={<ViolationsPage />} />
            <Route path="/campaigns" element={<CampaignsPage />} />
            <Route path="/submissions" element={<SubmissionsPage />} />
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/audit" element={<AuditLogsPage />} />
            <Route path="/integration" element={<IntegrationPage />} />
          </Routes>
        </Content>
      </Layout>
    </Layout>
  );
}
