import { Component, input } from "@angular/core";
import { environment } from "@sports-ui/api-types";

@Component({
  selector: "lib-google-signin",
  standalone: true,
  templateUrl: "./google-signin.component.html",
  styleUrl: "./google-signin.component.css",
})
export class GoogleSignInComponent {
  signingIn = input(false);

  signInWithGoogle() {
    const returnUrl = encodeURIComponent(`${window.location.origin}/auth/callback`);
    window.location.href = `${environment.apiUrl}${environment.identityApi}auth/google-login?returnUrl=${returnUrl}`;
  }
}
