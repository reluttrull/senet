using SenetServer.Model;

namespace SenetServer.Tests
{
    public class BoardStateTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestOpeningBoardSetup()
        {
            User testUser1 = new User("234234239848", "testuser1");
            User testUser2 = new User("121230198302", "testuser2");
            GameState testGameState = new GameState(testUser1, testUser2);

            Assert.That(testGameState.BoardState.SticksValue, Is.AtLeast(1));
            Assert.That(testGameState.BoardState.SticksValue, Is.AtMost(5));
            Assert.That(testGameState.BoardState.WhitePositions, Is.EquivalentTo([0, 2, 4, 6, 8]));
            Assert.That(testGameState.BoardState.BlackPositions, Is.EquivalentTo([1, 3, 5, 7, 9]));
            Assert.That(testGameState.BoardState.MovablePositions.Count, Is.GreaterThan(0));
        }

        [Test]
        public void TestSwap()
        {
            User testUser1 = new User("234234239848", "testuser1");
            User testUser2 = new User("121230198302", "testuser2");
            GameState testGameState = new GameState(testUser1, testUser2);
            testGameState.BoardState.WhitePositions = [5];
            testGameState.BoardState.BlackPositions = [4];
            testGameState.BoardState.SticksValue = 1;
            testGameState.BoardState.IsWhiteTurn = false;
            testGameState.BoardState.MovePawn(4);

            Assert.That(testGameState.BoardState.WhitePositions, Is.EquivalentTo([4]));
            Assert.That(testGameState.BoardState.BlackPositions, Is.EquivalentTo([5]));
        }

        [Test]
        public void TestGuarded()
        {
            User testUser1 = new User("234234239848", "testuser1");
            User testUser2 = new User("121230198302", "testuser2");
            GameState testGameState = new GameState(testUser1, testUser2);
            testGameState.BoardState.WhitePositions = [5, 6];
            testGameState.BoardState.BlackPositions = [3, 4];
            testGameState.BoardState.SticksValue = 2;
            testGameState.BoardState.IsWhiteTurn = false;
            testGameState.BoardState.SetCanMove();

            Assert.That(testGameState.BoardState.MovablePositions.Count, Is.EqualTo(0));
            testGameState.BoardState.SticksValue = 3;
            testGameState.BoardState.SetCanMove();
            Assert.That(testGameState.BoardState.MovablePositions.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestBlockaded()
        {
            User testUser1 = new User("234234239848", "testuser1");
            User testUser2 = new User("121230198302", "testuser2");
            GameState testGameState = new GameState(testUser1, testUser2);
            testGameState.BoardState.WhitePositions = [5, 6, 7];
            testGameState.BoardState.BlackPositions = [3, 4];
            testGameState.BoardState.SticksValue = 5;
            testGameState.BoardState.IsWhiteTurn = false;
            testGameState.BoardState.SetCanMove();

            Assert.That(testGameState.BoardState.MovablePositions.Count, Is.EqualTo(0));
            testGameState.BoardState.WhitePositions = [6, 7];
            testGameState.BoardState.SetCanMove();
            Assert.That(testGameState.BoardState.MovablePositions.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestWarpToLocation()
        {
            User testUser1 = new User("234234239848", "testuser1");
            User testUser2 = new User("121230198302", "testuser2");
            GameState testGameState = new GameState(testUser1, testUser2);
            testGameState.BoardState.WhitePositions = [25];
            testGameState.BoardState.BlackPositions = [10, 13, 14];
            testGameState.BoardState.SticksValue = 1;
            testGameState.BoardState.IsWhiteTurn = true;
            testGameState.BoardState.SetCanMove();
            testGameState.BoardState.MovePawn(25);

            Assert.That(testGameState.BoardState.WhitePositions, Is.EquivalentTo([12]));
        }
    }
}
