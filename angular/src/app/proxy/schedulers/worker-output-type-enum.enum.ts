import { mapEnumToOptions } from '@abp/ng.core';

export enum WorkerOutputTypeEnum {
  BookReport = 0,
  AuthorReport = 1,
}

export const workerOutputTypeEnumOptions = mapEnumToOptions(WorkerOutputTypeEnum);
