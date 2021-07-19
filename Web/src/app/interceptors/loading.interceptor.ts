import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { BusyService } from '../services/busy.service';
import { delay, finalize } from 'rxjs/operators';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private busyService: BusyService) {}

  //ΓΙΑ ΚΑΘΕ REQUEST ΘΑ ΚΑΝΩ ΕΝΑ DELAY 1S ΓΙΑ ΝΑ ΦΑΙΝΕΤΑΙ ΤΟ SPINNER κΑΙ ΟΤΑΝ ΤΕΛΕΙΩΝΕΙ ΤΟ REQUEST ΤΟ FINALIZE ΘΑ ΣΤΑΜΑΤΕΙ ΤΟ SPINNER
  
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    this.busyService.busy();
    return next.handle(request).pipe(
      delay(1000),
      finalize(() => {
        this.busyService.idle();
      })
    )
  }
}
