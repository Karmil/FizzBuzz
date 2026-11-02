import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FizzBuzzPage } from './fizzbuzz-page';

describe('FizzBuzzPage', () => {
  let fixture: ComponentFixture<FizzBuzzPage>;
  let element: HTMLElement;
  let http: HttpTestingController;

  const expectedUrl = '/api/v1/fizzbuzz?int1=3&int2=5&limit=15&str1=Fizz&str2=Buzz';
  const button = (label: string) =>
    Array.from(element.querySelectorAll('button')).find((b) => b.textContent?.includes(label))!;
  const status = () => element.querySelector('[role="status"]')?.textContent?.trim();

  async function clickGenerate(): Promise<void> {
    button('Générer').click();
    await fixture.whenStable();
  }

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(FizzBuzzPage);
    element = fixture.nativeElement;
    await fixture.whenStable();
  });

  afterEach(() => http.verify());

  it('invites the user to generate a sequence initially', () => {
    expect(element.textContent).toContain('cliquez sur « Générer »');
  });

  it('shows a loading state then the results returned by the API', async () => {
    await clickGenerate();

    expect(status()).toBe('Génération en cours…');
    expect(button('Génération').disabled).toBe(true);

    http.expectOne(expectedUrl).flush(['1', '2', 'Fizz', '4', 'Buzz']);
    await fixture.whenStable();

    expect(status()).toBe('5 résultat(s) généré(s).');
    expect(element.querySelector('app-fizzbuzz-results')).not.toBeNull();
    expect(button('Générer').disabled).toBe(false);
  });

  it('displays validation errors returned by the API', async () => {
    await clickGenerate();

    http.expectOne(expectedUrl).flush(
      { status: 400, errors: { limit: ['Must be between 1 and 10.'] }, traceId: 'trace-42' },
      { status: 400, statusText: 'Bad Request' },
    );
    await fixture.whenStable();

    const alert = element.querySelector('[role="alert"]');
    expect(alert?.textContent).toContain('Les paramètres ont été refusés par le serveur.');
    expect(alert?.textContent).toContain('Must be between 1 and 10.');
    expect(alert?.textContent).toContain('trace-42');
  });

  it('displays a clear message when the API is unreachable', async () => {
    await clickGenerate();

    http.expectOne(expectedUrl).error(new ProgressEvent('error'), { status: 0 });
    await fixture.whenStable();

    expect(element.querySelector('[role="alert"]')?.textContent).toContain('Le serveur est injoignable');
  });

  it('does not call the API when the form is invalid', async () => {
    const limit = element.querySelector<HTMLInputElement>('#limit')!;
    limit.value = '0';
    limit.dispatchEvent(new Event('input'));

    await clickGenerate();

    http.expectNone((request) => request.url.includes('fizzbuzz'));
  });

  it('cancels the pending request and clears the results on reset', async () => {
    await clickGenerate();
    const pending = http.expectOne(expectedUrl);

    button('Réinitialiser').click();
    await fixture.whenStable();

    expect(pending.cancelled).toBe(true);
    expect(element.textContent).toContain('cliquez sur « Générer »');
  });
});
