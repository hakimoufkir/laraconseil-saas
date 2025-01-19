import { Component } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { loadStripe, Stripe } from '@stripe/stripe-js';
import { v4 as uuidv4 } from 'uuid';
import { KeycloakService } from 'keycloak-angular'; // Assuming you're using Keycloak Angular
import { KeycloakProfile } from 'keycloak-js';

@Component({
  selector: 'app-card-selection',
  templateUrl: './app-card-selection.component.html',
  styleUrls: ['./app-card-selection.component.css']
})
export class AppCardSelectionComponent {
  cards = [
    { id: 1, title: 'Grower', imageUrl: '/assets/images/grower.png', altText: 'Grower Plan' },
    { id: 2, title: 'Station', imageUrl: '/assets/images/station.png', altText: 'Station Plan' }
  ];
  selectedCard: number | null = null;
  imageLoading: { [key: number]: boolean } = {};
  profile!: KeycloakProfile;
  tenantId = uuidv4();
  errorMessage = '';
  isLoading = false;

  private stripe: Stripe | null = null;
  private stripePublicKey = environment.stripePublicKey;
  private apiEndpoint = environment.apiEndpoint;

  constructor(private http: HttpClient, private keycloakService: KeycloakService) {
    this.initializeStripe();
  }

  async ngOnInit() {
    try {
      if (await this.keycloakService.isLoggedIn()) {
        this.profile = await this.keycloakService.loadUserProfile();
      }
    } catch (error) {
      console.error('Failed to load user profile:', error);
      this.errorMessage = 'Failed to load user profile. Please try again.';
    }
  }

  /**
   * Initializes Stripe instance with the provided public key.
   */
  private async initializeStripe(): Promise<void> {
    try {
      this.stripe = await loadStripe(this.stripePublicKey);
      if (!this.stripe) {
        throw new Error('Stripe could not be initialized.');
      }
    } catch (error) {
      console.error('Stripe initialization error:', error);
      this.errorMessage = 'Failed to initialize Stripe. Please try again later.';
    }
  }

  /**
   * Handles card selection and sets the selected card ID.
   */
  selectCard(cardId: number): void {
    this.selectedCard = cardId;
  }

  /**
   * Handles image load events.
   */
  onImageLoad(cardId: number): void {
    this.imageLoading[cardId] = false;
  }

  /**
   * Handles image error events.
   */
  onImageError(cardId: number): void {
    this.imageLoading[cardId] = false;
    console.error(`Image failed to load for card ID: ${cardId}`);
  }

  /**
   * Creates a checkout session using the selected plan and user details.
   */
  createCheckoutSession(): void {
    if (!this.isFormValid()) {
      return;
    }

    this.setLoadingState(true);

    const selectedCard = this.cards.find(card => card.id === this.selectedCard);
    const payload = {
      tenantName: this.profile?.firstName || 'Unknown Tenant',
      email: this.profile?.email || 'unknown@example.com',
      planType: selectedCard?.title || '',
      tenantId: this.tenantId
    };

    console.log('Payload:', payload);

    this.http.post<{ sessionId: string }>(`${this.apiEndpoint}/Payment/create-checkout-session`, payload).subscribe({
      next: (response) => this.redirectToStripeCheckout(response.sessionId),
      error: (error) => this.handleError('Failed to create a checkout session. Please try again.', error),
      complete: () => this.setLoadingState(false),
    });
  }

  /**
   * Validates the form input.
   */
  private isFormValid(): boolean {
    if (!this.selectedCard) {
      this.errorMessage = 'Please select a plan.';
      return false;
    }
    if (!this.profile || !this.profile.email) {
      this.errorMessage = 'User profile information is missing.';
      return false;
    }
    this.errorMessage = '';
    return true;
  }

  /**
   * Redirects the user to Stripe Checkout.
   */
  private async redirectToStripeCheckout(sessionId: string): Promise<void> {
    if (!this.stripe) {
      this.errorMessage = 'Stripe is not initialized.';
      return;
    }

    const { error } = await this.stripe.redirectToCheckout({ sessionId });

    if (error) {
      this.handleError(error.message || 'Failed to redirect to Stripe Checkout.', error);
    }
  }

  /**
   * Handles error messages and logs the error to the console.
   */
  private handleError(message: string, error: any): void {
    this.errorMessage = message;
    console.error(message, error);
  }

  /**
   * Sets the loading state and clears any existing error messages.
   */
  private setLoadingState(isLoading: boolean): void {
    this.isLoading = isLoading;
    if (isLoading) {
      this.errorMessage = '';
    }
  }
}
