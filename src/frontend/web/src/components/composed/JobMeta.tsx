import { MapPin, Clock, Briefcase } from "lucide-react";
import { SalaryDisplay } from "./SalaryDisplay";

interface JobMetaProps {
  location?: string;
  employmentType: string;
  minSalary?: number;
  maxSalary?: number;
  experienceMin?: number;
  experienceMax?: number;
  className?: string;
}

export function JobMeta({
  location,
  employmentType,
  minSalary,
  maxSalary,
  experienceMin,
  experienceMax,
  className,
}: JobMetaProps) {
  return (
    <div className={`space-y-3 ${className ?? ""}`}>
      {location && (
        <div className="flex items-center gap-2 text-sm text-on-surface-variant">
          <MapPin className="h-4 w-4" /> {location}
        </div>
      )}
      <div className="flex items-center gap-2 text-sm text-on-surface-variant">
        <Clock className="h-4 w-4" /> {employmentType}
      </div>
      <SalaryDisplay min={minSalary} max={maxSalary} />
      {experienceMin != null && (
        <div className="flex items-center gap-2 text-sm text-on-surface-variant">
          <Briefcase className="h-4 w-4" />
          {experienceMin}{experienceMax ? `–${experienceMax}` : "+"} years
        </div>
      )}
    </div>
  );
}
