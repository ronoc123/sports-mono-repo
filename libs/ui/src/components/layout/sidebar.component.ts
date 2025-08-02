import {
  Component,
  effect,
  inject,
  input,
  signal,
  OnInit,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatTreeModule, MatTreeNestedDataSource } from "@angular/material/tree";
import { MatIconModule } from "@angular/material/icon";
import { MatButtonModule } from "@angular/material/button";
import { MatSelectModule } from "@angular/material/select";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatDividerModule } from "@angular/material/divider";
import { MatTooltipModule } from "@angular/material/tooltip";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { NestedTreeControl } from "@angular/cdk/tree";
import { NavigationEnd, Router } from "@angular/router";
import { filter } from "rxjs";

import { NavItem } from './main-layout.component';

@Component({
  selector: "ui-sidebar",
  standalone: true,
  imports: [
    CommonModule,
    MatTreeModule,
    MatIconModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule,
    MatDividerModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: "./sidebar.component.html",
  styleUrl: "./sidebar.component.css",
})
export class SidebarComponent implements OnInit {
  readonly router = inject(Router);

  // Inputs
  readonly navItems = input.required<NavItem[]>();
  readonly appTitle = input<string>('Sports UI');
  readonly appLogo = input<string>('');
  readonly showOrganizationSelector = input<boolean>(false);
  readonly organizations = input<any[]>([]);
  readonly currentUser = input<any>(null);
  readonly selectedOrganization = input<any>(null);
  readonly permissionChecker = input<(item: NavItem) => boolean>(() => true);

  // Tree control
  treeControl = new NestedTreeControl<NavItem>((node) => node.children);
  dataSource = new MatTreeNestedDataSource<NavItem>();

  // Local state
  private readonly currentRoute = signal(this.router.url);
  private readonly organizationDropdownOpen = signal(false);

  constructor() {
    // Update tree data when nav items change
    effect(() => {
      this.dataSource.data = this.navItems();
    });

    // Track route changes
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe((e) => this.currentRoute.set(e.urlAfterRedirects));
  }

  ngOnInit() {
    // Component is now stateless and doesn't need to load data
  }

  // Navigation methods
  navigateTo(route?: string) {
    if (route) {
      this.router.navigate([route]);
    }
  }

  isSelected(route?: string): boolean {
    if (!route) return false;
    return this.currentRoute() === route || this.currentRoute().startsWith(route + '/');
  }

  hasChild = (_: number, node: NavItem) => !!node.children && node.children.length > 0;

  // Organization methods
  toggleOrganizationDropdown() {
    this.organizationDropdownOpen.update(open => !open);
  }

  isOrganizationDropdownOpen(): boolean {
    return this.organizationDropdownOpen();
  }

  getSelectedOrganizationName(): string {
    const selected = this.selectedOrganization();
    return selected?.name || 'Select Organization';
  }

  getSelectedOrganizationLogo(): string {
    const selected = this.selectedOrganization();
    return selected?.logoUrl || this.appLogo() || '/assets/default-org-logo.png';
  }

  selectOrganization(org: any) {
    // Emit event or call callback - this should be handled by parent component
    console.log('Organization selected:', org);
    this.organizationDropdownOpen.set(false);
  }

  // Permission checking
  isNavItemVisible(item: NavItem): boolean {
    return this.permissionChecker()(item);
  }

  // User methods
  isAuthenticated(): boolean {
    return !!this.currentUser();
  }

  getUserDisplayName(): string {
    const user = this.currentUser();
    if (!user) return 'Guest';
    return user.userName || user.email || 'User';
  }

  getUserInitials(): string {
    const user = this.currentUser();
    if (!user) return 'G';
    
    if (user.firstName && user.lastName) {
      return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase();
    } else if (user.userName) {
      return user.userName.substring(0, 2).toUpperCase();
    } else if (user.email) {
      return user.email.substring(0, 2).toUpperCase();
    }
    
    return 'U';
  }

  // Quick actions
  onQuickAction(action: string) {
    switch (action) {
      case 'profile':
        this.navigateTo('/profile');
        break;
      case 'settings':
        this.navigateTo('/settings');
        break;
      case 'logout':
        // This should be handled by parent component
        console.log('Logout requested');
        break;
      default:
        console.log('Unknown action:', action);
    }
  }

  trackByOrgId(_: number, org: any): string {
    return org.id;
  }
}
