"use client";

import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { Download, FileText } from "lucide-react";
import { Spinner } from "@/components/ui";

interface PdfViewerProps {
  url: string;
  label: string;
  type: "resume" | "coverLetter";
}

function isPdfUrl(url: string): boolean {
  // Strip query params before checking extension
  const path = url.split("?")[0];
  return path.toLowerCase().endsWith(".pdf");
}

export function PdfViewer({ url, label, type }: PdfViewerProps) {
  const [fileUrl, setFileUrl] = useState<string | null>(null);
  const [error, setError] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchUrl = async () => {
      try {
        let result: { downloadUrl: string };
        if (type === "resume") {
          result = await api.get<{ downloadUrl: string }>(`/api/resumes/${url}/download`);
        } else {
          result = await api.get<{ downloadUrl: string }>(`/api/documents/download?key=${encodeURIComponent(url)}`);
        }
        setFileUrl(result.downloadUrl);
      } catch {
        setError(true);
      } finally {
        setLoading(false);
      }
    };
    fetchUrl();
  }, [url, type]);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64 bg-surface-container-low rounded-lg">
        <Spinner size="md" />
      </div>
    );
  }

  if (error || !fileUrl) {
    return (
      <div className="flex items-center justify-center h-32 bg-surface-container-low rounded-lg">
        <p className="text-sm text-on-surface-variant">Failed to load {label.toLowerCase()}</p>
      </div>
    );
  }

  const canEmbed = isPdfUrl(fileUrl);

  if (!canEmbed) {
    const fileName = decodeURIComponent(fileUrl.split("?")[0].split("/").pop() || label);
    return (
      <div className="space-y-2">
        <div className="flex items-center gap-3 p-4 rounded-lg border border-outline-variant/20 bg-surface-container-low">
          <FileText className="h-8 w-8 text-primary shrink-0" />
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-on-surface truncate">{fileName}</p>
            <p className="text-xs text-on-surface-variant">
              This file type can&apos;t be previewed in the browser
            </p>
          </div>
          <a
            href={fileUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center gap-2 px-3 py-1.5 text-xs font-medium text-primary bg-primary-container/10 rounded-lg hover:bg-primary-container/20 transition-colors"
          >
            <Download className="h-3.5 w-3.5" />
            Download
          </a>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <div className="rounded-lg border border-outline-variant/20 overflow-hidden bg-surface-container-low">
        <iframe
          src={fileUrl}
          title={label}
          className="w-full h-[500px]"
        />
      </div>
      <a
        href={fileUrl}
        target="_blank"
        rel="noopener noreferrer"
        className="inline-flex items-center gap-2 text-xs text-primary hover:underline"
      >
        <Download className="h-3 w-3" />
        Download {label}
      </a>
    </div>
  );
}
