import { Injectable } from '@angular/core';
import {
    HttpRequest,
    HttpHandler,
    HttpEvent,
    HttpInterceptor,
    HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

    constructor(private router: Router) {}

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // Clone the request to add withCredentials: true
        // This ensures credentials are included in cross-origin requests
        request = request.clone({
            withCredentials: true
        });

        // Handle the response
        return next.handle(request).pipe(
            catchError((error: HttpErrorResponse) => {
                // Redirect to login if unauthorized
                if (error.status === 401) {
                    this.router.navigate(['/login']);
                }
                return throwError(() => error);
            })
        );
    }
}
