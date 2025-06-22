import { mapEnumToOptions } from '@abp/ng.core';

export enum RecipientTypeEnum {
  Individual = 0,
  RoleBased = 1,
  OrganizationBased = 2,
}

export const recipientTypeEnumOptions = mapEnumToOptions(RecipientTypeEnum);
