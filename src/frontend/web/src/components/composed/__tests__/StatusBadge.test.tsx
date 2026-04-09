import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { StatusBadge } from '../StatusBadge'

describe('StatusBadge', () => {
  it('renders the status text', () => {
    render(<StatusBadge status="Submitted" />)
    expect(screen.getByText('Submitted')).toBeInTheDocument()
  })

  it('applies primary variant for Submitted status', () => {
    render(<StatusBadge status="Submitted" />)
    const badge = screen.getByText('Submitted')
    expect(badge.className).toContain('text-primary')
  })

  it('applies success variant for Accepted status', () => {
    render(<StatusBadge status="Accepted" />)
    const badge = screen.getByText('Accepted')
    expect(badge.className).toContain('text-green-700')
  })

  it('applies danger variant for Rejected status', () => {
    render(<StatusBadge status="Rejected" />)
    const badge = screen.getByText('Rejected')
    expect(badge.className).toContain('text-error')
  })

  it('applies warning variant for Shortlisted status', () => {
    render(<StatusBadge status="Shortlisted" />)
    const badge = screen.getByText('Shortlisted')
    expect(badge.className).toContain('text-yellow-700')
  })

  it('applies muted variant for Withdrawn status', () => {
    render(<StatusBadge status="Withdrawn" />)
    const badge = screen.getByText('Withdrawn')
    expect(badge.className).toContain('text-on-surface-variant')
  })

  it('falls back to muted for unknown status', () => {
    render(<StatusBadge status="Unknown" />)
    const badge = screen.getByText('Unknown')
    expect(badge.className).toContain('text-on-surface-variant')
  })

  it('uses custom statusMap when provided', () => {
    const customMap = { Active: 'success' as const }
    render(<StatusBadge status="Active" statusMap={customMap} />)
    const badge = screen.getByText('Active')
    expect(badge.className).toContain('text-green-700')
  })
})
