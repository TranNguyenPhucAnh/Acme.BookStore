import type { AuditedEntityDto, EntityDto } from '@abp/ng.core';
import type { BookType } from './book-type.enum';

export interface AuthorLookupDto extends EntityDto<string> {
  name?: string;
}

export interface BookDto extends AuditedEntityDto<string> {
  authorId?: string;
  isbn?: string;
  authorName?: string;
  name?: string;
  type?: BookType;
  publishDate?: string;
  publisher?: string;
}

export interface CreateUpdateBookDto {
  authorId?: string;
  name: string;
  type: BookType;
  isbn?: string;
  publishDate: string;
  publisher?: string;
}
