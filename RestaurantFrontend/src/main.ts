import { enableProdMode } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { environment } from './environment/environment';
import { importProvidersFrom } from '@angular/core';
import { HttpClientModule } from '@angular/common/http'; // Use HttpClientModule without withFetch unless needed
import { RouterModule } from '@angular/router';
import { routes } from './app/app.routes';

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    importProvidersFrom([
      HttpClientModule,
      RouterModule.forRoot(routes), // Centralized routing setup
    ]),
    // If you need to use fetch API specifically, uncomment the next line
    // provideHttpClient(withFetch()),
  ],
}).catch(err => console.error(err));
