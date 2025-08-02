import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable, of, catchError, map } from "rxjs";
import { EnvironmentService } from "./environment.service";

export interface HealthCheckResult {
  service: string;
  status: "healthy" | "unhealthy" | "unknown";
  url: string;
  responseTime?: number;
  error?: string;
  timestamp: Date;
}

export interface ApiHealthStatus {
  mainApi: HealthCheckResult;
  identityApi: HealthCheckResult;
  overall: "healthy" | "degraded" | "unhealthy";
}

@Injectable({
  providedIn: "root",
})
export class ApiHealthService {
  private readonly http = inject(HttpClient);
  private readonly environmentService = inject(EnvironmentService);

  /**
   * Check health of main API
   */
  checkMainApiHealth(): Observable<HealthCheckResult> {
    const startTime = Date.now();
    const url = `${this.environmentService.apiUrl}/health`;

    return this.http.get(url, { observe: "response" }).pipe(
      map(
        (response) =>
          ({
            service: "Main API",
            status: response.status === 200 ? "healthy" : "unhealthy",
            url,
            responseTime: Date.now() - startTime,
            timestamp: new Date(),
          } as HealthCheckResult)
      ),
      catchError((error) =>
        of({
          service: "Main API",
          status: "unhealthy",
          url,
          responseTime: Date.now() - startTime,
          error: error.message || "Connection failed",
          timestamp: new Date(),
        } as HealthCheckResult)
      )
    );
  }

  /**
   * Check health of identity API
   */
  checkIdentityApiHealth(): Observable<HealthCheckResult> {
    const startTime = Date.now();
    const url = `${this.environmentService.identityApiUrl}/health`;

    return this.http.get(url, { observe: "response" }).pipe(
      map(
        (response) =>
          ({
            service: "Identity API",
            status: response.status === 200 ? "healthy" : "unhealthy",
            url,
            responseTime: Date.now() - startTime,
            timestamp: new Date(),
          } as HealthCheckResult)
      ),
      catchError((error) =>
        of({
          service: "Identity API",
          status: "unhealthy",
          url,
          responseTime: Date.now() - startTime,
          error: error.message || "Connection failed",
          timestamp: new Date(),
        } as HealthCheckResult)
      )
    );
  }

  /**
   * Check health of all APIs
   */
  checkAllApisHealth(): Observable<ApiHealthStatus> {
    return new Observable((observer) => {
      const results: Partial<ApiHealthStatus> = {};
      let completed = 0;
      const total = 2;

      const checkComplete = () => {
        completed++;
        if (completed === total) {
          const status: ApiHealthStatus = {
            mainApi: results.mainApi!,
            identityApi: results.identityApi!,
            overall: this.calculateOverallStatus(
              results.mainApi!,
              results.identityApi!
            ),
          };
          observer.next(status);
          observer.complete();
        }
      };

      // Check main API
      this.checkMainApiHealth().subscribe((result) => {
        results.mainApi = result;
        checkComplete();
      });

      // Check identity API
      this.checkIdentityApiHealth().subscribe((result) => {
        results.identityApi = result;
        checkComplete();
      });
    });
  }

  /**
   * Simple ping to check if API is reachable
   */
  pingMainApi(): Observable<boolean> {
    return this.checkMainApiHealth().pipe(
      map((result) => result.status === "healthy")
    );
  }

  /**
   * Simple ping to check if identity API is reachable
   */
  pingIdentityApi(): Observable<boolean> {
    return this.checkIdentityApiHealth().pipe(
      map((result) => result.status === "healthy")
    );
  }

  /**
   * Get API configuration info
   */
  getApiConfiguration() {
    return {
      mainApi: {
        url: this.environmentService.apiUrl,
        baseUrl: this.environmentService.apiBaseUrl,
        timeout: this.environmentService.apiTimeout,
        retryAttempts: this.environmentService.retryAttempts,
      },
      identityApi: {
        url: this.environmentService.identityApiUrl,
        baseUrl: this.environmentService.identityApiBaseUrl,
      },
      environment: {
        production: this.environmentService.isProduction,
        mockData: this.environmentService.enableMockData,
        realAuth: this.environmentService.enableRealAuth,
      },
    };
  }

  private calculateOverallStatus(
    mainApi: HealthCheckResult,
    identityApi: HealthCheckResult
  ): "healthy" | "degraded" | "unhealthy" {
    const healthyCount = [mainApi, identityApi].filter(
      (api) => api.status === "healthy"
    ).length;

    if (healthyCount === 2) {
      return "healthy";
    } else if (healthyCount === 1) {
      return "degraded";
    } else {
      return "unhealthy";
    }
  }
}
