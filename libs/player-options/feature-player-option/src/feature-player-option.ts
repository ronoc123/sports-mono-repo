import { Component, inject, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatChipsModule } from "@angular/material/chips";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatSnackBarModule } from "@angular/material/snack-bar";
import {
  PlayerOptionDto,
  PlayerOptionFeatureService,
} from "@sports-ui/player-options-data-access";
import { PlayerOptionCardComponent } from "@sports-ui/ui";
import { VoteAccountFacade } from "@sports-ui/vote-account-data-access";
import { AuthFacade } from "@sports-ui/auth-data-access";

import { UiInput } from "@sports-ui/ui";

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
    PlayerOptionCardComponent,
    UiInput,
  ],
  templateUrl: "./feature-player-option.html",
  styleUrl: "./feature-player-option.css",
})
export class FeaturePlayerOption implements OnInit {
  feature = inject(PlayerOptionFeatureService);
  selected = inject(VoteAccountFacade);
  authFacade = inject(AuthFacade);
  organization = this.feature.selectedOrganization;
  options = this.feature.options;
  loading = this.feature.status;
  error = this.feature.error;

  ngOnInit() {
    this.feature.loadPlayerOptions({
      organizationId: this.organization()?.id ?? "",
    });
  }

  onSelectOption(option: PlayerOptionDto) {
    this.selected.vote(
      option.id,
      this.authFacade.user()?.id ?? "",
      1,
      this.organization()?.id ?? ""
    );
  }
}
