import { RestService, Rest, PagedResultRequestDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import { ExtendedNotificationDto } from './model';

@Injectable({
    providedIn: 'root',
})
export class NotificationService {
    apiName = 'Default';

getAll = (input: PagedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExtendedNotificationDto>({
        method: 'GET',
        url: `/api/app/notification/notification-list`,
        params: { skipCount: input.skipCount, maxResultCount: input.maxResultCount }
    },
    { apiName: this.apiName,...config });

markAsRead = (ids: string[], config?: Partial<Rest.Config>) =>
    this.restService.request<any, any>({
        method: 'POST',
        url: `/api/app/notification/mark-as-read`,
        body: ids
    },
    { apiName: this.apiName,...config });

markAllAsRead = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, any>({
        method: 'POST',
        url: `/api/app/notification/mark-all-as-read`
    },
    { apiName: this.apiName,...config });

constructor(private restService: RestService) {}
}
