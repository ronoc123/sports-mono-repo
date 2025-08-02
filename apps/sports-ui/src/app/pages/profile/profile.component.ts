import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { Router } from '@angular/router';
import { UserStore } from '@sports-ui/data-access';
import { GoogleAuthService } from '@sports-ui/auth';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly userStore = inject(UserStore);
  private readonly googleAuthService = inject(GoogleAuthService);

  // Store signals
  currentUser = this.userStore.currentUser;

  ngOnInit() {
    this.userStore.loadCurrentUser();
  }

  onEditProfile() {
    // TODO: Navigate to edit profile page
    console.log('Edit profile clicked');
  }

  onChangePassword() {
    // TODO: Navigate to change password page
    console.log('Change password clicked');
  }

  onSignOut() {
    this.googleAuthService.signOut().subscribe({
      next: () => {
        this.router.navigate(['/auth/login']);
      },
      error: (error) => {
        console.error('Sign out error:', error);
      }
    });
  }

  getJoinDate(): string {
    const user = this.currentUser();
    if (user?.createdAt) {
      return new Date(user.createdAt).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    }
    return 'Unknown';
  }

  getLastLoginDate(): string {
    const user = this.currentUser();
    if (user?.lastLoginAt) {
      return new Date(user.lastLoginAt).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    }
    return 'Unknown';
  }

  getUserInitials(): string {
    const user = this.currentUser();
    if (user) {
      const firstName = user.firstName || '';
      const lastName = user.lastName || '';
      if (firstName && lastName) {
        return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
      } else if (user.userName) {
        return user.userName.substring(0, 2).toUpperCase();
      }
    }
    return 'U';
  }

  isGoogleUser(): boolean {
    const user = this.currentUser();
    return user?.provider === 'google';
  }
}
