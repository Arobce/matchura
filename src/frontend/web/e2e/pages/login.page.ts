import { type Page, type Locator } from '@playwright/test'

export class LoginPage {
  readonly page: Page
  readonly emailInput: Locator
  readonly passwordInput: Locator
  readonly submitButton: Locator
  readonly createAccountLink: Locator

  constructor(page: Page) {
    this.page = page
    this.emailInput = page.getByPlaceholder('name@company.com')
    this.passwordInput = page.getByPlaceholder('••••••••')
    this.submitButton = page.getByRole('button', { name: 'Sign In' })
    this.createAccountLink = page.getByRole('link', { name: 'Create one' })
  }

  async goto() {
    await this.page.goto('/login')
  }

  async fillEmail(email: string) {
    await this.emailInput.fill(email)
  }

  async fillPassword(password: string) {
    await this.passwordInput.fill(password)
  }

  async submit() {
    await this.submitButton.click()
  }

  async login(email: string, password: string) {
    await this.fillEmail(email)
    await this.fillPassword(password)
    await this.submit()
  }

  getError() {
    return this.page.getByRole('alert')
  }
}
