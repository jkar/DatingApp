import { Component, OnInit } from '@angular/core';
import { Message } from '../models/message';
import { Pagination } from '../models/pagination';
import { ConfirmService } from '../services/confirm.service';
import { MessageService } from '../services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  pagination: Pagination;
  container = 'Unread';
  pageNumber = 1;
  pageSize = 5;
  loading = false;

  constructor(private messageService: MessageService, private confirmService: ConfirmService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  //fortwnei ta mhnumta analoga me to container ('unread', 'inbox', 'outbox') pageSize, pageNumber kai ta fernei paginated
  loadMessages() {
    this.loading = true;
    console.log('p', this.pageNumber, this.pageSize, this.container)
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container)
      .subscribe(response => {
        console.log('res', response);
        this.messages = response.result;
        this.pagination = response.pagination;
        this.loading = false;
      });
  }

  //an to diagrafei apo ta inbox (eiserxomena) tote sti vasi sto table Messages to RecipientDeleted ginetai 1
  //kai den to vepei o idios pleon sta inbox kai sti suzitisi tous
  
  //an to diagrafei apo ta outbox (exerhomena ) tote sti vasi sto table Messages to SenderDeleted ginetai 1
  //kai den to vepei o idios pleon sta outnbox kai sti suzitisi tous
  
  //An to diagrapsoun k oi duo (RecipientDeleted==1 &&  SenderDeleted==1) tote diagrafetai apo tin vasi
  deleteMessage(id: number) {
    this.confirmService.confirm('Confirm delete message', 'This cannot be undone')
      .subscribe(result => {
        if (result) {
          this.messageService.deleteMessage(id)
          .subscribe(() => {
            this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
          });
        }
      });
  }

  pageChanged(event: any) {
    console.log('page')
    this.pageNumber = event.page;
    this.loadMessages();
  }
}
