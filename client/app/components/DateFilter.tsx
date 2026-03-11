"use client";
import { DashboardFilters } from "@/app/hooks/useDashboard";
import { ru } from "date-fns/locale";
import { Button, Label, Select } from "flowbite-react";
import { useState } from "react";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { HiRefresh } from "react-icons/hi";

export default function DateFilter({
  onPeriodChange,
  filters: externalFilters,
}: {
  onPeriodChange: (params: DashboardFilters) => void;
  filters: DashboardFilters;
}) {
  // Локальное состояние для формы фильтров
  const [localFilters, setLocalFilters] =
    useState<DashboardFilters>(externalFilters);

  // Обработчик нажатия на кнопку "Обновить"
  const handleApply = () => {
    onPeriodChange(localFilters);
  };

  const updateLocal = (patch: Partial<DashboardFilters>) => {
    setLocalFilters((prev) => ({ ...prev, ...patch }));
  };

  return (
    <div className="flex flex-wrap items-end gap-4 bg-white dark:bg-gray-800 p-4 rounded-lg shadow-sm border border-gray-100">
      {/* Тип фильтра */}
      <div>
        <div className="mb-2 block">
          <Label htmlFor="period">Тип фильтра</Label>
        </div>
        <Select
          id="period"
          value={localFilters.periodType}
          onChange={(e) => {
            const val = e.target.value as DashboardFilters["periodType"];
            updateLocal({ periodType: val });
          }}
        >
          <option value="month">По месяцам</option>
          <option value="today">Сегодня</option>
          <option value="all">За все время</option>
          <option value="custom">Указать диапазон</option>
        </Select>
      </div>

      {/* Выбор месяца */}
      {localFilters.periodType === "month" && (
        <div className="flex gap-2">
          <DatePicker
            selected={
              new Date(localFilters.year || 2025, (localFilters.month || 1) - 1)
            }
            onChange={(date: Date | null) => {
              if (date) {
                updateLocal({
                  month: date.getMonth() + 1,
                  year: date.getFullYear(),
                });
              }
            }}
            showMonthYearPicker
            dateFormat="MMMM yyyy"
            locale={ru}
            className="block w-full border border-gray-300 bg-gray-50 text-gray-900 p-2.5 text-sm rounded-lg focus:ring-cyan-500 focus:border-cyan-500"
          />
        </div>
      )}

      {/* Выбор диапазона */}
      {localFilters.periodType === "custom" && (
        <div className="flex items-center gap-2">
          <DatePicker
            selected={
              localFilters.fromDate ? new Date(localFilters.fromDate) : null
            }
            onChange={(date: Date | null) =>
              updateLocal({ fromDate: date?.toISOString().split("T")[0] })
            }
            placeholderText="От"
            className="block w-32 border border-gray-300 bg-gray-50 text-gray-900 p-2.5 text-sm rounded-lg"
          />
          <span className="text-gray-500">—</span>
          <DatePicker
            selected={
              localFilters.toDate ? new Date(localFilters.toDate) : null
            }
            onChange={(date: Date | null) =>
              updateLocal({ toDate: date?.toISOString().split("T")[0] })
            }
            placeholderText="До"
            className="block w-32 border border-gray-300 bg-gray-50 text-gray-900 p-2.5 text-sm rounded-lg"
          />
        </div>
      )}

      {/* Кнопка Обновить */}
      <Button
        color="info"
        onClick={handleApply}
        className="flex items-center gap-2"
      >
        <HiRefresh className="mr-2 h-5 w-5" />
        Обновить данные
      </Button>
    </div>
  );
}
