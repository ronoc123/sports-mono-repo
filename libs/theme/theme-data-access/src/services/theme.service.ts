import { Injectable } from "@angular/core";
import { ThemeConfig } from "../models/theme.model";
import { THEME_VARIABLES, LIGHT_THEME, DARK_THEME } from "./theme.tokens";

@Injectable({ providedIn: "root" })
export class ThemeService {
  private currentTheme: ThemeConfig = LIGHT_THEME;
  private orgOverrides: Partial<ThemeConfig> = {};

  setMode(mode: "light" | "dark") {
    const base = mode === "light" ? LIGHT_THEME : DARK_THEME;

    this.applyTheme(base);

    if (this.orgOverrides) {
      this.applyTheme(this.orgOverrides);
    }
  }

  applyOrgTheme(org: any) {
    this.orgOverrides = {
      colorTertiary: org.color1,
      colorSurface: org.color2,
    };

    this.applyTheme(this.orgOverrides);
  }

  private applyTheme(overrides: Partial<ThemeConfig>) {
    this.currentTheme = { ...this.currentTheme, ...overrides };

    const root = document.documentElement;

    for (const [key, value] of Object.entries(this.currentTheme) as [
      keyof ThemeConfig,
      string
    ][]) {
      const cssVarName = THEME_VARIABLES[key];
      root.style.setProperty(cssVarName, value);
    }
  }
}
