"use client";

import { Alert, Button, Card, Label, TextInput } from "flowbite-react";
import { signIn } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function LoginPage() {
  const [error, setError] = useState<string | null>(null);
  const router = useRouter();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    const formData = new FormData(e.currentTarget);
    // Берем "email", так как это ожидает провайдер в auth.ts
    const email = formData.get("email") as string;
    const password = formData.get("password") as string;

    const result = await signIn("credentials", {
      email, // Передаем email
      password,
      redirect: false,
    });

    if (result?.error) {
      setError("Ошибка входа. Проверьте данные.");
    } else {
      router.push("/clients");
      router.refresh();
    }
  };

  return (
    <div className="flex items-center justify-center min-h-[80vh]">
      <Card className="max-w-md w-full">
        <h2 className="text-2xl font-bold text-center dark:text-white">Вход</h2>
        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          {error && <Alert color="failure">{error}</Alert>}
          <div>
            <Label htmlFor="email">Email</Label>
            <TextInput
              id="email"
              name="email" // Важно: должно совпадать с formData.get("email")
              type="email"
              placeholder="name@company.com"
              required
            />
          </div>
          <div>
            <Label htmlFor="password">Пароль</Label>
            <TextInput id="password" name="password" type="password" required />
          </div>
          <Button type="submit">Войти</Button>
        </form>
      </Card>
    </div>
  );
}
