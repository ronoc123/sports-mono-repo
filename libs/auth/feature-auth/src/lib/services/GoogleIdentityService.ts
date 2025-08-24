import { Injectable, signal } from "@angular/core";

type CredentialResponse = { credential?: string; select_by?: string };

@Injectable({ providedIn: "root" })
export class GoogleIdentityService {
  private scriptLoaded = false;
  private inited = false;
  private onCredential?: (jwt: string) => void;

  loading = signal(false);
  error = signal<string | null>(null);

  private loadScript(): Promise<void> {
    if (this.scriptLoaded) return Promise.resolve();
    return new Promise((resolve, reject) => {
      const s = document.createElement("script");
      s.src = "https://accounts.google.com/gsi/client";
      s.async = true;
      s.defer = true;
      s.onload = () => {
        this.scriptLoaded = true;
        resolve();
      };
      s.onerror = () =>
        reject(new Error("Failed to load Google Identity Services"));
      document.head.appendChild(s);
    });
  }

  async init(clientId: string, callback: (jwt: string) => void) {
    this.loading.set(true);
    this.error.set(null);
    try {
      await this.loadScript();
      (window as any).google.accounts.id.initialize({
        client_id: clientId,
        callback: (res: CredentialResponse) => {
          if (res?.credential) this.onCredential?.(res.credential);
        },
        auto_select: false, // tweak if you want
        cancel_on_tap_outside: true, // UX nicety
      });
      this.onCredential = callback;
      this.inited = true;
    } catch (e: any) {
      this.error.set(e?.message || "Google init failed");
      throw e;
    } finally {
      this.loading.set(false);
    }
  }

  /** Render the official button into a container */
  renderButton(container: HTMLElement, opts?: Record<string, any>) {
    if (!this.inited) throw new Error("GIS not initialized");
    (window as any).google.accounts.id.renderButton(container, {
      theme: "outline",
      size: "large",
      type: "standard",
      text: "signin_with",
      shape: "rectangular",
      logo_alignment: "left",
      ...opts,
    });
  }

  /** Show One Tap / account chooser */
  prompt() {
    if (!this.inited) throw new Error("GIS not initialized");
    (window as any).google.accounts.id.prompt(); // FedCM will be used automatically on supported browsers
  }

  /** Optional helpers */
  disableAutoSelect() {
    if (!this.scriptLoaded) return;
    (window as any).google.accounts.id.disableAutoSelect();
  }
}
