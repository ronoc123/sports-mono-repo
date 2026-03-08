import { Injectable, inject } from "@angular/core";
import { ApiService } from "@sports-ui/http-client";
import { Observable } from "rxjs";
import {
  AuthenticationResponse,
  RegisterRequest,
  LoginRequest,
  GoogleLoginRequest,
  RefreshTokenRequest,
  ServiceResponse,
} from "@sports-ui/api-types";

@Injectable({ providedIn: "root" })
export class AuthApi {
  private readonly http = inject(ApiService);

  register(
    payload: RegisterRequest
  ): Observable<AuthenticationResponse> {
    return this.http.post(":5001/api/auth/register", payload);
  }

  login(
    payload: LoginRequest
  ): Observable<AuthenticationResponse> {
    return this.http.post(":5001/api/auth/login", payload);
  }

  loginWithGoogle(googleToken: string): Observable<AuthenticationResponse> {
    const body: GoogleLoginRequest = { googleToken };
    return this.http.post(":5001/api/auth/google", body);
  }

  refresh(
    refreshToken: string
  ): Observable<ServiceResponse<AuthenticationResponse>> {
    const body: RefreshTokenRequest = { refreshToken };
    return this.http.post("/api/auth/refresh", body);
  }

  logout(): Observable<{ success: boolean; message: string }> {
    return this.http.post("/api/auth/logout", {});
  }
}
