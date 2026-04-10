import { StatusBadge, SkillBadgeList } from "@/components/composed";
import { Card } from "@/components/ui";
import { Alert } from "@/components/ui";
import { formatDate } from "@/lib/utils";
import type { ResumeResponse } from "@/lib/types";
import { FileText } from "lucide-react";

interface ResumeCardProps {
  resume: ResumeResponse;
}

export function ResumeCard({ resume }: ResumeCardProps) {
  return (
    <Card variant="lowest" padding="md" className="border border-outline-variant/15">
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-center gap-3">
          <FileText className="h-8 w-8 text-primary" />
          <div>
            <p className="font-medium text-on-surface">{resume.originalFileName}</p>
            <p className="text-xs text-on-surface-variant">
              Uploaded {formatDate(resume.uploadedAt)}
            </p>
          </div>
        </div>
        <StatusBadge status={resume.status} />
      </div>

      {resume.errorMessage && (
        <Alert variant="error" className="mb-3">{resume.errorMessage}</Alert>
      )}

      {resume.parsedData && (
        <div className="space-y-4">
          {resume.parsedData.skills.length > 0 && (
            <div>
              <h3 className="text-sm font-semibold text-on-surface mb-2">Extracted Skills</h3>
              <SkillBadgeList
                skills={resume.parsedData.skills.map((s) => ({
                  skillName: s.name,
                  proficiencyLevel: s.proficiencyLevel,
                }))}
              />
            </div>
          )}
          {resume.parsedData.experience.length > 0 && (
            <div>
              <h3 className="text-sm font-semibold text-on-surface mb-2">Experience</h3>
              <div className="space-y-2">
                {resume.parsedData.experience.map((exp, i) => (
                  <div key={i} className="text-sm">
                    <p className="font-medium text-on-surface">{exp.title} at {exp.company}</p>
                    <p className="text-xs text-on-surface-variant">
                      {exp.startDate} - {exp.endDate || "Present"}
                    </p>
                  </div>
                ))}
              </div>
            </div>
          )}
          {resume.parsedData.education.length > 0 && (
            <div>
              <h3 className="text-sm font-semibold text-on-surface mb-2">Education</h3>
              <div className="space-y-1">
                {resume.parsedData.education.map((edu, i) => (
                  <p key={i} className="text-sm text-on-surface">
                    {edu.degree} in {edu.field} — {edu.institution}
                  </p>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </Card>
  );
}
