import { Component, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { MainLayoutComponent, NavItem, LayoutConfig } from '@sports-ui/ui';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'auth-app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    MatProgressSpinnerModule,
    MainLayoutComponent,
  ],
  template: `
    <!-- Loading State -->
    <div class="loading-container" *ngIf="authService.isLoading()">
      <mat-spinner diameter="50"></mat-spinner>
      <p>Loading...</p>
    </div>

    <!-- Authenticated State - Show Layout -->
    <ui-main-layout 
      *ngIf="authService.isAuthenticated() && !authService.isLoading()"
      [navItems]="navItems()"
      [config]="layoutConfig()"
      [permissionChecker]="permissionChecker()"
      [currentUser]="authService.currentUser()"
      [organizations]="organizations()"
      [selectedOrganization]="selectedOrganization()"
    ></ui-main-layout>

    <!-- Unauthenticated State - Show Router Outlet (Login Page) -->
    <router-outlet *ngIf="!authService.isAuthenticated() && !authService.isLoading()"></router-outlet>
  `,
  styles: [`
    :host {
      display: block;
      height: 100vh;
      width: 100vw;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      height: 100vh;
      gap: 16px;
    }

    .loading-container p {
      margin: 0;
      color: #666;
      font-size: 16px;
    }
  `],
})
export class AppShellComponent {
  readonly authService = inject(AuthService);

  // Inputs for configuration
  readonly navItems = input.required<NavItem[]>();
  readonly layoutConfig = input.required<LayoutConfig>();
  readonly permissionChecker = input<(item: NavItem) => boolean>(() => true);
  readonly organizations = input<any[]>([]);
  readonly selectedOrganization = input<any>(null);
}
