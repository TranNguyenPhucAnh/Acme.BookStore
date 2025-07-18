import { AuditedEntityDto, PagedResultDto } from "@abp/ng.core";
import { NotificationType } from "./notification-type.enum";

export interface NotificationDto extends AuditedEntityDto<string> {
    fromUserId: string;
    toUserId: string;
    type: NotificationType;
    isRead: boolean;
    localizationKey: string;
    localizationArguments: string[];
    redirectUrl: string;
}

export interface ExtendedNotificationDto {
    totalUnread: number;
    result: PagedResultDto<NotificationDto>
}