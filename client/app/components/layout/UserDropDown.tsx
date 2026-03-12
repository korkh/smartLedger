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
import { HiCog, HiLogout, HiTruck } from "react-icons/hi";

type UserActionsProps = {
  user: User;
};

export default function UserDropDown({ user }: UserActionsProps) {
  return (
    <Dropdown label={`Welcome ${user.name}`} inline color="gray">
      <DropdownHeader>
        <span className="block text-sm">Signed in as</span>
        <span className="block truncate text-sm font-medium">{user.name}</span>
      </DropdownHeader>
      <DropdownItem icon={HiTruck}>
        <Link href="/dashboard">Show dashboard</Link>
      </DropdownItem>

      <DropdownItem icon={HiTruck}>
        <Link href="/clients/create">Create new client</Link>
      </DropdownItem>
      <DropdownItem icon={HiTruck}>
        <Link href="/transactions/create">Create new transaction</Link>
      </DropdownItem>

      <DropdownItem icon={HiCog}>
        <Link href="/settings">Settings</Link>
      </DropdownItem>

      <DropdownDivider />

      <DropdownItem
        icon={HiLogout}
        onClick={() => signOut({ redirectTo: "/" })}
      >
        Sign out
      </DropdownItem>
    </Dropdown>
  );
}
