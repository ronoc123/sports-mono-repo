// ── Enums ─────────────────────────────────────────────────────────────────────

export type TriviaQuestionStatus = 'Pending' | 'Active' | 'Archived';

// ── DTOs ─────────────────────────────────────────────────────────────────────

export interface TriviaQuestionDto {
  questionId: string;
  questionText: string;
  answerOptions: string[];
  correctAnswer: string;
  voteRewardAmount: number;
  status: TriviaQuestionStatus;
  participationCount: number;
  createdAt: string;
}

export interface TriviaSeriesDto {
  seriesId: string;
  seriesName: string;
  createdAt: string;
  questions: TriviaQuestionDto[];
}

// ── Request bodies ────────────────────────────────────────────────────────────

export interface CreateTriviaSeriesRequest {
  organizationId: string;
  seriesName: string;
}

export interface AddTriviaQuestionRequest {
  seriesId: string;
  organizationId: string;
  questionText: string;
  answerOptions: string[];
  correctAnswer: string;
  voteRewardAmount: number;
}
