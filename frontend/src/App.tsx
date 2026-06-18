import { Button, Dropdown, Layout, Menu, Result, Spin, Typography } from 'antd';
import {
  ApartmentOutlined, BarsOutlined, ClusterOutlined, FundOutlined, LogoutOutlined,
  SafetyCertificateOutlined, TeamOutlined, UserOutlined,
} from '@ant-design/icons';
import { Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import OrgUnitsPage from './pages/OrgUnitsPage';
import UsersPage from './pages/UsersPage';
import RolesPage from './pages/RolesPage';
import IndicatorsPage from './pages/IndicatorsPage';
import ClustersPage from './pages/ClustersPage';
import ObservationsPage from './pages/ObservationsPage';

const { Header, Sider, Content } = Layout;

const navItems = [
  { key: 'org-units', icon: <ApartmentOutlined />, label: 'Cơ quan, đơn vị' },
  { key: 'users', icon: <TeamOutlined />, label: 'Người dùng' },
  { key: 'roles', icon: <SafetyCertificateOutlined />, label: 'Vai trò' },
  { key: 'indicators', icon: <BarsOutlined />, label: 'Chỉ tiêu thống kê' },
  { key: 'clusters', icon: <ClusterOutlined />, label: 'Cụm công nghiệp' },
  { key: 'observations', icon: <FundOutlined />, label: 'Số liệu chỉ tiêu' },
];

export default function App() {
  const navigate = useNavigate();
  const location = useLocation();
  const auth = useAuth();

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
  const selectedKey = location.pathname.split('/')[1] || 'org-units';

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
        <Dropdown
          menu={{
            items: [{ key: 'logout', icon: <LogoutOutlined />, label: 'Đăng xuất' }],
            onClick: () => void auth.signoutRedirect(),
          }}
        >
          <Button type="text" icon={<UserOutlined />}>{userName}</Button>
        </Dropdown>
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
            <Route path="/" element={<Navigate to="/org-units" replace />} />
            <Route path="/org-units" element={<OrgUnitsPage />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/roles" element={<RolesPage />} />
            <Route path="/indicators" element={<IndicatorsPage />} />
            <Route path="/clusters" element={<ClustersPage />} />
            <Route path="/observations" element={<ObservationsPage />} />
          </Routes>
        </Content>
      </Layout>
    </Layout>
  );
}
