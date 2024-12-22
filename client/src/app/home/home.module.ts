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
import { ItemActiveDistElamirComponent } from './item-active-dist-elamir/item-active-dist-elamir.component'; // Import MatButtonModule
import { PskuItemElamirSql2017Component } from './psku-item-elamir-sql2017/psku-item-elamir-sql2017.component';
import { PskuItemElamirDbElWagdComponent } from './psku-item-elamir-dbelwagd/psku-item-elamir-dbelwagd.component';

@NgModule({
  declarations: [
    HomeComponent,
    SmsComponent,
    QsCustomerBrandTargetComponent,
    QsCustomerTargetComponent,
    IgpSalesmanTargetComponent,
    IgpItemElamirComponent,
    PskuItemElamirSql2017Component,
    PskuItemElamirDbElWagdComponent,
    ItemActiveDistElamirComponent,
  ],
  imports: [
    CommonModule,
    MatMenuModule, // Add MatMenuModule
    MatButtonModule, // Add MatButtonModule
  ],
})
export class HomeModule {}
