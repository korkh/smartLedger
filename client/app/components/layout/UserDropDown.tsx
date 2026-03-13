"use client";

import {
  Dropdown,
  DropdownDivider,
  DropdownHeader,
  DropdownItem,
} from "flowbite-react";
import { User } from "next-auth";
import { signOut } from "next-auth/react";
import Link from "next/link";
import {
  HiBriefcase,
  HiChartBar,
  HiCog,
  HiCurrencyDollar,
  HiLogout,
  HiPlusCircle,
  HiUsers,
} from "react-icons/hi";

type UserActionsProps = {
  user: User;
};

export default function UserDropDown({ user }: UserActionsProps) {
  return (
    <Dropdown
      label={<span className="font-medium text-sm">Личный кабинет</span>}
      inline
    >
      <DropdownHeader>
        <span className="block text-xs text-gray-500 dark:text-gray-400">
          Вы вошли как
        </span>
        <span className="block truncate text-sm font-bold">{user.name}</span>
      </DropdownHeader>

      {/* Основные разделы */}
      <DropdownItem icon={HiChartBar} as={Link} href="/dashboard">
        Дашборд
      </DropdownItem>
      <DropdownItem icon={HiUsers} as={Link} href="/clients">
        Клиенты
      </DropdownItem>
      <DropdownItem icon={HiBriefcase} as={Link} href="/services">
        Услуги
      </DropdownItem>

      <DropdownDivider />

      {/* Действия (Быстрое создание) */}
      <DropdownItem icon={HiPlusCircle} as={Link} href="/clients/create">
        Новый клиент
      </DropdownItem>
      <DropdownItem
        icon={HiCurrencyDollar}
        as={Link}
        href="/transactions/create"
      >
        Новая транзакция
      </DropdownItem>

      <DropdownDivider />

      {/* Настройки и выход */}
      <DropdownItem icon={HiCog} as={Link} href="/settings">
        Настройки
      </DropdownItem>

      <DropdownDivider />

      <DropdownItem
        icon={HiLogout}
        onClick={() => signOut({ callbackUrl: "/" })}
        className="text-red-600 dark:text-red-400"
      >
        Выйти
      </DropdownItem>
    </Dropdown>
  );
}
