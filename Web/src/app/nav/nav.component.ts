import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '../models/user';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: { username: string; password: string; } = { username: '', password: ''};

  //Θα τραβάω το currentUser$ με το async pipe γιατί γίνεται αυτόματα to subscibe k unsubscribe
  currentUser$ : Observable<User>;
  // loggedIn: boolean = false;

  constructor(private accountService: AccountService) { }

  ngOnInit(): void {
    // this.getCurrentUser();
    this.currentUser$ = this.accountService.currentUser$;
  }

  login () {
    this.accountService.login(this.model)
      .subscribe(response => {
        console.log(response);
        // this.loggedIn = true;
      },
      error => {
        console.log(error);
      }
      );
  }

  logout() {
    // this.loggedIn = false;
    this.accountService.logout();
  }


  //1st way, θα το κάνω comment γιατι δεν είναι ιδανικό απο πλευράς μνήμης (δεν γίνεται unsubscribe), θα τραβάω το subject κατευθείαν στο onInit

  // getCurrentUser() {
  //   this.accountService.currentUser$
  //     .subscribe(user => {
  //       //αν είναι null false ,αν δεν είναι null true
  //       this.loggedIn = !!user;
  //     },
  //     error => {
  //       console.log(error);
  //     }
  //     );
  // }

}
