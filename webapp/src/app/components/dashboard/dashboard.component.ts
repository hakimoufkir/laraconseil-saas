import { Component } from '@angular/core';
import { User } from '../../interfaces/user';
import { PermissionCheckResult } from '../../interfaces/permission-check-result';
import { UserService } from '../../services/user.service';


@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent {
  newUser: User = {
    userName: '',
    email: '',
    roleIds: [],
  };

  checkUserId: number = 0;
  checkPermissionName: string = '';
  permissionCheckResult?: PermissionCheckResult;
  roles = [1, 2]; // Example roles

  constructor(private userService: UserService) {}

  createUser() {
    this.userService.createUser(this.newUser).subscribe(
      (user) => {
        alert(`User created successfully with ID: ${user.id}`);
        this.newUser = { userName: '', email: '', roleIds: [] }; // Reset form
      },
      (error) => {
        console.error('Error creating user:', error);
        alert('Failed to create user.');
      }
    );
  }

  checkPermission() {
    this.userService
      .checkPermission(this.checkUserId, this.checkPermissionName)
      .subscribe(
        (result) => {
          this.permissionCheckResult = result;
          alert(result.hasPermission ? 'Permission granted' : 'Permission denied');
        },
        (error) => {
          console.error('Error checking permission:', error);
          alert('Failed to check permission.');
        }
      );
  }
}
