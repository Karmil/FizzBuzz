import { InjectionToken } from '@angular/core';
import { environment } from '../../../environments/environment';

/** Base URL of the backend API, taken from the build environment and overridable in tests. */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => environment.apiBaseUrl,
});
