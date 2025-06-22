import { Component, OnInit } from '@angular/core';
import { OrganizationUnitEto } from '@proxy/volo/abp/identity';
import { FeatureManagementService } from '@proxy/feature-managements';
import { FeatureDefinition } from '@proxy/volo/abp/features';
import { FeatureProviderDto } from '@abp/ng.feature-management/proxy';
import { ToasterService } from '@abp/ng.theme.shared';
import { OrganizationUnitService } from '@proxy/organization-units';

@Component({
  selector: 'app-feature-management',
  standalone: false,
  templateUrl: './feature-management.component.html',
  styleUrl: './feature-management.component.scss'
})
export class FeatureManagementComponent implements OnInit {
  organizationUnits: OrganizationUnitEto[] = [];
  featureDefinitions: FeatureDefinition[] = [];
  featureValues: FeatureProviderDto[] = [];

  constructor(
    private featureManagementService: FeatureManagementService,
    private organizationUnitService: OrganizationUnitService,
    private toastr: ToasterService) {
  }

  ngOnInit(): void {
    this.organizationUnitService.getOrganizationUnits().subscribe(res => {
      this.organizationUnits = res
    });

    this.featureManagementService.getFeatureDefinitions().subscribe(res => {
      this.featureDefinitions = res
    });

    this.getFeatureValues();
  }

  onClick(row: OrganizationUnitEto, $index: number, $event: Event): void {
    if ($event.target['checked']) {
      this.featureValues.push({
        name: this.featureDefinitions[$index].name,
        key: row.id
      } as FeatureProviderDto);
    }
    else {
      this.featureValues = this.featureValues.filter(i => i.name !== this.featureDefinitions[$index].name || i.key !== row.id);
    }
  }

  isChecked(name: string, key: string) : boolean {
    return this.featureValues.some(f => f.key === key && f.name === name);
  }

  getFeatureValues() : void {
    this.featureManagementService.getFeatureValues().subscribe(res => {
      this.featureValues = res;
      console.log(this.featureValues);
    });
  }

  save(): void {
    this.featureManagementService.createFeatureValues(this.featureValues).subscribe(() => {
      this.toastr.success('::SavedSuccessfullly');
      this.getFeatureValues();
    })
  }
}
