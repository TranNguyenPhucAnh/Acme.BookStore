import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { IdentityUserDto } from '../volo/abp/identity/models';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  apiName = 'Default';
  

  getUsers = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, IdentityUserDto[]>({
      method: 'GET',
      url: '/api/app/user/users',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
