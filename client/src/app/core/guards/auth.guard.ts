import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from 'src/app/auth/auth.service';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const isLoggedIn = this.authService.isLoggedIn();
    const requiredRole = route.data['role']; // Get the required role from route data

    if (isLoggedIn && this.authService.hasRole(requiredRole)) {
      // Allow access if logged in and has the required role
      return true;
    } else if (isLoggedIn) {
      // Redirect to "Not Authorized" if logged in but lacks the required role
      this.router.navigate(['/not-authorized']);
      return false;
    } else {
      // Redirect to login if not logged in
      this.router.navigate(['/login']);
      return false;
    }
  }
}
