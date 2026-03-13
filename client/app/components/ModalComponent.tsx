// components/ModalComponent.tsx
import { Button, Modal, ModalBody, ModalHeader } from "flowbite-react";
import { HiOutlineExclamationCircle } from "react-icons/hi";

type Props = {
  openModal: boolean;
  setOpenModal: (open: boolean) => void;
  handleDelete: () => void;
  title: string;
  isHardDelete?: boolean;
};

export default function ModalComponent({
  openModal,
  setOpenModal,
  handleDelete,
  title,
  isHardDelete = false,
}: Props) {
  return (
    <Modal show={openModal} size="md" onClose={() => setOpenModal(false)} popup>
      <ModalHeader />
      <ModalBody>
        <div className="text-center">
          <HiOutlineExclamationCircle className="mx-auto mb-4 h-14 w-14 text-gray-400 dark:text-gray-200" />
          <h3 className="mb-5 text-lg font-normal text-gray-500 dark:text-gray-400">
            {title}
            {isHardDelete && (
              <>
                <br />
                <span className="text-sm text-red-500 font-bold uppercase">
                  Это действие нельзя отменить.
                </span>
              </>
            )}
          </h3>
          <div className="flex justify-center gap-4">
            <Button
              color={isHardDelete ? "failure" : "warning"}
              onClick={handleDelete}
            >
              {"Да, я уверен"}
            </Button>
            <Button color="gray" onClick={() => setOpenModal(false)}>
              Отмена
            </Button>
          </div>
        </div>
      </ModalBody>
    </Modal>
  );
}
