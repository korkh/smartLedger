// middleware.ts
import { auth } from "@/auth";

export default auth((req) => {
  // req.auth содержит сессию.
  // Если нужно добавить кастомную логику редиректа, можно сделать это здесь.
  const isLoggedIn = !!req.auth;
  const isProtectedRoute =
    req.nextUrl.pathname.startsWith("/clients") ||
    req.nextUrl.pathname.startsWith("/dashboard");

  if (isProtectedRoute && !isLoggedIn) {
    return Response.redirect(new URL("/login", req.nextUrl));
  }
});

export const config = {
  // Массив путей, для которых будет работать middleware
  matcher: ["/clients/:path*", "/dashboard/:path*"],
};
