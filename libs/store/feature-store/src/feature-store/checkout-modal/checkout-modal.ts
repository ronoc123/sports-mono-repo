import {
  Component,
  Output,
  EventEmitter,
  ElementRef,
  ViewChild,
  AfterViewInit,
  OnDestroy,
  ChangeDetectorRef,
  inject,
  computed,
  effect,
  Input,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import {
  loadStripe,
  Stripe,
  StripeElements,
  StripePaymentElement,
} from "@stripe/stripe-js";
import { environment } from "@sports-ui/api-types";
import { StoreService } from "@sports-ui/store-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";

type MountStatus = "idle" | "mounting" | "ready";

@Component({
  selector: "app-checkout-modal",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./checkout-modal.html",
  styleUrl: "./checkout-modal.css",
})
export class CheckoutModal implements AfterViewInit, OnDestroy {
  @Input() clientSecret: string | null = null;
  @Input() status: string | null = null;
  @Output() closed = new EventEmitter<void>();
  @Output() retryRequested = new EventEmitter<void>();

  // Always in DOM — never inside *ngIf — so ViewChild always resolves
  @ViewChild("paymentElement") paymentElementRef!: ElementRef<HTMLDivElement>;

  private readonly storeService = inject(StoreService);
  private readonly orgFacade = inject(OrganizationFeatureService);
  private readonly cd = inject(ChangeDetectorRef);

  // ── Store-driven signals ──
  readonly checkoutStatus = this.storeService.checkoutStatus;
  readonly checkoutError = this.storeService.checkoutError; // AC5 — from store signal
  readonly selectedBundle = this.storeService.selectedBundle;
  readonly orgName = computed(
    () => this.orgFacade.selectedOrganization()?.name ?? "your organization"
  );

  mountStatus: MountStatus = "idle";

  private stripe: Stripe | null = null;
  private elements: StripeElements | null = null;
  private paymentEl: StripePaymentElement | null = null;
  private viewReady = false;

  constructor() {
    // Watch clientSecret directly — no parent bridge needed, no @ViewChild race
    effect(() => {
      const secret = this.storeService.clientSecret();
      if (!this.viewReady) return;

      if (secret) {
        this.doMount(secret);
      } else {
        this.doUnmount();
      }
    });
  }

  ngOnChanges(): void {
    if (!this.viewReady) return;

    if (this.clientSecret) {
      this.doMount(this.clientSecret);
    } else {
      this.doUnmount();
    }
  }

  ngAfterViewInit(): void {
    this.viewReady = true;

    if (this.clientSecret) {
      this.doMount(this.clientSecret);
    }
  }

  ngOnDestroy(): void {
    this.doUnmount();
  }

  private async doMount(clientSecret: string): Promise<void> {
    this.doUnmount();
    this.mountStatus = "mounting";
    this.cd.markForCheck();

    try {
      if (!this.stripe) {
        this.stripe = await loadStripe(environment.stripePublicKey);
      }

      if (!this.stripe) {
        this.storeService.setCheckoutError(
          "Failed to load payment processor. Please refresh and try again."
        );
        this.mountStatus = "idle";
        this.cd.markForCheck();
        return;
      }

      this.elements = this.stripe.elements({ clientSecret });
      this.paymentEl = this.elements.create("payment");
      this.paymentEl.mount(this.paymentElementRef.nativeElement);

      this.paymentEl.on("ready", () => {
        this.mountStatus = "ready";
        this.cd.markForCheck();
      });
    } catch (err: any) {
      this.storeService.setCheckoutError(
        err?.message ?? "Failed to initialise payment form."
      );
      this.mountStatus = "idle";
      this.cd.markForCheck();
    }
  }

  private doUnmount(): void {
    this.paymentEl?.unmount();
    this.paymentEl?.destroy();
    this.paymentEl = null;
    this.elements = null;
    this.mountStatus = "idle";
  }

  async pay(): Promise<void> {
    if (!this.stripe || !this.elements || this.mountStatus !== "ready") return;

    this.storeService.setCheckoutProcessing();

    try {
      const { error } = await this.stripe.confirmPayment({
        elements: this.elements,
        confirmParams: { return_url: window.location.href },
        redirect: "if_required",
      });

      if (error) {
        this.storeService.setCheckoutError(
          error.message ?? "Payment failed. Please try again."
        );
      } else {
        this.storeService.markCheckoutSucceeded();
      }
    } catch (err: any) {
      this.storeService.setCheckoutError(
        err?.message ?? "An unexpected error occurred."
      );
    }

    this.cd.markForCheck();
  }

  isPaymentFormVisible(): boolean {
    const s = this.checkoutStatus();
    return s === "ready" || s === "processing";
  }

  onRetry(): void {
    this.retryRequested.emit();
  }

  close(): void {
    this.closed.emit();
  }
}
