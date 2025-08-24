// apps/sports-ui/src/app/app.config.ts
import { ApplicationConfig } from "@angular/core";
import { provideRouter } from "@angular/router";
import { provideHttpClient, withInterceptors } from "@angular/common/http";
import { provideAnimationsAsync } from "@angular/platform-browser/animations/async";
// eslint-disable-next-line @nx/enforce-module-boundaries
import { environment } from "../../../../libs/core/environments/index";

import { appRoutes } from "./app.routes";
import {
  API_URL,
  APP_ENVIRONMENT,
  apiBaseUrlInterceptor,
} from "@sports-ui/http-client";
// eslint-disable-next-line @nx/enforce-module-boundaries
import { GOOGLE_CLIENT_ID } from "@sports-ui/feature-auth";

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(appRoutes),
    provideHttpClient(withInterceptors([apiBaseUrlInterceptor])),
    provideAnimationsAsync(),

    // ✅ Only the CONFIG token is required

    { provide: APP_ENVIRONMENT, useValue: environment },
    { provide: API_URL, useValue: environment.apiUrl },
    { provide: GOOGLE_CLIENT_ID, useValue: environment.googleClientId },
  ],
};
