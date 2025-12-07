import { inject, Injectable } from "@angular/core";
import { firstValueFrom } from "rxjs";

import { OrganizationStore } from "./organziation.store";
import { OrganizationApi } from "./organization-data-access";

import {
  GetAllOrganizationsQuery,
  CreateOrganizationCommand,
  UpdateOrganizationCommand,
  OrganizationDto,
} from "./organization.model";

@Injectable({ providedIn: "root" })
export class OrganizationFeatureService {
  private readonly store = inject(OrganizationStore);
  private readonly api = inject(OrganizationApi);

  // Expose selectors
  readonly organizations = this.store.organizations;
  readonly selectedOrganization = this.store.selectedOrganization;
  readonly status = this.store.status;
  readonly error = this.store.error;

  // -----------------------------------------------------
  // Load organization list
  // -----------------------------------------------------
  async loadOrganizations(query: GetAllOrganizationsQuery = {}) {
    this.store.setLoading();

    try {
      const res = await firstValueFrom(this.api.getAll(query));
      const items = res.data?.items ?? [];

      this.store.setOrganizations(items);
      if (this.store.selectedOrganization() == null && items.length > 0) {
        this.store.setSelectedOrg(items[0]);
      }
    } catch (e: any) {
      this.store.setError(e?.message ?? "Failed to load organizations");
    }
  }

  // -----------------------------------------------------
  // Create organization
  // -----------------------------------------------------
  async createOrganization(payload: CreateOrganizationCommand) {
    this.store.setLoading();

    try {
      await firstValueFrom(this.api.addOrganization(payload));
      await this.loadOrganizations(); // refresh list automatically
    } catch (e: any) {
      this.store.setError(e?.message ?? "Failed to create organization");
    }
  }

  // -----------------------------------------------------
  // Update organization
  // -----------------------------------------------------
  async updateOrganization(payload: UpdateOrganizationCommand) {
    this.store.setLoading();

    try {
      await firstValueFrom(this.api.updateOrganization(payload));
      await this.loadOrganizations();
    } catch (e: any) {
      this.store.setError(e?.message ?? "Failed to update organization");
    }
  }

  // -----------------------------------------------------
  // Delete organization
  // -----------------------------------------------------
  async deleteOrganization(orgId: string) {
    this.store.setLoading();

    try {
      await firstValueFrom(this.api.deleteOrganization(orgId));
      await this.loadOrganizations();
    } catch (e: any) {
      this.store.setError(e?.message ?? "Failed to delete organization");
    }
  }

  // -----------------------------------------------------
  // Optional: load theme
  // -----------------------------------------------------
  async loadTheme(name: string) {
    try {
      return await firstValueFrom(this.api.getTheme(name));
    } catch (e) {
      console.error("Failed to load theme:", e);
    }
  }

  selectOrganization(org: OrganizationDto) {
    this.store.setSelectedOrg(org);
  }
}
