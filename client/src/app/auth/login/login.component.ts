import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  errorMessage: string | null = null; // Variable to hold error message
  isLoading: boolean = false; // State to control loading spinner

  constructor(
    private fb: FormBuilder,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Initialize the login form with validation
    this.loginForm = this.fb.group({
      username: ['', [Validators.required]], // Username is required
      password: ['', [Validators.required, Validators.minLength(6)]], // Password is required and must be at least 6 characters
    });
  }

  // Method to handle form submission
  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true; // Show spinner
      this.errorMessage = null; // Reset error message

      const loginData = this.loginForm.value;

      this.authService.checkUser(loginData).subscribe(
        (response) => {
          this.isLoading = false; // Hide spinner
          if (response && response.token) {
            // Store the JWT token and redirect to SMS page
            this.authService.storeUserData(response.token);
          }
        },
        (error) => {
          this.isLoading = false; // Hide spinner
          console.error('Login failed', error);
          this.errorMessage = error.error?.message || 'Login failed. Please try again.';
        }
      );
    }
  }
}
