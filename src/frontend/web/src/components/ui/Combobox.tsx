"use client";

import { useState, useRef, useEffect } from "react";
import { cn } from "@/lib/utils";

interface ComboboxOption {
  value: string;
  label: string;
}

interface ComboboxProps {
  label?: string;
  options: ComboboxOption[];
  value: string;
  onChange: (value: string, label: string) => void;
  onCreateNew?: (name: string) => void;
  placeholder?: string;
  error?: string;
}

export function Combobox({ label, options, value, onChange, onCreateNew, placeholder, error }: ComboboxProps) {
  const [query, setQuery] = useState("");
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const inputId = label?.toLowerCase().replace(/\s+/g, "-");

  // Set display text when value changes externally
  useEffect(() => {
    if (value) {
      const opt = options.find((o) => o.value === value);
      if (opt) setQuery(opt.label);
    } else {
      setQuery("");
    }
  }, [value, options]);

  // Close on click outside
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  const filtered = options.filter((o) =>
    o.label.toLowerCase().includes(query.toLowerCase())
  );

  const exactMatch = options.some((o) => o.label.toLowerCase() === query.toLowerCase());
  const showCreate = onCreateNew && query.trim().length > 0 && !exactMatch;

  return (
    <div ref={containerRef} className="relative space-y-1.5">
      {label && (
        <label
          htmlFor={inputId}
          className="text-[11px] uppercase tracking-widest font-bold text-on-surface-variant"
        >
          {label}
        </label>
      )}
      <input
        ref={inputRef}
        id={inputId}
        type="text"
        value={query}
        onChange={(e) => {
          setQuery(e.target.value);
          setOpen(true);
          if (!e.target.value) onChange("", "");
        }}
        onFocus={() => setOpen(true)}
        placeholder={placeholder}
        className={cn(
          "w-full px-4 py-3 bg-transparent border border-outline-variant/30 rounded-lg text-on-surface placeholder:text-outline",
          "focus:ring-2 focus:ring-primary-fixed focus:border-primary outline-none transition-all duration-200",
          error && "border-error focus:ring-error-container"
        )}
        autoComplete="off"
      />
      {open && (filtered.length > 0 || showCreate) && (
        <div className="absolute z-50 top-full left-0 right-0 mt-1 bg-surface-container-lowest border border-outline-variant/20 rounded-lg shadow-lg max-h-60 overflow-y-auto">
          {filtered.map((opt) => (
            <button
              key={opt.value}
              type="button"
              onClick={() => {
                onChange(opt.value, opt.label);
                setQuery(opt.label);
                setOpen(false);
              }}
              className={cn(
                "w-full text-left px-4 py-2.5 text-sm hover:bg-surface-container-low transition-colors",
                opt.value === value ? "text-primary font-semibold" : "text-on-surface"
              )}
            >
              {opt.label}
            </button>
          ))}
          {showCreate && (
            <button
              type="button"
              onClick={() => {
                onCreateNew(query.trim());
                setOpen(false);
              }}
              className="w-full text-left px-4 py-2.5 text-sm text-primary font-semibold hover:bg-surface-container-low transition-colors border-t border-outline-variant/10"
            >
              + Create &quot;{query.trim()}&quot;
            </button>
          )}
        </div>
      )}
      {error && <p className="text-xs text-error font-medium">{error}</p>}
    </div>
  );
}
