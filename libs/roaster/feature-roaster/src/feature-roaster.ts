import { Component, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { OffensiveLineup } from "./offensive-lineup/offensive-lineup";
import { RoasterStore } from "@sports-ui/roaster-data-access";
import { DefensiveLineup } from "./defensive-lineup/defensive-lineup";
import { MatIconModule } from "@angular/material/icon";

@Component({
  selector: "lib-feature-roaster",
  imports: [CommonModule, OffensiveLineup, DefensiveLineup, MatIconModule],
  standalone: true,
  providers: [RoasterStore],
  templateUrl: "./feature-roaster.html",
  styleUrl: "./feature-roaster.css",
})
export class FeatureRoaster {
  private readonly roasterStore = inject(RoasterStore);
  offense = this.roasterStore.offense;
  defense = this.roasterStore.defense;
}
