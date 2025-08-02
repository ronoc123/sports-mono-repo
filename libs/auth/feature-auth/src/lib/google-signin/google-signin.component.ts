import { Component, inject, input, output, signal, OnInit, OnDestroy, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { GoogleAuthService } from '@sports-ui/auth';
import { User } from '@sports-ui/api-types';

export interface GoogleSignInOptions {
  type?: 'standard' | 'icon';
  theme?: 'outline' | 'filled_blue' | 'filled_black';
  size?: 'large' | 'medium' | 'small';
  text?: 'signin_with' | 'signup_with' | 'continue_with' | 'signin';
  shape?: 'rectangular' | 'pill' | 'circle' | 'square';
  logo_alignment?: 'left' | 'center';
  width?: number;
}

@Component({
  selector: 'lib-google-signin',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './google-signin.component.html',
  styleUrl: './google-signin.component.css',
})
export class GoogleSignInComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly googleAuthService = inject(GoogleAuthService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  @ViewChild('googleButtonContainer', { static: false }) googleButtonContainer!: ElementRef;

  // Inputs
  options = input<GoogleSignInOptions>({});
  useCustomButton = input<boolean>(false);
  buttonText = input<string>('Sign in with Google');
  showSpinner = input<boolean>(true);

  // Outputs
  signInSuccess = output<User>();
  signInError = output<string>();

  // Local state
  private readonly isSigningIn = signal<boolean>(false);
  private readonly componentId = `google-signin-${Math.random().toString(36).substr(2, 9)}`;

  // Expose signals
  readonly signingIn = this.isSigningIn.asReadonly();
  readonly googleAuthInitialized = this.googleAuthService.initialized;
  readonly googleAuthLoading = this.googleAuthService.loading;
  readonly googleAuthError = this.googleAuthService.authError;

  ngOnInit() {
    // Clear any previous errors
    this.googleAuthService.clearError();
  }

  ngAfterViewInit() {
    // Render Google button if not using custom button and Google Auth is initialized
    if (!this.useCustomButton() && this.googleAuthInitialized()) {
      this.renderGoogleButton();
    }

    // Watch for initialization changes
    setTimeout(() => {
      if (!this.useCustomButton() && this.googleAuthInitialized() && !this.googleButtonContainer?.nativeElement.hasChildNodes()) {
        this.renderGoogleButton();
      }
    }, 100);
  }

  ngOnDestroy() {
    // Cleanup if needed
  }

  /**
   * Render the official Google Sign-In button
   */
  private renderGoogleButton(): void {
    if (this.googleButtonContainer?.nativeElement) {
      // Clear any existing content
      this.googleButtonContainer.nativeElement.innerHTML = '';
      
      // Create a unique ID for this button
      const buttonId = `${this.componentId}-button`;
      const buttonDiv = document.createElement('div');
      buttonDiv.id = buttonId;
      this.googleButtonContainer.nativeElement.appendChild(buttonDiv);

      // Render the Google button
      this.googleAuthService.renderSignInButton(buttonId, this.options());
    }
  }

  /**
   * Handle custom button click
   */
  onCustomButtonClick(): void {
    if (this.isSigningIn() || !this.googleAuthInitialized()) {
      return;
    }

    this.signInWithGoogle();
  }

  /**
   * Sign in with Google
   */
  private signInWithGoogle(): void {
    this.isSigningIn.set(true);

    this.googleAuthService.signInWithGoogle().subscribe({
      next: (user: User) => {
        this.isSigningIn.set(false);
        this.signInSuccess.emit(user);
        
        this.snackBar.open('Successfully signed in with Google!', 'Close', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });

        // Navigate to dashboard or return URL
        const returnUrl = this.getReturnUrl();
        this.router.navigate([returnUrl || '/']);
      },
      error: (error) => {
        this.isSigningIn.set(false);
        const errorMessage = error.message || 'Google sign-in failed';
        this.signInError.emit(errorMessage);
        
        this.snackBar.open(errorMessage, 'Close', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  /**
   * Get return URL from query params
   */
  private getReturnUrl(): string | null {
    if (typeof window !== 'undefined') {
      const urlParams = new URLSearchParams(window.location.search);
      return urlParams.get('returnUrl');
    }
    return null;
  }

  /**
   * Retry Google Auth initialization
   */
  retryInitialization(): void {
    window.location.reload();
  }
}
