import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FizzBuzzRequest } from '../fizzbuzz.models';
import { FizzBuzzForm } from './fizzbuzz-form';

describe('FizzBuzzForm', () => {
  let fixture: ComponentFixture<FizzBuzzForm>;
  let element: HTMLElement;
  let emitted: FizzBuzzRequest[];

  const input = (name: string) => element.querySelector<HTMLInputElement>(`#${name}`)!;
  const button = (label: string) =>
    Array.from(element.querySelectorAll('button')).find((b) => b.textContent?.includes(label))!;

  async function type(name: string, value: string): Promise<void> {
    const field = input(name);
    field.value = value;
    field.dispatchEvent(new Event('input'));
    field.dispatchEvent(new Event('blur'));
    await fixture.whenStable();
  }

  async function submit(): Promise<void> {
    button('Générer').click();
    await fixture.whenStable();
  }

  beforeEach(async () => {
    fixture = TestBed.createComponent(FizzBuzzForm);
    element = fixture.nativeElement;
    emitted = [];
    fixture.componentInstance.generate.subscribe((request) => emitted.push(request));
    await fixture.whenStable();
  });

  it('shows the default values', () => {
    expect(input('int1').value).toBe('3');
    expect(input('int2').value).toBe('5');
    expect(input('limit').value).toBe('15');
    expect(input('str1').value).toBe('Fizz');
    expect(input('str2').value).toBe('Buzz');
  });

  it('labels every field', () => {
    for (const name of ['int1', 'int2', 'limit', 'str1', 'str2']) {
      expect(element.querySelector(`label[for="${name}"]`)).not.toBeNull();
    }
  });

  it('emits a typed request when the form is valid', async () => {
    await type('int1', '7');
    await type('str2', 'C++ 😀');

    await submit();

    expect(emitted).toEqual([{ int1: 7, int2: 5, limit: 15, str1: 'Fizz', str2: 'C++ 😀' }]);
  });

  it.each([
    ['int1', '0', 'supérieure ou égale à 1'],
    ['int2', '', 'obligatoire'],
    ['limit', '10001', 'inférieure ou égale à 10000'],
    ['limit', '2.5', 'nombre entier'],
    ['str1', '', 'obligatoire'],
    ['str2', '   ', 'uniquement des espaces'],
    ['str1', 'x'.repeat(51), '50 caractères maximum'],
  ])('rejects %s = "%s" with an accessible message', async (name, value, message) => {
    await type(name, value);

    await submit();

    expect(emitted).toEqual([]);
    const field = input(name);
    expect(field.getAttribute('aria-invalid')).toBe('true');
    const error = element.querySelector(`#${name}-error`);
    expect(error?.textContent).toContain(message);
    expect(field.getAttribute('aria-describedby')).toContain(`${name}-error`);
  });

  it('accepts 50 emojis although they use 100 UTF-16 code units', async () => {
    await type('str1', '😀'.repeat(50));

    await submit();

    expect(emitted).toHaveLength(1);
  });

  it('focuses the first invalid field on submit', async () => {
    await type('limit', '');

    await submit();

    expect(document.activeElement).toBe(input('limit'));
  });

  it('restores the defaults and notifies the parent on reset', async () => {
    let resets = 0;
    fixture.componentInstance.resetRequested.subscribe(() => resets++);
    await type('int1', '0');
    await submit();

    button('Réinitialiser').click();
    await fixture.whenStable();

    expect(input('int1').value).toBe('3');
    expect(element.querySelector('#int1-error')).toBeNull();
    expect(resets).toBe(1);
  });

  it('disables generation while loading', async () => {
    fixture.componentRef.setInput('loading', true);
    await fixture.whenStable();

    const generate = element.querySelector<HTMLButtonElement>('button[type="submit"]')!;
    expect(generate.disabled).toBe(true);
    expect(generate.getAttribute('aria-busy')).toBe('true');
    expect(generate.textContent).toContain('Génération');
  });
});
