import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hub_url;
  private hubConnection: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService) { }

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder().withUrl(this.hubUrl + 'presence', {
      accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build();

    this.hubConnection
      .start()
      .catch(error => console.log(error));

      this.hubConnection.on('UserIsOnLine', username => {
        this.toastr.info(username + ' has connected');
      });

      this.hubConnection.on('UserIsOffLine', username => {
        this.toastr.warning(username + ' has disconnected');
      });

      this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
        this.onlineUsersSource.next(usernames);
      });
  }

  stopHubConnection() {
    this.hubConnection.stop().catch(error => console.log(error));
  }
}
