"use client";

import { Card } from "@/components/ui";
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from "recharts";

interface TrendChartProps {
  data: Array<{ week: string; value: number }>;
  label: string;
  color?: string;
}

export function TrendChart({ data, label, color = "#3152d3" }: TrendChartProps) {
  return (
    <Card variant="lowest" padding="lg" className="rounded-2xl">
      <h3 className="text-sm font-bold text-on-surface-variant uppercase tracking-widest mb-6">{label}</h3>
      <ResponsiveContainer width="100%" height={250}>
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e2e1ed" />
          <XAxis dataKey="week" stroke="#7a7a84" fontSize={12} />
          <YAxis stroke="#7a7a84" fontSize={12} />
          <Tooltip />
          <Line type="monotone" dataKey="value" stroke={color} strokeWidth={2} dot={{ r: 4 }} />
        </LineChart>
      </ResponsiveContainer>
    </Card>
  );
}
