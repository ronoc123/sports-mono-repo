import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { OrganizationDto } from "./organization.model";

type Status = "idle" | "loading" | "error" | "success";

interface OrganizationState {
  status: Status;
  error?: string;
  organizations: OrganizationDto[];
  selectedOrganization: OrganizationDto | null;
}

export const OrganizationStore = signalStore(
  withState<OrganizationState>({
    status: "idle",
    organizations: [],
    selectedOrganization: null,
    error: undefined,
  }),

  withMethods((store) => ({
    setLoading() {
      patchState(store, { status: "loading", error: undefined });
    },

    setError(msg: string) {
      patchState(store, { status: "error", error: msg });
    },

    setOrganizations(orgs: OrganizationDto[]) {
      patchState(store, { organizations: orgs, status: "success" });
    },

    setSelectedOrg(org: OrganizationDto) {
      patchState(store, { selectedOrganization: org });
    },

    clearSelected() {
      patchState(store, { selectedOrganization: null });
    },
  }))
);
