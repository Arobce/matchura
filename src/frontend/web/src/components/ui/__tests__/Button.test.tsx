import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Button } from '../Button'

vi.mock('next/link', () => ({
  default: ({ href, children, className }: { href: string; children: React.ReactNode; className?: string }) => (
    <a href={href} className={className}>{children}</a>
  ),
}))

describe('Button', () => {
  it('renders children text', () => {
    render(<Button>Click me</Button>)
    expect(screen.getByRole('button', { name: 'Click me' })).toBeInTheDocument()
  })

  it('applies primary variant styles by default', () => {
    render(<Button>Primary</Button>)
    const button = screen.getByRole('button')
    expect(button.className).toContain('from-primary')
  })

  it('applies danger variant styles', () => {
    render(<Button variant="danger">Delete</Button>)
    const button = screen.getByRole('button')
    expect(button.className).toContain('bg-error')
  })

  it('applies outline variant styles', () => {
    render(<Button variant="outline">Outline</Button>)
    const button = screen.getByRole('button')
    expect(button.className).toContain('border')
  })

  it('applies ghost variant styles', () => {
    render(<Button variant="ghost">Ghost</Button>)
    const button = screen.getByRole('button')
    expect(button.className).toContain('hover:bg-surface-container-low')
  })

  it('applies size styles', () => {
    render(<Button size="lg">Large</Button>)
    const button = screen.getByRole('button')
    expect(button.className).toContain('px-10')
  })

  it('calls onClick handler when clicked', () => {
    const handleClick = vi.fn()
    render(<Button onClick={handleClick}>Click</Button>)
    fireEvent.click(screen.getByRole('button'))
    expect(handleClick).toHaveBeenCalledOnce()
  })

  it('is disabled when disabled prop is true', () => {
    render(<Button disabled>Disabled</Button>)
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('is disabled when loading prop is true', () => {
    render(<Button loading>Loading</Button>)
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('applies opacity when disabled', () => {
    render(<Button disabled>Disabled</Button>)
    expect(screen.getByRole('button').className).toContain('opacity-50')
  })

  it('applies fullWidth class', () => {
    render(<Button fullWidth>Full</Button>)
    expect(screen.getByRole('button').className).toContain('w-full')
  })

  it('renders as a link when href is provided', () => {
    render(<Button href="/test">Link</Button>)
    const link = screen.getByRole('link', { name: 'Link' })
    expect(link).toBeInTheDocument()
    expect(link).toHaveAttribute('href', '/test')
  })

  it('does not call onClick when disabled', () => {
    const handleClick = vi.fn()
    render(<Button disabled onClick={handleClick}>No click</Button>)
    fireEvent.click(screen.getByRole('button'))
    expect(handleClick).not.toHaveBeenCalled()
  })
})
