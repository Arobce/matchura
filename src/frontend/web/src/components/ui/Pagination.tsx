import { cn } from "@/lib/utils";
import { ChevronLeft, ChevronRight } from "lucide-react";

interface PaginationProps {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  className?: string;
}

export function Pagination({ page, totalPages, onPageChange, className }: PaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <div className={cn("flex justify-center items-center gap-4 mt-10", className)}>
      <button
        onClick={() => onPageChange(page - 1)}
        disabled={page <= 1}
        className="p-2 rounded-lg border border-outline-variant/20 disabled:opacity-30 hover:bg-surface-container-low transition-colors"
      >
        <ChevronLeft className="h-5 w-5 text-on-surface" />
      </button>
      <span className="text-sm text-on-surface-variant font-medium">
        Page {page} of {totalPages}
      </span>
      <button
        onClick={() => onPageChange(page + 1)}
        disabled={page >= totalPages}
        className="p-2 rounded-lg border border-outline-variant/20 disabled:opacity-30 hover:bg-surface-container-low transition-colors"
      >
        <ChevronRight className="h-5 w-5 text-on-surface" />
      </button>
    </div>
  );
}
