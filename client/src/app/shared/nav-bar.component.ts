import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.scss']
})
export class NavBarComponent {
  firstName: string = '';
  lastName: string = '';
  isLoggedIn: boolean = false;

  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit(): void {
    // Check if the user is logged in
    this.isLoggedIn = this.authService.isLoggedIn();
    
    // If the user is logged in, fetch the username from the claims
    if (this.isLoggedIn) {
      const claims = this.authService.getClaims();
      if (claims && claims.sub) {
        const fullName = claims.sub.split('.'); // Split name by '.'
        this.firstName = fullName[0];
        this.lastName = fullName[1];
      }
    }
  }

  // Logout method to clear session and navigate to login page
  logout(): void {
    this.authService.logout();
  }

  // Login method to navigate to login page
  login(): void {
    this.router.navigate(['/login']);
  }

}
