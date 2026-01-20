import { Injectable, signal, TemplateRef } from "@angular/core";

export interface ModalConfig {
  title?: string;
  width?: "sm" | "md" | "lg";
  content: TemplateRef<any>;
  context?: any;
}

@Injectable({ providedIn: "root" })
export class ModalService {
  private _open = signal(false);
  private _config = signal<ModalConfig | null>(null);

  open(config: ModalConfig) {
    this._config.set(config);
    this._open.set(true);
  }

  close() {
    this._open.set(false);
    this._config.set(null);
  }

  // exposed signals
  readonly isOpen = this._open;
  readonly config = this._config;
}
