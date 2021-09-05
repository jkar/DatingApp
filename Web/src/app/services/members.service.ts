import { HttpClient, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';
import { PaginatedResult } from '../models/pagination';
import { User } from '../models/user';
import { UseParams } from '../models/userParams';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationHeaders } from './pagintationHelper';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.base_url;
  members: Member[] = [];
  memberCache = new Map();
  user: User;
  userParams: UseParams;
 
  //Pleon travaw to token k to pernaw se kathe request me to jwt interceptor
  //httpOptions = {};

  constructor(private http: HttpClient,
    private accountService: AccountService) {
    // this.httpOptions = {
    //   headers : new HttpHeaders ({
    //     Authorization:  'Bearer ' + JSON.parse(localStorage.getItem('user'))?.token
    //     })
    // }
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UseParams(user);
    });
   }

   getUserParams() {
     return this.userParams;
   }

   setUserParams(params: UseParams) {
     this.userParams = params;
   }

   resetParams() {
     this.userParams = new UseParams(this.user);
     return this.userParams;
   }

  getMembers(userParams: UseParams) {
    // return this.http.get<Member[]>(this.baseUrl + 'Users', this.httpOptions)
   
    // if (this.members.length > 0) {
    //   //To of γυρναει observable
    //   return of(this.members);
    // }

    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // Στο memberCache θα περνάω κάθε φορά που τραβάω members με φιλτρα και τα σώζω σ 'ενα Map με key τα στοιχεία των filter (Object.values ...)
    //στο pipe.map, Aρχικά βλέπω αν έχω ήδη στο Map (memberCache) το key με τα filters apo to useParams, αν τα χω τα γυρνάω απο το Map (memberCache)
    //και δεν κάνω httpRequest.
    // console.log('up', Object.values(userParams).join('-'));
    // console.log('mcache', this.memberCache);
    var response = this.memberCache.get(Object.values(userParams).join('-'));
    if (response) {
      //To of γυρναει observable
      return of(response);
    }

    let params = getPaginationHeaders(userParams.pageNumber, userParams.pageSize);
    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    return getPaginatedResult<Member[]>(this.baseUrl + 'Users', params, this.http)
      .pipe(map(res => {
        this.memberCache.set(Object.values(userParams).join('-'), res);
        return res;
      }));
      //   map(members => {
      //   this.members = members;
      //   return members;
      // })
  }

  getMember(username: string) {
    //return this.http.get<Member>(this.baseUrl + '/Users' + username, this.httpOptions);

    // const member = this.members.find(x => x.username === username);
    // if (member !== undefined) return of(member);

    //!!!!!!!
    //kanw concat ta arrays tou memberCache me value to result gia na psaxnw me to find to member me basi to username, tha xei duplicates sto concatenated alla den mas
    //peirazei sto find giati vriskei to prwto panta
    const member = [...this.memberCache.values()]
      .reduce((arr, elem)=> {
        return arr.concat(elem.result);
      }, [])
      .find((m: Member) => m.username === username);
      if (member) {
        console.log('m', member);
        return of(member);
      }


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

  addLike (username: string) {
    return this.http.post(this.baseUrl + 'Likes/' + username, {});
  }

  getLikes (predicate: string, pageNumber, pageSize) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('predicate', predicate);
    // return this.http.get<Partial<Member[]>>(this.baseUrl + 'Likes?predicate=' + predicate);
    return getPaginatedResult<Partial<Member[]>>(this.baseUrl + 'Likes',  params, this.http);
  }
}
