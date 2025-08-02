import { Route } from "@angular/router";
import { authGuard, loginGuard } from "@sports-ui/feature-auth";

export const appRoutes: Route[] = [
  // Login route (unauthenticated only)
  {
    path: "login",
    canActivate: [loginGuard],
    loadComponent: () =>
      import("@sports-ui/feature-auth").then((m) => m.LoginPageComponent),
  },

  // Main app shell (authenticated only)
  {
    path: "",
    canActivate: [authGuard],
    loadComponent: () =>
      import("./shell/shell.component").then((m) => m.ShellComponent),
    children: [
      // Dashboard route
      {
        path: "",
        loadChildren: () =>
          import("@sports-ui/feature-dashboard").then((m) => m.dashBoardRoutes),
      },
      // GM-specific routes will be added later
      // For now, all routes redirect to dashboard
    ],
  },

  // Fallback route
  { path: "**", redirectTo: "" },
];
