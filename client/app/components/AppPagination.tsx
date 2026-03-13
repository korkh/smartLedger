"use client";

import { Pagination } from "flowbite-react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";

export default function AppPagination({
  currentPage,
  totalPages,
}: {
  currentPage: number;
  totalPages: number;
}) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const onPageChange = (page: number) => {
    // Не даем перейти за границы
    if (page < 1 || page > totalPages) return;

    const params = new URLSearchParams(searchParams.toString());
    params.set("pageNumber", page.toString());

    // Используем router.push для обновления URL
    router.push(`${pathname}?${params.toString()}`);
  };

  return (
    <div className="flex overflow-x-auto justify-center">
      <Pagination
        layout="pagination"
        currentPage={currentPage}
        totalPages={totalPages}
        onPageChange={onPageChange}
        showIcons={true} // Отображает иконки < и >
        previousLabel="Назад"
        nextLabel="Вперед"
        className="text-sm md:text-base"
      />
    </div>
  );
}
