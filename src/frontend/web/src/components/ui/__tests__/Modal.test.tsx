import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Modal } from '../Modal'

describe('Modal', () => {
  it('renders nothing when open is false', () => {
    render(
      <Modal open={false} onClose={vi.fn()} title="Test">
        <p>Content</p>
      </Modal>
    )
    expect(screen.queryByText('Test')).not.toBeInTheDocument()
  })

  it('renders title and children when open', () => {
    render(
      <Modal open={true} onClose={vi.fn()} title="My Modal">
        <p>Modal body</p>
      </Modal>
    )
    expect(screen.getByText('My Modal')).toBeInTheDocument()
    expect(screen.getByText('Modal body')).toBeInTheDocument()
  })

  it('calls onClose when close button is clicked', () => {
    const onClose = vi.fn()
    render(
      <Modal open={true} onClose={onClose} title="Close Test">
        <p>Body</p>
      </Modal>
    )
    // The close button contains the X icon; find the button inside the header area
    const buttons = screen.getAllByRole('button')
    fireEvent.click(buttons[0])
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onClose when backdrop is clicked', () => {
    const onClose = vi.fn()
    const { container } = render(
      <Modal open={true} onClose={onClose} title="Backdrop Test">
        <p>Body</p>
      </Modal>
    )
    // The backdrop is the div with bg-inverse-surface class
    const backdrop = container.querySelector('.backdrop-blur-sm')
    expect(backdrop).not.toBeNull()
    fireEvent.click(backdrop!)
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onClose when Escape key is pressed', () => {
    const onClose = vi.fn()
    render(
      <Modal open={true} onClose={onClose} title="Escape Test">
        <p>Body</p>
      </Modal>
    )
    fireEvent.keyDown(document, { key: 'Escape' })
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('sets body overflow to hidden when open', () => {
    render(
      <Modal open={true} onClose={vi.fn()} title="Overflow">
        <p>Body</p>
      </Modal>
    )
    expect(document.body.style.overflow).toBe('hidden')
  })
})
