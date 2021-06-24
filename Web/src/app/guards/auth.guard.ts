import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AccountService } from '../services/account.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(
              private router: Router,
              private accountService: AccountService,
              private toastr: ToastrService
              ) {}

  canActivate(): Observable<boolean> | Promise<boolean> | boolean {
    return this.accountService.currentUser$
      .pipe(map(user => {
        if (user) {
          return true;
        } else {
          this.toastr.error("Access denied");
          this.router.navigate(['/']);
        }
      }));
  }
  
}
