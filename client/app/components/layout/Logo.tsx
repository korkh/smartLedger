import { NavbarBrand } from "flowbite-react";
import Link from "next/link";

export default function Logo() {
  return (
    <NavbarBrand as={Link} href="/">
      <span className="self-center whitespace-nowrap text-xl font-bold italic dark:text-white text-blue-700">
        SmartLedger
      </span>
    </NavbarBrand>
  );
}
