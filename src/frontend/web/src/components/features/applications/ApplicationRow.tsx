import { StatusBadge } from "@/components/composed";
import { formatDate } from "@/lib/utils";
import type { Application } from "@/lib/types";
import Link from "next/link";
import { FileText } from "lucide-react";

interface ApplicationRowProps {
  application: Application;
}

export function ApplicationRow({ application: app }: ApplicationRowProps) {
  return (
    <Link
      href={`/applications/${app.applicationId}`}
      className="block bg-surface-container-lowest p-5 rounded-xl editorial-shadow flex items-center justify-between transition-all hover:translate-x-1 cursor-pointer"
    >
      <div className="flex items-center gap-4">
        <FileText className="h-8 w-8 text-primary shrink-0" />
        <div>
          <span className="text-on-surface font-semibold text-lg">
            Job: {app.jobId.slice(0, 8)}...
          </span>
          <p className="text-on-surface-variant text-sm">
            Applied {formatDate(app.appliedAt)}
            {app.updatedAt !== app.appliedAt && (
              <> &middot; Updated {formatDate(app.updatedAt)}</>
            )}
          </p>
          {app.coverLetter && (
            <p className="text-sm text-on-surface-variant mt-1 line-clamp-1">{app.coverLetter}</p>
          )}
        </div>
      </div>
      <StatusBadge status={app.status} />
    </Link>
  );
}
