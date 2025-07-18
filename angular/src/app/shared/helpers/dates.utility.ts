import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";

export class DateHelper {
    static dateToIsoDateString(date: Date): string {
        return date.toISOString().split('T')[0];
    }

    static isoDatetoStruct(value: string) : NgbDateStruct {
        const struct = value.split("-");
        return { year: +struct[0], month: +struct[1], day: +struct[2] } as NgbDateStruct;
    }

    static fromDateToStruct(value: Date) : NgbDateStruct {
        const date = this.dateToIsoDateString(value);
        return this.isoDatetoStruct(date);
    }

    static toFileName(header: string) : string {
        return header.split(';')[1].split('=')[1];
    }
}