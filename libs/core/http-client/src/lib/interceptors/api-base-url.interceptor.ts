import { HttpInterceptorFn } from "@angular/common/http";

export const apiBaseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  // Add auth token if available
  const token = localStorage.getItem("authToken");
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(req);
};
