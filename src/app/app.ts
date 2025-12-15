import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import * as signalR from '@microsoft/signalr';
import { GameInfoBlock } from './components/game-info-block/game-info-block';
import { Board } from './components/board/board';
import { Score } from './components/score/score';
import { ApiService } from './services/api-service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, GameInfoBlock, Board, Score],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly serverUrl = 'https://localhost:7019';
  protected readonly title = signal('Senet Client');
  public gameStarted = signal(false);
  public gameOver = signal(false);
  public userid = signal('');
  public username = signal('');
  public isPlayerWhite = signal(false);
  public opponentUsername = signal('');
  public isWhiteTurn = signal(true);
  public sticksValue = signal(0);
  public whitePawns = signal([]);
  public blackPawns = signal([]);
  public movablePawns = signal([]);
  public winner = signal('');

  apiService = inject(ApiService);

  requestJoinGame() {
    this.gameOver.set(false);
    this.apiService.apiRequestJoinGame()
      .subscribe((startUserInfo) => {
        console.log('initial server response (generated user info)', startUserInfo);
        this.username.set(startUserInfo.userName);
        this.userid.set(startUserInfo.userId);
        this.opponentUsername.set('');
        this.connectSignalR(startUserInfo.userId);
      })
  }

  rollSticks() {
    this.apiService.apiRollSticks(this.userid())
      .subscribe((result) => {
        
      })
  }

  skipTurn() {
    this.apiService.apiChangeTurn(this.userid(), !this.isWhiteTurn())
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
      if (message.playerWhite?.userId == id) {
        this.opponentUsername.set(message.playerBlack?.userName);
        this.isPlayerWhite.set(true);
      }
      else {
        this.opponentUsername.set(message.playerWhite?.userName);
        this.isPlayerWhite.set(false);
      }
      this.gameStarted.set(true);
    });

    connection.on("MatchNotFound", (message) => {
      console.log("Message from SignalR hub: no other players online");
    });

    // receive board state updates from server
    connection.on("BoardUpdated", (message) => {
      console.log("Message from SignalR hub: game board updated", message);
      this.sticksValue.set(message.sticksValue);
      this.whitePawns.set(message.whitePositions);
      this.blackPawns.set(message.blackPositions);
      this.movablePawns.set(message.movablePositions);
      this.isWhiteTurn.set(message.isWhiteTurn);
    });
    connection.on("GameOver", (message) => {
      console.log("Message from SignalR hub: game over", message);
      this.gameOver.set(true);
      this.winner.set(message.userId);
      this.whitePawns.set([]);
      this.blackPawns.set([]);
      this.movablePawns.set([]);
      this.sticksValue.set(0);
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
