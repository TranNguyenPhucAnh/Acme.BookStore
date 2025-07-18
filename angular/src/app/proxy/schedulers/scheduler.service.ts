import type { CreateUpdateSchedulerDto, SchedulerDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SchedulerService {
  apiName = 'Default';
  

  create = (input: CreateUpdateSchedulerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SchedulerDto>({
      method: 'POST',
      url: '/api/app/scheduler',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, any>({
      method: 'DELETE',
      url: `/api/app/scheduler/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SchedulerDto>({
      method: 'GET',
      url: `/api/app/scheduler/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SchedulerDto>>({
      method: 'GET',
      url: '/api/app/scheduler',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateSchedulerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SchedulerDto>({
      method: 'PUT',
      url: `/api/app/scheduler/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
