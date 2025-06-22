import { Component, OnInit } from '@angular/core';
import { ListService, PagedResultDto, PermissionService } from '@abp/ng.core';
import { AuthorService, AuthorDto } from '@proxy/authors';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbDateNativeAdapter, NgbDateAdapter } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Constants } from '../shared/constants/constant';

@Component({
  selector: 'app-author',
  templateUrl: './author.component.html',
  styleUrls: ['./author.component.scss'],
  providers: [ListService, { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }],
  standalone: false
})
export class AuthorComponent implements OnInit {
  author = { items: [], totalCount: 0 } as PagedResultDto<AuthorDto>;
  isModalOpen = false;
  form: FormGroup;
  selectedAuthor = {} as AuthorDto;
  pageSizes = Constants.PageSizeOption;

  constructor(
    public readonly list: ListService,
    private authorService: AuthorService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private permissionService: PermissionService,
    private toasterService: ToasterService
  ) {}

  ngOnInit(): void {
    this.list.hookToQuery(query => this.authorService.getList(query)).subscribe(res => this.author = res);
  }

  isEditAndDelete() : boolean {
    return this.permissionService.getGrantedPolicy("BookStore.Authors.Edit && BookStore.Authors.Delete")
  }

  onPageSizeChange(input: number) {
    this.list.maxResultCount = input;
    this.list.get();
  }

  createAuthor() {
    this.selectedAuthor = {} as AuthorDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  editAuthor(id: string) {
    this.authorService.get(id).subscribe((author) => {
      this.selectedAuthor = author;
      this.buildForm();
      this.isModalOpen = true;
    });
  }
  
  buildForm() {
    this.form = this.fb.group({
      name: [this.selectedAuthor.name || '', Validators.required],
      birthDate: [
        this.selectedAuthor.birthDate ? new Date(this.selectedAuthor.birthDate) : null,
        Validators.required,
      ]
    });
  }

  save() {
    if (this.form.invalid) {
      return;
    }
    
    if (this.selectedAuthor.id) {
      this.authorService
        .update(this.selectedAuthor.id, this.form.value)
        .subscribe(() => {
          this.isModalOpen = false;
          this.form.reset();
          this.list.get();
          this.toasterService.success("Updated Successfully");
        });
    } else {
      this.authorService.create(this.form.value).subscribe(() => {
        this.isModalOpen = false;
        this.form.reset();
        this.list.get();
        this.toasterService.success("Created Successfully");
      });
    }
  }

  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure')
        .subscribe((status) => {
          if (status === Confirmation.Status.confirm) {
            this.authorService.delete(id).subscribe(() => this.list.get());
            this.toasterService.success("Deleted Successfully");
          }
	  });
  }
}
