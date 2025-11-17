//import { useEffect, useState } from 'react'
import './App.css'

type PawnProps = {
  index: number
  canMove: boolean
  moveCallback: (index: number) => void
}

function Pawn({ index, canMove, moveCallback }: PawnProps) {

  return (
    <>
        <div className={canMove ? "can-move piece" : "piece"} onClick={() => moveCallback(index)}>P</div>
    </>
  )
}

export default Pawn
