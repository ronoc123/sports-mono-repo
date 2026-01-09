import { Component, computed, inject, input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { NgControl } from "@angular/forms";

@Component({
  selector: "lib-ui-input",
  imports: [CommonModule],
  templateUrl: "./ui-input.html",
  styleUrl: "./ui-input.css",
})
export class UiInput {
  // Signal inputs
  label = input<string>();
  required = input<boolean>(false);

  // Auto-detect the form control
  private ngControl = inject(NgControl, { optional: true });

  control = computed(() => this.ngControl?.control ?? null);

  showError = computed(() => {
    const c = this.control();
    return !!(c && c.invalid && (c.touched || c.dirty));
  });

  errorMessage = computed(() => {
    const errors = this.control()?.errors;
    if (!errors) return null;

    if (errors["required"]) return "This field is required";
    if (errors["email"]) return "Enter a valid email";
    if (errors["minlength"])
      return `Minimum ${errors["minlength"].requiredLength} characters`;
    if (errors["maxlength"])
      return `Maximum ${errors["maxlength"].requiredLength} characters`;

    return "Invalid value";
  });
}
