import { HttpClient } from '@angular/common/http';
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
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToolbarModule } from 'primeng/toolbar';

import { AuthService } from '../../core/services/auth.service';

interface LeaderboardEntry {
  userId: string;
  name: string;
  points: number;
  badgesCount: number;
}

@Component({
  selector: 'app-leaderboard',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    ButtonModule,
    CardModule,
    TableModule,
    TagModule,
    ProgressSpinnerModule,
    ToolbarModule,
    MenuModule,
    AvatarModule
  ],
  template: `
    <div class="leaderboard-shell">
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
  icon="pi pi-user"
  shape="circle"
  styleClass="user-avatar"
  role="button"
  tabindex="0"
  [attr.aria-label]="'Open user menu for ' + currentUserName()"
  (click)="userMenu.toggle($event)"
  (keydown.enter)="userMenu.toggle($event)"
  (keydown.space)="userMenu.toggle($event)"
></p-avatar>
<p-menu #userMenu [popup]="true" [model]="userMenuItems()" appendTo="body"></p-menu>            }
          </nav>
        </ng-template>
      </p-toolbar>

      <main class="leaderboard-content">
        <section class="page-header" aria-labelledby="lb-title">
          <p class="eyebrow">Hall of fame</p>
          <h1 id="lb-title">Leaderboard</h1>
          <p class="page-subtitle">The teammates earning the most recognition this period.</p>
        </section>

        @if (isLoading()) {
          <div class="status-panel" aria-live="polite">
            <p-progressSpinner strokeWidth="4" ariaLabel="Loading leaderboard"></p-progressSpinner>
            <p>Loading leaderboard...</p>
          </div>
        } @else if (errorMessage()) {
          <div class="status-panel error-state" aria-live="polite">
            <h2>Leaderboard unavailable</h2>
            <p>{{ errorMessage() }}</p>
          </div>
        } @else if (!hasEntries()) {
          <div class="status-panel empty-state" aria-live="polite">
            <h2>No entries yet</h2>
            <p>Rankings will appear here once kudos have been shared.</p>
          </div>
        } @else {
          <p-card styleClass="lb-card">
            <p-table
              [value]="leaderboard()"
              [rowHover]="true"
              responsiveLayout="scroll"
              aria-label="Kudos leaderboard"
            >
              <ng-template pTemplate="header">
                <tr>
                  <th scope="col" style="width: 5rem">Rank</th>
                  <th scope="col">Name</th>
                  <th scope="col" style="width: 8rem">Points</th>
                  <th scope="col" style="width: 8rem">Badges</th>
                </tr>
              </ng-template>

              <ng-template pTemplate="body" let-entry let-rowIndex="rowIndex">
                <tr [class]="rowClass(rowIndex)">
                  <td class="rank-cell" [attr.aria-label]="rankLabel(rowIndex)">
                    {{ rankDisplay(rowIndex) }}
                  </td>
                  <td>{{ entry.name }}</td>
                  <td class="points-cell">{{ entry.points }}</td>
                  <td>
                    @if (entry.badgesCount > 0) {
                      <p-tag [value]="entry.badgesCount + ' badge' + (entry.badgesCount === 1 ? '' : 's')" severity="info"></p-tag>
                    } @else {
                      <span class="no-badges">—</span>
                    }
                  </td>
                </tr>
              </ng-template>
            </p-table>
          </p-card>
        }
      </main>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
      }

      .leaderboard-shell {
        min-height: 100dvh;
        background:
          radial-gradient(circle at 20% 5%, color-mix(in srgb, var(--p-primary-color) 16%, transparent) 0, transparent 30%),
          linear-gradient(180deg, #f7f9fc 0%, #f0f5ff 100%);
      }

      /* ── Navbar (mirrors feed) ────────────────────────── */
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

      /* ── Page layout ──────────────────────────────────── */
      .leaderboard-content {
        width: min(900px, calc(100% - 2rem));
        margin: 0 auto;
        padding: 2rem 0 3rem;
      }

      .page-header {
        margin-bottom: 1.5rem;
      }

      .eyebrow {
        margin: 0 0 0.5rem;
        color: var(--p-primary-color);
        font-size: 0.82rem;
        font-weight: 700;
        letter-spacing: 0.08em;
        text-transform: uppercase;
      }

      .page-header h1 {
        margin: 0;
        color: #14324b;
        font-size: clamp(2rem, 4vw, 3rem);
        line-height: 1.05;
      }

      .page-subtitle {
        margin: 0.75rem 0 0;
        color: #466078;
        font-size: 1rem;
      }

      /* ── Status panels ────────────────────────────────── */
      .status-panel {
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

      /* ── Table ────────────────────────────────────────── */
      :host ::ng-deep .lb-card {
        border: 1px solid color-mix(in srgb, var(--p-primary-color) 10%, #dce7f5);
        box-shadow: 0 18px 40px rgb(17 37 63 / 0.08);
      }

      .rank-cell {
        font-size: 1.15rem;
        text-align: center;
      }

      .points-cell {
        font-weight: 700;
        color: var(--p-primary-color);
      }

      .no-badges {
        color: #9bafc2;
      }

      :host ::ng-deep tr.rank-1 td {
        background: color-mix(in srgb, #ffd700 18%, transparent) !important;
      }

      :host ::ng-deep tr.rank-2 td {
        background: color-mix(in srgb, #c0c0c0 18%, transparent) !important;
      }

      :host ::ng-deep tr.rank-3 td {
        background: color-mix(in srgb, #cd7f32 18%, transparent) !important;
      }

      /* ── Responsive ───────────────────────────────────── */
      @media (max-width: 720px) {
        :host ::ng-deep .feed-navbar {
          align-items: flex-start;
          flex-wrap: wrap;
        }

        .nav-actions {
          width: 100%;
          justify-content: flex-start;
        }

        .leaderboard-content {
          width: min(100%, calc(100% - 1.5rem));
          padding-top: 1.5rem;
        }
      }
    `
  ]
})
export class LeaderboardComponent {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly authVersion = signal(0);
  private readonly apiUrl = 'https://localhost:7162/api';

  protected readonly leaderboard = signal<LeaderboardEntry[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly hasEntries = computed(() => this.leaderboard().length > 0);

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

  ngOnInit(): void {
    this.loadLeaderboard();
  }

  protected logout(): void {
    this.authService.logout();
    this.authVersion.update((v) => v + 1);
    void this.router.navigate(['/login']);
  }

  protected rankDisplay(index: number): string {
    switch (index) {
      case 0: return '🥇';
      case 1: return '🥈';
      case 2: return '🥉';
      default: return String(index + 1);
    }
  }

  protected rankLabel(index: number): string {
    switch (index) {
      case 0: return 'Rank 1 – Gold';
      case 1: return 'Rank 2 – Silver';
      case 2: return 'Rank 3 – Bronze';
      default: return `Rank ${index + 1}`;
    }
  }

  protected rowClass(index: number): string {
    switch (index) {
      case 0: return 'rank-1';
      case 1: return 'rank-2';
      case 2: return 'rank-3';
      default: return '';
    }
  }

  private loadLeaderboard(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.http
      .get<unknown>(`${this.apiUrl}/leaderboard`)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (response) => {
          this.leaderboard.set(this.normalizeLeaderboard(response));
        },
        error: () => {
          this.leaderboard.set([]);
          this.errorMessage.set('We could not load the leaderboard right now. Please try again in a moment.');
        }
      });
  }

  private normalizeLeaderboard(response: unknown): LeaderboardEntry[] {
    if (!Array.isArray(response)) {
      return [];
    }

    return response
      .map((item, index) => this.normalizeEntry(item, index))
      .filter((entry): entry is LeaderboardEntry => entry !== null);
  }

  private normalizeEntry(item: unknown, fallbackIndex: number): LeaderboardEntry | null {
    if (!this.isRecord(item)) {
      return null;
    }

    const userId =
      this.readString(item, 'userId') ??
      this.readString(item, 'id') ??
      String(fallbackIndex);
    const name = this.readString(item, 'name') ?? this.readString(item, 'fullName') ?? 'Unknown';
    const points = this.readNumber(item, 'points') ?? 0;
    const badgesCount =
      this.readNumber(item, 'badgesCount') ??
      this.readNumber(item, 'badges') ??
      this.readArrayLength(item, 'badges') ??
      0;

    return { userId, name, points, badgesCount };
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

  private readArrayLength(record: Record<string, unknown>, key: string): number | null {
    const value = record[key];
    return Array.isArray(value) ? value.length : null;
  }
}
