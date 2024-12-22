import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login.component';
import { SmsComponent } from './home/sms/sms.component';
import { AuthGuard } from './core/guards/auth.guard';
import { HomeComponent } from './home/home.component';
import { NotAuthorizedComponent } from './auth/not-authorized/not-authorized.component';
import { QsCustomerBrandTargetComponent } from './home/qs-customer-brand-target/qs-customer-brand-target.component';
import { QsCustomerTargetComponent } from './home/qs-customer-target/qs-customer-target.component';
import { IgpSalesmanTargetComponent } from './home/igp-salesman-target/igp-salesman-target.component';
import { PskuItemElamirSql2017Component } from './home/psku-item-elamir-sql2017/psku-item-elamir-sql2017.component';
import { IgpItemElamirComponent } from './home/igp-item-elamir/igp-item-elamir.component';
import { ItemActiveDistElamirComponent } from './home/item-active-dist-elamir/item-active-dist-elamir.component';
import { PskuItemElamirDbElWagdComponent } from './home/psku-item-elamir-dbelwagd/psku-item-elamir-dbelwagd.component';

const routes: Routes = [
  // Login route does not have the NavBar
  { path: 'login', component: LoginComponent },

  // All routes below this will have the NavBar (as it's in AppComponent)
  { path: 'home', component: HomeComponent },
  
  { path: 'sms', component: SmsComponent, canActivate: [AuthGuard], data: { role: 'SMS' } },  // Role required to access this route
  { path: 'qscustomertarget', component: QsCustomerTargetComponent, canActivate: [AuthGuard], data: { role: 'QSCustomerTarget' } },  // Role required to access this route
  { path: 'igpsalesmantarget', component: IgpSalesmanTargetComponent, canActivate: [AuthGuard], data: { role: 'IGPSalesmanTarget' } },  // Role required to access this route
  { path: 'igpitemelamir', component: IgpItemElamirComponent, canActivate: [AuthGuard], data: { role: 'IGPItemElamir' } },  // Role required to access this route
  { path: 'pskuitemelamirsql2017', component: PskuItemElamirSql2017Component, canActivate: [AuthGuard], data: { role: 'PSKUItemElamirSql2017' } },  // Role required to access this route
  { path: 'pskuitemelamirdbelwagd', component: PskuItemElamirDbElWagdComponent, canActivate: [AuthGuard], data: { role: 'PSKUItemElamirDbElWagd' } },  // Role required to access this route
  { path: 'qscustomerbrandtarget', component: QsCustomerBrandTargetComponent, canActivate: [AuthGuard], data: { role: 'QSCustomerBrandTarget' } },  // Role required to access this route
  { path: 'itemactivedistelamir', component: ItemActiveDistElamirComponent, canActivate: [AuthGuard], data: { role: 'ItemActiveDistElamir' } },  // Role required to access this route
  
  { path: 'not-authorized', component: NotAuthorizedComponent },
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: '**', redirectTo: 'home' },  // Catch-all route for undefined paths
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
