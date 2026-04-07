import { create } from "zustand";

interface Notification {
  id: string;
  type: "success" | "error" | "warning" | "info";
  message: string;
  duration?: number;
}

interface NotificationState {
  notifications: Notification[];
  addNotification: (n: Omit<Notification, "id">) => void;
  removeNotification: (id: string) => void;
  clearAll: () => void;
}

export const useNotificationStore = create<NotificationState>((set) => ({
  notifications: [],
  addNotification: (n) => {
    const id = crypto.randomUUID();
    set((s) => ({ notifications: [...s.notifications, { ...n, id }] }));
    const duration = n.duration ?? 5000;
    if (duration > 0) {
      setTimeout(() => {
        set((s) => ({ notifications: s.notifications.filter((x) => x.id !== id) }));
      }, duration);
    }
  },
  removeNotification: (id) =>
    set((s) => ({ notifications: s.notifications.filter((x) => x.id !== id) })),
  clearAll: () => set({ notifications: [] }),
}));
