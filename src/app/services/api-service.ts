import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { UserInfo } from '../model/user-info';
import { utilities } from '../shared/utilities'

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  http = inject(HttpClient);
  
  apiRequestJoinMultiplayerGame() {
    return this.http.get<UserInfo>(`${utilities.serverUrl}/multiplayer/games`, {
      withCredentials: true
    })
  }
  
  apiRequestJoinSingleplayerGame(userid:string, username:string) {
    console.log('got here', userid, username);
    return this.http.get(`${utilities.serverUrl}/singleplayer/games/${username}/${userid}`, {
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
