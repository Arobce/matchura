import { describe, it, expect, beforeEach } from 'vitest'
import { useJobFilterStore } from '../jobFilterStore'

describe('jobFilterStore', () => {
  beforeEach(() => {
    useJobFilterStore.getState().resetFilters()
  })

  it('starts with default values', () => {
    const state = useJobFilterStore.getState()
    expect(state.search).toBe('')
    expect(state.location).toBe('')
    expect(state.employmentType).toBe('')
    expect(state.page).toBe(1)
    expect(state.pageSize).toBe(12)
  })

  it('sets search and resets page to 1', () => {
    useJobFilterStore.getState().setPage(3)
    useJobFilterStore.getState().setSearch('developer')
    const state = useJobFilterStore.getState()
    expect(state.search).toBe('developer')
    expect(state.page).toBe(1)
  })

  it('sets location and resets page to 1', () => {
    useJobFilterStore.getState().setPage(5)
    useJobFilterStore.getState().setLocation('Remote')
    const state = useJobFilterStore.getState()
    expect(state.location).toBe('Remote')
    expect(state.page).toBe(1)
  })

  it('sets employment type and resets page to 1', () => {
    useJobFilterStore.getState().setPage(2)
    useJobFilterStore.getState().setEmploymentType('FullTime')
    const state = useJobFilterStore.getState()
    expect(state.employmentType).toBe('FullTime')
    expect(state.page).toBe(1)
  })

  it('sets page without affecting filters', () => {
    useJobFilterStore.getState().setSearch('engineer')
    useJobFilterStore.getState().setPage(4)
    const state = useJobFilterStore.getState()
    expect(state.page).toBe(4)
    expect(state.search).toBe('engineer')
  })

  it('resets all filters to defaults', () => {
    useJobFilterStore.getState().setSearch('test')
    useJobFilterStore.getState().setLocation('NYC')
    useJobFilterStore.getState().setEmploymentType('PartTime')
    useJobFilterStore.getState().setPage(3)
    useJobFilterStore.getState().resetFilters()

    const state = useJobFilterStore.getState()
    expect(state.search).toBe('')
    expect(state.location).toBe('')
    expect(state.employmentType).toBe('')
    expect(state.page).toBe(1)
    expect(state.pageSize).toBe(12)
  })

  it('builds query string with all filters', () => {
    useJobFilterStore.getState().setSearch('react')
    useJobFilterStore.getState().setLocation('Remote')
    useJobFilterStore.getState().setEmploymentType('Contract')

    const qs = useJobFilterStore.getState().getQueryString()
    const params = new URLSearchParams(qs)
    expect(params.get('search')).toBe('react')
    expect(params.get('location')).toBe('Remote')
    expect(params.get('employmentType')).toBe('Contract')
    expect(params.get('page')).toBe('1')
    expect(params.get('pageSize')).toBe('12')
  })

  it('omits empty filters from query string', () => {
    const qs = useJobFilterStore.getState().getQueryString()
    const params = new URLSearchParams(qs)
    expect(params.has('search')).toBe(false)
    expect(params.has('location')).toBe(false)
    expect(params.has('employmentType')).toBe(false)
    expect(params.get('page')).toBe('1')
    expect(params.get('pageSize')).toBe('12')
  })

  it('includes only set filters in query string', () => {
    useJobFilterStore.getState().setSearch('python')
    const qs = useJobFilterStore.getState().getQueryString()
    const params = new URLSearchParams(qs)
    expect(params.get('search')).toBe('python')
    expect(params.has('location')).toBe(false)
    expect(params.has('employmentType')).toBe(false)
  })
})
