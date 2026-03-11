import { fetchWrapper } from "@/lib/fetchWrapper";
import { Client, PagedResult } from "@/Types";

export async function getData(params: string): Promise<PagedResult<Client>> {
  return fetchWrapper.get(`clients${params}`);
}
