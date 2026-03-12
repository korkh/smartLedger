"use client";
import {
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
} from "recharts";

interface Props {
  data: { name: string; value: number; color: string }[];
}

export default function RiskDistribution({ data }: Props) {
  // Функция для отрисовки меток снаружи
  const renderLabel = (entry: any) => {
    if (entry.value === 0) return null; // Не рисуем нули
    return `${entry.value}`;
  };

  return (
    <div className="h-75 w-full">
      <ResponsiveContainer width="100%" height="100%">
        <PieChart>
          <Pie
            data={data}
            cx="50%"
            cy="50%"
            innerRadius={70}
            outerRadius={100}
            paddingAngle={5}
            dataKey="value"
            label={renderLabel}
            labelLine={true}
          >
            {data.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={entry.color} stroke="none" />
            ))}
          </Pie>

          {/* Общее количество в центре */}
          <text
            x="50%"
            y="50%"
            textAnchor="middle"
            dominantBaseline="middle"
            className="fill-gray-800 dark:fill-white font-bold text-xxl"
          >
            {data.reduce((acc, item) => acc + item.value, 0)}
          </text>

          <Tooltip
            formatter={(value: any) => [`${value} клиентов`, "Статус"]}
            contentStyle={{ borderRadius: "8px", border: "none" }}
          />
          <Legend verticalAlign="bottom" height={36} iconType="circle" />
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
}
