import { fetchWrapper } from "@/lib/fetchWrapper";
import { Client, PagedResult } from "@/Types";

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
