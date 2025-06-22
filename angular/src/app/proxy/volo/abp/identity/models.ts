import type { ExtensibleEntityDto, ExtensibleFullAuditedEntityDto } from '@abp/ng.core';

export interface IdentityRoleDto extends ExtensibleEntityDto<string> {
  name?: string;
  isDefault: boolean;
  isStatic: boolean;
  isPublic: boolean;
  concurrencyStamp?: string;
  creationTime?: string;
}

export interface IdentityUserDto extends ExtensibleFullAuditedEntityDto<string> {
  tenantId?: string;
  userName?: string;
  name?: string;
  surname?: string;
  email?: string;
  emailConfirmed: boolean;
  phoneNumber?: string;
  phoneNumberConfirmed: boolean;
  isActive: boolean;
  lockoutEnabled: boolean;
  accessFailedCount: number;
  lockoutEnd?: string;
  concurrencyStamp?: string;
  entityVersion: number;
  lastPasswordChangeTime?: string;
}

export interface OrganizationUnitEto {
  id?: string;
  tenantId?: string;
  parentId?: string;
  code?: string;
  displayName?: string;
  entityVersion: number;
}
