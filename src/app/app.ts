import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import * as signalR from '@microsoft/signalr';
import { GameInfoBlock } from './components/game-info-block/game-info-block';
import { Board } from './components/board/board';
import { ApiService } from './services/api-service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, GameInfoBlock, Board],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly serverUrl = 'https://localhost:7019';
  protected readonly title = signal('Senet Client');
  public gameStarted = signal(false);
  public userid = signal('');
  public username = signal('');
  public opponentUsername = signal('');
  public sticksValue = signal(0);
  public whitePawns = signal([]);
  public blackPawns = signal([]);

  apiService = inject(ApiService);

  requestJoinGame() {
    this.apiService.apiRequestJoinGame()
      .subscribe((startUserInfo) => {
        console.log('initial server response (generated user info)', startUserInfo);
        this.username.set(startUserInfo.userName);
        this.userid.set(startUserInfo.userId);
        this.connectSignalR(startUserInfo.userId);
      })
  }

  rollSticks() {
    this.apiService.apiRollSticks()
      .subscribe((result) => {
        
      })
  }
  
  async connectSignalR(id: string) {
    if (!id) {
      console.warn("connectSignalR called without id");
      return;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.serverUrl}/notifications`, { withCredentials: true })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // receive match info from server
    connection.on("MatchFound", (message) => {
      console.log("Message from SignalR hub: matched with opponent", message);
      if (message.playerWhite?.userId == id) this.opponentUsername.set(message.playerBlack?.userName);
      else this.opponentUsername.set(message.playerWhite?.userName);
      this.gameStarted.set(true);
    });
    // receive board state updates from server
    connection.on("BoardUpdated", (message) => {
      console.log("Message from SignalR hub: game board updated", message);
      this.sticksValue.set(message.sticksValue);
      this.whitePawns.set(message.whitePositions);
      this.blackPawns.set(message.blackPositions);
    });

    try {
      await connection.start();
      console.log("SignalR connected with userId:", id);
    } catch (err) {
      console.error("Failed to start or register SignalR connection", err);
    }

    return async () => {
      await connection.stop();
    };
  }
}
