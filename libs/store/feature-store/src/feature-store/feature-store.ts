import { Component, ViewChild, effect, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { StoreService } from "@sports-ui/store-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";
import { AuthFacade } from "@sports-ui/auth-data-access";
import { VoteAccountFacade } from "@sports-ui/vote-account-data-access";
import { CheckoutModal } from "./checkout-modal/checkout-modal";

@Component({
  selector: "lib-feature-store",
  standalone: true,
  imports: [CommonModule, CheckoutModal],
  templateUrl: "./feature-store.html",
  styleUrl: "./feature-store.css",
})
export class FeatureStore {
  private readonly orgFacade = inject(OrganizationFeatureService);
  private readonly authFacade = inject(AuthFacade);
  private readonly voteAccountFacade = inject(VoteAccountFacade);
  readonly storeService = inject(StoreService);

  @ViewChild(CheckoutModal) checkoutModal?: CheckoutModal;

  readonly bundles = this.storeService.bundles;
  readonly status = this.storeService.status;
  readonly checkoutStatus = this.storeService.checkoutStatus;
  readonly organization = this.orgFacade.selectedOrganization;

  constructor() {
    // Load bundles when org changes
    effect(() => {
      const org = this.organization();
      if (!org) return;
      this.storeService.loadBundles(org.id);
    });

    // Forward clientSecret changes into the modal (initial mount + retry remount)
    // effect(() => {
    //   const secret = this.storeService.clientSecret();
    //   this.checkoutModal?.onClientSecretChanged(secret);
    // });

    // Story 3.3: Refresh vote balance when payment succeeds (no manual reload required)
    effect(() => {
      if (this.checkoutStatus() !== "succeeded") return;
      const org = this.organization();
      const user = this.authFacade.user();
      if (org && user) {
        this.voteAccountFacade.load(user.id, org.id);
      }
    });
  }

  async onBuy(productId: string): Promise<void> {
    const org = this.organization();
    const user = this.authFacade.user();
    if (!org || !user) return;
    await this.storeService.initiateCheckout(user.id, org.id, productId);
  }

  async onRetryCheckout(): Promise<void> {
    const org = this.organization();
    const user = this.authFacade.user();
    if (!org || !user) return;
    await this.storeService.retryCheckout(user.id, org.id);
  }

  onCheckoutClosed(): void {
    this.storeService.clearCheckout();
  }

  formatPrice(amount: number, currency: string): string {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency,
    }).format(amount);
  }
}
