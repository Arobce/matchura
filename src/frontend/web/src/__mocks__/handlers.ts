import { http, HttpResponse } from 'msw'
import type {
  AuthResponse,
  Job,
  JobListResponse,
  Application,
  ApplicationListResponse,
  UserNotification,
  NotificationListResponse,
  UnreadCountResponse,
} from '@/lib/types'

const API_BASE = 'http://localhost:5010'

// -- Mock Data --

export const mockUser = {
  userId: 'user-1',
  email: 'john@example.com',
  role: 'Candidate',
}

export const mockAuthResponse: AuthResponse = {
  token:
    'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.' +
    btoa(
      JSON.stringify({
        sub: 'user-1',
        email: 'john@example.com',
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'Candidate',
        exp: Math.floor(Date.now() / 1000) + 3600,
        iss: 'matchura',
        aud: 'matchura',
      })
    ) +
    '.fake-signature',
  email: 'john@example.com',
  role: 'Candidate',
  userId: 'user-1',
}

export const mockJobs: Job[] = [
  {
    jobId: 'job-1',
    employerId: 'emp-1',
    title: 'Senior React Developer',
    description: 'We are looking for a senior React developer to join our team.',
    location: 'New York, NY',
    employmentType: 'FullTime',
    minSalary: 120000,
    maxSalary: 160000,
    experienceYearsMin: 5,
    experienceYearsMax: 10,
    status: 'Active',
    skills: [
      { skillId: 'skill-1', name: 'React', category: 'Frontend', importanceLevel: 'Required' },
      { skillId: 'skill-2', name: 'TypeScript', category: 'Language', importanceLevel: 'Required' },
    ],
    createdAt: '2026-03-01T00:00:00Z',
    updatedAt: '2026-03-01T00:00:00Z',
  },
  {
    jobId: 'job-2',
    employerId: 'emp-2',
    title: 'Backend Engineer',
    description: 'Join our backend team working with .NET and microservices.',
    location: 'Remote',
    employmentType: 'FullTime',
    minSalary: 100000,
    maxSalary: 140000,
    experienceYearsMin: 3,
    experienceYearsMax: 7,
    status: 'Active',
    skills: [
      { skillId: 'skill-3', name: 'C#', category: 'Language', importanceLevel: 'Required' },
      { skillId: 'skill-4', name: '.NET', category: 'Backend', importanceLevel: 'Required' },
    ],
    createdAt: '2026-03-05T00:00:00Z',
    updatedAt: '2026-03-05T00:00:00Z',
  },
]

export const mockApplications: Application[] = [
  {
    applicationId: 'app-1',
    candidateId: 'cand-1',
    jobId: 'job-1',
    coverLetter: 'I am very interested in this position.',
    status: 'Submitted',
    appliedAt: '2026-03-10T00:00:00Z',
    updatedAt: '2026-03-10T00:00:00Z',
  },
  {
    applicationId: 'app-2',
    candidateId: 'cand-1',
    jobId: 'job-2',
    coverLetter: 'I would love to join your backend team.',
    status: 'Reviewed',
    appliedAt: '2026-03-12T00:00:00Z',
    updatedAt: '2026-03-13T00:00:00Z',
  },
]

export const mockNotifications: UserNotification[] = [
  {
    notificationId: 'notif-1',
    type: 'ApplicationUpdate',
    title: 'Application Reviewed',
    message: 'Your application for Senior React Developer has been reviewed.',
    relatedEntityId: 'app-1',
    relatedEntityType: 'Application',
    isRead: false,
    createdAt: '2026-03-15T00:00:00Z',
  },
  {
    notificationId: 'notif-2',
    type: 'JobMatch',
    title: 'New Job Match',
    message: 'A new job matching your profile has been posted.',
    relatedEntityId: 'job-2',
    relatedEntityType: 'Job',
    isRead: true,
    createdAt: '2026-03-14T00:00:00Z',
  },
]

// -- Handlers --

export const handlers = [
  // Auth
  http.post(`${API_BASE}/api/auth/login`, async () => {
    return HttpResponse.json(mockAuthResponse)
  }),

  http.post(`${API_BASE}/api/auth/register`, async () => {
    return new HttpResponse(null, { status: 204 })
  }),

  http.get(`${API_BASE}/api/auth/me`, () => {
    return HttpResponse.json(mockUser)
  }),

  // Jobs
  http.get(`${API_BASE}/api/jobs`, ({ request }) => {
    const url = new URL(request.url)
    const page = parseInt(url.searchParams.get('page') || '1', 10)
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10', 10)
    const response: JobListResponse = {
      items: mockJobs,
      totalCount: mockJobs.length,
      page,
      pageSize,
      totalPages: 1,
    }
    return HttpResponse.json(response)
  }),

  http.get(`${API_BASE}/api/jobs/:id`, ({ params }) => {
    const job = mockJobs.find((j) => j.jobId === params.id)
    if (!job) {
      return new HttpResponse(null, { status: 404 })
    }
    return HttpResponse.json(job)
  }),

  // Applications
  http.post(`${API_BASE}/api/applications`, async () => {
    return HttpResponse.json(mockApplications[0], { status: 201 })
  }),

  http.get(`${API_BASE}/api/applications`, () => {
    const response: ApplicationListResponse = {
      items: mockApplications,
      totalCount: mockApplications.length,
      page: 1,
      pageSize: 10,
      totalPages: 1,
    }
    return HttpResponse.json(response)
  }),

  // Notifications
  http.get(`${API_BASE}/api/notifications`, () => {
    const response: NotificationListResponse = {
      items: mockNotifications,
      totalCount: mockNotifications.length,
      page: 1,
      pageSize: 10,
      totalPages: 1,
    }
    return HttpResponse.json(response)
  }),

  http.get(`${API_BASE}/api/notifications/unread-count`, () => {
    const unread = mockNotifications.filter((n) => !n.isRead).length
    const response: UnreadCountResponse = { count: unread }
    return HttpResponse.json(response)
  }),
]
