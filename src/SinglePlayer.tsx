import { useEffect, useRef, useState } from 'react'
import { FaChessPawn, FaAnkh, FaGuitar, FaWater, FaDiceThree, FaDiceTwo } from 'react-icons/fa6'
import { GiEyeOfHorus } from 'react-icons/gi';
import Pawn from './Pawn'
import './App.css'

function SinglePlayer() {
  const [sticks, setSticks] = useState([0,0,0,0]);
  const board = new Array(30).fill(null);
  const isPlayerTurnRef = useRef(true);
  const [isPlayerTurn, setIsPlayerTurn] = useState(isPlayerTurnRef.current);
  // keep refs so async enemyTurn sees latest arrays
  const pawnsRef = useRef([0,2,4,6,8]);
  const enemyPawnsRef = useRef([1,3,5,7,9]);
  const [pawns, setPawns] = useState(pawnsRef.current); 
  const [enemyPawns, setEnemyPawns] = useState(enemyPawnsRef.current); 

  useEffect(() => { setIsPlayerTurn(isPlayerTurnRef.current);}, [isPlayerTurnRef.current]);
  //useEffect(() => { setPawns(pawnsRef.current); setEnemyPawns(enemyPawnsRef.current); }, [pawnsRef.current, enemyPawnsRef.current]);

  const sleep = (ms:number) => new Promise(resolve => setTimeout(resolve, ms));

  function movePiece(index:number) {
    if (!isPlayerTurnRef.current) return;
    let sticksValue:number = getSticksValue();
    let toLocation:number = index + sticksValue;
    if (!pawnCanMove(index, true)) return;
    pawnsRef.current = pawnsRef.current.map(pawnLocation => pawnLocation == index ? toLocation : pawnLocation);
    enemyPawnsRef.current = enemyPawnsRef.current.map(pawnLocation => pawnLocation == toLocation ? index : pawnLocation);
    setPawns(pawnsRef.current);
    setEnemyPawns(enemyPawnsRef.current);
    if ([1,4,5].includes(sticksValue)) rollSticks();
    else {
      isPlayerTurnRef.current = false;
      console.log("no more rolls, now it's opponent's turn", isPlayerTurnRef.current);
      if (!isPlayerTurnRef.current) enemyTurn();
    }
  };
  
  function moveEnemyPiece(index:number) {
    let sticksValue:number = getSticksValue();
    let toLocation:number = index + sticksValue;
    console.log(`moving from ${index}: ${sticksValue} squares to ${toLocation}`);
    enemyPawnsRef.current = enemyPawnsRef.current.map(pawnLocation => pawnLocation == index ? toLocation : pawnLocation);
    setEnemyPawns(enemyPawnsRef.current);
    if (pawnsRef.current.includes(toLocation)) console.log(`capturing pawn at ${toLocation}`);
    pawnsRef.current = pawnsRef.current.map(pawnLocation => pawnLocation == toLocation ? index : pawnLocation);
    setPawns(pawnsRef.current);
    if ([1,4,5].includes(sticksValue)) rollSticks();
    else {
      isPlayerTurnRef.current = true;
      console.log("no more rolls, now it's player's turn", isPlayerTurnRef.current);
      if (!isPlayerTurnRef.current) enemyTurn();
    }
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
    console.log(sticks);
    return 0;
  }

  const isEnemyGuarded = (index:number, isPlayer:boolean = true) => {
    let otherPieces = isPlayer ? enemyPawnsRef.current : pawnsRef.current;
    console.log(`checking other player isn't guarded at ${index}`, otherPieces);
    if (!otherPieces.includes(index)) return false;
    if ((otherPieces.includes(index + 1) && index < 29) || otherPieces.includes(index - 1)) return true;
    return false;
  }

  const pawnCanMove = (index:number, isPlayer:boolean = true) => {
    let myPieces = isPlayer ? pawnsRef.current : enemyPawnsRef.current;
    if (!isPlayer) console.log("index at canmove", index);
    if (!isPlayer) console.log("mypieces at canmove", myPieces);
    if (isPlayer && !isPlayerTurnRef.current) return false;
    let toLocation:number = index + getSticksValue();
    if (isEnemyGuarded(toLocation, isPlayer)) return false; // if guarded
    if (toLocation < 30 && myPieces.includes(toLocation)) return false; // if same color
    if (index == 25 || index == 29 && toLocation > 29) return true; // if home free
    if (index != 25 && toLocation > 25 && toLocation < 29) return false; // if did not pass go
    if (index == 27 && toLocation != 30) return false; // need exactly 3 here
    if (index == 28 && toLocation != 30) return false; // need exactly 2 here
    return true;
  }

  function rollSticks(){
    let newSticks:number[] = [];
    for (let i: number = 0; i < 4; i++)
    {
      newSticks[i] = Math.floor(Math.random() * 2);
    }
    setSticks(newSticks);
  }

  async function enemyTurn(){
    console.log("got here");
    while (!isPlayerTurnRef.current)
    {
      rollSticks();
      console.log("sticks", getSticksValue());
      // get all possible moves
      let movableEnemyPawns = enemyPawnsRef.current.filter(pawn => pawnCanMove(pawn, false));
      console.log("movable pawns", movableEnemyPawns);
      await sleep(1000);
      if (movableEnemyPawns.length == 0) {
        isPlayerTurnRef.current = true;
        break;
      }
      // if any, pick random
      let pickIndex = Math.floor(Math.random() * movableEnemyPawns.length);
      console.log("picked index...", pickIndex);
      // make move      
      moveEnemyPiece(movableEnemyPawns[pickIndex]);
    }
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
                    {!pawns.includes(index) && !enemyPawns.includes(index) && index == 14 
                      && <FaAnkh className="house" />}
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
                    {!pawns.includes(index) && !enemyPawns.includes(index) && index == 25 
                      && <FaGuitar className="house" />}
                    {!pawns.includes(index) && !enemyPawns.includes(index) && index == 26 
                      && <FaWater className="house" />}
                    {!pawns.includes(index) && !enemyPawns.includes(index) && index == 27 
                      && <FaDiceThree className="house" />}
                    {!pawns.includes(index) && !enemyPawns.includes(index) && index == 28 
                      && <FaDiceTwo className="house" />}
                    {!pawns.includes(index) && !enemyPawns.includes(index) && index == 29 
                      && <GiEyeOfHorus className="house" />}
                    {pawns.includes(index) && 
                      <Pawn index={index} canMove={pawnCanMove(index)} moveCallback={movePiece} />}
                    {enemyPawns.includes(index) && 
                      <div className="enemy-piece">P</div>}
                  </div>)
                })}
            </div>
        </div>
        <div>{isPlayerTurn ? <span>You can </span> : <span>Opponent can </span>}move {getSticksValue()} spaces</div>
        <div>{pawns.map(pawnLocation => {if (pawnLocation > 29) return (<FaChessPawn style={{color:'white'}} />)})}</div>
        <div>{enemyPawns.map(pawnLocation => {if (pawnLocation > 29) return (<FaChessPawn style={{color:'black'}} />)})}</div>
    </>
  )
}

export default SinglePlayer
