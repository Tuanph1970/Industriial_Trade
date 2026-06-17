import axios from 'axios';

// Same-origin '/api' in dev (Vite proxy) and prod (Nginx). A Keycloak bearer-token
// interceptor is added in Phase 1.
export const api = axios.create({ baseURL: '/' });

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
