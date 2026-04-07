"use client";

import Link from "next/link";
import { useAuth } from "@/hooks/useAuth";
import { Button } from "@/components/ui";
import { Bell, Settings, LogOut, Menu, X } from "lucide-react";
import { useState } from "react";

export function Navbar() {
  const { user, isAuthenticated, logout } = useAuth();
  const [mobileOpen, setMobileOpen] = useState(false);

  const candidateDash = "/dashboard";
  const employerDash = "/employer/dashboard";
  const dashboardPath = user?.role === "Employer" ? employerDash : candidateDash;

  const candidateLinks = [
    { href: "/jobs", label: "Browse Jobs" },
    { href: candidateDash, label: "Dashboard" },
    { href: "/applications", label: "Applications" },
    { href: "/resumes", label: "Resumes" },
    { href: "/skill-gap", label: "Skill Gap" },
  ];

  const employerLinks = [
    { href: "/jobs", label: "Jobs" },
    { href: employerDash, label: "Dashboard" },
    { href: "/employer/analytics", label: "Analytics" },
  ];

  const navLinks = !isAuthenticated
    ? [{ href: "/jobs", label: "Jobs" }]
    : user?.role === "Employer"
    ? employerLinks
    : candidateLinks;

  return (
    <nav className="bg-surface sticky top-0 z-50 transition-colors duration-200">
      <div className="flex justify-between items-center w-full px-6 py-3 max-w-screen-2xl mx-auto">
        {/* Left: Logo + Links */}
        <div className="flex items-center gap-8">
          <Link href="/" className="text-2xl font-black tracking-tighter text-primary">
            Matchura
          </Link>
          <div className="hidden md:flex items-center gap-6">
            {navLinks.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                className="font-semibold text-sm tracking-tight text-on-surface-variant hover:text-on-surface transition-colors duration-200"
              >
                {link.label}
              </Link>
            ))}
          </div>
        </div>

        {/* Right: Actions */}
        <div className="hidden md:flex items-center gap-4">
          {isAuthenticated ? (
            <>
              {user?.role === "Employer" && (
                <Button href="/employer/postjob" size="sm">Post a Job</Button>
              )}
              <div className="flex items-center gap-2 border-l border-outline-variant/20 ml-2 pl-4">
                <button className="p-1.5 text-on-surface-variant hover:bg-surface-container-low rounded-full transition-colors">
                  <Bell className="h-5 w-5" />
                </button>
                <button className="p-1.5 text-on-surface-variant hover:bg-surface-container-low rounded-full transition-colors">
                  <Settings className="h-5 w-5" />
                </button>
                <div className="w-8 h-8 rounded-full bg-primary-container flex items-center justify-center text-on-primary text-xs font-bold ml-2 ring-2 ring-primary/10">
                  {user?.email?.charAt(0).toUpperCase()}
                </div>
                <button
                  onClick={logout}
                  className="p-1.5 text-on-surface-variant hover:text-error transition-colors"
                  title="Log out"
                >
                  <LogOut className="h-4 w-4" />
                </button>
              </div>
            </>
          ) : (
            <div className="flex items-center gap-3">
              <Link
                href="/login"
                className="text-on-surface-variant hover:text-on-surface font-medium text-sm transition-colors"
              >
                Sign In
              </Link>
              <Button href="/register" size="sm">Get Started</Button>
            </div>
          )}
        </div>

        {/* Mobile menu toggle */}
        <button
          className="md:hidden text-on-surface-variant"
          onClick={() => setMobileOpen(!mobileOpen)}
        >
          {mobileOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
        </button>
      </div>

      {/* Mobile nav */}
      {mobileOpen && (
        <div className="md:hidden px-6 pb-4 space-y-2 bg-surface">
          {navLinks.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className="block py-2 text-on-surface-variant hover:text-on-surface font-medium"
              onClick={() => setMobileOpen(false)}
            >
              {link.label}
            </Link>
          ))}
          {isAuthenticated ? (
            <button
              onClick={() => { setMobileOpen(false); logout(); }}
              className="block py-2 text-error font-medium"
            >
              Sign Out
            </button>
          ) : (
            <>
              <Link href="/login" className="block py-2 text-on-surface-variant" onClick={() => setMobileOpen(false)}>
                Sign In
              </Link>
              <Link href="/register" className="block py-2 text-primary font-bold" onClick={() => setMobileOpen(false)}>
                Get Started
              </Link>
            </>
          )}
        </div>
      )}
    </nav>
  );
}
