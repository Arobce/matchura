import { request } from '@playwright/test'

const API_BASE = 'http://localhost:5010'

// Call services directly to bypass API gateway rate limiting during seeding
const AUTH_SERVICE = 'http://localhost:5001'
const JOB_SERVICE = 'http://localhost:5003'
const PG_CONNECTION = {
  host: process.env.PG_HOST ?? 'localhost',
  port: Number(process.env.PG_PORT ?? '5433'),
  database: process.env.PG_DATABASE ?? 'auth_db',
  user: process.env.POSTGRES_USER ?? '',
  password: process.env.POSTGRES_PASSWORD ?? '',
}

function uniqueEmail(prefix: string): string {
  return `${prefix}-${Date.now()}@e2e-test.matchura.dev`
}

interface RegisterParams {
  firstName?: string
  lastName?: string
  email?: string
  password?: string
  role?: 'Candidate' | 'Employer'
}

interface UserCredentials {
  email: string
  password: string
  token?: string
  userId?: string
}

export async function registerUser(params: RegisterParams = {}): Promise<UserCredentials> {
  const email = params.email ?? uniqueEmail('user')
  const password = params.password ?? 'Test1234!'
  const ctx = await request.newContext({ baseURL: AUTH_SERVICE })

  const res = await ctx.post('/api/auth/register', {
    data: {
      email,
      password,
      firstName: params.firstName ?? 'E2E',
      lastName: params.lastName ?? 'Tester',
      role: params.role ?? 'Candidate',
    },
  })

  if (!res.ok()) {
    const body = await res.text()
    throw new Error(`Register failed (${res.status()}): ${body}`)
  }

  await ctx.dispose()
  return { email, password }
}

export async function getVerificationCode(email: string): Promise<string> {
  // Use a child process to query postgres since Playwright tests run in Node
  const { execSync } = await import('child_process')
  const query = `SELECT \\"EmailVerificationCode\\" FROM \\"AspNetUsers\\" WHERE \\"NormalizedEmail\\" = '${email.toUpperCase()}' LIMIT 1`
  const result = execSync(
    `PGPASSWORD=${PG_CONNECTION.password} psql -h ${PG_CONNECTION.host} -p ${PG_CONNECTION.port} -U ${PG_CONNECTION.user} -d ${PG_CONNECTION.database} -t -A -c "${query}"`,
    { encoding: 'utf-8' },
  ).trim()

  if (!result) {
    throw new Error(`No verification code found for ${email}`)
  }
  return result
}

export async function verifyEmail(email: string, code: string): Promise<void> {
  const ctx = await request.newContext({ baseURL: AUTH_SERVICE })
  const res = await ctx.post('/api/auth/verify-email', {
    data: { email, code },
  })
  if (!res.ok()) {
    const body = await res.text()
    throw new Error(`Verify email failed (${res.status()}): ${body}`)
  }
  await ctx.dispose()
}

export async function loginUser(email: string, password: string): Promise<UserCredentials> {
  const ctx = await request.newContext({ baseURL: AUTH_SERVICE })
  const res = await ctx.post('/api/auth/login', {
    data: { email, password },
  })
  if (!res.ok()) {
    const body = await res.text()
    throw new Error(`Login failed (${res.status()}): ${body}`)
  }
  const data = await res.json()
  await ctx.dispose()
  return {
    email,
    password,
    token: data.token,
    userId: data.userId,
  }
}

export async function registerAndLogin(
  params: RegisterParams = {},
): Promise<UserCredentials> {
  const creds = await registerUser(params)
  const code = await getVerificationCode(creds.email)
  await verifyEmail(creds.email, code)
  const loggedIn = await loginUser(creds.email, creds.password)
  return loggedIn
}

export async function createJob(
  token: string,
  overrides: Record<string, unknown> = {},
): Promise<{ jobId: string }> {
  const ctx = await request.newContext({
    baseURL: JOB_SERVICE,
    extraHTTPHeaders: { Authorization: `Bearer ${token}` },
  })

  const data = {
    title: overrides.title ?? `E2E Test Job ${Date.now()}`,
    description: overrides.description ?? 'This is an automated E2E test job posting.',
    location: overrides.location ?? 'Remote',
    employmentType: overrides.employmentType ?? 0, // FullTime
    experienceRequired: overrides.experienceRequired ?? 2,
    salaryMin: overrides.salaryMin ?? 50000,
    salaryMax: overrides.salaryMax ?? 80000,
    skills: overrides.skills ?? [],
  }

  const res = await ctx.post('/api/jobs', { data })
  if (!res.ok()) {
    const body = await res.text()
    throw new Error(`Create job failed (${res.status()}): ${body}`)
  }
  const job = await res.json()
  await ctx.dispose()
  return { jobId: job.jobId }
}

export async function activateJob(token: string, jobId: string): Promise<void> {
  const ctx = await request.newContext({
    baseURL: JOB_SERVICE,
    extraHTTPHeaders: { Authorization: `Bearer ${token}` },
  })

  const res = await ctx.patch(`/api/jobs/${jobId}/status`, {
    data: { status: 1 }, // Active
  })
  if (!res.ok()) {
    const body = await res.text()
    throw new Error(`Activate job failed (${res.status()}): ${body}`)
  }
  await ctx.dispose()
}

export async function seedActiveJob(
  employerParams: RegisterParams = {},
): Promise<{ employer: UserCredentials; jobId: string }> {
  const employer = await registerAndLogin({
    ...employerParams,
    role: 'Employer',
  })
  const { jobId } = await createJob(employer.token!)
  await activateJob(employer.token!, jobId)
  return { employer, jobId }
}
