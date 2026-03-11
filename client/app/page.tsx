"use client";
import { Button } from "flowbite-react";
import Link from "next/link";
import { Header } from "./components/layout/Header";

export default function Home() {
  return (
    <main>
      <Header />
      <section className="bg-white dark:bg-gray-900">
        <div className="py-8 px-4 mx-auto max-w-7xl text-center lg:py-16">
          <h1 className="mb-4 text-4xl font-extrabold tracking-tight leading-none text-gray-900 md:text-5xl lg:text-6xl dark:text-white">
            Управляйте бухгалтерией с умом
          </h1>
          <p className="mb-8 text-lg font-normal text-gray-500 lg:text-xl sm:px-16 lg:px-48 dark:text-gray-400">
            Smart Ledger объединяет автоматизацию отчетности и анализ данных с
            помощью ИИ для вашего бизнеса в Норвегии.
          </p>
          <div className="flex flex-col space-y-4 sm:flex-row sm:justify-center sm:space-y-0">
            <Button size="xl" as={Link} href="/login" className="mr-3">
              Начать работу
            </Button>
            <Button size="xl" color="light">
              Узнать больше
            </Button>
          </div>
        </div>
      </section>
    </main>
  );
}
