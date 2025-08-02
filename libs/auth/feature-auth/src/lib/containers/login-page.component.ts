import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

import { LoginFormComponent } from '../components/login-form.component';
import { AuthService, LoginCredentials } from '../services/auth.service';

@Component({
  selector: 'auth-login-page',
  standalone: true,
  imports: [
    CommonModule,
    LoginFormComponent,
  ],
  template: `
    <auth-login-form
      [appTitle]="appTitle"
      [appLogo]="appLogo"
      [isLoading]="authService.isLoading()"
      [error]="authService.error()"
      [showRegisterLink]="showRegisterLink"
      (loginSubmit)="onLogin($event)"
      (forgotPassword)="onForgotPassword()"
      (register)="onRegister()"
    ></auth-login-form>
  `,
  styles: [`
    :host {
      display: block;
      height: 100vh;
      width: 100vw;
    }
  `],
})
export class LoginPageComponent {
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  readonly authService = inject(AuthService);

  // Configuration - these could be injected from app config
  appTitle = 'Sports UI';
  appLogo = '/assets/sports-logo.png';
  showRegisterLink = true;

  onLogin(credentials: LoginCredentials): void {
    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.snackBar.open(`Welcome back, ${response.user.firstName || response.user.email}!`, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        
        // Navigate to dashboard or intended route
        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error('Login failed:', error);
        // Error is already handled by the auth service and displayed in the form
      }
    });
  }

  onForgotPassword(): void {
    // Navigate to forgot password page or show dialog
    this.snackBar.open('Forgot password functionality coming soon!', 'Close', {
      duration: 3000,
    });
  }

  onRegister(): void {
    // Navigate to registration page
    this.router.navigate(['/register']);
  }
}
