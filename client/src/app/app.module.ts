import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { AuthService } from './auth/auth.service';
import { LoginModule } from './auth/login/login.module';
import { AuthGuard } from './core/guards/auth.guard';
import { NotAuthorizedComponent } from './auth/not-authorized/not-authorized.component';
import { HomeModule } from './home/home.module';  // Importing the HomeModule here
import { NavBarComponent } from './shared/nav-bar.component'; // NavBar should be here
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';


@NgModule({
  declarations: [
    AppComponent,
    NotAuthorizedComponent,
    NavBarComponent,  // No need to add HomeComponent here
  ],
  imports: [
    BrowserModule,
    LoginModule,
    HomeModule,  // HomeModule should be imported to handle all home-related components
    AppRoutingModule,
    ReactiveFormsModule,
    HttpClientModule,
    BrowserAnimationsModule,

  ],
  providers: [AuthService, AuthGuard],
  bootstrap: [AppComponent],
})
export class AppModule { }
