"use client";

import {
  Button,
  DarkThemeToggle,
  Navbar,
  NavbarBrand,
  NavbarCollapse,
  NavbarLink,
  NavbarToggle,
} from "flowbite-react";
import { signOut, useSession } from "next-auth/react";
import Link from "next/link";

export function Header() {
  const { data: session } = useSession();

  return (
    <Navbar fluid rounded border>
      <NavbarBrand as={Link} href="/">
        <span className="self-center whitespace-nowrap text-xl font-bold italic dark:text-white text-blue-700">
          SmartLedger
        </span>
      </NavbarBrand>
      <div className="flex md:order-2 gap-2 items-center">
        <DarkThemeToggle />
        {session ? (
          <div className="flex items-center gap-4">
            <span className="text-sm hidden md:block">
              Привет, <strong>{session.user?.username}</strong>
            </span>
            <Button size="sm" color="failure" onClick={() => signOut()}>
              Выйти
            </Button>
          </div>
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
