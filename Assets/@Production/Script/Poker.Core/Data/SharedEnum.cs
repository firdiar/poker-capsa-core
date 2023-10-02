namespace Pker
{
    public enum CardNumber : byte //4 bit
    {
        None = 0,
        //LowAce = 1,
        //LowTwo = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        J = 11,
        Q = 12,
        K = 13,
        A = 14,
        Two = 15, //two is 'poker' the highest card
    }

    public enum CardSymbol : byte //2 bit
    {
        Diamond = 0,
        Club = 1, //berlian
        Heart = 2, //keriting
        Spade = 3, //hati
    }

    public enum PokerCombination : byte
    {
        None = 0,
        HighCard= 1, //nomor kartu
        Pair = 2, //sama 2x
        ThreeOfAKind = 3, //sama 3x

        Straight = 4, //berurutan beda simbol 5 kartu
        Flush = 5, //sama simbol 5x
        FullHouse = 6, //sama 3x sama 2x, ambil yg terbesar
        FourOfAKind_1 = 7 , //sama 4x + 1 Highcard
        StraightFlush = 8, //berurutan sama simbol 5 kartu
        RoyalStraightFlush = 9, //berurutan sama simbol 5 kartu, kartunya [A,K,Q,J,10]
    }

    public enum GameState
    { 
        Prepare,
        Play,
        EndGame,
        GameOver
    }

    public enum PlayerAction
    {
        Skip,
        Play
    }
}
