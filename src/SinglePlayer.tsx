import { useEffect, useState } from 'react'
import { FaChessPawn } from 'react-icons/fa6'
import Pawn from './Pawn'
import './App.css'

function SinglePlayer() {
  // for now, one pawn in the first spot
  const [sticks, setSticks] = useState([0,0,0,0,0]);
  const [pawns, setPawns] = useState([0,2,4,6,8]); 
  const [enemyPawns, setEnemyPawns] = useState([1,3,5,7,9]); 
  const board = new Array(30).fill(null);
  function movePiece(index:number) {
    let toLocation:number = index + getSticksValue();
    setPawns(pawns.map(pawnLocation => {
        if (pawnLocation == index) return toLocation;
        else return pawnLocation;
    }));
    setEnemyPawns(enemyPawns.map(pawnLocation => {
      if (pawnLocation == toLocation) return index;
      else return pawnLocation;
    }));
    rollSticks();
  };

  const getSticksValue = () => {
    switch (sticks.filter(s => s == 1).length) {
      case 1: 
        return 1;
      case 2:
        return 2;
      case 3:
        return 3;
      case 4:
        return 4;
      case 0:
        return 5;
    }
    return 0;
  }

  const isEnemyGuarded = (index:number) => {
    if (!enemyPawns.includes(index)) return false;
    if ((enemyPawns.includes(index + 1) && index < 29) || enemyPawns.includes(index - 1)) return true;
    return false;
  }

  const pawnCanMove = (index:number) => {
    let toLocation:number = index + getSticksValue();
    if (isEnemyGuarded(toLocation)) return false; // if guarded
    if (pawns.includes(toLocation)) return false; // if same color
    if (index == 25 || index == 29 && toLocation > 29) return true; // if home free
    if (index != 25 && toLocation > 25 && toLocation < 29) return false; // if did not pass go
    if (index == 27 && toLocation != 30) return false; // need exactly 3 here
    if (index == 28 && toLocation != 30) return false; // need exactly 2 here
    return true;
  }

  function rollSticks(){
    let newSticks:number[] = [];
    for (let i: number = 0; i < 5; i++)
    {
      newSticks[i] = Math.floor(Math.random() * 2);
    }
    setSticks(newSticks);
  }

  useEffect(() => {
    rollSticks();
  }, [])

  return (
    <>
        <div className="game-container">
            <div className="game-row">
                {board.map((_, index) => { 
                if (index < 10) return (
                  <div key={index} className="square">
                      {pawns.includes(index) && 
                        <Pawn index={index} canMove={pawnCanMove(index)} moveCallback={movePiece} />}
                      {enemyPawns.includes(index) && 
                        <div className="enemy-piece">P</div>}
                  </div>)
                })}
            </div>
            <div className="game-row">
              {board
                .map((_, index) => index >= 10 && index < 20 ? index : null)
                .filter(index => index != null)
                .reverse()
                .map((index) => (
                  <div key={index} className="square">
                      {pawns.includes(index) && 
                        <Pawn index={index} canMove={pawnCanMove(index)} moveCallback={movePiece} />}
                    {enemyPawns.includes(index) && 
                      <div className="enemy-piece">P</div>}
                  </div>
                ))}
            </div>
            <div className="game-row">
                {board.map((_, index) => { 
                if (index >= 20) return (
                  <div key={index} className="square">
                      {pawns.includes(index) && 
                        <Pawn index={index} canMove={pawnCanMove(index)} moveCallback={movePiece} />}
                      {enemyPawns.includes(index) && 
                        <div className="enemy-piece">P</div>}
                  </div>)
                })}
            </div>
        </div>
        <div>move {getSticksValue()} spaces</div>
        <div>{pawns.map(pawnLocation => {if (pawnLocation > 29) return (<FaChessPawn style={{color:'white'}} />)})}</div>
        <div>{enemyPawns.map(pawnLocation => {if (pawnLocation > 29) return (<FaChessPawn style={{color:'black'}} />)})}</div>
    </>
  )
}

export default SinglePlayer
