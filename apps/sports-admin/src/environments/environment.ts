export const environment = {
  production: false,

  // API Configuration
  apiUrl: "http://localhost:5000",
  apiBaseUrl: "http://localhost:5000/api",
  identityApiUrl: "http://localhost:5001",
  identityApiBaseUrl: "http://localhost:5001/api",

  // Feature flags
  enableMockData: true, // Use mock data during development
  enableRealAuth: false, // Use mock auth during development
  enableLogging: true,

  // Development settings
  logLevel: "debug",
  enableConsoleLogging: true,

  // API Configuration
  apiTimeout: 10000, // 10 seconds for development
  retryAttempts: 2,
  retryDelay: 1000, // 1 second

  // CORS and SSL
  allowSelfSignedCerts: true, // Allow in development
  corsEnabled: true,

  // App-specific settings
  appName: "Sports Admin",
  appVersion: "1.0.0",

  // Admin-specific settings
  enableAdvancedFeatures: true,
  enableSystemLogs: true,
  enableUserImpersonation: true, // Only in development

  // Google OAuth (development)
  googleClientId:
    "437642911171-5qq4jsh5qvhkh17ua79srdrr16i0p0c4.apps.googleusercontent.com",
};
