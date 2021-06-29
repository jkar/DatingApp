import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  base_url = environment.base_url;
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }

  login(model: { username: string; password: string; }) {
    return this.http.post(this.base_url + 'Account/login', model)
      .pipe(map((response: User) => {
        const user = response;
        if (user) {
          localStorage.setItem("user", JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      }))
  }

  register(model: { username: string; password: string; }) {
    return this.http.post(this.base_url + 'Account/register', model)
      .pipe(map(
        (user: User) => {
          if (user) {
            localStorage.setItem("user", JSON.stringify(user));
            this.currentUserSource.next(user);
          }
          //δεν είναι madatory Να γυρναω κάτι, επιστρεφω μόνο για να το κανω console.log στο subscribe
          return user;
        }));
  }

  setCurrentUser(user: User) {
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem("user");
    this.currentUserSource.next(null);
  }
}
