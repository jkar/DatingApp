import { HttpClient, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';
import { PaginatedResult } from '../models/pagination';
import { UseParams } from '../models/userParams';


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

  getMembers(userParams: UseParams) {
    // return this.http.get<Member[]>(this.baseUrl + 'Users', this.httpOptions)
   
    // if (this.members.length > 0) {
    //   //To of γυρναει observable
    //   return of(this.members);
    // }
    let params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize);
    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);

    return this.getPaginatedResult<Member[]>(this.baseUrl + 'Users', params);
      //   map(members => {
      //   this.members = members;
      //   return members;
      // })
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

  private getPaginatedResult<T>(url, params) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
    return this.http.get<T>(url, { observe: 'response', params })
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('pagination') !== null) {
            paginatedResult.pagination = JSON.parse(response.headers.get('pagination'));
          }
          return paginatedResult;
        })
      );
  }

  getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();

      params = params.append('pageNumber', pageNumber.toString());
      params = params.append('pageSize', pageSize.toString());

      return params;
  }
}
