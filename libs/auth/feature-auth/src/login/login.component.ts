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
import { GoogleSignInComponent } from "../google-signin/google-signin.component";
import { AuthFacade } from "@sports-ui/auth-data-access";

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
  facade = inject(AuthFacade);
  private snack = inject(MatSnackBar);
  private router = inject(Router);

  async onGoogleCredential(jwt: string) {
    try {
      await this.facade.signInWithGoogle(jwt);
      this.snack.open("Logged in!", "Close", { duration: 2000 });
      await this.router.navigateByUrl("/");
    } catch {
      this.snack.open(this.facade.error() ?? "Login failed", "Close", {
        duration: 4000,
      });
    }
  }

  onGoogleError(msg: any) {
    this.snack.open(msg, "Close", { duration: 4000 });
  }
}
