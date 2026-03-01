// apps/sports-ui/src/app/app.config.ts
import { ApplicationConfig } from "@angular/core";
import { provideRouter } from "@angular/router";
import { provideHttpClient, withInterceptors } from "@angular/common/http";
import { provideAnimationsAsync } from "@angular/platform-browser/animations/async";
import { environment } from "@sports-ui/api-types";

import { appRoutes } from "./app.routes";
import {
  API_URL,
  APP_ENVIRONMENT,
  apiBaseUrlInterceptor,
} from "@sports-ui/http-client";
// eslint-disable-next-line @nx/enforce-module-boundaries
import { GOOGLE_CLIENT_ID } from "@sports-ui/feature-auth";
import { AuthStore } from "@sports-ui/auth-data-access";
import { PlayerOptionStore } from "@sports-ui/player-options-data-access";
import { OrganizationStore } from "@sports-ui/organization-data-access";
import { VoteAccountStore } from "@sports-ui/vote-account-data-access";
import { NotificationStore } from "@sports-ui/notification-data-access";
import { PlayerStore } from "@sports-ui/player-data-access";
import { RedeemStore } from "@sports-ui/redeem-data-access";
import { SendVotesStore } from "@sports-ui/send-votes-data-access";
import { BundleCatalogStore } from "@sports-ui/store-data-access";

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(appRoutes),
    provideHttpClient(withInterceptors([apiBaseUrlInterceptor])),
    provideAnimationsAsync(),
    AuthStore,
    PlayerOptionStore,
    OrganizationStore,
    VoteAccountStore,
    NotificationStore,
    PlayerStore,
    SendVotesStore,
    RedeemStore,
    BundleCatalogStore,
    { provide: APP_ENVIRONMENT, useValue: environment },
    { provide: API_URL, useValue: environment.apiUrl },
    { provide: GOOGLE_CLIENT_ID, useValue: environment.googleClientId },
  ],
};
