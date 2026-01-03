import { Injectable, inject } from "@angular/core";
import { VoteAccountStore } from "./vote-account.store";
import { VoteAccountApi } from "./vote-account-data-access";
import { firstValueFrom } from "rxjs";
import { ToastService } from "@sports-ui/toast";

@Injectable({ providedIn: "root" })
export class VoteAccountFacade {
  private store = inject(VoteAccountStore);
  private api = inject(VoteAccountApi);
  private toast = inject(ToastService);
  account = this.store.account;
  balance = this.store.balance;
  transactions = this.store.transactions;
  loading = this.store.loading;
  error = this.store.error;

  async load(userId: string, orgId: string) {
    this.store.setLoading();

    try {
      const account = await firstValueFrom(
        this.api.getVoteAccount(userId, orgId)
      );

      this.store.setAccount(account?.data ?? null);
      // this.toast.success(account?.message ?? "Vote account loaded");
    } catch (err: any) {
      this.toast.error(err?.message ?? "Failed to load vote account");
    }
  }

  async vote(
    playerOptionId: string,
    userId: string,
    voteAmount: number,
    organizationId: string
  ) {
    this.store.setLoading();

    try {
      const res = await firstValueFrom(
        this.api.castVote(playerOptionId, userId, voteAmount)
      );
      this.toast.success(res?.message ?? "Votes applied!");
      await this.load(userId, organizationId);
    } catch (err: any) {
      this.toast.error(err?.error?.message ?? "Failed to cast vote");
    }
  }
}
