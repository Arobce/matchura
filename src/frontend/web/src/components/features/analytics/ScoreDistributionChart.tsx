"use client";

import { Card } from "@/components/ui";
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from "recharts";

interface ScoreDistributionChartProps {
  distribution: Record<string, number>;
}

export function ScoreDistributionChart({ distribution }: ScoreDistributionChartProps) {
  const data = Object.entries(distribution).map(([range, count]) => ({ range, count }));

  return (
    <Card variant="lowest" padding="lg" className="rounded-2xl">
      <h3 className="text-sm font-bold text-on-surface-variant uppercase tracking-widest mb-6">Score Distribution</h3>
      <ResponsiveContainer width="100%" height={250}>
        <BarChart data={data}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e2e1ed" />
          <XAxis dataKey="range" stroke="#7a7a84" fontSize={12} />
          <YAxis stroke="#7a7a84" fontSize={12} />
          <Tooltip />
          <Bar dataKey="count" fill="#5e7cfd" radius={[4, 4, 0, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </Card>
  );
}
