import axios from 'axios';
import { userManager } from '../auth';

// Same-origin '/api' in dev (Vite proxy) and prod (Nginx).
export const api = axios.create({ baseURL: '/' });

// Attach the Keycloak access token to every request.
api.interceptors.request.use(async (config) => {
  const user = await userManager.getUser();
  if (user?.access_token) {
    config.headers.Authorization = `Bearer ${user.access_token}`;
  }
  return config;
});

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export type OrgUnitType = 1 | 2 | 3; // Department | Division | Commune

export interface OrgUnit {
  id: string;
  code: string;
  name: string;
  type: OrgUnitType;
  parentId: string | null;
  path: string;
  isActive: boolean;
}

export async function getOrgUnits(page: number, pageSize: number, keyword?: string) {
  const { data } = await api.get<PagedResult<OrgUnit>>('/api/identity/org-units', {
    params: { page, pageSize, keyword: keyword || undefined },
  });
  return data;
}

export async function createOrgUnit(input: {
  code: string;
  name: string;
  type: OrgUnitType;
  parentId?: string | null;
}) {
  const { data } = await api.post<{ id: string }>('/api/identity/org-units', input);
  return data;
}
