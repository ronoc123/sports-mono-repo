import { inject, Injectable } from "@angular/core";
import { RedeemApi } from "./redeem.api";
import { RedeemStore } from "./redeem.store";
import { firstValueFrom } from "rxjs";
import { ToastService } from "@sports-ui/toast";

@Injectable({ providedIn: "root" })
export class RedeemFacade {
  private api = inject(RedeemApi);
  private store = inject(RedeemStore);
  private toast = inject(ToastService);
  async redeemReward(userId: string, rewardId: string) {
    this.store.setLoading();

    try {
      await firstValueFrom(this.api.redeemReward(userId, rewardId));
      this.toast.success("Reward redeemed successfully");
    } catch (e: any) {
      this.toast.error(e?.error?.message || "Unable to load player options");
    }
  }
}
