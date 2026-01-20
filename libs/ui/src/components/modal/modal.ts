import { Component, input, output, TemplateRef } from "@angular/core";
import { CommonModule, NgTemplateOutlet } from "@angular/common";

@Component({
  selector: "lib-ui-modal",
  standalone: true,
  imports: [CommonModule, NgTemplateOutlet],
  templateUrl: "./modal.html",
  styleUrls: ["./modal.css"],
})
export class UiModalComponent {
  // visibility
  open = input<boolean>(false);

  // chrome
  title = input<string | undefined>();
  width = input<"sm" | "md" | "lg">("md");

  // 🔑 flexible content
  content = input<TemplateRef<any> | null>(null);
  context = input<any>(null);

  // outputs
  // eslint-disable-next-line @angular-eslint/no-output-native
  close = output<void>();
  confirm = output<void>();

  onBackdropClick() {
    this.close.emit();
  }
}
