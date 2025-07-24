import { Component, OnDestroy, OnInit } from '@angular/core';
import { EnvironmentService, LIST_QUERY_DEBOUNCE_TIME, ListService, PagedResultDto, PermissionService } from '@abp/ng.core';
import { AuthorService, AuthorDto, AuthorGetListInput } from '@proxy/authors';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbDateNativeAdapter, NgbDateAdapter, NgbDateStruct, NgbDatepickerNavigateEvent } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Constants } from '../shared/constants/constant';
import { catchError, filter, Observer, of, Subject, switchMap, takeUntil, tap } from 'rxjs';
import { DateHelper } from '../shared/helpers/dates.utility';
import { SignalRService } from '../signalR.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-author',
  templateUrl: './author.component.html',
  styleUrls: ['./author.component.scss'],
  providers: [ListService,
    { provide: LIST_QUERY_DEBOUNCE_TIME, useValue: 500 },
    { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }],
  standalone: false
})

export class AuthorComponent implements OnInit, OnDestroy {
  author = {
    items: [],
    totalCount: 0
  } as PagedResultDto<AuthorDto>;

    booksSearchParams = {
      birthDate: {
        min: undefined as string,
        max: undefined as string
      }
    } as AuthorGetListInput;
  
    dates = {
      from: undefined as Date,
      to: undefined as Date,
      min: undefined as NgbDateStruct,
      max: undefined as NgbDateStruct
    };

  isModalOpen = false;
  form: FormGroup;
  selectedAuthor = {} as AuthorDto;
  pageSizes = Constants.PageSizeOption;
  envUrl: string;
  private destroy$ = new Subject<void>(); // Để dọn dẹp listener

  constructor(
    public readonly list: ListService,
    private authorService: AuthorService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private permissionService: PermissionService,
    private toasterService: ToasterService,
    private signalRService: SignalRService,
    private envService: EnvironmentService 
  ) {}

  ngOnInit(): void {
    this.authorService.getMinDateTime().pipe(
      tap((res) => {
        //set date, ngb date struct and string
        this.setDateValues(res);
      }),
      switchMap(() =>
        this.list.hookToQuery(query => {
          this.updateQueryParams(query);
          return this.authorService.getList(this.booksSearchParams);
        })
      ),
      catchError((error) => {
        //console.log(error);
        this.toasterService.error('An error occurred, please retry');
        return of(null);
      }),
      //filter(value => !!value),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (val) => {
        this.author = val;
        console.log(val);
      },
      error: (err) => console.log(err),
      complete: () => console.log('fetch authors completed')
    } as Observer<PagedResultDto<AuthorDto>>);

    this.envUrl = this.envService.getEnvironment().apis.default.url;
    this.signalRService.addListener(
      this.envUrl + Constants.EntityHubUrl,
      "AuthorEventThatNeedReloadList",
      message => {
      console.log('Received SignalR message:', message);
      this.getMinDate(); //làm mới min date
    });
  }

  getMinDate() : void {
    this.authorService.getMinDateTime().pipe(
      catchError((error) => {
        this.toasterService.error("An error occurred, please retry");
        //console.log(error);
        return of(null);
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (val) => this.setDateValues(val),
      error: (err) => console.log(err),
      complete: () => console.log('fetch author min birthdate completed')
    } as Observer<string>)
  }

  sendMessage(hubUrl: string, method: string, data: any) : void {
    this.signalRService.send(this.envUrl + hubUrl, method, data);
  }

  isEditAndDelete() : boolean {
    return this.permissionService.getGrantedPolicy("BookStore.Authors.Edit || BookStore.Authors.Delete");
  }

  onPageSizeChange(input: number) : void {
    this.list.maxResultCount = input;
    this.list.get();
  }

  createAuthor() : void {
    this.selectedAuthor = {} as AuthorDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  editAuthor(id: string) : void {
    this.authorService.get(id).pipe(
      catchError((error) => {
        //console.log(error);
        this.toasterService.error('An error occurred, please retry');
        return of(null);
      }),
      //filter(value => !!value),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => {
        this.selectedAuthor = val;
        this.buildForm();
        this.isModalOpen = true; 
      },
      error: (err) => console.log(err),
      complete: () =>  console.log('fetch author for edit completed')
    } as Observer<AuthorDto>);
  }
  
  buildForm() : void {
    this.form = this.fb.group({
      name: [this.selectedAuthor.name || '', Validators.required],
      birthDate: [
        this.selectedAuthor.birthDate ? new Date(this.selectedAuthor.birthDate + 'Z') : null,  Validators.required,
      ]
    });
  }

  save() : void {
    if (this.form.invalid) {
      return;
    }

    const request = this.selectedAuthor.id
    ? this.authorService.update(this.selectedAuthor.id, this.form.value)
    : this.authorService.create(this.form.value);

    request.pipe(
      catchError((error) => {
          //console.log(error);
          this.toasterService.error('An error occurred, please retry');
          return of(null);
        }),
        //filter(value => !!value),
        // tap(() => {
        //   this.isModalOpen = false;
        //   this.form.reset();
        //   this.list.get();
        //   this.selectedAuthor.id ? this.toasterService.success("Updated Successfully") : this.toasterService.success("Created Successfully");
        // }),
        takeUntil(this.destroy$),
      ).subscribe({
        next: (val) => {
          this.isModalOpen = false;
          this.form.reset();
          this.list.get();
          this.selectedAuthor.id ? this.toasterService.success("Updated Successfully") : this.toasterService.success("Created Successfully");
          this.sendMessage(Constants.EntityHubUrl, "SendAuthorListReload", "Author created/updated");
          this.sendMessage(Constants.NotificationHubUrl, "SendNotificationListReload", "Author created/updated");
        },
        error: (err) => console.log(err),
        complete: () => console.log('create/update author completed')
      } as Observer<AuthorDto>);
  }

  delete(id: string) : void {
    this.confirmation.warn('::AreYouSureToDelete', 'AbpAccount::AreYouSure')
    .pipe(
      filter((status) => status == Confirmation.Status.confirm),
      switchMap(() => {
        return this.authorService.delete(id);
      }),
      catchError((res: HttpErrorResponse) => {
        //console.log(error);
        this.toasterService.error(res.error.error.details); //toast the exception message thrown from BE
        return of(null);
      }),
      //tap(() => this.list.get()),
      //filter(value => !!value),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (val) => {
        if (val) {
          this.list.get();
          this.sendMessage(Constants.EntityHubUrl, "SendAuthorListReload", "Author deleted");
          this.sendMessage(Constants.NotificationHubUrl, "SendNotificationListReload", "Author deleted");
          this.toasterService.success("Deleted Successfully"); //should place success toaster here, not switchMap because error might not been catched
        }
      },
      error: (err) => console.log(err),
      complete: () => console.log('delete author completed')
    } as Observer<any>);
  }

    //consider date-fns library to handle date type
    onNavigate(event: NgbDatepickerNavigateEvent, field: 'from' | 'to') : void {
      if (event.current) {
        //new Date() with numbers: pass Date.UTC(year, monthIndex, date) to the constructor
        this.dates[field] = new Date(Date.UTC(event.next.year, event.next.month -1, this.dates[field].getUTCDate()))
        //bind (ngModel) from template => component (ngModelChange) trigger
        //bind [ngModel] from component => template (ngModelChange) not trigger
      }
    }

    //[(ngModel)] uses Date type and binds 2 ways, formControl use NgbDateStruct type and can't bind
    onClosed() : void {
      this.list.get();
    }

  setDateValues(res: string) : void {
    //new Date() with a string: make it ISO 8601 format, then append 'Z' to make it Zulu (UTC) time
    this.dates.from = new Date(res.concat('Z'));
    this.booksSearchParams.birthDate.min = DateHelper.dateToIsoDateString(this.dates.from);
    this.dates.min = DateHelper.isoDatetoStruct(this.booksSearchParams.birthDate.min);

    this.dates.to = new Date();
    this.booksSearchParams.birthDate.max = DateHelper.dateToIsoDateString(this.dates.to);
    this.dates.max = DateHelper.isoDatetoStruct(this.booksSearchParams.birthDate.max);
  }

  updateQueryParams(input: AuthorGetListInput): void {
    this.booksSearchParams = {
      ...input,
      birthDate: {
        min: this.dates?.from ? DateHelper.dateToIsoDateString(this.dates.from) : undefined,
        max: this.dates?.to ? DateHelper.dateToIsoDateString(this.dates.to) : undefined
      },
    };
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
