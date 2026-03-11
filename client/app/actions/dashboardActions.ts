"use client";

import { fetchWrapper } from "@/lib/fetchWrapper";
import { DashboardData } from "@/Types";

export async function getAiInsight(
  id: string,
  data: DashboardData,
): Promise<{ insight: string }> {
  // Передаем id в URL, а данные в теле
  return fetchWrapper.post(`dashboard/${id}/analyze`, data);
}
