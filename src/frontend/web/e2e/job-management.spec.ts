import { test, expect } from '@playwright/test'
import { registerAndLogin, createJob, activateJob } from './helpers/api'

test.describe('Job management (employer)', () => {
  test('employer creates a job via API and it appears in the public job listings', async ({ page }) => {
    // Register and login as employer via API
    const employer = await registerAndLogin({ role: 'Employer' })

    const jobTitle = `E2E Test Position ${Date.now()}`

    // Create and activate the job via API (since the create page may have
    // complex server-side rendering requirements)
    const { jobId } = await createJob(employer.token!, { title: jobTitle })
    await activateJob(employer.token!, jobId)

    // Verify the job shows up in the public jobs list
    await page.goto('/jobs')
    await expect(page.getByRole('heading', { name: 'Browse Jobs' })).toBeVisible()

    // The job should be visible
    const jobEntry = page.getByText(jobTitle)
    await expect(jobEntry).toBeVisible({ timeout: 10000 })
  })

  test('employer can navigate to create job page when authenticated', async ({ page }) => {
    const employer = await registerAndLogin({ role: 'Employer' })

    // Set auth token via cookie before navigating
    await page.context().addCookies([
      {
        name: 'token',
        value: employer.token!,
        domain: 'localhost',
        path: '/',
      },
    ])

    await page.goto('/employer/jobs/create')

    // Should not be redirected to login
    await expect(page).not.toHaveURL(/\/login/, { timeout: 5000 })
  })
})
