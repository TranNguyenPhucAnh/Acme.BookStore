import { ListService, PagedResultDto, PermissionService } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { BookService, BookDto, bookTypeOptions, AuthorLookupDto } from '@proxy/books';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbDateNativeAdapter, NgbDateAdapter } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Constants } from '../shared/constants/constant';

@Component({
  selector: 'app-book',
  templateUrl: './book.component.html',
  styleUrls: ['./book.component.scss'],
  providers: [ListService, { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }],
  standalone: false
})
export class BookComponent implements OnInit {
  book = {
    items: [],
    totalCount: 0
  } as PagedResultDto<BookDto>;
  form: FormGroup;
  selectedBook = {} as BookDto;
  authors$: Observable<AuthorLookupDto[]>;
  bookTypes = bookTypeOptions;
  isModalOpen = false;
  pageSizes = Constants.PageSizeOption;

  constructor(
    public readonly list: ListService,
    private bookService: BookService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private permissionService: PermissionService,
    private toasterService: ToasterService
  ) {
    this.authors$ = bookService.getAuthorLookup().pipe(map((r) => r.items));
  }

  ngOnInit() {
    this.list.hookToQuery(query => this.bookService.getList(query)).subscribe(res => this.book = res);
  }

  isEditAndDelete() : boolean {
    return this.permissionService.getGrantedPolicy("BookStore.Books.Edit && BookStore.Books.Delete")
  }

  onPageSizeChange(input: number) {
    this.list.maxResultCount = input;
    this.list.get();
  }

  createBook() {
    this.selectedBook = {} as BookDto;
    this.buildForm();
    this.isModalOpen = true;
  }
  
  editBook(id: string) {
    this.bookService.get(id).subscribe((book) => {
      this.selectedBook = book;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  buildForm() {
    this.form = this.fb.group({
      authorId: [this.selectedBook.authorId || null, Validators.required],
      authorName: [this.selectedBook.authorName || null],
      name: [this.selectedBook.name || null, Validators.required],
      type: [this.selectedBook.type || null, Validators.required],
      publishDate: [
        this.selectedBook.publishDate ? new Date(this.selectedBook.publishDate) : null,
        Validators.required,
      ],
      isbn: [this.selectedBook.isbn || null, Validators.required],
      publisher: [this.selectedBook.publisher || null, Validators.required]
    });
  }
  
  save() {
    if (this.form.invalid) {
      return;
    }

    const request = this.selectedBook.id
      ? this.bookService.update(this.selectedBook.id, this.form.value)
      : this.bookService.create(this.form.value);
      
      request.subscribe(() => {
        this.isModalOpen = false;
        this.form.reset();
        this.toasterService.success("Created/Updated Successfully")
        this.list.get();
    });
  }

  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', 'AbpAccount::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.bookService.delete(id).subscribe(() => this.list.get());
        this.toasterService.success("Deleted Successfully")
      }
    });
  }
}
