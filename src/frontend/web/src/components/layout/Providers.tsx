"use client";

import { AuthProvider } from "@/hooks/useAuth";
import { useNotificationStore } from "@/stores";
import { Alert } from "@/components/ui";
import type { ReactNode } from "react";

function NotificationToaster() {
  const { notifications, removeNotification } = useNotificationStore();
  if (notifications.length === 0) return null;

  return (
    <div className="fixed top-4 right-4 z-[100] space-y-2 max-w-sm w-full">
      {notifications.map((n) => (
        <Alert
          key={n.id}
          variant={n.type}
          onDismiss={() => removeNotification(n.id)}
          className="animate-in slide-in-from-right"
        >
          {n.message}
        </Alert>
      ))}
    </div>
  );
}

export function Providers({ children }: { children: ReactNode }) {
  return (
    <AuthProvider>
      {children}
      <NotificationToaster />
    </AuthProvider>
  );
}
