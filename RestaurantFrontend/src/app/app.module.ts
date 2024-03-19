import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { ReservationFormComponent } from './reservation-form/reservation-form.component'

@NgModule({
  declarations: [
    AppComponent,
    ReservationFormComponent // This line is crucial
  ],
  imports: [
    BrowserModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
