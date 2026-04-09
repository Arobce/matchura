import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { ScoreDisplay } from '../ScoreDisplay'

describe('ScoreDisplay', () => {
  it('renders the score with percentage sign', () => {
    render(<ScoreDisplay score={85} />)
    expect(screen.getByText('85%')).toBeInTheDocument()
  })

  it('renders the label when provided', () => {
    render(<ScoreDisplay score={70} label="Match Score" />)
    expect(screen.getByText('Match Score')).toBeInTheDocument()
  })

  it('does not render a label when not provided', () => {
    const { container } = render(<ScoreDisplay score={50} />)
    const spans = container.querySelectorAll('span')
    expect(spans).toHaveLength(0)
  })

  it('applies high score color band (>= 85)', () => {
    const { container } = render(<ScoreDisplay score={90} />)
    const scoreDiv = container.querySelector('.rounded-lg')
    expect(scoreDiv?.className).toContain('bg-tertiary-container')
    expect(scoreDiv?.className).not.toContain('/60')
  })

  it('applies good score color band (70-84)', () => {
    const { container } = render(<ScoreDisplay score={75} />)
    const scoreDiv = container.querySelector('.rounded-lg')
    expect(scoreDiv?.className).toContain('bg-tertiary-container/60')
  })

  it('applies medium score color band (50-69)', () => {
    const { container } = render(<ScoreDisplay score={55} />)
    const scoreDiv = container.querySelector('.rounded-lg')
    expect(scoreDiv?.className).toContain('text-warning')
  })

  it('applies low score color band (< 50)', () => {
    const { container } = render(<ScoreDisplay score={30} />)
    const scoreDiv = container.querySelector('.rounded-lg')
    expect(scoreDiv?.className).toContain('text-error')
  })

  it('applies sm size styling', () => {
    const { container } = render(<ScoreDisplay score={80} size="sm" />)
    const scoreDiv = container.querySelector('.rounded-lg')
    expect(scoreDiv?.className).toContain('w-10')
    expect(scoreDiv?.className).toContain('h-10')
  })

  it('applies lg size styling', () => {
    const { container } = render(<ScoreDisplay score={80} size="lg" />)
    const scoreDiv = container.querySelector('.rounded-lg')
    expect(scoreDiv?.className).toContain('w-20')
    expect(scoreDiv?.className).toContain('h-20')
  })

  it('applies md size by default', () => {
    const { container } = render(<ScoreDisplay score={80} />)
    const scoreDiv = container.querySelector('.rounded-lg')
    expect(scoreDiv?.className).toContain('w-14')
    expect(scoreDiv?.className).toContain('h-14')
  })
})
