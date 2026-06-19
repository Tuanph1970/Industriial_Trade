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

export const updateOrgUnit = (id: string, b: { name: string; isActive: boolean }) =>
  api.put(`/api/identity/org-units/${id}`, b);
export const deleteOrgUnit = (id: string) => api.delete(`/api/identity/org-units/${id}`);

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

export const updateRole = (id: string, b: { name: string; permissions: string[] }) =>
  api.put(`/api/identity/roles/${id}`, b);

export const deleteRole = (id: string) => api.delete(`/api/identity/roles/${id}`);

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

export const updateUser = (id: string, b: {
  fullName?: string | null; email?: string | null; orgUnitId?: string | null;
  roleIds: string[]; isActive: boolean;
}) => api.put(`/api/identity/users/${id}`, b);

export const deleteUser = (id: string) => api.delete(`/api/identity/users/${id}`);

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

export const updateIndicator = (id: string, b: {
  name: string; unit: string; dataType: IndicatorDataType; sector: IndustrySector;
}) => api.put(`/api/catalog/indicators/${id}`, b);
export const deleteIndicator = (id: string) => api.delete(`/api/catalog/indicators/${id}`);

// Catalog master data: indicator sets, report templates, reporting periods.
export interface IndicatorSet {
  id: string; code: string; name: string; description: string | null; indicatorIds: string[]; isActive: boolean;
}
export const getIndicatorSets = (p: PageParams) =>
  api.get<PagedResult<IndicatorSet>>('/api/catalog/indicator-sets', { params: pageParams(p) }).then((r) => r.data);
export const createIndicatorSet = (b: { code: string; name: string; description?: string; indicatorIds: string[] }) =>
  api.post<{ id: string }>('/api/catalog/indicator-sets', b).then((r) => r.data);

export interface TemplateLine { indicatorId: string; label: string; rowOrder: number; }
export interface ReportTemplate {
  id: string; code: string; name: string; description: string | null; lines: TemplateLine[]; isActive: boolean;
}
export const getReportTemplates = (p: PageParams) =>
  api.get<PagedResult<ReportTemplate>>('/api/catalog/report-templates', { params: pageParams(p) }).then((r) => r.data);
export const createReportTemplate = (b: { code: string; name: string; description?: string; lines: TemplateLine[] }) =>
  api.post<{ id: string }>('/api/catalog/report-templates', b).then((r) => r.data);

export type Periodicity = 1 | 2 | 3; // Monthly | Quarterly | Yearly
export interface ReportingPeriod { id: string; code: string; name: string; periodicity: Periodicity; isActive: boolean; }
export const getReportingPeriods = (p: PageParams) =>
  api.get<PagedResult<ReportingPeriod>>('/api/catalog/reporting-periods', { params: pageParams(p) }).then((r) => r.data);
export const createReportingPeriod = (b: { code: string; name: string; periodicity: Periodicity }) =>
  api.post<{ id: string }>('/api/catalog/reporting-periods', b).then((r) => r.data);

export const updateIndicatorSet = (id: string, b: { name: string; description?: string; indicatorIds: string[] }) =>
  api.put(`/api/catalog/indicator-sets/${id}`, b);
export const updateReportTemplate = (id: string, b: { name: string; description?: string; lines: TemplateLine[] }) =>
  api.put(`/api/catalog/report-templates/${id}`, b);
export const updateReportingPeriod = (id: string, b: { name: string; periodicity: Periodicity }) =>
  api.put(`/api/catalog/reporting-periods/${id}`, b);

export const deleteIndicatorSet = (id: string) => api.delete(`/api/catalog/indicator-sets/${id}`);
export const deleteReportTemplate = (id: string) => api.delete(`/api/catalog/report-templates/${id}`);
export const deleteReportingPeriod = (id: string) => api.delete(`/api/catalog/reporting-periods/${id}`);

// ---- Administrative units (catalog reference data) -----------------------
export type AdministrativeLevel = 1 | 2 | 3; // Province | District | Commune
export interface AdministrativeUnit {
  id: string; code: string; name: string; level: AdministrativeLevel; parentId: string | null; isActive: boolean;
}
export const getAdministrativeUnits = (p: PageParams & { level?: AdministrativeLevel }) =>
  api.get<PagedResult<AdministrativeUnit>>('/api/catalog/administrative-units', {
    params: { ...pageParams(p), level: p.level },
  }).then((r) => r.data);
export const createAdministrativeUnit = (b: { code: string; name: string; level: AdministrativeLevel; parentId?: string | null }) =>
  api.post<{ id: string }>('/api/catalog/administrative-units', b).then((r) => r.data);
export const updateAdministrativeUnit = (id: string, b: { name: string; level: AdministrativeLevel; parentId?: string | null; isActive: boolean }) =>
  api.put(`/api/catalog/administrative-units/${id}`, b);
export const deleteAdministrativeUnit = (id: string) => api.delete(`/api/catalog/administrative-units/${id}`);

// ---- Classifications (catalog code lists) --------------------------------
export interface ClassificationItem { code: string; name: string; sortOrder: number; }
export interface Classification {
  id: string; code: string; name: string; description: string | null; items: ClassificationItem[]; isActive: boolean;
}
export const getClassifications = (p: PageParams) =>
  api.get<PagedResult<Classification>>('/api/catalog/classifications', { params: pageParams(p) }).then((r) => r.data);
export const createClassification = (b: { code: string; name: string; description?: string; items: ClassificationItem[] }) =>
  api.post<{ id: string }>('/api/catalog/classifications', b).then((r) => r.data);
export const updateClassification = (id: string, b: { name: string; description?: string; items: ClassificationItem[] }) =>
  api.put(`/api/catalog/classifications/${id}`, b);
export const deleteClassification = (id: string) => api.delete(`/api/catalog/classifications/${id}`);

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

export const updateCluster = (id: string, b: {
  name: string; areaHa?: number | null;
  latitude?: number | null; longitude?: number | null; status: ClusterStatus;
}) => api.put(`/api/sector/clusters/${id}`, b);

export const updatePetrolStation = (id: string, b: {
  name: string; licenseNo?: string | null; address?: string | null;
  latitude?: number | null; longitude?: number | null; status: StationStatus;
}) => api.put(`/api/sector/petrol-stations/${id}`, b);

export const updateCommerceLocation = (id: string, b: {
  name: string; type: CommerceLocationType; address?: string | null;
  latitude?: number | null; longitude?: number | null;
}) => api.put(`/api/sector/commerce-locations/${id}`, b);

export const updateEcommerce = (id: string, b: {
  businessName: string; platforms: string[]; mainGoods?: string | null;
}) => api.put(`/api/sector/ecommerce-participants/${id}`, b);

export const updateViolation = (id: string, b: {
  group: ViolationGroup; businessName: string; inspectedOn: string; violationContent: string;
  sanctionContent?: string | null; fineAmount?: number | null; status: ViolationStatus;
}) => api.put(`/api/sector/violations/${id}`, b);

export const deleteCluster = (id: string) => api.delete(`/api/sector/clusters/${id}`);
export const deletePetrolStation = (id: string) => api.delete(`/api/sector/petrol-stations/${id}`);
export const deleteCommerceLocation = (id: string) => api.delete(`/api/sector/commerce-locations/${id}`);
export const deleteEcommerce = (id: string) => api.delete(`/api/sector/ecommerce-participants/${id}`);
export const deleteViolation = (id: string) => api.delete(`/api/sector/violations/${id}`);

// ---- Batch import (Excel / XML / CSV) ------------------------------------
export interface ImportParseResult {
  columns: string[];
  rows: { rowNumber: number; cells: Record<string, string> }[];
}
export interface BulkImportResult {
  created: number;
  failed: number;
  errors: { index: number; message: string }[];
}

export const parseImportFile = (file: File) => {
  const fd = new FormData();
  fd.append('file', file);
  return api.post<ImportParseResult>('/api/sector/import/parse', fd).then((r) => r.data);
};

const bulk = (path: string) => (items: unknown[]) =>
  api.post<BulkImportResult>(path, { items }).then((r) => r.data);

export const bulkImportObservations = bulk('/api/sector/observations/import');
export const bulkImportClusters = bulk('/api/sector/clusters/import');
export const bulkImportPetrolStations = bulk('/api/sector/petrol-stations/import');
export const bulkImportCommerceLocations = bulk('/api/sector/commerce-locations/import');
export const bulkImportEcommerce = bulk('/api/sector/ecommerce-participants/import');
export const bulkImportViolations = bulk('/api/sector/violations/import');

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

// ---- Analytics & Dashboards ----------------------------------------------
export interface Dashboard {
  clusters: number; petrolStations: number; commerceLocations: number; ecommerceParticipants: number;
  violations: number; observations: number; indicators: number; campaigns: number;
  submissions: number; pendingApproval: number;
}
export interface ViolationSummaryRow { group: number; status: number; count: number; totalFine: number; }
export interface StateCount { state: number; count: number; }

export const getDashboard = () => api.get<Dashboard>('/api/analytics/dashboard').then((r) => r.data);
export const getViolationsSummary = () =>
  api.get<ViolationSummaryRow[]>('/api/analytics/violations-summary').then((r) => r.data);
export const getReportingSummary = () =>
  api.get<StateCount[]>('/api/analytics/reporting-summary').then((r) => r.data);

// ---- Audit log -----------------------------------------------------------
export interface AuditLog {
  id: string; actor: string | null; action: string; payload: string;
  success: boolean; error: string | null; atUtc: string;
}

export const getAuditLogs = (p: PageParams & { actor?: string; action?: string }) =>
  api.get<PagedResult<AuditLog>>('/api/audit/logs', {
    params: { page: p.page, pageSize: p.pageSize, actor: p.actor || undefined, action: p.action || undefined },
  }).then((r) => r.data);

// ---- Integration (LGSP/NDXP) ---------------------------------------------
export type ServiceDirection = 1 | 2; // Provide | Consume
export type ServiceStatus = 1 | 2 | 3; // Registered | Published | Revoked
export const ServiceLifecycleAction = { Publish: 0, Revoke: 1 } as const;
export type ServiceLifecycleActionValue = (typeof ServiceLifecycleAction)[keyof typeof ServiceLifecycleAction];

export interface DataSharingService {
  id: string; code: string; name: string; direction: ServiceDirection;
  endpointUrl: string | null; description: string | null; status: ServiceStatus;
}

export const getServices = (p: PageParams) =>
  api.get<PagedResult<DataSharingService>>('/api/integration/services', { params: pageParams(p) }).then((r) => r.data);

export const createService = (b: {
  code: string; name: string; direction: ServiceDirection; endpointUrl?: string | null; description?: string | null;
}) => api.post<{ id: string }>('/api/integration/services', b).then((r) => r.data);

export const changeServiceStatus = (id: string, action: ServiceLifecycleActionValue) =>
  api.post(`/api/integration/services/${id}/status`, { action });

export interface ComponentStatus { component: string; level: number; healthy: boolean; detail: string | null; }
export interface ConnectionStatus { healthy: boolean; components: ComponentStatus[]; }

export const getConnectionStatus = () =>
  api.get<ConnectionStatus>('/api/integration/connection-status').then((r) => r.data);
