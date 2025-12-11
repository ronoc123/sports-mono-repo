import { Component, effect, input, output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { OrganizationBadgeComponent } from "../organization-badge/organization-badge";
import { MatIcon } from "@angular/material/icon";
import {
  MatNestedTreeNode,
  MatTree,
  MatTreeModule,
  MatTreeNestedDataSource,
  MatTreeNode,
} from "@angular/material/tree";
import { MatDivider } from "@angular/material/divider";
import { NestedTreeControl } from "@angular/cdk/tree";

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
  imports: [
    CommonModule,
    OrganizationBadgeComponent,
    MatIcon,
    MatNestedTreeNode,
    MatTree,
    MatTreeNode,
    MatDivider,
    MatTreeModule,
  ],
  standalone: true,
  templateUrl: "./sidebar.component.html",
  styleUrls: ["./sidebar.component.css"],
})
export class SidebarComponent {
  readonly navItems = input.required<NavItem[]>();
  readonly currentRoute = input<string>("");
  readonly organizations = input<any[]>([]);
  readonly selectedOrganization = input<any>(null);

  // eslint-disable-next-line @angular-eslint/no-output-on-prefix
  readonly onOrgSelected = output<any>();
  // eslint-disable-next-line @angular-eslint/no-output-on-prefix
  readonly onNavSelected = output<NavItem>();

  treeControl = new NestedTreeControl<NavItem>((node) => node.children);
  dataSource = new MatTreeNestedDataSource<NavItem>();

  hasChild = (_: number, node: NavItem) =>
    !!node.children && node.children.length > 0;

  isLeaf = (_: number, node: NavItem) =>
    !node.children || node.children.length === 0;

  constructor() {
    effect(() => {
      this.dataSource.data = this.navItems();
    });
  }
}
