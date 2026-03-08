import { inject } from "@angular/core";
import { HttpInterceptorFn } from "@angular/common/http";
import { AuthStore } from "@sports-ui/auth-data-access";

export const apiBaseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  const authStore = inject(AuthStore);
  const token = authStore.bearer();
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(req);
};
