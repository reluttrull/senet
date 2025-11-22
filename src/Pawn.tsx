//import { useEffect, useState } from 'react'
import { FaChessPawn } from 'react-icons/fa6'
import './App.css'

type PawnProps = {
  index: number
  canMove: boolean
  moveCallback: (index: number) => void
}

function Pawn({ index, canMove, moveCallback }: PawnProps) {

  return (
    <>
        <div className={canMove ? "can-move piece" : "piece"} onClick={() => moveCallback(index)}><FaChessPawn style={{fontSize:'3.5em'}} /></div>
    </>
  )
}

export default Pawn
