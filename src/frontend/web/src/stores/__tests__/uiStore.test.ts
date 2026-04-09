import { describe, it, expect, beforeEach } from 'vitest'
import { useUIStore } from '../uiStore'

describe('uiStore', () => {
  beforeEach(() => {
    useUIStore.getState().closeModal()
    // Reset sidebar to closed
    if (useUIStore.getState().sidebarOpen) {
      useUIStore.getState().toggleSidebar()
    }
  })

  it('starts with sidebar closed', () => {
    expect(useUIStore.getState().sidebarOpen).toBe(false)
  })

  it('starts with no active modal', () => {
    expect(useUIStore.getState().activeModal).toBeNull()
    expect(useUIStore.getState().modalData).toEqual({})
  })

  it('toggles sidebar open', () => {
    useUIStore.getState().toggleSidebar()
    expect(useUIStore.getState().sidebarOpen).toBe(true)
  })

  it('toggles sidebar closed', () => {
    useUIStore.getState().toggleSidebar()
    useUIStore.getState().toggleSidebar()
    expect(useUIStore.getState().sidebarOpen).toBe(false)
  })

  it('opens a modal with id', () => {
    useUIStore.getState().openModal('confirm-delete')
    expect(useUIStore.getState().activeModal).toBe('confirm-delete')
    expect(useUIStore.getState().modalData).toEqual({})
  })

  it('opens a modal with id and data', () => {
    useUIStore.getState().openModal('edit-job', { jobId: 'job-1', title: 'Dev' })
    expect(useUIStore.getState().activeModal).toBe('edit-job')
    expect(useUIStore.getState().modalData).toEqual({ jobId: 'job-1', title: 'Dev' })
  })

  it('closes modal and clears data', () => {
    useUIStore.getState().openModal('settings', { theme: 'dark' })
    useUIStore.getState().closeModal()
    expect(useUIStore.getState().activeModal).toBeNull()
    expect(useUIStore.getState().modalData).toEqual({})
  })

  it('replaces modal when opening a new one', () => {
    useUIStore.getState().openModal('modal-a', { key: 'a' })
    useUIStore.getState().openModal('modal-b', { key: 'b' })
    expect(useUIStore.getState().activeModal).toBe('modal-b')
    expect(useUIStore.getState().modalData).toEqual({ key: 'b' })
  })

  it('sidebar state is independent of modal state', () => {
    useUIStore.getState().toggleSidebar()
    useUIStore.getState().openModal('test')
    expect(useUIStore.getState().sidebarOpen).toBe(true)
    expect(useUIStore.getState().activeModal).toBe('test')

    useUIStore.getState().closeModal()
    expect(useUIStore.getState().sidebarOpen).toBe(true)
  })
})
