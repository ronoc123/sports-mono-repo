import { Component, input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
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
  private readonly route = inject(ActivatedRoute);

  options = input.required<TrendingPlayerOptionDto[]>();

  navigate(playerOptionId: string): void {
    this.router.navigate(['../player-option'], {
      relativeTo: this.route,
      queryParams: { id: playerOptionId },
    });
  }
}
