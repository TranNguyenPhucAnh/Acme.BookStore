import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { FeatureProviderDto } from '../volo/abp/feature-management/models';
import type { FeatureDefinition } from '../volo/abp/features/models';

@Injectable({
  providedIn: 'root',
})
export class FeatureManagementService {
  apiName = 'Default';
  

  createFeatureValues = (input: FeatureProviderDto[], config?: Partial<Rest.Config>) =>
    this.restService.request<any, any>({
      method: 'POST',
      url: '/api/app/feature-management/feature-values',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  getFeatureDefinitions = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeatureDefinition[]>({
      method: 'GET',
      url: '/api/app/feature-management/feature-definitions',
    },
    { apiName: this.apiName,...config });
  

  getFeatureValues = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeatureProviderDto[]>({
      method: 'GET',
      url: '/api/app/feature-management/feature-values',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
