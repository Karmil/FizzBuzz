/** Parameters of `GET /api/v1/fizzbuzz`. */
export interface FizzBuzzRequest {
  readonly int1: number;
  readonly int2: number;
  readonly limit: number;
  readonly str1: string;
  readonly str2: string;
}

/** Input constraints mirrored from the backend for immediate feedback; the backend remains the authority. */
export const FIZZBUZZ_CONSTRAINTS = {
  maxLimit: 10_000,
  maxTextLength: 50,
} as const;

/** Default values shown in the form. */
export const DEFAULT_FIZZBUZZ_REQUEST: FizzBuzzRequest = {
  int1: 3,
  int2: 5,
  limit: 15,
  str1: 'Fizz',
  str2: 'Buzz',
};

/** API failure translated for display. */
export interface ApiError {
  /** User-facing summary. */
  readonly message: string;
  /** Validation messages returned by the API, indexed by parameter name. */
  readonly fieldErrors: Readonly<Record<string, readonly string[]>>;
  /** Server trace identifier to quote when reporting the problem. */
  readonly traceId?: string;
}
