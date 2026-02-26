import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BaseResponse, GetPeopleResult, GetPersonByNameResult } from '../models';

@Injectable({ providedIn: 'root' })
export class PersonService {
  private readonly apiBaseUrl = (environment.apiBaseUrl ?? '').replace(/\/$/, '');

  constructor(private readonly http: HttpClient) {}

  getPeople(): Observable<GetPeopleResult> {
    return this.http.get<GetPeopleResult>(`${this.apiBaseUrl}/Person`);
  }

  getPersonByName(name: string): Observable<GetPersonByNameResult> {
    return this.http.get<GetPersonByNameResult>(`${this.apiBaseUrl}/Person/${encodeURIComponent(name)}`);
  }

  createPerson(name: string): Observable<BaseResponse> {
    return this.http.post<BaseResponse>(`${this.apiBaseUrl}/Person`, JSON.stringify(name), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}

