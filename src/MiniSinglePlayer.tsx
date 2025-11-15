import { useState } from 'react'
import './App.css'

function MiniSinglePlayer() {
  // for now, one pawn in the first spot
  const [pawns, setPawns] = useState([0]);
  const board = new Array(5).fill(null);
  const movePiece = (index:number, amount:number) => {
    setPawns(pawns.map(pawnLocation => {
        if (pawnLocation == index) return pawnLocation + amount;
        else return pawnLocation;
    }));
  };

  return (
    <>
        <div className="game-container">
            <div className="game-board">
                {board.map((_, index) => (
                <div key={index} className="square">
                    {pawns.includes(index) && 
                        <div className="piece" onClick={() => movePiece(index, 1)}>P</div>}
                </div>
                ))}
            </div>
        </div>
    </>
  )
}

export default MiniSinglePlayer
