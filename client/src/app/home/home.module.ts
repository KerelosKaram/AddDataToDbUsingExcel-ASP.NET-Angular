import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SmsComponent } from './sms/sms.component';
import { HomeComponent } from './home.component';
import { QsCustomerBrandTargetComponent } from './qs-customer-brand-target/qs-customer-brand-target.component';
import { QsCustomerTargetComponent } from './qs-customer-target/qs-customer-target.component';
import { MatMenuModule } from '@angular/material/menu'; // Import MatMenuModule
import { MatButtonModule } from '@angular/material/button';
import { IgpSalesmanTargetComponent } from './igp-salesman-target/igp-salesman-target.component';
import { IgpItemElamirComponent } from './igp-item-elamir/igp-item-elamir.component';
import { PskuItemElamirComponent } from './psku-item-elamir/psku-item-elamir.component';
import { ItemActiveDistElamirComponent } from './item-active-dist-elamir/item-active-dist-elamir.component'; // Import MatButtonModule

@NgModule({
  declarations: [
    HomeComponent,
    SmsComponent,
    QsCustomerBrandTargetComponent,
    QsCustomerTargetComponent,
    IgpSalesmanTargetComponent,
    IgpItemElamirComponent,
    PskuItemElamirComponent,
    ItemActiveDistElamirComponent,
  ],
  imports: [
    CommonModule,
    MatMenuModule, // Add MatMenuModule
    MatButtonModule, // Add MatButtonModule
  ],
})
export class HomeModule {}
