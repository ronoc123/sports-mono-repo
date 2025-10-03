import {
  Component,
  ElementRef,
  inject,
  input,
  output,
  signal,
  ViewChild,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatSnackBar, MatSnackBarModule } from "@angular/material/snack-bar";

import { GOOGLE_CLIENT_ID } from "../tokens/google-tokens";
import { GoogleIdentityService } from "@sports-ui/auth-data-access";
import { AuthService } from "@sports-ui/auth-data-access";

@Component({
  selector: "lib-google-signin",
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: "./google-signin.component.html",
  styleUrl: "./google-signin.component.css",
})
export class GoogleSignInComponent {
  private readonly gis = inject(GoogleIdentityService);
  private readonly snack = inject(MatSnackBar);
  private readonly clientId = inject(GOOGLE_CLIENT_ID);
  private readonly auth = inject(AuthService);

  @ViewChild("googleButtonContainer", { static: false })
  googleButtonContainer?: ElementRef<HTMLDivElement>;

  buttonText = input("Sign in with Google");
  showSpinner = input(true);
  signInError = output<string>();

  // simple local UI state
  signingIn = signal(false);

  async ngAfterViewInit() {
    // Initialize GIS once with a credential callback
    await this.gis.init(this.clientId, (jwt) => this.onCredential(jwt));

    // If you want the official button rendered:
    if (this.googleButtonContainer?.nativeElement) {
      this.gis.renderButton(this.googleButtonContainer.nativeElement, {
        type: "standard",
        size: "large",
      });
    }
  }

  /** Called when GIS returns a credential (ID token JWT) */
  private onCredential(idToken: string) {
    this.signingIn.set(true);
    this.auth.loginWithGoogle(idToken).subscribe({
      next: () => {
        this.signingIn.set(true);
        this.snack.open("Logged in with Google!", "Close", {
          duration: 3000,
          panelClass: ["success-snackbar"],
        });
      },
      error: (e) => {
        this.signingIn.set(false);
        const msg = e?.message || "Google login failed";
        this.signInError.emit(msg);
        this.snack.open(msg, "Close", {
          duration: 5000,
          panelClass: ["error-snackbar"],
        });
      },
    });
  }

  onCustomButtonClick() {
    this.gis.prompt();
  }
}
