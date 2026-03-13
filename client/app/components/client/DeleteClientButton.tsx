"use client";

import { deleteClient, hardDeleteClient } from "@/app/actions/clientsActions";
import { Button, Modal, ModalBody, ModalHeader } from "flowbite-react";
import { useState } from "react";
import { HiOutlineExclamationCircle, HiTrash } from "react-icons/hi";
import { toast } from "react-toastify";

type Props = {
  clientId: string;
  clientName: string;
  isAdmin: boolean;
};

export default function DeleteClientButton({
  clientId,
  clientName,
  isAdmin,
}: Props) {
  const [openModal, setOpenModal] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleDelete = async (isHard: boolean) => {
    setLoading(true);
    try {
      if (isHard) {
        await hardDeleteClient(clientId);
        toast.success("Клиент полностью удален");
      } else {
        await deleteClient(clientId);
        toast.success("Клиент перемещен в архив");
      }
      setOpenModal(false);
    } catch (error) {
      toast.error("Ошибка при удалении");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <button
        onClick={() => setOpenModal(true)}
        className="font-medium text-red-600 hover:text-red-800 dark:text-red-500"
      >
        <HiTrash className="h-5 w-5" />
      </button>

      <Modal
        show={openModal}
        size="md"
        onClose={() => setOpenModal(false)}
        popup
      >
        <ModalHeader />
        <ModalBody>
          <div className="text-center">
            <HiOutlineExclamationCircle className="mx-auto mb-4 h-14 w-14 text-gray-400 dark:text-gray-200" />
            <h3 className="mb-5 text-lg font-normal text-gray-500 dark:text-gray-400">
              Вы уверены, что хотите удалить клиента <br />
              <strong>{clientName}</strong>?
            </h3>
            <div className="flex justify-center gap-4">
              {/* Обычное удаление (Level 2+) */}
              <Button
                color="failure"
                disabled={loading}
                onClick={() => handleDelete(false)}
              >
                Удалить (в архив)
              </Button>

              {/* Hard Delete - показываем только Админу (Level 3) */}
              {isAdmin && (
                <Button
                  color="dark"
                  disabled={loading}
                  onClick={() => handleDelete(true)}
                >
                  Удалить навсегда
                </Button>
              )}

              <Button color="gray" onClick={() => setOpenModal(false)}>
                Отмена
              </Button>
            </div>
          </div>
        </ModalBody>
      </Modal>
    </>
  );
}
