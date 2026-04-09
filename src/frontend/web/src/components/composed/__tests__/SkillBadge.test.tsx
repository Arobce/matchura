import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { SkillBadge } from '../SkillBadge'

describe('SkillBadge', () => {
  it('renders the skill name', () => {
    render(<SkillBadge name="React" />)
    expect(screen.getByText('React')).toBeInTheDocument()
  })

  it('applies Required importance styling', () => {
    const { container } = render(<SkillBadge name="React" importanceLevel="Required" />)
    const badge = container.firstChild as HTMLElement
    expect(badge.className).toContain('text-primary')
    expect(badge.className).toContain('border-primary/20')
  })

  it('applies Preferred importance styling', () => {
    const { container } = render(<SkillBadge name="Node" importanceLevel="Preferred" />)
    const badge = container.firstChild as HTMLElement
    expect(badge.className).toContain('text-on-tertiary-container')
  })

  it('applies default styling for no importance level', () => {
    const { container } = render(<SkillBadge name="CSS" />)
    const badge = container.firstChild as HTMLElement
    expect(badge.className).toContain('bg-surface-container')
  })

  it('shows proficiency level when provided', () => {
    render(<SkillBadge name="TypeScript" proficiencyLevel="Advanced" />)
    expect(screen.getByText('(Advanced)')).toBeInTheDocument()
  })

  it('shows importance level when no proficiency is provided', () => {
    render(<SkillBadge name="TypeScript" importanceLevel="Required" />)
    expect(screen.getByText('(Required)')).toBeInTheDocument()
  })

  it('shows proficiency over importance when both provided', () => {
    render(<SkillBadge name="TypeScript" importanceLevel="Required" proficiencyLevel="Expert" />)
    expect(screen.getByText('(Expert)')).toBeInTheDocument()
    expect(screen.queryByText('(Required)')).not.toBeInTheDocument()
  })

  it('applies additional className', () => {
    const { container } = render(<SkillBadge name="Go" className="extra" />)
    const badge = container.firstChild as HTMLElement
    expect(badge.className).toContain('extra')
  })
})
