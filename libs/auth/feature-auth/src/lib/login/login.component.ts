import { Component, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from "@angular/forms";
import { Router } from "@angular/router";
import { MatCardModule } from "@angular/material/card";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatSnackBar, MatSnackBarModule } from "@angular/material/snack-bar";
import { MatDividerModule } from "@angular/material/divider";
import { AuthService, LoginRequest } from "@sports-ui/auth";
import { GoogleSignInComponent } from "../google-signin/google-signin.component";
import { User } from "@sports-ui/api-types";

@Component({
  selector: "lib-login",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDividerModule,
    GoogleSignInComponent,
  ],
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.css",
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);

  // Form
  loginForm: FormGroup;

  // State
  hidePassword = signal<boolean>(true);

  // Auth service signals
  authenticating = this.authService.authenticating;
  error = this.authService.error;

  constructor() {
    this.loginForm = this.fb.group({
      email: ["", [Validators.required, Validators.email]],
      password: ["", [Validators.required, Validators.minLength(6)]],
      rememberMe: [false],
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      const credentials: LoginRequest = this.loginForm.value;

      this.authService.login(credentials).subscribe({
        next: (response) => {
          this.snackBar.open("Login successful!", "Close", {
            duration: 3000,
            panelClass: ["success-snackbar"],
          });
        },
        error: (error) => {
          this.snackBar.open(error.message || "Login failed", "Close", {
            duration: 5000,
            panelClass: ["error-snackbar"],
          });
        },
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  onRegisterClick() {
    this.router.navigate(["/auth/register"]);
  }

  onForgotPasswordClick() {
    this.router.navigate(["/auth/forgot-password"]);
  }

  togglePasswordVisibility() {
    this.hidePassword.set(!this.hidePassword());
  }

  clearError() {
    this.authService.clearError();
  }

  onGoogleSignInSuccess(user: User) {
    console.log("Google sign-in successful:", user);
    // The GoogleSignInComponent already handles navigation
  }

  onGoogleSignInError(error: string) {
    console.error("Google sign-in error:", error);
    // The GoogleSignInComponent already shows the error
  }

  private markFormGroupTouched() {
    Object.keys(this.loginForm.controls).forEach((key) => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
  }

  // Getters for form controls
  get email() {
    return this.loginForm.get("email");
  }

  get password() {
    return this.loginForm.get("password");
  }

  get rememberMe() {
    return this.loginForm.get("rememberMe");
  }
}
