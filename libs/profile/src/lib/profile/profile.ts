import { Component, OnInit, inject, effect } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatTabsModule } from "@angular/material/tabs";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatChipsModule } from "@angular/material/chips";
import { MatSnackBarModule, MatSnackBar } from "@angular/material/snack-bar";
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from "@angular/forms";
import {
  ProfileStore,
  UserProfile,
  UpdateProfileRequest,
  UpdatePreferencesRequest,
} from "@sports-ui/profile-data-access";

@Component({
  selector: "lib-profile",
  imports: [
    CommonModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatProgressBarModule,
    MatChipsModule,
    MatSnackBarModule,
    ReactiveFormsModule,
  ],
  templateUrl: "./profile-simple.html",
  styleUrl: "./profile.css",
})
export class Profile implements OnInit {
  private readonly profileStore = inject(ProfileStore);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);

  // Store signals
  profile = this.profileStore.profile;
  isLoading = this.profileStore.isLoading;
  isUpdating = this.profileStore.isUpdating;
  error = this.profileStore.error;
  fullName = this.profileStore.fullName;
  votesPercentageUsed = this.profileStore.votesPercentageUsed;
  accountLevelColor = this.profileStore.accountLevelColor;

  // Forms
  profileForm: FormGroup;
  preferencesForm: FormGroup;

  // UI state
  selectedTabIndex = 0;
  avatarFile: File | null = null;
  avatarPreview: string | null = null;

  constructor() {
    this.profileForm = this.fb.group({
      firstName: ["", [Validators.required, Validators.minLength(2)]],
      lastName: ["", [Validators.required, Validators.minLength(2)]],
      username: ["", [Validators.minLength(3)]],
      phone: [""],
      dateOfBirth: [""],
      bio: ["", [Validators.maxLength(500)]],
    });

    this.preferencesForm = this.fb.group({
      emailNotifications: [true],
      pushNotifications: [false],
      theme: ["auto"],
      language: ["en"],
      timezone: ["America/New_York"],
      profileVisibility: ["public"],
      showEmail: [false],
      showPhone: [false],
    });
  }

  ngOnInit() {
    this.profileStore.loadProfile();

    // Effect to update forms when profile changes
    effect(() => {
      const profile = this.profile();
      if (profile) {
        this.updateForms(profile);
      }
    });

    // Effect to show errors
    effect(() => {
      const error = this.error();
      if (error) {
        this.snackBar.open(error, "Close", {
          duration: 5000,
          horizontalPosition: "right",
          verticalPosition: "top",
        });
      }
    });
  }

  private updateForms(profile: UserProfile) {
    this.profileForm.patchValue({
      firstName: profile.firstName,
      lastName: profile.lastName,
      username: profile.username || "",
      phone: profile.phone || "",
      dateOfBirth: profile.dateOfBirth || "",
      bio: profile.bio || "",
    });

    this.preferencesForm.patchValue({
      emailNotifications: profile.preferences.emailNotifications,
      pushNotifications: profile.preferences.pushNotifications,
      theme: profile.preferences.theme,
      language: profile.preferences.language,
      timezone: profile.preferences.timezone,
      profileVisibility: profile.preferences.privacy.profileVisibility,
      showEmail: profile.preferences.privacy.showEmail,
      showPhone: profile.preferences.privacy.showPhone,
    });
  }

  onTabChange(index: number) {
    this.selectedTabIndex = index;
  }

  onAvatarSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.avatarFile = input.files[0];

      // Create preview
      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        this.avatarPreview = e.target?.result as string;
      };
      reader.readAsDataURL(this.avatarFile);
    }
  }

  uploadAvatar() {
    if (this.avatarFile) {
      this.profileStore.uploadAvatar(this.avatarFile);
      this.avatarFile = null;
      this.avatarPreview = null;
    }
  }

  saveProfile() {
    if (this.profileForm.valid) {
      const updates: UpdateProfileRequest = this.profileForm.value;
      this.profileStore.updateProfile(updates);

      this.snackBar.open("Profile updated successfully!", "Close", {
        duration: 3000,
        horizontalPosition: "right",
        verticalPosition: "top",
      });
    }
  }

  savePreferences() {
    if (this.preferencesForm.valid) {
      const formValue = this.preferencesForm.value;
      const updates: UpdatePreferencesRequest = {
        emailNotifications: formValue.emailNotifications,
        pushNotifications: formValue.pushNotifications,
        theme: formValue.theme,
        language: formValue.language,
        timezone: formValue.timezone,
        privacy: {
          profileVisibility: formValue.profileVisibility,
          showEmail: formValue.showEmail,
          showPhone: formValue.showPhone,
        },
      };

      this.profileStore.updatePreferences(updates);

      this.snackBar.open("Preferences updated successfully!", "Close", {
        duration: 3000,
        horizontalPosition: "right",
        verticalPosition: "top",
      });
    }
  }

  clearError() {
    this.profileStore.clearError();
  }

  getAccountLevelIcon(level: string): string {
    switch (level) {
      case "bronze":
        return "workspace_premium";
      case "silver":
        return "military_tech";
      case "gold":
        return "emoji_events";
      case "platinum":
        return "diamond";
      default:
        return "workspace_premium";
    }
  }
}
