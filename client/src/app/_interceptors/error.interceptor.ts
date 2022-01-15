import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private toastr: ToastrService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError(error => {
        if(error){
          switch(error.status){
           
            case 400:
              // Check if it's a validation error and show all the validation constraints that 
              // are not respected
              if(error.error.errors){
                const modalStateErrors = [];
                for(const key in error.error.errors){
                  if(error.error.errors[key]){
                    modalStateErrors.push(error.error.errors[key]);
                  }
                }
                throw modalStateErrors.flat();
              } else if(typeof(error.error) === 'object'){ // This is a normal bad-request error, show error via toastr notification
                this.toastr.error("Bad request", error.status);
              } else { // This is a 400 Bad request with a specific message
                this.toastr.error(error.error, error.status);
              }
              break;

            // Show a 'Not authorized' toastr notification
            case 401:
              this.toastr.error("Unauthorized", error.status);
              break;
            
            // Redirect the user to a '404 page not found' component
            case 404:
              this.router.navigateByUrl('/not-found');
              break;

            // Redirect the user to a '500 internal server error' component but also
            // save the message of the error in 'navigation extras'
            case 500:
              const navigationExtras: NavigationExtras  = {state: {error: error.error}};
              this.router.navigateByUrl('/server-error', navigationExtras);
              break;

            default:
              this.toastr.error('Something unexpected went wrong');
              console.log(error);
              break;
          }
        }
        return throwError(error);
      })
    )
  }
}
