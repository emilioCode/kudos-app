import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'feed'
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component').then((m) => m.RegisterComponent)
  },
  {
    path: 'feed',
    loadComponent: () =>
      import('./features/feed/feed.component').then((m) => m.FeedComponent)
  },
  {
    path: 'kudos/give',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/kudos/give-kudos/give-kudos.component').then((m) => m.GiveKudosComponent)
  },
  {
    path: 'leaderboard',
    loadComponent: () =>
      import('./features/leaderboard/leaderboard.component').then((m) => m.LeaderboardComponent)
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/admin/admin.component').then((m) => m.AdminComponent)
  }
];
