"use client";

import { useState, useRef, useEffect } from "react";
import { useNotifications } from "@/hooks/useNotifications";
import { Bell, Check, CheckCheck } from "lucide-react";
import { formatRelativeDate } from "@/lib/utils";
import Link from "next/link";

export function NotificationDropdown() {
  const { notifications, unreadCount, markAsRead, markAllAsRead } = useNotifications();
  const [open, setOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Close on outside click
  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  const getLink = (n: { relatedEntityId?: string; relatedEntityType?: string }) => {
    if (!n.relatedEntityId || !n.relatedEntityType) return null;
    if (n.relatedEntityType === "Application") return `/applications/${n.relatedEntityId}`;
    if (n.relatedEntityType === "Job") return `/jobs/${n.relatedEntityId}`;
    return null;
  };

  return (
    <div className="relative" ref={dropdownRef}>
      <button
        onClick={() => setOpen(!open)}
        className="p-1.5 text-on-surface-variant hover:bg-surface-container-low rounded-full transition-colors relative"
      >
        <Bell className="h-5 w-5" />
        {unreadCount > 0 && (
          <span className="absolute -top-0.5 -right-0.5 h-4 w-4 bg-error text-on-error text-[10px] font-bold rounded-full flex items-center justify-center">
            {unreadCount > 9 ? "9+" : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 mt-2 w-80 bg-surface-container-lowest rounded-xl editorial-shadow border border-outline-variant/15 overflow-hidden z-50">
          <div className="flex items-center justify-between px-4 py-3 border-b border-outline-variant/10">
            <h3 className="text-sm font-bold text-on-surface">Notifications</h3>
            {unreadCount > 0 && (
              <button
                onClick={markAllAsRead}
                className="text-xs text-primary hover:text-primary-dim font-medium flex items-center gap-1 transition-colors"
              >
                <CheckCheck className="h-3.5 w-3.5" />
                Mark all read
              </button>
            )}
          </div>

          <div className="max-h-80 overflow-y-auto">
            {notifications.length === 0 ? (
              <div className="px-4 py-8 text-center text-sm text-on-surface-variant">
                No notifications yet
              </div>
            ) : (
              notifications.map((n) => {
                const link = getLink(n);
                const content = (
                  <div
                    className={`px-4 py-3 border-b border-outline-variant/5 hover:bg-surface-container-low transition-colors cursor-pointer ${
                      !n.isRead ? "bg-primary-container/5" : ""
                    }`}
                    onClick={() => {
                      if (!n.isRead) markAsRead(n.notificationId);
                      if (link) setOpen(false);
                    }}
                  >
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex-1 min-w-0">
                        <p className={`text-sm ${!n.isRead ? "font-semibold text-on-surface" : "text-on-surface-variant"}`}>
                          {n.title}
                        </p>
                        <p className="text-xs text-on-surface-variant mt-0.5 line-clamp-2">{n.message}</p>
                        <p className="text-[11px] text-on-surface-variant/60 mt-1">{formatRelativeDate(n.createdAt)}</p>
                      </div>
                      {!n.isRead && (
                        <div className="h-2 w-2 rounded-full bg-primary shrink-0 mt-1.5" />
                      )}
                    </div>
                  </div>
                );

                return link ? (
                  <Link key={n.notificationId} href={link}>
                    {content}
                  </Link>
                ) : (
                  <div key={n.notificationId}>{content}</div>
                );
              })
            )}
          </div>
        </div>
      )}
    </div>
  );
}
