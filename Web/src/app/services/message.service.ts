import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Group } from '../models/group';
import { Message } from '../models/message';
import { User } from '../models/user';
import { getPaginatedResult, getPaginationHeaders } from './pagintationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.base_url;
  hubUrl = environment.hub_url;
  private hubConnection: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient) { }

  //to HubConnection orizei oti o user exei bei stin sunonilia (connection) me ton otherUserName
  //kai thetei tous parakatw listeners, "ReceiveMessageThread" - na ferei ola ta metaxu tous mnmnta, "NewMessage" - enimerwnei ton otherUserName oti irthe neo mnm k to 
  //observable me ta mnmta, "UpdatedGroup" - gia na enimerwsei ton recipients (otherUserName) oti exei bei neos xristis (kai poios einai)
    //kai etsi ananewnei to observable messageThread$ oti pleon ola ta mnmta einai read apo ton currentUser k enimerwnontai k oi subscribers sto front
  createHubConnection(user: User, otherUserName: string) {
    this.hubConnection = new HubConnectionBuilder().withUrl(this.hubUrl + 'Message?user=' + otherUserName,
    {
      accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build();

    this.hubConnection.start().catch(error => console.log(error));

    //me to pou bei o xristis stin sunomilia, kaleitai to messageHub->OnConnectedAsync (logw tou createHubConnection pou ekteleitai sto tab twn messages sto member-detail),
    //sto telos mia methodos kalei auton ton listener k pernaei ta messages
    //metaxu twn  xristwn 
    this.hubConnection.on("ReceiveMessageThread", messages => {
      this.messageThreadSource.next(messages);
    });

    //to back kalei ton listener gia na enimerwsei ton recipient (otherUserName) gia neo mnma kai na to perasei san data gia na to dei to front
    //edw to pernaei sto messageThread$ gia na enimerwthoun oi subscribers sto front
    this.hubConnection.on("NewMessage", message => {
      this.messageThread$.pipe(take(1)).subscribe(messages => {
        this.messageThreadSource.next([...messages, message]);
      });
    });

    //to back kalei ton listener gia na enimerwsei ton recipients (otherUserName) oti exei bei neos xristis (kai poios einai)
    //kai etsi ananewnei to observable messageThread$ oti pleon ola ta mnmta einai read apo ton currentUser k enimerwnontai k oi subscribers sto front
    this.hubConnection.on("UpdatedGroup", (group: Group) => {
      if (group.connections.some(x => x.username === otherUserName)) {
        this.messageThread$.pipe(take(1)).subscribe(messages => {
          messages.forEach(message =>{
            if (!message.dateRead) {
              message.dateRead = new Date(Date.now());
            }
          });
          this.messageThreadSource.next([...messages]);
        });
      }
    });
  }

  stopHubConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  //epistrfei ola ta mnmta pou sxetizetai o xrisits  olous tous upoloipous (paginated - kai sto header exei key pagination me data gia to pagination)
  getMessages(pageNumber, pageSize, container) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);

    return getPaginatedResult<Message[]>(this.baseUrl + 'Messages', params, this.http);
  }

  //unused anymore
  getMessageThred(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'Messages/thread/' + username);
  }

  //(ap oti katalava) kalei tin methodo SendMessage apo to messageHub gia na perasei to neo mnm
  //to return einai Promise, giauto k i methodos otan kaleitai exei k then meta
  async sendMessage(username: string, content: string) {
    //return this.http.post<Message>(this.baseUrl + 'messages', {recipientUsername: username, content: content});
    return this.hubConnection.invoke("SendMessage", {recipientUsername: username, content: content})
      .catch(error => console.log(error));
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
