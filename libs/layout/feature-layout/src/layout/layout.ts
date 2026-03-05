import {
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
  effect,
  inject,
  input,
  signal,
} from "@angular/core";
import { Router, RouterOutlet } from "@angular/router";
import { MatSidenav } from "@angular/material/sidenav";

import { AuthFacade } from "@sports-ui/auth-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";
import { ThemeService } from "@sports-ui/ui-theme-data-access";
import { ToastFeature } from "@sports-ui/toast-feature";

import {
  NavItem,
  LayoutConfig,
  NavbarComponent,
  SidebarComponent,
} from "@sports-ui/ui";
import { NotificationFacade } from "@sports-ui/notification-data-access";
import { VoteAccountFacade } from "@sports-ui/vote-account-data-access";
import { LeaguesFacade } from "@sports-ui/league-data-access";

@Component({
  selector: "lib-feature-layout",
  standalone: true,
  imports: [NavbarComponent, SidebarComponent, RouterOutlet, ToastFeature],
  templateUrl: "./layout.html",
  styleUrls: ["./layout.css"],
})
export class LayoutFeatureComponent implements OnInit, OnDestroy {
  protected router = inject(Router);
  private authFacade = inject(AuthFacade);
  private orgFacade = inject(OrganizationFeatureService);
  private theme = inject(ThemeService);
  private notificationFacade = inject(NotificationFacade);
  private voteAccountFacade = inject(VoteAccountFacade);
  private leagueFacade = inject(LeaguesFacade);
  user = this.authFacade.user;
  organizations = this.orgFacade.organizations;
  selectedOrganization = this.orgFacade.selectedOrganization;
  readonly pointsBalance = this.voteAccountFacade.balance;
  @ViewChild("drawer") drawer!: MatSidenav;
  readonly navItems = input.required<NavItem[]>();
  mode = signal<"light" | "dark">("light");
  readonly notifications = this.notificationFacade.notifications;
  readonly status = this.notificationFacade.status;

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

  constructor() {
    effect(() => {
      const currentMode = this.mode();
      this.theme.setMode(currentMode);
    });

    effect(() => {
      const org = this.selectedOrganization();

      if (org) {
        this.theme.applyOrgTheme(org);
      }
    });

    // Notifications effect
    effect(() => {
      const user = this.user();

      if (!user) return;

      this.notificationFacade.loadNotifications({
        userId: user.id,
        pageNumber: 1,
        pageSize: 50,
      });
    });

    // Points balance effect — load whenever user + league are both available,
    // and connect to real-time balance hub
    effect(() => {
      const user = this.user();
      const leagueId = this.leagueFacade.selectedLeagueId();
      if (!user || !leagueId) return;
      this.voteAccountFacade.load(user.id, leagueId);
      this.voteAccountFacade.connectBalance(user.id, leagueId);
    });
  }

  ngOnInit() {
    const leagueId = this.leagueFacade.selectedLeagueId() || "";
    this.orgFacade.loadOrganizations({
      leagueId: leagueId || "698200CB-7255-4C57-8475-52D5385860D7",
    });
  }

  ngOnDestroy() {
    this.voteAccountFacade.disconnectBalance();
  }

  // Event handlers from UI
  onToggleSidebar() {
    this.drawer.toggle();
  }

  onLogout() {
    this.voteAccountFacade.disconnectBalance();
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
    const org = this.selectedOrganization();

    if (!item.route || !org) return;

    this.router.navigate([org.id, ...item.route.split('/')]);
  }

  onToggleTheme() {
    this.mode.update((prev) => (prev === "light" ? "dark" : "light"));
  }

  onBuyVotes() {
    const org = this.selectedOrganization();
    if (!org) return;
    this.router.navigate([org.id, "store"]);
  }

  onMarkRead(id: string) {
    this.notificationFacade.markAsRead(id);
  }
}
