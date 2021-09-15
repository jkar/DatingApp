import { Component, OnInit } from '@angular/core';
import { User } from './models/user';
import { AccountService } from './services/account.service';
import { PresenceService } from './services/presence.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'Dating App';

  constructor(private accountService: AccountService, private presence: PresenceService) {}


  ngOnInit(): void {
    this.setCurrentUser();
  }

  //me to pou xekinaei to app, vlepei an sto localstorage uparxei o user kai sthnei to ConnectionHub gia tin online interaction metaxu twn users
  //kai gia na enimerwsei to curentUser$ pou tha purodothsei tous subscribers gia na emfanisoun oti xreiazetai san loggedIn
  setCurrentUser() {
    const user: User = JSON.parse(localStorage.getItem("user"));
    if (user) {
      this.accountService.setCurrentUser(user);
      this.presence.createHubConnection(user);
    }

  }

}
