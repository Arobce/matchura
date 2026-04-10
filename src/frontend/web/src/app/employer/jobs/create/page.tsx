"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Input, Textarea, Select, Button, Alert, Card, SectionHeader, Badge, Combobox } from "@/components/ui";
import { api } from "@/lib/api";
import type { CreateJobRequest, Skill } from "@/lib/types";
import { X } from "lucide-react";

const EMPLOYMENT_TYPES = [
  { value: "FullTime", label: "Full Time" },
  { value: "PartTime", label: "Part Time" },
  { value: "Contract", label: "Contract" },
  { value: "Internship", label: "Internship" },
  { value: "Remote", label: "Remote" },
];

const IMPORTANCE_LEVELS = [
  { value: "Required", label: "Required" },
  { value: "Preferred", label: "Preferred" },
  { value: "NiceToHave", label: "Nice to Have" },
];

export default function CreateJobPage() {
  const router = useRouter();
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [allSkills, setAllSkills] = useState<Skill[]>([]);
  const [selectedSkillId, setSelectedSkillId] = useState("");
  const [selectedImportance, setSelectedImportance] = useState("Required");

  const [form, setForm] = useState<CreateJobRequest>({
    title: "",
    description: "",
    location: "",
    employmentType: "FullTime",
    experienceRequired: 0,
    salaryMin: undefined,
    salaryMax: undefined,
    applicationDeadline: "",
    skills: [],
  });

  useEffect(() => {
    api.get<Skill[]>("/api/skills").then(setAllSkills).catch(() => {});
  }, []);

  const update = (field: string, value: string | number | undefined) =>
    setForm((prev) => ({ ...prev, [field]: value }));

  const addSkill = () => {
    if (!selectedSkillId) return;
    if (form.skills.some((s) => s.skillId === selectedSkillId)) return;
    setForm((prev) => ({
      ...prev,
      skills: [...prev.skills, { skillId: selectedSkillId, importanceLevel: selectedImportance }],
    }));
    setSelectedSkillId("");
  };

  const removeSkill = (skillId: string) => {
    setForm((prev) => ({
      ...prev,
      skills: prev.skills.filter((s) => s.skillId !== skillId),
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const payload = {
        ...form,
        salaryMin: form.salaryMin || undefined,
        salaryMax: form.salaryMax || undefined,
        applicationDeadline: form.applicationDeadline || undefined,
        location: form.location || undefined,
      };
      const job = await api.post<{ jobId: string }>("/api/jobs", payload);
      router.push(`/jobs/${job.jobId}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create job");
    } finally {
      setLoading(false);
    }
  };

  const getSkillName = (skillId: string) =>
    allSkills.find((s) => s.skillId === skillId)?.skillName || skillId;

  return (
    <>
      <Navbar />
      <PageContainer>
        <SectionHeader title="Post a New Job" subtitle="Fill in the details to create a job listing" />

        <form onSubmit={handleSubmit} className="space-y-8 mt-8">
          {error && <Alert variant="error">{error}</Alert>}

          <Card>
            <h3 className="text-lg font-bold text-on-surface mb-4">Basic Information</h3>
            <div className="space-y-4">
              <Input
                label="Job Title"
                value={form.title}
                onChange={(e) => update("title", e.target.value)}
                placeholder="e.g. Senior Software Engineer"
                required
              />
              <Textarea
                label="Description"
                value={form.description}
                onChange={(e) => update("description", e.target.value)}
                placeholder="Describe the role, responsibilities, and requirements..."
                rows={6}
                required
              />
              <div className="grid grid-cols-2 gap-4">
                <Input
                  label="Location"
                  value={form.location || ""}
                  onChange={(e) => update("location", e.target.value)}
                  placeholder="e.g. New York, NY"
                />
                <Select
                  label="Employment Type"
                  value={form.employmentType}
                  onChange={(e) => update("employmentType", e.target.value)}
                  options={EMPLOYMENT_TYPES}
                />
              </div>
            </div>
          </Card>

          <Card>
            <h3 className="text-lg font-bold text-on-surface mb-4">Compensation & Experience</h3>
            <div className="space-y-4">
              <div className="grid grid-cols-3 gap-4">
                <Input
                  label="Min Salary ($)"
                  type="number"
                  value={form.salaryMin?.toString() || ""}
                  onChange={(e) => update("salaryMin", e.target.value ? Number(e.target.value) : undefined)}
                  placeholder="50000"
                />
                <Input
                  label="Max Salary ($)"
                  type="number"
                  value={form.salaryMax?.toString() || ""}
                  onChange={(e) => update("salaryMax", e.target.value ? Number(e.target.value) : undefined)}
                  placeholder="80000"
                />
                <Input
                  label="Experience (years)"
                  type="number"
                  value={form.experienceRequired.toString()}
                  onChange={(e) => update("experienceRequired", Number(e.target.value))}
                  min={0}
                  max={30}
                  required
                />
              </div>
              <Input
                label="Application Deadline"
                type="date"
                value={form.applicationDeadline || ""}
                onChange={(e) => update("applicationDeadline", e.target.value)}
              />
            </div>
          </Card>

          <Card>
            <h3 className="text-lg font-bold text-on-surface mb-4">Required Skills</h3>
            <div className="flex gap-3 items-end">
              <div className="flex-1">
                <Combobox
                  label="Skill"
                  value={selectedSkillId}
                  onChange={(value) => setSelectedSkillId(value)}
                  onCreateNew={async (name) => {
                    try {
                      const created = await api.post<Skill>("/api/skills", { skillName: name, skillCategory: "General" });
                      setAllSkills((prev) => [...prev, created]);
                      setSelectedSkillId(created.skillId);
                    } catch (err) {
                      setError(err instanceof Error ? err.message : "Failed to create skill");
                    }
                  }}
                  options={allSkills.map((s) => ({ value: s.skillId, label: s.skillName }))}
                  placeholder="Search or type a new skill..."
                />
              </div>
              <div className="w-40">
                <Select
                  label="Importance"
                  value={selectedImportance}
                  onChange={(e) => setSelectedImportance(e.target.value)}
                  options={IMPORTANCE_LEVELS}
                />
              </div>
              <Button type="button" variant="secondary" onClick={addSkill}>
                Add
              </Button>
            </div>
            {form.skills.length > 0 && (
              <div className="flex flex-wrap gap-2 mt-4">
                {form.skills.map((s) => (
                  <Badge key={s.skillId} variant={s.importanceLevel === "Required" ? "primary" : s.importanceLevel === "Preferred" ? "tertiary" : "muted"}>
                    {getSkillName(s.skillId)} ({s.importanceLevel})
                    <button type="button" onClick={() => removeSkill(s.skillId)} className="ml-1 hover:text-on-primary">
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                ))}
              </div>
            )}
          </Card>

          <div className="flex gap-4 justify-end">
            <Button type="button" variant="outline" onClick={() => router.back()}>
              Cancel
            </Button>
            <Button type="submit" loading={loading}>
              Publish Job
            </Button>
          </div>
        </form>
      </PageContainer>
    </>
  );
}
