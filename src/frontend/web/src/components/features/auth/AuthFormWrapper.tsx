import type { ReactNode } from "react";
import Link from "next/link";

interface AuthFormWrapperProps {
  title: string;
  subtitle: string;
  children: ReactNode;
  footer?: ReactNode;
}

export function AuthFormWrapper({ title, subtitle, children, footer }: AuthFormWrapperProps) {
  return (
    <div className="min-h-screen bg-surface flex flex-col">
      <main className="flex-grow flex items-center justify-center px-4 py-12">
        <div className="w-full max-w-md bg-surface-container-lowest rounded-lg editorial-shadow p-8 md:p-10 border border-outline-variant/15">
          <div className="flex flex-col items-center mb-10">
            <Link href="/" className="mb-6 text-2xl font-black tracking-tighter text-primary">
              Matchura
            </Link>
            <h1 className="text-2xl font-bold text-on-surface tracking-tight mb-2">{title}</h1>
            <p className="text-on-surface-variant text-sm font-medium">{subtitle}</p>
          </div>
          {children}
          {footer}
        </div>
      </main>
      <footer className="py-8 px-6">
        <div className="max-w-7xl mx-auto flex flex-col md:flex-row justify-between items-center gap-4">
          <div className="text-lg font-black tracking-tighter text-on-surface">Matchura</div>
          <div className="flex gap-8">
            <span className="text-[10px] uppercase tracking-widest font-bold text-outline">Privacy Policy</span>
            <span className="text-[10px] uppercase tracking-widest font-bold text-outline">Terms of Service</span>
            <span className="text-[10px] uppercase tracking-widest font-bold text-outline">Support</span>
          </div>
        </div>
      </footer>
    </div>
  );
}
