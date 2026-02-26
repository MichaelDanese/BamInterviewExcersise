import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BaseResponse, CreateAstronautDutyRequest, GetAstronautDutiesByNameResult } from '../models';

@Injectable({ providedIn: 'root' })
export class AstronautService {
  private readonly apiBaseUrl = (environment.apiBaseUrl ?? '').replace(/\/$/, '');

  constructor(private readonly http: HttpClient) {}

  getAstronautDutiesByName(name: string): Observable<GetAstronautDutiesByNameResult> {
    return this.http.get<GetAstronautDutiesByNameResult>(`${this.apiBaseUrl}/AstronautDuty/${encodeURIComponent(name)}`);
  }

  createAstronautDuty(payload: CreateAstronautDutyRequest): Observable<BaseResponse> {
    return this.http.post<BaseResponse>(`${this.apiBaseUrl}/AstronautDuty`, payload);
  }
}

