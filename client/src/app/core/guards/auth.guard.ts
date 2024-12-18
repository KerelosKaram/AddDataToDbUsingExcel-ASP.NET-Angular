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
    const roles = this.authService.getRoles(); // Retrieve roles
  
    if (isLoggedIn && roles.includes(requiredRole)) {
      return true;
    } else if (isLoggedIn) {
      this.router.navigate(['/not-authorized']);
      return false;
    } else {
      this.router.navigate(['/login']);
      return false;
    }
  }
}
