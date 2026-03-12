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
  totalToPay: number;
  grandTotalDue: number;

  // --- БЛОК 4: ЛИМИТЫ НДС ---
  ndsThreshold: number;
  monthlyTurnover: number;
  currentYearTurnover: number;
  ndsProgressPercentage: number;

  // --- БЛОК 5: СРОКИ И ЗАДАЧИ ---
  ecpExpiryDate: string | null;
  daysUntilEcpExpires: number;
  activeTasksCount: number;
  overdueTasksCount: number;

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

  // Типизация через Enum
  serviceType: ServiceType;
  serviceTypeName: string; // Локализованное название (например, "Стат. отчет")
  serviceId?: string;
  serviceName?: string;

  performerName: string;
  operationsCount: number;
  actualTimeMinutes: number;
  billableTimeMinutes: number;
  communicationTimeMinutes: number;
  status: string;

  isExtraService: boolean;
  extraServiceAmount: number;
  ndsBaseAmount: number;
}

export interface CurrentTariff {
  id: string;
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

export enum ServiceType {
  None = 0,
  BankStatement = 1,
  TaxCalculation = 2,
  CargoCustoms = 3,
  InventoryWriteOff = 4,
  Payroll = 5,
  StatReport = 6,
  TaxReport = 7,
  QuarterlyTaxReport = 8,
  SemiAnnualTaxReport = 9,
  AnnualTaxReport = 10,
}

export const getServiceTypeName = (type: ServiceType): string => {
  const names: Record<number, string> = {
    [ServiceType.BankStatement]: "Выписка банка",
    [ServiceType.CargoCustoms]: "ЭАВР / СНТ",
    [ServiceType.Payroll]: "Кадры / ЗП",
    [ServiceType.StatReport]: "Стат. отчет",
    [ServiceType.TaxReport]: "Месячный отчет",
    [ServiceType.QuarterlyTaxReport]: "Квартальный отчет",
    [ServiceType.SemiAnnualTaxReport]: "Полугодовой отчет",
    [ServiceType.AnnualTaxReport]: "Годовой отчет",
  };
  return names[type] || "Прочая услуга";
};

export type Clients = Client[];
