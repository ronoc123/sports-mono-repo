import { Component, input, output, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
  FormGroup,
} from "@angular/forms";
import { MatCardModule } from "@angular/material/card";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatSnackBarModule } from "@angular/material/snack-bar";

import { LoginCredentials } from "../services/auth.service";

@Component({
  selector: "auth-login-form",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  template: `
    <div class="login-container">
      <mat-card class="login-card">
        <mat-card-header>
          <div class="login-header">
            <img
              [src]="appLogo() || '/assets/logo.png'"
              [alt]="appTitle()"
              class="app-logo"
              *ngIf="appLogo()"
            />
            <h1>{{ appTitle() }}</h1>
            <p class="login-subtitle">Sign in to your account</p>
          </div>
        </mat-card-header>

        <mat-card-content>
          <form
            [formGroup]="loginForm"
            (ngSubmit)="onSubmit()"
            class="login-form"
          >
            <!-- Email Field -->
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Email</mat-label>
              <input
                matInput
                type="email"
                formControlName="email"
                placeholder="Enter your email"
                autocomplete="email"
              />
              <mat-icon matSuffix>email</mat-icon>
              <mat-error *ngIf="loginForm.get('email')?.hasError('required')">
                Email is required
              </mat-error>
              <mat-error *ngIf="loginForm.get('email')?.hasError('email')">
                Please enter a valid email
              </mat-error>
            </mat-form-field>

            <!-- Password Field -->
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Password</mat-label>
              <input
                matInput
                [type]="hidePassword ? 'password' : 'text'"
                formControlName="password"
                placeholder="Enter your password"
                autocomplete="current-password"
              />
              <button
                mat-icon-button
                matSuffix
                type="button"
                (click)="hidePassword = !hidePassword"
                [attr.aria-label]="'Hide password'"
                [attr.aria-pressed]="hidePassword"
              >
                <mat-icon>{{
                  hidePassword ? "visibility_off" : "visibility"
                }}</mat-icon>
              </button>
              <mat-error
                *ngIf="loginForm.get('password')?.hasError('required')"
              >
                Password is required
              </mat-error>
              <mat-error
                *ngIf="loginForm.get('password')?.hasError('minlength')"
              >
                Password must be at least 6 characters
              </mat-error>
            </mat-form-field>

            <!-- Error Message -->
            <div class="error-message" *ngIf="error()">
              <mat-icon color="warn">error</mat-icon>
              <span>{{ error() }}</span>
            </div>

            <!-- Submit Button -->
            <button
              mat-raised-button
              color="primary"
              type="submit"
              class="login-button full-width"
              [disabled]="loginForm.invalid || isLoading()"
            >
              <mat-spinner diameter="20" *ngIf="isLoading()"></mat-spinner>
              <span *ngIf="!isLoading()">Sign In</span>
              <span *ngIf="isLoading()">Signing In...</span>
            </button>
          </form>
        </mat-card-content>

        <mat-card-actions class="login-actions">
          <div class="forgot-password">
            <button
              mat-button
              color="primary"
              type="button"
              (click)="onForgotPassword()"
            >
              Forgot Password?
            </button>
          </div>

          <div class="register-link" *ngIf="showRegisterLink()">
            <span>Don't have an account?</span>
            <button
              mat-button
              color="primary"
              type="button"
              (click)="onRegister()"
            >
              Sign Up
            </button>
          </div>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [
    `
      .login-container {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 100vh;
        background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
        padding: 20px;
      }

      .login-card {
        width: 100%;
        max-width: 420px;
        padding: 0;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        box-shadow: 0 20px 60px rgba(102, 126, 234, 0.4);
        border-radius: 20px;
        border: none;
        overflow: hidden;
      }

      .login-header {
        text-align: center;
        padding: 32px 24px 0;
        color: white;
      }

      .app-logo {
        height: 64px;
        width: auto;
        margin-bottom: 20px;
        filter: brightness(0) invert(1);
      }

      .login-header h1 {
        margin: 0 0 12px 0;
        font-size: 32px;
        font-weight: 600;
        color: white;
        text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      }

      .login-subtitle {
        margin: 0;
        color: rgba(255, 255, 255, 0.9);
        font-size: 16px;
        font-weight: 400;
      }

      .login-form {
        display: flex;
        flex-direction: column;
        gap: 20px;
        padding: 32px 28px;
        background: rgba(255, 255, 255, 0.95);
        backdrop-filter: blur(10px);
        margin: 0 16px 16px 16px;
        border-radius: 16px;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
      }

      .full-width {
        width: 100%;
      }

      .error-message {
        display: flex;
        align-items: center;
        gap: 8px;
        color: #dc2626;
        font-size: 14px;
        padding: 12px 16px;
        background-color: rgba(254, 226, 226, 0.9);
        border-radius: 8px;
        border-left: 4px solid #dc2626;
        backdrop-filter: blur(10px);
      }

      .login-button {
        height: 52px;
        font-size: 16px;
        font-weight: 600;
        margin-top: 12px;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        border-radius: 12px;
        text-transform: none;
        letter-spacing: 0.5px;
        box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
        transition: all 0.3s ease;
      }

      .login-button:hover {
        transform: translateY(-2px);
        box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6);
      }

      .login-actions {
        padding: 0 28px 32px;
        display: flex;
        flex-direction: column;
        gap: 16px;
      }

      .forgot-password {
        text-align: center;
      }

      .forgot-password button {
        color: rgba(255, 255, 255, 0.9);
        font-weight: 500;
        text-decoration: none;
        transition: color 0.3s ease;
      }

      .forgot-password button:hover {
        color: white;
      }

      .register-link {
        text-align: center;
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 4px;
        font-size: 14px;
      }

      .register-link span {
        color: rgba(255, 255, 255, 0.8);
      }

      .register-link button {
        color: white;
        font-weight: 600;
        text-decoration: none;
        transition: color 0.3s ease;
      }

      .register-link button:hover {
        color: rgba(255, 255, 255, 0.9);
      }

      /* Input field styling */
      ::ng-deep .mat-mdc-form-field {
        width: 100%;
      }

      ::ng-deep .mat-mdc-text-field-wrapper {
        background-color: rgba(255, 255, 255, 0.9);
        border-radius: 12px;
      }

      ::ng-deep .mat-mdc-form-field-focus-overlay {
        background-color: transparent;
      }

      ::ng-deep .mdc-text-field--focused .mdc-line-ripple::before {
        border-bottom-color: #667eea;
      }

      /* Responsive adjustments */
      @media (max-width: 480px) {
        .login-container {
          padding: 12px;
        }

        .login-card {
          max-width: 100%;
          margin: 0;
        }

        .login-header {
          padding: 24px 20px 0;
        }

        .login-header h1 {
          font-size: 28px;
        }

        .login-form {
          padding: 24px 20px;
          margin: 0 12px 12px 12px;
        }

        .login-actions {
          padding: 0 20px 24px;
        }
      }

      @media (max-width: 360px) {
        .login-header h1 {
          font-size: 24px;
        }

        .login-subtitle {
          font-size: 14px;
        }
      }
    `,
  ],
})
export class LoginFormComponent {
  private readonly fb = inject(FormBuilder);

  // Inputs
  readonly appTitle = input<string>("Sports UI");
  readonly appLogo = input<string>("");
  readonly isLoading = input<boolean>(false);
  readonly error = input<string | null>(null);
  readonly showRegisterLink = input<boolean>(true);

  // Outputs
  readonly loginSubmit = output<LoginCredentials>();
  readonly forgotPassword = output<void>();
  readonly register = output<void>();

  // Form state
  hidePassword = true;

  // Reactive form
  loginForm: FormGroup = this.fb.group({
    email: ["", [Validators.required, Validators.email]],
    password: ["", [Validators.required, Validators.minLength(6)]],
  });

  onSubmit(): void {
    if (this.loginForm.valid) {
      const credentials: LoginCredentials = {
        email: this.loginForm.value.email,
        password: this.loginForm.value.password,
      };
      this.loginSubmit.emit(credentials);
    }
  }

  onForgotPassword(): void {
    this.forgotPassword.emit();
  }

  onRegister(): void {
    this.register.emit();
  }
}
