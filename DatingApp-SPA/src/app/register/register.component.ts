import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker/bs-datepicker.config';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // @Input() valuesFromHome: any;
  @Output() cancelRegister = new EventEmitter();
  // model: any = {};  // relpaced by user
  user: User; // L.132

  registerForm: FormGroup;
  bsConfig: Partial<BsDatepickerConfig>; // L.130

  constructor(private authService: AuthService,
              private alertify: AlertifyService,
              private fb: FormBuilder,
              private router: Router) { }

  ngOnInit() {
    // without formbuilder
    //this.registerForm = new FormGroup({
    //     username: new FormControl('', Validators.required),
    //     password: new FormControl('', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]),
    //     confirmPassword: new FormControl('', Validators.required)
    // }, this.passwordMatchValidator);

    // L.130
    this.bsConfig = {
        containerClass: 'theme-red'
    };

    // with form builder L.128
    this.createRegisterForm();

  }

  createRegisterForm() {
    this.registerForm = this.fb.group({
        gender: ['male'],
        username: ['', Validators.required],
        knownAs: ['', Validators.required],
        dateOfBirth: [null, Validators.required],
        city: ['', Validators.required],
        country: ['', Validators.required],
        password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
        confirmPassword: ['', Validators.required]
    }, {validator: this.passwordMatchValidator });

  }

  register() {
      // *** removed to be replaced by reactive form
      // this.authService.register(this.model).subscribe(() => {
      //   this.alertify.success('successfully registered');
      // }, error => {
      //   this.alertify.error(error);
      // });

      // L.132
      if (this.registerForm.valid) {
        this.user = Object.assign({}, this.registerForm.value); // assgin the values to an empty object then to user
        this.authService.register(this.user).subscribe(() => {
          this.alertify.success('successfully registered');
        }, error => {
          this.alertify.error(error);
        }, () => {
          this.authService.login(this.user).subscribe(() =>{
            this.router.navigate(['/members']);
          }

          );
        });
      }

      // console.log(this.registerForm.value);
  }

  // L.126
 passwordMatchValidator(g: FormGroup) {
   return g.get('password').value === g.get('confirmPassword').value ? null : {'mismatch' : true};
 }

  cancel() {
    this.cancelRegister.emit(false);
    console.log('cancelled');
  }

}
