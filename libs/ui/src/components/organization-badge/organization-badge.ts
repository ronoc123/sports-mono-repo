import { Component, input, output, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { OrganizationDto } from "@sports-ui/organization-data-access";

@Component({
  selector: "lib-organization-badge",
  imports: [CommonModule],
  standalone: true,
  templateUrl: "./organization-badge.html",
  styleUrls: ["./organization-badge.css"],
})
export class OrganizationBadgeComponent {
  // ----------- INPUTS -----------
  organizations = input<OrganizationDto[]>([]);
  selectedOrg = input<OrganizationDto | null>(null);

  // ----------- OUTPUTS -----------
  selectOrganization = output<OrganizationDto>();

  dropdownOpen = signal(false);

  toggleDropdown() {
    this.dropdownOpen.update((v) => !v);
  }

  chooseOrg(org: OrganizationDto) {
    this.selectOrganization.emit(org);
    this.dropdownOpen.set(false);
  }
}
