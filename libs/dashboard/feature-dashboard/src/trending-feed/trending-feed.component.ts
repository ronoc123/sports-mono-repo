import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { inject } from '@angular/core';
import { TrendingPlayerOptionDto } from '@sports-ui/dashboard-data-access';

@Component({
  selector: 'lib-trending-feed',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './trending-feed.component.html',
  styleUrl: './trending-feed.component.css',
})
export class TrendingFeedComponent {
  private readonly router = inject(Router);

  options = input.required<TrendingPlayerOptionDto[]>();

  navigate(playerOptionId: string): void {
    this.router.navigate(['player-option'], {
      queryParams: { id: playerOptionId },
    });
  }
}
