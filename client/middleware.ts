// middleware.ts
import { auth } from "@/auth";
import { NextResponse } from "next/server";

export default auth((req) => {
  const isLoggedIn = !!req.auth;
  const { nextUrl } = req;

  const isProtectedRoute =
    nextUrl.pathname.startsWith("/clients") ||
    nextUrl.pathname.startsWith("/dashboard");

  // if (isProtectedRoute && !isLoggedIn) {
  //   return NextResponse.redirect(new URL("/login", nextUrl));
  // }

  return NextResponse.next();
});

export const config = {
  // Исключаем внутренние запросы Next.js, картинки и статику
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico).*)"],
};
