export const formatDate = (dateString: string | null) => {
  if (!dateString) return "—";
  const date = new Date(dateString);
  return isNaN(date.getTime()) ? "—" : date.toLocaleDateString();
};
