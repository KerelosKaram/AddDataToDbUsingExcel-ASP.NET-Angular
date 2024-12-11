import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SmsComponent } from './sms/sms.component';
import { HomeComponent } from './home.component';
import { QsCustomerBrandTargetComponent } from './qs-customer-brand-target/qs-customer-brand-target.component';
import { QsCustomerTargetComponent } from './qs-customer-target/qs-customer-target.component';
import { MatMenuModule } from '@angular/material/menu'; // Import MatMenuModule
import { MatButtonModule } from '@angular/material/button'; // Import MatButtonModule

@NgModule({
  declarations: [
    HomeComponent,
    SmsComponent,
    QsCustomerBrandTargetComponent,
    QsCustomerTargetComponent,
  ],
  imports: [
    CommonModule,
    MatMenuModule, // Add MatMenuModule
    MatButtonModule, // Add MatButtonModule
  ],
})
export class HomeModule {}
