import { Injectable, signal } from "@angular/core";

@Injectable({ providedIn: "root" })
export class ToastService {
  private readonly _toast = signal<{
    message: string;
    outcome: "success" | "error";
  } | null>(null);

  toast = this._toast.asReadonly();

  success(message: string) {
    this.show(message, "success");
  }

  error(message: string) {
    this.show(message, "error");
  }

  private show(message: string, outcome: "success" | "error") {
    this._toast.set({ message, outcome });

    setTimeout(() => {
      this._toast.set(null);
    }, 3000);
  }
}
