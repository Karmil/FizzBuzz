import { HttpErrorResponse } from '@angular/common/http';
import { ApiError } from './fizzbuzz.models';

/** Subset of RFC 9457 problem details returned by the API. */
interface ProblemDetails {
  readonly title?: string;
  readonly errors?: Record<string, string[]>;
  readonly traceId?: string;
}

/** Translates any HTTP failure into a message the user can act upon. */
export function toApiError(error: unknown): ApiError {
  if (!(error instanceof HttpErrorResponse)) {
    return { message: 'Une erreur inattendue est survenue.', fieldErrors: {} };
  }

  const problem = asProblemDetails(error.error);
  const traceId = problem?.traceId;

  switch (true) {
    case error.status === 0:
      return {
        message: 'Le serveur est injoignable. Vérifiez votre connexion puis réessayez.',
        fieldErrors: {},
      };
    case error.status === 400:
      return {
        message: 'Les paramètres ont été refusés par le serveur.',
        fieldErrors: problem?.errors ?? {},
        traceId,
      };
    case error.status === 429:
      return {
        message: 'Trop de requêtes ont été envoyées. Patientez quelques secondes puis réessayez.',
        fieldErrors: {},
        traceId,
      };
    case error.status >= 500:
      return {
        message: 'Le serveur a rencontré une erreur. Réessayez plus tard.',
        fieldErrors: {},
        traceId,
      };
    default:
      return {
        message: `La requête a échoué (code ${error.status}).`,
        fieldErrors: {},
        traceId,
      };
  }
}

function asProblemDetails(body: unknown): ProblemDetails | undefined {
  return typeof body === 'object' && body !== null ? (body as ProblemDetails) : undefined;
}
