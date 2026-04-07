"use client";

import { useState } from "react";
import { DragDropZone } from "@/components/composed";
import { Alert } from "@/components/ui";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";
import type { ResumeUploadResponse, ResumeStatusResponse } from "@/lib/types";

interface ResumeUploaderProps {
  onUploadComplete: () => void;
}

export function ResumeUploader({ onUploadComplete }: ResumeUploaderProps) {
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

      // Poll for completion
      const poll = async () => {
        for (let i = 0; i < 30; i++) {
          await new Promise((r) => setTimeout(r, 2000));
          const status = await api.get<ResumeStatusResponse>(`/api/resumes/${result.resumeId}/status`);
          if (status.status === "Completed" || status.status === "Failed") {
            onUploadComplete();
            return;
          }
        }
        onUploadComplete();
      };
      poll();
      onUploadComplete();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Upload failed");
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="mb-8">
      <DragDropZone onFileDrop={uploadFile} uploading={uploading} />
      {error && <Alert variant="error" className="mt-4">{error}</Alert>}
    </div>
  );
}
