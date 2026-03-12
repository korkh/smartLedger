import NextAuth from "next-auth";
import Credentials from "next-auth/providers/credentials";

if (process.env.NODE_ENV === "development") {
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
}

export const { handlers, signIn, signOut, auth } = NextAuth({
  secret: process.env.AUTH_SECRET,
  providers: [
    Credentials({
      name: "Credentials",
      credentials: {
        // Мы используем email как логин
        email: { label: "Email", type: "email" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials) {
        if (!credentials?.email || !credentials?.password) return null;

        try {
          const res = await fetch("https://localhost:5001/api/account/login", {
            method: "POST",
            body: JSON.stringify({
              email: credentials.email,
              password: credentials.password,
            }),
            headers: { "Content-Type": "application/json" },
          });

          // Безопасное чтение ответа
          const contentType = res.headers.get("content-type");

          if (
            res.ok &&
            contentType &&
            contentType.includes("application/json")
          ) {
            const user = await res.json();
            return {
              id: user.userName,
              name: user.displayedName,
              email: credentials.email as string,
              accessToken: user.accessToken,
              userName: user.userName,
            };
          } else {
            // Если это текст (например "Invalid Email"), логируем его и возвращаем null
            const errorText = await res.text();
            console.warn("Бэкенд вернул ошибку:", errorText);
            return null;
          }
        } catch (error) {
          console.error("Сетевая ошибка при авторизации:", error);
          return null;
        }
      },
    }),
  ],
  session: {
    strategy: "jwt",
    maxAge: 30 * 24 * 60 * 60, // 30 days
  },
  callbacks: {
    async jwt({ token, user }) {
      if (user) {
        token.accessToken = (user as any).accessToken;
        token.username = (user as any).userName;
      }
      return token;
    },
    async session({ session, token }) {
      if (token && session.user) {
        session.accessToken = token.accessToken as string;
        session.user.username = token.username as string;
      }
      return session;
    },
  },
  pages: {
    signIn: "/login",
  },
});
