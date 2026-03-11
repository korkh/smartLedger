import { auth } from "@/auth";
import { getSession } from "next-auth/react";

const baseUrl = process.env.NEXT_PUBLIC_API_URL;

function getFullUrl(url: string): string {
  // Проверяем, задан ли baseUrl вообще
  if (!baseUrl) {
    console.error(
      "Критическая ошибка: NEXT_PUBLIC_API_URL не задан в .env файле!",
    );
    return url;
  }

  const cleanBaseUrl = baseUrl.endsWith("/") ? baseUrl : `${baseUrl}/`;
  const cleanUrl = url.startsWith("/") ? url.substring(1) : url;
  return `${cleanBaseUrl}${cleanUrl}`;
}

export async function get<T>(url: string): Promise<T> {
  const requestOptions = {
    method: "GET",
    headers: await getHeaders(),
  };
  const response = await fetch(getFullUrl(url), requestOptions);
  return handleResponse(response);
}

export async function post(url: string, body: object) {
  const requestOptions = {
    method: "POST",
    headers: await getHeaders(),
    body: JSON.stringify(body),
  };
  return await fetch(getFullUrl(url), requestOptions).then(handleResponse);
}

export async function put(url: string, body: object) {
  const requestOptions = {
    method: "PUT",
    headers: await getHeaders(),
    body: JSON.stringify(body),
  };
  return await fetch(getFullUrl(url), requestOptions).then(handleResponse);
}

export async function del(url: string) {
  const requestOptions = {
    method: "DELETE",
    headers: await getHeaders(),
  };
  return await fetch(getFullUrl(url), requestOptions).then(handleResponse);
}

async function getHeaders(): Promise<Headers> {
  let session;

  // Проверяем, где выполняется код
  if (typeof window === "undefined") {
    // Мы на СЕРВЕРЕ
    session = await auth();
  } else {
    // Мы на КЛИЕНТЕ (в браузере)
    session = await getSession();
  }

  const headers = new Headers();
  headers.set("Content-Type", "application/json");

  if (session?.accessToken) {
    headers.set("Authorization", `Bearer ${session.accessToken}`);
  }

  return headers;
}

// Helper function to handle responses
async function handleResponse(response: Response) {
  const text = await response.text();
  let data;

  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    data = text;
  }

  if (response.ok) {
    return data || response.statusText;
  } else {
    const error = {
      status: response.status,
      message:
        typeof data === "object" && data !== null && "message" in data
          ? data.message
          : data || response.statusText,
    };
    return { error };
  }
}

export const fetchWrapper = {
  get,
  post,
  put,
  del,
};
