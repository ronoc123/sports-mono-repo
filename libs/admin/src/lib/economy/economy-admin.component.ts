import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EconomyAdminFacade } from './economy-admin.facade';
import { OrganizationFeatureService } from '@sports-ui/organization-data-access';
import { RarityTierConfigDto } from './economy-admin.model';

@Component({
  selector: 'lib-economy-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './economy-admin.component.html',
  styleUrl: './economy-admin.component.css',
})
export class EconomyAdminComponent implements OnInit {
  private readonly facade = inject(EconomyAdminFacade);
  private readonly orgService = inject(OrganizationFeatureService);

  // ── Facade signals ────────────────────────────────────────────────────────
  readonly rarityTiers = this.facade.rarityTiers;
  readonly packConfig = this.facade.packConfig;
  readonly isLoadingTiers = this.facade.isLoadingTiers;
  readonly isLoadingPack = this.facade.isLoadingPack;
  readonly isSaving = this.facade.isSaving;
  readonly totalPullWeight = this.facade.totalPullWeight;
  readonly tiersError = this.facade.tiersError;
  readonly saveError = this.facade.saveError;

  // ── Local edit state ──────────────────────────────────────────────────────
  readonly editingTierId = signal<string | null>(null);
  readonly editRatingMin = signal(0);
  readonly editRatingMax = signal(99);
  readonly editPullWeightBps = signal(0);
  readonly editPackCost = signal(500);

  async ngOnInit(): Promise<void> {
    const orgId = this.orgService.selectedOrganization()?.id;
    if (orgId) {
      await Promise.all([
        this.facade.loadRarityTiers(orgId),
        this.facade.loadPackConfig(orgId),
      ]);
      const packCost = this.facade.packConfig()?.packPointCost;
      if (packCost) this.editPackCost.set(packCost);
    }
  }

  startEditTier(tier: RarityTierConfigDto): void {
    this.editingTierId.set(tier.id);
    this.editRatingMin.set(tier.ratingMin);
    this.editRatingMax.set(tier.ratingMax);
    this.editPullWeightBps.set(tier.pullWeightBps);
  }

  cancelEditTier(): void {
    this.editingTierId.set(null);
  }

  async saveTier(): Promise<void> {
    const tierId = this.editingTierId();
    if (!tierId) return;
    await this.facade.updateRarityTier(tierId, {
      ratingMin: this.editRatingMin(),
      ratingMax: this.editRatingMax(),
      pullWeightBps: this.editPullWeightBps(),
    });
    this.editingTierId.set(null);
  }

  async savePackConfig(): Promise<void> {
    const orgId = this.orgService.selectedOrganization()?.id;
    if (!orgId) return;
    await this.facade.updatePackConfig(orgId, { packPointCost: this.editPackCost() });
  }

  getRarityColor(rarity: string): string {
    const map: Record<string, string> = {
      Common: '#64748b',
      Rare: '#1d4ed8',
      Epic: '#7c3aed',
      Legendary: '#b45309',
    };
    return map[rarity] ?? '#64748b';
  }

  pullWeightPct(bps: number): string {
    return (bps / 100).toFixed(2) + '%';
  }

  trackById(_: number, item: { id: string }): string {
    return item.id;
  }
}
