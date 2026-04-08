// ── Auth ──

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: "Candidate" | "Employer";
}

export interface AuthResponse {
  token: string;
  email: string;
  role: string;
  userId: string;
}

// ── Profile ──

export interface CandidateProfile {
  candidateId: string;
  userId: string;
  phone?: string;
  location?: string;
  professionalSummary?: string;
  yearsOfExperience: number;
  highestEducation?: string;
  linkedinUrl?: string;
  createdAt: string;
  updatedAt: string;
}

export interface EmployerProfile {
  employerId: string;
  userId: string;
  companyName: string;
  companyDescription?: string;
  industry?: string;
  websiteUrl?: string;
  companyLocation?: string;
  logoUrl?: string;
  createdAt: string;
  updatedAt: string;
}

// ── Jobs ──

export interface JobSkill {
  skillId: string;
  name: string;
  category: string;
  importanceLevel: string;
}

export interface Job {
  jobId: string;
  employerId: string;
  title: string;
  description: string;
  location?: string;
  employmentType: string;
  minSalary?: number;
  maxSalary?: number;
  experienceYearsMin?: number;
  experienceYearsMax?: number;
  status: string;
  skills: JobSkill[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateJobRequest {
  title: string;
  description: string;
  location?: string;
  employmentType: string;
  experienceRequired: number;
  salaryMin?: number;
  salaryMax?: number;
  applicationDeadline?: string;
  skills: { skillId: string; importanceLevel: string }[];
}

export interface JobListResponse {
  items: Job[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface Skill {
  skillId: string;
  name: string;
  category: string;
}

// ── Applications ──

export interface Application {
  applicationId: string;
  candidateId: string;
  jobId: string;
  coverLetter?: string;
  coverLetterUrl?: string;
  resumeUrl?: string;
  status: string;
  employerNotes?: string;
  appliedAt: string;
  updatedAt: string;
}

export interface ApplicationListResponse {
  items: Application[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export type ApplicationStatus = "Submitted" | "Reviewed" | "Shortlisted" | "Accepted" | "Rejected" | "Withdrawn";

// ── Resume ──

export interface ParsedResumeData {
  personalInfo?: {
    name?: string;
    email?: string;
    phone?: string;
    location?: string;
  };
  summary?: string;
  experience: Array<{
    company: string;
    title: string;
    startDate?: string;
    endDate?: string;
    description: string;
    highlights: string[];
  }>;
  education: Array<{
    institution: string;
    degree: string;
    field: string;
    graduationDate?: string;
    gpa?: number;
  }>;
  skills: Array<{
    name: string;
    category: string;
    proficiencyLevel: string;
    yearsUsed?: number;
  }>;
  certifications: Array<{
    name: string;
    issuer?: string;
    date?: string;
  }>;
  projects: Array<{
    name: string;
    description: string;
    technologies: string[];
  }>;
}

export interface ResumeResponse {
  resumeId: string;
  candidateId: string;
  originalFileName: string;
  fileUrl: string;
  status: string;
  errorMessage?: string;
  parsedData?: ParsedResumeData;
  uploadedAt: string;
  parsedAt?: string;
}

export interface ResumeStatusResponse {
  resumeId: string;
  status: string;
  errorMessage?: string;
}

export interface ResumeUploadResponse {
  resumeId: string;
  status: string;
  message: string;
}

export interface DocumentUploadResponse {
  fileUrl: string;
  extractedText: string;
}

// ── Matching ──

export interface MatchScoreResponse {
  matchScoreId: string;
  candidateId: string;
  jobId: string;
  overallScore: number;
  skillScore: number;
  experienceScore: number;
  educationScore: number;
  explanation?: string;
  strengths: string[];
  gaps: string[];
  generatedAt: string;
}

export interface MatchListResponse {
  items: MatchScoreResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ── Skill Gap ──

export interface MissingSkillEntry {
  skillName: string;
  importance: string;
  currentLevel?: string;
  requiredLevel: string;
  gapSeverity: number;
  recommendation: string;
}

export interface RecommendedAction {
  priority: number;
  action: string;
  estimatedTime: string;
  resourceType: string;
  rationale: string;
}

export interface SkillGapReportResponse {
  reportId: string;
  candidateId: string;
  jobId: string;
  summary?: string;
  overallReadiness: number;
  estimatedTimeToReady?: string;
  missingSkills: MissingSkillEntry[];
  recommendedActions: RecommendedAction[];
  strengths: string[];
  generatedAt: string;
}

// ── Analytics ──

export interface EmployerDashboardResponse {
  totalActiveJobs: number;
  totalApplications: number;
  averageMatchScore: number;
  pipelineBreakdown: Record<string, number>;
  topSkillsInDemand: Array<{ skill: string; count: number }>;
}

export interface JobAnalyticsResponse {
  jobId: string;
  totalApplicants: number;
  averageMatchScore: number;
  scoreDistribution: Record<string, number>;
  pipelineBreakdown: Record<string, number>;
  topCandidates: MatchScoreResponse[];
  commonSkillGaps: Array<{ skill: string; count: number }>;
  daysSincePosting: number;
}

export interface TrendDataResponse {
  applicationsPerWeek: Array<{ week: string; value: number }>;
  averageScorePerWeek: Array<{ week: string; value: number }>;
  mostRequestedSkills: Array<{ skill: string; count: number }>;
}

// ── Notifications ──

export interface UserNotification {
  notificationId: string;
  type: string;
  title: string;
  message: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationListResponse {
  items: UserNotification[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UnreadCountResponse {
  count: number;
}
