"use client";

import { useState, useEffect } from "react";
import { useApi } from "@/hooks/useApi";
import { DragDropZone } from "@/components/composed";
import { Spinner, Alert } from "@/components/ui";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";
import type { ResumeResponse, ResumeUploadResponse, ResumeStatusResponse } from "@/lib/types";
import { FileText, CheckCircle2, Plus } from "lucide-react";
import { formatRelativeDate } from "@/lib/utils";

interface ResumeSelectorProps {
  selectedId: string;
  onSelect: (resumeId: string) => void;
}

export function ResumeSelector({ selectedId, onSelect }: ResumeSelectorProps) {
  const { data: resumes, loading, refetch } = useApi<ResumeResponse[]>("/api/resumes/me");
  const [showUpload, setShowUpload] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState("");
  const { addNotification } = useNotificationStore();

  const uploadFile = async (file: File) => {
    const allowed = ["application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"];
    if (!allowed.includes(file.type)) {
      setError("Only PDF and DOCX files are supported");
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      setError("File size cannot exceed 10MB");
      return;
    }

    setError("");
    setUploading(true);
    try {
      const formData = new FormData();
      formData.append("file", file);
      const result = await api.upload<ResumeUploadResponse>("/api/resumes/upload", formData);
      addNotification({ type: "success", message: "Resume uploaded! Parsing in progress..." });

      // Poll for completion and auto-select
      for (let i = 0; i < 30; i++) {
        await new Promise((r) => setTimeout(r, 2000));
        const status = await api.get<ResumeStatusResponse>(`/api/resumes/${result.resumeId}/status`);
        if (status.status === "Completed") {
          onSelect(result.resumeId);
          refetch();
          setShowUpload(false);
          return;
        }
        if (status.status === "Failed") {
          setError("Resume parsing failed. Please try a different file.");
          refetch();
          return;
        }
      }
      // Timeout — still select it
      onSelect(result.resumeId);
      refetch();
      setShowUpload(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Upload failed");
    } finally {
      setUploading(false);
    }
  };

  if (loading) return <Spinner />;

  const completed = resumes?.filter((r) => r.status === "Completed") ?? [];

  // Auto-select if there's only one completed resume and nothing selected
  useEffect(() => {
    if (!selectedId && completed.length > 0) {
      onSelect(completed[0].resumeId);
    }
  }, [completed.length, selectedId, onSelect]);

  return (
    <div className="space-y-3">
      <label className="text-[11px] uppercase tracking-widest font-bold text-on-surface-variant">
        Select Resume *
      </label>

      {completed.length === 0 && !showUpload && (
        <p className="text-sm text-on-surface-variant">No resumes uploaded yet. Upload one to apply.</p>
      )}

      <div className="space-y-2 max-h-48 overflow-y-auto">
        {completed.map((resume) => (
          <button
            key={resume.resumeId}
            type="button"
            onClick={() => onSelect(resume.resumeId)}
            className={`w-full flex items-center gap-3 p-3 rounded-lg border transition-all text-left ${
              selectedId === resume.resumeId
                ? "border-primary bg-primary-container/10 ring-1 ring-primary/20"
                : "border-outline-variant/20 hover:border-outline-variant/40"
            }`}
          >
            <FileText className={`h-5 w-5 shrink-0 ${selectedId === resume.resumeId ? "text-primary" : "text-on-surface-variant"}`} />
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-on-surface truncate">{resume.originalFileName}</p>
              <p className="text-xs text-on-surface-variant">Uploaded {formatRelativeDate(resume.uploadedAt)}</p>
            </div>
            {selectedId === resume.resumeId && (
              <CheckCircle2 className="h-5 w-5 text-primary shrink-0" />
            )}
          </button>
        ))}
      </div>

      {error && <Alert variant="error">{error}</Alert>}

      {showUpload ? (
        <DragDropZone onFileDrop={uploadFile} uploading={uploading} className="!p-6" />
      ) : (
        <button
          type="button"
          onClick={() => setShowUpload(true)}
          className="flex items-center gap-2 text-sm text-primary font-medium hover:text-primary-dim transition-colors"
        >
          <Plus className="h-4 w-4" />
          Upload new resume
        </button>
      )}
    </div>
  );
}
