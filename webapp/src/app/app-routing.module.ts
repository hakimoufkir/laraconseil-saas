import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GrowerComponent } from './components/grower/grower.component';
import { AuthGuard } from './guards/auth.guard';
import { StationComponent } from './components/station/station.component';
import { CheckoutComponent } from './components/checkout/checkout.component';
import { CancelComponent } from './components/cancel/cancel.component';
import { SuccessComponent } from './components/success/success.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { AppComponent } from './app.component';

const routes: Routes = [
  {path: 'grower', component: GrowerComponent, canActivate: [AuthGuard], data: { roles: ['Grower'] }},
  {path: 'station', component: StationComponent, canActivate: [AuthGuard], data: { roles: ['Station'] }},
  { path: 'checkout', component: CheckoutComponent },
  { path: 'success', component: SuccessComponent },
  { path: 'cancel', component: CancelComponent },
  { path: 'dashboard', component: DashboardComponent },
  {path : '', component : DashboardComponent},
  { path: '**', component: NotFoundComponent },

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
