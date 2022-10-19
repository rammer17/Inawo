import { NotExpr } from "@angular/compiler";
import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    isLogged$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

    constructor() {
        this.isLoggedIn();
    }
    loggedStateChange(newStatus: boolean) {
        this.isLogged$.next(newStatus);
    }

    isLoggedIn() {
        const token = localStorage.getItem('token');
        if(token) {
            this.isLogged$.next(true);
        } else {
            this.isLogged$.next(false);
        }
    }
}
