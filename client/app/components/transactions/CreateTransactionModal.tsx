"use client";

import { getDataPaged1000 } from "@/app/actions/clientsActions";
import { Client, ServiceType, getServiceTypeName } from "@/Types";
import {
  Button,
  Checkbox,
  HelperText,
  Label,
  Modal,
  ModalBody,
  ModalHeader,
  Select,
  Spinner,
  TextInput,
} from "flowbite-react";
import { useCallback, useEffect, useRef, useState } from "react";
import { HiChevronDown, HiSearch, HiUser, HiX } from "react-icons/hi";
import { useDebouncedCallback } from "use-debounce";

type Props = {
  show: boolean;
  onClose: () => void;
  onCreated: (data: any) => void;
};

export default function CreateTransactionModal({
  show,
  onClose,
  onCreated,
}: Props) {
  const [clients, setClients] = useState<Client[]>([]);
  const [loadingClients, setLoadingClients] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);
  const [isListOpen, setIsListOpen] = useState(false);
  const listRef = useRef<HTMLDivElement>(null);

  const [formData, setFormData] = useState({
    clientId: "",
    date: new Date().toISOString().split("T")[0],
    serviceType: ServiceType.BankStatement,
    operationsCount: 0,
    actualTimeMinutes: 0,
    billableTimeMinutes: 0,
    performerName: "",
    isExtraService: false,
    extraServiceAmount: 0,
    status: "Pending",
  });

  // Основная функция загрузки (без дебаунса)
  const loadClients = useCallback(async (query: string = "") => {
    setLoadingClients(true);
    try {
      const data = await getDataPaged1000(query);
      const list = data.items || data;
      setClients(Array.isArray(list) ? list : []);
    } catch (error) {
      console.error("Load clients error:", error);
    } finally {
      setLoadingClients(false);
    }
  }, []);

  // СТАБИЛЬНЫЙ ДЕБАУНС: используем useDebouncedCallback правильно
  // Мы гарантируем, что он не пересоздается, не передавая лишних зависимостей
  const debouncedSearch = useDebouncedCallback((query: string) => {
    loadClients(query);
  }, 500);

  // Обработка ввода
  const onSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    setSearchTerm(val);

    // Сбрасываем выбор, если начали печатать
    if (selectedClient) {
      setSelectedClient(null);
      setFormData((prev) => ({ ...prev, clientId: "" }));
    }

    setIsListOpen(true);

    if (val.trim() === "") {
      debouncedSearch.cancel(); // Отменяем текущий дебаунс, если пусто
      loadClients(""); // Грузим начальный список мгновенно
    } else {
      debouncedSearch(val);
    }
  };

  const handleSelectClient = useCallback((client: Client) => {
    setSelectedClient(client);
    setFormData((prev) => ({ ...prev, clientId: client.id }));
    setSearchTerm(`${client.firstName} ${client.lastName}`);
    setIsListOpen(false);
  }, []);

  const clearSelection = () => {
    setSelectedClient(null);
    setSearchTerm("");
    setFormData((prev) => ({ ...prev, clientId: "" }));
    loadClients("");
    setIsListOpen(true);
  };

  const handleInputFocus = () => {
    setIsListOpen(true);
    if (clients.length === 0) {
      loadClients(searchTerm);
    }
  };

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (listRef.current && !listRef.current.contains(event.target as Node)) {
        setIsListOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  useEffect(() => {
    if (show) {
      setSearchTerm("");
      setSelectedClient(null);
      setClients([]);
      setFormData({
        clientId: "",
        date: new Date().toISOString().split("T")[0],
        serviceType: ServiceType.BankStatement,
        operationsCount: 0,
        actualTimeMinutes: 0,
        billableTimeMinutes: 0,
        performerName: "",
        isExtraService: false,
        extraServiceAmount: 0,
        status: "Pending",
      });
    }
  }, [show]);

  return (
    <Modal show={show} onClose={onClose} size="xl">
      <ModalHeader>Новая транзакция</ModalHeader>
      <ModalBody className="overflow-visible">
        {" "}
        {/* Важно для выпадающего списка */}
        <form
          onSubmit={(e) => {
            e.preventDefault();
            onCreated(formData);
          }}
          className="flex flex-col gap-4"
        >
          <div className="grid grid-cols-2 gap-4">
            {/* ГИБРИДНЫЙ ПОИСК */}
            <div className="relative" ref={listRef}>
              <Label htmlFor="clientSearch">Клиент</Label>
              <div className="relative mt-1">
                <TextInput
                  id="clientSearch"
                  icon={HiSearch}
                  placeholder="Поиск по имени или БИН..."
                  value={searchTerm}
                  onFocus={handleInputFocus}
                  autoComplete="off"
                  onChange={onSearchChange}
                  color={selectedClient ? "success" : "gray"}
                />
                <div className="absolute right-2 top-2.5 flex items-center gap-1">
                  {searchTerm && (
                    <button
                      type="button"
                      onClick={clearSelection}
                      className="p-1 hover:bg-gray-100 rounded-full text-gray-400"
                    >
                      <HiX className="h-4 w-4" />
                    </button>
                  )}
                  <HiChevronDown
                    className={`h-5 w-5 text-gray-400 transition-transform cursor-pointer ${isListOpen ? "rotate-180" : ""}`}
                    onClick={() => setIsListOpen(!isListOpen)}
                  />
                </div>
              </div>

              {selectedClient && (
                <HelperText color="success">
                  Клиент выбран:{" "}
                  <span className="font-medium text-green-700 dark:text-green-400">
                    {selectedClient.binIin}
                  </span>
                </HelperText>
              )}

              {/* СПИСОК РЕЗУЛЬТАТОВ */}
              {isListOpen && (
                <div className="absolute z-999 w-full mt-1 bg-white border border-gray-200 rounded-lg shadow-xl dark:bg-gray-800 dark:border-gray-600 max-h-60 overflow-y-auto">
                  {loadingClients ? (
                    <div className="p-6 text-center">
                      <Spinner size="md" />
                      <p className="mt-2 text-xs text-gray-500 italic">
                        Ищем в базе...
                      </p>
                    </div>
                  ) : clients.length > 0 ? (
                    clients.map((c) => (
                      <div
                        key={c.id}
                        className="p-3 cursor-pointer hover:bg-blue-50 dark:hover:bg-gray-700 border-b border-gray-100 dark:border-gray-700 last:border-0 flex flex-col"
                        onClick={() => handleSelectClient(c)}
                      >
                        <span className="font-bold text-sm text-gray-900 dark:text-white">
                          {c.firstName} {c.lastName}
                        </span>
                        <span className="text-xs text-gray-500 dark:text-gray-400">
                          БИН: {c.binIin}
                        </span>
                      </div>
                    ))
                  ) : (
                    <div className="p-4 text-sm text-gray-500 text-center">
                      Ничего не найдено по запросу
                    </div>
                  )}
                </div>
              )}
            </div>

            {/* ДАТА */}
            <div>
              <Label htmlFor="date">Дата</Label>
              <TextInput
                id="date"
                type="date"
                className="mt-1"
                value={formData.date}
                required
                onChange={(e) =>
                  setFormData({ ...formData, date: e.target.value })
                }
              />
            </div>

            {/* ТИП УСЛУГИ */}
            <div>
              <Label htmlFor="serviceType">Тип услуги</Label>
              <Select
                id="serviceType"
                className="mt-1"
                value={formData.serviceType}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    serviceType: Number(e.target.value),
                  })
                }
              >
                {Object.values(ServiceType)
                  .filter((v) => typeof v === "number" && v !== 0)
                  .map((type) => (
                    <option key={type} value={type}>
                      {getServiceTypeName(type as ServiceType)}
                    </option>
                  ))}
              </Select>
            </div>

            {/* ИСПОЛНИТЕЛЬ */}
            <div>
              <Label htmlFor="performer">Исполнитель</Label>
              <TextInput
                id="performer"
                icon={HiUser}
                className="mt-1"
                required
                placeholder="ФИО сотрудника"
                value={formData.performerName}
                onChange={(e) =>
                  setFormData({ ...formData, performerName: e.target.value })
                }
              />
            </div>
          </div>

          <hr className="my-2 dark:border-gray-700" />

          {/* ПОКАЗАТЕЛИ */}
          <div className="grid grid-cols-3 gap-4">
            <div>
              <Label>Операции</Label>
              <TextInput
                type="number"
                className="mt-1"
                required
                min={0}
                value={formData.operationsCount}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    operationsCount: Number(e.target.value),
                  })
                }
              />
            </div>
            <div>
              <Label>Факт (мин)</Label>
              <TextInput
                type="number"
                className="mt-1"
                min={0}
                value={formData.actualTimeMinutes}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    actualTimeMinutes: Number(e.target.value),
                  })
                }
              />
            </div>
            <div>
              <Label>Биллинг (мин)</Label>
              <TextInput
                type="number"
                className="mt-1"
                min={0}
                value={formData.billableTimeMinutes}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    billableTimeMinutes: Number(e.target.value),
                  })
                }
              />
            </div>
          </div>

          {/* ДОП УСЛУГА */}
          <div className="mt-2 p-3 bg-gray-50 dark:bg-gray-800 rounded-lg border dark:border-gray-700">
            <div className="flex items-center gap-2">
              <Checkbox
                id="extra"
                checked={formData.isExtraService}
                onChange={(e) =>
                  setFormData({ ...formData, isExtraService: e.target.checked })
                }
              />
              <Label htmlFor="extra" className="cursor-pointer">
                Дополнительная услуга
              </Label>
            </div>
            {formData.isExtraService && (
              <div className="mt-3">
                <Label>Сумма доплаты (₸)</Label>
                <TextInput
                  type="number"
                  className="mt-1"
                  placeholder="0.00"
                  value={formData.extraServiceAmount}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      extraServiceAmount: Number(e.target.value),
                    })
                  }
                />
              </div>
            )}
          </div>

          <div className="flex justify-end gap-2 mt-4 border-t pt-4 dark:border-gray-700">
            <Button color="gray" onClick={onClose}>
              Отмена
            </Button>
            <Button type="submit" color="blue" disabled={!formData.clientId}>
              Создать транзакцию
            </Button>
          </div>
        </form>
      </ModalBody>
    </Modal>
  );
}
