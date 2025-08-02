import { Component, inject, input, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { MatTabsModule } from "@angular/material/tabs";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatSelectModule } from "@angular/material/select";
import { MatInputModule } from "@angular/material/input";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatSnackBar, MatSnackBarModule } from "@angular/material/snack-bar";
import { PlayerOptionStore } from "@sports-ui/player-options-data-access";
import { UserStore, VoteService } from "@sports-ui/data-access";
import { VotingPanelComponent } from "@sports-ui/ui";
import {
  PlayerOption,
  PlayerOptionFilter,
  VoteRequest,
} from "@sports-ui/api-types";

@Component({
  selector: "lib-feature-player-option",
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    VotingPanelComponent,
  ],
  templateUrl: "./feature-player-option.html",
  styleUrl: "./feature-player-option.css",
})
export class FeaturePlayerOption implements OnInit {
  readonly playerOptionStore = inject(PlayerOptionStore); // Make public for template access
  private readonly userStore = inject(UserStore);
  private readonly voteService = inject(VoteService);
  private readonly snackBar = inject(MatSnackBar);

  orgId = input<string>("3B60378E-782A-45D5-B6C4-AA7466F8D5FD");

  // Store signals
  playerOptions = this.playerOptionStore.playerOptions;
  loading = this.playerOptionStore.loading;
  error = this.playerOptionStore.error;
  currentUser = this.userStore.currentUser;

  // Local state
  selectedTab = signal<number>(0);
  filterType = signal<string>("all");
  userVotesAvailable = signal<number>(5); // Mock user votes
  userVotes = signal<Set<string>>(new Set()); // Track user votes

  ngOnInit(): void {
    this.loadPlayerOptions();
    this.loadCurrentUser();
    this.loadUserVotes();
  }

  loadPlayerOptions() {
    const filter: PlayerOptionFilter = {
      pageNumber: 1,
      pageSize: 20,
      organizationId: this.orgId(),
      isActive: this.filterType() === "active" ? true : undefined,
      isExpired: this.filterType() === "expired" ? true : undefined,
      sortBy: "votes",
      sortDescending: true,
    };

    this.playerOptionStore.loadPlayerOptions(filter);
  }

  loadCurrentUser() {
    this.userStore.loadCurrentUser();
  }

  loadUserVotes() {
    const user = this.currentUser();
    if (user) {
      this.voteService.getVotesByUser(user.id).subscribe({
        next: (response) => {
          if (response.success) {
            const votedOptionIds = new Set(
              response.data.map((vote) => vote.playerOptionId)
            );
            this.userVotes.set(votedOptionIds);
          }
        },
        error: (error) => {
          console.error("Failed to load user votes:", error);
        },
      });
    }
  }

  onVote(playerOption: PlayerOption) {
    const user = this.currentUser();
    if (!user) {
      this.snackBar.open("Please log in to vote", "Close", { duration: 3000 });
      return;
    }

    if (this.userVotesAvailable() <= 0) {
      this.snackBar.open("No votes available", "Close", { duration: 3000 });
      return;
    }

    const voteRequest: VoteRequest = {
      playerOptionId: playerOption.id,
      organizationId: playerOption.organizationId,
    };

    this.voteService.castVote(voteRequest).subscribe({
      next: (response) => {
        if (response.success) {
          // Update local state
          this.userVotes.update(
            (votes) => new Set([...votes, playerOption.id])
          );
          this.userVotesAvailable.update((count) => count - 1);

          // Update player option vote count locally
          this.playerOptionStore.voteOnPlayerOption(playerOption.id);

          this.snackBar.open("Vote cast successfully!", "Close", {
            duration: 3000,
            panelClass: ["success-snackbar"],
          });
        } else {
          this.snackBar.open(
            response.message || "Failed to cast vote",
            "Close",
            {
              duration: 3000,
              panelClass: ["error-snackbar"],
            }
          );
        }
      },
      error: (error) => {
        this.snackBar.open(error.message || "Failed to cast vote", "Close", {
          duration: 3000,
          panelClass: ["error-snackbar"],
        });
      },
    });
  }

  onRemoveVote(playerOption: PlayerOption) {
    // Find the user's vote for this option
    const user = this.currentUser();
    if (!user) return;

    this.voteService
      .getUserVoteOnPlayerOption(user.id, playerOption.id)
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.voteService.removeVote(response.data.id).subscribe({
              next: (removeResponse) => {
                if (removeResponse.success) {
                  // Update local state
                  this.userVotes.update((votes) => {
                    const newVotes = new Set(votes);
                    newVotes.delete(playerOption.id);
                    return newVotes;
                  });
                  this.userVotesAvailable.update((count) => count + 1);

                  this.snackBar.open("Vote removed successfully!", "Close", {
                    duration: 3000,
                    panelClass: ["success-snackbar"],
                  });
                }
              },
              error: (error) => {
                this.snackBar.open("Failed to remove vote", "Close", {
                  duration: 3000,
                  panelClass: ["error-snackbar"],
                });
              },
            });
          }
        },
      });
  }

  onViewDetails(playerOption: PlayerOption) {
    // TODO: Navigate to player option details page
    console.log("View details for:", playerOption);
  }

  onFilterChange(filterType: string) {
    this.filterType.set(filterType);
    this.loadPlayerOptions();
  }

  onRefresh() {
    this.loadPlayerOptions();
    this.loadUserVotes();
  }

  hasUserVoted(playerOptionId: string): boolean {
    return this.userVotes().has(playerOptionId);
  }

  canUserVote(): boolean {
    return !!this.currentUser() && this.userVotesAvailable() > 0;
  }

  getFilteredPlayerOptions(): PlayerOption[] {
    const options = this.playerOptions();
    switch (this.filterType()) {
      case "trending":
        return options.filter((option) => option.isTrending);
      case "popular":
        return options.filter((option) => option.isPopular);
      case "active":
        return options.filter((option) => option.isActive && !option.isExpired);
      case "expired":
        return options.filter((option) => option.isExpired);
      default:
        return options;
    }
  }

  trackByOptionId(index: number, option: PlayerOption): string {
    return option.id;
  }
}
