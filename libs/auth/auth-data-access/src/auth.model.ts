// libs/auth/store/src/lib/auth.model.ts

export interface SessionUser {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;

  profilePictureUrl?: string;
  isGoogleUser?: boolean;

  createdAt?: string;
  lastLoginAt?: string;

  roles?: string[];
  permissions?: string[];
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string | null;
  expiresAt: string;
}

export interface AuthState {
  // core
  loggedIn: boolean;
  authenticating: boolean;
  error: string | null;

  // session
  user: SessionUser | null;
  tokens: AuthTokens | null;
}

export const initialAuthState: AuthState = {
  loggedIn: false,
  authenticating: false,
  error: null,

  user: null,
  tokens: null,
};
