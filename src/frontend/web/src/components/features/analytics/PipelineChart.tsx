"use client";

import { Card } from "@/components/ui";
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip } from "recharts";

const COLORS = ["#3152d3", "#5e7cfd", "#6f557d", "#e7c6f5", "#b1b1bc", "#22c55e", "#ef4444"];

interface PipelineChartProps {
  breakdown: Record<string, number>;
}

export function PipelineChart({ breakdown }: PipelineChartProps) {
  const data = Object.entries(breakdown).map(([name, value]) => ({ name, value }));

  return (
    <Card variant="lowest" padding="lg" className="rounded-2xl">
      <h3 className="text-sm font-bold text-on-surface-variant uppercase tracking-widest mb-6">Application Pipeline</h3>
      <ResponsiveContainer width="100%" height={300}>
        <PieChart>
          <Pie data={data} cx="50%" cy="50%" innerRadius={60} outerRadius={100} dataKey="value" paddingAngle={2}>
            {data.map((_, i) => (
              <Cell key={i} fill={COLORS[i % COLORS.length]} />
            ))}
          </Pie>
          <Tooltip />
          <Legend />
        </PieChart>
      </ResponsiveContainer>
    </Card>
  );
}
