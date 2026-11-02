import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FizzBuzzResults } from './fizzbuzz-results';

describe('FizzBuzzResults', () => {
  let fixture: ComponentFixture<FizzBuzzResults>;
  let element: HTMLElement;

  async function render(values: readonly string[]): Promise<void> {
    fixture.componentRef.setInput('values', values);
    await fixture.whenStable();
  }

  beforeEach(() => {
    fixture = TestBed.createComponent(FizzBuzzResults);
    element = fixture.nativeElement;
  });

  it('summarizes the number of results and of replaced values', async () => {
    await render(['1', '2', 'Fizz', '4', 'Buzz']);

    expect(element.querySelector('.summary')?.textContent).toMatch(/5\s*résultat\(s\), dont\s*2\s*remplacé/);
  });

  it('renders rows with their position and highlights replaced values', async () => {
    await render(['1', '2', 'Fizz']);

    const rows = Array.from(element.querySelectorAll('[role="listitem"]'));
    expect(rows.map((row) => row.querySelector('.value')?.textContent?.trim())).toEqual(['1', '2', 'Fizz']);
    expect(rows[2].querySelector('.position')?.textContent).toBe('3');
    expect(rows[2].querySelector('.value--replaced')).not.toBeNull();
    expect(rows[0].querySelector('.value--replaced')).toBeNull();
    expect(rows[2].getAttribute('aria-setsize')).toBe('3');
  });

  it('keeps the DOM small for large sequences thanks to virtual scrolling', async () => {
    const values = Array.from({ length: 10_000 }, (_, index) => String(index + 1));

    await render(values);

    const rendered = element.querySelectorAll('[role="listitem"]').length;
    expect(rendered).toBeLessThan(100);
    expect(element.querySelector('.summary')?.textContent).toContain('10000');
  });
});
