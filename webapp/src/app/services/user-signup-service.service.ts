import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserSignup } from '../interfaces/user-signup';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserSignupServiceService {

  private apiUrlFixed = environment.apiEndpoint;

  constructor(private http: HttpClient) { }

  createUserWithoutRoles(user: UserSignup): Observable<any> {
    return this.http.post(`${this.apiUrlFixed}/User/signup`, user);
  }
}
