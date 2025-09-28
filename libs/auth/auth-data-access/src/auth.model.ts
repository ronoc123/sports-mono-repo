export type AuthTokens = {
  accessToken: string;
  refreshToken: string;
  expiresAt: string; // ISO
};

export type SessionUser = {
  id: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  fullName?: string | null;
  profilePictureUrl?: string | null;
  roles: string[];
};

export type AuthState = {
  loggedIn: boolean;
  user: SessionUser | null;
  tokens: AuthTokens | null;
  authenticating: boolean;
  error: string | null;
  rememberMe: boolean;
};

export const initialAuthState: AuthState = {
  loggedIn: false,
  user: null,
  tokens: null,
  authenticating: false,
  error: null,
  rememberMe: false,
};
