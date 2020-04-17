import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/user';
import { AlertifyService } from '../../_services/alertify.service';
import { UserService } from '../../_services/user.service';
import { Router, ActivatedRouteSnapshot, ActivatedRoute } from '@angular/router';
import { Pagination, PaginatedResult } from 'src/app/_models/pagination';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  users: User[];
  pagination: Pagination; // L.144

  // L.147 for filtering
  user: User = JSON.parse(localStorage.getItem('user'));
  genderList = [{value: 'male', display: 'Males'} , {value: 'female', display: 'Females'} ];
  userParams: any = {} ;

  constructor(private userService: UserService,
              private alertify: AlertifyService,
              private route: ActivatedRoute) { }

  ngOnInit() {
    // this.loadUsers();
    this.route.data.subscribe(data => {
      // this.users = data['users'];  // data['users'] is the same "users" defined in the resolve of routes.ts 
      this.users = data['users'].result;  // L.143 to see the users and th pagination
      this.pagination = data['users'].pagination;

    // L.147 set filters
      this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
      this.userParams.minAge = 18;
      this.userParams.maxAge = 99;
      this.userParams.orderBy = 'lastActive';

    });

  }

  // L.147
  resetFilters() {
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.loadUsers();
  }

  // L.144
  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    console.log(this.pagination.currentPage);
    this.loadUsers();
  }

  // L.144
  loadUsers() {
    this.userService
      .getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, this.userParams) // L.147 userParams for filtering
      .subscribe((res: PaginatedResult<User[]>) => {
      this.users = res.result;
    }, error => {
      this.alertify.error(error);
    });
  }
  


  // loadUsers() {
  //   this.userService.getUsers().subscribe((users: User[]) => {
  //     this.users = users;
  //   }, error => {
  //     this.alertify.error(error);
  //   });
  // }
}
