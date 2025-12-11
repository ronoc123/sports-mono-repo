import { Component, input, output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatSidenav } from "@angular/material/sidenav";
import { MatDivider } from "@angular/material/divider";
import { MatIcon } from "@angular/material/icon";
import { MatMenu } from "@angular/material/menu";
import { MatToolbar } from "@angular/material/toolbar";
import { LayoutConfig } from "../sidebar/sidebar.component";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatButtonToggleModule } from "@angular/material/button-toggle";

@Component({
  selector: "lib-ui-navbar",
  imports: [
    CommonModule,
    MatDivider,
    MatIcon,
    MatMenu,
    MatToolbar,
    MatSlideToggleModule,
    MatButtonToggleModule,
  ],
  standalone: true,
  templateUrl: "./navbar.component.html",
  styleUrls: ["./navbar.component.css"],
})
export class NavbarComponent {
  readonly config = input<LayoutConfig>();
  readonly drawer = input<MatSidenav>();
  readonly user = input<any>();

  readonly toggleNavBar = output<void>();
  readonly logout = output<void>();
  readonly openProfile = output<void>();
  readonly openSettings = output<void>();
  readonly toggleTheme = output<void>();
}
