import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { UserStore } from '@sports-ui/data-access';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const userStore = inject(UserStore);
  const router = inject(Router);

  // Get the auth token from storage or user store
  const authToken = getAuthToken();

  // Clone the request and add the authorization header if token exists
  let authReq = req;
  if (authToken) {
    authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${authToken}`)
    });
  }

  // Add common headers
  authReq = authReq.clone({
    headers: authReq.headers
      .set('Content-Type', 'application/json')
      .set('Accept', 'application/json')
  });

  return next(authReq).pipe(
    catchError(error => {
      // Handle authentication errors
      if (error.status === 401) {
        // Clear user session and redirect to login
        clearAuthToken();
        userStore.setCurrentUser(null);
        router.navigate(['/auth/login']);
      } else if (error.status === 403) {
        // Handle forbidden access
        router.navigate(['/unauthorized']);
      }

      return throwError(() => error);
    })
  );
};

// Helper functions for token management
function getAuthToken(): string | null {
  if (typeof window !== 'undefined') {
    return localStorage.getItem('auth_token') || sessionStorage.getItem('auth_token');
  }
  return null;
}

function clearAuthToken(): void {
  if (typeof window !== 'undefined') {
    localStorage.removeItem('auth_token');
    sessionStorage.removeItem('auth_token');
  }
}
