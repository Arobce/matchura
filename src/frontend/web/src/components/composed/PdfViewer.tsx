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

export function PdfViewer({ url, label, type }: PdfViewerProps) {
  const [pdfUrl, setPdfUrl] = useState<string | null>(null);
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
        setPdfUrl(result.downloadUrl);
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

  if (error || !pdfUrl) {
    return (
      <div className="flex items-center justify-center h-32 bg-surface-container-low rounded-lg">
        <p className="text-sm text-on-surface-variant">Failed to load {label.toLowerCase()}</p>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <div className="rounded-lg border border-outline-variant/20 overflow-hidden bg-surface-container-low">
        <iframe
          src={pdfUrl}
          title={label}
          className="w-full h-[500px]"
        />
      </div>
      <a
        href={pdfUrl}
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
