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
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        padding: 20px;
      }

      .login-card {
        width: 100%;
        max-width: 400px;
        padding: 0;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
        border-radius: 16px;
      }

      .login-header {
        text-align: center;
        padding: 24px 24px 0;
      }

      .app-logo {
        height: 64px;
        width: auto;
        margin-bottom: 16px;
      }

      .login-header h1 {
        margin: 0 0 8px 0;
        font-size: 28px;
        font-weight: 500;
        color: #333;
      }

      .login-subtitle {
        margin: 0;
        color: #666;
        font-size: 14px;
      }

      .login-form {
        display: flex;
        flex-direction: column;
        gap: 16px;
        padding: 24px;
      }

      .full-width {
        width: 100%;
      }

      .error-message {
        display: flex;
        align-items: center;
        gap: 8px;
        color: #f44336;
        font-size: 14px;
        padding: 8px 12px;
        background-color: #ffebee;
        border-radius: 4px;
        border-left: 4px solid #f44336;
      }

      .login-button {
        height: 48px;
        font-size: 16px;
        font-weight: 500;
        margin-top: 8px;
      }

      .login-actions {
        padding: 0 24px 24px;
        display: flex;
        flex-direction: column;
        gap: 12px;
      }

      .forgot-password {
        text-align: center;
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
        color: #666;
      }

      /* Responsive adjustments */
      @media (max-width: 480px) {
        .login-container {
          padding: 16px;
        }

        .login-card {
          max-width: 100%;
        }

        .login-header h1 {
          font-size: 24px;
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
