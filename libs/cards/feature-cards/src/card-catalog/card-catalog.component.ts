import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CardsFacade } from '@sports-ui/cards-data-access';
import { CardPlayer, CreateCardPlayerRequest, UpdateCardPlayerRequest } from '@sports-ui/cards-data-access';
import { OrganizationFeatureService } from '@sports-ui/organization-data-access';

interface CardForm {
  name: string;
  position: string;
  overallRating: number | null;
  leagueId: string;
}

function emptyForm(): CardForm {
  return { name: '', position: '', overallRating: null, leagueId: '' };
}

@Component({
  selector: 'lib-card-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './card-catalog.component.html',
  styleUrl: './card-catalog.component.css',
})
export class CardCatalogComponent implements OnInit {
  private readonly facade = inject(CardsFacade);
  private readonly orgFacade = inject(OrganizationFeatureService);
  private readonly route = inject(ActivatedRoute);

  // ── Exposed facade signals ────────────────────────
  readonly cardPlayers = this.facade.cardPlayers;
  readonly isLoading = this.facade.isLoading;
  readonly isSaving = this.facade.isSaving;
  readonly error = this.facade.error;
  readonly saveError = this.facade.saveError;

  // ── Local UI state ────────────────────────────────
  readonly leagueId = signal('');
  readonly showForm = signal(false);
  readonly editingCard = signal<CardPlayer | null>(null);
  readonly form = signal<CardForm>(emptyForm());

  readonly isEditing = computed(() => this.editingCard() !== null);
  readonly formTitle = computed(() =>
    this.isEditing() ? 'Edit Card Player' : 'Add Card Player'
  );

  readonly rarityColors: Record<string, string> = {
    Common: '#6b7280',
    Rare: '#3b82f6',
    Epic: '#8b5cf6',
    Legendary: '#f59e0b',
  };

  ngOnInit(): void {
    const qp = this.route.snapshot.queryParamMap.get('leagueId');
    if (qp) {
      this.leagueId.set(qp);
      this.facade.loadCardPlayers(qp);
    }
  }

  // ── Actions ───────────────────────────────────────
  onLoadLeague(): void {
    const id = this.leagueId().trim();
    if (!id) return;
    this.facade.loadCardPlayers(id);
  }

  openAddForm(): void {
    this.editingCard.set(null);
    this.form.set({ ...emptyForm(), leagueId: this.leagueId() });
    this.facade.resetSaveState();
    this.showForm.set(true);
  }

  openEditForm(card: CardPlayer): void {
    this.editingCard.set(card);
    this.form.set({
      name: card.name,
      position: card.position,
      overallRating: card.overallRating,
      leagueId: card.leagueId,
    });
    this.facade.resetSaveState();
    this.showForm.set(true);
  }

  cancelForm(): void {
    this.showForm.set(false);
    this.editingCard.set(null);
    this.facade.resetSaveState();
  }

  async submitForm(): Promise<void> {
    const f = this.form();
    const orgId = this.orgFacade.selectedOrganization()?.id;
    if (!orgId || !f.leagueId || !f.name || !f.position || f.overallRating === null) return;

    const editing = this.editingCard();

    if (editing) {
      const request: UpdateCardPlayerRequest = {
        cardPlayerId: editing.id,
        orgId,
        leagueId: f.leagueId,
        name: f.name,
        position: f.position,
        overallRating: f.overallRating!,
      };
      const ok = await this.facade.updateCardPlayer(editing.id, request);
      if (ok) this.cancelForm();
    } else {
      const request: CreateCardPlayerRequest = {
        leagueId: f.leagueId,
        orgId,
        name: f.name,
        position: f.position,
        overallRating: f.overallRating!,
      };
      const ok = await this.facade.createCardPlayer(request);
      if (ok) this.cancelForm();
    }
  }

  updateFormField<K extends keyof CardForm>(field: K, value: CardForm[K]): void {
    this.form.update((f) => ({ ...f, [field]: value }));
  }

  rarityColor(tier: string): string {
    return this.rarityColors[tier] ?? '#6b7280';
  }
}
