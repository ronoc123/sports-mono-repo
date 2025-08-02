import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MainLayoutComponent, NavItem } from '@sports-ui/ui';

@Component({
  selector: 'gm-root',
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
      [currentUser]="currentUser"
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
export class AppComponent {
  // Mock data for now
  currentUser = null;
  organizations: any[] = [];
  selectedOrganization = null;

  // Layout configuration for GM Portal
  layoutConfig = {
    appTitle: 'GM Portal',
    appLogo: '/assets/gm-logo.png',
    showUserMenu: true,
    showNotifications: true,
    showSearch: true,
    sidenavMode: 'side' as const,
    sidenavOpened: true,
    showFooter: true,
  };

  // Permission checker function (simplified for now)
  checkPermission = (item: NavItem): boolean => {
    return true; // Allow all for now
  };

  // GM-specific navigation items
  navItems: NavItem[] = [
    { name: 'Dashboard', icon: 'dashboard', route: '/' },
    { name: 'My Organization', icon: 'business', route: '/organization' },
    { 
      name: 'Player Management', 
      icon: 'people', 
      children: [
        { 
          name: 'Active Roster', 
          icon: 'people_alt', 
          route: '/players/roster'
        },
        { 
          name: 'Player Options', 
          icon: 'how_to_vote', 
          route: '/player-options'
        },
        { 
          name: 'Create Player Option', 
          icon: 'add_circle', 
          route: '/player-options/create'
        },
        { 
          name: 'Draft Players', 
          icon: 'person_add', 
          route: '/players/draft'
        },
      ],
    },
    { 
      name: 'Organization Settings', 
      icon: 'settings', 
      children: [
        { 
          name: 'Organization Info', 
          icon: 'info', 
          route: '/organization/settings'
        },
        { 
          name: 'Team Configuration', 
          icon: 'sports', 
          route: '/organization/team-config'
        },
        { 
          name: 'League Settings', 
          icon: 'emoji_events', 
          route: '/organization/league'
        },
      ],
    },
    { 
      name: 'Analytics', 
      icon: 'analytics', 
      children: [
        { 
          name: 'Player Performance', 
          icon: 'trending_up', 
          route: '/analytics/players'
        },
        { 
          name: 'Voting Trends', 
          icon: 'poll', 
          route: '/analytics/voting'
        },
        { 
          name: 'Organization Stats', 
          icon: 'bar_chart', 
          route: '/analytics/organization'
        },
      ],
    },
    { 
      name: 'Reports', 
      icon: 'assessment', 
      route: '/reports'
    },
    { name: 'Profile', icon: 'person', route: '/profile' },
    { name: 'Settings', icon: 'settings', route: '/settings' },
  ];
}
