import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { DEFAULT_FIZZBUZZ_REQUEST, FIZZBUZZ_CONSTRAINTS, FizzBuzzRequest } from '../fizzbuzz.models';
import { integerValidator, maxCodePointsValidator, notBlankValidator } from './fizzbuzz-validators';

type FieldName = keyof FizzBuzzRequest;

interface FieldDefinition {
  readonly name: FieldName;
  readonly label: string;
  readonly hint: string;
  readonly type: 'number' | 'text';
}

const divisorValidators = [Validators.required, Validators.min(1), integerValidator];
const textValidators = [
  Validators.required,
  notBlankValidator,
  maxCodePointsValidator(FIZZBUZZ_CONSTRAINTS.maxTextLength),
];

/** Collects and validates the FizzBuzz parameters; emits a request only when they are valid. */
@Component({
  selector: 'app-fizzbuzz-form',
  imports: [ReactiveFormsModule],
  templateUrl: './fizzbuzz-form.html',
  styleUrl: './fizzbuzz-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FizzBuzzForm {
  readonly loading = input(false);
  readonly generate = output<FizzBuzzRequest>();
  readonly resetRequested = output();

  protected readonly fields: readonly FieldDefinition[] = [
    { name: 'int1', label: 'Premier diviseur (int1)', hint: 'Entier ≥ 1', type: 'number' },
    { name: 'int2', label: 'Second diviseur (int2)', hint: 'Entier ≥ 1', type: 'number' },
    {
      name: 'limit',
      label: 'Limite (limit)',
      hint: `Entier entre 1 et ${FIZZBUZZ_CONSTRAINTS.maxLimit.toLocaleString('fr-FR')}`,
      type: 'number',
    },
    {
      name: 'str1',
      label: 'Texte des multiples de int1 (str1)',
      hint: `1 à ${FIZZBUZZ_CONSTRAINTS.maxTextLength} caractères`,
      type: 'text',
    },
    {
      name: 'str2',
      label: 'Texte des multiples de int2 (str2)',
      hint: `1 à ${FIZZBUZZ_CONSTRAINTS.maxTextLength} caractères`,
      type: 'text',
    },
  ];

  /** `null` mirrors what a cleared number input produces. */
  protected readonly form = inject(NonNullableFormBuilder).group({
    int1: [DEFAULT_FIZZBUZZ_REQUEST.int1 as number | null, divisorValidators],
    int2: [DEFAULT_FIZZBUZZ_REQUEST.int2 as number | null, divisorValidators],
    limit: [
      DEFAULT_FIZZBUZZ_REQUEST.limit as number | null,
      [...divisorValidators, Validators.max(FIZZBUZZ_CONSTRAINTS.maxLimit)],
    ],
    str1: [DEFAULT_FIZZBUZZ_REQUEST.str1, textValidators],
    str2: [DEFAULT_FIZZBUZZ_REQUEST.str2, textValidators],
  });

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly submitted = signal(false);

  protected showError(name: FieldName): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.touched || this.submitted());
  }

  protected errorMessage(name: FieldName): string {
    return describe(this.form.controls[name].errors);
  }

  protected submit(): void {
    this.submitted.set(true);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.host.nativeElement.querySelector<HTMLElement>('[aria-invalid="true"]')?.focus();
      return;
    }

    const value = this.form.getRawValue();
    this.generate.emit({
      int1: value.int1 as number,
      int2: value.int2 as number,
      limit: value.limit as number,
      str1: value.str1,
      str2: value.str2,
    });
  }

  protected reset(): void {
    this.submitted.set(false);
    this.form.reset();
    this.resetRequested.emit();
  }
}

function describe(errors: ValidationErrors | null): string {
  if (!errors) {
    return '';
  }
  if (errors['required']) {
    return 'Ce champ est obligatoire.';
  }
  if (errors['blank']) {
    return 'Ce champ ne peut pas contenir uniquement des espaces.';
  }
  if (errors['integer']) {
    return 'La valeur doit être un nombre entier.';
  }
  if (errors['min']) {
    return `La valeur doit être supérieure ou égale à ${errors['min'].min}.`;
  }
  if (errors['max']) {
    return `La valeur doit être inférieure ou égale à ${errors['max'].max}.`;
  }
  if (errors['maxCodePoints']) {
    const { max, actual } = errors['maxCodePoints'];
    return `${max} caractères maximum (actuellement ${actual}).`;
  }
  return 'Valeur invalide.';
}
