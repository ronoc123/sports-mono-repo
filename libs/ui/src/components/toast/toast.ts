import { Component, input } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "lib-toast",
  imports: [CommonModule],
  templateUrl: "./toast.html",
  styleUrl: "./toast.css",
})
export class Toast {
  message = input<string>();
  type = input<ToastType>();
}
export type ToastType = "success" | "error";
