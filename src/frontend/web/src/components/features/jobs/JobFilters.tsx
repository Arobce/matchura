"use client";

import { SearchInput } from "@/components/composed";
import { Button, Select } from "@/components/ui";
import { useJobFilterStore } from "@/stores";
import { MapPin } from "lucide-react";
import { Input } from "@/components/ui";

const employmentTypes = [
  { value: "", label: "All Types" },
  { value: "FullTime", label: "Full Time" },
  { value: "PartTime", label: "Part Time" },
  { value: "Contract", label: "Contract" },
  { value: "Internship", label: "Internship" },
  { value: "Remote", label: "Remote" },
];

export function JobFilters() {
  const { search, location, employmentType, setSearch, setLocation, setEmploymentType } =
    useJobFilterStore();

  return (
    <section className="bg-surface-container-low rounded-xl p-8 mb-12 shadow-sm">
      <div className="grid grid-cols-1 md:grid-cols-12 gap-4">
        <div className="md:col-span-4">
          <SearchInput
            value={search}
            onChange={setSearch}
            placeholder="Search for jobs"
          />
        </div>
        <div className="md:col-span-3">
          <Input
            icon={MapPin}
            value={location}
            onChange={(e) => setLocation(e.target.value)}
            placeholder="City, state, or remote"
          />
        </div>
        <div className="md:col-span-3">
          <Select
            options={employmentTypes}
            value={employmentType}
            onChange={(e) => setEmploymentType(e.target.value)}
          />
        </div>
        <div className="md:col-span-2">
          <Button fullWidth className="h-full">Search</Button>
        </div>
      </div>
    </section>
  );
}
