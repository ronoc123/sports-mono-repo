import { Route } from "@angular/router";
import { Component } from "@angular/core";
import { RouterModule } from "@angular/router";
import { authGuard, loginGuard } from "@sports-ui/feature-auth";

// Simple error components
@Component({
  template: `
    <div style="text-align: center; padding: 2rem;">
      <h1>Unauthorized</h1>
      <p>You do not have permission to access this page.</p>
      <a routerLink="/" style="color: #2196f3;">Go to Dashboard</a>
    </div>
  `,
  standalone: true,
  imports: [RouterModule],
})
class UnauthorizedComponent {}

@Component({
  template: `
    <div style="text-align: center; padding: 2rem;">
      <h1>Page Not Found</h1>
      <p>The page you are looking for does not exist.</p>
      <a routerLink="/" style="color: #2196f3;">Go to Dashboard</a>
    </div>
  `,
  standalone: true,
  imports: [RouterModule],
})
class NotFoundComponent {}

export const appRoutes: Route[] = [
  // Login route (accessible only to unauthenticated users)
  {
    path: "login",
    canActivate: [loginGuard],
    loadComponent: () =>
      import("@sports-ui/feature-auth").then((m) => m.LoginPageComponent),
  },

  // Main app shell with layout (protected routes)
  {
    path: "",
    canActivate: [authGuard],
    loadComponent: () =>
      import("./shell/shell.component").then((m) => m.ShellComponent),
    children: [
      // Dashboard (default route)
      {
        path: "",
        loadChildren: () =>
          import("@sports-ui/feature-dashboard").then((m) => m.dashBoardRoutes),
      },

      // Feature routes
      {
        path: "active-roaster",
        loadChildren: () =>
          import("@sports-ui/feature-roaster").then((m) => m.roasterRoutes),
      },
      {
        path: "player-option",
        loadChildren: () =>
          import("@sports-ui/feature-player-option").then(
            (m) => m.playerOptionRoutes
          ),
      },
      {
        path: "redeem",
        loadComponent: () => import("@sports-ui/redeem").then((m) => m.Redeem),
      },

      // Profile route
      {
        path: "profile",
        loadComponent: () =>
          import("./pages/profile/profile.component").then(
            (m) => m.ProfileComponent
          ),
      },

      // Settings route
      {
        path: "settings",
        loadComponent: () =>
          import("./pages/settings/settings.component").then(
            (m) => m.SettingsComponent
          ),
      },
    ],
  },

  // Simple error routes
  {
    path: "unauthorized",
    component: UnauthorizedComponent,
  },
  {
    path: "not-found",
    component: NotFoundComponent,
  },

  // Wildcard route
  {
    path: "**",
    redirectTo: "not-found",
  },
];
