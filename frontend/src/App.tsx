import { Button, Dropdown, Layout, Menu, Result, Space, Spin, Typography } from 'antd';
import { ApartmentOutlined, LogoutOutlined, UserOutlined } from '@ant-design/icons';
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import OrgUnitsPage from './pages/OrgUnitsPage';

const { Header, Sider, Content } = Layout;

export default function App() {
  const navigate = useNavigate();
  const auth = useAuth();

  if (auth.isLoading) {
    return <Spin fullscreen tip="Đang tải..." />;
  }

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

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography.Title level={4} style={{ color: '#fff', margin: 0 }}>
          CSDL ngành Công Thương — Hưng Yên
        </Typography.Title>
        <Dropdown
          menu={{
            items: [{ key: 'logout', icon: <LogoutOutlined />, label: 'Đăng xuất' }],
            onClick: () => void auth.signoutRedirect(),
          }}
        >
          <Button type="text" style={{ color: '#fff' }} icon={<UserOutlined />}>
            {userName}
          </Button>
        </Dropdown>
      </Header>
      <Layout>
        <Sider width={240} theme="light">
          <Menu
            mode="inline"
            defaultSelectedKeys={['org-units']}
            onClick={({ key }) => navigate(`/${key}`)}
            items={[{ key: 'org-units', icon: <ApartmentOutlined />, label: 'Cơ quan, đơn vị' }]}
          />
        </Sider>
        <Content style={{ padding: 24 }}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <Routes>
              <Route path="/" element={<Navigate to="/org-units" replace />} />
              <Route path="/org-units" element={<OrgUnitsPage />} />
            </Routes>
          </Space>
        </Content>
      </Layout>
    </Layout>
  );
}
