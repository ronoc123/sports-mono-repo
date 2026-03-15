import { Component, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { AuthFacade } from '@sports-ui/auth-data-access';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterModule],
  styles: [`
    :host { display: flex; flex-direction: column; height: 100vh; overflow: hidden; }

    /* ── Top bar (mobile only) ── */
    .topbar {
      display: none;
      align-items: center;
      gap: 0.75rem;
      padding: 0 1rem;
      height: 52px;
      background: #13162a;
      border-bottom: 1px solid #2a2d3e;
      flex-shrink: 0;
      z-index: 400;
    }

    .hamburger-btn {
      background: transparent;
      border: none;
      color: #8b8fa8;
      cursor: pointer;
      font-size: 1.4rem;
      line-height: 1;
      padding: 0.25rem;
      border-radius: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: color 0.15s;
    }
    .hamburger-btn:hover { color: #d0d3e8; }

    .topbar-title { font-size: 0.9rem; font-weight: 700; color: #f0f0f0; }

    /* ── Body (sidebar + main) ── */
    .body { display: flex; flex: 1; overflow: hidden; position: relative; }

    /* ── Mobile overlay ── */
    .overlay {
      display: none;
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.5);
      z-index: 300;
      opacity: 0;
      pointer-events: none;
      transition: opacity 0.25s ease;
    }
    .overlay.visible { opacity: 1; pointer-events: auto; }

    /* ── Sidebar ── */
    .sidebar {
      width: 220px;
      background: #13162a;
      border-right: 1px solid #2a2d3e;
      display: flex;
      flex-direction: column;
      flex-shrink: 0;
      padding: 1.5rem 0;
      transition: transform 0.25s ease;
      z-index: 350;
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 0.6rem;
      padding: 0 1.25rem 1.5rem;
      border-bottom: 1px solid #2a2d3e;
    }

    .brand .icon { font-size: 1.4rem; }

    .brand-text h2 {
      font-size: 0.95rem;
      font-weight: 700;
      color: #f0f0f0;
      margin: 0;
      line-height: 1.2;
    }

    .brand-text p {
      font-size: 0.7rem;
      color: #8b8fa8;
      margin: 0;
    }

    .nav {
      flex: 1;
      padding: 1rem 0.75rem;
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
      overflow-y: auto;
    }

    .nav-label {
      font-size: 0.65rem;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      color: #4a4d5e;
      padding: 0.5rem 0.5rem 0.35rem;
      font-weight: 600;
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      padding: 0.65rem 0.75rem;
      border-radius: 8px;
      color: #8b8fa8;
      font-size: 0.88rem;
      font-weight: 500;
      cursor: pointer;
      text-decoration: none;
      transition: all 0.15s;
      border: none;
      background: transparent;
      width: 100%;
      text-align: left;
      font-family: inherit;
    }

    .nav-item:hover { background: rgba(255,255,255,0.04); color: #d0d3e8; }

    .nav-item.active {
      background: rgba(0, 212, 170, 0.1);
      color: #00d4aa;
    }

    .nav-icon { font-size: 1rem; width: 20px; text-align: center; }

    .nav-item.locked { opacity: 0.4; cursor: default; }
    .nav-item.locked:hover { background: transparent; color: #8b8fa8; }

    .lock-badge {
      margin-left: auto;
      font-size: 0.65rem;
      background: rgba(255,77,109,0.15);
      color: #ff4d6d;
      padding: 0.1rem 0.4rem;
      border-radius: 4px;
      font-weight: 600;
    }

    .sidebar-footer {
      padding: 1rem 0.75rem 0;
      border-top: 1px solid #2a2d3e;
    }

    .user-info {
      display: flex;
      align-items: center;
      gap: 0.6rem;
      padding: 0.5rem 0.75rem;
      margin-bottom: 0.25rem;
    }

    .user-avatar {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      background: rgba(0, 212, 170, 0.2);
      color: #00d4aa;
      font-size: 0.75rem;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .user-email {
      font-size: 0.78rem;
      color: #8b8fa8;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .btn-signout {
      display: flex;
      align-items: center;
      gap: 0.6rem;
      padding: 0.6rem 0.75rem;
      border-radius: 8px;
      color: #8b8fa8;
      font-size: 0.85rem;
      cursor: pointer;
      background: transparent;
      border: none;
      width: 100%;
      text-align: left;
      font-family: inherit;
      transition: all 0.15s;
    }

    .btn-signout:hover { background: rgba(255,77,109,0.08); color: #ff4d6d; }

    .main-content {
      flex: 1;
      overflow-y: auto;
      background: #0f1117;
      min-width: 0;
    }

    /* ── Mobile ── */
    @media (max-width: 768px) {
      .topbar { display: flex; }

      .sidebar {
        position: fixed;
        top: 0;
        left: 0;
        bottom: 0;
        padding-top: 1.5rem;
        transform: translateX(-100%);
        box-shadow: 4px 0 24px rgba(0,0,0,0.3);
      }

      .sidebar.open { transform: translateX(0); }

      .overlay { display: block; }
    }
  `],
  template: `
    <!-- Mobile top bar -->
    <div class="topbar">
      <button class="hamburger-btn" (click)="toggleSidebar()" aria-label="Toggle navigation">☰</button>
      <span class="topbar-title">📈 Trading Journal</span>
    </div>

    <div class="body">
      <!-- Mobile overlay -->
      <div class="overlay" [class.visible]="sidebarOpen()" (click)="closeSidebar()"></div>

      <div class="sidebar" [class.open]="sidebarOpen()">
        <div class="brand">
          <span class="icon">📈</span>
          <div class="brand-text">
            <h2>Trading Journal</h2>
            <p>Emotional edge</p>
          </div>
        </div>

        <nav class="nav">
          <div class="nav-label">Setup</div>
          <a class="nav-item" routerLink="/backtest" [class.active]="activeRoute() === 'backtest'" (click)="closeSidebar()">
            <span class="nav-icon">🔬</span>
            Backtest Gate
          </a>

          <div class="nav-label">Trading</div>
          <a class="nav-item" routerLink="/journal" [class.active]="activeRoute() === 'journal'" (click)="closeSidebar()">
            <span class="nav-icon">📋</span>
            Trade Journal
          </a>

          <div class="nav-label">Review</div>
          <a class="nav-item" routerLink="/review" [class.active]="activeRoute() === 'review'" (click)="closeSidebar()">
            <span class="nav-icon">🧠</span>
            Biweekly Review
          </a>
        </nav>

        <div class="sidebar-footer">
          @if (auth.user(); as user) {
            <div class="user-info">
              <div class="user-avatar">{{ (user.firstName || user.email).charAt(0).toUpperCase() }}</div>
              <div class="user-email">{{ user.email }}</div>
            </div>
          }
          <button class="btn-signout" (click)="signOut()">
            <span>↩</span> Sign Out
          </button>
        </div>
      </div>

      <main class="main-content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
})
export class ShellComponent {
  readonly auth = inject(AuthFacade);
  private router = inject(Router);

  activeRoute = signal('');
  sidebarOpen = signal(false);

  constructor() {
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe((e) => {
      const url = (e as NavigationEnd).urlAfterRedirects;
      this.activeRoute.set(url.split('/')[1] ?? '');
      this.sidebarOpen.set(false);
    });
    this.activeRoute.set(this.router.url.split('/')[1] ?? '');
  }

  @HostListener('window:resize')
  onResize() {
    if (window.innerWidth > 768) {
      this.sidebarOpen.set(false);
    }
  }

  toggleSidebar() {
    this.sidebarOpen.update(v => !v);
  }

  closeSidebar() {
    this.sidebarOpen.set(false);
  }

  signOut() {
    this.auth.logout();
  }
}
