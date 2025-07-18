import type { StringSegment } from '../../../extensions/primitives/models';

export interface EntityTagHeaderValue {
  tag: StringSegment;
  isWeak: boolean;
}
