import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {BehaviorSubject} from 'rxjs';   // L.119
import {map} from 'rxjs/operators';
import {JwtHelperService} from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root' // the root module is the appmodule
})
export class AuthService {
  baseUrl = environment.apiUrl + 'auth/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: User; // L.117
  photoUrl = new BehaviorSubject<string>('../../assets/user.png'); // default value // L.119
  currentPhotoUrl = this.photoUrl.asObservable(); // L.119

  constructor(private http: HttpClient) { }

  changeMemberPhoto(photoUrl: string) {  // L.119
    this.photoUrl.next(photoUrl);
  }


  login(model: any) {
    return this.http.post(this.baseUrl + 'login', model)
      .pipe(
        map((response: any) => {
          const user = response;
          if (user) {
            localStorage.setItem('token', user.token);
            // store the additional  user information // L.117
            localStorage.setItem('user', JSON.stringify(user.user)); // L.117
            this.decodedToken = this.jwtHelper.decodeToken(user.token);
            this.currentUser = user.user; // L.117
            this.changeMemberPhoto(this.currentUser.photoUrl);  // L.119
          }
        }
        )
      );
  }

  register(model: any) {
    return this.http.post(this.baseUrl + 'register', model);
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token);
  }


}
