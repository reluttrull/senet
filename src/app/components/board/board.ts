import { Component, inject, input } from '@angular/core';
import { ApiService } from '../../services/api-service';
import { utilities } from '../../shared/utilities';


@Component({
  selector: 'app-board',
  standalone: true,
  imports: [],
  templateUrl: './board.html',
  styleUrls: ['./board.css']
})
export class Board {
  userid = input.required<string>();
  isPlayerTurn = input();
  sticksValue = input();
  whitePawns = input.required<number[]>();
  blackPawns = input.required<number[]>();
  movablePawns = input.required<number[]>();
  isMultiplayer = input(true);

  apiService = inject(ApiService);

  movePawn(indexToMove:number) {
    if (!this.isPlayerTurn() || !this.movablePawns().includes(indexToMove)) {
      console.log(`can't move pawn at index ${indexToMove}.`);
      return;
    }
    console.log(`ready to move pawn at index ${indexToMove} by ${this.sticksValue()} spaces.`, this.isMultiplayer() ? 'is multiplayer' : 'is singleplayer');
    this.apiService.apiMovePawn(this.userid(), indexToMove, utilities.getPath(this.isMultiplayer()))
      .subscribe({
        next: () => {},
        error: (err) => {
          console.log('Error moving pawn', err);
          alert(`Error moving pawn: ${err.message}`);
        }
      });
  }
}
