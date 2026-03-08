import { Component, inject, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { AuthFacade } from "@sports-ui/auth-data-access";

@Component({
  selector: "lib-auth-callback",
  standalone: true,
  template: `
    <div style="display:flex;align-items:center;justify-content:center;height:100vh;font-family:sans-serif;color:#6b7280;">
      Signing you in…
    </div>
  `,
})
export class AuthCallbackComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private auth = inject(AuthFacade);

  ngOnInit() {
    const p = this.route.snapshot.queryParamMap;

    if (p.get("error")) {
      this.router.navigateByUrl("/login");
      return;
    }

    const accessToken = p.get("accessToken");
    const expiresAt = p.get("expiresAt");

    if (!accessToken || !expiresAt) {
      this.router.navigateByUrl("/login");
      return;
    }

    this.auth.handleOAuthCallback({
      accessToken,
      refreshToken: p.get("refreshToken"),
      expiresAt,
      userId: p.get("userId") ?? "",
      email: p.get("email") ?? "",
      firstName: p.get("firstName") ?? undefined,
      lastName: p.get("lastName") ?? undefined,
      picture: p.get("picture") ?? undefined,
      roles: (p.get("roles") ?? "").split(",").filter(Boolean),
    });

    this.router.navigateByUrl("/league");
  }
}
