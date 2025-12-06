import {
  Component,
  ElementRef,
  inject,
  input,
  output,
  ViewChild,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatSnackBarModule } from "@angular/material/snack-bar";
import { GOOGLE_CLIENT_ID } from "../tokens/google-tokens";
import { GoogleIdentityService } from "@sports-ui/auth-data-access";

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
  private readonly clientId = inject(GOOGLE_CLIENT_ID);

  @ViewChild("googleButtonContainer", { static: false })
  googleButtonContainer?: ElementRef<HTMLDivElement>;

  // purely presentational API
  signingIn = input(false);
  credential = output<string>();

  async ngAfterViewInit() {
    try {
      await this.gis.init(this.clientId, (jwt) => this.credential.emit(jwt));
      if (this.googleButtonContainer?.nativeElement) {
        this.gis.renderButton(this.googleButtonContainer.nativeElement, {
          type: "standard",
          size: "large",
        });
      }
      // eslint-disable-next-line no-empty
    } catch (e: any) {}
  }

  // optional manual prompt if you add a custom button
  prompt() {
    this.gis.prompt();
  }
}
