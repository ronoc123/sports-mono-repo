import { Injectable, inject } from "@angular/core";
import { VoteAccountStore } from "./vote-account.store";
import { VoteAccountApi } from "./vote-account-data-access";
import { firstValueFrom } from "rxjs";
import { ToastService } from "@sports-ui/toast";
import { environment } from "@sports-ui/api-types";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { AuthStore } from "@sports-ui/auth-data-access";

@Injectable({ providedIn: "root" })
export class VoteAccountFacade {
  private store = inject(VoteAccountStore);
  private api = inject(VoteAccountApi);
  private toast = inject(ToastService);
  private authStore = inject(AuthStore);
  account = this.store.account;
  balance = this.store.balance;
  transactions = this.store.transactions;
  loading = this.store.loading;
  error = this.store.error;

  private connection: HubConnection | null = null;

  async load(userId: string, leagueId: string) {
    this.store.setLoading();

    try {
      const account = await firstValueFrom(
        this.api.getVoteAccount(userId, leagueId)
      );

      this.store.setAccount(account?.data ?? null);
    } catch (err: any) {
      this.toast.error(err?.message ?? "Failed to load vote account");
    }
  }

  async vote(
    playerOptionId: string,
    userId: string,
    voteAmount: number,
    leagueId: string
  ) {
    this.store.setLoading();

    try {
      const res = await firstValueFrom(
        this.api.castVote(playerOptionId, userId, voteAmount, leagueId)
      );
      this.toast.success(res?.message ?? "Votes applied!");
      await this.load(userId, leagueId);
    } catch (err: any) {
      this.toast.error(err?.error?.message ?? "Failed to cast vote");
    }
  }

  connectBalance(userId: string, leagueId: string): void {
    if (this.connection) return; // already connected

    const hubUrl = `${environment.apiUrl}${environment.sportsApi.replace(/\/api\/?$/, "")}/hubs/balance`;

    this.connection = new HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => this.authStore.bearer() ?? '' })
      .withAutomaticReconnect()
      .build();

    this.connection.on("BalanceChanged", ({ balance }: { balance: number }) => {
      const current = this.store.account();
      if (current) {
        this.store.setAccount({ ...current, balance });
      } else {
        this.load(userId, leagueId);
      }
    });

    this.connection.start().catch((err) => console.error("BalanceHub connect error:", err));
  }

  disconnectBalance(): void {
    this.connection?.stop();
    this.connection = null;
  }
}
