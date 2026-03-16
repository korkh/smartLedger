"use client";

import {
  Button,
  DarkThemeToggle,
  Navbar,
  NavbarCollapse,
  NavbarLink,
  NavbarToggle,
  Spinner,
} from "flowbite-react";
import { useSession } from "next-auth/react";
import Link from "next/link";
import { usePathname } from "next/navigation"; // Для активных ссылок
import Logo from "./Logo";
import UserDropDown from "./UserDropDown";

export function Header() {
  const { data: session, status } = useSession();
  const pathname = usePathname();

  const user = session?.user;
  const role = (user as any)?.role;
  const isLoading = status === "loading";

  // Логика доступов для фронтенда (согласно твоему Excel)
  const isAdmin = role === "Admin"; // Уровень 3
  const isSenior = role === "Senior_Accountant" || isAdmin; // Уровень 2+
  const isJunior = role === "Junior_Accountant" || isSenior;

  return (
    <Navbar fluid rounded border className="sticky top-0 z-50">
      <Logo />

      <div className="flex md:order-2 gap-2 items-center">
        <DarkThemeToggle />

        {isLoading ? (
          <Spinner size="sm" />
        ) : user ? (
          <UserDropDown user={user} />
        ) : (
          <Button size="sm" as={Link} href="/login">
            Войти
          </Button>
        )}
        <NavbarToggle />
      </div>

      <NavbarCollapse>
        <NavbarLink as={Link} href="/" active={pathname === "/"}>
          Главная
        </NavbarLink>

        {session && (
          <>
            {/* LEVEL 3: Только Admin */}
            {isAdmin && (
              <NavbarLink
                as={Link}
                href="/dashboard"
                active={pathname === "/dashboard"}
              >
                Дашборд
              </NavbarLink>
            )}

            {/* LEVEL 1+: Клиенты доступны ВСЕМ сотрудникам (Junior тоже видит реестр) */}
            {isJunior && (
              <>
                <NavbarLink
                  as={Link}
                  href="/clients"
                  active={pathname === "/clients"}
                >
                  Клиенты
                </NavbarLink>
                <NavbarLink
                  as={Link}
                  href="/transactions"
                  active={pathname === "/transactions"}
                >
                  Транзакции
                </NavbarLink>
              </>
            )}

            {/* LEVEL 2+: Услуги/Тарифы обычно управляются Senior+ */}
            {isSenior && (
              <>
                <NavbarLink
                  as={Link}
                  href="/services"
                  active={pathname === "/services"}
                >
                  Услуги
                </NavbarLink>
                <NavbarLink
                  as={Link}
                  href="/transactions"
                  active={pathname === "/transactions"}
                >
                  Транзакции
                </NavbarLink>
              </>
            )}

            <NavbarLink
              as={Link}
              href="/contacts"
              active={pathname === "/contacts"}
            >
              Контакты
            </NavbarLink>
          </>
        )}
      </NavbarCollapse>
    </Navbar>
  );
}
