import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { UserService } from 'src/app/core/services/user.service';

@Component({
  selector: 'app-signin',
  templateUrl: './signin.component.html',
  styleUrls: ['./signin.component.scss'],
})
export class SigninComponent implements OnInit {
  signInSubs?: Subscription;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private authService: AuthService,
    private router: Router
  ) {}

  signInForm = this.fb.group({
    email: ['', Validators.required],
    password: ['', Validators.required],
  });

  ngOnInit(): void {}

  onSignIn() {
    const body = {
      email: this.signInForm.get('email')?.value,
      password: this.signInForm.get('password')?.value,
    };
    this.signInSubs = this.userService.signIn(body).subscribe({
      next: (resp: any) => {
        if (resp && resp.token != null) {
          localStorage.setItem('token', JSON.stringify(resp.token));
          this.authService.loggedStateChange(true);
          this.authService.isLoggedIn();
          this.router.navigate(['/home']);
        }
      },
    });
  }
}
