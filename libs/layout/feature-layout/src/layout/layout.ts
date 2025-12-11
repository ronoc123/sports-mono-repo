import { Component, OnInit, ViewChild, inject, input } from "@angular/core";
import { Router, RouterOutlet } from "@angular/router";
import { MatSidenav } from "@angular/material/sidenav";

import { AuthFacade } from "@sports-ui/auth-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";

import {
  NavItem,
  LayoutConfig,
  NavbarComponent,
  SidebarComponent,
} from "@sports-ui/ui";

@Component({
  selector: "lib-feature-layout",
  standalone: true,
  imports: [NavbarComponent, SidebarComponent, RouterOutlet],
  templateUrl: "./layout.html",
  styleUrls: ["./layout.css"],
})
export class LayoutFeatureComponent implements OnInit {
  protected router = inject(Router);
  private authFacade = inject(AuthFacade);
  private orgFacade = inject(OrganizationFeatureService);
  user = this.authFacade.user;
  organizations = this.orgFacade.organizations;
  selectedOrganization = this.orgFacade.selectedOrganization;
  @ViewChild("drawer") drawer!: MatSidenav;
  readonly navItems = input.required<NavItem[]>();

  // Layout configuration
  layoutConfig: LayoutConfig = {
    appTitle: "Sportify",
    showUserMenu: true,
    showNotifications: true,
    showSearch: false,
    sidenavMode: "side",
    sidenavOpened: true,
    showFooter: false,
  };

  ngOnInit() {
    this.orgFacade.loadOrganizations();
  }

  // Event handlers from UI
  onToggleSidebar() {
    this.drawer.toggle();
  }

  onLogout() {
    this.authFacade.logout();
  }

  onOpenProfile() {
    this.router.navigate(["/profile"]);
  }

  onOpenSettings() {
    this.router.navigate(["/settings"]);
  }

  onSelectOrg(org: any) {
    this.orgFacade.selectOrganization(org);
  }

  onNavItemClick(item: NavItem) {
    if (item.route) {
      this.router.navigate([item.route]);
    }
  }
}
