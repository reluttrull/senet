import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly serverUrl = 'https://localhost:7019';
  protected readonly title = signal('Senet Client');
  public userid = signal('');
  public username = signal('');
  public opponentUsername = signal('');

  requestJoinGame() {
    fetch(`${this.serverUrl}/game/requestjoingame`, { credentials: 'include' })
      .then((response) => response.json())
      .then((data) => {
        console.log('initial server response (generated user info)', data);
        this.userid.set(data.userId);
        this.username.set(data.userName);
        this.connectSignalR(data.userId);
      });
  }
  
  async connectSignalR(id: string) {
    if (!id) {
      console.warn("connectSignalR called without id");
      return;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.serverUrl}/notifications`, { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    // receive match info from server
    connection.on("MatchFound", (message) => {
      console.log("Message from SignalR hub: matched with opponent", message);
      if (message.playerWhite?.userId == id) this.opponentUsername.set(message.playerBlack?.userName);
      else this.opponentUsername.set(message.playerWhite?.userName);
    });
    // receive board state updates from server
    connection.on("BoardUpdated", (message) => {
      console.log("Message from SignalR hub: game board updated", message);
      //setSticksValue(message.sticksValue);
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
