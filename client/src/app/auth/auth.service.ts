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
  // private rolesKey = 'userRoles';
  // private claimsKey = 'userClaims';

  constructor(private http: HttpClient, private router: Router) {}

  // Method to call the backend API and send user credentials
  checkUser(userData: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, userData, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
      }),
    });
  }

  // Update storeUserData to only store the token
  storeUserData(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }


  // Retrieve the JWT token
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  // Decode roles from the JWT on demand
  getRoles(): string[] {
    const token = this.getToken();
    if (!token) return [];
    const decodedToken = this.decodeJwtToken(token);
    return decodedToken?.role || []; // Adjust based on your JWT structure
  }

  // Decode claims on demand
  getClaims(): any {
    const token = this.getToken();
    return token ? this.decodeJwtToken(token) : null;
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

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.router.navigateByUrl('/login');
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