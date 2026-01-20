import { Component, inject } from "@angular/core";
import { UiModalComponent } from "@sports-ui/ui";
import { ModalService } from "../modal-service/modal-service";

@Component({
  selector: "lib-modal-host",
  standalone: true,
  imports: [UiModalComponent],
  template: `
    <lib-ui-modal
      [open]="modal.isOpen()"
      [title]="modal.config()?.title"
      [width]="modal.config()?.width ?? 'md'"
      [content]="modal.config()?.content ?? null"
      [context]="modal.config()?.context"
      (close)="modal.close()"
      (confirm)="modal.close()"
    />
  `,
})
export class ModalHostComponent {
  modal = inject(ModalService);
}
