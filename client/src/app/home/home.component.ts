import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  // Define the menu items
  menuItems = [
    { name: 'SMS Service', action: '/sms' },
    {
      name: 'Add Data',
      submenus: [
        { name: 'QSCustomerBrandTarget', action: '/qscustomerbrandtarget' },
        { name: 'QSCustomerTarget', action: '/qscustomertarget' },
        { name: 'IGPSalesmanTarget', action: '/igpsalesmantarget' },
        { name: 'IGPItemElamir', action: '/igpitemelamir' },
        { name: 'PSKUItemElamir', action: '/pskuitemelamir' },
        { name: 'ItemActiveDistElamir', action: '/itemactivedistelamir' },
      ]
    }
  ];

  constructor(private router: Router) {}

    // Define the submenusMap object to map menu names to mat-menu references
    submenusMap: { [key: string]: any } = {};
    
  navigateTo(action: string): void {
    if (action) {
      this.router.navigate([action]);
    }
  }

  // Helper method to generate submenu IDs
  // getSubmenuId(name: string): string {
  //   return `submenu_${name.replace(/\s+/g, '_')}`;
  // }
}
