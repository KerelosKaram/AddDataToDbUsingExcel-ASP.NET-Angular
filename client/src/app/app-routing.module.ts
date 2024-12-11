import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login.component';
import { SmsComponent } from './home/sms/sms.component';
import { AuthGuard } from './core/guards/auth.guard';
import { HomeComponent } from './home/home.component';
import { NotAuthorizedComponent } from './auth/not-authorized/not-authorized.component';
import { QsCustomerBrandTargetComponent } from './home/qs-customer-brand-target/qs-customer-brand-target.component';
import { QsCustomerTargetComponent } from './home/qs-customer-target/qs-customer-target.component';

const routes: Routes = [
  // Login route does not have the NavBar
  { path: 'login', component: LoginComponent },

  // All routes below this will have the NavBar (as it's in AppComponent)
  { path: 'home', component: HomeComponent },
  
  { path: 'sms', component: SmsComponent, canActivate: [AuthGuard], data: { role: 'SMS' } },  // Role required to access this route
  { path: 'qscustomertarget', component: QsCustomerTargetComponent },  // Role required to access this route
  { path: 'qscustomerbrandtarget', component: QsCustomerBrandTargetComponent },  // Role required to access this route
  // { path: 'qscustomerbrandtarget', component: QsCustomerBrandTargetComponent, canActivate: [AuthGuard], data: { role: 'QsCustomerBrandTarget' } },  // Role required to access this route
  
  { path: 'not-authorized', component: NotAuthorizedComponent },
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: '**', redirectTo: 'home' },  // Catch-all route for undefined paths
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}