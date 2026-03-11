export type PagedResult<T> = {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
};

export type DashboardData = {
  id: string;
  firstName: string;
  lastName: string;
  taxRegime: string;
  taxRiskLevel: string;
  operationsLimit: number;
  operationsActual: number;
  operationsRemaining: number;
  consultingMinutesLimit: number;
  consultingMinutesActual: number;
  statReportsCount: number;
  monthlyTaxReports: number;
  quarterlyTaxReports: number;
  semiAnnualTaxReports: number;
  annualTaxReports: number;
  personnelCount: number;
  tariffAmount: number;
  extraServicesAmount: number;
  totalToPay: number;
  ndsThreshold: number;
  currentYearTurnover: number;
  ndsProgressPercentage: number;
  ecpExpiryDate: string;
  daysUntilEcpExpires: number;
  activeTasksCount: number;
  overdueTasksCount: number;
  aiInsight?: string;
};

export type Clients = Client[];

export interface Client {
  id: string;
  firstName: string;
  lastName: string;
  binIin: string;
  address: string;
  taxRegime: string;
  ndsStatus: string;
  taxRiskLevel: string;
  ecpExpiryDate: string;
  daysUntilEcpExpires: number;
  responsiblePersonContact: string;
  bankManagerContact: string;
  managerNotes: string;
  ecpPassword: string;
  esfPassword: string;
  bankingPasswords: string;
  strategicNotes: string;
  personalInfo: string;
  totalDebt: number;
  transactions: Transaction[];
  currentTariff: CurrentTariff;
  currentYearTurnover?: number;
}

export interface Transaction {
  id: string;
  date: string;
  clientId: string;
  clientName: string;
  serviceId: string;
  serviceName: string;
  performerName: string;
  operationsCount: number;
  actualTimeMinutes: number;
  billableTimeMinutes: number;
  communicationTimeMinutes: number;
  status: string;
  extraServiceAmount: number;
}

export interface CurrentTariff {
  monthlyFee: number;
  contractAmount: number;
  startDate: string;
  contractSigningDate: any;
  allowedOperations: number;
  carriedOverOperations: number;
  totalOperationsLimit: number;
  allowedCommunicationMinutes: number;
  carriedOverMinutes: number;
  totalMinutesLimit: number;
  statisticalReportsLimit: number;
  monthlyTaxReportsLimit: number;
  quarterlyTaxReportsLimit: number;
  semiAnnualTaxReportsLimit: number;
  annualTaxReportsLimit: number;
  employeeCountLimit: number;
}
