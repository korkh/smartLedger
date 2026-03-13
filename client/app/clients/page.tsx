import { auth } from "@/auth";
import { formatDate } from "@/lib/formatDate";
import { Client } from "@/Types";
import {
  Badge,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeadCell,
  TableRow,
  Tooltip,
} from "flowbite-react";
import Link from "next/link";
import { HiIdentification, HiTrendingUp } from "react-icons/hi";
import { getData } from "../actions/clientsActions";
import AppPagination from "../components/AppPagination";
import DeleteClientButton from "../components/client/DeleteClientButton";
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
  const session = await auth();
  const role = (session?.user as any)?.role;

  if (!role) {
    return <div className="p-6 text-center">Войдите в систему</div>;
  }

  const params = await searchParams;

  const urlParams = new URLSearchParams({
    pageNumber: params.pageNumber || "1",
    pageSize: "8",
    search: params.search || "",
    sortField: params.sortField || "firstname",
    order: "asc",
  });

  if (params.taxRiskLevel) {
    urlParams.append("taxRiskLevel", params.taxRiskLevel);
  }

  const result = await getData(`?${urlParams.toString()}`);
  console.log("Result from clients page", result);

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
    <div className="px-6 flex flex-col overflow-x-hidden">
      <div className=" z-20 bg-gray-50/95 dark:bg-gray-900/95 backdrop-blur-sm py-4  px-6 border-b border-gray-200 dark:border-gray-800 transition-colors">
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
      </div>

      <div className="overflow-x-auto shadow-xl border border-gray-200 dark:border-gray-700 sm:rounded-lg">
        <Table hoverable striped>
          <TableHead>
            <TableRow>
              <TableHeadCell>Клиент / БИН</TableHeadCell>
              <TableHeadCell>Налоговый режим</TableHeadCell>
              <TableHeadCell>НДС Статус</TableHeadCell>
              <TableHeadCell>Оборот (Год)</TableHeadCell>
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
                  <Link
                    href={`/clients/${client.id}`}
                    className="flex flex-col hover:text-blue-600 transition-colors group"
                  >
                    <span className="font-bold text-gray-900 dark:text-white group-hover:underline">
                      {client.lastName} {client.firstName}
                    </span>
                    <span className="text-xs text-gray-500">
                      {client.binIin}
                    </span>
                  </Link>
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
                <TableCell className="text-xs font-medium">
                  {(
                    client.transactions?.reduce(
                      (sum, t) => sum + (t.ndsBaseAmount || 0),
                      0,
                    ) || 0
                  ).toLocaleString()}{" "}
                  ₸
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
                      {formatDate(client.ecpExpiryDate)}
                    </span>
                  </div>
                </TableCell>
                <TableCell className="text-xs">
                  {client.responsiblePersonContact}
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-4">
                    {/* 1. Аналитика — ТОЛЬКО Admin (соответствует DashboardController [Authorize(Policy = "Level3Only")]) */}
                    {role === "Admin" && (
                      <Tooltip content="Аналитика">
                        <Link
                          href={`/dashboard/${client.id}`}
                          className="text-blue-600 hover:underline"
                        >
                          <HiTrendingUp className="h-5 w-5" />
                        </Link>
                      </Tooltip>
                    )}

                    {/* 2. Досье — ВСЕ (соответствует [Authorize(Policy = "Level1Only")]) */}
                    <Tooltip content="Досье">
                      <Link
                        href={`/clients/${client.id}`}
                        className="text-emerald-600 hover:underline"
                      >
                        <HiIdentification className="h-5 w-5" />
                      </Link>
                    </Tooltip>

                    {/* 3. Править и Удалить — Senior и Admin (соответствует [Authorize(Policy = "Level2Only")]) */}
                    {(role === "Senior_Accountant" || role === "Admin") && (
                      <>
                        <Tooltip content="Править">
                          <Link
                            href={`/clients/edit/${client.id}`}
                            className="text-gray-400 hover:text-gray-600"
                          >
                            <svg
                              className="w-5 h-5"
                              fill="none"
                              stroke="currentColor"
                              viewBox="0 0 24 24"
                            >
                              <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth="2"
                                d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"
                              />
                            </svg>
                          </Link>
                        </Tooltip>

                        <Tooltip content="Удалить">
                          <DeleteClientButton
                            clientId={client.id}
                            clientName={`${client.firstName} ${client.lastName}`}
                            isAdmin={role === "Admin"}
                          />
                        </Tooltip>
                      </>
                    )}
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
