import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:44322/api/login';
  private tokenKey = 'authToken';
  private userKey = 'authUser';
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient) {
    this.checkAuthStatus();
  }

  login(user: string, password: string): Observable<any> {
    return this.http.post<any>(this.apiUrl, { user, password }).pipe(
      tap(response => {
        if (response && response.token) {
          localStorage.setItem(this.tokenKey, response.token);
          localStorage.setItem(this.userKey, response.user);
          this.isAuthenticatedSubject.next(true);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.isAuthenticatedSubject.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getUser(): string | null {
    return localStorage.getItem(this.userKey);
  }

  isLoggedIn(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  private checkAuthStatus(): void {
    const token = localStorage.getItem(this.tokenKey);
    this.isAuthenticatedSubject.next(!!token);
  }
}
