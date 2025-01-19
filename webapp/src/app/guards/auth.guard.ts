import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot
} from '@angular/router';
import { KeycloakAuthGuard, KeycloakService } from 'keycloak-angular';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard extends KeycloakAuthGuard {
  constructor(
    protected override router: Router,
    protected readonly keycloak: KeycloakService
  ) {
    super(router, keycloak);
  }

  public async isAccessAllowed(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ) {
    // Force the user to log in if currently unauthenticated.
    if (!this.authenticated) {
      await this.keycloak.login({
        redirectUri: window.location.origin + state.url
      });
    }

    // Get the roles required from the route.
    const requiredRoles = route.data['roles'];
    const roleMode = route.data['roleMode'] || 'any'; // Default to 'any' if not specified

    // Allow the user to proceed if no additional roles are required to access the route.
    if (!Array.isArray(requiredRoles) || requiredRoles.length === 0) {
      return true;
    }

    // Allow the user to proceed based on the role mode
    if (roleMode === 'all') {
      // Check if the user has all the required roles
      return requiredRoles.every((role) => this.roles.includes(role));
    } else if (roleMode === 'any') {
      // Check if the user has at least one of the required roles
      return requiredRoles.some((role) => this.roles.includes(role));
    }

    // If roleMode is unknown or not specified, deny access
    return false;
  }
}
