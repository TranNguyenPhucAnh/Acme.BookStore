import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { IdentityUserDto } from '../volo/abp/identity/models';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  apiName = 'Default';
  

  getAll = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, IdentityUserDto[]>({
      method: 'GET',
      url: '/api/app/user',
    },
    { apiName: this.apiName,...config });
  

  getUsers = (ids: string[], config?: Partial<Rest.Config>) =>
    this.restService.request<any, IdentityUserDto[]>({
      method: 'GET',
      url: '/api/app/user/users',
      params: { ids },
    },
    { apiName: this.apiName,...config });
  

  getUsersByRoles = (roleId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IdentityUserDto[]>({
      method: 'GET',
      url: `/api/app/user/users-by-roles/${roleId}`,
    },
    { apiName: this.apiName,...config });
  

  getUsersInOrganizationUnits = (ids: string[], config?: Partial<Rest.Config>) =>
    this.restService.request<any, IdentityUserDto[]>({
      method: 'GET',
      url: '/api/app/user/users-in-organization-units',
      params: { ids },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
