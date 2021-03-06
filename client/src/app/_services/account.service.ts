import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {map} from 'rxjs/operators';
import { User } from '../_models/user';
import { ReplaySubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { PresenceService } from './presence.service';
import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';

@Injectable({
  providedIn: 'root'
})

export class AccountService {

  baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient, private presence: PresenceService) {}
    
    login(model : any){
      return this.http.post(this.baseUrl + 'account/login', model).pipe(
        map((response: User) => { // Ensure that login information is saved (persisting the login)
          const user = response;
          if(user){
            this.setCurrentUser(user);
            this.presence.createHubConnection(user);
          }
        })
      )
    }

    register(model: any){
      return this.http.post(this.baseUrl + 'account/register', model).pipe(
        map((user: User) =>{
          if(user){
            this.setCurrentUser(user);
            this.presence.createHubConnection(user);
          }
        })
      )
    }

    setCurrentUser(user: User){
      user.roles = [];
      const roles = this.getDecodedToken(user.token).role;
      
      // check to see if roles is an array or not (if the peron have multiple roles)
      Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);

      localStorage.setItem('user', JSON.stringify(user));
      this.currentUserSource.next(user);
    }

    logout(){
      localStorage.removeItem('user');
      this.currentUserSource.next(null);
      this.presence.stopHubConnection();
    }

    getDecodedToken(token) {
      // Decode the token and take the middle part (the payload)
      return JSON.parse(atob(token.split('.')[1]));
    }
  
}
