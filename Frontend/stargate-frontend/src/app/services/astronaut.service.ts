import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseResponse, CreateAstronautDutyRequest, GetAstronautDutiesByNameResult } from '../models';

@Injectable({ providedIn: 'root' })
export class AstronautService {
  constructor(private readonly http: HttpClient) {}

  getAstronautDutiesByName(name: string): Observable<GetAstronautDutiesByNameResult> {
    return this.http.get<GetAstronautDutiesByNameResult>(`/AstronautDuty/${encodeURIComponent(name)}`);
  }

  createAstronautDuty(payload: CreateAstronautDutyRequest): Observable<BaseResponse> {
    return this.http.post<BaseResponse>('/AstronautDuty', payload);
  }
}

