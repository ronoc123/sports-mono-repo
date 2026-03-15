import { Component, inject, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthFacade } from '@sports-ui/auth-data-access';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule],
  template: `<router-outlet></router-outlet>`,
  styles: [`:host { display: block; height: 100vh; }`],
})
export class AppComponent implements OnInit {
  private auth = inject(AuthFacade);

  ngOnInit() {
    this.auth.hydrate();
  }
}
