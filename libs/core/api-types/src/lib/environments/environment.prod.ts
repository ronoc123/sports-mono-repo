export const environment = {
  production: true,

  // API Configuration — values injected by CI from GitHub Actions secrets
  apiUrl: "__SPORTS_API_URL__",
  sportsApi: "/api/",
  identityApi: "__IDENTITY_API_URL__/api/",

  // Keep these matching the AppEnvironment interface
  apiBaseUrl: "__SPORTS_API_URL__/api",
  identityApiUrl: "__IDENTITY_API_URL__",
  identityApiBaseUrl: "__IDENTITY_API_URL__/api",

  // Feature flags
  enableMockData: false,
  enableRealAuth: true,
  enableLogging: false,

  // Production settings
  logLevel: "error" as const,
  enableConsoleLogging: false,

  // API Configuration
  apiTimeout: 30000,
  retryAttempts: 3,
  retryDelay: 2000,

  // CORS and SSL
  allowSelfSignedCerts: false,
  corsEnabled: true,

  // App-specific settings
  appName: "Sports Admin",
  appVersion: "1.0.0",

  // Admin-specific settings
  enableAdvancedFeatures: true,
  enableSystemLogs: true,
  enableUserImpersonation: false,

  // OAuth — injected by CI
  googleClientId: "__GOOGLE_CLIENT_ID__",

  // Stripe public key — injected by CI
  stripePublicKey: "__STRIPE_PUBLIC_KEY__",

  socialMediaApi: "__SOCIAL_MEDIA_API_URL__/api/",
};
