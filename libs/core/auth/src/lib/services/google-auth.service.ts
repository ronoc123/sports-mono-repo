import { Injectable, inject, signal, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Observable, from, throwError, BehaviorSubject } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { UserStore } from '@sports-ui/data-access';
import { User } from '@sports-ui/api-types';

export interface GoogleAuthConfig {
  clientId: string;
  scopes?: string[];
}

export interface GoogleUser {
  id: string;
  email: string;
  name: string;
  picture: string;
  given_name: string;
  family_name: string;
}

export interface GoogleAuthResponse {
  credential: string;
  select_by: string;
}

declare global {
  interface Window {
    google: any;
    googleAuthCallback: (response: GoogleAuthResponse) => void;
  }
}

@Injectable({
  providedIn: 'root'
})
export class GoogleAuthService {
  private readonly authService = inject(AuthService);
  private readonly userStore = inject(UserStore);
  private readonly platformId = inject(PLATFORM_ID);

  // Configuration
  private readonly config: GoogleAuthConfig = {
    clientId: '1234567890-abcdefghijklmnopqrstuvwxyz.apps.googleusercontent.com', // TODO: Replace with actual client ID
    scopes: ['email', 'profile']
  };

  // State
  private readonly isInitialized = signal<boolean>(false);
  private readonly isLoading = signal<boolean>(false);
  private readonly error = signal<string | null>(null);

  // Expose read-only signals
  readonly initialized = this.isInitialized.asReadonly();
  readonly loading = this.isLoading.asReadonly();
  readonly authError = this.error.asReadonly();

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      this.initializeGoogleAuth();
    }
  }

  /**
   * Initialize Google Auth library
   */
  private async initializeGoogleAuth(): Promise<void> {
    try {
      // Load Google Identity Services script
      await this.loadGoogleScript();
      
      // Initialize Google Auth
      if (window.google?.accounts?.id) {
        window.google.accounts.id.initialize({
          client_id: this.config.clientId,
          callback: this.handleGoogleResponse.bind(this),
          auto_select: false,
          cancel_on_tap_outside: true,
        });

        this.isInitialized.set(true);
      } else {
        throw new Error('Google Identity Services not loaded');
      }
    } catch (error) {
      console.error('Failed to initialize Google Auth:', error);
      this.error.set('Failed to initialize Google authentication');
    }
  }

  /**
   * Load Google Identity Services script
   */
  private loadGoogleScript(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (window.google?.accounts?.id) {
        resolve();
        return;
      }

      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;
      
      script.onload = () => resolve();
      script.onerror = () => reject(new Error('Failed to load Google script'));
      
      document.head.appendChild(script);
    });
  }

  /**
   * Handle Google authentication response
   */
  private async handleGoogleResponse(response: GoogleAuthResponse): Promise<void> {
    try {
      this.isLoading.set(true);
      this.error.set(null);

      // Decode the JWT token to get user info
      const userInfo = this.decodeJWT(response.credential);
      
      // Convert Google user to our User format
      const user: User = {
        id: userInfo.sub,
        userName: userInfo.name,
        email: userInfo.email,
        firstName: userInfo.given_name,
        lastName: userInfo.family_name,
        profilePictureUrl: userInfo.picture,
        isActive: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        lastLoginAt: new Date().toISOString(),
        emailVerified: userInfo.email_verified || false,
        provider: 'google'
      };

      // Update user store
      this.userStore.setCurrentUser(user);

      // Store the Google credential for API calls
      this.storeGoogleCredential(response.credential);

      console.log('Google sign-in successful:', user);
    } catch (error) {
      console.error('Google sign-in error:', error);
      this.error.set('Google sign-in failed');
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Decode JWT token
   */
  private decodeJWT(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (error) {
      throw new Error('Invalid JWT token');
    }
  }

  /**
   * Store Google credential
   */
  private storeGoogleCredential(credential: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('google_credential', credential);
    }
  }

  /**
   * Get stored Google credential
   */
  getGoogleCredential(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem('google_credential');
    }
    return null;
  }

  /**
   * Sign in with Google
   */
  signInWithGoogle(): Observable<User> {
    return new Observable(observer => {
      if (!this.isInitialized()) {
        observer.error(new Error('Google Auth not initialized'));
        return;
      }

      try {
        // Show Google One Tap or sign-in prompt
        window.google.accounts.id.prompt((notification: any) => {
          if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
            // Fallback to popup sign-in
            this.showGoogleSignInPopup();
          }
        });

        // Set up a temporary callback to handle the response
        const originalCallback = window.googleAuthCallback;
        window.googleAuthCallback = (response: GoogleAuthResponse) => {
          this.handleGoogleResponse(response).then(() => {
            const user = this.userStore.currentUser();
            if (user) {
              observer.next(user);
              observer.complete();
            } else {
              observer.error(new Error('Failed to get user after Google sign-in'));
            }
          }).catch(error => {
            observer.error(error);
          });
          
          // Restore original callback
          window.googleAuthCallback = originalCallback;
        };

      } catch (error) {
        observer.error(error);
      }
    });
  }

  /**
   * Show Google sign-in popup as fallback
   */
  private showGoogleSignInPopup(): void {
    if (window.google?.accounts?.oauth2) {
      const client = window.google.accounts.oauth2.initTokenClient({
        client_id: this.config.clientId,
        scope: this.config.scopes?.join(' ') || 'email profile',
        callback: (response: any) => {
          if (response.access_token) {
            this.getUserInfoFromToken(response.access_token);
          }
        },
      });
      client.requestAccessToken();
    }
  }

  /**
   * Get user info from access token
   */
  private async getUserInfoFromToken(accessToken: string): Promise<void> {
    try {
      const response = await fetch(`https://www.googleapis.com/oauth2/v2/userinfo?access_token=${accessToken}`);
      const userInfo = await response.json();
      
      // Create a mock credential response
      const mockResponse: GoogleAuthResponse = {
        credential: accessToken,
        select_by: 'popup'
      };
      
      await this.handleGoogleResponse(mockResponse);
    } catch (error) {
      console.error('Failed to get user info from token:', error);
      this.error.set('Failed to get user information');
    }
  }

  /**
   * Sign out from Google
   */
  signOut(): Observable<boolean> {
    return new Observable(observer => {
      try {
        if (window.google?.accounts?.id) {
          window.google.accounts.id.disableAutoSelect();
        }

        // Clear stored credential
        if (isPlatformBrowser(this.platformId)) {
          localStorage.removeItem('google_credential');
        }

        // Clear user from store
        this.userStore.setCurrentUser(null);

        observer.next(true);
        observer.complete();
      } catch (error) {
        observer.error(error);
      }
    });
  }

  /**
   * Render Google Sign-In button
   */
  renderSignInButton(elementId: string, options?: any): void {
    if (!this.isInitialized()) {
      console.warn('Google Auth not initialized');
      return;
    }

    const defaultOptions = {
      type: 'standard',
      theme: 'outline',
      size: 'large',
      text: 'signin_with',
      shape: 'rectangular',
      logo_alignment: 'left',
      width: 250,
    };

    const buttonOptions = { ...defaultOptions, ...options };

    window.google.accounts.id.renderButton(
      document.getElementById(elementId),
      buttonOptions
    );
  }

  /**
   * Clear error
   */
  clearError(): void {
    this.error.set(null);
  }
}
