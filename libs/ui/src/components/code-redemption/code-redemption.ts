import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CodeRedemptionResponse } from '@sports-ui/api-types';

@Component({
  selector: 'lib-code-redemption',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './code-redemption.html',
  styleUrl: './code-redemption.css',
})
export class CodeRedemptionComponent {
  loading = input<boolean>(false);
  error = input<string | null>(null);
  balance = input<number>(0);

  redeemCode = output<string>();

  redemptionForm: FormGroup;
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

    const alphaBeforeCursor = inputEl.value.slice(0, selStart).replace(/[^A-Z0-9]/gi, '').length;
    const clean = inputEl.value.replace(/[^A-Z0-9]/gi, '').toUpperCase().slice(0, 12);

    let formatted = clean;
    if (clean.length > 4) formatted = clean.slice(0, 4) + '-' + clean.slice(4);
    if (clean.length > 8) formatted = formatted.slice(0, 9) + '-' + formatted.slice(9);

    inputEl.value = formatted;
    this.redemptionForm.patchValue({ codeValue: formatted }, { emitEvent: false });

    let newCursor = formatted.length;
    let count = 0;
    for (let i = 0; i < formatted.length; i++) {
      if (/[A-Z0-9]/i.test(formatted[i])) {
        count++;
        if (count === alphaBeforeCursor) {
          newCursor = i + 1;
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
