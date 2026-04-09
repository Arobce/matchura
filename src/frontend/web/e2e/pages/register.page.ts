import { type Page, type Locator } from '@playwright/test'

export interface RegisterData {
  firstName: string
  lastName: string
  email: string
  password: string
  role: 'Candidate' | 'Employer'
}

export class RegisterPage {
  readonly page: Page
  readonly firstNameInput: Locator
  readonly lastNameInput: Locator
  readonly emailInput: Locator
  readonly passwordInput: Locator
  readonly submitButton: Locator

  constructor(page: Page) {
    this.page = page
    this.firstNameInput = page.getByPlaceholder('Jane')
    this.lastNameInput = page.getByPlaceholder('Doe')
    this.emailInput = page.getByPlaceholder('name@company.com')
    this.passwordInput = page.getByPlaceholder('Minimum 8 characters')
    this.submitButton = page.getByRole('button', { name: 'Create Account' })
  }

  async goto() {
    await this.page.goto('/register')
  }

  async fillForm(data: RegisterData) {
    await this.firstNameInput.fill(data.firstName)
    await this.lastNameInput.fill(data.lastName)
    await this.emailInput.fill(data.email)
    await this.passwordInput.fill(data.password)
    await this.selectRole(data.role)
  }

  async selectRole(role: 'Candidate' | 'Employer') {
    await this.page.getByRole('button', { name: role }).click()
  }

  async submit() {
    await this.submitButton.click()
  }
}
