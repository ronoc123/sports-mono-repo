import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { CodeRedemptionResponse } from '@sports-ui/api-types';

@Component({
  selector: 'lib-code-redemption',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './code-redemption.html',
  styleUrl: './code-redemption.css',
})
export class CodeRedemptionComponent {
  loading = input<boolean>(false);
  error = input<string | null>(null);
  
  // Outputs
  redeemCode = output<string>();
  
  // Form
  redemptionForm: FormGroup;
  
  // Local state
  redemptionResult = signal<CodeRedemptionResponse | null>(null);
  showResult = signal<boolean>(false);

  constructor(private fb: FormBuilder) {
    this.redemptionForm = this.fb.group({
      codeValue: ['', [Validators.required, Validators.pattern(/^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$/)]],
    });
  }

  onSubmit() {
    if (this.redemptionForm.valid) {
      const formatted = this.redemptionForm.get('codeValue')?.value as string;
      this.redeemCode.emit(formatted);
    }
  }

  onCodeInput(event: Event) {
    const inputEl = event.target as HTMLInputElement;
    const selStart = inputEl.selectionStart ?? inputEl.value.length;

    // Count alphanumeric chars before the cursor in the current value
    const alphaBeforeCursor = inputEl.value.slice(0, selStart).replace(/[^A-Z0-9]/gi, '').length;

    // Strip non-alphanumeric, uppercase, cap at 12 chars
    const clean = inputEl.value.replace(/[^A-Z0-9]/gi, '').toUpperCase().slice(0, 12);

    // Format as XXXX-XXXX-XXXX
    let formatted = clean;
    if (clean.length > 4) formatted = clean.slice(0, 4) + '-' + clean.slice(4);
    if (clean.length > 8) formatted = formatted.slice(0, 9) + '-' + formatted.slice(9);

    // Update DOM directly and sync form model
    inputEl.value = formatted;
    this.redemptionForm.patchValue({ codeValue: formatted }, { emitEvent: false });

    // Restore cursor: walk formatted string counting alphanumeric chars
    let newCursor = formatted.length;
    let count = 0;
    for (let i = 0; i < formatted.length; i++) {
      if (/[A-Z0-9]/i.test(formatted[i])) {
        count++;
        if (count === alphaBeforeCursor) {
          newCursor = i + 1;
          // Skip past a dash immediately after the cursor
          if (newCursor < formatted.length && formatted[newCursor] === '-') newCursor++;
          break;
        }
      }
    }
    if (alphaBeforeCursor === 0) newCursor = 0;
    inputEl.setSelectionRange(newCursor, newCursor);
  }

  clearForm() {
    this.redemptionForm.reset();
    this.showResult.set(false);
    this.redemptionResult.set(null);
  }

  setRedemptionResult(result: CodeRedemptionResponse) {
    this.redemptionResult.set(result);
    this.showResult.set(true);
    if (result.success) {
      this.clearForm();
    }
  }

  get codeValue() {
    return this.redemptionForm.get('codeValue');
  }

  get isFormValid() {
    return this.redemptionForm.valid;
  }
}
