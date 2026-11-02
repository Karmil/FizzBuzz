import { FormControl } from '@angular/forms';
import { integerValidator, maxCodePointsValidator, notBlankValidator } from './fizzbuzz-validators';

describe('fizzbuzz validators', () => {
  describe('integerValidator', () => {
    it.each([3, 0, -2, null])('accepts %s', (value) => {
      expect(integerValidator(new FormControl(value))).toBeNull();
    });

    it('rejects decimals', () => {
      expect(integerValidator(new FormControl(1.5))).toEqual({ integer: true });
    });
  });

  describe('notBlankValidator', () => {
    it.each(['Fizz', ' a ', ''])('accepts "%s"', (value) => {
      expect(notBlankValidator(new FormControl(value))).toBeNull();
    });

    it('rejects whitespace only', () => {
      expect(notBlankValidator(new FormControl('  \t'))).toEqual({ blank: true });
    });
  });

  describe('maxCodePointsValidator', () => {
    const validator = maxCodePointsValidator(3);

    it('counts emojis as one character', () => {
      expect(validator(new FormControl('😀😀😀'))).toBeNull();
    });

    it('rejects longer values with details', () => {
      expect(validator(new FormControl('abcd'))).toEqual({ maxCodePoints: { max: 3, actual: 4 } });
    });
  });
});
