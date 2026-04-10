import { render, type RenderOptions } from '@testing-library/react'
import { AuthProvider } from '@/hooks/useAuth'
import type { ReactElement, ReactNode } from 'react'
import type { Job } from '@/lib/types'

function AllProviders({ children }: { children: ReactNode }) {
  return <AuthProvider>{children}</AuthProvider>
}

export function renderWithProviders(
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) {
  return render(ui, { wrapper: AllProviders, ...options })
}

export function createMockUser(overrides: Record<string, unknown> = {}) {
  const defaults = {
    userId: 'user-1',
    email: 'john@example.com',
    role: 'Candidate' as const,
  }
  const user = { ...defaults, ...overrides }

  // Build a JWT-like token from the user data
  const payload = {
    sub: user.userId,
    email: user.email,
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': user.role,
    exp: Math.floor(Date.now() / 1000) + 3600,
    iss: 'matchura',
    aud: 'matchura',
  }
  const token =
    'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.' +
    btoa(JSON.stringify(payload)) +
    '.fake-signature'

  return { user, token }
}

export function createMockJob(overrides: Partial<Job> = {}): Job {
  return {
    jobId: 'job-1',
    employerId: 'emp-1',
    title: 'Senior React Developer',
    description: 'We are looking for a senior React developer.',
    location: 'New York, NY',
    employmentType: 'FullTime',
    minSalary: 120000,
    maxSalary: 160000,
    experienceYearsMin: 5,
    experienceYearsMax: 10,
    status: 'Active',
    skills: [
      { skillId: 'skill-1', skillName: 'React', skillCategory: 'Frontend', importanceLevel: 'Required' },
      { skillId: 'skill-2', skillName: 'TypeScript', skillCategory: 'Language', importanceLevel: 'Required' },
    ],
    createdAt: '2026-03-01T00:00:00Z',
    updatedAt: '2026-03-01T00:00:00Z',
    ...overrides,
  }
}
