import { HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { EnvironmentService } from "../services/environment.service";

export const apiBaseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  const environmentService = inject(EnvironmentService);

  // Only modify requests that don't already have a full URL
  if (req.url.startsWith("http://") || req.url.startsWith("https://")) {
    return next(req);
  }

  // Determine which API base URL to use
  let baseUrl: string;

  if (req.url.startsWith("/identity/") || req.url.startsWith("identity/")) {
    // Identity API requests
    baseUrl = environmentService.identityApiBaseUrl;
    // Remove 'identity/' prefix since it's already in the base URL
    const cleanUrl = req.url.replace(/^\/?(identity\/?)/, "");
    req = req.clone({
      url: `${baseUrl}/${cleanUrl}`,
    });
  } else if (req.url.startsWith("/api/") || req.url.startsWith("api/")) {
    // Main API requests
    baseUrl = environmentService.apiBaseUrl;
    // Remove 'api/' prefix since it's already in the base URL
    const cleanUrl = req.url.replace(/^\/?(api\/?)/, "");
    req = req.clone({
      url: `${baseUrl}/${cleanUrl}`,
    });
  } else {
    // Default to main API for relative URLs
    baseUrl = environmentService.apiBaseUrl;
    const cleanUrl = req.url.startsWith("/") ? req.url.slice(1) : req.url;
    req = req.clone({
      url: `${baseUrl}/${cleanUrl}`,
    });
  }

  // Add common headers
  req = req.clone({
    setHeaders: {
      "Content-Type": "application/json",
      Accept: "application/json",
      "X-App-Name": environmentService.appName,
      "X-App-Version": environmentService.appVersion,
    },
  });

  // Add auth token if available
  const token = localStorage.getItem("authToken");
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  // Log request in development
  if (environmentService.isDevelopment && environmentService.enableLogging) {
    console.log(`🌐 API Request: ${req.method} ${req.url}`);
  }

  return next(req);
};
