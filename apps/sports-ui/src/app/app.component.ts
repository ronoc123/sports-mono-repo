import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { ModalHostComponent } from "@sports-ui/modal-service";

@Component({
  selector: "app-root",
  standalone: true,
  imports: [CommonModule, RouterModule, ModalHostComponent],
  template: ` <router-outlet></router-outlet><lib-modal-host /> `,
  styles: [
    `
      :host {
        display: block;
        height: 100vh;
      }
    `,
  ],
})
export class AppComponent {}
