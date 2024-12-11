import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/login`; // API endpoint for login
  private tokenKey = 'jwtToken';
  private rolesKey = 'userRoles';
  private claimsKey = 'userClaims';

  constructor(private http: HttpClient, private router: Router) {}

  // Method to call the backend API and send user credentials
  checkUser(userData: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, userData, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
      }),
    });
  }

  // Store token, roles, and claims in localStorage
  storeUserData(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  
    // Decode token to extract roles and claims
    const decodedToken = this.decodeJwtToken(token);
  
    // Extract roles and claims (you can adjust this based on your token structure)
    // Assuming 'role' is stored as a string
    const roles = decodedToken['role'] ? [decodedToken['role']] : decodedToken['userClaims']?.role ? [decodedToken['userClaims'].role] : [];
    
    const claims = decodedToken; // You can modify this to extract specific claims if necessary
  
    // Store roles and claims in localStorage
    localStorage.setItem(this.rolesKey, JSON.stringify(roles));
    localStorage.setItem(this.claimsKey, JSON.stringify(claims));
  
    this.router.navigate(['/home']);
  }

  // Retrieve the JWT token
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  // Retrieve roles from localStorage
  getRoles(): string[] {
    const roles = localStorage.getItem(this.rolesKey);
    return roles ? JSON.parse(roles) : [];
  }

  // Retrieve claims from localStorage
  getClaims(): any {
    const claims = localStorage.getItem(this.claimsKey);
    return claims ? JSON.parse(claims) : null;
  }

  // Check if token is valid and not expired
  isLoggedIn(): boolean {
    const token = this.getToken();
    if (token) {
      try {
        const decodedToken = this.decodeJwtToken(token);
        const currentTime = Math.floor(Date.now() / 1000); // Current time in seconds
        return decodedToken.exp > currentTime; // Token is valid if not expired
      } catch (error) {
        console.error('Failed to decode token:', error);
        return false; // Invalid token
      }
    }
    return false; // No token present
  }

  // Decode JWT token using jwt-decode
  decodeJwtToken(token: string): any {
    try {
      return jwtDecode(token);
    } catch (error) {
      console.error('Failed to decode JWT:', error);
      return null;
    }
  }

  // Check if the user has a specific role
  hasRole(role: string): boolean {
    const roles = this.getRoles();
    return roles.includes(role);
  }

  // Method to log the user in (this is just an example, adjust as needed)
  login(): void {
    // Implement your login logic here, like redirecting to a login page or calling an API
    this.router.navigate(['/login']);
  }

  // Logout and clear localStorage
  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.rolesKey);
    localStorage.removeItem(this.claimsKey);
    this.router.navigateByUrl('/login'); // Redirect to login page after logout
  }

  // Example of making a request with the Bearer token
  getUserData(): Observable<any> {
    const token = this.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });
    return this.http.get<any>(`${environment.apiUrl}/user-data`, { headers });
  }

  // Check if the token has expired
  isTokenExpired(token: string): boolean {
    try {
      const decoded: any = jwtDecode(token);
      const currentTime = Math.floor(Date.now() / 1000); // Current time in seconds
      return decoded.exp < currentTime; // Token expired if current time is greater than exp
    } catch (e) {
      console.error('Error decoding token', e);
      return true; // Invalid token
    }
  }
}