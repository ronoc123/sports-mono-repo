import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService { 
  applyTheme(themeVars: { [key: string]: string }) {
    const root = document.documentElement;

    for (const [key, value] of Object.entries(themeVars)) {
      root.style.setProperty(`--${key}`, value);
    }
  }
}
