import { type Page, type Locator } from '@playwright/test'

export class JobsPage {
  readonly page: Page
  readonly heading: Locator

  constructor(page: Page) {
    this.page = page
    this.heading = page.getByRole('heading', { name: 'Browse Jobs' })
  }

  async goto() {
    await this.page.goto('/jobs')
  }

  getJobCards() {
    return this.page.locator('[data-testid="job-card"]')
  }

  getResultsCount() {
    return this.page.getByText(/\d+ job\(s\) found/)
  }

  async clickJob(index: number) {
    const cards = this.getJobCards()
    await cards.nth(index).click()
  }
}
