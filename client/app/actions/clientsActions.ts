"use server";
import { fetchWrapper } from "@/lib/fetchWrapper";
import { Client, PagedResult } from "@/Types";
import { revalidatePath } from "next/cache";

export async function getData(params: string): Promise<PagedResult<Client>> {
  return fetchWrapper.get(`clients${params}`);
}

export async function getClientById(id: string): Promise<Client> {
  return fetchWrapper.get(`clients/${id}`);
}

export async function getAiInsightForClient(
  id: string,
  client: Client,
): Promise<{ insight: string }> {
  return fetchWrapper.post(`clients/${id}/analyze`, client);
}

export async function getDataPaged1000(
  params: string,
): Promise<PagedResult<Client>> {
  return fetchWrapper.get(`clients?searchTerm=${params}&pageSize=1000`);
}

export async function deleteClient(id: string) {
  const response = await fetchWrapper.del(`clients/${id}`);
  revalidatePath("/clients");
  return response;
}

export async function hardDeleteClient(id: string) {
  const response = await fetchWrapper.del(`clients/${id}/hard`);
  revalidatePath("/clients");
  return response;
}
