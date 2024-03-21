import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  standalone: true,
  imports: [
    RouterModule, 
    HttpClientModule,
    // Import other standalone components or modules as needed
  ],
})
export class AppComponent {
  title = 'RestaurantFrontend';
}
