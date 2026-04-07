import { create } from "zustand";

interface JobFilterState {
  search: string;
  location: string;
  employmentType: string;
  page: number;
  pageSize: number;
  setSearch: (s: string) => void;
  setLocation: (l: string) => void;
  setEmploymentType: (t: string) => void;
  setPage: (p: number) => void;
  resetFilters: () => void;
  getQueryString: () => string;
}

const defaults = {
  search: "",
  location: "",
  employmentType: "",
  page: 1,
  pageSize: 12,
};

export const useJobFilterStore = create<JobFilterState>((set, get) => ({
  ...defaults,
  setSearch: (search) => set({ search, page: 1 }),
  setLocation: (location) => set({ location, page: 1 }),
  setEmploymentType: (employmentType) => set({ employmentType, page: 1 }),
  setPage: (page) => set({ page }),
  resetFilters: () => set(defaults),
  getQueryString: () => {
    const s = get();
    const params = new URLSearchParams();
    if (s.search) params.set("search", s.search);
    if (s.location) params.set("location", s.location);
    if (s.employmentType) params.set("employmentType", s.employmentType);
    params.set("page", String(s.page));
    params.set("pageSize", String(s.pageSize));
    return params.toString();
  },
}));
