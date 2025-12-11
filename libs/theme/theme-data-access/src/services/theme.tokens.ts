import { ThemeConfig } from "../models/theme.model";

export const THEME_VARIABLES = {
  colorPrimary: "--color-primary",
  colorSecondary: "--color-secondary",
  colorTertiary: "--color-tertiary",
  colorSurface: "--color-surface",
  colorBorder: "--color-border",
  colorCard: "--color-card",
  colorText: "--color-text",
  colorBackground: "--color-background",
} satisfies Record<keyof ThemeConfig, string>;

export const LIGHT_THEME: ThemeConfig = {
  colorPrimary: "#ffffff",
  colorSecondary: "#f5f5f5",
  colorBorder: "#dddddd",
  colorCard: "#fafafa",
  colorText: "#121212",
  colorTertiary: "#4aa8ff",
  colorSurface: "#121212",
  colorBackground: "#ffffff",
};

export const DARK_THEME: ThemeConfig = {
  colorPrimary: "#181818",
  colorSecondary: "#121212",
  colorBorder: "#222222",
  colorCard: "#111111",
  colorText: "#ffffff",
  colorTertiary: "#667eea",
  colorSurface: "#764ba2",
  colorBackground: "#2a2a2a",
};

export function orgToTheme(org: any): Partial<ThemeConfig> {
  return {
    colorTertiary: org.color1,
    colorSurface: org.color2,
  };
}
