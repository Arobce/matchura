import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { JobCard } from '../JobCard'
import { createMockJob } from '@/__tests__/test-utils'

vi.mock('next/link', () => ({
  default: ({ href, children, className }: { href: string; children: React.ReactNode; className?: string }) => (
    <a href={href} className={className}>{children}</a>
  ),
}))

describe('JobCard', () => {
  const mockJob = createMockJob()

  it('renders the job title', () => {
    render(<JobCard job={mockJob} />)
    expect(screen.getByText('Senior React Developer')).toBeInTheDocument()
  })

  it('renders the job description', () => {
    render(<JobCard job={mockJob} />)
    expect(screen.getByText('We are looking for a senior React developer.')).toBeInTheDocument()
  })

  it('renders the job location', () => {
    render(<JobCard job={mockJob} />)
    expect(screen.getByText('New York, NY')).toBeInTheDocument()
  })

  it('renders employment type', () => {
    render(<JobCard job={mockJob} />)
    expect(screen.getByText('FullTime')).toBeInTheDocument()
  })

  it('renders skill badges', () => {
    render(<JobCard job={mockJob} />)
    expect(screen.getByText('React')).toBeInTheDocument()
    expect(screen.getByText('TypeScript')).toBeInTheDocument()
  })

  it('links to the job detail page', () => {
    render(<JobCard job={mockJob} />)
    const link = screen.getByRole('link')
    expect(link).toHaveAttribute('href', '/jobs/job-1')
  })

  it('renders the title abbreviation icon', () => {
    render(<JobCard job={mockJob} />)
    expect(screen.getByText('SE')).toBeInTheDocument()
  })

  it('renders with custom job data', () => {
    const customJob = createMockJob({
      title: 'Backend Engineer',
      location: 'Remote',
      jobId: 'job-custom',
    })
    render(<JobCard job={customJob} />)
    expect(screen.getByText('Backend Engineer')).toBeInTheDocument()
    expect(screen.getByText('Remote')).toBeInTheDocument()
    expect(screen.getByRole('link')).toHaveAttribute('href', '/jobs/job-custom')
  })
})
