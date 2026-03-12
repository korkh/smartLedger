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
import Logo from "./Logo";
import UserDropDown from "./UserDropDown";

export function Header() {
  const { data: session, status } = useSession();
  const user = session?.user;
  const isLoading = status === "loading";

  return (
    <Navbar fluid rounded border className="sticky top-0 z-50">
      <Logo />

      <div className="flex md:order-2 gap-2 items-center">
        <DarkThemeToggle />

        {isLoading ? (
          <Spinner size="sm" />
        ) : user && user.name ? (
          <UserDropDown user={user} />
        ) : (
          <Button size="sm" as={Link} href="/login">
            Войти
          </Button>
        )}
        <NavbarToggle />
      </div>

      <NavbarCollapse>
        <NavbarLink as={Link} href="/" active>
          Главная
        </NavbarLink>
        {session && (
          <>
            <NavbarLink as={Link} href="/dashboard">
              Дашборд
            </NavbarLink>
            <NavbarLink as={Link} href="/clients">
              Клиенты
            </NavbarLink>
          </>
        )}
        <NavbarLink href="#">Услуги</NavbarLink>
        <NavbarLink href="#">Контакты</NavbarLink>
      </NavbarCollapse>
    </Navbar>
  );
}
