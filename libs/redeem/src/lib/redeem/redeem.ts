import { Component, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatTabsModule } from "@angular/material/tabs";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatTableModule } from "@angular/material/table";
import { MatChipsModule } from "@angular/material/chips";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { CodeRedemptionComponent } from "@sports-ui/ui";
import { CodeService } from "@sports-ui/data-access";
import { UserStore } from "@sports-ui/data-access";
import {
  CodeRedemptionResponse,
  RedeemCodeRequest,
  Code,
  ServiceResponse,
} from "@sports-ui/api-types";

@Component({
  selector: "lib-redeem",
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    CodeRedemptionComponent,
  ],
  templateUrl: "./redeem.html",
  styleUrl: "./redeem.css",
})
export class Redeem {
  private readonly codeService = inject(CodeService);
  private readonly userStore = inject(UserStore);

  // Local state
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  redemptionResult = signal<CodeRedemptionResponse | null>(null);
  userCodes = signal<Code[]>([]);
  selectedTabIndex = signal<number>(0);

  // User store signals
  currentUser = this.userStore.currentUser;

  displayedColumns: string[] = [
    "codeValue",
    "organizationName",
    "votes",
    "redeemedAt",
    "status",
  ];

  ngOnInit() {
    this.loadCurrentUser();
    this.loadUserCodes();
  }

  loadCurrentUser() {
    this.userStore.loadCurrentUser();
  }

  loadUserCodes() {
    const user = this.currentUser();
    if (user) {
      this.loading.set(true);
      this.codeService.getCodesByUser(user.id).subscribe({
        next: (response: ServiceResponse<Code[]>) => {
          if (response.success) {
            this.userCodes.set(response.data);
          } else {
            this.error.set(response.message);
          }
          this.loading.set(false);
        },
        error: (error) => {
          this.error.set(error.message || "Failed to load user codes");
          this.loading.set(false);
        },
      });
    }
  }

  onRedeemCode(codeValue: string) {
    const user = this.currentUser();
    if (!user) {
      this.error.set("Please log in to redeem codes");
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const request: RedeemCodeRequest = {
      codeValue,
      userId: user.id,
    };

    this.codeService.redeemCode(request).subscribe({
      next: (response: ServiceResponse<CodeRedemptionResponse>) => {
        this.loading.set(false);
        if (response.success) {
          this.redemptionResult.set(response.data);
          this.loadUserCodes(); // Refresh the codes list
        } else {
          this.error.set(response.message);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.error.set(error.message || "Failed to redeem code");
      },
    });
  }

  getCodeStatusColor(code: Code): string {
    if (code.isRedeemed) return "primary";
    if (code.isExpired) return "warn";
    return "accent";
  }

  getCodeStatusText(code: Code): string {
    if (code.isRedeemed) return "Redeemed";
    if (code.isExpired) return "Expired";
    return "Active";
  }

  clearError() {
    this.error.set(null);
  }

  goToRedeemTab() {
    this.selectedTabIndex.set(0);
  }
}
