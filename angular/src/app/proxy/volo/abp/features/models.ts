import type { ILocalizableString } from '../localization/models';
import type { IStringValueType } from '../validation/string-values/models';

export interface FeatureDefinition {
  name?: string;
  displayName: ILocalizableString;
  description: ILocalizableString;
  parent: FeatureDefinition;
  children: FeatureDefinition[];
  defaultValue?: string;
  isVisibleToClients: boolean;
  isAvailableToHost: boolean;
  allowedProviders: string[];
  item: object;
  properties: Record<string, object>;
  valueType: IStringValueType;
}
