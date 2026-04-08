"use client";

import { useState, useEffect, useCallback, useRef } from "react";
import { HubConnectionBuilder, HubConnection, LogLevel } from "@microsoft/signalr";
import { api } from "@/lib/api";
import type { UserNotification, UnreadCountResponse, NotificationListResponse } from "@/lib/types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5010";

export function useNotifications() {
  const [notifications, setNotifications] = useState<UserNotification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const connectionRef = useRef<HubConnection | null>(null);

  const fetchUnreadCount = useCallback(async () => {
    try {
      const result = await api.get<UnreadCountResponse>("/api/notifications/unread-count");
      setUnreadCount(result.count);
    } catch {
      // silently fail — user may not be authenticated
    }
  }, []);

  const fetchNotifications = useCallback(async () => {
    try {
      const result = await api.get<NotificationListResponse>("/api/notifications?pageSize=10");
      setNotifications(result.items);
      setUnreadCount(result.items.filter((n) => !n.isRead).length);
    } catch {
      // silently fail
    }
  }, []);

  const markAsRead = useCallback(async (id: string) => {
    try {
      await api.patch(`/api/notifications/${id}/read`);
      setNotifications((prev) =>
        prev.map((n) => (n.notificationId === id ? { ...n, isRead: true } : n))
      );
      setUnreadCount((prev) => Math.max(0, prev - 1));
    } catch {
      // silently fail
    }
  }, []);

  const markAllAsRead = useCallback(async () => {
    try {
      await api.post("/api/notifications/mark-all-read");
      setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch {
      // silently fail
    }
  }, []);

  useEffect(() => {
    const token = typeof window !== "undefined" ? localStorage.getItem("token") : null;
    if (!token) return;

    // Fetch initial data
    fetchNotifications();

    // Connect SignalR
    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/notifications-hub`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on("ReceiveNotification", (notification: UserNotification) => {
      setNotifications((prev) => [notification, ...prev].slice(0, 20));
      setUnreadCount((prev) => prev + 1);
    });

    connection
      .start()
      .then(() => {
        connectionRef.current = connection;
      })
      .catch(() => {
        // Fallback: poll every 30 seconds
        const interval = setInterval(fetchUnreadCount, 30000);
        return () => clearInterval(interval);
      });

    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, [fetchNotifications, fetchUnreadCount]);

  return { notifications, unreadCount, markAsRead, markAllAsRead, refetch: fetchNotifications };
}
