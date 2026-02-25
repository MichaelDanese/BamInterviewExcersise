
export interface BaseResponse {
  success: boolean;
  message: string;
  responseCode: number;
}

export interface PersonAstronaut {
  personId: number;
  name: string;
  currentRank: string | null;
  currentDutyTitle: string | null;
  careerStartDate: string | null;
  careerEndDate: string | null;
}

export interface AstronautDutyDTO {
  rank: string;
  dutyTitle: string;
  dutyStartDate: string;
  dutyEndDate: string | null;
}

export interface GetPeopleResult extends BaseResponse {
  people: PersonAstronaut[];
}

export interface GetPersonByNameResult extends BaseResponse {
  person: PersonAstronaut | null;
}

export interface GetAstronautDutiesByNameResult extends BaseResponse {
  person: PersonAstronaut | null;
  astronautDuties: AstronautDutyDTO[];
}

export interface CreateAstronautDutyRequest {
  name: string;
  rank: string;
  dutyTitle: string;
  dutyStartDate: string;
}


