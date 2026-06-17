import { Layout, Menu, Typography } from 'antd';
import { ApartmentOutlined } from '@ant-design/icons';
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import OrgUnitsPage from './pages/OrgUnitsPage';

const { Header, Sider, Content } = Layout;

export default function App() {
  const navigate = useNavigate();
  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ display: 'flex', alignItems: 'center' }}>
        <Typography.Title level={4} style={{ color: '#fff', margin: 0 }}>
          CSDL ngành Công Thương — Hưng Yên
        </Typography.Title>
      </Header>
      <Layout>
        <Sider width={240} theme="light">
          <Menu
            mode="inline"
            defaultSelectedKeys={['org-units']}
            onClick={({ key }) => navigate(`/${key}`)}
            items={[
              { key: 'org-units', icon: <ApartmentOutlined />, label: 'Cơ quan, đơn vị' },
            ]}
          />
        </Sider>
        <Content style={{ padding: 24 }}>
          <Routes>
            <Route path="/" element={<Navigate to="/org-units" replace />} />
            <Route path="/org-units" element={<OrgUnitsPage />} />
          </Routes>
        </Content>
      </Layout>
    </Layout>
  );
}
