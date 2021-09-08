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

  createHubConnection(user: User, otherUserName: string) {
    this.hubConnection = new HubConnectionBuilder().withUrl(this.hubUrl + 'Message?user=' + otherUserName,
    {
      accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build();

    this.hubConnection.start().catch(error => console.log(error));

    this.hubConnection.on("ReceiveMessageThread", messages => {
      this.messageThreadSource.next(messages);
    });

    this.hubConnection.on("NewMessage", message => {
      this.messageThread$.pipe(take(1)).subscribe(messages => {
        this.messageThreadSource.next([...messages, message]);
      });
    });

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

  getMessages(pageNumber, pageSize, container) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);

    return getPaginatedResult<Message[]>(this.baseUrl + 'Messages', params, this.http);
  }

  getMessageThred(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'Messages/thread/' + username);
  }

  async sendMessage(username: string, content: string) {
    //return this.http.post<Message>(this.baseUrl + 'messages', {recipientUsername: username, content: content});
    return this.hubConnection.invoke("SendMessage", {recipientUsername: username, content: content})
      .catch(error => console.log(error));
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
