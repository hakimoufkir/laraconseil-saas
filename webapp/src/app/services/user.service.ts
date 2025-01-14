import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../interfaces/user';
import { PermissionCheckResult } from '../interfaces/permission-check-result';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private baseUrl = 'https://localhost:7006/api'; // Adjust the URL as needed

  constructor(private http: HttpClient) {}

  createUser(user: User): Observable<User> {
    return this.http.post<User>(`${this.baseUrl}/User/create`, user);
  }

  checkPermission(userId: number, permissionName: string): Observable<PermissionCheckResult> {
    return this.http.get<PermissionCheckResult>(
      `${this.baseUrl}/Role/check-permission?userId=${userId}&permissionName=${permissionName}`
    );
  }
}
