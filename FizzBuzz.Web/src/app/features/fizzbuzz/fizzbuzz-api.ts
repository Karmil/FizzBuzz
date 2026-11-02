import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../core/config/api-base-url';
import { StrictHttpParameterCodec } from '../../core/http/strict-http-parameter-codec';
import { FizzBuzzRequest } from './fizzbuzz.models';

/** HTTP client of the FizzBuzz API. The sequence is always computed by the backend. */
@Injectable({ providedIn: 'root' })
export class FizzBuzzApi {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${inject(API_BASE_URL)}/fizzbuzz`;

  generate(request: FizzBuzzRequest): Observable<string[]> {
    const params = new HttpParams({
      encoder: new StrictHttpParameterCodec(),
      fromObject: {
        int1: request.int1,
        int2: request.int2,
        limit: request.limit,
        str1: request.str1,
        str2: request.str2,
      },
    });

    return this.http.get<string[]>(this.endpoint, { params });
  }
}
