import { useEffect, useState } from 'react'
import './App.css'

function SinglePlayer() {
  // for now, one pawn in the first spot
  const [sticks, setSticks] = useState([0,0,0,0,0]);
  const [pawns, setPawns] = useState([0,2,4]); 
  const [enemyPawns, setEnemyPawns] = useState([1,3,5]); 
  const board = new Array(30).fill(null);
  const movePiece = (index:number) => {
    setPawns(pawns.map(pawnLocation => {
        if (pawnLocation == index) return pawnLocation + getSticksValue();
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
                        <div className="piece" onClick={() => movePiece(index)}>P</div>}
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
                      <div className="piece" onClick={() => movePiece(index)}>P</div>
                    }
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
                        <div className="piece" onClick={() => movePiece(index)}>P</div>}
                      {enemyPawns.includes(index) && 
                        <div className="enemy-piece">P</div>}
                  </div>)
                })}
            </div>
        </div>
        <div>move {getSticksValue()} spaces</div>
        <div>{pawns.map(pawnLocation => {if (pawnLocation > 29) return (<span>x</span>)})}</div>
    </>
  )
}

export default SinglePlayer
