import { Component, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Router } from "@angular/router";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatSnackBar, MatSnackBarModule } from "@angular/material/snack-bar";
import { GoogleSignInComponent } from "../google-signin/google-signin.component";
import { AuthFacade } from "@sports-ui/auth-data-access";
import { ToastService } from "@sports-ui/toast";

@Component({
  selector: "lib-login",
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    GoogleSignInComponent,
  ],
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.css",
})
export class LoginComponent {
  facade = inject(AuthFacade);
  private snack = inject(MatSnackBar);
  private readonly toastService = inject(ToastService);
  private router = inject(Router);

  mode = signal<"login" | "register">("login");

  async onGoogleCredential(jwt: string) {
    try {
      await this.facade.signInWithGoogle(jwt);
      this.toastService.success("Logged in successfully!");
      await this.router.navigateByUrl("/league");
    } catch (error: any) {
      this.toastService.error(error?.message ?? "Login failed");
    }
  }

  onGoogleError(msg: any) {
    this.toastService.error(msg ?? "Login failed");
  }
}
