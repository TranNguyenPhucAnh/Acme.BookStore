import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OrganizationUnitService } from '@proxy/organization-units';
import { RoleService } from '@proxy/roles';
import { RecipientTypeEnum, recipientTypeEnumOptions, SchedulerDto, workerOutputTypeEnumOptions } from '@proxy/schedulers';
import { UserService } from '@proxy/users';
import { CronJobsConfig, CronJobsValidationConfig } from 'ngx-cron-jobs/src/app/lib/contracts/contracts';
import { catchError, finalize, Observer, of, startWith, Subject, switchMap, takeUntil } from 'rxjs';
import { NgbActiveModal, NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';
import { ToasterService } from '@abp/ng.theme.shared';

@Component({
  selector: 'app-scheduler-details',
  templateUrl: './scheduler-details.component.html',
  styleUrl: './scheduler-details.component.scss',
  standalone: false
})
export class SchedulerDetailsComponent implements OnInit, OnDestroy {
  private unsubscribe$ = new Subject<void>();

  @Input() selectedSchedule: SchedulerDto;

  isVisible: boolean;
  option: NgbModalOptions = { size: 'xl' };
  
  form: FormGroup;
  recipientEntities: { id: string, displayName: string }[] = [];

  workerOutputType = workerOutputTypeEnumOptions;
  recipientType = recipientTypeEnumOptions;

  cronConfig: CronJobsConfig = {
    multiple: false,
    quartz: false,
    bootstrap: true,
    option: {
      minute: false,
      hour: false
    }
  };
  
  cronValidate: CronJobsValidationConfig = {
    validate: true,
  };

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private organizationService: OrganizationUnitService,
    private roleService: RoleService,
    private activeModal: NgbActiveModal,
    private toaster: ToasterService
  ) {}
  
  ngOnInit(): void {
    this.isVisible = true;
    this.buildForm();

    this.form.get('recipientType').valueChanges
    .pipe(
      startWith(this.selectedSchedule),
      switchMap(value => {
        if (typeof value !== 'object' || value === null) {
          this.recipientEntities = [];
          this.form.get('recipientEntityId').reset();
          this.form.get('workerOutputType').reset();
          if (value === null) {
            return of([]);
          }
        }
        const type = typeof value === 'object' && 'recipientType' in value 
          ? value.recipientType 
          : value;
        switch(type) {
          case RecipientTypeEnum.Individual:
            return this.userService.getAll();
          case RecipientTypeEnum.OrganizationBased:
            return this.organizationService.getAll();
          case RecipientTypeEnum.RoleBased:
            return this.roleService.getAll();
          default:
            return of([]);
          }
        }),
        catchError((error) => {
          //console.log('Fetch failed: ', error);
          this.toaster.error('Failed to fetch');
          return of([]);
        }),
        finalize(() => (this.isVisible = false)), //clean up
        takeUntil(this.unsubscribe$), //unsubscribe/destroy component
      )
      .subscribe({
        next: (val) => {
          this.recipientEntities = val.map(e => ({
            id: e.id,
            displayName:'userName' in e ? e.userName :
                        'displayName' in e ? e.displayName :
                        'name' in e ? e.name : ''
                      }));
        },
        error: (err) => console.log(err),
        complete: () => console.log('fetch dropdown completed')
      } as Observer<any>
    )
  }
  
  buildForm() : void {
    this.form = this.fb.group({
      recipientEntityId: [this.selectedSchedule.recipientEntityId ?? null, Validators.required],
      recipientEntity: [this.selectedSchedule.recipientEntity ?? null],
      cronExpression: [this.selectedSchedule.cronExpression ?? null, Validators.required],
      recipientType: [this.selectedSchedule.recipientType ?? null, Validators.required],
      workerOutputType: [this.selectedSchedule.workerOutputType ?? null, Validators.required],
    });
  }

  save() : void {
    if (this.form.valid) {
      this.isVisible = false;
      const recipientEntity = this.recipientEntities.find(e => e.id === this.form.get('recipientEntityId')?.value).displayName;
      this.form.get('recipientEntity').setValue(recipientEntity);
      this.activeModal.close(this.form.value);
      this.toaster.success("Saved Successfully");
    }
  }

  onClose() : void {
    this.isVisible = false;
    this.activeModal.dismiss();
  }

  get cronExpressionControl() {
    return this.form.get('cronExpression');
  }

  get recipientTypeControl() {
    return this.form.get('recipientType');
  }

  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
}