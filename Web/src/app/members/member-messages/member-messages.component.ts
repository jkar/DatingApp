import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/models/message';
import { MessageService } from 'src/app/services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm: NgForm;
  @Input() messages: Message[] = [];
  @Input() username: string;
  messageContent: string;

  constructor(public messageService: MessageService) { }

  ngOnInit(): void {
  }

  sendMessage() {
    // this.messageService.sendMessage(this.username, this.messageContent)
    //   .subscribe(message => {
    //     this.messages.push(message);
    //     this.messageForm.reset();
    //   });

    //to service->method kalei methodo sto messageHub->SendMessage k tin kanei invoke, auto gurnaei Promise
    //giauto meta exei then, pou kanei reset tin forma
    this.messageService.sendMessage(this.username, this.messageContent)
    .then(() => {
      this.messageForm.reset();
    });
  }

}
