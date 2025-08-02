import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from '@sports-ui/http-client';
import {
  Theme,
  ServiceResponse,
  PaginatedResponse,
  ThemeFilter,
  CreateThemeRequest,
  UpdateThemeRequest,
  THEME_TEMPLATES,
  ThemeTemplate,
} from '@sports-ui/api-types';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly apiService = inject(ApiService);

  /**
   * Get all themes with pagination and filtering
   */
  getThemes(filter: ThemeFilter): Observable<ServiceResponse<PaginatedResponse<Theme>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.searchTerm) {
      params = params.set('searchTerm', filter.searchTerm);
    }
    if (filter.organizationId) {
      params = params.set('organizationId', filter.organizationId);
    }
    if (filter.isActive !== undefined) {
      params = params.set('isActive', filter.isActive.toString());
    }
    if (filter.isDefault !== undefined) {
      params = params.set('isDefault', filter.isDefault.toString());
    }
    if (filter.sortBy) {
      params = params.set('sortBy', filter.sortBy);
    }
    if (filter.sortDescending !== undefined) {
      params = params.set('sortDescending', filter.sortDescending.toString());
    }

    return this.apiService.get<ServiceResponse<PaginatedResponse<Theme>>>('api/Theme/all', params);
  }

  /**
   * Get a single theme by ID
   */
  getTheme(themeId: string): Observable<ServiceResponse<Theme>> {
    return this.apiService.get<ServiceResponse<Theme>>(`api/Theme/${themeId}`);
  }

  /**
   * Create a new theme
   */
  createTheme(theme: CreateThemeRequest): Observable<ServiceResponse<string>> {
    return this.apiService.post<ServiceResponse<string>, CreateThemeRequest>('api/Theme/create', theme);
  }

  /**
   * Update an existing theme
   */
  updateTheme(theme: UpdateThemeRequest): Observable<ServiceResponse<boolean>> {
    return this.apiService.put<ServiceResponse<boolean>, UpdateThemeRequest>('api/Theme/update', theme);
  }

  /**
   * Delete a theme
   */
  deleteTheme(themeId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.delete<ServiceResponse<boolean>>(`api/Theme/delete/${themeId}`);
  }

  /**
   * Activate a theme
   */
  activateTheme(themeId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { themeId: string }>('api/Theme/activate', { themeId });
  }

  /**
   * Deactivate a theme
   */
  deactivateTheme(themeId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { themeId: string }>('api/Theme/deactivate', { themeId });
  }

  /**
   * Set theme as default
   */
  setAsDefault(themeId: string): Observable<ServiceResponse<boolean>> {
    return this.apiService.post<ServiceResponse<boolean>, { themeId: string }>('api/Theme/set-default', { themeId });
  }

  /**
   * Get themes by organization
   */
  getThemesByOrganization(organizationId: string): Observable<ServiceResponse<Theme[]>> {
    const params = new HttpParams().set('organizationId', organizationId);
    return this.apiService.get<ServiceResponse<Theme[]>>('api/Theme/by-organization', params);
  }

  /**
   * Get active themes
   */
  getActiveThemes(): Observable<ServiceResponse<Theme[]>> {
    return this.apiService.get<ServiceResponse<Theme[]>>('api/Theme/active');
  }

  /**
   * Get default themes
   */
  getDefaultThemes(): Observable<ServiceResponse<Theme[]>> {
    return this.apiService.get<ServiceResponse<Theme[]>>('api/Theme/default');
  }

  /**
   * Get organization's active theme
   */
  getOrganizationActiveTheme(organizationId: string): Observable<ServiceResponse<Theme | null>> {
    return this.apiService.get<ServiceResponse<Theme | null>>(`api/Theme/organization/${organizationId}/active`);
  }

  /**
   * Get organization's default theme
   */
  getOrganizationDefaultTheme(organizationId: string): Observable<ServiceResponse<Theme | null>> {
    return this.apiService.get<ServiceResponse<Theme | null>>(`api/Theme/organization/${organizationId}/default`);
  }

  /**
   * Clone a theme
   */
  cloneTheme(themeId: string, newName: string, organizationId?: string): Observable<ServiceResponse<string>> {
    return this.apiService.post<ServiceResponse<string>, { themeId: string; newName: string; organizationId?: string }>(
      'api/Theme/clone',
      { themeId, newName, organizationId }
    );
  }

  /**
   * Create theme from template
   */
  createThemeFromTemplate(template: ThemeTemplate, organizationId: string, name?: string): Observable<ServiceResponse<string>> {
    const request: CreateThemeRequest = {
      name: name || template.name,
      organizationId,
      primaryColor: template.primaryColor,
      secondaryColor: template.secondaryColor,
      accentColor: template.accentColor,
      backgroundColor: template.backgroundColor,
      textColor: template.textColor,
      fontFamily: template.fontFamily,
    };

    return this.createTheme(request);
  }

  /**
   * Get theme templates
   */
  getThemeTemplates(): ThemeTemplate[] {
    return THEME_TEMPLATES;
  }

  /**
   * Preview theme
   */
  previewTheme(themeId: string): Observable<ServiceResponse<ThemePreview>> {
    return this.apiService.get<ServiceResponse<ThemePreview>>(`api/Theme/${themeId}/preview`);
  }

  /**
   * Export theme
   */
  exportTheme(themeId: string): Observable<ServiceResponse<ThemeExport>> {
    return this.apiService.get<ServiceResponse<ThemeExport>>(`api/Theme/${themeId}/export`);
  }

  /**
   * Import theme
   */
  importTheme(themeData: ThemeExport, organizationId: string): Observable<ServiceResponse<string>> {
    return this.apiService.post<ServiceResponse<string>, { themeData: ThemeExport; organizationId: string }>(
      'api/Theme/import',
      { themeData, organizationId }
    );
  }

  /**
   * Validate theme colors
   */
  validateThemeColors(colors: ThemeColors): Observable<ServiceResponse<ThemeValidationResult>> {
    return this.apiService.post<ServiceResponse<ThemeValidationResult>, ThemeColors>(
      'api/Theme/validate-colors',
      colors
    );
  }

  /**
   * Generate theme from primary color
   */
  generateThemeFromColor(primaryColor: string, organizationId: string, name: string): Observable<ServiceResponse<string>> {
    return this.apiService.post<ServiceResponse<string>, { primaryColor: string; organizationId: string; name: string }>(
      'api/Theme/generate-from-color',
      { primaryColor, organizationId, name }
    );
  }
}

// Additional interfaces for theme operations
export interface ThemePreview {
  themeId: string;
  name: string;
  previewUrl: string;
  colors: ThemeColors;
  sampleComponents: string[];
}

export interface ThemeExport {
  name: string;
  primaryColor: string;
  secondaryColor?: string;
  accentColor?: string;
  backgroundColor?: string;
  textColor?: string;
  fontFamily?: string;
  logoUrl?: string;
  bannerUrl?: string;
  metadata: {
    exportedAt: string;
    version: string;
    organizationName?: string;
  };
}

export interface ThemeColors {
  primaryColor: string;
  secondaryColor?: string;
  accentColor?: string;
  backgroundColor?: string;
  textColor?: string;
}

export interface ThemeValidationResult {
  isValid: boolean;
  errors: string[];
  warnings: string[];
  suggestions: string[];
  contrastRatios: {
    primaryOnBackground: number;
    textOnBackground: number;
    textOnPrimary: number;
  };
  accessibility: {
    wcagAA: boolean;
    wcagAAA: boolean;
  };
}
