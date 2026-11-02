import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Rejects decimal numbers. Empty values are left to `Validators.required`. */
export function integerValidator(control: AbstractControl): ValidationErrors | null {
  const value: unknown = control.value;
  return value === null || value === '' || Number.isInteger(value) ? null : { integer: true };
}

/** Rejects text made only of whitespace. Empty values are left to `Validators.required`. */
export function notBlankValidator(control: AbstractControl): ValidationErrors | null {
  const value: unknown = control.value;
  return typeof value === 'string' && value.length > 0 && value.trim().length === 0
    ? { blank: true }
    : null;
}

/**
 * Limits the length in Unicode code points, like the backend: an emoji counts as one character
 * whereas `String.length` (and `Validators.maxLength`) would count two.
 */
export function maxCodePointsValidator(max: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value: unknown = control.value;
    if (typeof value !== 'string') {
      return null;
    }

    const actual = Array.from(value).length;
    return actual > max ? { maxCodePoints: { max, actual } } : null;
  };
}
