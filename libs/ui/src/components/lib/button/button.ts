import { Component, input } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "lib-button",
  imports: [CommonModule],
  templateUrl: "./button.html",
  styleUrl: "./button.css",
  standalone: true,
})
export class Button {
  variant = input.required<"primary" | "secondary" | "danger">();
  size = input.required<"sm" | "md" | "lg">();
  disabled = input<boolean>();
}
