"use client";

import { TextInput } from "flowbite-react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { HiSearch } from "react-icons/hi";
import { useDebouncedCallback } from "use-debounce";
import { useParamsStore } from "../hooks/useParamsStore";

export default function SearchInput() {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const { replace } = useRouter();
  const setParams = useParamsStore((state) => state.setParams);

  // Функция задержки (300мс), чтобы не делать запрос на каждый символ
  const handleSearch = useDebouncedCallback((term: string) => {
    const params = new URLSearchParams(searchParams);
    params.set("pageNumber", "1"); // При поиске всегда возвращаемся на 1 страницу

    if (term) {
      params.set("search", term);
    } else {
      params.delete("search");
    }

    // Обновляем Zustand для синхронизации (опционально)
    setParams({ search: term, pageNumber: 1 });

    // Обновляем URL без перезагрузки страницы
    replace(`${pathname}?${params.toString()}`);
  }, 300);

  return (
    <div className="relative w-full">
      <TextInput
        id="search"
        type="text"
        icon={HiSearch}
        placeholder="Поиск клиента..."
        defaultValue={searchParams.get("search")?.toString()}
        onChange={(e) => handleSearch(e.target.value)}
      />
    </div>
  );
}
