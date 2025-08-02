import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatPaginatorModule } from '@angular/material/paginator';
import { OrganizationStore } from '@sports-ui/data-access';
import { OrganizationCardComponent } from '@sports-ui/ui';
import { Organization, OrganizationFilter } from '@sports-ui/api-types';

@Component({
  selector: 'lib-feature-organization',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatPaginatorModule,
    OrganizationCardComponent,
  ],
  templateUrl: './feature-organization.html',
  styleUrl: './feature-organization.css',
})
export class FeatureOrganization implements OnInit {
  private readonly organizationStore = inject(OrganizationStore);

  // Store signals
  organizations = this.organizationStore.organizations;
  selectedOrganization = this.organizationStore.selectedOrganization;
  loading = this.organizationStore.loading;
  error = this.organizationStore.error;
  totalCount = this.organizationStore.totalCount;
  pageNumber = this.organizationStore.pageNumber;
  pageSize = this.organizationStore.pageSize;
  totalPages = this.organizationStore.totalPages;

  // Local state
  currentFilter = signal<OrganizationFilter>({
    pageNumber: 1,
    pageSize: 12,
    searchTerm: '',
    sortBy: 'name',
    sortDescending: false,
  });

  selectedTabIndex = signal<number>(0);

  ngOnInit() {
    this.loadOrganizations();
  }

  loadOrganizations() {
    this.organizationStore.loadOrganizations(this.currentFilter());
  }

  onSearchChange(searchTerm: string) {
    this.currentFilter.update(filter => ({
      ...filter,
      searchTerm,
      pageNumber: 1,
    }));
    this.loadOrganizations();
  }

  onSportFilterChange(sport: string) {
    this.currentFilter.update(filter => ({
      ...filter,
      sport: sport || undefined,
      pageNumber: 1,
    }));
    this.loadOrganizations();
  }

  onPageChange(event: any) {
    this.currentFilter.update(filter => ({
      ...filter,
      pageNumber: event.pageIndex + 1,
      pageSize: event.pageSize,
    }));
    this.loadOrganizations();
  }

  onSortChange(sortBy: string) {
    this.currentFilter.update(filter => ({
      ...filter,
      sortBy,
      sortDescending: filter.sortBy === sortBy ? !filter.sortDescending : false,
    }));
    this.loadOrganizations();
  }

  onOrganizationSelected(organization: Organization) {
    this.organizationStore.setSelectedOrganization(organization);
    this.selectedTabIndex.set(1); // Switch to details tab
  }

  onOrganizationEdit(organization: Organization) {
    // TODO: Open edit dialog
    console.log('Edit organization:', organization);
  }

  onOrganizationDelete(organization: Organization) {
    // TODO: Open delete confirmation dialog
    console.log('Delete organization:', organization);
  }

  onOrganizationLock(organization: Organization) {
    // TODO: Open lock reason dialog
    console.log('Lock organization:', organization);
  }

  onOrganizationUnlock(organization: Organization) {
    this.organizationStore.unlockOrganization(organization.id);
  }

  onViewPlayers(organization: Organization) {
    // TODO: Navigate to players view
    console.log('View players for organization:', organization);
  }

  onViewPlayerOptions(organization: Organization) {
    // TODO: Navigate to player options view
    console.log('View player options for organization:', organization);
  }

  onCreateOrganization() {
    // TODO: Open create organization dialog
    console.log('Create new organization');
  }

  onRefresh() {
    this.loadOrganizations();
  }

  clearError() {
    this.organizationStore.clearError();
  }

  getSportOptions(): string[] {
    return ['Football', 'Basketball', 'Baseball', 'Soccer', 'Hockey', 'Tennis'];
  }

  getSortOptions(): { value: string; label: string }[] {
    return [
      { value: 'name', label: 'Name' },
      { value: 'createdAt', label: 'Created Date' },
      { value: 'totalVotes', label: 'Total Votes' },
      { value: 'activePlayersCount', label: 'Active Players' },
    ];
  }
}
