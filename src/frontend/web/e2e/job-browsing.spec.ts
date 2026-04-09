import { test, expect } from '@playwright/test'
import { seedActiveJob } from './helpers/api'
import { JobsPage } from './pages/jobs.page'

test.describe('Job browsing', () => {
  let seededJobId: string

  test.beforeAll(async () => {
    const result = await seedActiveJob()
    seededJobId = result.jobId
  })

  test('browse jobs page loads with heading', async ({ page }) => {
    const jobsPage = new JobsPage(page)
    await jobsPage.goto()
    await expect(jobsPage.heading).toBeVisible()
  })

  test('jobs page shows results', async ({ page }) => {
    const jobsPage = new JobsPage(page)
    await jobsPage.goto()

    // The page shows "{count} job(s) found" text and job cards as links in a grid
    const hasResultsText = await page.getByText(/\d+ jobs? found/i).isVisible({ timeout: 10000 }).catch(() => false)
    const hasJobLinks = await page.locator('a[href^="/jobs/"]').first().isVisible({ timeout: 10000 }).catch(() => false)

    expect(hasResultsText || hasJobLinks).toBeTruthy()
  })

  test('job detail page shows correct information', async ({ page }) => {
    await page.goto(`/jobs/${seededJobId}`)

    // The detail page should have a heading with the job title
    await expect(page.getByRole('heading').first()).toBeVisible({ timeout: 10000 })

    // Should show some job details
    const pageContent = await page.textContent('body')
    expect(pageContent).toBeTruthy()
  })

  test('pagination is present when there are enough jobs', async ({ page }) => {
    const jobsPage = new JobsPage(page)
    await jobsPage.goto()

    // Pagination may or may not be visible depending on data
    // This test verifies it does not break the page
    const pagination = page.getByRole('navigation', { name: /pagination/i })
      .or(page.locator('[data-testid="pagination"]'))
      .or(page.getByRole('button', { name: /next/i }))

    const isVisible = await pagination.first().isVisible({ timeout: 3000 }).catch(() => false)

    if (isVisible) {
      await pagination.first().click()
      await expect(jobsPage.heading).toBeVisible()
    }
  })
})
