import { Component, ViewChild, computed, effect, inject, signal } from "@angular/core";
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
  readonly selectedBundle = this.storeService.selectedBundle;
  readonly organization = this.orgFacade.selectedOrganization;
  readonly voteBalance = this.voteAccountFacade.balance;

  readonly selectedCardId = signal<string | null>(null);
  readonly selectedBundleData = computed(() =>
    this.bundles().find(b => b.id === this.selectedCardId()) ?? null
  );
  readonly showRightPanel = computed(() =>
    this.selectedCardId() !== null || this.checkoutStatus() !== 'idle'
  );

  private readonly bundleColors = ['blue', 'purple', 'green', 'amber'];
  private readonly bundleIcons = ['confirmation_number', 'bolt', 'trending_up', 'workspace_premium'];

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

  bundleColorClass(index: number): string {
    return `bundle-color-${this.bundleColors[index % this.bundleColors.length]}`;
  }

  bundleIcon(index: number): string {
    return this.bundleIcons[index % this.bundleIcons.length];
  }

  selectBundle(bundleId: string): void {
    if (this.checkoutStatus() !== 'idle') {
      this.storeService.clearCheckout();
    }
    this.selectedCardId.set(bundleId);
  }

  async onStripeCheckout(): Promise<void> {
    const id = this.selectedCardId();
    if (!id) return;
    await this.onBuy(id);
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
    this.selectedCardId.set(null);
  }

  formatPrice(amount: number, currency: string): string {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency,
    }).format(amount);
  }
}
