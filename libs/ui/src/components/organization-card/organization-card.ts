import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { Organization } from '@sports-ui/api-types';

@Component({
  selector: 'lib-organization-card',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
  ],
  templateUrl: './organization-card.html',
  styleUrl: './organization-card.css',
})
export class OrganizationCardComponent {
  organization = input.required<Organization>();
  showActions = input<boolean>(true);
  
  // Outputs
  selected = output<Organization>();
  edit = output<Organization>();
  delete = output<Organization>();
  lock = output<Organization>();
  unlock = output<Organization>();
  viewPlayers = output<Organization>();
  viewPlayerOptions = output<Organization>();

  onSelect() {
    this.selected.emit(this.organization());
  }

  onEdit() {
    this.edit.emit(this.organization());
  }

  onDelete() {
    this.delete.emit(this.organization());
  }

  onLock() {
    this.lock.emit(this.organization());
  }

  onUnlock() {
    this.unlock.emit(this.organization());
  }

  onViewPlayers() {
    this.viewPlayers.emit(this.organization());
  }

  onViewPlayerOptions() {
    this.viewPlayerOptions.emit(this.organization());
  }

  getStatusColor(): string {
    const org = this.organization();
    if (org.isLocked) return 'warn';
    if (org.hasActivePlayerOptions) return 'primary';
    return 'accent';
  }

  getStatusText(): string {
    const org = this.organization();
    if (org.isLocked) return 'Locked';
    if (org.hasActivePlayerOptions) return 'Active';
    return 'Inactive';
  }
}
