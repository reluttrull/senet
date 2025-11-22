import { useState } from 'react';
import './App.css'
import SinglePlayer from './SinglePlayer.tsx'

function App() {
  const [gameOver, setGameOver] = useState(false);
  const [playerWonGame, setPlayerWonGame] = useState(false);
  function handleGameOver(playerWon:boolean) {
    setPlayerWonGame(playerWon);
    setGameOver(true);
  }

  return (
    <>
      {!gameOver && <SinglePlayer gameOverCallback={handleGameOver} />}
      {gameOver && playerWonGame && <h1>You won!</h1>}
      {gameOver && !playerWonGame && <h1>You lost...</h1>}
    </>
  )
}

export default App
