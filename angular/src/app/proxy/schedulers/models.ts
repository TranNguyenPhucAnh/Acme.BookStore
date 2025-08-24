import type { EntityDto } from '@abp/ng.core';
import type { RecipientTypeEnum } from './recipient-type-enum.enum';
import type { WorkerOutputTypeEnum } from './worker-output-type-enum.enum';

export interface CreateUpdateSchedulerDto extends EntityDto<string> {
  recipientEntityId?: string;
  recipientEntity?: string;
  cronExpression?: string;
  timeZone?: string;
  recipientType?: RecipientTypeEnum;
  workerOutputType?: WorkerOutputTypeEnum;
}

export interface SchedulerDto extends EntityDto<string> {
  recipientEntityId?: string;
  recipientEntity?: string;
  cronExpression?: string;
  timeZone?: string;
  recipientType?: RecipientTypeEnum;
  workerOutputType?: WorkerOutputTypeEnum;
}
