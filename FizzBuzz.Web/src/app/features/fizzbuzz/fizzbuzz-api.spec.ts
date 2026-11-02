import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../core/config/api-base-url';
import { FizzBuzzApi } from './fizzbuzz-api';

describe('FizzBuzzApi', () => {
  let api: FizzBuzzApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '/api/v1' },
      ],
    });
    api = TestBed.inject(FizzBuzzApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('sends a GET with every parameter and returns the values', () => {
    let result: string[] | undefined;

    api.generate({ int1: 3, int2: 5, limit: 15, str1: 'Fizz', str2: 'Buzz' }).subscribe((values) => {
      result = values;
    });

    const request = http.expectOne(
      '/api/v1/fizzbuzz?int1=3&int2=5&limit=15&str1=Fizz&str2=Buzz',
    );
    expect(request.request.method).toBe('GET');
    request.flush(['1', '2', 'Fizz']);

    expect(result).toEqual(['1', '2', 'Fizz']);
  });

  it('percent-encodes reserved and Unicode characters', () => {
    api.generate({ int1: 2, int2: 3, limit: 6, str1: 'C++', str2: 'a&b 😀' }).subscribe();

    const request = http.expectOne((req) => req.url === '/api/v1/fizzbuzz');
    expect(request.request.urlWithParams).toBe(
      '/api/v1/fizzbuzz?int1=2&int2=3&limit=6&str1=C%2B%2B&str2=a%26b%20%F0%9F%98%80',
    );
    request.flush([]);
  });

  it('propagates HTTP errors', () => {
    let error: HttpErrorResponse | undefined;

    api.generate({ int1: 3, int2: 5, limit: 15, str1: 'Fizz', str2: 'Buzz' }).subscribe({
      error: (e: HttpErrorResponse) => (error = e),
    });

    http
      .expectOne((req) => req.url === '/api/v1/fizzbuzz')
      .flush({ title: 'Bad' }, { status: 400, statusText: 'Bad Request' });

    expect(error?.status).toBe(400);
  });
});
