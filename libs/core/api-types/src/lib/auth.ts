export interface LoginUserResponse {
  token: string;
  username: string;
}

export interface LoginUserRequest {
  token: string;
}

// keep in sync with IdentityService AuthenticationResponse
export type UserInfo = {
  id: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  fullName?: string | null;
  profilePictureUrl?: string | null;
  isGoogleUser?: boolean;
  roles: string[];
  createdAt?: string | null;
  lastLoginAt?: string | null;
};

export type AuthenticationResponse = {
  success: boolean;
  message: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string; // ISO string from API
  user: UserInfo | null;
};

export type RegisterRequest = {
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
};

export type LoginRequest = {
  email: string;
  password: string;
};

export type GoogleLoginRequest = {
  googleToken: string; // your controller expects "GoogleToken"
};

export type RefreshTokenRequest = {
  refreshToken: string;
};

// your standard envelope
export type ServiceResponse<T> = {
  data?: T | null;
  success: boolean;
  message: string;
  errorCode?: string | null;
  traceId?: string | null;
  validationErrors?: Record<string, string[]> | null;
  details?: unknown;
};
