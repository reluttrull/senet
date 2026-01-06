import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { UserInfo } from '../model/user-info';
import { utilities } from '../shared/utilities'

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  http = inject(HttpClient);
  
  apiRequestJoinMultiplayerGame(userid:string, username:string) {
    return this.http.post<UserInfo>(`${utilities.serverUrl}/multiplayer/games`,
      { "userId": userid, "userName": username }, 
      { withCredentials: true })
  }
  
  apiRequestJoinSingleplayerGame(userid:string, username:string) {
    return this.http.post(`${utilities.serverUrl}/singleplayer/games`,
      { "userId": userid, "userName": username }, 
      { withCredentials: true })
  }

  apiQuitGame(userid:string, gametype:string) {
    return this.http.delete(`${utilities.serverUrl}/${gametype}/game/${userid}`, {
      withCredentials: true
    })
  }

  apiRollSticks(userid:string, gametype:string) {
    return this.http.put(`${utilities.serverUrl}/${gametype}/sticks/${userid}`, {
      withCredentials: true
    })
  }

  apiChangeTurn(userid:string, nextIsWhiteTurn:boolean, gametype:string) {
    return this.http.put(`${utilities.serverUrl}/${gametype}/turns/${userid}/${nextIsWhiteTurn}`, {
      withCredentials: true
    })
  }

  apiMovePawn(userid:string, startPosition: number, gametype:string) {
    let params = new HttpParams();
    params = params.set('startPosition', startPosition);
    return this.http.put(`${utilities.serverUrl}/${gametype}/pawns/${userid}/${startPosition}`, {
      params: params,
      withCredentials: true
    })
  }
}
