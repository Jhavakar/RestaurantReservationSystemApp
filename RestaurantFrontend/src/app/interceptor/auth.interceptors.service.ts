import { Injectable } from '@angular/core';
import {
    HttpRequest,
    HttpHandler,
    HttpEvent,
    HttpInterceptor,
    HttpResponse,
    HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

    constructor(private router: Router) {}

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // Add auth token to headers if it exists
        const authToken = localStorage.getItem('authToken');
        if (authToken) {
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${authToken}`
                }
            });
        }

        // Handle the response
        return next.handle(request).pipe(
            catchError((error: HttpErrorResponse) => {
                // Redirect to login if unauthorized
                if (error.status === 401) {
                    this.router.navigate(['/login']);
                }
                return throwError(error);
            })
        );
    }
}
