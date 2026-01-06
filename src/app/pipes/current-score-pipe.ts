import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'currentScore'
})
export class CurrentScorePipe implements PipeTransform {

  transform(inputArray:number[]): number {
    return inputArray.filter(pawn => pawn >= 30).length;
  }

}
