import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { CanActivateFn } from "@angular/router";
import { UserStore } from "@sports-ui/data-access";

export const authGuard: CanActivateFn = (route, state) => {
  const userStore = inject(UserStore);
  const router = inject(Router);

  const user = userStore.currentUser();
  if (user) {
    return true;
  } else {
    // Redirect to login page with return url
    router.navigate(["/auth/login"], {
      queryParams: { returnUrl: state.url },
    });
    return false;
  }
};

export const guestGuard: CanActivateFn = (route, state) => {
  const userStore = inject(UserStore);
  const router = inject(Router);

  const user = userStore.currentUser();
  if (!user) {
    return true;
  } else {
    // Redirect to dashboard if already authenticated
    router.navigate(["/"]);
    return false;
  }
};

export const adminGuard: CanActivateFn = (route, state) => {
  const userStore = inject(UserStore);
  const router = inject(Router);

  const user = userStore.currentUser();
  if (user && isAdmin(user)) {
    return true;
  } else if (user) {
    // User is authenticated but not admin
    router.navigate(["/unauthorized"]);
    return false;
  } else {
    // User is not authenticated
    router.navigate(["/auth/login"], {
      queryParams: { returnUrl: state.url },
    });
    return false;
  }
};

// Helper function to check if user is admin
function isAdmin(user: any): boolean {
  // TODO: Implement proper role checking based on your user model
  // This is a placeholder implementation
  return user.email?.includes("admin") || user.userName?.includes("admin");
}
