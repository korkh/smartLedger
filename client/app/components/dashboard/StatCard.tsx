// app/dashboard/components/StatCard.tsx
import { IconType } from "react-icons";

interface Props {
  title: string;
  value: string;
  icon: IconType;
  trend: string;
  color: "blue" | "green" | "red" | "yellow";
}

const colors = {
  blue: "bg-blue-100 text-blue-600",
  green: "bg-green-100 text-green-600",
  red: "bg-red-100 text-red-600",
  yellow: "bg-yellow-100 text-yellow-600",
};

export default function StatCard({
  title,
  value,
  icon: Icon,
  trend,
  color,
}: Props) {
  return (
    <div className="bg-white dark:bg-gray-800 p-5 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 flex items-center justify-between">
      <div>
        <p className="text-sm text-gray-500 dark:text-gray-400 font-medium">
          {title}
        </p>
        <h2 className="text-2xl font-bold mt-1 dark:text-white">{value}</h2>
        <p
          className={`text-xs mt-2 font-medium ${color === "red" ? "text-red-500" : "text-green-500"}`}
        >
          {trend}
        </p>
      </div>
      <div className={`p-3 rounded-lg ${colors[color]}`}>
        <Icon className="w-6 h-6" />
      </div>
    </div>
  );
}
