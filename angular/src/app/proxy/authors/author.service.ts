import type { AuthorDto, CreateAuthorDto, GetAuthorListDto, UpdateAuthorDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AuthorService {
  apiName = 'Default';

//this service calls backend APIs via HttpClient returning a cold observable, executed only if subscribed
//each subscription creates a new request to backend, can make use of share/shareReplay operators

  create = (input: CreateAuthorDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AuthorDto>({
      method: 'POST',
      url: '/api/app/author',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/author/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AuthorDto>({
      method: 'GET',
      url: `/api/app/author/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetAuthorListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AuthorDto>>({
      method: 'GET',
      url: '/api/app/author',
      params: { filter: input.filter, ["BirthDate.Min"]: input.birthDate.min, ["BirthDate.Max"]: input.birthDate.max, skipCount: input.skipCount, maxResultCount: input.maxResultCount, sorting: input.sorting, combineWith: input.combineWith },
    },
    { apiName: this.apiName,...config });

    
  getMinDateTime = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, string>({
      method: 'GET',
      url: `/api/app/author/min-date-time`,
      responseType: 'json'
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateAuthorDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AuthorDto>({
      method: 'PUT',
      url: `/api/app/author/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
