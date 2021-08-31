import { HttpClient, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';
import { PaginatedResult } from '../models/pagination';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.base_url;
  members: Member[] = [];
  paginatedResult: PaginatedResult<Member[]> = new PaginatedResult<Member[]>();
 
  //Pleon travaw to token k to pernaw se kathe request me to jwt interceptor
  //httpOptions = {};

  constructor(private http: HttpClient) {
    // this.httpOptions = {
    //   headers : new HttpHeaders ({
    //     Authorization:  'Bearer ' + JSON.parse(localStorage.getItem('user'))?.token
    //     })
    // }
   }

  getMembers(page?: number, itemsPerPage?: number) {
    // return this.http.get<Member[]>(this.baseUrl + 'Users', this.httpOptions)
    let params = new HttpParams();

    if (page !== null && itemsPerPage !== null) {
      params = params.append('pageNumber', page.toString());
      params = params.append('pageSize', itemsPerPage.toString());
    }

    // if (this.members.length > 0) {
    //   //To of γυρναει observable
    //   return of(this.members);
    // }
    return this.http.get<Member[]>(this.baseUrl + 'Users', {observe: 'response', params})
      .pipe(
        map(response => {
          this.paginatedResult.result = response.body;
          if (response.headers.get('pagination') !== null) {
            this.paginatedResult.pagination = JSON.parse(response.headers.get('pagination'));
          }
          return this.paginatedResult;
        })
      //   map(members => {
      //   this.members = members;
      //   return members;
      // })
      );
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

  setMainPhoto (photoId: number) {
    return this.http.put(this.baseUrl + 'Users/set-main-photo/' + photoId, {});
  }

  deletePhoto (photoId: number) {
    return this.http.delete(this.baseUrl + 'Users/delete-photo/' + photoId);
  }
}
