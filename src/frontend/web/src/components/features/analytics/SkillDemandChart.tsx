"use client";

import { Card } from "@/components/ui";
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from "recharts";

interface SkillDemandChartProps {
  skills: Array<{ skill: string; count: number }>;
}

export function SkillDemandChart({ skills }: SkillDemandChartProps) {
  return (
    <Card variant="lowest" padding="lg" className="rounded-2xl">
      <h3 className="text-sm font-bold text-on-surface-variant uppercase tracking-widest mb-6">Top Skills in Demand</h3>
      <ResponsiveContainer width="100%" height={300}>
        <BarChart data={skills} layout="vertical" margin={{ left: 20 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e2e1ed" />
          <XAxis type="number" stroke="#7a7a84" fontSize={12} />
          <YAxis type="category" dataKey="skill" stroke="#7a7a84" fontSize={12} width={100} />
          <Tooltip />
          <Bar dataKey="count" fill="#3152d3" radius={[0, 4, 4, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </Card>
  );
}
