import { formatDate } from "@/lib/utils";
import { Button } from "@/components/ui";
import type { Application } from "@/lib/types";

interface ApplicationDetailsSectionProps {
  application: Application;
}

export function ApplicationDetailsSection({ application }: ApplicationDetailsSectionProps) {
  return (
    <div className="bg-surface-container-lowest rounded-xl editorial-shadow p-6 space-y-6">
      <h3 className="text-lg font-bold text-on-surface">Application Details</h3>

      <div className="grid grid-cols-2 gap-4 text-sm">
        <div>
          <span className="text-on-surface-variant">Applied</span>
          <p className="font-medium text-on-surface">{formatDate(application.appliedAt)}</p>
        </div>
        <div>
          <span className="text-on-surface-variant">Last Updated</span>
          <p className="font-medium text-on-surface">{formatDate(application.updatedAt)}</p>
        </div>
      </div>

      {application.resumeUrl && (
        <div>
          <h4 className="text-sm font-bold text-on-surface mb-2">Resume</h4>
          <Button variant="outline" size="sm" href={application.resumeUrl}>
            View Resume
          </Button>
        </div>
      )}

      {application.coverLetter && (
        <div>
          <h4 className="text-sm font-bold text-on-surface mb-2">Cover Letter</h4>
          <div className="bg-surface-container-low rounded-lg p-4 text-sm text-on-surface-variant leading-relaxed whitespace-pre-wrap">
            {application.coverLetter}
          </div>
        </div>
      )}
    </div>
  );
}
