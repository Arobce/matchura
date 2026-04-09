import { test, expect } from '@playwright/test'
import { seedActiveJob } from './helpers/api'

test.describe('Public pages', () => {
  test('landing page loads', async ({ page }) => {
    await page.goto('/')
    await expect(page).toHaveTitle(/Matchura/i)
  })

  test('jobs page loads and shows listings', async ({ page }) => {
    await page.goto('/jobs')
    await expect(page.getByRole('heading', { name: 'Browse Jobs' })).toBeVisible()
  })

  test('job detail page loads for a seeded job', async ({ page }) => {
    const { jobId } = await seedActiveJob()
    await page.goto(`/jobs/${jobId}`)
    await expect(page.getByRole('heading').first()).toBeVisible()
  })

  test('unauthenticated access to employer dashboard redirects to login', async ({ page }) => {
    await page.goto('/employer/jobs/create')
    await expect(page).toHaveURL(/\/login\?redirect=/)
  })

  test('unauthenticated access to candidate dashboard redirects to login', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page).toHaveURL(/\/login\?redirect=/)
  })
})
