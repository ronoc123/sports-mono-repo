import { Injectable, inject, InjectionToken } from '@angular/core';

// Environment interface that all apps should implement
export interface AppEnvironment {
  production: boolean;
  
  // API Configuration
  apiUrl: string;
  apiBaseUrl: string;
  identityApiUrl: string;
  identityApiBaseUrl: string;
  
  // Feature flags
  enableMockData: boolean;
  enableRealAuth: boolean;
  enableLogging: boolean;
  
  // Settings
  logLevel: 'debug' | 'info' | 'warn' | 'error';
  enableConsoleLogging: boolean;
  
  // API Configuration
  apiTimeout: number;
  retryAttempts: number;
  retryDelay: number;
  
  // CORS and SSL
  allowSelfSignedCerts: boolean;
  corsEnabled: boolean;
  
  // App-specific settings
  appName: string;
  appVersion: string;
  
  // OAuth
  googleClientId: string;
  
  // Additional app-specific properties
  [key: string]: any;
}

// Injection token for environment
export const APP_ENVIRONMENT = new InjectionToken<AppEnvironment>('APP_ENVIRONMENT');

@Injectable({
  providedIn: 'root'
})
export class EnvironmentService {
  private readonly environment = inject(APP_ENVIRONMENT);

  // Core API URLs
  get apiUrl(): string {
    return this.environment.apiUrl;
  }

  get apiBaseUrl(): string {
    return this.environment.apiBaseUrl;
  }

  get identityApiUrl(): string {
    return this.environment.identityApiUrl;
  }

  get identityApiBaseUrl(): string {
    return this.environment.identityApiBaseUrl;
  }

  // Feature flags
  get isProduction(): boolean {
    return this.environment.production;
  }

  get isDevelopment(): boolean {
    return !this.environment.production;
  }

  get enableMockData(): boolean {
    return this.environment.enableMockData;
  }

  get enableRealAuth(): boolean {
    return this.environment.enableRealAuth;
  }

  get enableLogging(): boolean {
    return this.environment.enableLogging;
  }

  // API Configuration
  get apiTimeout(): number {
    return this.environment.apiTimeout;
  }

  get retryAttempts(): number {
    return this.environment.retryAttempts;
  }

  get retryDelay(): number {
    return this.environment.retryDelay;
  }

  // App info
  get appName(): string {
    return this.environment.appName;
  }

  get appVersion(): string {
    return this.environment.appVersion;
  }

  // OAuth
  get googleClientId(): string {
    return this.environment.googleClientId;
  }

  // Utility methods
  getApiEndpoint(path: string): string {
    const cleanPath = path.startsWith('/') ? path.slice(1) : path;
    return `${this.apiBaseUrl}/${cleanPath}`;
  }

  getIdentityEndpoint(path: string): string {
    const cleanPath = path.startsWith('/') ? path.slice(1) : path;
    return `${this.identityApiBaseUrl}/${cleanPath}`;
  }

  // Get custom environment property
  getCustomProperty<T = any>(key: string, defaultValue?: T): T {
    return this.environment[key] ?? defaultValue;
  }

  // Check if feature is enabled
  isFeatureEnabled(featureName: string): boolean {
    const key = `enable${featureName.charAt(0).toUpperCase()}${featureName.slice(1)}`;
    return this.environment[key] === true;
  }

  // Get full environment (for debugging)
  getEnvironment(): AppEnvironment {
    return { ...this.environment };
  }

  // Log environment info (development only)
  logEnvironmentInfo(): void {
    if (this.isDevelopment && this.environment.enableConsoleLogging) {
      console.group('🌍 Environment Configuration');
      console.log('App Name:', this.appName);
      console.log('Version:', this.appVersion);
      console.log('Mode:', this.isProduction ? 'Production' : 'Development');
      console.log('API URL:', this.apiUrl);
      console.log('Identity API URL:', this.identityApiUrl);
      console.log('Mock Data:', this.enableMockData ? 'Enabled' : 'Disabled');
      console.log('Real Auth:', this.enableRealAuth ? 'Enabled' : 'Disabled');
      console.groupEnd();
    }
  }
}
