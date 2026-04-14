import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    CardModule,
    ToastModule,
    MessageModule
  ],
  providers: [MessageService],
  template: `
    <p-toast></p-toast>

    <section class="auth-page" aria-labelledby="register-title">
      <p-card styleClass="auth-card">
        <ng-template pTemplate="title">
          <h1 id="register-title" class="auth-title">Create account</h1>
        </ng-template>

        <ng-template pTemplate="subtitle">
          <p class="auth-subtitle">Join KudosApp and start sharing recognition.</p>
        </ng-template>

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-form" novalidate>
          <label for="name">Full name</label>
          <input id="name" type="text" pInputText formControlName="name" class="w-full" autocomplete="name" />
          @if (showNameError()) {
            <p-message severity="error" size="small" variant="simple">Name is required.</p-message>
          }

          <label for="email">Email</label>
          <input id="email" type="email" pInputText formControlName="email" class="w-full" autocomplete="email" />
          @if (showEmailError()) {
            <p-message severity="error" size="small" variant="simple">Enter a valid email address.</p-message>
          }

          <label for="password">Password</label>
          <p-password
            inputId="password"
            formControlName="password"
            [feedback]="false"
            [toggleMask]="true"
            inputStyleClass="w-full"
            styleClass="w-full"
            autocomplete="new-password"
          ></p-password>
          @if (showPasswordError()) {
            <p-message severity="error" size="small" variant="simple">Password must be at least 6 characters.</p-message>
          }

          <button
            pButton
            type="submit"
            label="Create account"
            class="w-full"
            [loading]="isSubmitting()"
            [disabled]="isSubmitting()"
          ></button>

          <p class="auth-link-row">
            Already have an account?
            <a routerLink="/login">Sign in</a>
          </p>
        </form>
      </p-card>
    </section>
  `,
  styles: [
    `
      :host {
        display: block;
      }

      .auth-page {
        min-height: 100dvh;
        display: grid;
        place-items: center;
        padding: 1.5rem;
        background:
          radial-gradient(circle at 85% 12%, color-mix(in srgb, var(--p-primary-color) 20%, transparent) 0, transparent 45%),
          linear-gradient(160deg, #f3fff8 0%, #f4f8ff 50%, #fff7f2 100%);
      }

      .auth-form {
        display: grid;
        gap: 0.85rem;
      }

      .auth-title {
        margin: 0;
        font-size: 1.6rem;
      }

      .auth-subtitle {
        margin: 0;
      }

      .auth-link-row {
        margin: 0.25rem 0 0;
        text-align: center;
      }

      .auth-link-row a {
        font-weight: 600;
      }

      :host ::ng-deep .auth-card {
        width: min(100%, 30rem);
      }

      .w-full {
        width: 100%;
      }
    `
  ]
})
export class RegisterComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  protected readonly isSubmitting = signal(false);
  protected readonly submitted = signal(false);

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  protected showNameError(): boolean {
    const control = this.form.controls.name;
    return control.invalid && (control.touched || this.submitted());
  }

  protected showEmailError(): boolean {
    const control = this.form.controls.email;
    return control.invalid && (control.touched || this.submitted());
  }

  protected showPasswordError(): boolean {
    const control = this.form.controls.password;
    return control.invalid && (control.touched || this.submitted());
  }

  protected onSubmit(): void {
    this.submitted.set(true);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const { name, email, password } = this.form.getRawValue();

    this.authService
      .register(name, email, password)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.authService.saveSession(response);
          void this.router.navigate(['/feed']);
        },
        error: (error: unknown) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Registration failed',
            detail: this.resolveErrorMessage(error)
          });
        }
      });
  }

  private resolveErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (typeof error.error === 'string' && error.error.length > 0) {
        return error.error;
      }

      if (
        typeof error.error === 'object' &&
        error.error !== null &&
        'message' in error.error &&
        typeof error.error.message === 'string'
      ) {
        return error.error.message;
      }
    }

    return 'Unable to create your account. Please review your details and try again.';
  }
}
