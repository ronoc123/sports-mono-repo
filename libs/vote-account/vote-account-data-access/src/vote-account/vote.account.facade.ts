import { Injectable, inject } from "@angular/core";
import { VoteAccountStore } from "./vote-account.store";
import { VoteAccountApi } from "./vote-account-data-access";
import { firstValueFrom } from "rxjs";

@Injectable({ providedIn: "root" })
export class VoteAccountFacade {
  private store = inject(VoteAccountStore);
  private api = inject(VoteAccountApi);

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
    } catch (err: any) {
      this.store.setError(err?.message ?? "Failed to load vote account");
    }
  }
}
