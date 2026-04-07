"use client";

import { cn } from "@/lib/utils";

interface RoleToggleProps {
  value: "Candidate" | "Employer";
  onChange: (role: "Candidate" | "Employer") => void;
}

export function RoleToggle({ value, onChange }: RoleToggleProps) {
  return (
    <div className="flex rounded-lg overflow-hidden border border-outline-variant/30">
      {(["Candidate", "Employer"] as const).map((role) => (
        <button
          key={role}
          type="button"
          onClick={() => onChange(role)}
          className={cn(
            "flex-1 py-3 text-sm font-semibold transition-all duration-200",
            value === role
              ? "bg-primary text-on-primary"
              : "bg-transparent text-on-surface-variant hover:bg-surface-container-low"
          )}
        >
          {role}
        </button>
      ))}
    </div>
  );
}
