import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseResponse, GetPeopleResult, GetPersonByNameResult } from '../models';

@Injectable({ providedIn: 'root' })
export class PersonService {
  constructor(private readonly http: HttpClient) {}

  getPeople(): Observable<GetPeopleResult> {
    return this.http.get<GetPeopleResult>('/Person');
  }

  getPersonByName(name: string): Observable<GetPersonByNameResult> {
    return this.http.get<GetPersonByNameResult>(`/Person/${encodeURIComponent(name)}`);
  }

  createPerson(name: string): Observable<BaseResponse> {
    return this.http.post<BaseResponse>('/Person', JSON.stringify(name), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}

