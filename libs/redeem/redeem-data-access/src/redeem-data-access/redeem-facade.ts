import { computed, inject, Injectable } from "@angular/core";
import { RedeemApi } from "./redeem.api";
import { RedeemStore } from "./redeem.store";
import { firstValueFrom } from "rxjs";
import { ToastService } from "@sports-ui/toast";

@Injectable({ providedIn: "root" })
export class RedeemFacade {
  private api = inject(RedeemApi);
  private store = inject(RedeemStore);
  private toast = inject(ToastService);

  readonly loading = computed(() => this.store.status() === "loading");
  readonly error = computed(() => this.store.error() ?? null);

  async redeemReward(userId: string, promoCode: string, leagueId: string) {
    this.store.setLoading();

    try {
      await firstValueFrom(this.api.redeemReward(userId, promoCode, leagueId));
      this.store.setSuccess();
      this.toast.success("Reward redeemed successfully");
    } catch (e: any) {
      const msg =
        e?.error?.message || "Failed to redeem code. Please try again.";
      this.store.setError(msg);
      this.toast.error(msg);
    }
  }
}
