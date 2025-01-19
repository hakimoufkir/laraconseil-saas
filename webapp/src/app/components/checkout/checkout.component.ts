import { Component } from '@angular/core';


@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css']
})
export class CheckoutComponent {

  selectedOption?: CardOption;

  // onCardSelected(option: CardOption) {
  //   this.selectedOption = option;
  //   console.log('Selected:', option);
  // }

  onCardSelected(event: Event) {

    const cardOption = event as unknown as CardOption;
  
    this.selectedOption = cardOption;
    console.log('Selected:', event);

  
  }

  
}
export type CardOption = 'grower' | 'company';
