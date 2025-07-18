import { mapEnumToOptions } from "@abp/ng.core";

export enum NotificationType {
    BookCRUD,
    AuthorCRUD,
}

export const notificationTypeOptions = mapEnumToOptions(NotificationType);
