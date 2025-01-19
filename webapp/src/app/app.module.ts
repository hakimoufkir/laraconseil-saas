import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { KeycloakAngularModule, KeycloakService } from 'keycloak-angular';
import { GrowerComponent } from './components/grower/grower.component';
import { StationComponent } from './components/station/station.component';
import { CancelComponent } from './components/cancel/cancel.component';
import { CheckoutComponent } from './components/checkout/checkout.component';
import { SuccessComponent } from './components/success/success.component';


import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { NavbarComponent } from './components/navbar/navbar.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { FooterComponent } from './components/footer/footer.component';
import { UserSignupComponent } from './components/user-signup/user-signup.component';
import { HomeComponent } from './components/home/home.component';
import { AppCardSelectionComponent } from './components/app-card-selection/app-card-selection.component';


function initializeKeycloak(keycloak: KeycloakService) {
  return () =>
    keycloak.init({
      config: {
        url: 'https://keycloaklaraconseil.azurewebsites.net', // Replace with your Keycloak server URL
        realm: 'MultiTenantRealm', // Replace with your realm
        clientId: 'LaraConseil', // Replace with your client ID
      },
      initOptions: {
        onLoad: 'check-sso', 
        silentCheckSsoRedirectUri:
          window.location.origin + '/assets/silent-check-sso.html'
      },
    });
}


@NgModule({
  declarations: [
    AppComponent,
    GrowerComponent,
    StationComponent,
    CancelComponent,
    CheckoutComponent,
    SuccessComponent,
    DashboardComponent,
    NavbarComponent,
    NotFoundComponent,
    FooterComponent,
    UserSignupComponent,
    HomeComponent,
    AppCardSelectionComponent,
  ],
  imports: [
    KeycloakAngularModule,
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    CommonModule,
    HttpClientModule,
  ],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: initializeKeycloak,
      multi: true,
      deps: [KeycloakService]
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
