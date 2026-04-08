"use client";

import { useState } from "react";
import { Modal, Button, Textarea, Alert } from "@/components/ui";
import { ResumeSelector } from "@/components/features/resumes";
import { DragDropZone } from "@/components/composed";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";
import type { DocumentUploadResponse } from "@/lib/types";
import { FileText, Type, Upload } from "lucide-react";

interface ApplyModalProps {
  jobId: string;
  jobTitle: string;
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export function ApplyModal({ jobId, jobTitle, open, onClose, onSuccess }: ApplyModalProps) {
  const [selectedResumeId, setSelectedResumeId] = useState("");
  const [coverLetterMode, setCoverLetterMode] = useState<"write" | "upload">("write");
  const [coverLetter, setCoverLetter] = useState("");
  const [coverLetterUrl, setCoverLetterUrl] = useState("");
  const [coverLetterFileName, setCoverLetterFileName] = useState("");
  const [uploading, setUploading] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const { addNotification } = useNotificationStore();

  const handleCoverLetterUpload = async (file: File) => {
    if (file.type !== "application/pdf") {
      setError("Only PDF files are supported for cover letters");
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      setError("Cover letter file cannot exceed 5MB");
      return;
    }

    setError("");
    setUploading(true);
    try {
      const formData = new FormData();
      formData.append("file", file);
      const result = await api.upload<DocumentUploadResponse>("/api/documents/upload", formData);
      setCoverLetterUrl(result.fileUrl);
      setCoverLetter(result.extractedText);
      setCoverLetterFileName(file.name);
      addNotification({ type: "success", message: "Cover letter uploaded" });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Cover letter upload failed");
    } finally {
      setUploading(false);
    }
  };

  const handleSubmit = async () => {
    if (!selectedResumeId) {
      setError("Please select a resume");
      return;
    }

    setLoading(true);
    setError("");
    try {
      await api.post("/api/applications", {
        jobId,
        resumeUrl: selectedResumeId,
        coverLetter: coverLetter || undefined,
        coverLetterUrl: coverLetterUrl || undefined,
      });
      addNotification({ type: "success", message: "Application submitted successfully!" });
      onSuccess();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to apply");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal open={open} onClose={onClose} title={`Apply to ${jobTitle}`}>
      <div className="space-y-6">
        {error && <Alert variant="error">{error}</Alert>}

        {/* Resume Selection */}
        <ResumeSelector selectedId={selectedResumeId} onSelect={setSelectedResumeId} />

        {/* Cover Letter */}
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <label className="text-[11px] uppercase tracking-widest font-bold text-on-surface-variant">
              Cover Letter (optional)
            </label>
            <div className="flex gap-1 bg-surface-container-low rounded-lg p-0.5">
              <button
                type="button"
                onClick={() => setCoverLetterMode("write")}
                className={`flex items-center gap-1 px-2.5 py-1 rounded-md text-xs font-medium transition-colors ${
                  coverLetterMode === "write"
                    ? "bg-surface text-on-surface shadow-sm"
                    : "text-on-surface-variant hover:text-on-surface"
                }`}
              >
                <Type className="h-3 w-3" /> Write
              </button>
              <button
                type="button"
                onClick={() => setCoverLetterMode("upload")}
                className={`flex items-center gap-1 px-2.5 py-1 rounded-md text-xs font-medium transition-colors ${
                  coverLetterMode === "upload"
                    ? "bg-surface text-on-surface shadow-sm"
                    : "text-on-surface-variant hover:text-on-surface"
                }`}
              >
                <Upload className="h-3 w-3" /> Upload PDF
              </button>
            </div>
          </div>

          {coverLetterMode === "write" ? (
            <Textarea
              value={coverLetter}
              onChange={(e) => setCoverLetter(e.target.value)}
              rows={5}
              placeholder="Tell the employer why you're a great fit..."
            />
          ) : coverLetterUrl ? (
            <div className="flex items-center gap-3 p-3 rounded-lg border border-outline-variant/20 bg-surface-container-lowest">
              <FileText className="h-5 w-5 text-primary" />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-on-surface truncate">{coverLetterFileName}</p>
                <p className="text-xs text-on-surface-variant">Uploaded successfully</p>
              </div>
              <button
                type="button"
                onClick={() => { setCoverLetterUrl(""); setCoverLetterFileName(""); setCoverLetter(""); }}
                className="text-xs text-error font-medium"
              >
                Remove
              </button>
            </div>
          ) : (
            <DragDropZone
              onFileDrop={handleCoverLetterUpload}
              accept={[".pdf"]}
              maxSizeMB={5}
              uploading={uploading}
              className="!p-6"
            />
          )}
        </div>

        <div className="flex gap-3 justify-end pt-2">
          <Button variant="outline" onClick={onClose}>Cancel</Button>
          <Button onClick={handleSubmit} loading={loading} disabled={!selectedResumeId}>
            Submit Application
          </Button>
        </div>
      </div>
    </Modal>
  );
}
