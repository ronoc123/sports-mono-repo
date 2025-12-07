import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthFacade } from "./auth.facade";

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthFacade);
  const router = inject(Router);
  // auth.user is a SIGNAL — call it to get the value
  if (auth.user()) {
    return true;
  }

  // Return a redirect UrlTree instead of navigating + returning true
  return router.parseUrl("/login");
};
