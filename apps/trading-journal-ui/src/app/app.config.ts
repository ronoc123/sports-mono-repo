import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { appRoutes } from './app.routes';
import { API_URL, apiBaseUrlInterceptor } from '@sports-ui/http-client';
import { AuthStore } from '@sports-ui/auth-data-access';
import { BacktestStore } from '@trading/backtest-data-access';
import { TradeJournalStore } from '@trading/trade-journal-data-access';
import { AIConversationStore } from '@trading/ai-conversation-data-access';
import { ReviewStore } from '@trading/review-data-access';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(appRoutes),
    provideHttpClient(withInterceptors([apiBaseUrlInterceptor])),
    provideAnimationsAsync(),
    AuthStore,
    BacktestStore,
    TradeJournalStore,
    AIConversationStore,
    ReviewStore,
    { provide: API_URL, useValue: 'http://localhost:5280/api' },
  ],
};
