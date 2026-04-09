"use client";

import { formatDate } from "@/lib/utils";
import { Button } from "@/components/ui";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";
import { Download, FileText } from "lucide-react";
import type { Application } from "@/lib/types";

interface ApplicationDetailsSectionProps {
  application: Application;
}

export function ApplicationDetailsSection({ application }: ApplicationDetailsSectionProps) {
  const { addNotification } = useNotificationStore();

  const handleDownloadResume = async () => {
    if (!application.resumeUrl) return;
    try {
      const result = await api.get<{ downloadUrl: string }>(`/api/resumes/${application.resumeUrl}/download`);
      window.open(result.downloadUrl, "_blank");
    } catch {
      addNotification({ type: "error", message: "Failed to download resume" });
    }
  };

  const handleDownloadCoverLetter = async () => {
    if (!application.coverLetterUrl) return;
    try {
      const result = await api.get<{ downloadUrl: string }>(`/api/documents/download?key=${encodeURIComponent(application.coverLetterUrl)}`);
      window.open(result.downloadUrl, "_blank");
    } catch {
      addNotification({ type: "error", message: "Failed to download cover letter" });
    }
  };

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
          <button
            onClick={handleDownloadResume}
            className="flex items-center gap-3 w-full p-4 rounded-lg border border-outline-variant/20 hover:border-primary/30 hover:bg-primary-container/5 transition-all text-left"
          >
            <FileText className="h-8 w-8 text-primary shrink-0" />
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-on-surface">Candidate Resume</p>
              <p className="text-xs text-on-surface-variant">Click to download</p>
            </div>
            <Download className="h-5 w-5 text-on-surface-variant" />
          </button>
        </div>
      )}

      {application.coverLetterUrl && (
        <div>
          <h4 className="text-sm font-bold text-on-surface mb-2">Cover Letter</h4>
          <button
            onClick={handleDownloadCoverLetter}
            className="flex items-center gap-3 w-full p-4 rounded-lg border border-outline-variant/20 hover:border-primary/30 hover:bg-primary-container/5 transition-all text-left"
          >
            <FileText className="h-8 w-8 text-primary shrink-0" />
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-on-surface">Cover Letter PDF</p>
              <p className="text-xs text-on-surface-variant">Click to download</p>
            </div>
            <Download className="h-5 w-5 text-on-surface-variant" />
          </button>
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
