import { Component, effect, input, output, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { OrganizationBadgeComponent } from "../organization-badge/organization-badge";

export interface LayoutConfig {
  appTitle: string;
  appLogo?: string;
  showUserMenu: boolean;
  showNotifications: boolean;
  showSearch: boolean;
  sidenavMode: "over" | "push" | "side";
  sidenavOpened: boolean;
  showFooter?: boolean;
}

export interface NavItem {
  name: string;
  icon?: string;
  route?: string;
  children?: NavItem[];
  permission?: string;
  role?: string;
  roles?: string[];
}

@Component({
  selector: "lib-ui-sidebar",
  imports: [CommonModule, OrganizationBadgeComponent],
  standalone: true,
  templateUrl: "./sidebar.component.html",
  styleUrls: ["./sidebar.component.css"],
})
export class SidebarComponent {
  readonly navItems    = input.required<NavItem[]>();
  readonly currentRoute = input<string>("");
  readonly organizations = input<any[]>([]);
  readonly selectedOrganization = input<any>(null);
  readonly user = input<any>(null);

  // eslint-disable-next-line @angular-eslint/no-output-on-prefix
  readonly onOrgSelected  = output<any>();
  // eslint-disable-next-line @angular-eslint/no-output-on-prefix
  readonly onNavSelected  = output<NavItem>();

  expandedItems = signal<string[]>([]);

  isExpanded(name: string): boolean {
    return this.expandedItems().includes(name);
  }

  toggleExpand(name: string) {
    this.expandedItems.update((items) =>
      items.includes(name) ? items.filter((i) => i !== name) : [...items, name]
    );
  }

  isActive(node: NavItem): boolean {
    if (!node.route) return false;
    return this.currentRoute().endsWith("/" + node.route) ||
           this.currentRoute() === node.route;
  }
}
