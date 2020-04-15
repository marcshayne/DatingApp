import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css'],
})
export class NavComponent implements OnInit {
  model: any = {};
  photoUrl: string;  // L.119

  constructor(
    public authService: AuthService,
    private alertify: AlertifyService,
    private router: Router
  ) {}

  ngOnInit() {
    this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl ); // L.119
  }

  login() {
    this.authService.login(this.model).subscribe(
      (next) => {
        // console.log('logged in successfully');
        this.alertify.success('logged in successfully');
      },
      (error) => {
        // console.log(error);
        this.alertify.error(error);
      },
      () => {
        this.router.navigate(['/members']);
      }
    );
    console.log(this.model);
  }

  loggedIn() {
    return this.authService.loggedIn();
    // const token = localStorage.getItem('token');
    // return !!token;   // shorthand for if statement
  }

  logout() {
    localStorage.removeItem('token');
    this.authService.decodedToken = null;
    localStorage.removeItem('user'); // L.117
    this.authService.currentUser = null; // L.117

    // console.log ('usr logged out');
    this.alertify.message('logged out');
    this.router.navigate(['/home']);
  }
}
