import { enableProdMode } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { environment } from './environment/environment';
import { importProvidersFrom } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { routes } from './app/app.routes';
import { AuthInterceptor } from './app/interceptor/auth.interceptors.service';

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    importProvidersFrom(
      HttpClientModule,
      RouterModule.forRoot(routes)
    ),
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },

    // If you need to use fetch API specifically, uncomment the next line
    // provideHttpClient(withFetch()),
  ],
  
}).catch(err => console.error(err));
