import { Component } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'El Amir Group';
  showNavBar = true; // This flag will control the visibility of the NavBar
  
  constructor(private router: Router) {}

  ngOnInit() {
    // Listen to route changes
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      // Hide NavBar on the login route
      this.showNavBar = this.router.url !== '/login';
    });
  }
}
