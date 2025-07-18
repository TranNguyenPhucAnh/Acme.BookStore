import type { CombineType } from '../enums/combine-type.enum';

export interface FilterBase {
  combineWith?: CombineType;
}

export interface Range<T> {
  min?: T;
  max?: T;
}
