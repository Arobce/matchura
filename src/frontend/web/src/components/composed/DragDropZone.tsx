"use client";

import { useCallback, useState } from "react";
import { Upload, Loader2 } from "lucide-react";

interface DragDropZoneProps {
  onFileDrop: (file: File) => void;
  accept?: string[];
  maxSizeMB?: number;
  uploading?: boolean;
  className?: string;
}

export function DragDropZone({
  onFileDrop,
  accept = [".pdf", ".docx"],
  maxSizeMB = 10,
  uploading = false,
  className,
}: DragDropZoneProps) {
  const [dragOver, setDragOver] = useState(false);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setDragOver(false);
      const file = e.dataTransfer.files[0];
      if (file) onFileDrop(file);
    },
    [onFileDrop]
  );

  return (
    <div
      onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
      onDragLeave={() => setDragOver(false)}
      onDrop={handleDrop}
      className={`border-2 border-dashed rounded-xl p-10 text-center transition-colors ${
        dragOver ? "border-primary bg-primary-container/5" : "border-outline-variant/30"
      } ${className ?? ""}`}
    >
      <Upload className="h-10 w-10 text-on-surface-variant mx-auto mb-4" />
      <p className="text-on-surface font-medium mb-1">Drag and drop your file here</p>
      <p className="text-sm text-on-surface-variant mb-4">
        {accept.join(", ").toUpperCase()}, max {maxSizeMB}MB
      </p>
      <label className="cursor-pointer bg-gradient-to-br from-primary to-primary-container text-on-primary px-6 py-2.5 rounded-lg font-medium hover:shadow-md active:scale-[0.98] transition-all inline-flex items-center gap-2">
        {uploading && <Loader2 className="h-4 w-4 animate-spin" />}
        {uploading ? "Uploading..." : "Choose File"}
        <input
          type="file"
          accept={accept.join(",")}
          className="hidden"
          onChange={(e) => e.target.files?.[0] && onFileDrop(e.target.files[0])}
          disabled={uploading}
        />
      </label>
    </div>
  );
}
