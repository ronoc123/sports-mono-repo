import { Component, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { GoogleSignInComponent } from "../google-signin/google-signin.component";
import { AuthFacade } from "@sports-ui/auth-data-access";
import { ToastService } from "@sports-ui/toast";

@Component({
  selector: "lib-login",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    GoogleSignInComponent,
  ],
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.css",
})
export class LoginComponent {
  facade = inject(AuthFacade);
  private router = inject(Router);
  private toast = inject(ToastService);

  mode = signal<"login" | "register">("login");

  // form fields
  email = signal("");
  password = signal("");
  confirmPassword = signal("");
  firstName = signal("");

  async onSubmit() {
    if (this.mode() === "login") {
      try {
        await this.facade.login(this.email(), this.password());
        this.router.navigateByUrl("/league");
      } catch (e: any) {
        this.toast.error(e?.message ?? "Login failed");
      }
    } else {
      if (this.password() !== this.confirmPassword()) {
        this.toast.error("Passwords do not match");
        return;
      }
      try {
        await this.facade.register(
          this.email(),
          this.password(),
          this.confirmPassword(),
          this.firstName() || undefined
        );
        this.router.navigateByUrl("/league");
      } catch (e: any) {
        this.toast.error(e?.message ?? "Registration failed");
      }
    }
  }
}
