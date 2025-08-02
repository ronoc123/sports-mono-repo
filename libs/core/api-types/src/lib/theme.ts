// Theme interfaces matching C# domain models

export interface Theme {
  id: string;
  name: string;
  organizationId: string;
  primaryColor: string;
  secondaryColor?: string;
  accentColor?: string;
  backgroundColor?: string;
  textColor?: string;
  fontFamily?: string;
  logoUrl?: string;
  bannerUrl?: string;
  isActive: boolean;
  isDefault: boolean;
  createdAt: string;
  updatedAt?: string;
  
  // Related entities (optional populated data)
  organization?: Organization;
}

// Request/Response DTOs for Theme operations
export interface CreateThemeRequest {
  name: string;
  organizationId: string;
  primaryColor: string;
  secondaryColor?: string;
  accentColor?: string;
  backgroundColor?: string;
  textColor?: string;
  fontFamily?: string;
  logoUrl?: string;
  bannerUrl?: string;
  isDefault?: boolean;
}

export interface UpdateThemeRequest {
  id: string;
  name?: string;
  primaryColor?: string;
  secondaryColor?: string;
  accentColor?: string;
  backgroundColor?: string;
  textColor?: string;
  fontFamily?: string;
  logoUrl?: string;
  bannerUrl?: string;
  isActive?: boolean;
  isDefault?: boolean;
}

export interface ThemeListResponse {
  themes: Theme[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Predefined theme templates
export interface ThemeTemplate {
  name: string;
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
  backgroundColor: string;
  textColor: string;
  fontFamily: string;
  description: string;
}

export const THEME_TEMPLATES: ThemeTemplate[] = [
  {
    name: 'Classic Blue',
    primaryColor: '#1976d2',
    secondaryColor: '#42a5f5',
    accentColor: '#ff4081',
    backgroundColor: '#fafafa',
    textColor: '#212121',
    fontFamily: 'Roboto, sans-serif',
    description: 'A classic blue theme with modern accents'
  },
  {
    name: 'Forest Green',
    primaryColor: '#388e3c',
    secondaryColor: '#66bb6a',
    accentColor: '#ff9800',
    backgroundColor: '#f1f8e9',
    textColor: '#1b5e20',
    fontFamily: 'Roboto, sans-serif',
    description: 'A natural green theme inspired by forests'
  },
  {
    name: 'Sunset Orange',
    primaryColor: '#f57c00',
    secondaryColor: '#ffb74d',
    accentColor: '#e91e63',
    backgroundColor: '#fff8e1',
    textColor: '#e65100',
    fontFamily: 'Roboto, sans-serif',
    description: 'A warm orange theme reminiscent of sunsets'
  },
  {
    name: 'Royal Purple',
    primaryColor: '#7b1fa2',
    secondaryColor: '#ba68c8',
    accentColor: '#00bcd4',
    backgroundColor: '#f3e5f5',
    textColor: '#4a148c',
    fontFamily: 'Roboto, sans-serif',
    description: 'An elegant purple theme with royal vibes'
  }
];

// Import types from other files
import { Organization } from './organization';
