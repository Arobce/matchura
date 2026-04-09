import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Alert } from '../Alert'

describe('Alert', () => {
  it('renders children content', () => {
    render(<Alert>Something happened</Alert>)
    expect(screen.getByText('Something happened')).toBeInTheDocument()
  })

  it('applies error variant by default', () => {
    const { container } = render(<Alert>Error</Alert>)
    const alertDiv = container.firstChild as HTMLElement
    expect(alertDiv.className).toContain('text-error')
  })

  it('applies success variant styling', () => {
    const { container } = render(<Alert variant="success">Success</Alert>)
    const alertDiv = container.firstChild as HTMLElement
    expect(alertDiv.className).toContain('bg-green-100')
    expect(alertDiv.className).toContain('text-green-700')
  })

  it('applies warning variant styling', () => {
    const { container } = render(<Alert variant="warning">Warning</Alert>)
    const alertDiv = container.firstChild as HTMLElement
    expect(alertDiv.className).toContain('bg-yellow-100')
    expect(alertDiv.className).toContain('text-yellow-700')
  })

  it('applies info variant styling', () => {
    const { container } = render(<Alert variant="info">Info</Alert>)
    const alertDiv = container.firstChild as HTMLElement
    expect(alertDiv.className).toContain('text-primary')
  })

  it('renders dismiss button when onDismiss is provided', () => {
    render(<Alert onDismiss={vi.fn()}>Dismissible</Alert>)
    expect(screen.getByRole('button')).toBeInTheDocument()
  })

  it('does not render dismiss button when onDismiss is not provided', () => {
    render(<Alert>No dismiss</Alert>)
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('calls onDismiss when dismiss button is clicked', () => {
    const onDismiss = vi.fn()
    render(<Alert onDismiss={onDismiss}>Dismiss me</Alert>)
    fireEvent.click(screen.getByRole('button'))
    expect(onDismiss).toHaveBeenCalledOnce()
  })

  it('applies additional className', () => {
    const { container } = render(<Alert className="my-custom">Styled</Alert>)
    const alertDiv = container.firstChild as HTMLElement
    expect(alertDiv.className).toContain('my-custom')
  })
})
