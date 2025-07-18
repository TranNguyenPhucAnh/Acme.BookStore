import { mapEnumToOptions } from '@abp/ng.core';

export enum BookType {
  UNDEFINED = 0,
  MYTHOLOGY = 1,
  FICTION = 2,
  HISTORICAL_FICTION = 3,
  NON_FICTION = 4,
  BIOGRAPHY = 5,
  MYSTERY = 6,
  FANTASY = 7,
  SCIENCE_FICTION = 8,
  HUMOR = 9,
  ROMANCE = 10,
  YOUNG_ADULT = 11,
  ETIQUETTE = 12,
  SPIRITUAL = 13,
  SHORT_STORIES = 14,
  ADVENTURE = 15,
  CHILDREN = 16,
}

export const bookTypeOptions = mapEnumToOptions(BookType);
