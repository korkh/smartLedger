export type PagedResult<T> = {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
};

/**
 * Основной интерфейс для данных дашборда клиента.
 * Соответствует ClientDashboardDto.cs
 */
export type DashboardData = {
  id: string;
  firstName: string;
  lastName: string;
  taxRegime: string;
  taxRiskLevel: string;

  // --- БЛОК 1: ОПЕРАЦИИ ---
  operationsLimit: number;
  operationsActual: number;
  operationsRemaining: number;
  consultingMinutesLimit: number;
  consultingMinutesActual: number;

  // --- БЛОК 2: ОТЧЕТНОСТЬ ---
  statReportsCount: number;
  monthlyTaxReports: number;
  quarterlyTaxReports: number;
  semiAnnualTaxReports: number;
  annualTaxReports: number;
  personnelCount: number;

  // --- БЛОК 3: ФИНАНСЫ ---
  tariffAmount: number;
  monthlyExtraServicesAmount: number;
  totalOutstandingDebt: number;
  totalToPay: number; // Вычисляемое на бэкенде
  grandTotalDue: number;

  // --- БЛОК 4: ЛИМИТЫ НДС ---
  ndsThreshold: number;
  monthlyTurnover: number;
  currentYearTurnover: number;
  ndsProgressPercentage: number;

  // --- БЛОК 5: СРОКИ И ЗАДАЧИ ---
  ecpExpiryDate: string | null; // ISO Date String
  daysUntilEcpExpires: number;
  activeTasksCount: number;
  overdueTasksCount: number;

  // AI-аналитика
  aiInsight?: string;
};

export interface Client {
  id: string;
  firstName: string;
  lastName: string;
  binIin: string;
  address: string;
  taxRegime: string;
  ndsStatus: string;
  taxRiskLevel: string;
  ecpExpiryDate: string | null;
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

export type Clients = Client[];
