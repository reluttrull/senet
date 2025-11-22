import { useState } from 'react';
import { FaUpRightFromSquare } from 'react-icons/fa6'
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
      <a href="https://royalur.net/senet#how-to-play" target="_blank" rel="noopener noreferrer">Game rules <FaUpRightFromSquare /></a>
      {!gameOver && <SinglePlayer gameOverCallback={handleGameOver} />}
      {gameOver && playerWonGame && <h1>You won!</h1>}
      {gameOver && !playerWonGame && <h1>You lost...</h1>}
    </>
  )
}

export default App
