import { eIdentityComponents, IdentityEntityPropContributors } from '@abp/ng.identity';
import { EntityPropList } from '@abp/ng.components/extensible';
import { IdentityUserDto } from '@abp/ng.identity/proxy';

export function entityPropContributor(propList: EntityPropList<IdentityUserDto>) {
    const index = propList.indexOf('phoneNumber', (value, name) => value.name === name);
    propList.dropByIndex(index);
}

export const identityEntityPropContributors: IdentityEntityPropContributors = {
  // enum indicates the page to add contributors to
  [eIdentityComponents.Users]: [
    entityPropContributor,
    // You can add more contributors here
  ],
};