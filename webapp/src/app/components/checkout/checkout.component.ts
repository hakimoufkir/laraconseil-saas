import { Component } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { loadStripe, Stripe } from '@stripe/stripe-js';

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent {
  tenantName = '';
  email = '';
  errorMessage = '';
  isLoading = false;

  ngOnInit() {
    console.log("environment.apiEndpoint : " + environment.apiEndpoint);
  }

  private stripe: Stripe | null = null;
  private stripePublicKey = environment.stripePublicKey;
  private apiEndpoint = environment.apiEndpoint;

  constructor(private http: HttpClient) {
    this.initializeStripe();
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
   * Handles creating a checkout session and redirecting to Stripe Checkout.
   */
  createCheckoutSession(): void {
    if (!this.isFormValid()) {
      return;
    }

    this.setLoadingState(true);
    const payload = { tenantName: this.tenantName, email: this.email };

    this.http.post<{ sessionId: string }>(this.apiEndpoint, payload).subscribe({
      next: (response) => this.redirectToStripeCheckout(response.sessionId),
      error: (error) => this.handleError('Failed to create a checkout session. Please try again.', error),
      complete: () => this.setLoadingState(false),
    });
  }

  /**
   * Validates the form input.
   * @returns {boolean} True if the form is valid, otherwise false.
   */
  private isFormValid(): boolean {
    if (!this.tenantName || !this.email) {
      this.errorMessage = 'Both Tenant Name and Email are required.';
      return false;
    }
    this.errorMessage = '';
    return true;
  }

  /**
   * Redirects the user to Stripe Checkout.
   * @param sessionId The Stripe Checkout session ID.
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
   * @param message The user-friendly error message.
   * @param error The error object to log.
   */
  private handleError(message: string, error: any): void {
    this.errorMessage = message;
    console.error(message, error);
  }

  /**
   * Sets the loading state and clears any existing error messages.
   * @param isLoading The loading state.
   */
  private setLoadingState(isLoading: boolean): void {
    this.isLoading = isLoading;
    if (isLoading) {
      this.errorMessage = '';
    }
  }
}
