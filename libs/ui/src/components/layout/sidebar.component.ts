import {
  Component,
  effect,
  inject,
  input,
  signal,
  OnInit,
  computed,
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

import { NavItem } from "./main-layout.component";
import { AuthStore } from "@sports-ui/auth-data-access";

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
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
export class SidebarComponent {
  readonly router = inject(Router);
  protected userStore = inject(AuthStore);
  // Inputs
  readonly navItems = input.required<NavItem[]>();
  currentUser = computed(() => this.userStore.user());

  // Tree control
  treeControl = new NestedTreeControl<NavItem>((node) => node.children);
  dataSource = new MatTreeNestedDataSource<NavItem>();

  hasChild = (_: number, node: NavItem) =>
    !!node.children && node.children.length > 0;
  isLeaf = (_: number, node: NavItem) =>
    !node.children || node.children.length === 0;

  // Local state
  private readonly currentRoute = signal(this.router.url);

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
  // Navigation methods
  navigateTo(route?: string) {
    if (route) {
      this.router.navigate([route]);
    }
  }

  isSelected(route?: string): boolean {
    if (!route) return false;
    return (
      this.currentRoute() === route ||
      this.currentRoute().startsWith(route + "/")
    );
  }

  // Quick actions
  onQuickAction(action: string) {
    switch (action) {
      case "profile":
        this.navigateTo("/profile");
        break;
      case "settings":
        this.navigateTo("/settings");
        break;
      case "logout":
        // This should be handled by parent component
        console.log("Logout requested");
        break;
      default:
        console.log("Unknown action:", action);
    }
  }
}
