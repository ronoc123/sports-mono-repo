import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthFacade } from '@sports-ui/auth-data-access';

@Component({
  selector: 'app-trading-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`
    :host { display: block; }

    .page {
      min-height: 100vh;
      background: #0f1117;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem;
    }

    .card {
      background: #1a1d2e;
      border: 1px solid #2a2d3e;
      border-radius: 16px;
      padding: 2.5rem;
      width: 100%;
      max-width: 420px;
    }

    .logo {
      text-align: center;
      margin-bottom: 2rem;
    }

    .logo .icon {
      font-size: 2.5rem;
      display: block;
      margin-bottom: 0.5rem;
    }

    .logo h1 {
      font-size: 1.5rem;
      font-weight: 800;
      color: #f0f0f0;
      margin: 0 0 0.25rem;
    }

    .logo p {
      color: #8b8fa8;
      font-size: 0.85rem;
      margin: 0;
    }

    .tabs {
      display: flex;
      background: #0f1117;
      border-radius: 8px;
      padding: 4px;
      margin-bottom: 1.75rem;
    }

    .tab {
      flex: 1;
      padding: 0.6rem;
      border: none;
      background: transparent;
      color: #8b8fa8;
      font-size: 0.9rem;
      font-weight: 600;
      border-radius: 6px;
      cursor: pointer;
      transition: all 0.2s;
    }

    .tab.active {
      background: #1a1d2e;
      color: #f0f0f0;
      box-shadow: 0 1px 4px rgba(0,0,0,0.3);
    }

    .form-group {
      margin-bottom: 1.1rem;
    }

    .form-group label {
      display: block;
      font-size: 0.8rem;
      color: #8b8fa8;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      margin-bottom: 0.45rem;
      font-weight: 500;
    }

    .form-group input {
      width: 100%;
      background: #0f1117;
      border: 1px solid #2a2d3e;
      border-radius: 8px;
      padding: 0.75rem 1rem;
      color: #f0f0f0;
      font-size: 0.95rem;
      font-family: inherit;
      transition: border-color 0.2s;
    }

    .form-group input:focus {
      outline: none;
      border-color: #00d4aa;
    }

    .form-group input::placeholder {
      color: #4a4d5e;
    }

    .btn-submit {
      width: 100%;
      padding: 0.85rem;
      background: linear-gradient(135deg, #00d4aa, #00b894);
      color: #0f1117;
      font-weight: 700;
      font-size: 1rem;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      margin-top: 0.5rem;
      transition: opacity 0.2s;
      font-family: inherit;
    }

    .btn-submit:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .error-msg {
      background: rgba(255, 77, 109, 0.1);
      border: 1px solid rgba(255, 77, 109, 0.3);
      border-radius: 8px;
      padding: 0.75rem 1rem;
      color: #ff4d6d;
      font-size: 0.88rem;
      margin-bottom: 1rem;
    }

    .divider {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin: 1.5rem 0;
      color: #4a4d5e;
      font-size: 0.8rem;
    }

    .divider::before, .divider::after {
      content: '';
      flex: 1;
      height: 1px;
      background: #2a2d3e;
    }

    .footer-text {
      text-align: center;
      margin-top: 1.5rem;
      color: #8b8fa8;
      font-size: 0.82rem;
      line-height: 1.5;
    }

    .footer-text strong {
      color: #00d4aa;
    }
  `],
  template: `
    <div class="page">
      <div class="card">
        <div class="logo">
          <span class="icon">📈</span>
          <h1>Trading Journal</h1>
          <p>AI-guided trade reflection & review</p>
        </div>

        <div class="tabs">
          <button class="tab" [class.active]="mode() === 'login'" (click)="mode.set('login')">Sign In</button>
          <button class="tab" [class.active]="mode() === 'register'" (click)="mode.set('register')">Register</button>
        </div>

        @if (error()) {
          <div class="error-msg">{{ error() }}</div>
        }

        <form (ngSubmit)="onSubmit()">
          @if (mode() === 'register') {
            <div class="form-group">
              <label>First Name</label>
              <input type="text" [(ngModel)]="firstName" name="firstName" placeholder="Your name" />
            </div>
          }
          <div class="form-group">
            <label>Email</label>
            <input type="email" [(ngModel)]="email" name="email" placeholder="you@example.com" required />
          </div>
          <div class="form-group">
            <label>Password</label>
            <input type="password" [(ngModel)]="password" name="password" placeholder="••••••••" required />
          </div>
          @if (mode() === 'register') {
            <div class="form-group">
              <label>Confirm Password</label>
              <input type="password" [(ngModel)]="confirmPassword" name="confirmPassword" placeholder="••••••••" required />
            </div>
          }
          <button class="btn-submit" type="submit" [disabled]="loading()">
            {{ loading() ? 'Please wait…' : (mode() === 'login' ? 'Sign In' : 'Create Account') }}
          </button>
        </form>

        <div class="footer-text">
          <strong>Trading Journal</strong> · Emotional edge for serious traders
        </div>
      </div>
    </div>
  `,
})
export class TradingLoginComponent {
  private auth = inject(AuthFacade);
  private router = inject(Router);

  mode = signal<'login' | 'register'>('login');
  loading = signal(false);
  error = signal('');

  email = '';
  password = '';
  confirmPassword = '';
  firstName = '';

  async onSubmit() {
    this.error.set('');

    if (this.mode() === 'register') {
      if (this.password !== this.confirmPassword) {
        this.error.set('Passwords do not match');
        return;
      }
    }

    this.loading.set(true);
    try {
      if (this.mode() === 'login') {
        await this.auth.login(this.email, this.password);
      } else {
        await this.auth.register(this.email, this.password, this.confirmPassword, this.firstName || undefined);
      }
      this.router.navigateByUrl('/backtest');
    } catch (e: unknown) {
      this.error.set((e as { message?: string })?.message ?? 'Authentication failed. Please try again.');
    } finally {
      this.loading.set(false);
    }
  }
}
