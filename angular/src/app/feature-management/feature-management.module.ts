import { NgModule } from '@angular/core';
import { FeatureManagementRoutingModule } from './feature-management-routing.module';
import { FeatureManagementComponent } from './feature-management.component';
import { SharedModule } from '../shared/shared.module';


@NgModule({
  declarations: [
    FeatureManagementComponent
  ],
  imports: [
    SharedModule,
    FeatureManagementRoutingModule
  ]
})
export class FeatureManagementModule { }
