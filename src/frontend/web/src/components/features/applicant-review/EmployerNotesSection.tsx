"use client";

import { useState } from "react";
import { Button, Textarea } from "@/components/ui";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";

interface EmployerNotesSectionProps {
  applicationId: string;
  initialNotes: string;
}

export function EmployerNotesSection({ applicationId, initialNotes }: EmployerNotesSectionProps) {
  const [notes, setNotes] = useState(initialNotes);
  const [saving, setSaving] = useState(false);
  const { addNotification } = useNotificationStore();
  const isDirty = notes !== initialNotes;

  const handleSave = async () => {
    setSaving(true);
    try {
      await api.patch(`/api/applications/${applicationId}/notes`, { employerNotes: notes });
      addNotification({ type: "success", message: "Notes saved" });
    } catch {
      addNotification({ type: "error", message: "Failed to save notes" });
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="bg-surface-container-lowest rounded-xl editorial-shadow p-6 space-y-4">
      <h3 className="text-lg font-bold text-on-surface">Employer Notes</h3>
      <Textarea
        value={notes}
        onChange={(e) => setNotes(e.target.value)}
        placeholder="Add private notes about this candidate..."
        rows={4}
      />
      <div className="flex justify-end">
        <Button onClick={handleSave} loading={saving} disabled={!isDirty} size="sm">
          Save Notes
        </Button>
      </div>
    </div>
  );
}
