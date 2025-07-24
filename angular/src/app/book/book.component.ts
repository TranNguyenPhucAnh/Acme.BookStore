import { EnvironmentService, LIST_QUERY_DEBOUNCE_TIME, ListService, PagedResultDto, PermissionService } from '@abp/ng.core';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { BookService, BookDto, bookTypeOptions, AuthorLookupDto, BookGetListInput } from '@proxy/books';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbDateNativeAdapter, NgbDateAdapter, NgbDateStruct, NgbDatepickerNavigateEvent, NgbCollapse } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { EMPTY, NEVER, Observable, Observer, of, Subject } from 'rxjs';
import { catchError, filter, map, switchMap, takeUntil, tap } from 'rxjs/operators';
import { Constants } from '../shared/constants/constant';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { SignalRService } from '../signalR.service';
import { AbpWindowService } from '@abp/ng.core';
import { DateHelper } from '../shared/helpers/dates.utility';

@Component({
  selector: 'app-book',
  templateUrl: './book.component.html',
  styleUrls: ['./book.component.scss'],
  providers: [ListService,
    { provide: LIST_QUERY_DEBOUNCE_TIME, useValue: 500 },
    { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }
  ],
  standalone: false
})
export class BookComponent implements OnInit, OnDestroy {
  book = {
    items: [],
    totalCount: 0
  } as PagedResultDto<BookDto>;

  form: FormGroup;
  selectedBook = {} as BookDto;

  booksSearchParams = {
    publishDate: {
      min: undefined as string,
      max: undefined as string
    }
  } as BookGetListInput;

  dates = {
    from: undefined as Date,
    to: undefined as Date,
    min: undefined as NgbDateStruct,
    max: undefined as NgbDateStruct
  };

  authors$: Observable<AuthorLookupDto[]>;
  bookTypes = bookTypeOptions;
  isModalOpen = false;
  pageSizes = Constants.PageSizeOption;
  file: File;
  uploads: FileList;
  envUrl: string;
  private destroy$ = new Subject<void>(); // Để dọn dẹp listener

  //to do:
  //super constructor
  //protected & protected abstract method
  //string constants
  //separate books & authors component modal
  //finish UI
  //finish authors component
  //finish localization
  //finish permission
  //replace home component, project name, logo
  //deploy, ci/cd, domain name,

  constructor(
    public readonly list: ListService,
    private bookService: BookService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private permissionService: PermissionService,
    private toasterService: ToasterService,
    private signalRService: SignalRService,
    private abpWindowService: AbpWindowService,
    private envService: EnvironmentService
  ) {
    //update dropdown whenever a CRUD event occurred on the lookup page
    this.authors$ = this.bookService.getAuthorLookup().pipe(map((r) => r.items));
  }

  ngOnInit(): void {
    this.bookService.getMinDateTime().pipe(
      tap((res) => {
        //set date, ngb date struct and string
        this.setDateValues(res);
      }),
      switchMap(() =>
        this.list.hookToQuery(query => {
          this.updateQueryParams(query);
          return this.bookService.getList(this.booksSearchParams);
        })
      ),
      catchError((error) => {
        //console.log(error);
        this.toasterService.error('An error occurred, please retry');
        return of(null);
        //return NEVER; không emit observable, không complete()
        //return EMPTY; complete() ngay lập tức
      }),
      //filter(value => !!value),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (val) => this.book = val,
      error: (err) => console.log(err),
      complete: () => console.log('fetch books completed')
    } as Observer<PagedResultDto<BookDto>>);

    this.envUrl = this.envService.getEnvironment().apis.default.url;

    this.signalRService.addListener(
      this.envUrl.concat(Constants.EntityHubUrl),
      "BookEventThatNeedReloadList",
      message => {
      console.log('Received SignalR message:', message);
      this.getMinDate(); //làm mới min date
    });

    this.signalRService.addListener(
      this.envUrl.concat(Constants.EntityHubUrl),
      "AuthorEventThatNeedReloadList",
      message => {
      console.log('Received SignalR message:', message);
      this.authors$ = this.bookService.getAuthorLookup().pipe(map((r) => r.items));
    });
  }

  getMinDate() : void {
    this.bookService.getMinDateTime().pipe(
      catchError((error) => {
        this.toasterService.error("An error occurred, please retry");
        //console.log(error);
        return of(null);
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (val) => this.setDateValues(val),
      error: (err) => console.log(err),
      complete: () => console.log('fetch book min publish date completed')
    } as Observer<string>)
  }

  sendMessage(hubUrl: string, method: string, data: any) : void {
    this.signalRService.send(this.envUrl.concat(hubUrl), method, data);
  }

  isEditAndDelete() : boolean {
    return this.permissionService.getGrantedPolicy("BookStore.Books.Edit || BookStore.Books.Delete")
  }

  onPageSizeChange(input: number) : void {
    this.list.maxResultCount = input;
    this.list.get();
  }

  createBook() : void {
    this.selectedBook = {} as BookDto;
    this.buildForm();
    this.isModalOpen = true;
    this.subscribeToAuthorIdChanges();
  }
  
  editBook(id: string) : void {
    this.bookService.get(id).pipe(
      catchError((error) => {
        //console.log(error);
        this.toasterService.error('An error occurred, please retry');
        return of(null);
      }),
      //filter(value => !!value),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => {
        this.selectedBook = val;
        this.buildForm();
        this.isModalOpen = true;
        this.subscribeToAuthorIdChanges();
        },
      error: (err) => console.log(err),
      complete: () => console.log('fetch book for edit completed')
    } as Observer<BookDto>);
  }

  buildForm() : void {
    this.form = this.fb.group({
      authorId: [this.selectedBook.authorId || null, Validators.required],
      authorName: [this.selectedBook.authorName || null],
      name: [this.selectedBook.name || null, Validators.required],
      type: [this.selectedBook.type || null, Validators.required],
      publishDate: [
        this.selectedBook.publishDate ? new Date(this.selectedBook.publishDate + 'Z') : null,
        Validators.required,
      ],
      isbn: [this.selectedBook.isbn || null, Validators.required],
      publisher: [this.selectedBook.publisher || null, Validators.required]
    });
  }
  
  save() : void {
    if (this.form.invalid) {
      return;
    }
    
    const request = this.selectedBook.id
    ? this.bookService.update(this.selectedBook.id, this.form.value)
    : this.bookService.create(this.form.value);
      
      request.pipe(
        catchError((error) => {
          //console.log(error);
          this.toasterService.error('An error occurred, please retry');
          return of(null);
        }),
        // tap(() => {
        //   this.isModalOpen = false;
        //   this.form.reset();
        //   this.toasterService.success("Created/Updated Successfully")
        //   this.list.get();
        //   this.sendMessage();
        // }),
        //filter(value => !!value),
        takeUntil(this.destroy$),
      ).subscribe({
        next: (val) => {
          this.isModalOpen = false;
          this.form.reset();
          this.toasterService.success("Created/Updated Successfully")
          this.list.get();
          this.sendMessage(Constants.NotificationHubUrl, "SendNotificationListReload", "Book created/updated");
          this.sendMessage(Constants.EntityHubUrl, "SendBookListReload", "Book created/updated");
        },
        error: (err) => console.log(err),
        complete: () => console.log('create/update book completed')
      } as Observer<BookDto>);
  }
  
  delete(id: string) : void {
    this.confirmation.warn('::AreYouSureToDelete', 'AbpAccount::AreYouSure').pipe(
      filter((status) => status == Confirmation.Status.confirm),
      switchMap(() => {
        return this.bookService.delete(id);
      }),
      catchError((res : HttpErrorResponse) => {
        //console.log(error);
        this.toasterService.error(res.error.error.details); //toast the exception message thrown from BE
        return of(null);
      }),
      // tap(() => {
      //   this.list.get();
      //   this.sendMessage();
      // }),
      //filter(value => !!value),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => {
        if (val) {
          this.list.get();
          this.sendMessage(Constants.NotificationHubUrl, "SendNotificationListReload", "Book deleted");
          this.sendMessage(Constants.EntityHubUrl, "SendBookListReload", "Book deleted");
          this.toasterService.success("Deleted Successfully"); //should place success toaster here, not switchMap because error might not been catched
        }
      },
      error: (err) => console.log(err),
      complete: () => console.log('delete book completed')
    } as Observer<any>);
  }

  subscribeToAuthorIdChanges(): void {
    this.form.get('authorId')!.valueChanges.pipe(
      switchMap(authorId =>
        this.authors$.pipe(
          map(authors => {
            const selectedAuthor = authors.find(author => author.id === authorId);
            return selectedAuthor ? selectedAuthor.name : null;
          }),
          catchError((error) => {
            //console.log(error);
            this.toasterService.error('An error occurred, please retry');
            return of(null);
          })
        )
      ),
      //filter(value => !!value),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (val) => this.form.get('authorName')?.setValue(val),
      error: (err) => console.log(err),
      complete: () => console.log('change author completed')
    } as Observer<any>);
  }

  export() : void {
    if (this.book.items.length === 0 || this.book.totalCount === 0) {
      this.toasterService.warn('No data to export. Please add some.');
    }
    this.bookService.export({ ...this.booksSearchParams, skipCount: 0, maxResultCount: undefined })
      .pipe(
        catchError((error) => {
          //console.log(error);
          this.toasterService.error('An error occurred, please retry');
          return of(null);
        }),
        //tap(() => this.toasterService.success('::Export started. Please check your downloads')),
        takeUntil(this.destroy$),
      ).subscribe({
        next: (val) => {
          this.abpWindowService.downloadBlob(val.body, DateHelper.toFileName(val.headers.get('Content-Disposition')));
          this.toasterService.success('Export started. Please check your downloads');
        },
        error: (err) => console.log(err),
        complete: () => console.log('export book completed')
      } as Observer<HttpResponse<Blob>>
    )}

  import(files: FileList, inputRef: HTMLInputElement) : void {
    this.file = files[0];
    let formData = new FormData();
    formData.append('file', this.file, this.file.name)
    this.bookService.import(formData)
      .pipe(
        catchError((error) => {
          //console.log(error);
          this.toasterService.error('An error occurred, please retry');
          return of(null);
        }),
        //filter(value => !!value),
        takeUntil(this.destroy$)
    ).subscribe({
      next: (val) => {
        inputRef.value = '';
        if (!val.body.size) {
          this.list.get();
          this.toasterService.success('Imported Successfully.');
        }
        else {
          this.abpWindowService.downloadBlob(val.body, DateHelper.toFileName(val.headers.get('Content-Disposition')));
          this.toasterService.warn('Invalid data, please check your file');
        }
      },
      error: (err) => console.log(err),
      complete: () => console.log('import books completed')
    } as Observer<HttpResponse<Blob>>
  )
}

  upload(files: FileList, id: string) : void {
    let formData = new FormData();
    for (let index = 0; index < files.length; index++) {
      formData.append('files', files[index], files[index].name);
    }
    this.bookService.upload(formData, id)
    .pipe(
      catchError((error) => {
        //console.log(error);
        this.toasterService.error('An error occurred, please retry');
        return of(null);
      }),
      //filter(value => !!value),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => {
        val ? this.toasterService.success('Uploaded Successfully.') :
        this.toasterService.warn('Please upload pdf, epub or mobi instead.');
      },
      error: (err) => console.log(err),
      complete: () => console.log('upload books completed')
    } as Observer<boolean>)
  }

  preview(id: string) : void {
    this.bookService.preview(id)
    .pipe(
      catchError((error) => {
        //console.log(error);
        this.toasterService.error('An error occurred, please retry');
        return of(null);
      }),
      //filter(value => !!value),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => val ? window.open(val, '_blank') : this.toasterService.info('No preview available, please upload'),
      error: (err) => console.log(err),
      complete: () => console.log('preview book completed')
    } as Observer<string>);
    //const escapeURI = encodeURI(res); already encoded
  }

  download(id: string) : void {
    this.bookService.download(id)
    .pipe(
      catchError((error) => {
        //console.log(error);
        this.toasterService.error('An error occurred, please retry');
        return of(null);
      }),
      //filter(value => !!value),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => {
        if (!val.body.size) {
          this.toasterService.info('No book available, please upload')
        }
        else {
          //saveAs(res.body, this.toFileName(res.headers.get('Content-Disposition')));
          this.abpWindowService.downloadBlob(val.body, DateHelper.toFileName(val.headers.get('Content-Disposition')));
          this.toasterService.success('File is ready for download')
        }
      },
      error: (err) => console.log(err),
      complete: () => console.log('download book completed')
    } as Observer<HttpResponse<Blob>>);
  }

  updateQueryParams(input: BookGetListInput): void {
    this.booksSearchParams = {
      ...input,
      publishDate: {
        min: this.dates?.from ? DateHelper.dateToIsoDateString(this.dates.from) : undefined,
        max: this.dates?.to ? DateHelper.dateToIsoDateString(this.dates.to) : undefined
      },
      type: this.booksSearchParams.type ?? null
    };
  }

  //consider date-fns library to handle date type
  onNavigate(event: NgbDatepickerNavigateEvent, field: 'from' | 'to') : void {
    if (event.current) {
      this.dates[field] = new Date(Date.UTC(event.next.year, event.next.month - 1, this.dates[field].getUTCDate()));
      //new Date() with numbers: pass Date.UTC(year, monthIndex, date) to the constructor
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
    this.booksSearchParams.publishDate.min = DateHelper.dateToIsoDateString(this.dates.from);
    this.dates.min = DateHelper.isoDatetoStruct(this.booksSearchParams.publishDate.min);

    this.dates.to = new Date();
    this.booksSearchParams.publishDate.max = DateHelper.dateToIsoDateString(this.dates.to);
    this.dates.max = DateHelper.isoDatetoStruct(this.booksSearchParams.publishDate.max);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
