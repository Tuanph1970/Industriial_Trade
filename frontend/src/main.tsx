import React from 'react';
import ReactDOM from 'react-dom/client';
import { App as AntApp, ConfigProvider, theme } from 'antd';
import viVN from 'antd/locale/vi_VN';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from 'react-oidc-context';
import App from './App';
import { onSigninCallback, userManager } from './auth';
import 'antd/dist/reset.css';

const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <AuthProvider userManager={userManager} onSigninCallback={onSigninCallback}>
      <ConfigProvider
        locale={viVN}
        theme={{ algorithm: theme.defaultAlgorithm, token: { colorPrimary: '#1677ff' } }}
      >
        <QueryClientProvider client={queryClient}>
          <AntApp>
            <BrowserRouter>
              <App />
            </BrowserRouter>
          </AntApp>
        </QueryClientProvider>
      </ConfigProvider>
    </AuthProvider>
  </React.StrictMode>,
);
