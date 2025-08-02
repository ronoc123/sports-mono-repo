import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from "@angular/core";
import { provideRouter } from "@angular/router";
import { provideHttpClient, withInterceptors } from "@angular/common/http";
import { provideAnimationsAsync } from "@angular/platform-browser/animations/async";

import { appRoutes } from "./app.routes";
import { environment } from "../environments/environment";
import { APP_ENVIRONMENT, apiBaseUrlInterceptor } from "@sports-ui/http-client";

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes),
    provideHttpClient(withInterceptors([apiBaseUrlInterceptor])),
    provideAnimationsAsync(),

    // Provide environment configuration
    { provide: APP_ENVIRONMENT, useValue: environment },
  ],
};
