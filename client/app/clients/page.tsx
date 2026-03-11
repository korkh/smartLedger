import { Client } from "@/Types";
import {
  Badge,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeadCell,
  TableRow,
} from "flowbite-react";
import Link from "next/link";
import { getData } from "../actions/clientsActions";
import AppPagination from "../components/AppPagination";
import SearchInput from "../components/SearchInput";

export default async function ClientsPage({
  searchParams,
}: {
  searchParams: Promise<{
    search?: string;
    pageNumber?: string;
    sortField?: string;
    taxRiskLevel?: string;
  }>;
}) {
  const params = await searchParams;

  const urlParams = new URLSearchParams({
    pageNumber: params.pageNumber || "1",
    pageSize: "15", // Увеличим размер страницы для таблицы
    search: params.search || "",
    sortField: params.sortField || "firstname",
    order: "asc",
  });

  if (params.taxRiskLevel) {
    urlParams.append("taxRiskLevel", params.taxRiskLevel);
  }

  const result = await getData(`?${urlParams.toString()}`);

  if (!result || !result.items) {
    return (
      <div className="p-6 dark:text-white text-center">
        <p className="text-xl">
          Данные не найдены или произошла ошибка запроса.
        </p>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-4">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold dark:text-white text-gray-800 italic">
            Реестр клиентов SmartLedger
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Всего записей: {result.totalCount}
          </p>
        </div>
        <div className="w-full md:max-w-md">
          <SearchInput />
        </div>
      </div>

      <div className="overflow-x-auto shadow-xl border border-gray-200 dark:border-gray-700 sm:rounded-lg">
        <Table hoverable striped>
          <TableHead>
            <TableRow>
              <TableHeadCell>Клиент / БИН</TableHeadCell>
              <TableHeadCell>Налоговый режим</TableHeadCell>
              <TableHeadCell>НДС Статус</TableHeadCell>
              <TableHeadCell>Риск</TableHeadCell>
              <TableHeadCell>Долг (Хвосты)</TableHeadCell>
              <TableHeadCell>ЭЦП (дн.)</TableHeadCell>
              <TableHeadCell>Ответственный</TableHeadCell>
              <TableHeadCell>
                <span className="sr-only">Действия</span>
              </TableHeadCell>
            </TableRow>
          </TableHead>
          <TableBody className="divide-y">
            {result.items.map((client: Client) => (
              <TableRow
                key={client.id}
                className="bg-white dark:border-gray-700 dark:bg-gray-800"
              >
                <TableCell className="whitespace-nowrap">
                  <div className="flex flex-col">
                    <span className="font-bold text-gray-900 dark:text-white">
                      {client.lastName} {client.firstName}
                    </span>
                    <span className="text-xs text-gray-500">
                      {client.binIin}
                    </span>
                  </div>
                </TableCell>
                <TableCell className="text-xs">{client.taxRegime}</TableCell>
                <TableCell>
                  <Badge
                    color={
                      client.ndsStatus === "Плательщик НДС" ? "purple" : "gray"
                    }
                  >
                    {client.ndsStatus === "Плательщик НДС" ? "НДС" : "Без НДС"}
                  </Badge>
                </TableCell>
                <TableCell>
                  <Badge
                    color={
                      client.taxRiskLevel === "High"
                        ? "failure"
                        : client.taxRiskLevel === "Medium"
                          ? "warning"
                          : "success"
                    }
                  >
                    {client.taxRiskLevel}
                  </Badge>
                </TableCell>
                <TableCell>
                  <span
                    className={
                      client.totalDebt > 0 ? "text-red-500 font-semibold" : ""
                    }
                  >
                    {client.totalDebt?.toLocaleString()} ₸
                  </span>
                </TableCell>
                <TableCell>
                  <div className="flex flex-col text-xs">
                    <span
                      className={
                        client.daysUntilEcpExpires < 15
                          ? "text-red-600 font-bold"
                          : ""
                      }
                    >
                      {client.daysUntilEcpExpires} дн.
                    </span>
                    <span className="text-gray-400 text-[10px]">
                      {new Date(client.ecpExpiryDate).toLocaleDateString()}
                    </span>
                  </div>
                </TableCell>
                <TableCell className="text-xs">
                  {client.responsiblePersonContact}
                </TableCell>
                <TableCell>
                  <div className="flex gap-3">
                    <Link
                      href={`/dashboard/${client.id}`}
                      className="font-medium text-blue-600 hover:underline dark:text-blue-500"
                    >
                      Анализ ИИ
                    </Link>
                    <Link
                      href={`/clients/edit/${client.id}`}
                      className="font-medium text-gray-400 hover:text-gray-600"
                    >
                      Карточка
                    </Link>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {result.totalPages > 1 && (
        <div className="flex justify-center py-4">
          <AppPagination
            currentPage={result.pageNumber}
            totalPages={result.totalPages}
          />
        </div>
      )}
    </div>
  );
}
