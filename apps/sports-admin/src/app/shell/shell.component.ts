import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MainLayoutComponent, NavItem } from '@sports-ui/ui';
import { AuthService } from '@sports-ui/feature-auth';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MainLayoutComponent,
  ],
  template: `
    <ui-main-layout 
      [navItems]="navItems"
      [config]="layoutConfig"
      [permissionChecker]="checkPermission"
      [currentUser]="authService.currentUser()"
      [organizations]="organizations"
      [selectedOrganization]="selectedOrganization"
    ></ui-main-layout>
  `,
  styles: [`
    :host {
      display: block;
      height: 100vh;
    }
  `],
})
export class ShellComponent {
  protected readonly authService = inject(AuthService);

  // Mock data for now
  organizations: any[] = [];
  selectedOrganization = null;

  // Layout configuration for Sports Admin
  layoutConfig = {
    appTitle: 'Sports Admin',
    appLogo: '/assets/admin-logo.png',
    showUserMenu: true,
    showNotifications: true,
    showSearch: true,
    sidenavMode: 'side' as const,
    sidenavOpened: true,
    showFooter: true,
  };

  // Permission checker function (admin has access to everything)
  checkPermission = (item: NavItem): boolean => {
    return true; // Admins have access to all features
  };

  // Admin-specific navigation items
  navItems: NavItem[] = [
    { name: 'Dashboard', icon: 'dashboard', route: '/' },
    { 
      name: 'User Management', 
      icon: 'people', 
      children: [
        { 
          name: 'All Users', 
          icon: 'people_alt', 
          route: '/users'
        },
        { 
          name: 'User Roles', 
          icon: 'admin_panel_settings', 
          route: '/users/roles'
        },
        { 
          name: 'Permissions', 
          icon: 'security', 
          route: '/users/permissions'
        },
      ],
    },
    { 
      name: 'Organization Management', 
      icon: 'business', 
      children: [
        { 
          name: 'All Organizations', 
          icon: 'business_center', 
          route: '/organizations'
        },
        { 
          name: 'Organization Settings', 
          icon: 'settings_applications', 
          route: '/organizations/settings'
        },
        { 
          name: 'League Management', 
          icon: 'emoji_events', 
          route: '/organizations/leagues'
        },
      ],
    },
    { 
      name: 'System Administration', 
      icon: 'settings', 
      children: [
        { 
          name: 'System Settings', 
          icon: 'tune', 
          route: '/system/settings'
        },
        { 
          name: 'System Logs', 
          icon: 'description', 
          route: '/system/logs'
        },
        { 
          name: 'Database Management', 
          icon: 'storage', 
          route: '/system/database'
        },
        { 
          name: 'API Management', 
          icon: 'api', 
          route: '/system/api'
        },
      ],
    },
    { 
      name: 'Analytics & Reports', 
      icon: 'analytics', 
      children: [
        { 
          name: 'User Analytics', 
          icon: 'trending_up', 
          route: '/analytics/users'
        },
        { 
          name: 'System Performance', 
          icon: 'speed', 
          route: '/analytics/performance'
        },
        { 
          name: 'Usage Reports', 
          icon: 'assessment', 
          route: '/analytics/usage'
        },
      ],
    },
    { 
      name: 'Content Management', 
      icon: 'edit', 
      children: [
        { 
          name: 'Announcements', 
          icon: 'campaign', 
          route: '/content/announcements'
        },
        { 
          name: 'Help Documentation', 
          icon: 'help', 
          route: '/content/help'
        },
        { 
          name: 'Email Templates', 
          icon: 'email', 
          route: '/content/email-templates'
        },
      ],
    },
    { name: 'Profile', icon: 'person', route: '/profile' },
    { name: 'Settings', icon: 'settings', route: '/settings' },
  ];
}
