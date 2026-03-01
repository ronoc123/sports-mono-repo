import { Injectable, inject, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthStore } from '@sports-ui/auth-data-access';
import { environment } from '@sports-ui/api-types';
import { BidPlacedEvent, OutbidNotificationEvent, AuctionSettledEvent } from './marketplace.model';

const HUB_URL = `${environment.apiUrl}:5000/hubs/auction`;

@Injectable({ providedIn: 'root' })
export class MarketplaceSignalRService implements OnDestroy {
  private readonly authStore = inject(AuthStore);

  private connection: signalR.HubConnection | null = null;

  readonly bidPlaced$ = new Subject<BidPlacedEvent>();
  readonly outbid$ = new Subject<OutbidNotificationEvent>();
  readonly auctionSettled$ = new Subject<AuctionSettledEvent>();

  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;

    const token = this.authStore.bearer();

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => token ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('BidPlaced', (event: BidPlacedEvent) => {
      this.bidPlaced$.next(event);
    });

    this.connection.on('OutbidNotification', (event: OutbidNotificationEvent) => {
      this.outbid$.next(event);
    });

    this.connection.on('AuctionSettled', (event: AuctionSettledEvent) => {
      this.auctionSettled$.next(event);
    });

    await this.connection.start();
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  async joinListing(listingId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('JoinListing', listingId);
    }
  }

  async leaveListing(listingId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('LeaveListing', listingId);
    }
  }

  ngOnDestroy(): void {
    this.connection?.stop();
    this.bidPlaced$.complete();
    this.outbid$.complete();
    this.auctionSettled$.complete();
  }
}
