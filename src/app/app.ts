import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatButton } from '@angular/material/button'
import * as signalR from '@microsoft/signalr';
import { GameInfoBlock } from './components/game-info-block/game-info-block';
import { Board } from './components/board/board';
import { Score } from './components/score/score';
import { ApiService } from './services/api-service';
import { utilities } from './shared/utilities';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MatButton, GameInfoBlock, Board, Score],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  public gameStarted = false;
  public gameOver = false;
  public connected = false;
  public numberOfWins = 0;
  public numberOfLosses = 0;
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
  public isMultiplayer = signal(true);
  public waitingForMatch = false;

  apiService = inject(ApiService);

  requestJoinMultiplayerGame() {
    this.gameOver = false;
    this.gameStarted = false;
    this.waitingForMatch = true;
    this.apiService.apiRequestJoinMultiplayerGame(this.userid(), this.username())
      .subscribe((startUserInfo) => {
        console.log('initial server response (generated user info)', startUserInfo);
        this.username.set(startUserInfo.userName);
        this.userid.set(startUserInfo.userId);
        this.opponentUsername.set('');
        if (!this.connected) this.connectSignalR(startUserInfo.userId);
      })
  }
  requestJoinSingleplayerGame() {
    this.gameOver = false;
    this.gameStarted = false;
    this.apiService.apiRequestJoinSingleplayerGame(this.userid(), this.username())
      .subscribe(() => {
        console.log('initial server response');
        this.opponentUsername.set('Computer');
        if (!this.connected) this.connectSignalR(this.userid());
      })
  }

  quitGame() {
    this.apiService.apiQuitGame(this.userid(), utilities.getPath(this.isMultiplayer()))
      .subscribe(() => {
        
      });
  }

  rollSticks() {
    this.apiService.apiRollSticks(this.userid(), utilities.getPath(this.isMultiplayer()))
      .subscribe((result) => {
        
      })
  }

  skipTurn() {
    this.apiService.apiChangeTurn(this.userid(), !this.isWhiteTurn(), utilities.getPath(this.isMultiplayer()))
      .subscribe((result) => {
        
      })
  }
  
  async connectSignalR(id: string) {
    if (!id) {
      console.warn("connectSignalR called without id");
      return;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${utilities.serverUrl}/notifications`, { withCredentials: true })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // receive match info from server
    connection.on("MatchFound", (message) => {
      console.log("Message from SignalR hub: matched with opponent", message);
      this.isMultiplayer.set(true);
      if (message.playerWhite?.userId == id) {
        this.opponentUsername.set(message.playerBlack?.userName);
        this.isPlayerWhite.set(true);
      }
      else {
        this.opponentUsername.set(message.playerWhite?.userName);
        this.isPlayerWhite.set(false);
      }
      this.gameStarted = true;
    });
    
    connection.on("ComputerMatchFound", (message) => {
      console.log("Message from SignalR hub: matched with computer opponent", message);
      if (message.playerWhite?.userId == id) {
        this.opponentUsername.set(message.playerBlack?.userName);
        this.isPlayerWhite.set(true);
      }
      else {
        this.opponentUsername.set(message.playerWhite?.userName);
        this.isPlayerWhite.set(false);
      }
      this.gameStarted = true;
    });

    connection.on("MatchNotFound", (message) => {
      console.log("Message from SignalR hub: no other players online");
      alert('No players found to match with - connecting to computer opponent.');
      this.isMultiplayer.set(false);
      this.requestJoinSingleplayerGame();
    });

    // receive board state updates from server
    connection.on("BoardUpdated", (message) => {
      console.log("Message from SignalR hub: game board updated", message);
      this.sticksValue.set(message.sticksValue);
      this.whitePawns.set(message.whitePositions);
      this.blackPawns.set(message.blackPositions);
      this.movablePawns.set(message.movablePositions);
      this.isWhiteTurn.set(message.isWhiteTurn);
      this.waitingForMatch = false;
    });
    connection.on("GameOver", (message) => {
      console.log("Message from SignalR hub: game over", message);
      this.gameOver = true;
      this.winner.set(message.userId);
      this.numberOfWins += message.userId == this.userid() ? 1 : 0;
      this.numberOfLosses += message.userId == this.userid() ? 0 : 1;
      this.whitePawns.set([]);
      this.blackPawns.set([]);
      this.movablePawns.set([]);
      this.sticksValue.set(0);
    });

    try {
      await connection.start();
      console.log("SignalR connected with userId:", id);
      this.connected = true;
    } catch (err) {
      console.error("Failed to start or register SignalR connection", err);
    }

    return async () => {
      await connection.stop();
    };
  }
}
