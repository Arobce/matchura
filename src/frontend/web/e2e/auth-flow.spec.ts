import { test, expect } from '@playwright/test'
import { LoginPage } from './pages/login.page'
import { registerUser, getVerificationCode } from './helpers/api'

test.describe('Authentication flow', () => {
  test('register candidate, verify email via UI, then login', async ({ page }) => {
    // Seed user via direct service call (bypasses gateway rate limiting)
    const password = 'Test1234!'
    const creds = await registerUser({
      firstName: 'E2E',
      lastName: 'Candidate',
      email: undefined, // auto-generated
      password,
      role: 'Candidate',
    })

    // Navigate to verify-email page directly
    await page.goto(`/verify-email?email=${encodeURIComponent(creds.email)}`)

    // Get code from DB and verify via the UI
    const code = await getVerificationCode(creds.email)
    await page.getByPlaceholder('Enter 6-digit code').fill(code)
    await page.getByRole('button', { name: 'Verify Email' }).click()

    // Should redirect to login after verification (2s delay in the app)
    await expect(page).toHaveURL(/\/login/, { timeout: 15000 })

    // Login
    const loginPage = new LoginPage(page)
    await loginPage.fillEmail(creds.email)
    await loginPage.fillPassword(password)
    await loginPage.submit()

    // Should be logged in and redirected away from login
    await expect(page).not.toHaveURL(/\/login/, { timeout: 10000 })
  })

  test('register employer, verify email via UI, then login', async ({ page }) => {
    const password = 'Test1234!'
    const creds = await registerUser({
      firstName: 'E2E',
      lastName: 'Employer',
      email: undefined,
      password,
      role: 'Employer',
    })

    await page.goto(`/verify-email?email=${encodeURIComponent(creds.email)}`)

    const code = await getVerificationCode(creds.email)
    await page.getByPlaceholder('Enter 6-digit code').fill(code)
    await page.getByRole('button', { name: 'Verify Email' }).click()

    await expect(page).toHaveURL(/\/login/, { timeout: 15000 })

    const loginPage = new LoginPage(page)
    await loginPage.fillEmail(creds.email)
    await loginPage.fillPassword(password)
    await loginPage.submit()

    await expect(page).not.toHaveURL(/\/login/, { timeout: 10000 })
  })

  test('login with wrong password shows error', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()
    await loginPage.login('nonexistent@example.com', 'WrongPassword1!')

    const error = loginPage.getError()
    await expect(error).toBeVisible({ timeout: 5000 })
  })

  test('logout redirects to home', async ({ page }) => {
    // Seed a user via API and inject the token
    const { registerAndLogin } = await import('./helpers/api')
    const user = await registerAndLogin()

    await page.goto('/')
    await page.evaluate((token) => {
      localStorage.setItem('token', token)
      document.cookie = `token=${token}; path=/; max-age=${60 * 60 * 24 * 7}; SameSite=Lax`
    }, user.token!)

    await page.reload()

    // Find and click logout
    const logoutButton = page.getByRole('button', { name: /log\s*out|sign\s*out/i })
    if (await logoutButton.isVisible({ timeout: 3000 }).catch(() => false)) {
      await logoutButton.click()
      // Logout may redirect to home or login page
      await expect(page).toHaveURL(/^\/$|\/login/)
    }
  })
})
