"use client";

import { formatDate } from "@/lib/formatDate";
import { Client, getServiceTypeName, Transaction } from "@/Types";
import {
  Badge,
  Card,
  TabItem,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeadCell,
  TableRow,
  Tabs,
} from "flowbite-react";
import { HiCash, HiClipboardList, HiUserCircle } from "react-icons/hi";
import AiInsightCard from "../AiInsightCard";

export default function ClientTabs({
  client,
  role,
}: {
  client: Client;
  role: string;
}) {
  const isAdmin = role === "Admin";
  const isSenior = role === "Senior_Accountant" || isAdmin;
  return (
    <div className="p-6 space-y-6 bg-gray-50 dark:bg-gray-900 min-h-screen">
      <div className="flex justify-between items-start">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          {client.lastName} {client.firstName}
        </h1>
        <Badge
          size="xl"
          color={client.taxRiskLevel === "High" ? "failure" : "success"}
        >
          Риск: {client.taxRiskLevel}
        </Badge>
      </div>

      {/* 1. AI Insight доступен только Level 2+ (согласно бэкенду [Authorize(Policy = "Level2Only")]) */}
      {isSenior && <AiInsightCard mode="client" externalData={client} />}

      <Tabs aria-label="Client details" variant="underline">
        {/* Tab 1: Dossier */}
        <TabItem active title="Досье" icon={HiUserCircle}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-4">
            <Card>
              <h5 className="text-xl font-bold pb-2 border-b dark:text-white">
                Контактные данные
              </h5>
              <div className="space-y-3 mt-2">
                <InfoRow label="БИН/ИИН" value={client.binIin} />
                <InfoRow label="Адрес" value={client.address} />
                <InfoRow
                  label="Ответственное лицо"
                  value={client.responsiblePersonContact}
                />
                <InfoRow
                  label="Менеджер банка"
                  value={client.bankManagerContact}
                />
              </div>
            </Card>
            {/* 2. Доступы: Junior видит заблюренные звезды, Senior видит пароли */}
            <Card>
              <h5 className="text-xl font-bold pb-2 border-b dark:text-white">
                Доступы и Безопасность
              </h5>
              <div className="space-y-3 mt-2">
                <InfoRow
                  label="Пароль ЭЦП"
                  value={client.ecpPassword}
                  isSecret
                />
                <InfoRow
                  label="Пароль ЭСФ"
                  value={client.esfPassword}
                  isSecret
                />
                <InfoRow
                  label="Банкинг"
                  value={client.bankingPasswords}
                  isSecret
                />
                <div className="mt-4 pt-4 border-t border-gray-100 italic text-sm text-gray-500">
                  ЭЦП годен до: {formatDate(client.ecpExpiryDate)}
                </div>
              </div>
            </Card>

            {/* 3. Заметки: Скрываем заголовки, если данных нет (бэкенд присылает null) */}
            {(client.strategicNotes ||
              client.personalInfo ||
              client.managerNotes) && (
              <Card className="md:col-span-2">
                <h5 className="text-xl font-bold pb-2 border-b dark:text-white">
                  Заметки
                </h5>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-2">
                  {/* Стратегические заметки (Level 3 Only) */}
                  {client.strategicNotes && (
                    <div>
                      <h6 className="font-bold text-blue-600 mb-2">
                        Стратегия
                      </h6>
                      <p className="text-sm text-gray-600">
                        {client.strategicNotes}
                      </p>
                    </div>
                  )}
                  {/* Личная информация (Level 3 Only) */}
                  {client.personalInfo && (
                    <div>
                      <h6 className="font-bold text-purple-600 mb-2">Личное</h6>
                      <p className="text-sm text-gray-600">
                        {client.personalInfo}
                      </p>
                    </div>
                  )}
                  {/* Заметки менеджера (Level 2+) */}
                  {client.managerNotes && (
                    <div className="md:col-span-2">
                      <h6 className="font-bold text-orange-600 mb-2">
                        Заметки менеджера
                      </h6>
                      <p className="text-sm text-gray-600">
                        {client.managerNotes}
                      </p>
                    </div>
                  )}
                </div>
              </Card>
            )}
          </div>
        </TabItem>

        {/* Tab 2: Transactions */}
        <TabItem title="Транзакции и Услуги" icon={HiClipboardList}>
          <div className="mt-4 overflow-x-auto shadow-md rounded-lg">
            <Table hoverable striped>
              <TableHead>
                <TableRow>
                  <TableHeadCell>Дата</TableHeadCell>
                  <TableHeadCell>Услуга / Операция</TableHeadCell>
                  <TableHeadCell>Исполнитель</TableHeadCell>
                  <TableHeadCell>Кол-во / Время</TableHeadCell>
                  <TableHeadCell>Доп. доход</TableHeadCell>
                  <TableHeadCell>Статус</TableHeadCell>
                </TableRow>
              </TableHead>
              <TableBody className="divide-y">
                {client.transactions?.map((t: Transaction) => (
                  <TableRow key={t.id}>
                    <TableCell>{formatDate(t.date)}</TableCell>
                    <TableCell>
                      <div className="font-bold dark:text-white">
                        {getServiceTypeName(t.serviceType)}
                      </div>
                      <div className="text-[10px] text-gray-400 uppercase">
                        ID типа: {t.serviceType}
                      </div>
                    </TableCell>
                    <TableCell>{t.performerName}</TableCell>
                    <TableCell>
                      <div className="text-xs dark:text-gray-300">
                        Оп: {t.operationsCount}
                      </div>
                      <div className="text-[10px] text-gray-500">
                        {t.actualTimeMinutes} мин.
                      </div>
                    </TableCell>
                    <TableCell>
                      {t.extraServiceAmount > 0 ? (
                        <span className="text-green-600 font-bold">
                          +{t.extraServiceAmount.toLocaleString()} ₸
                        </span>
                      ) : (
                        "-"
                      )}
                    </TableCell>
                    <TableCell>
                      <Badge
                        color={t.status === "Completed" ? "success" : "warning"}
                      >
                        {t.status}
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </TabItem>

        {/* Tab 3: Tariff */}
        <TabItem title="Тариф" icon={HiCash}>
          <Card className="mt-4 max-w-lg">
            <h5 className="text-xl font-bold pb-2 border-b dark:text-white">
              Текущий план: {client.currentTariff ? "Активен" : "Не указан"}
            </h5>
            <div className="space-y-4 mt-2">
              <div className="flex justify-between dark:text-gray-300">
                <span>Сумма договора:</span>
                <span className="font-bold">
                  {client.currentTariff?.contractAmount?.toLocaleString() ??
                    "0"}{" "}
                  ₸
                </span>
              </div>
              <div className="flex justify-between dark:text-gray-300">
                <span>Лимит операций:</span>
                <span className="font-bold">
                  {client.currentTariff?.allowedOperations ?? 0}
                </span>
              </div>
              <div className="flex justify-between dark:text-gray-300">
                <span>Перенос операций:</span>
                <span className="text-blue-600 font-semibold">
                  +{client.currentTariff?.carriedOverOperations ?? 0} оп.
                </span>
              </div>
            </div>
          </Card>
        </TabItem>
      </Tabs>
    </div>
  );
}

function InfoRow({
  label,
  value,
  isSecret = false,
}: {
  label: string;
  value: string | null | undefined;
  isSecret?: boolean;
}) {
  const shouldBlur = isSecret && value !== "********";
  return (
    <div className="flex justify-between border-b border-gray-100 dark:border-gray-700 py-2">
      <span className="text-sm text-gray-500">{label}:</span>
      <span
        className={`text-sm font-medium ${
          shouldBlur
            ? "bg-gray-200 dark:bg-gray-700 px-2 rounded blur-sm hover:blur-none transition-all cursor-pointer"
            : "dark:text-white"
        }`}
      >
        {value || "—"}
      </span>
    </div>
  );
}
