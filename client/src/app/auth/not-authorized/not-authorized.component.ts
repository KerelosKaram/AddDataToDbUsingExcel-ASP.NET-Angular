import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-not-authorized',
  templateUrl: './not-authorized.component.html',
  styleUrls: ['./not-authorized.component.scss'],
})
export class NotAuthorizedComponent {
  constructor(private router: Router) {}

  goBack(): void {
    this.router.navigate(['/home']); // Navigate to homepage
  }
}
