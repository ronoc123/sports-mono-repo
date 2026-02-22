import { computed, inject, Injectable } from "@angular/core";
import { firstValueFrom } from "rxjs";
import { SendVotesApi } from "./send-votes.api";
import { SendVotesStore } from "./send-votes.store";
import { ToastService } from "@sports-ui/toast";

@Injectable({ providedIn: "root" })
export class SendVotesFacade {
  private api = inject(SendVotesApi);
  private store = inject(SendVotesStore);
  private toast = inject(ToastService);

  readonly users = this.store.users;
  readonly loading = computed(() => this.store.status() === "loading");
  readonly error = computed(() => this.store.error() || null);

  async loadUsers(): Promise<void> {
    this.store.setLoading();
    try {
      const users = await firstValueFrom(this.api.getUsers());
      this.store.setUsers(users);
    } catch (e: any) {
      const msg = e?.error?.message || "Failed to load users.";
      this.store.setError(msg);
      this.toast.error(msg);
    }
  }

  async sendVotes(userId: string, amount: number, orgId: string): Promise<void> {
    this.store.setLoading();
    try {
      await firstValueFrom(
        this.api.sendVotes({ UserId: userId, Amount: amount, OrganizationId: orgId })
      );
      this.store.setSuccess();
      this.toast.success("Votes sent successfully");
    } catch (e: any) {
      const msg = e?.error?.message || "Failed to send votes. Please try again.";
      this.store.setError(msg);
      this.toast.error(msg);
      throw e;
    }
  }
}
