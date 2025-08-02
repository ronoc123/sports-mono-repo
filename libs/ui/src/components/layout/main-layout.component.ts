import { Component, input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';

import { SidebarComponent } from './sidebar.component';
import { NavbarComponent } from './navbar.component';

export interface NavItem {
  name: string;
  icon?: string;
  route?: string;
  children?: NavItem[];
  permission?: string;
  role?: string;
  roles?: string[];
}

export interface LayoutConfig {
  appTitle: string;
  appLogo?: string;
  showUserMenu: boolean;
  showNotifications: boolean;
  showSearch: boolean;
  sidenavMode: 'over' | 'push' | 'side';
  sidenavOpened: boolean;
  showFooter?: boolean;
}

@Component({
  selector: 'ui-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
    MatTooltipModule,
    SidebarComponent,
    NavbarComponent,
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css',
})
export class MainLayoutComponent {
  private readonly breakpointObserver = inject(BreakpointObserver);

  // Inputs
  readonly navItems = input.required<NavItem[]>();
  readonly config = input<LayoutConfig>({
    appTitle: 'Sports UI',
    showUserMenu: true,
    showNotifications: true,
    showSearch: false,
    sidenavMode: 'side',
    sidenavOpened: true,
  });

  // Permission checker function input
  readonly permissionChecker = input<(item: NavItem) => boolean>(() => true);

  // User data inputs
  readonly currentUser = input<any>(null);
  readonly organizations = input<any[]>([]);
  readonly selectedOrganization = input<any>(null);

  // Responsive breakpoints
  readonly isHandset$ = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(map(result => result.matches));

  readonly isTablet$ = this.breakpointObserver.observe(Breakpoints.Tablet)
    .pipe(map(result => result.matches));

  readonly isDesktop$ = this.breakpointObserver.observe([Breakpoints.Large, Breakpoints.XLarge])
    .pipe(map(result => result.matches));

  // Get responsive sidenav mode
  getSidenavMode(): 'over' | 'push' | 'side' {
    // On mobile, always use 'over' mode
    if (this.breakpointObserver.isMatched(Breakpoints.Handset)) {
      return 'over';
    }
    
    // On tablet, use 'push' mode
    if (this.breakpointObserver.isMatched(Breakpoints.Tablet)) {
      return 'push';
    }
    
    // On desktop, use configured mode or default to 'side'
    return this.config().sidenavMode || 'side';
  }

  // Get responsive sidenav opened state
  getSidenavOpened(): boolean {
    // On mobile, default to closed
    if (this.breakpointObserver.isMatched(Breakpoints.Handset)) {
      return false;
    }
    
    // On larger screens, use configured state
    return this.config().sidenavOpened;
  }

  // Get responsive sidenav width
  getSidenavWidth(): string {
    if (this.breakpointObserver.isMatched(Breakpoints.Handset)) {
      return '280px'; // Wider on mobile for better touch targets
    }
    return '260px'; // Standard width on desktop
  }
}
