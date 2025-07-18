import { Component, OnDestroy, OnInit } from '@angular/core';
import { OrganizationUnitEto } from '@proxy/volo/abp/identity';
import { FeatureManagementService } from '@proxy/feature-managements';
import { FeatureDefinition } from '@proxy/volo/abp/features';
import { FeatureProviderDto } from '@abp/ng.feature-management/proxy';
import { ToasterService } from '@abp/ng.theme.shared';
import { OrganizationUnitService } from '@proxy/organization-units';
import { catchError, filter, Observer, of, Subject, takeUntil, tap } from 'rxjs';

@Component({
  selector: 'app-feature-management',
  standalone: false,
  templateUrl: './feature-management.component.html',
  styleUrl: './feature-management.component.scss'
})
export class FeatureManagementComponent implements OnInit, OnDestroy {
  organizationUnits: OrganizationUnitEto[] = [];
  featureDefinitions: FeatureDefinition[] = [];
  featureValues: FeatureProviderDto[] = [];
  private destroy$ = new Subject<void>(); // Để dọn dẹp listener

  constructor(
    private featureManagementService: FeatureManagementService,
    private organizationUnitService: OrganizationUnitService,
    private toastr: ToasterService
  ) {}

  ngOnInit(): void {
    this.organizationUnitService.getAll().pipe(
      catchError((error) => {
        //console.log(error);
        this.toastr.error('An error occurred, please retry');
        return of(null);
      }),
      //filter(value => !!value),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => this.organizationUnits = val,
      error: (err) => console.log(err),
      complete: () => console.log('fetch organizations completed')
    } as Observer<OrganizationUnitEto[]>);

    this.featureManagementService.getFeatureDefinitions().pipe(
      catchError((error) => {
        //console.log(error);
        this.toastr.error('An error occurred, please retry');
        return of(null);
      }),
      //filter(value => !!value),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => this.featureDefinitions = val,
      error: (err) => console.log(err),
      complete: () => console.log('fetch feature definitions completed')
    } as Observer<FeatureDefinition[]>);

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
    this.featureManagementService.getFeatureValues().pipe(
      catchError((error) => {
        //console.log(error);
        this.toastr.error('An error occurred, please retry');
        return of(null);
      }),
      //filter(value => !!value),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => this.featureValues = val,
      error: (err) => console.log(err),
      complete: () => console.log('fetch feature values completed')
    } as Observer<FeatureProviderDto[]>);
  }

  save(): void {
    this.featureManagementService.createFeatureValues(this.featureValues).pipe(
      catchError((error) => {
        //console.log(error);
        this.toastr.error('An error occurred, please retry');
        return of(null);
      }),
      // tap(() => {
      //   this.toastr.success('::SavedSuccessfullly');
      //   this.getFeatureValues();
      // }),
      //filter(value => !!value),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => {
        this.toastr.success('Saved Successfullly');
        this.getFeatureValues();
      },
      error: (err) => console.log(err),
      complete: () => console.log('save feature values completed')
    } as Observer<any>);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
