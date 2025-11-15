import { useState } from 'react'
import './App.css'

function SinglePlayer() {
  // for now, one pawn in the first spot
  const [sticks, rollSticks] = useState(Math.ceil(Math.random() * 5));
  const [pawns, setPawns] = useState([0,1]); 
  const board = new Array(30).fill(null);
  const movePiece = (index:number) => {
    setPawns(pawns.map(pawnLocation => {
        if (pawnLocation == index) return pawnLocation + sticks;
        else return pawnLocation;
    }));
    rollSticks(Math.ceil(Math.random() * 5));
  };

  return (
    <>
        <div className="game-container">
            <div className="game-row">
                {board.map((_, index) => { 
                if (index < 10) return (
                  <div key={index} className="square">
                      {pawns.includes(index) && 
                          <div className="piece" onClick={() => movePiece(index)}>P</div>}
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
                    {pawns.includes(index) && (
                      <div className="piece" onClick={() => movePiece(index)}>P</div>
                    )}
                  </div>
                ))}
            </div>
            <div className="game-row">
                {board.map((_, index) => { 
                if (index >= 20) return (
                  <div key={index} className="square">
                      {pawns.includes(index) && 
                          <div className="piece" onClick={() => movePiece(index)}>P</div>}
                  </div>)
                })}
            </div>
        </div>
        <div>move {sticks} spaces</div>
        <div>{pawns.map(pawnLocation => {if (pawnLocation > 29) return (<span>x</span>)})}</div>
    </>
  )
}

export default SinglePlayer
