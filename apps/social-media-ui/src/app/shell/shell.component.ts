import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterModule],
  styles: [`
    :host { display: flex; height: 100vh; overflow: hidden; }

    .sidebar {
      width: 240px;
      flex-shrink: 0;
      background: #0f1117;
      display: flex;
      flex-direction: column;
      padding: 0;
      border-right: 1px solid #1e2030;
      transition: transform 0.25s ease;
    }

    .sidebar-brand {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 20px 20px 16px;
      border-bottom: 1px solid #1e2030;
    }

    .brand-icon { font-size: 22px; }

    .brand-text h2 {
      font-size: 14px;
      font-weight: 700;
      color: #f0f0f0;
      margin: 0;
      letter-spacing: -0.01em;
    }

    .brand-text span {
      font-size: 11px;
      color: #5a5d72;
      font-weight: 400;
    }

    .nav-section {
      padding: 16px 12px 8px;
      flex: 1;
    }

    .nav-section-label {
      font-size: 10px;
      font-weight: 600;
      color: #3a3d52;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      padding: 0 8px;
      margin-bottom: 6px;
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 9px 10px;
      border-radius: 8px;
      cursor: pointer;
      color: #8b8fa8;
      font-size: 13.5px;
      font-weight: 500;
      text-decoration: none;
      transition: background 0.15s, color 0.15s;
      margin-bottom: 2px;
    }

    .nav-item:hover { background: #1a1d2e; color: #d0d3e8; }

    .nav-item.active { background: #1e2240; color: #818cf8; }

    .nav-item .nav-icon { font-size: 16px; opacity: 0.85; }

    .sidebar-footer {
      padding: 14px 16px;
      border-top: 1px solid #1e2030;
      font-size: 11px;
      color: #3a3d52;
      text-align: center;
    }

    .main {
      flex: 1;
      overflow-y: auto;
      background: #f5f6fa;
    }

    .mobile-toggle {
      display: none;
      position: fixed;
      top: 12px;
      left: 12px;
      z-index: 200;
      background: #1a1d2e;
      border: 1px solid #2a2d3e;
      border-radius: 8px;
      color: #d0d3e8;
      font-size: 18px;
      width: 38px;
      height: 38px;
      cursor: pointer;
      align-items: center;
      justify-content: center;
    }

    .overlay {
      display: none;
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.5);
      z-index: 99;
    }

    @media (max-width: 768px) {
      .sidebar {
        position: fixed;
        top: 0;
        left: 0;
        height: 100vh;
        z-index: 100;
        transform: translateX(-100%);
      }

      .sidebar.open { transform: translateX(0); }

      .main { width: 100%; padding-top: 56px; }

      .mobile-toggle { display: flex; }

      .overlay.open { display: block; }
    }
  `],
  template: `
    <button class="mobile-toggle" (click)="sidebarOpen.set(true)">☰</button>
    <div class="overlay" [class.open]="sidebarOpen()" (click)="sidebarOpen.set(false)"></div>

    <nav class="sidebar" [class.open]="sidebarOpen()">
      <div class="sidebar-brand">
        <span class="brand-icon">🎬</span>
        <div class="brand-text">
          <h2>Social Media AI</h2>
          <span>Video publishing tool</span>
        </div>
      </div>

      <div class="nav-section">
        <div class="nav-section-label">Channels</div>
        <a class="nav-item" routerLink="/channels" routerLinkActive="active"
           [routerLinkActiveOptions]="{ exact: false }" (click)="sidebarOpen.set(false)">
          <span class="nav-icon">📺</span>
          All Channels
        </a>
        <a class="nav-item" routerLink="/channels/new" routerLinkActive="active"
           (click)="sidebarOpen.set(false)">
          <span class="nav-icon">➕</span>
          New Channel
        </a>
      </div>

      <div class="sidebar-footer">Social Media AI</div>
    </nav>

    <main class="main">
      <router-outlet></router-outlet>
    </main>
  `,
})
export class ShellComponent {
  sidebarOpen = signal(false);
}
