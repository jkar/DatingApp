import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';

// const httpOptions = {
//   headers : new HttpHeaders ({
//     Authorization:  'Bearer ' + JSON.parse(localStorage.getItem('user'))?.token
//     })
// }

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.base_url;
  httpOptions = {};

  constructor(private http: HttpClient) {
    this.httpOptions = {
      headers : new HttpHeaders ({
        Authorization:  'Bearer ' + JSON.parse(localStorage.getItem('user'))?.token
        })
    }
   }

  getMembers(): Observable<Member[]> {
    return this.http.get<Member[]>(this.baseUrl + 'Users', this.httpOptions)
  }

  getMember(username: string) {
    return this.http.get<Member>(this.baseUrl + '/Users' + username, this.httpOptions);
  }
}
