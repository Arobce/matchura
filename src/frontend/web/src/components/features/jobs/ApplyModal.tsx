"use client";

import { useState } from "react";
import { Modal, Button, Textarea, Alert } from "@/components/ui";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";

interface ApplyModalProps {
  jobId: string;
  jobTitle: string;
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export function ApplyModal({ jobId, jobTitle, open, onClose, onSuccess }: ApplyModalProps) {
  const [coverLetter, setCoverLetter] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const { addNotification } = useNotificationStore();

  const handleSubmit = async () => {
    setLoading(true);
    setError("");
    try {
      await api.post("/api/applications", {
        jobId,
        coverLetter: coverLetter || undefined,
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
      {error && <Alert variant="error" className="mb-4">{error}</Alert>}
      <Textarea
        label="Cover Letter (optional)"
        value={coverLetter}
        onChange={(e) => setCoverLetter(e.target.value)}
        rows={5}
        placeholder="Tell the employer why you're a great fit..."
      />
      <div className="flex gap-3 justify-end mt-6">
        <Button variant="outline" onClick={onClose}>Cancel</Button>
        <Button onClick={handleSubmit} loading={loading}>Submit Application</Button>
      </div>
    </Modal>
  );
}
