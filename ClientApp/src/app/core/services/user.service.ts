import {
  HttpClient,
  HttpBackend,
  HttpHeaders,
} from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private httpWithoutInterceptor: HttpClient;
  private url = 'http://localhost:5001';

  constructor(private http: HttpClient, private httpBackend: HttpBackend) {
    this.httpWithoutInterceptor = new HttpClient(httpBackend);
  }

  signIn(body: any) {
    var headers = new HttpHeaders({
      Accept: 'application/json',
    });
    return this.http.post(`${this.url}/User/Login`, body, {
      headers: headers,
      responseType: 'json',
    });
  }

  signUp(body: any) {
    var headers = new HttpHeaders({
      Accept: 'application/json',
    });

    return this.http.post(`${this.url}/User/Register`, body, {
      headers: headers,
      responseType: 'json',
    });
  }

  getAll() {
    return this.http.get(`${this.url}/User/GetAll`);
  }

  getInfo() {
    var headers = new HttpHeaders({
      Accept: 'application/json',
    });
    return this.http.get(`${this.url}/User/GetById`, { headers: headers });
  }

  update(body: any) {
    var headers = new HttpHeaders({
      Accept: 'application/json',
    });
    return this.http.put(`${this.url}/User/UpdateAccount`, body, {
      headers: headers,
      responseType: 'json',
    });
  }

  delete(body: any) {
    return this.http.delete(`${this.url}/User/DeleteAccount`, {
      body: body,
    });
  }
}
