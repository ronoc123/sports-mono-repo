import { Component, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ToastService } from "@sports-ui/toast";
import { Toast } from "@sports-ui/ui";

@Component({
  selector: "lib-toast-feature",
  imports: [CommonModule, Toast],
  templateUrl: "./toast-feature.html",
  styleUrl: "./toast-feature.css",
})
export class ToastFeature {
  private readonly toastService = inject(ToastService);
  toast = this.toastService.toast;
}
