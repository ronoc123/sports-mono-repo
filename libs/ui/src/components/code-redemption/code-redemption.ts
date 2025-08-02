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
      codeValue: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit() {
    if (this.redemptionForm.valid) {
      const codeValue = this.redemptionForm.get('codeValue')?.value;
      this.redeemCode.emit(codeValue);
    }
  }

  onCodeInput(event: Event) {
    const input = event.target as HTMLInputElement;
    // Convert to uppercase and remove spaces
    const cleanCode = input.value.toUpperCase().replace(/\s/g, '');
    this.redemptionForm.patchValue({ codeValue: cleanCode });
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
