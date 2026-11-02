import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { KeyValuePipe } from '@angular/common';
import { Subscription } from 'rxjs';
import { toApiError } from '../api-error';
import { FizzBuzzApi } from '../fizzbuzz-api';
import { FizzBuzzForm } from '../fizzbuzz-form/fizzbuzz-form';
import { FizzBuzzResults } from '../fizzbuzz-results/fizzbuzz-results';
import { ApiError, FizzBuzzRequest } from '../fizzbuzz.models';

type ViewState =
  | { readonly status: 'idle' }
  | { readonly status: 'loading' }
  | { readonly status: 'success'; readonly values: readonly string[] }
  | { readonly status: 'error'; readonly error: ApiError };

/** Orchestrates the form, the API call and the display of its outcome. */
@Component({
  selector: 'app-fizzbuzz-page',
  imports: [FizzBuzzForm, FizzBuzzResults, KeyValuePipe],
  templateUrl: './fizzbuzz-page.html',
  styleUrl: './fizzbuzz-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FizzBuzzPage {
  private readonly api = inject(FizzBuzzApi);
  private pending?: Subscription;

  protected readonly state = signal<ViewState>({ status: 'idle' });
  protected readonly loading = computed(() => this.state().status === 'loading');
  protected readonly statusMessage = computed(() => {
    const state = this.state();
    switch (state.status) {
      case 'loading':
        return 'Génération en cours…';
      case 'success':
        return `${state.values.length} résultat(s) généré(s).`;
      case 'error':
        return 'La génération a échoué.';
      default:
        return '';
    }
  });

  constructor() {
    inject(DestroyRef).onDestroy(() => this.pending?.unsubscribe());
  }

  protected generate(request: FizzBuzzRequest): void {
    // A new request supersedes the previous one: its late response must not overwrite the display.
    this.pending?.unsubscribe();
    this.state.set({ status: 'loading' });

    this.pending = this.api.generate(request).subscribe({
      next: (values) => this.state.set({ status: 'success', values }),
      error: (error: unknown) => this.state.set({ status: 'error', error: toApiError(error) }),
    });
  }

  protected reset(): void {
    this.pending?.unsubscribe();
    this.state.set({ status: 'idle' });
  }
}
