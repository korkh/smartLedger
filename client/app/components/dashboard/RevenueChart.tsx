"use client";

import {
  Area,
  AreaChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

interface Props {
  data: { name: string; base: number; extra: number }[];
}

export default function RevenueChart({ data }: Props) {
  return (
    <ResponsiveContainer width="100%" height="100%">
      <AreaChart
        data={data}
        margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
      >
        <defs>
          <linearGradient id="colorBase" x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.3} />
            <stop offset="95%" stopColor="#3b82f6" stopOpacity={0} />
          </linearGradient>
          <linearGradient id="colorExtra" x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%" stopColor="#10b981" stopOpacity={0.3} />
            <stop offset="95%" stopColor="#10b981" stopOpacity={0} />
          </linearGradient>
        </defs>

        <CartesianGrid
          strokeDasharray="3 3"
          vertical={false}
          stroke="#e5e7eb"
          className="dark:stroke-gray-700"
        />

        <XAxis
          dataKey="name"
          axisLine={false}
          tickLine={false}
          tick={{ fill: "#9ca3af", fontSize: 12 }}
          dy={10}
        />

        <YAxis
          axisLine={false}
          tickLine={false}
          tick={{ fill: "#9ca3af", fontSize: 12 }}
          tickFormatter={(value) => `${(value / 1000).toFixed(0)}k`}
        />

        <Tooltip
          formatter={(val: any) => `${Number(val).toLocaleString()} ₸`}
          contentStyle={{
            backgroundColor: "rgba(255, 255, 255, 0.9)",
            borderRadius: "12px",
            border: "none",
            boxShadow: "0 10px 15px -3px rgb(0 0 0 / 0.1)",
          }}
        />

        <Legend verticalAlign="top" align="right" iconType="circle" />

        {/* Базовая выручка (Тарифы) */}
        <Area
          type="monotone"
          name="Тарифы"
          dataKey="base"
          stackId="1" // Важно для Stacked эффекта
          stroke="#3b82f6"
          strokeWidth={3}
          fill="url(#colorBase)"
        />

        {/* Доп. услуги */}
        <Area
          type="monotone"
          name="Доп. услуги"
          dataKey="extra"
          stackId="1"
          stroke="#10b981"
          strokeWidth={3}
          fill="url(#colorExtra)"
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}
