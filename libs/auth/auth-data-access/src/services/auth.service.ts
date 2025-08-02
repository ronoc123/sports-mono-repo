import { Injectable, inject } from '@angular/core';
import { ApiService } from '@sports-ui/http-client';
import { Observable } from 'rxjs';
import { LoginUserRequest, LoginUserResponse, ServiceResponse} from '@sports-ui/api-types';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiService = inject(ApiService);

  login(token: string) : Observable<ServiceResponse<LoginUserResponse>> {
    return this.apiService.post<ServiceResponse<LoginUserResponse>, LoginUserRequest>('/signin-google', { token });
  }
}