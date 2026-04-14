import { HttpClient } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { MenuItem } from 'primeng/api';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MenuModule } from 'primeng/menu';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { ToolbarModule } from 'primeng/toolbar';

import { AuthService } from '../../core/services/auth.service';

interface KudosFeedItem {
  id: string;
  senderName: string;
  recipientName: string;
  categoryName: string;
  message: string;
  points: number;
  createdAt: string;
}

@Component({
  selector: 'app-feed',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    DatePipe,
    CardModule,
    TagModule,
    ButtonModule,
    ProgressSpinnerModule,
    ToolbarModule,
    MenuModule,
    AvatarModule
  ],
  template: `
    <div class="feed-shell">
      <p-toolbar styleClass="feed-navbar">
        <ng-template pTemplate="start">
          <a routerLink="/feed" class="brand" aria-label="KudosApp home">KudosApp</a>
        </ng-template>

        <ng-template pTemplate="end">
          <nav class="nav-actions" aria-label="Primary navigation">
            <a pButton class="nav-option" routerLink="/kudos/give" label="Give Kudos" [text]="true"></a>
            <a pButton class="nav-option" routerLink="/leaderboard" label="Leaderboard" [text]="true" severity="secondary"></a>

            @if (isAdmin()) {
              <a pButton class="nav-option" routerLink="/admin" label="Admin" [text]="true" severity="contrast"></a>
            }

            @if (isLoggedIn()) {
              <p-avatar
                shape="circle"
                styleClass="user-avatar"
                role="button"
                tabindex="0"
                [attr.aria-label]="'Open user menu for ' + currentUserName()"
                (click)="userMenu.toggle($event)"
                (keydown.enter)="userMenu.toggle($event)"
                (keydown.space)="userMenu.toggle($event)"
              >
                <i class="pi pi-user user-avatar-icon" aria-hidden="true"></i>
              </p-avatar>
              <p-menu #userMenu [popup]="true" [model]="userMenuItems()" appendTo="body"></p-menu>
            }
          </nav>
        </ng-template>
      </p-toolbar>

      <main class="feed-content">
        <section class="feed-header" aria-labelledby="feed-title">
          <p class="eyebrow">Recognition feed</p>
          <h1 id="feed-title">Recent kudos across the team</h1>
          <p class="feed-subtitle">See who is recognizing great work and what impact they are celebrating.</p>
        </section>

        @if (isLoading()) {
          <section class="feed-status" aria-live="polite">
            <p-progressSpinner strokeWidth="4" ariaLabel="Loading kudos feed"></p-progressSpinner>
            <p>Loading kudos feed...</p>
          </section>
        } @else if (errorMessage()) {
          <section class="feed-status error-state" aria-live="polite">
            <h2>Feed unavailable</h2>
            <p>{{ errorMessage() }}</p>
          </section>
        } @else if (!hasKudos()) {
          <section class="feed-status empty-state" aria-live="polite">
            <h2>No kudos yet</h2>
            <p>Recognition will appear here as soon as the first kudos is shared.</p>
          </section>
        } @else {
          <section class="feed-grid" aria-label="Kudos feed">
            @for (kudos of kudosFeed(); track kudos.id) {
              <p-card styleClass="kudos-card">
                <ng-template pTemplate="title">
                  <div class="card-heading">
                    <span>{{ kudos.senderName }} -> {{ kudos.recipientName }}</span>
                    <p-tag [value]="kudos.categoryName" [severity]="categorySeverity(kudos.categoryName)"></p-tag>
                  </div>
                </ng-template>

                <div class="card-body">
                  <p class="message">{{ kudos.message }}</p>
                  <div class="card-meta">
                    <span class="points">{{ kudos.points }} pts</span>
                    <time [attr.datetime]="kudos.createdAt">{{ kudos.createdAt | date: 'MMM d, y' }}</time>
                  </div>
                </div>
              </p-card>
            }
          </section>
        }
      </main>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
      }

      .feed-shell {
        min-height: 100dvh;
        background:
          radial-gradient(circle at top left, color-mix(in srgb, var(--p-primary-color) 18%, transparent) 0, transparent 28%),
          radial-gradient(circle at bottom right, color-mix(in srgb, var(--p-primary-color) 10%, transparent) 0, transparent 30%),
          linear-gradient(180deg, #f7f9fc 0%, #eef4ff 100%);
      }

      .feed-navbar {
        position: sticky;
        top: 0;
        z-index: 10;
        backdrop-filter: blur(16px);
        background: color-mix(in srgb, #ffffff 84%, transparent);
        border-bottom: 1px solid color-mix(in srgb, var(--p-primary-color) 12%, #d9e2f2);
      }

      :host ::ng-deep .feed-navbar {
        border-radius: 0;
        padding: 1rem 1.5rem;
      }

      .brand {
        color: #12324a;
        font-size: 1.35rem;
        font-weight: 800;
        letter-spacing: 0.02em;
        text-decoration: none;
      }

      .nav-actions {
        display: flex;
        align-items: center;
        justify-content: flex-end;
        flex-wrap: wrap;
        gap: 0.75rem;
      }

      :host ::ng-deep .nav-option.p-button {
        padding: 0.35rem 0.5rem;
        font-weight: 600;
      }

      :host ::ng-deep .user-avatar {
        background: color-mix(in srgb, var(--p-primary-color) 18%, #ffffff);
        color: #12324a;
        font-weight: 700;
        cursor: pointer;
      }

      :host ::ng-deep .user-avatar-icon {
        font-size: 1rem;
      }

      .feed-content {
        width: min(1120px, calc(100% - 2rem));
        margin: 0 auto;
        padding: 2rem 0 3rem;
      }

      .feed-header {
        margin-bottom: 1.5rem;
      }

      .eyebrow {
        margin: 0 0 0.5rem;
        color: var(--p-primary-color);
        font-size: 0.85rem;
        font-weight: 700;
        letter-spacing: 0.08em;
        text-transform: uppercase;
      }

      .feed-header h1 {
        margin: 0;
        color: #14324b;
        font-size: clamp(2rem, 4vw, 3rem);
        line-height: 1.05;
      }

      .feed-subtitle {
        max-width: 42rem;
        margin: 0.75rem 0 0;
        color: #466078;
        font-size: 1rem;
      }

      .feed-status {
        min-height: 40dvh;
        display: grid;
        place-content: center;
        gap: 0.9rem;
        text-align: center;
        color: #35516b;
      }

      .empty-state,
      .error-state {
        padding: 2rem;
        border-radius: 1.5rem;
        background: color-mix(in srgb, #ffffff 92%, transparent);
        border: 1px solid color-mix(in srgb, var(--p-primary-color) 10%, #dce7f5);
        box-shadow: 0 18px 40px rgb(17 37 63 / 0.08);
      }

      .feed-grid {
        display: grid;
        gap: 1rem;
        grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      }

      :host ::ng-deep .kudos-card {
        height: 100%;
        border: 1px solid color-mix(in srgb, var(--p-primary-color) 10%, #dce7f5);
        box-shadow: 0 18px 40px rgb(17 37 63 / 0.08);
        background: color-mix(in srgb, #ffffff 94%, transparent);
      }

      .card-heading {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 0.75rem;
        color: #15334c;
        font-size: 1.05rem;
        font-weight: 700;
      }

      .card-body {
        display: grid;
        gap: 1rem;
      }

      .message {
        margin: 0;
        color: #2f4e67;
        line-height: 1.6;
      }

      .card-meta {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        color: #5a7287;
        font-size: 0.95rem;
      }

      .points {
        font-weight: 700;
        color: var(--p-primary-color);
      }

      @media (max-width: 720px) {
        :host ::ng-deep .feed-navbar {
          align-items: flex-start;
          flex-wrap: wrap;
        }

        .nav-actions {
          width: 100%;
          justify-content: flex-start;
        }

        .feed-content {
          width: min(100%, calc(100% - 1.5rem));
          padding-top: 1.5rem;
        }

        .card-heading,
        .card-meta {
          align-items: flex-start;
          flex-direction: column;
        }
      }
    `
  ]
})
export class FeedComponent {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly authVersion = signal(0);
  private readonly feedUrl = 'https://localhost:7162/api/kudos/feed';

  protected readonly kudosFeed = signal<KudosFeedItem[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isLoggedIn = computed(() => {
    this.authVersion();
    return this.authService.isLoggedIn();
  });
  protected readonly isAdmin = computed(() => {
    this.authVersion();
    return this.authService.isAdmin();
  });
  protected readonly currentUserName = computed(() => {
    this.authVersion();
    return this.authService.getUser()?.name ?? '';
  });
  protected readonly userMenuItems = computed<MenuItem[]>(() => [
    {
      label: this.currentUserName(),
      disabled: true
    },
    {
      separator: true
    },
    {
      label: 'Logout',
      icon: 'pi pi-sign-out',
      command: () => this.logout()
    }
  ]);
  protected readonly hasKudos = computed(() => this.kudosFeed().length > 0);

  ngOnInit(): void {
    this.loadFeed();
  }

  protected logout(): void {
    this.authService.logout();
    this.authVersion.update((value) => value + 1);
    void this.router.navigate(['/login']);
  }

  protected categorySeverity(categoryName: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    switch (categoryName.trim().toLowerCase()) {
      case 'teamwork':
      case 'collaboration':
        return 'info';
      case 'innovation':
      case 'leadership':
        return 'success';
      case 'customer focus':
      case 'impact':
        return 'warn';
      case 'excellence':
      case 'ownership':
        return 'contrast';
      default:
        return 'secondary';
    }
  }

  private loadFeed(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.http
      .get<unknown>(this.feedUrl)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (response) => {
          this.kudosFeed.set(this.normalizeFeed(response));
        },
        error: () => {
          this.kudosFeed.set([]);
          this.errorMessage.set('We could not load the kudos feed right now. Please try again in a moment.');
        }
      });
  }

  private normalizeFeed(response: unknown): KudosFeedItem[] {
    if (!Array.isArray(response)) {
      return [];
    }

    return response
      .map((item, index) => this.normalizeFeedItem(item, index))
      .filter((item): item is KudosFeedItem => item !== null);
  }

  private normalizeFeedItem(item: unknown, index: number): KudosFeedItem | null {
    if (!this.isRecord(item)) {
      return null;
    }

    const senderName = this.readString(item, 'senderName') ?? this.readNestedString(item, 'sender', 'name') ?? 'Unknown sender';
    const recipientName =
      this.readString(item, 'recipientName') ?? this.readNestedString(item, 'recipient', 'name') ?? 'Unknown recipient';
    const categoryName =
      this.readString(item, 'categoryName') ?? this.readNestedString(item, 'category', 'name') ?? 'General';
    const message = this.readString(item, 'message') ?? this.readString(item, 'description') ?? 'No message provided.';
    const points = this.readNumber(item, 'points') ?? 0;
    const createdAt =
      this.readString(item, 'createdAt') ??
      this.readString(item, 'createdOn') ??
      this.readString(item, 'dateSent') ??
      new Date().toISOString();
    const id = this.readString(item, 'id') ?? `${senderName}-${recipientName}-${index}`;

    return {
      id,
      senderName,
      recipientName,
      categoryName,
      message,
      points,
      createdAt
    };
  }

  private isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null;
  }

  private readString(record: Record<string, unknown>, key: string): string | null {
    const value = record[key];
    return typeof value === 'string' && value.trim().length > 0 ? value : null;
  }

  private readNumber(record: Record<string, unknown>, key: string): number | null {
    const value = record[key];
    return typeof value === 'number' ? value : null;
  }

  private readNestedString(record: Record<string, unknown>, parentKey: string, childKey: string): string | null {
    const parent = record[parentKey];
    if (!this.isRecord(parent)) {
      return null;
    }

    return this.readString(parent, childKey);
  }
}
