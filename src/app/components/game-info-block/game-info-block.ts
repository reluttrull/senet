import { Component, input } from '@angular/core';
import { CurrentScorePipe } from '../../pipes/current-score-pipe';

@Component({
  selector: 'app-game-info-block',
  imports: [CurrentScorePipe],
  templateUrl: './game-info-block.html',
  styleUrl: './game-info-block.css'
})
export class GameInfoBlock {
  username = input('');
  opponentUsername = input('');
  isPlayerWhite = input();
  whitePawns = input([]);
  blackPawns = input([]);
}
