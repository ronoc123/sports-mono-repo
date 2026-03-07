// ── Enums ─────────────────────────────────────────────────────────────────────

export type PollStatus = 'Draft' | 'Active' | 'Archived';

// ── DTOs ─────────────────────────────────────────────────────────────────────

export interface PollOptionDto {
  pollOptionId: string;
  optionText: string;
  voteCount: number;
  sortOrder: number;
}

export interface PollDto {
  pollId: string;
  questionText: string;
  status: PollStatus;
  totalVotes: number;
  createdAt: string;
  options: PollOptionDto[];
}

// ── Request bodies ────────────────────────────────────────────────────────────

export interface CreatePollRequest {
  organizationId: string;
  questionText: string;
  options: string[];
}
