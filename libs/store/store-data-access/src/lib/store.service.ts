import { Injectable, computed, inject } from "@angular/core";
import { firstValueFrom } from "rxjs";
import { BundleCatalogStore } from "./store.store";
import { StoreApi } from "./store.api";
import { ToastService } from "@sports-ui/toast";

@Injectable({ providedIn: "root" })
export class StoreService {
  private readonly store = inject(BundleCatalogStore);
  private readonly api = inject(StoreApi);
  private readonly toast = inject(ToastService);

  // ── Catalog signals ──
  readonly bundles = this.store.bundles;
  readonly status = this.store.status;
  readonly error = this.store.error;

  // ── Checkout signals ──
  readonly checkoutStatus = this.store.checkoutStatus;
  readonly clientSecret = this.store.clientSecret;
  readonly checkoutError = this.store.checkoutError;
  readonly selectedBundleId = this.store.selectedBundleId;
  readonly selectedBundle = computed(() =>
    this.bundles().find((b) => b.id === this.selectedBundleId())
  );

  async loadBundles(organizationId: string): Promise<void> {
    this.store.setLoading();
    try {
      const res = await firstValueFrom(this.api.getBundles(organizationId));
      this.store.setBundles(res.data ?? []);
    } catch (e: any) {
      const msg = e?.error?.message ?? "Unable to load vote bundles.";
      this.store.setError(msg);
      this.toast.error(msg);
    }
  }

  async initiateCheckout(
    userId: string,
    organizationId: string,
    productId: string
  ): Promise<void> {
    this.store.setCheckoutInitiating(productId);
    try {
      const res = await firstValueFrom(
        this.api.createPurchase({ userId, organizationId, productId })
      );
      if (res.data?.clientSecret) {
        this.store.setClientSecret(res.data.clientSecret);
      } else {
        throw new Error("No client secret returned.");
      }
    } catch (e: any) {
      const msg = e?.error?.message ?? "Unable to initiate checkout.";
      this.store.setCheckoutError(msg);
      this.toast.error(msg);
    }
  }

  async retryCheckout(userId: string, organizationId: string): Promise<void> {
    const bundleId = this.selectedBundleId();
    if (!bundleId) return;
    await this.initiateCheckout(userId, organizationId, bundleId);
  }

  setCheckoutError(msg: string): void {
    this.store.setCheckoutError(msg);
  }

  setCheckoutProcessing(): void {
    this.store.setCheckoutProcessing();
  }

  markCheckoutSucceeded(): void {
    this.store.setCheckoutSucceeded();
  }

  clearCheckout(): void {
    this.store.clearCheckout();
  }
}
