export interface TrendingPlayerOptionDto {
  playerOptionId: string;
  playerName: string;
  positionLabel: string;
  voteCount: number;
}

export interface ActiveTriviaQuestionDto {
  questionId: string;
  questionText: string;
  answerOptions: string[];
  seriesLabel: string;
  voteRewardAmount: number;
  answeredByMe: boolean;
  selectedAnswer: string | null;
}

export interface ActivePollOptionDto {
  pollOptionId: string;
  optionText: string;
  voteCount: number;
}

export interface ActivePollDto {
  pollId: string;
  questionText: string;
  options: ActivePollOptionDto[];
  votedByMe: boolean;
  selectedOptionId: string | null;
}

export interface DashboardResponse {
  trendingPlayerOptions: TrendingPlayerOptionDto[];
  activeTriviaQuestions: ActiveTriviaQuestionDto[];
  activePoll: ActivePollDto | null;
}

// ── Poll vote ─────────────────────────────────────────────────────────────────

export interface SubmitPollVoteRequest {
  userId: string;
  organizationId: string;
  pollId: string;
  pollOptionId: string;
}

export interface PollVoteOptionResult {
  pollOptionId: string;
  optionText: string;
  voteCount: number;
}

export interface SubmitPollVoteResult {
  votedOptionId: string;
  options: PollVoteOptionResult[];
}

// ── Trivia answer ─────────────────────────────────────────────────────────────

export interface SubmitTriviaAnswerRequest {
  userId: string;
  organizationId: string;
  triviaQuestionId: string;
  selectedAnswer: string;
}

export interface SubmitTriviaAnswerResult {
  isCorrect: boolean;
  votesEarned: number;
  correctAnswer: string;
}
