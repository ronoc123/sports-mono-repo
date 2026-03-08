import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";
import {
  AuthenticationResponse,
  RegisterRequest,
  LoginRequest,
  GoogleLoginRequest,
  RefreshTokenRequest,
  ServiceResponse,
  environment,
} from "@sports-ui/api-types";

@Injectable({ providedIn: "root" })
export class AuthApi {
  private readonly http = inject(HttpClient);

  private readonly base = `${environment.identityApiBaseUrl}/auth`;
  private readonly headers = new HttpHeaders({ "Content-Type": "application/json", Accept: "application/json" });

  register(payload: RegisterRequest): Observable<AuthenticationResponse> {
    return this.http.post<AuthenticationResponse>(`${this.base}/register`, payload, { headers: this.headers });
  }

  login(payload: LoginRequest): Observable<AuthenticationResponse> {
    return this.http.post<AuthenticationResponse>(`${this.base}/login`, payload, { headers: this.headers });
  }

  loginWithGoogle(googleToken: string): Observable<AuthenticationResponse> {
    const body: GoogleLoginRequest = { googleToken };
    return this.http.post<AuthenticationResponse>(`${this.base}/google`, body, { headers: this.headers });
  }

  refresh(refreshToken: string): Observable<ServiceResponse<AuthenticationResponse>> {
    const body: RefreshTokenRequest = { refreshToken };
    return this.http.post<ServiceResponse<AuthenticationResponse>>(`${this.base}/refresh`, body, { headers: this.headers });
  }

  logout(): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.base}/logout`, {}, { headers: this.headers });
  }
}
