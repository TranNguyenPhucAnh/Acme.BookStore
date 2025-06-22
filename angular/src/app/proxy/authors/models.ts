import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface AuthorDto extends EntityDto<string> {
  name?: string;
  birthDate?: string;
}

export interface CreateAuthorDto {
  name: string;
  birthDate: string;
}

export interface GetAuthorListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
}

export interface UpdateAuthorDto {
  name: string;
  birthDate: string;
}
