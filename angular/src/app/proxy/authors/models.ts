import type { EntityDto } from '@abp/ng.core';
import type { FilterBase, Range } from '../auto-filterer/types/models';

export interface AuthorDto extends EntityDto<string> {
  name?: string;
  birthDate?: string;
}

export interface CreateAuthorDto {
  name: string;
  birthDate: string;
}

export interface GetAuthorListDto extends FilterBase {
  filter?: string;
  birthDate: Range<string>;
  skipCount: number;
  maxResultCount: number;
  sorting?: string;
}

export interface UpdateAuthorDto {
  name: string;
  birthDate: string;
}

export interface AuthorGetListInput extends FilterBase {
  filter?: string;
  birthDate: Range<string>;
  skipCount: number;
  maxResultCount: number;
  sorting?: string;
}