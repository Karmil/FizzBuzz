import { HttpErrorResponse } from '@angular/common/http';
import { toApiError } from './api-error';

describe('toApiError', () => {
  const httpError = (status: number, error: unknown = null) =>
    new HttpErrorResponse({ status, error });

  it('reports an unreachable server for status 0', () => {
    expect(toApiError(httpError(0)).message).toContain('injoignable');
  });

  it('exposes validation errors and trace id for a 400', () => {
    const result = toApiError(
      httpError(400, {
        title: 'One or more validation errors occurred.',
        errors: { limit: ['Must be between 1 and 100.'] },
        traceId: 'trace-123',
      }),
    );

    expect(result.message).toContain('refusés');
    expect(result.fieldErrors).toEqual({ limit: ['Must be between 1 and 100.'] });
    expect(result.traceId).toBe('trace-123');
  });

  it('tolerates a 400 without a problem details body', () => {
    expect(toApiError(httpError(400, 'plain text')).fieldErrors).toEqual({});
  });

  it('asks to slow down on 429', () => {
    expect(toApiError(httpError(429)).message).toContain('Trop de requêtes');
  });

  it('hides server errors behind a generic message', () => {
    const result = toApiError(httpError(500, { title: 'Boom', traceId: 'abc' }));

    expect(result.message).toBe('Le serveur a rencontré une erreur. Réessayez plus tard.');
    expect(result.traceId).toBe('abc');
  });

  it('handles other statuses and non-HTTP errors', () => {
    expect(toApiError(httpError(404)).message).toContain('404');
    expect(toApiError(new Error('x')).message).toBe('Une erreur inattendue est survenue.');
  });
});
