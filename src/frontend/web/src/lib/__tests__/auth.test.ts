import { describe, it, expect, beforeEach, vi } from 'vitest'
import { getToken, setToken, removeToken, decodeToken, getUserFromToken, isTokenExpired } from '../auth'

// Build a fake JWT with a given payload
function buildToken(payload: Record<string, unknown>): string {
  return (
    'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.' +
    btoa(JSON.stringify(payload)) +
    '.fake-signature'
  )
}

const validPayload = {
  sub: 'user-42',
  email: 'test@example.com',
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'Employer',
  exp: Math.floor(Date.now() / 1000) + 3600,
  iss: 'matchura',
  aud: 'matchura',
}

describe('auth utilities', () => {
  beforeEach(() => {
    localStorage.clear()
    // Clear any cookies
    document.cookie = 'token=; path=/; max-age=0'
  })

  describe('getToken', () => {
    it('returns null when no token is stored', () => {
      expect(getToken()).toBeNull()
    })

    it('returns the stored token', () => {
      localStorage.setItem('token', 'my-token')
      expect(getToken()).toBe('my-token')
    })
  })

  describe('setToken', () => {
    it('saves token to localStorage', () => {
      setToken('abc123')
      expect(localStorage.getItem('token')).toBe('abc123')
    })

    it('sets a cookie with the token', () => {
      setToken('abc123')
      expect(document.cookie).toContain('token=abc123')
    })
  })

  describe('removeToken', () => {
    it('removes token from localStorage', () => {
      localStorage.setItem('token', 'to-remove')
      removeToken()
      expect(localStorage.getItem('token')).toBeNull()
    })
  })

  describe('decodeToken', () => {
    it('decodes a valid JWT payload', () => {
      const token = buildToken(validPayload)
      const decoded = decodeToken(token)
      expect(decoded).toMatchObject({
        sub: 'user-42',
        email: 'test@example.com',
        iss: 'matchura',
        aud: 'matchura',
      })
    })

    it('returns null for a malformed token', () => {
      expect(decodeToken('not-a-jwt')).toBeNull()
    })

    it('returns null for an empty string', () => {
      expect(decodeToken('')).toBeNull()
    })

    it('returns null when payload is not valid JSON', () => {
      const token = 'header.' + btoa('not json') + '.signature'
      expect(decodeToken(token)).toBeNull()
    })
  })

  describe('getUserFromToken', () => {
    it('extracts user data from a valid token', () => {
      const token = buildToken(validPayload)
      const user = getUserFromToken(token)
      expect(user).toEqual({
        userId: 'user-42',
        email: 'test@example.com',
        role: 'Employer',
        exp: validPayload.exp,
      })
    })

    it('returns null for an invalid token', () => {
      expect(getUserFromToken('garbage')).toBeNull()
    })
  })

  describe('isTokenExpired', () => {
    it('returns false for a token that expires in the future', () => {
      const payload = { ...validPayload, exp: Math.floor(Date.now() / 1000) + 7200 }
      const token = buildToken(payload)
      expect(isTokenExpired(token)).toBe(false)
    })

    it('returns true for a token that already expired', () => {
      const payload = { ...validPayload, exp: Math.floor(Date.now() / 1000) - 60 }
      const token = buildToken(payload)
      expect(isTokenExpired(token)).toBe(true)
    })

    it('returns true for a token expiring right now', () => {
      const payload = { ...validPayload, exp: Math.floor(Date.now() / 1000) }
      const token = buildToken(payload)
      expect(isTokenExpired(token)).toBe(true)
    })

    it('returns true for an invalid token', () => {
      expect(isTokenExpired('invalid')).toBe(true)
    })
  })
})
