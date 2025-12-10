import { Component, inject, input } from '@angular/core';
import { ApiService } from '../../services/api-service';


@Component({
  selector: 'app-board',
  standalone: true,
  imports: [],
  templateUrl: './board.html',
  styleUrls: ['./board.css']
})
export class Board {
  isPlayerTurn = input();
  sticksValue = input();
  whitePawns = input.required<number[]>();
  blackPawns = input.required<number[]>();
  movablePawns = input.required<number[]>();

  apiService = inject(ApiService);

  movePawn(indexToMove:number) {
    console.log(`ready to move pawn at index ${indexToMove} by ${this.sticksValue()} spaces.`);
    this.apiService.apiMovePawn(indexToMove)
      .subscribe((result) => {
        
      })
  }
}
