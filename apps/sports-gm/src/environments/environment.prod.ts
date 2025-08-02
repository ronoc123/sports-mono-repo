export const environment = {
  production: true,

  // API Configuration (production URLs)
  apiUrl: "https://api.sports-ui.com",
  apiBaseUrl: "https://api.sports-ui.com/api",
  identityApiUrl: "https://identity.sports-ui.com",
  identityApiBaseUrl: "https://identity.sports-ui.com/api",

  // Feature flags
  enableMockData: false, // Use real data in production
  enableRealAuth: true, // Use real auth in production
  enableLogging: false,

  // Production settings
  logLevel: "error",
  enableConsoleLogging: false,

  // API Configuration
  apiTimeout: 30000, // 30 seconds
  retryAttempts: 3,
  retryDelay: 2000, // 2 seconds

  // CORS and SSL
  allowSelfSignedCerts: false, // Never allow in production
  corsEnabled: true,

  // App-specific settings
  appName: "Sports GM",
  appVersion: "1.0.0",

  // GM-specific settings
  enableAnalytics: true,
  enableReporting: true,
  enablePlayerManagement: true,

  // Google OAuth (production)
  googleClientId: "your-prod-google-client-id.apps.googleusercontent.com",
};
