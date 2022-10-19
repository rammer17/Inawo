import { Component } from '@angular/core';
import { Subscription } from 'rxjs';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  title = 'ClientApp';
  isLogged: boolean = false;
  subjectSubs?: Subscription;

  constructor(private AuthService: AuthService) {}

  ngOnInit() {
    this.subjectSubs = this.AuthService.isLogged$.subscribe({
      next: (data: any) => {
        this.isLogged = data;
      }
    })
  }

  ngOnDestroy() {
    this.subjectSubs?.unsubscribe();
  }

}
