import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/models/member';
import { Pagination } from 'src/app/models/pagination';
import { User } from 'src/app/models/user';
import { UseParams } from 'src/app/models/userParams';
import { AccountService } from 'src/app/services/account.service';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {

  members: Member[];
  pagination: Pagination;
  userParams: UseParams;
  user: User;
  genderList = [{ value: 'male', display: 'Males' }, { value: 'female', display: 'Females' }];
  // members$: Observable<Member[]>;

  //travaei ta userParams(filtra) apo to service gia na borei na fernei ta members analoga me ta filtra (exei mesa kai ta stoixeia gia to pagination (PageSize, PageNumber))
  //sto service o constructor tou UserParams exei ola ta default stoixeia gia pagination k filtering ,thelei mono to user.gender gia na fernei ta anapoda fulla
  constructor(private memberService: MembersService) { 
    this.userParams = this.memberService.getUserParams();
  }

  ngOnInit(): void {
    this.loadMembers();
    // this.members$ = this.memberService.getMembers();
  }

  loadMembers() {
    this.memberService.setUserParams(this.userParams);
    this.memberService.getMembers(this.userParams)
      .subscribe( response => {
        this.members = response.result;
        this.pagination = response.pagination;
      });
  }

  resetFilters() {
    this.userParams = this.memberService.resetParams();
    this.loadMembers();
  }

  pageChanged(event: any) {
    this.userParams.pageNumber = event.page;
    this.memberService.setUserParams(this.userParams);
    this.loadMembers();
  }
}
