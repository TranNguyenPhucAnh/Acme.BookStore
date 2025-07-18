import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { OrganizationUnitEto } from '../volo/abp/identity/models';

@Injectable({
  providedIn: 'root',
})
export class OrganizationUnitService {
  apiName = 'Default';
  

  getAll = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, OrganizationUnitEto[]>({
      method: 'GET',
      url: '/api/app/organization-unit',
    },
    { apiName: this.apiName,...config });
  

  getUserOrganizationUnitIds = (userId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, string[]>({
      method: 'GET',
      url: `/api/app/organization-unit/user-organization-unit-ids/${userId}`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
