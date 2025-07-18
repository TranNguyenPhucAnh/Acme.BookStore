import type { AuditedEntityDto, EntityDto } from '@abp/ng.core';
import type { BookType } from './book-type.enum';
import type { FilterBase, Range } from '../auto-filterer/types/models';

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

export interface BookGetListInput extends FilterBase {
  filter?: string;
  publishDate: Range<string>;
  type?: BookType;
  skipCount: number;
  maxResultCount: number;
  sorting?: string;
}

export interface CreateUpdateBookDto {
  authorId?: string;
  authorName: string;
  name?: string;
  type?: BookType;
  isbn?: string;
  publishDate?: string;
  publisher?: string;
}
