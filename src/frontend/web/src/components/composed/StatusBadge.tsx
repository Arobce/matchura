import { Badge } from "@/components/ui";

const defaultStatusMap: Record<string, "primary" | "success" | "warning" | "danger" | "muted" | "accent"> = {
  Submitted: "primary",
  Reviewed: "accent",
  Shortlisted: "warning",
  Accepted: "success",
  Rejected: "danger",
  Withdrawn: "muted",
  Completed: "success",
  Failed: "danger",
  Processing: "warning",
  Pending: "primary",
};

interface StatusBadgeProps {
  status: string;
  statusMap?: Record<string, "primary" | "success" | "warning" | "danger" | "muted" | "accent">;
}

export function StatusBadge({ status, statusMap }: StatusBadgeProps) {
  const map = statusMap ?? defaultStatusMap;
  const variant = map[status] ?? "muted";

  return <Badge variant={variant}>{status}</Badge>;
}
