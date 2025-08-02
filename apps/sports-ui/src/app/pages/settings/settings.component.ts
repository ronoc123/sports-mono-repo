import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { UserStore } from '@sports-ui/data-access';

interface AppSettings {
  theme: 'light' | 'dark' | 'auto';
  notifications: boolean;
  emailNotifications: boolean;
  autoRefresh: boolean;
  language: string;
  timezone: string;
}

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatSelectModule,
    MatFormFieldModule,
    MatDividerModule,
    MatSnackBarModule,
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css',
})
export class SettingsComponent {
  private readonly userStore = inject(UserStore);
  private readonly snackBar = inject(MatSnackBar);

  // Store signals
  currentUser = this.userStore.currentUser;

  // Local settings state
  settings = signal<AppSettings>({
    theme: 'light',
    notifications: true,
    emailNotifications: true,
    autoRefresh: false,
    language: 'en',
    timezone: 'America/New_York',
  });

  // Available options
  themes = [
    { value: 'light', label: 'Light' },
    { value: 'dark', label: 'Dark' },
    { value: 'auto', label: 'Auto (System)' },
  ];

  languages = [
    { value: 'en', label: 'English' },
    { value: 'es', label: 'Spanish' },
    { value: 'fr', label: 'French' },
    { value: 'de', label: 'German' },
  ];

  timezones = [
    { value: 'America/New_York', label: 'Eastern Time (ET)' },
    { value: 'America/Chicago', label: 'Central Time (CT)' },
    { value: 'America/Denver', label: 'Mountain Time (MT)' },
    { value: 'America/Los_Angeles', label: 'Pacific Time (PT)' },
    { value: 'UTC', label: 'UTC' },
  ];

  constructor() {
    this.loadSettings();
  }

  loadSettings() {
    // Load settings from localStorage or API
    const savedSettings = localStorage.getItem('app_settings');
    if (savedSettings) {
      try {
        const parsed = JSON.parse(savedSettings);
        this.settings.set({ ...this.settings(), ...parsed });
      } catch (error) {
        console.error('Failed to parse saved settings:', error);
      }
    }
  }

  updateSetting<K extends keyof AppSettings>(key: K, value: AppSettings[K]) {
    this.settings.update(current => ({
      ...current,
      [key]: value
    }));
  }

  saveSettings() {
    try {
      localStorage.setItem('app_settings', JSON.stringify(this.settings()));
      this.snackBar.open('Settings saved successfully!', 'Close', {
        duration: 3000,
        panelClass: ['success-snackbar']
      });
    } catch (error) {
      console.error('Failed to save settings:', error);
      this.snackBar.open('Failed to save settings', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar']
      });
    }
  }

  resetSettings() {
    this.settings.set({
      theme: 'light',
      notifications: true,
      emailNotifications: true,
      autoRefresh: false,
      language: 'en',
      timezone: 'America/New_York',
    });
    
    this.snackBar.open('Settings reset to defaults', 'Close', {
      duration: 3000,
      panelClass: ['info-snackbar']
    });
  }

  exportSettings() {
    const dataStr = JSON.stringify(this.settings(), null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = 'sports-ui-settings.json';
    link.click();
    
    URL.revokeObjectURL(url);
    
    this.snackBar.open('Settings exported successfully!', 'Close', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e) => {
        try {
          const settings = JSON.parse(e.target?.result as string);
          this.settings.set({ ...this.settings(), ...settings });
          this.snackBar.open('Settings imported successfully!', 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
        } catch (error) {
          console.error('Failed to import settings:', error);
          this.snackBar.open('Failed to import settings', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar']
          });
        }
      };
      reader.readAsText(file);
    }
  }

  clearData() {
    if (confirm('Are you sure you want to clear all local data? This action cannot be undone.')) {
      localStorage.clear();
      sessionStorage.clear();
      this.resetSettings();
      this.snackBar.open('All local data cleared', 'Close', {
        duration: 3000,
        panelClass: ['warning-snackbar']
      });
    }
  }
}
