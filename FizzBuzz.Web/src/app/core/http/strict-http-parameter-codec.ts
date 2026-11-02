import { HttpParameterCodec } from '@angular/common/http';

/**
 * Encodes query parameters with `encodeURIComponent`.
 *
 * Angular's default codec leaves `+`, `=`, `;`, `/`, `?`, `@`, `:`, `$` and `,` unencoded for readability,
 * but a raw `+` in a query string is decoded as a space by the server:
 * `str1=C++` would reach the API as `"C  "`. Strict encoding preserves every character.
 */
export class StrictHttpParameterCodec implements HttpParameterCodec {
  encodeKey(key: string): string {
    return encodeURIComponent(key);
  }

  encodeValue(value: string): string {
    return encodeURIComponent(value);
  }

  decodeKey(key: string): string {
    return decodeURIComponent(key);
  }

  decodeValue(value: string): string {
    return decodeURIComponent(value);
  }
}
