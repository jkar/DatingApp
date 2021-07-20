import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.base_url;
  members: Member[] = [];
 
  //Pleon travaw to token k to pernaw se kathe request me to jwt interceptor
  //httpOptions = {};

  constructor(private http: HttpClient) {
    // this.httpOptions = {
    //   headers : new HttpHeaders ({
    //     Authorization:  'Bearer ' + JSON.parse(localStorage.getItem('user'))?.token
    //     })
    // }
   }

  getMembers(): Observable<Member[]> {
    // return this.http.get<Member[]>(this.baseUrl + 'Users', this.httpOptions)

    if (this.members.length > 0) {
      //To of γυρναει observable
      return of(this.members);
    }
    return this.http.get<Member[]>(this.baseUrl + 'Users')
      .pipe(map(members => {
        this.members = members;
        return members;
      }));
  }

  getMember(username: string) {
    //return this.http.get<Member>(this.baseUrl + '/Users' + username, this.httpOptions);

    const member = this.members.find(x => x.username === username);
    if (member !== undefined) return of(member);

    return this.http.get<Member>(this.baseUrl + 'Users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'Users', member)
      .pipe(map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      }));
  }
}
