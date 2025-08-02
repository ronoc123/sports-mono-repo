import { Routes } from "@angular/router";
import { FeatureDashboard } from "./index";

export const dashBoardRoutes: Routes = [
  {
    path: "",
    loadComponent: () =>
      import(
        "../../../../apps/sports-ui/src/app/pages/dashboard/dashboard.component"
      ).then((m) => m.DashboardComponent),
  },
  {
    path: "legacy",
    component: FeatureDashboard,
  },
];
