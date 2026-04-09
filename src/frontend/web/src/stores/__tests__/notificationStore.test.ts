import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { useNotificationStore } from '../notificationStore'

// Mock crypto.randomUUID to return predictable IDs
let uuidCounter = 0
vi.stubGlobal('crypto', {
  randomUUID: () => `uuid-${++uuidCounter}`,
})

describe('notificationStore', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    uuidCounter = 0
    useNotificationStore.getState().clearAll()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('starts with an empty notification list', () => {
    expect(useNotificationStore.getState().notifications).toEqual([])
  })

  it('adds a notification with a generated id', () => {
    useNotificationStore.getState().addNotification({
      type: 'success',
      message: 'Job posted',
    })

    const notifications = useNotificationStore.getState().notifications
    expect(notifications).toHaveLength(1)
    expect(notifications[0]).toMatchObject({
      id: 'uuid-1',
      type: 'success',
      message: 'Job posted',
    })
  })

  it('adds multiple notifications', () => {
    useNotificationStore.getState().addNotification({ type: 'info', message: 'First' })
    useNotificationStore.getState().addNotification({ type: 'error', message: 'Second' })

    expect(useNotificationStore.getState().notifications).toHaveLength(2)
  })

  it('removes a notification by id', () => {
    useNotificationStore.getState().addNotification({
      type: 'warning',
      message: 'Warning!',
      duration: 0,
    })
    const id = useNotificationStore.getState().notifications[0].id

    useNotificationStore.getState().removeNotification(id)
    expect(useNotificationStore.getState().notifications).toHaveLength(0)
  })

  it('clears all notifications', () => {
    useNotificationStore.getState().addNotification({ type: 'info', message: 'A', duration: 0 })
    useNotificationStore.getState().addNotification({ type: 'info', message: 'B', duration: 0 })
    useNotificationStore.getState().addNotification({ type: 'info', message: 'C', duration: 0 })

    useNotificationStore.getState().clearAll()
    expect(useNotificationStore.getState().notifications).toEqual([])
  })

  it('auto-removes notification after default duration (5000ms)', () => {
    useNotificationStore.getState().addNotification({
      type: 'success',
      message: 'Temporary',
    })

    expect(useNotificationStore.getState().notifications).toHaveLength(1)

    vi.advanceTimersByTime(5000)
    expect(useNotificationStore.getState().notifications).toHaveLength(0)
  })

  it('auto-removes notification after custom duration', () => {
    useNotificationStore.getState().addNotification({
      type: 'info',
      message: 'Quick',
      duration: 2000,
    })

    vi.advanceTimersByTime(1999)
    expect(useNotificationStore.getState().notifications).toHaveLength(1)

    vi.advanceTimersByTime(1)
    expect(useNotificationStore.getState().notifications).toHaveLength(0)
  })

  it('does not auto-remove when duration is 0', () => {
    useNotificationStore.getState().addNotification({
      type: 'error',
      message: 'Persistent',
      duration: 0,
    })

    vi.advanceTimersByTime(60000)
    expect(useNotificationStore.getState().notifications).toHaveLength(1)
  })

  it('only removes the target notification, leaving others', () => {
    useNotificationStore.getState().addNotification({ type: 'info', message: 'Keep', duration: 0 })
    useNotificationStore.getState().addNotification({ type: 'error', message: 'Remove', duration: 0 })

    const notifications = useNotificationStore.getState().notifications
    const removeId = notifications.find((n) => n.message === 'Remove')!.id

    useNotificationStore.getState().removeNotification(removeId)

    const remaining = useNotificationStore.getState().notifications
    expect(remaining).toHaveLength(1)
    expect(remaining[0].message).toBe('Keep')
  })
})
