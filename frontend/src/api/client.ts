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

interface PageParams {
  page: number;
  pageSize: number;
  keyword?: string;
}

const pageParams = (p: PageParams) => ({ page: p.page, pageSize: p.pageSize, keyword: p.keyword || undefined });

// ---- Org units -----------------------------------------------------------
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

export const getOrgUnits = (p: PageParams) =>
  api.get<PagedResult<OrgUnit>>('/api/identity/org-units', { params: pageParams(p) }).then((r) => r.data);

export const createOrgUnit = (b: { code: string; name: string; type: OrgUnitType; parentId?: string | null }) =>
  api.post<{ id: string }>('/api/identity/org-units', b).then((r) => r.data);

// ---- Roles ---------------------------------------------------------------
export interface Role {
  id: string;
  code: string;
  name: string;
  permissions: string[];
  isActive: boolean;
}

export const ALL_PERMISSIONS = [
  'identity.orgunits.read', 'identity.orgunits.manage',
  'identity.users.read', 'identity.users.manage',
  'identity.roles.read', 'identity.roles.manage',
  'catalog.indicators.read', 'catalog.indicators.manage',
];

export const getRoles = (p: PageParams) =>
  api.get<PagedResult<Role>>('/api/identity/roles', { params: pageParams(p) }).then((r) => r.data);

export const createRole = (b: { code: string; name: string; permissions: string[] }) =>
  api.post<{ id: string }>('/api/identity/roles', b).then((r) => r.data);

// ---- Users ---------------------------------------------------------------
export interface UserAccount {
  id: string;
  userName: string;
  fullName: string | null;
  email: string | null;
  orgUnitId: string | null;
  roleIds: string[];
  isActive: boolean;
}

export const getUsers = (p: PageParams) =>
  api.get<PagedResult<UserAccount>>('/api/identity/users', { params: pageParams(p) }).then((r) => r.data);

export const createUser = (b: {
  userName: string; fullName?: string; email?: string; orgUnitId?: string | null; roleIds: string[];
}) => api.post<{ id: string }>('/api/identity/users', b).then((r) => r.data);

// ---- Indicators (Catalog) ------------------------------------------------
export type IndicatorDataType = 1 | 2 | 3; // Number | Text | Enumeration
export type IndustrySector = 1 | 2 | 3 | 4; // Industry | Energy | Commerce | MarketSurveillance

export interface Indicator {
  id: string;
  code: string;
  name: string;
  unit: string;
  dataType: IndicatorDataType;
  sector: IndustrySector;
  effectiveFrom: string;
  retiredAt: string | null;
  version: number;
  isActive: boolean;
}

export const getIndicators = (p: PageParams & { sector?: IndustrySector }) =>
  api.get<PagedResult<Indicator>>('/api/catalog/indicators', {
    params: { ...pageParams(p), sector: p.sector },
  }).then((r) => r.data);

export const createIndicator = (b: {
  code: string; name: string; unit: string;
  dataType: IndicatorDataType; sector: IndustrySector; effectiveFrom: string;
}) => api.post<{ id: string }>('/api/catalog/indicators', b).then((r) => r.data);

// ---- Sector Data ---------------------------------------------------------
export type ObservationStatus = 1 | 2 | 3; // Draft | Submitted | Approved

export interface Observation {
  id: string;
  indicatorId: string;
  orgUnitId: string;
  periodYear: number;
  periodMonth: number | null;
  value: number | null;
  valueText: string | null;
  source: string | null;
  status: ObservationStatus;
}

export const getObservations = (p: PageParams & { periodYear?: number }) =>
  api.get<PagedResult<Observation>>('/api/sector/observations', {
    params: { page: p.page, pageSize: p.pageSize, periodYear: p.periodYear },
  }).then((r) => r.data);

export const createObservation = (b: {
  indicatorId: string; orgUnitId: string; periodYear: number; periodMonth?: number | null;
  value?: number | null; valueText?: string | null; source?: string | null;
}) => api.post<{ id: string }>('/api/sector/observations', b).then((r) => r.data);

export type ClusterStatus = 1 | 2 | 3; // Planned | Operating | Suspended

export interface Cluster {
  id: string;
  code: string;
  name: string;
  orgUnitId: string;
  areaHa: number | null;
  latitude: number | null;
  longitude: number | null;
  status: ClusterStatus;
}

export const getClusters = (p: PageParams) =>
  api.get<PagedResult<Cluster>>('/api/sector/clusters', { params: pageParams(p) }).then((r) => r.data);

export const createCluster = (b: {
  code: string; name: string; orgUnitId: string; areaHa?: number | null;
  latitude?: number | null; longitude?: number | null; status: ClusterStatus;
}) => api.post<{ id: string }>('/api/sector/clusters', b).then((r) => r.data);
