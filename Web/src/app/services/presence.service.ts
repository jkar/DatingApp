import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
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

  constructor(private toastr: ToastrService, private router: Router) { }

  //eminerwnei tous allous xristes an den einai syndedemenos o xristis ap alli suskeui, oti einai onLine kai enimerwnetai k o xristis me tous onlineusers
  //thetei kai tous listeners gia 'UserIsOnLine' (oti sundethike neos xristis kai poios), 'UserIsOffLine' (oti aposundethike xristis kai poios einai)
  //'GetOnlineUsers' (fernei olous tous onlieUsers molis ginei online), 'NewMessageReceived' (enimerwnetai gia neo mnm k apo poion xristi)
  createHubConnection(user: User) {
    //stelnei sto presenceHub stin methodo OnConnectedAsync to token k enimerwnei etsi oti exei sundethei
    //sto back enhmerwnei tous allous users me to 'UserIsOnLine' an einai i moni sundesi p exei k den einai m alli suskeui connected.
    //enimerwnei to Dictionary me tous onLineUsers kai fernei pisw ti lista me tous onlineUsers me to "GetOnlineUsers" (apo katw exw listeners giauta ta 2 events)
    this.hubConnection = new HubConnectionBuilder().withUrl(this.hubUrl + 'presence', {
      accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build();

    this.hubConnection
      .start()
      .catch(error => console.log(error));

      //otan stalthei apo to back, o listener edw pairnei t username tou xristi pou egine onLine
      // k kanei emit to neo array me tous onLineUsers (prostethike k autos) stous subscribers gia to onlineUsers$
      this.hubConnection.on('UserIsOnLine', username => {
        //den to krataw san functionality
        // this.toastr.info(username + ' has connected');

        this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
          this.onlineUsersSource.next([...usernames, username]);
        });
      });

      //otan stalthei apo to back, o listener edw pairnei t username tou xristi pou egine offLine
      // k kanei emit to neo array me tous onLineUsers (autos exei afairethei) stous subscribers gia to onlineUsers$
      this.hubConnection.on('UserIsOffLine', username => {
        // //den to krataw san functionality
        // this.toastr.warning(username + ' has disconnected');

        this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
          this.onlineUsersSource.next([...usernames.filter(x => x !== username)]);
        });
      });

      //gurnaei ti lista me tou energous xrhstes ston xristi pou egine onLine (presenceHub apo methodo OnConnectedAsync)
      this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
        this.onlineUsersSource.next(usernames);
      });

      //otan stelnei allos xristis mhnuma, sto back (messageHub->sendMessage) stelnei s auton ton listener tou recipient tin pliroforia oti irthe neo mnma kai
      //to username, knownAs tou sender
      //emfanizei ston recipient ston browser o toastr ena pop up
      //an patithei to pop up, ton kanei navigate sti sunomilia (onTap)  
      this.hubConnection.on('NewMessageReceived', ({username, knownAs}) => {
        this.toastr.info(knownAs + ' has sent you a new message!')
        .onTap
        .pipe(take(1))
        .subscribe(() => this.router.navigateByUrl('/members/' + username + '?tab=3'));

      });
  }

  //stamataei to hubConnection
  stopHubConnection() {
    this.hubConnection.stop().catch(error => console.log(error));
  }
}
