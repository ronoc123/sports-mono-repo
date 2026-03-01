import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { ProductDto } from "./store.model";

type CatalogStatus = "idle" | "loading" | "success" | "error";
export type CheckoutStatus =
  | "idle"
  | "initiating"
  | "ready"
  | "processing"
  | "succeeded"
  | "error";

interface StoreState {
  bundles: ProductDto[];
  status: CatalogStatus;
  error: string | undefined;

  checkoutStatus: CheckoutStatus;
  clientSecret: string;
  checkoutError: string | undefined;
  selectedBundleId: string | undefined;
}

export const BundleCatalogStore = signalStore(
  withState<StoreState>({
    bundles: [],
    status: "idle",
    error: undefined,

    checkoutStatus: "idle",
    clientSecret: "",
    checkoutError: undefined,
    selectedBundleId: undefined,
  }),

  withMethods((store) => ({
    // ── Catalog ──
    setLoading() {
      patchState(store, { status: "loading", error: undefined });
    },
    setBundles(bundles: ProductDto[]) {
      patchState(store, { bundles, status: "success" });
    },
    setError(msg: string) {
      patchState(store, { status: "error", error: msg });
    },

    // ── Checkout ──
    setCheckoutInitiating(bundleId: string) {
      patchState(store, {
        checkoutStatus: "initiating",
        checkoutError: undefined,
        clientSecret: undefined,
        selectedBundleId: bundleId,
      });
    },
    setClientSecret(clientSecret: string) {
      patchState(store, { clientSecret, checkoutStatus: "ready" });
    },
    setCheckoutProcessing() {
      patchState(store, { checkoutStatus: "processing" });
    },
    setCheckoutSucceeded() {
      patchState(store, { checkoutStatus: "succeeded" });
    },
    setCheckoutError(msg: string) {
      patchState(store, { checkoutStatus: "error", checkoutError: msg });
    },
    clearCheckout() {
      patchState(store, {
        checkoutStatus: "idle",
        clientSecret: undefined,
        checkoutError: undefined,
        selectedBundleId: undefined,
      });
    },
  }))
);
