import { Component, inject, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatChipsModule } from "@angular/material/chips";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatSnackBar, MatSnackBarModule } from "@angular/material/snack-bar";
import { PlayerOptionStore } from "../../player-option-data-access/src/lib/player-option-data-access/player-option-data-access";

@Component({
  selector: "lib-feature-player-option",
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatProgressBarModule,
    MatSnackBarModule,
  ],
  templateUrl: "./feature-player-option.html",
  styleUrl: "./feature-player-option.css",
})
export class FeaturePlayerOption implements OnInit {
  readonly playerOptionStore = inject(PlayerOptionStore);
  private readonly snackBar = inject(MatSnackBar);

  // Store signals
  playerOptions = this.playerOptionStore.playerOptions;
  isLoading = this.playerOptionStore.isLoading;
  error = this.playerOptionStore.error;

  ngOnInit(): void {
    this.loadPlayerOptions();
  }

  loadPlayerOptions() {
    // Load player options using the store
    this.playerOptionStore.loadPlayerOptions("user-123");
  }

  onVote(playerOptionId: string) {
    const voteRequest = {
      playerOptionId,
      choiceId: "choice-1", // Mock choice ID
      userId: "user-123",
    };
    this.playerOptionStore.voteOnOption(voteRequest);
    this.snackBar.open("Vote cast successfully!", "Close", { duration: 3000 });
  }

  onRefresh() {
    this.loadPlayerOptions();
  }
}
