import { LocalizationService } from "@abp/ng.core";
import { Pipe } from "@angular/core";

@Pipe({ name: 'localize', standalone: false })
export class LocalizePipe {
constructor(
    private localization: LocalizationService) {}

    transform(key: string, args: any[]): string {
        return this.localization.instant(key, ...args);
    }
}
