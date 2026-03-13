"use client";
import {
  Bar,
  BarChart,
  CartesianGrid,
  LabelList,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

interface Props {
  title: string;
  data: { name: string; value: number }[];
  color: string;
}

export default function TopClientsChart({ title, data, color }: Props) {
  return (
    <div className="h-full w-full">
      <h3 className="text-lg font-bold mb-4 dark:text-gray-300">{title}</h3>
      <ResponsiveContainer width="100%" height={400}>
        <BarChart
          data={data}
          layout="vertical"
          margin={{ left: 20, right: 80, top: 5, bottom: 5 }}
        >
          <CartesianGrid
            strokeDasharray="3 3"
            horizontal={true}
            vertical={false}
          />
          <XAxis type="number" hide />
          <YAxis
            dataKey="name"
            type="category"
            width={120}
            tick={{ fontSize: 10, fill: "#9ca3af" }}
          />
          <Tooltip
            formatter={(value: any) => {
              const numValue = Number(value);
              return !isNaN(numValue)
                ? `${numValue.toLocaleString()} ₸`
                : value;
            }}
            contentStyle={{
              backgroundColor: "rgba(255, 255, 255, 0.9)",
              borderRadius: "8px",
              border: "none",
              boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
            }}
            cursor={{ fill: "transparent" }}
          />
          <Bar dataKey="value" fill={color} radius={[0, 4, 4, 0]} barSize={20}>
            <LabelList
              dataKey="value"
              position="right"
              formatter={(value: any) => {
                const numValue = Number(value);
                return !isNaN(numValue)
                  ? `${numValue.toLocaleString()}`
                  : value;
              }}
              style={{ fontSize: "11px", fontWeight: "600", fill: "#6b7280" }}
              offset={10}
            />
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
