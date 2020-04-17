import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()

export class MemberListResolver implements Resolve<User[]> {
pageNumber = 1; // L.143
pageSize = 5; // L.143

    constructor(private userService: UserService,
                private router: Router,
                private alertify: AlertifyService) {}

    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
            return this.userService.getUsers(this.pageNumber, this.pageSize).pipe(  // L.143
                catchError(error => {
                    this.alertify.error('Problem retriving data');
                    this.router.navigate(['/home']);
                    return of(null);
                })
            );
    }
}
