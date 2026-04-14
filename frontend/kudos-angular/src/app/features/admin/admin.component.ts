import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { MenuItem, MessageService } from 'primeng/api';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MenuModule } from 'primeng/menu';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { AuthService } from '../../core/services/auth.service';

interface AnalyticsCard {
  totalUsers: number;
  totalKudos: number;
  totalPoints: number;
  topCategory: string;
}

interface AdminUser {
  id: string;
  name: string;
  email: string;
  role: 'User' | 'Admin';
  points: number;
}

@Component({
  selector: 'app-admin',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    RouterLink,
    CardModule,
    TableModule,
    ButtonModule,
    TagModule,
    ToastModule,
    ToolbarModule,
    MenuModule,
    AvatarModule,
    ProgressSpinnerModule,
    ToggleSwitchModule
  ],
  providers: [MessageService],
  template: `
    <p-toast></p-toast>

    <div class="admin-shell">
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

      <main class="admin-content">
        <section class="page-header" aria-labelledby="admin-title">
          <p class="eyebrow">Administration</p>
          <h1 id="admin-title">Admin dashboard</h1>
          <p class="page-subtitle">Manage user roles and monitor key recognition metrics.</p>
        </section>

        @if (isLoading()) {
          <section class="status-panel" aria-live="polite">
            <p-progressSpinner strokeWidth="4" ariaLabel="Loading admin dashboard"></p-progressSpinner>
            <p>Loading analytics and users...</p>
          </section>
        } @else if (errorMessage()) {
          <section class="status-panel error-state" aria-live="polite">
            <h2>Dashboard unavailable</h2>
            <p>{{ errorMessage() }}</p>
          </section>
        } @else {
          <section class="stats-grid" aria-label="Analytics summary">
            <p-card>
              <ng-template pTemplate="title">Total Users</ng-template>
              <p class="stat-value">{{ analytics().totalUsers }}</p>
            </p-card>
            <p-card>
              <ng-template pTemplate="title">Total Kudos</ng-template>
              <p class="stat-value">{{ analytics().totalKudos }}</p>
            </p-card>
            <p-card>
              <ng-template pTemplate="title">Total Points</ng-template>
              <p class="stat-value">{{ analytics().totalPoints }}</p>
            </p-card>
            <p-card>
              <ng-template pTemplate="title">Top Category</ng-template>
              <p class="stat-value top-category">{{ analytics().topCategory }}</p>
            </p-card>
          </section>

          <section class="users-section" aria-labelledby="users-title">
            <div class="users-header">
              <h2 id="users-title">Users</h2>
              <p-tag [value]="users().length + ' users'" severity="info"></p-tag>
            </div>

            @if (!hasUsers()) {
              <div class="empty-state">
                <p>No users available.</p>
              </div>
            } @else {
              <p-table [value]="users()" [rowHover]="true" responsiveLayout="scroll" aria-label="Admin users table">
                <ng-template pTemplate="header">
                  <tr>
                    <th scope="col">Name</th>
                    <th scope="col">Email</th>
                    <th scope="col">Role</th>
                    <th scope="col">Points</th>
                    <th scope="col" style="width: 13rem">Is Admin?</th>
                  </tr>
                </ng-template>

                <ng-template pTemplate="body" let-user>
                  <tr>
                    <td>{{ user.name }}</td>
                    <td>{{ user.email }}</td>
                    <td>
                      <p-tag [value]="user.role" [severity]="user.role === 'Admin' ? 'contrast' : 'secondary'"></p-tag>
                    </td>
                    <td class="points-cell">{{ user.points }}</td>
                    <td>
                        <p-toggleswitch
                            [ngModel]="user.role === 'Admin'"
                            [disabled]="isUpdatingRole(user.id)"
                            (onChange)="changeRole(user.id, $event.checked ? 'Admin' : 'User')"
                        ></p-toggleswitch>
                    </td>
                  </tr>
                </ng-template>
              </p-table>
            }
          </section>
        }
      </main>
    </div>
  `,
  styles: [
    `
      :host { display: block; }

      .admin-shell {
        min-height: 100dvh;
        background:
          radial-gradient(circle at 15% 4%, color-mix(in srgb, var(--p-primary-color) 16%, transparent) 0, transparent 30%),
          linear-gradient(180deg, #f7f9fc 0%, #edf5ff 100%);
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

      .admin-content {
        width: min(1120px, calc(100% - 2rem));
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
      }

      .page-subtitle {
        margin: 0.75rem 0 0;
        color: #466078;
      }

      .status-panel {
        min-height: 40dvh;
        display: grid;
        place-content: center;
        gap: 0.9rem;
        text-align: center;
        color: #35516b;
      }

      .error-state,
      .empty-state {
        padding: 1.25rem;
        border-radius: 1rem;
        background: color-mix(in srgb, #ffffff 92%, transparent);
        border: 1px solid color-mix(in srgb, var(--p-primary-color) 10%, #dce7f5);
      }

      .stats-grid {
        display: grid;
        gap: 1rem;
        grid-template-columns: repeat(4, minmax(0, 1fr));
        margin-bottom: 1.25rem;
      }

      .stat-value {
        margin: 0;
        font-size: 1.7rem;
        font-weight: 800;
        color: #12324a;
      }

      .top-category {
        font-size: 1.1rem;
      }

      .users-section {
        padding: 1rem;
        border: 1px solid color-mix(in srgb, var(--p-primary-color) 10%, #dce7f5);
        border-radius: 1rem;
        background: color-mix(in srgb, #ffffff 92%, transparent);
      }

      .users-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        margin-bottom: 0.75rem;
      }

      .users-header h2 {
        margin: 0;
        color: #14324b;
      }

      .points-cell {
        font-weight: 700;
        color: var(--p-primary-color);
      }

      .w-full { width: 100%; }

      @media (max-width: 900px) {
        .stats-grid {
          grid-template-columns: repeat(2, minmax(0, 1fr));
        }
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

        .admin-content {
          width: min(100%, calc(100% - 1.5rem));
          padding-top: 1.5rem;
        }

        .stats-grid {
          grid-template-columns: 1fr;
        }
      }
    `
  ]
})
export class AdminComponent {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly apiUrl = 'https://localhost:7162/api';
  private readonly authVersion = signal(0);

  protected readonly analytics = signal<AnalyticsCard>({
    totalUsers: 0,
    totalKudos: 0,
    totalPoints: 0,
    topCategory: 'N/A'
  });
  protected readonly users = signal<AdminUser[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly updatingUserIds = signal<Set<string>>(new Set());

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
  protected readonly hasUsers = computed(() => this.users().length > 0);

  protected readonly roleOptions: Array<{ label: 'User' | 'Admin'; value: 'User' | 'Admin' }> = [
    { label: 'User', value: 'User' },
    { label: 'Admin', value: 'Admin' }
  ];

  ngOnInit(): void {
    if (!this.authService.isAdmin()) {
      void this.router.navigate(['/feed']);
      return;
    }

    this.loadDashboard();
  }

  protected logout(): void {
    this.authService.logout();
    this.authVersion.update((value) => value + 1);
    void this.router.navigate(['/login']);
  }

  protected isUpdatingRole(userId: string): boolean {
    return this.updatingUserIds().has(userId);
  }

  protected changeRole(userId: string, role: 'User' | 'Admin'): void {
    const currentUser = this.users().find((user) => user.id === userId);
    if (!currentUser || currentUser.role === role) {
      return;
    }

    this.setUpdatingRole(userId, true);

    this.http
      .put(`${this.apiUrl}/admin/users/${userId}/role`, { role })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.setUpdatingRole(userId, false))
      )
      .subscribe({
        next: () => {
          this.users.update((list) =>
            list.map((user) => (user.id === userId ? { ...user, role } : user))
          );
          this.messageService.add({
            severity: 'success',
            summary: 'Role updated',
            detail: `${currentUser.name} is now ${role}.`
          });
        },
        error: (error: unknown) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Role update failed',
            detail: this.resolveErrorMessage(error)
          });
        }
      });
  }

  private loadDashboard(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      analytics: this.http.get<unknown>(`${this.apiUrl}/admin/analytics`),
      users: this.http.get<unknown>(`${this.apiUrl}/admin/users`)
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: ({ analytics, users }) => {
          this.analytics.set(this.normalizeAnalytics(analytics));
          this.users.set(this.normalizeUsers(users));
        },
        error: () => {
          this.analytics.set({
            totalUsers: 0,
            totalKudos: 0,
            totalPoints: 0,
            topCategory: 'N/A'
          });
          this.users.set([]);
          this.errorMessage.set('Unable to load admin dashboard. Please refresh and try again.');
        }
      });
  }

  private normalizeAnalytics(input: unknown): AnalyticsCard {
    if (!this.isRecord(input)) {
      return {
        totalUsers: 0,
        totalKudos: 0,
        totalPoints: 0,
        topCategory: 'N/A'
      };
    }

    const totalUsers = this.readNumber(input, 'totalUsers') ?? this.readNumber(input, 'usersCount') ?? 0;
    const totalKudos = this.readNumber(input, 'totalKudos') ?? this.readNumber(input, 'kudosCount') ?? 0;
    const totalPoints = this.readNumber(input, 'totalPoints') ?? 0;
    const topCategory = this.resolveTopCategory(input['kudosByCategory']);

    return {
      totalUsers,
      totalKudos,
      totalPoints,
      topCategory
    };
  }

  private resolveTopCategory(raw: unknown): string {
    if (Array.isArray(raw)) {
      const entries = raw
        .map((item) => {
          if (!this.isRecord(item)) {
            return null;
          }

          const name = this.readString(item, 'categoryName') ?? this.readString(item, 'name');
          const count = this.readNumber(item, 'count') ?? this.readNumber(item, 'value') ?? 0;
          return name ? { name, count } : null;
        })
        .filter((entry): entry is { name: string; count: number } => entry !== null)
        .sort((a, b) => b.count - a.count);

      return entries[0]?.name ?? 'N/A';
    }

    if (this.isRecord(raw)) {
      const sorted = Object.entries(raw)
        .filter(([, value]) => typeof value === 'number')
        .sort((a, b) => (b[1] as number) - (a[1] as number));
      return sorted[0]?.[0] ?? 'N/A';
    }

    return 'N/A';
  }

  private normalizeUsers(input: unknown): AdminUser[] {
    if (!Array.isArray(input)) {
      return [];
    }

    return input
      .map((item, index) => {
        if (!this.isRecord(item)) {
          return null;
        }

        const id = this.readString(item, 'id') ?? this.readString(item, 'userId') ?? String(index);
        const name = this.readString(item, 'name') ?? 'Unknown user';
        const email = this.readString(item, 'email') ?? 'unknown@local';
        const role = this.readString(item, 'role') === 'Admin' ? 'Admin' : 'User';
        const points = this.readNumber(item, 'points') ?? 0;

        return {
          id,
          name,
          email,
          role,
          points
        } satisfies AdminUser;
      })
      .filter((user): user is AdminUser => user !== null);
  }

  private setUpdatingRole(userId: string, isUpdating: boolean): void {
    this.updatingUserIds.update((existing) => {
      const next = new Set(existing);
      if (isUpdating) {
        next.add(userId);
      } else {
        next.delete(userId);
      }
      return next;
    });
  }

  private resolveErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (typeof error.error === 'string' && error.error.length > 0) {
        return error.error;
      }

      if (this.isRecord(error.error)) {
        const message = this.readString(error.error, 'message') ?? this.readString(error.error, 'title');
        if (message) {
          return message;
        }
      }
    }

    return 'Please try again in a moment.';
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
}
