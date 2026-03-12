// app/layout.tsx
import { ThemeModeScript } from "flowbite-react";
import type { Metadata } from "next";
import { SessionProvider } from "next-auth/react";
import { Inter } from "next/font/google";
import { ToastContainer } from "react-toastify";
import { Header } from "./components/layout/Header";
import "./globals.css";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "Smart Ledger",
  description: "Smart Ledger Management System",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="ru" suppressHydrationWarning>
      <head>
        <ThemeModeScript />
      </head>
      <body
        className={inter.className + "bg-gray-50 dark:bg-gray-900 min-h-screen"}
      >
        <SessionProvider>
          <Header />
          <ToastContainer
            position="bottom-right"
            hideProgressBar
            theme="colored"
          />
          {children}
        </SessionProvider>
      </body>
    </html>
  );
}
