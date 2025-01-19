import { Component } from '@angular/core';
import { UserSignup } from '../../interfaces/user-signup';
import { UserSignupServiceService } from '../../services/user-signup-service.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user-signup',
  templateUrl: './user-signup.component.html',
  styleUrl: './user-signup.component.css'
})
export class UserSignupComponent {
  tenantName: string = '';
  tenantEmail: string = '';
  isSubmitting: boolean = false;
  successMessage: string = '';
  errorMessage: string = '';

  constructor(private userSignupService: UserSignupServiceService, private router:Router) { }

  onSubmit() {
    this.isSubmitting = true;
    const user: UserSignup = {
      tenantName: this.tenantName,
      tenantEmail: this.tenantEmail,
    };

    this.userSignupService.createUserWithoutRoles(user).subscribe(
      (response:any) => {
        this.successMessage = 'User created successfully!';
        this.errorMessage = '';
        this.router.navigate(['/dashboard']);
      },
      (error:any) => {
        this.errorMessage = 'Failed to create user. Please try again later.';
        this.successMessage = '';
      },
      () => {
        this.isSubmitting = false;
      }
    );
  }
}
