import { Component, OnDestroy, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { Subject } from "rxjs";
import { distinctUntilChanged, map, takeUntil } from "rxjs/operators";
import { LoaderService } from "../../shared/totvs-loader/loader/loader.service";
import { LearningPathsPageStateService } from "./learning-paths-page-state.service";

@Component({
  selector: "app-learning-paths-page",
  templateUrl: "./learning-paths-page.component.html",
  styleUrls: ["./learning-paths-page.component.css"],
  providers: [LearningPathsPageStateService],
})
export class LearningPathsPageComponent implements OnInit, OnDestroy {
  state$ = this.stateService.state$;
  private destroySubject = new Subject<void>();

  constructor(
    private stateService: LearningPathsPageStateService,
    private loaderService: LoaderService,
    private activatedRoute: ActivatedRoute
  ) {}

  async ngOnInit(): Promise<void> {
    this.activateLoader();
    this.activatedRoute.queryParams
      .pipe(takeUntil(this.destroySubject))
      .subscribe(async (params) => {
        const filter: string = params.filter ?? "";
        await this.stateService.initialize(filter);
      });
  }

  private activateLoader(): void {
    this.state$
      .pipe(
        takeUntil(this.destroySubject),
        map((state) => state.isLoading),
        distinctUntilChanged()
      )
      .subscribe((value) => {
        if (value) {
          this.loaderService.show();
        } else {
          this.loaderService.hide();
        }
      });
  }

  onNextClicked(): Promise<void> {
    return this.stateService.goToNextPage();
  }

  ngOnDestroy(): void {
    this.destroySubject.next();
  }
}