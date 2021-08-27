import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Input() usersFromHomeComponent: any;
  @Output() cancelRegister = new EventEmitter<boolean>();
  model: { username: string; password: string; } = { username: '', password: ''};
  registerForm: FormGroup;
  maxDate: Date;

  constructor(
              private accountService: AccountService,
              private toastr: ToastrService,
              private fb: FormBuilder
              ) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() -18);
  }

  initializeForm () {
    // this.registerForm = new FormGroup({
    //   username: new FormControl('', [Validators.required]),
    //   password: new FormControl('', [Validators.required, Validators.minLength(2), Validators.maxLength(8)]),
    //   confirmPassword: new FormControl('', [Validators.required, this.matchValues('password')])
    // });
    this.registerForm = this.fb.group({
      username: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]],
      gender: ['male'],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required]
    });
    //ΑΝ ΔΕΝ ΤΟ ΠΕΡΑΣΩ ΟΤΑΝ ΑΛΛΑΞΩ ΤΟ PASSWORD ΔΕΝ ΞΑΝΑΚΑΝΕΙ VALIDATION ΚΑΙ ΔΙΝΕΙ VALID ΤΗ ΜΕΤΑΞΥ ΤΟΥΣ ΙΣΟΤΗΤΑ ΠΑΡΟΛΟ ΠΟΥ ΑΛΛΑΞΕ ΤΟ PSW,
    // M ΑΥΤΟ ΞΑΝΑΚΑΛΩ ΤΟ VALIDITY TOY CONFIRMPASSWRD
    this.registerForm.controls.password.valueChanges.subscribe(() => {
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    });
  }

  matchValues (matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control?.value === control?.parent?.controls[matchTo]?.value ? null : {isMatching: true};
    }
  }

  register() {
    console.log(this.registerForm.value);
    // this.accountService.register(this.model)
    //   .subscribe(response => {
    //     console.log(response);
    //     this.cancel();
    //   },
    //   error => {
    //     console.log(error);
    //     this.toastr.error(error.error);
    //   }
    //   );
  }

  cancel() {
    this.cancelRegister.emit(false);
  }

}
