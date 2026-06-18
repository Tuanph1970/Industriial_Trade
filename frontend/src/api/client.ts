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

export type ViolationGroup = 1 | 2; // ProhibitedAndCounterfeit | FoodSafety
export type ViolationStatus = 1 | 2 | 3; // Reported | UnderHandling | Resolved

export interface Violation {
  id: string;
  caseNo: string;
  group: ViolationGroup;
  orgUnitId: string;
  businessName: string;
  inspectedOn: string;
  violationContent: string;
  sanctionContent: string | null;
  fineAmount: number | null;
  status: ViolationStatus;
}

export const getViolations = (p: PageParams & { violationGroup?: ViolationGroup }) =>
  api.get<PagedResult<Violation>>('/api/sector/violations', {
    params: { ...pageParams(p), violationGroup: p.violationGroup },
  }).then((r) => r.data);

export const createViolation = (b: {
  caseNo: string; group: ViolationGroup; orgUnitId: string; businessName: string;
  inspectedOn: string; violationContent: string;
}) => api.post<{ id: string }>('/api/sector/violations', b).then((r) => r.data);

export type StationStatus = 1 | 2 | 3; // Operating | Suspended | Closed

export interface PetrolStation {
  id: string; code: string; name: string; orgUnitId: string;
  licenseNo: string | null; address: string | null;
  latitude: number | null; longitude: number | null; status: StationStatus;
}

export const getPetrolStations = (p: PageParams) =>
  api.get<PagedResult<PetrolStation>>('/api/sector/petrol-stations', { params: pageParams(p) }).then((r) => r.data);

export const createPetrolStation = (b: {
  code: string; name: string; orgUnitId: string; licenseNo?: string | null; address?: string | null;
  latitude?: number | null; longitude?: number | null; status: StationStatus;
}) => api.post<{ id: string }>('/api/sector/petrol-stations', b).then((r) => r.data);

export type CommerceLocationType = 1 | 2 | 3 | 4; // Market | Supermarket | Mall | ConvenienceStore

export interface CommerceLocation {
  id: string; code: string; name: string; type: CommerceLocationType; orgUnitId: string;
  address: string | null; latitude: number | null; longitude: number | null;
}

export const getCommerceLocations = (p: PageParams & { type?: CommerceLocationType }) =>
  api.get<PagedResult<CommerceLocation>>('/api/sector/commerce-locations', {
    params: { ...pageParams(p), type: p.type },
  }).then((r) => r.data);

export const createCommerceLocation = (b: {
  code: string; name: string; type: CommerceLocationType; orgUnitId: string;
  address?: string | null; latitude?: number | null; longitude?: number | null;
}) => api.post<{ id: string }>('/api/sector/commerce-locations', b).then((r) => r.data);

export interface EcommerceParticipant {
  id: string; taxCode: string; businessName: string; orgUnitId: string;
  platforms: string[]; mainGoods: string | null;
}

export const getEcommerce = (p: PageParams) =>
  api.get<PagedResult<EcommerceParticipant>>('/api/sector/ecommerce-participants', { params: pageParams(p) }).then((r) => r.data);

export const createEcommerce = (b: {
  taxCode: string; businessName: string; orgUnitId: string; platforms: string[]; mainGoods?: string | null;
}) => api.post<{ id: string }>('/api/sector/ecommerce-participants', b).then((r) => r.data);

// ---- Reporting & Workflow ------------------------------------------------
export type CampaignStatus = 1 | 2; // Open | Closed

export interface Campaign {
  id: string; code: string; name: string;
  periodYear: number; periodMonth: number | null; deadline: string | null; status: CampaignStatus;
}

export const getCampaigns = (p: PageParams) =>
  api.get<PagedResult<Campaign>>('/api/reporting/campaigns', { params: pageParams(p) }).then((r) => r.data);

export const createCampaign = (b: {
  code: string; name: string; periodYear: number; periodMonth?: number | null; deadline?: string | null;
}) => api.post<{ id: string }>('/api/reporting/campaigns', b).then((r) => r.data);

// Matches the C# ReportState (Draft=1 … Rejected=6).
export type ReportState = 1 | 2 | 3 | 4 | 5 | 6;
// Matches the C# ReportAction enum order (Submit=0 … Reopen=6); sent as a number.
export const ReportAction = {
  Submit: 0, AcceptForReview: 1, Return: 2, ForwardForApproval: 3, Approve: 4, Reject: 5, Reopen: 6,
} as const;
export type ReportActionValue = (typeof ReportAction)[keyof typeof ReportAction];

export interface Submission {
  id: string; campaignId: string; orgUnitId: string; title: string; state: ReportState; createdAtUtc: string;
}
export interface Transition {
  fromState: ReportState; toState: ReportState; action: string; actorName: string | null; atUtc: string; note: string | null;
}
export interface SubmissionDetail extends Submission { history: Transition[]; }

export const getSubmissions = (p: PageParams & { state?: ReportState; campaignId?: string }) =>
  api.get<PagedResult<Submission>>('/api/reporting/submissions', {
    params: { page: p.page, pageSize: p.pageSize, state: p.state, campaignId: p.campaignId },
  }).then((r) => r.data);

export const getSubmissionDetail = (id: string) =>
  api.get<SubmissionDetail>(`/api/reporting/submissions/${id}`).then((r) => r.data);

export const createSubmission = (b: { campaignId: string; orgUnitId: string; title: string }) =>
  api.post<{ id: string }>('/api/reporting/submissions', b).then((r) => r.data);

export const submissionAction = (id: string, action: ReportActionValue, note?: string) =>
  api.post(`/api/reporting/submissions/${id}/actions`, { action, note });

// ---- Notifications -------------------------------------------------------
export interface Notification {
  id: string; title: string; message: string; category: string; refId: string | null;
  isRead: boolean; createdAtUtc: string;
}

export const getNotifications = (p: PageParams & { unreadOnly?: boolean }) =>
  api.get<PagedResult<Notification>>('/api/notifications', {
    params: { page: p.page, pageSize: p.pageSize, unreadOnly: p.unreadOnly },
  }).then((r) => r.data);

export const getUnreadCount = () =>
  api.get<number>('/api/notifications/unread-count').then((r) => r.data);

export const markNotificationRead = (id: string) => api.post(`/api/notifications/${id}/read`);
export const markAllNotificationsRead = () => api.post('/api/notifications/read-all');
