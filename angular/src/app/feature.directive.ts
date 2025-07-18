import { Directive, EmbeddedViewRef, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { ConfigStateService } from '@abp/ng.core';
import { catchError, of, Subject, takeUntil } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

@Directive({
  selector: '[appFeature]',
  standalone: false
})
export class FeatureDirective {

  private _thenTemplateRef: TemplateRef<any> | null = null;
  private _thenViewRef: EmbeddedViewRef<any> | null = null;
  private _elseTemplateRef: TemplateRef<any> | null = null;
  private _elseViewRef: EmbeddedViewRef<any> | null = null;
  private condition!: string;
  private destroy$ = new Subject<void>();

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private configState: ConfigStateService
  ) {
    this._thenTemplateRef = templateRef;
  }

  @Input() set appFeature(condition: string) {
    this.condition = condition;
    this.renderView();
  }

  @Input('appFeatureThen') set appFeatureThen(templateRef: TemplateRef<any> | null) {
    this._thenTemplateRef = templateRef;
    this._thenViewRef = null;
    this.renderView();
  }

  @Input('appFeatureElse') set appFeatureElse(templateRef: TemplateRef<any> | null) {
    this._elseTemplateRef = templateRef;
    this._elseViewRef = null;
    this.renderView();
  }

  private renderView(): void {
    const features = String(this.condition).split(',');
    let isEnable = false;
    this.configState.refreshAppState().pipe(
      catchError((res: HttpErrorResponse) => {
        console.log(res);
        return of(null);
      }),
      takeUntil(this.destroy$))
      .subscribe(() => {
      const userFeatures = this.configState.getAll().extraProperties.EnabledFeatures as string[];
      isEnable = features.some(item => userFeatures.includes(item));
      if (isEnable) {
        if (this._thenViewRef) return;
        this.viewContainer.clear();
        this._elseViewRef = null;
        if (this._thenTemplateRef) {
          this._thenViewRef = this.viewContainer.createEmbeddedView(this._thenTemplateRef);
        }
      } else {
        if (this._elseViewRef) return;
        this.viewContainer.clear();
        this._thenViewRef = null;
        if (this._elseTemplateRef) {
          this._elseViewRef = this.viewContainer.createEmbeddedView(this._elseTemplateRef);
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
