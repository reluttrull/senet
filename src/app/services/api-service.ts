import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserInfo } from '../model/user-info';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  protected readonly serverUrl = 'https://localhost:7019';
  http = inject(HttpClient);
  
  apiRequestJoinGame() {
    return this.http.get<UserInfo>(`${this.serverUrl}/game/requestjoingame`, {
      withCredentials: true
    })
  }

  apiRollSticks() {
    console.log('sdfsjdfksj');
    return this.http.get(`${this.serverUrl}/game/rollsticks`, {
      withCredentials: true
    })
  }
}
