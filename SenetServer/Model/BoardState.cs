using Microsoft.AspNetCore.Http.HttpResults;

namespace SenetServer.Model
{
    public class BoardState
    {
        public List<int> WhitePositions { get; set; }
        public List<int> BlackPositions { get; set; }
        public List<int> MovablePositions { get; set; }
        public List<int> Sticks { get; set; } = new List<int>();
        public int SticksValue { get; set; } = 0;
        public bool IsWhiteTurn { get; set; } = true;
        private List<int> ReRollValues { get; set; } = [0, 1, 4, 5];
        public BoardState()
        {
            WhitePositions = new List<int>() { 0, 2, 4, 6, 8 };
            BlackPositions = new List<int>() { 1, 3, 5, 7, 9 };
            RollSticks();
        }
        public void SetCanMove()
        {
            MovablePositions = (IsWhiteTurn ? WhitePositions : BlackPositions)
                .Where(pawn => PawnCanMove(pawn))
                .ToList();
        }
        public void RollSticks()
        {
            if (!ReRollValues.Contains(SticksValue)) IsWhiteTurn = !IsWhiteTurn;
            Sticks.Clear();
            Random random = new Random();
            for (int i = 0; i < 4; i++)
            {
                Sticks.Add(random.Next(2));
            }
            SticksValue = GetSticksValue();
            SetCanMove();
        }

        public bool MovePawn(int position)
        {
            int index = (IsWhiteTurn ? WhitePositions : BlackPositions).FindIndex(pawn => pawn == position);
            if (index < 0) return false; // could not find pawn
            (IsWhiteTurn ? WhitePositions : BlackPositions)[index] = position + SticksValue;
            return true;
        }

        private bool PawnCanMove(int location)
        {
            List<int> sameColor = IsWhiteTurn ? WhitePositions : BlackPositions;
            List<int> differentColor = IsWhiteTurn ? BlackPositions : WhitePositions;
            List<int> safeSquares = [14, 25, 27, 28];
            int targetLocation = location + SticksValue;

            if (IsEnemyGuarded(differentColor, targetLocation)) return false; // can't swap guarded pawns
            if (safeSquares.Contains(targetLocation)) return false; // can't capture on safe squares
            if (IsEnemyBlockaded(differentColor, targetLocation)) return false; // can't pass blockade
            if (sameColor.Contains(targetLocation) && targetLocation < 30) return false; // can't oust own pawn
            if (location == 25 || (location == 29 && targetLocation > 29)) return true; // home free from 25
            if (targetLocation > 25 && targetLocation < 30 && location != 25) return false; // did not pass go
            if ((location == 27 || location == 28) && targetLocation != 30) return false; // need to roll exactly 3 and 2, respectively
            return true; // otherwise, all good
        }

        private bool IsEnemyGuarded(List<int> enemyPieces, int targetIndex)
        {
            if (!enemyPieces.Contains(targetIndex)) return false;
            // two or more pieces next to each other can't be captured
            // ...just double-check we're not including pieces past the finish line
            if ((enemyPieces.Contains(targetIndex + 1) && targetIndex < 29) 
                || enemyPieces.Contains(targetIndex - 1)) return true;
            return false;
        }

        private bool IsEnemyBlockaded(List<int> enemyPieces, int targetIndex)
        {
            if (!enemyPieces.Contains(targetIndex)) return false;
            // from start to end index, blockade can either look like:
            //  sxxx_e  or s_xxxe
            if (SticksValue > 3
                && enemyPieces.Contains(targetIndex - 2)
                && enemyPieces.Contains(targetIndex - 3)
                && (enemyPieces.Contains(targetIndex - 1) || enemyPieces.Contains(targetIndex - 4))) return true;
            return false;

        }

        private int GetSticksValue()
        {
            switch (Sticks.Count(n => n == 1))
            {
                case 0: return 5;
                case 1: return 1;
                case 2: return 2;
                case 3: return 3;
                case 4: return 4;
            }
            return 0;
        }
    }
}
