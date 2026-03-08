import { Injectable, inject } from "@angular/core";
// eslint-disable-next-line @nx/enforce-module-boundaries
import { ApiService } from "@sports-ui/http-client";
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
  private readonly http = inject(ApiService);

  private readonly base = `${environment.identityApi}auth`;

  register(
    payload: RegisterRequest
  ): Observable<AuthenticationResponse> {
    return this.http.post(`${this.base}/register`, payload);
  }

  login(
    payload: LoginRequest
  ): Observable<AuthenticationResponse> {
    return this.http.post(`${this.base}/login`, payload);
  }

  loginWithGoogle(googleToken: string): Observable<AuthenticationResponse> {
    const body: GoogleLoginRequest = { googleToken };
    return this.http.post(`${this.base}/google`, body);
  }

  refresh(
    refreshToken: string
  ): Observable<ServiceResponse<AuthenticationResponse>> {
    const body: RefreshTokenRequest = { refreshToken };
    return this.http.post(`${this.base}/refresh`, body);
  }

  logout(): Observable<{ success: boolean; message: string }> {
    return this.http.post(`${this.base}/logout`, {});
  }
}
