import { Search } from "lucide-react";

interface SearchInputProps {
  value: string;
  onChange: (value: string) => void;
  onSubmit?: () => void;
  placeholder?: string;
  className?: string;
}

export function SearchInput({ value, onChange, onSubmit, placeholder = "Search...", className }: SearchInputProps) {
  return (
    <div className={`relative flex-1 ${className ?? ""}`}>
      <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-outline" />
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onKeyDown={(e) => e.key === "Enter" && onSubmit?.()}
        placeholder={placeholder}
        className="w-full pl-10 pr-4 py-3 bg-transparent border border-outline-variant/30 rounded-lg text-on-surface placeholder:text-outline focus:ring-2 focus:ring-primary-fixed focus:border-primary outline-none transition-all duration-200"
      />
    </div>
  );
}
