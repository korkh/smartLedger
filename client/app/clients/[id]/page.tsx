import { getClientById } from "@/app/actions/clientsActions";
import ClientTabs from "@/app/components/client/ClientTabs";
import { auth } from "@/auth";
import { notFound } from "next/navigation";

export default async function ClientDetailsPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const session = await auth();
  const role = (session?.user as any)?.role;
  const client = await getClientById(id);

  if (!client) {
    return notFound();
  }

  return <ClientTabs client={client} role={role} />;
}
