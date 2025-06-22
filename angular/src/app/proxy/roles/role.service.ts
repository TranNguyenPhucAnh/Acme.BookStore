import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { IdentityRoleDto } from '../volo/abp/identity/models';

@Injectable({
  providedIn: 'root',
})
export class RoleService {
  apiName = 'Default';
  

  getRoles = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, IdentityRoleDto[]>({
      method: 'GET',
      url: '/api/app/role/roles',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
