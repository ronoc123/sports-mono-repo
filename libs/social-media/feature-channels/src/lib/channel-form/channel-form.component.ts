import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ChannelStore } from '@sports-ui/social-media-data-access';

@Component({
  selector: 'lib-channel-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="form-container">
      <div class="form-header">
        <button class="btn-back" (click)="cancel()">← Back</button>
        <h2>{{ isEditMode ? 'Edit Channel' : 'New Channel' }}</h2>
      </div>

      <form [formGroup]="form" (ngSubmit)="submit()" class="channel-form">
        <div class="form-field">
          <label for="name">Channel Name *</label>
          <input id="name" formControlName="name" placeholder="e.g. TechReviews" />
          @if (form.get('name')?.invalid && form.get('name')?.touched) {
            <span class="field-error">Channel name is required.</span>
          }
        </div>

        <div class="form-field">
          <label for="description">Description</label>
          <textarea
            id="description"
            formControlName="description"
            rows="3"
            placeholder="Brief description of this channel's content and audience"
          ></textarea>
        </div>

        <div class="form-field">
          <label for="styleToneContext">Style / Tone Context</label>
          <textarea
            id="styleToneContext"
            formControlName="styleToneContext"
            rows="4"
            placeholder="Describe the voice, tone, and content style for AI generation context. e.g. 'High-energy fitness challenges, upbeat tone, 3–5 minute videos targeting home workout enthusiasts.'"
          ></textarea>
          <span class="field-hint">This context seeds AI video generation for this channel.</span>
        </div>

        @if (store.saveError()) {
          <div class="save-error">{{ store.saveError() }}</div>
        }

        <div class="form-actions">
          <button type="button" class="btn-secondary" (click)="cancel()">Cancel</button>
          <button type="submit" class="btn-primary" [disabled]="form.invalid || store.isSaving()">
            {{ store.isSaving() ? 'Saving...' : (isEditMode ? 'Save Changes' : 'Create Channel') }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .form-container { padding: 24px; max-width: 680px; margin: 0 auto; }
    .form-header { display: flex; align-items: center; gap: 16px; margin-bottom: 32px; }
    .form-header h2 { margin: 0; font-size: 24px; }
    .btn-back { background: none; border: none; cursor: pointer; color: #1976d2; font-size: 14px; padding: 0; }
    .channel-form { display: flex; flex-direction: column; gap: 24px; }
    .form-field { display: flex; flex-direction: column; gap: 6px; }
    .form-field label { font-weight: 500; font-size: 14px; color: #333; }
    .form-field input, .form-field textarea {
      border: 1px solid #ddd; border-radius: 6px; padding: 10px 12px;
      font-size: 14px; font-family: inherit; resize: vertical;
    }
    .form-field input:focus, .form-field textarea:focus { outline: none; border-color: #1976d2; }
    .field-error { color: #d32f2f; font-size: 12px; }
    .field-hint { color: #777; font-size: 12px; }
    .save-error { background: #ffebee; color: #c62828; padding: 12px; border-radius: 6px; font-size: 14px; }
    .form-actions { display: flex; justify-content: flex-end; gap: 12px; }
    .btn-primary { background: #1976d2; color: white; border: none; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-secondary { background: white; color: #333; border: 1px solid #ddd; padding: 10px 24px; border-radius: 6px; cursor: pointer; font-size: 14px; }
    .btn-secondary:hover { background: #f5f5f5; }
  `]
})
export class ChannelFormComponent implements OnInit {
  readonly store = inject(ChannelStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  private channelId: string | null = null;

  get isEditMode(): boolean {
    return !!this.channelId;
  }

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', Validators.maxLength(500)],
    styleToneContext: ['', Validators.maxLength(1000)],
  });

  async ngOnInit(): Promise<void> {
    this.store.resetSaveState();
    this.channelId = this.route.snapshot.paramMap.get('id');

    if (this.channelId) {
      await this.store.loadChannel(this.channelId);
      this.populateForm();
    }
  }

  private populateForm(): void {
    const channel = this.store.selectedChannel();
    if (channel) {
      this.form.patchValue({
        name: channel.name,
        description: channel.description,
        styleToneContext: channel.styleToneContext,
      });
    }
  }

  async submit(): Promise<void> {
    if (this.form.invalid) return;

    const { name, description, styleToneContext } = this.form.getRawValue();
    const payload = {
      name: name!,
      description: description ?? '',
      styleToneContext: styleToneContext ?? '',
    };

    if (this.channelId) {
      const success = await this.store.updateChannel(this.channelId, payload);
      if (success) this.router.navigate(['..', this.channelId], { relativeTo: this.route });
    } else {
      const newId = await this.store.createChannel(payload);
      if (newId) this.router.navigate(['..', newId], { relativeTo: this.route });
    }
  }

  cancel(): void {
    if (this.channelId) {
      this.router.navigate(['..', this.channelId], { relativeTo: this.route });
    } else {
      this.router.navigate(['..'], { relativeTo: this.route });
    }
  }
}
