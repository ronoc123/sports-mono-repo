import { Component, EventEmitter, OnDestroy, Output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Subject, debounceTime, distinctUntilChanged } from "rxjs";

@Component({
  selector: "lib-ui-searchbar",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./searchbar.html",
  styleUrls: ["./searchbar.css"],
})
export class SearchbarComponent implements OnDestroy {
  // eslint-disable-next-line @angular-eslint/no-output-native
  @Output() search = new EventEmitter<string>();

  private input$ = new Subject<string>();

  private sub = this.input$
    .pipe(debounceTime(300), distinctUntilChanged())
    .subscribe((v) => {
      this.search.emit(v);
    });

  onInput(value: string) {
    this.input$.next(value ?? "");
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
    this.input$.complete();
  }
}
