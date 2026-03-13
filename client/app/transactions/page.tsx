"use client";

import { getServiceTypeName } from "@/Types";
import {
  Badge,
  Button,
  Spinner,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeadCell,
  TableRow,
} from "flowbite-react";
import { useSession } from "next-auth/react";
import { useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import { HiOutlineTrash, HiPencil, HiTrash } from "react-icons/hi";
import { toast } from "react-toastify";
import AppPagination from "../components/AppPagination";
import ModalComponent from "../components/ModalComponent";
import CreateTransactionModal from "../components/transactions/CreateTransactionModal";
import { useTransactionStore } from "../hooks/useTransactionStore";

export default function TransactionsPage() {
  const { data: session } = useSession();
  const {
    transactions,
    loading,
    loadTransactions,
    createTransaction,
    softDeleteTransaction,
    hardDeleteTransaction,
    params,
    setParams,
    totalPages,
  } = useTransactionStore();

  const searchParams = useSearchParams();

  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [isHardDeleteMode, setIsHardDeleteMode] = useState(false);
  const [createModalOpen, setCreateModalOpen] = useState(false);

  const role = (session?.user as any)?.role;
  const isAdmin = role === "Admin";
  const isSenior = role === "Senior_Accountant" || isAdmin;

  // 1. Синхронизация URL -> Store
  // Когда пользователь нажимает на пагинацию, AppPagination меняет URL.
  // Этот эффект увидит изменение и обновит стор.
  useEffect(() => {
    const page = Number(searchParams.get("pageNumber")) || 1;
    if (page !== params.pageNumber) {
      setParams({ pageNumber: page });
    }
  }, [searchParams, setParams, params.pageNumber]);

  // 2. Загрузка данных
  // Теперь, когда стор обновился (либо через эффект выше, либо через поиск),
  // загружаем данные.
  useEffect(() => {
    loadTransactions();
  }, [params.pageNumber, params.searchTerm, loadTransactions]);

  const onPageChange = (page: number) => {
    useTransactionStore.getState().setParams({ pageNumber: page });
  };

  const openDeleteConfirm = (id: string, hard: boolean) => {
    setSelectedId(id);
    setIsHardDeleteMode(hard);
    setDeleteModalOpen(true);
  };

  const executeDelete = async () => {
    if (!selectedId) return;
    try {
      if (isHardDeleteMode) {
        await hardDeleteTransaction(selectedId);
        toast.error("Транзакция полностью удалена");
      } else {
        await softDeleteTransaction(selectedId);
        toast.info("Транзакция перемещена в архив");
      }
    } catch (error) {
      toast.error("Произошла ошибка при выполнении операции");
    } finally {
      setDeleteModalOpen(false);
      setSelectedId(null);
    }
  };

  const handleCreate = async (data: any) => {
    const success = await createTransaction(data);
    if (success) {
      toast.success("Транзакция успешно добавлена");
      setCreateModalOpen(false);
    } else {
      toast.error("Ошибка при создании транзакции");
    }
  };

  const renderStatusBadge = (status: string) => {
    const map: Record<string, { label: string; color: string }> = {
      Completed: { label: "Завершено", color: "success" },
      Pending: { label: "В работе", color: "warning" },
      Canceled: { label: "Отменено", color: "failure" },
    };
    const current = map[status] || { label: status, color: "gray" };
    return <Badge color={current.color}>{current.label}</Badge>;
  };

  return (
    <div className="p-4">
      <CreateTransactionModal
        show={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        onCreated={handleCreate}
      />

      <ModalComponent
        openModal={deleteModalOpen}
        setOpenModal={setDeleteModalOpen}
        handleDelete={executeDelete}
        isHardDelete={isHardDeleteMode}
        title={isHardDeleteMode ? "Удалить навсегда?" : "В архив?"}
      />

      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Реестр транзакций</h1>
        {isSenior && (
          <Button onClick={() => setCreateModalOpen(true)} color="blue">
            + Новая запись
          </Button>
        )}
      </div>

      <div className="overflow-x-auto shadow-md sm:rounded-lg">
        <Table hoverable>
          <TableHead>
            <TableRow>
              <TableHeadCell>Дата</TableHeadCell>
              <TableHeadCell>Клиент</TableHeadCell>
              <TableHeadCell>Тип услуги</TableHeadCell>
              <TableHeadCell>Операции</TableHeadCell>
              <TableHeadCell>Время</TableHeadCell>
              <TableHeadCell>Доп. услуга</TableHeadCell>
              <TableHeadCell>Статус</TableHeadCell>
              <TableHeadCell>Исполнитель</TableHeadCell>
              <TableHeadCell>
                <span className="sr-only">Действия</span>
              </TableHeadCell>
            </TableRow>
          </TableHead>

          <TableBody className="divide-y">
            {loading ? (
              <TableRow>
                <TableCell colSpan={9} className="text-center py-10">
                  <Spinner size="xl" />
                </TableCell>
              </TableRow>
            ) : transactions.length === 0 ? (
              <TableRow>
                <TableCell colSpan={9} className="text-center py-10">
                  Транзакции не найдены
                </TableCell>
              </TableRow>
            ) : (
              transactions.map((t) => (
                <TableRow
                  key={t.id}
                  className="bg-white dark:border-gray-700 dark:bg-gray-800"
                >
                  <TableCell>{new Date(t.date).toLocaleDateString()}</TableCell>
                  <TableCell className="font-semibold">
                    {t.clientName}
                  </TableCell>
                  <TableCell>
                    <div className="flex flex-col">
                      <span>{getServiceTypeName(t.serviceType)}</span>
                      <span className="text-xs text-gray-500">
                        {t.serviceName}
                      </span>
                    </div>
                  </TableCell>
                  <TableCell className="text-center font-bold text-blue-600">
                    {t.operationsCount}
                  </TableCell>
                  <TableCell>
                    <div className="text-xs">
                      ⚡ {t.actualTimeMinutes}м / 💰 {t.billableTimeMinutes}м
                    </div>
                  </TableCell>
                  <TableCell>
                    {t.isExtraService ? (
                      <Badge color="purple">+{t.extraServiceAmount} ₸</Badge>
                    ) : (
                      "—"
                    )}
                  </TableCell>
                  <TableCell>{renderStatusBadge(t.status)}</TableCell>
                  <TableCell>{t.performerName}</TableCell>
                  <TableCell>
                    <div className="flex gap-2">
                      {isSenior && (
                        <Button size="xs" color="gray">
                          <HiPencil className="h-4 w-4" />
                        </Button>
                      )}
                      {isSenior && (
                        <Button
                          size="xs"
                          color="warning"
                          onClick={() => openDeleteConfirm(t.id, false)}
                        >
                          <HiOutlineTrash className="h-4 w-4" />
                        </Button>
                      )}
                      {isAdmin && (
                        <Button
                          size="xs"
                          color="failure"
                          onClick={() => openDeleteConfirm(t.id, true)}
                        >
                          <HiTrash className="h-4 w-4" />
                        </Button>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {totalPages > 1 && (
        <div className="flex justify-center py-6 border-t dark:border-gray-700">
          <AppPagination
            currentPage={params.pageNumber}
            totalPages={totalPages}
          />
        </div>
      )}
    </div>
  );
}
