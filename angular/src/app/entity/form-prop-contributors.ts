// src/app/form-prop-contributors.ts

import {
  eIdentityComponents,
  IdentityCreateFormPropContributors,
} from '@abp/ng.identity';
import { ePropType, FormProp, FormPropList  } from '@abp/ng.components/extensible';
import { Validators } from '@angular/forms';
import { map, tap } from 'rxjs';
import { OrganizationUnitService } from '@proxy/organization-units';
import { IdentityUserDto, OrganizationUnitEto } from '@proxy/volo/abp/identity';

const entityUnitProp = new FormProp<IdentityUserDto>({
    type: ePropType.String,
    name: 'EntityId', //elementId, khi save, giá trị field trên form chỉ map đúng khi 'name' trùng với field trong extraProperties
    displayName: 'Entity',
    validators: () => [Validators.required],
    options: getEntityOptions,
    isExtra: true //framework tự map & set defaulValue, nếu không thì phải setTimeout() chờ DOM render xong rồi set defaultValue trên dropdown
});

export function getEntityOptions(data) {
  const service = data.getInjected(OrganizationUnitService);
  const entityId = data.record?.extraProperties?.EntityId;

  return service.getAll().pipe(
    map((items: OrganizationUnitEto[]) =>
      items.map(item => ({
        key: item.displayName,
        value: item.id,
      }))
    ), //vì options() là hàm callback chạy bất đồng bộ với defaultValue => cần xử lý sau khi DOM đã render xong để kích hoạt sự kiện change trên dropdown
    tap(() => { 
      // Delay nhỏ để đợi DOM render
      setTimeout(() => {
        const dropdown = document.getElementById('EntityId') as HTMLSelectElement;
        
        if (dropdown && dropdown.options.length > 0) {
          // Tìm index của option có value === organizationUnitId
          const targetIndex = Array.from(dropdown.options).findIndex(
            opt => opt.value.split(':')[1].trim() === entityId
          );
          targetIndex >= 0 ? dropdown.selectedIndex = targetIndex : dropdown.selectedIndex = 0;
          // Tạo sự kiện change để trigger cập nhật
          dropdown.dispatchEvent(new Event('change'));
        }
      }, 200); // Có thể tăng lên nếu dropdown render chậm
    })
  );
}

export function entityPropContributor(propList: FormPropList<IdentityUserDto>) {
    propList.addByIndex(entityUnitProp, 6);

    const phoneNumberIndex = propList.indexOf('phoneNumber', (value, name) => value.name === name);
    propList.dropByIndex(phoneNumberIndex);

    const tailNodes = propList.dropManyTail(3);
    
    propList.addByIndex(tailNodes.pop().value, 5);
}

export const identityCreateFormPropContributors: IdentityCreateFormPropContributors = {
  // enum indicates the page to add contributors to
  [eIdentityComponents.Users]: [
    entityPropContributor,
    // You can add more contributors here
  ],
};

export const identityEditFormPropContributors = identityCreateFormPropContributors;
// you may define different contributors for edit form if you like