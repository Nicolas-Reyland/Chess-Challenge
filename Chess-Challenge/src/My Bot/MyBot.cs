#define PIECE_POSITION_VALUES
#define ADAPTATIVE_DEPTH

using ChessChallenge.API;
using System;
#if PIECE_POSITION_VALUES
using System.Collections.Generic;
#endif
using System.Linq;
using System.Collections.Concurrent;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    readonly int[] pieceValues = { 0, 100, 320, 330, 500, 900, 20000 };
#if PIECE_POSITION_VALUES
    readonly Dictionary<PieceType, int[,]> middleGamePiecePositions = new()
    {
        {
            PieceType.Pawn, new int[8,8] {
                {  0,  0,  0,  0,  0,  0,  0,  0, },
                { 50, 50, 50, 50, 50, 50, 50, 50, },
                { 10, 10, 20, 30, 30, 20, 10, 10, },
                {  5,  5, 10, 25, 25, 10,  5,  5, },
                {  0,  0,  0, 20, 20,  0,  0,  0, },
                {  5, -5,-10,  0,  0,-10, -5,  5, },
                {  5, 10, 10,-20,-20, 10, 10,  5, },
                {  0,  0,  0,  0,  0,  0,  0,  0, },
            }
        },
        {
            PieceType.Knight, new int[8,8] {
                { -50,-40,-30,-30,-30,-30,-40,-50, },
                { -40,-20,  0,  0,  0,  0,-20,-40, },
                { -30,  0, 10, 15, 15, 10,  0,-30, },
                { -30,  5, 15, 20, 20, 15,  5,-30, },
                { -30,  0, 15, 20, 20, 15,  0,-30, },
                { -30,  5, 10, 15, 15, 10,  5,-30, },
                { -40,-20,  0,  5,  5,  0,-20,-40, },
                { -50,-40,-30,-30,-30,-30,-40,-50, },
            }
        },
        {
            PieceType.Bishop, new int[8,8] {
                { -20,-10,-10,-10,-10,-10,-10,-20, },
                { -10,  0,  0,  0,  0,  0,  0,-10, },
                { -10,  0,  5, 10, 10,  5,  0,-10, },
                { -10,  5,  5, 10, 10,  5,  5,-10, },
                { -10,  0, 10, 10, 10, 10,  0,-10, },
                { -10, 10, 10, 10, 10, 10, 10,-10, },
                { -10,  5,  0,  0,  0,  0,  5,-10, },
                { -20,-10,-10,-10,-10,-10,-10,-20, },
            }
        },
        {
            PieceType.Rook, new int[8,8] {
                {   0,  0,  0,  0,  0,  0,  0,  0, },
                {   5, 10, 10, 10, 10, 10, 10,  5, },
                {  -5,  0,  0,  0,  0,  0,  0, -5, },
                {  -5,  0,  0,  0,  0,  0,  0, -5, },
                {  -5,  0,  0,  0,  0,  0,  0, -5, },
                {  -5,  0,  0,  0,  0,  0,  0, -5, },
                {  -5,  0,  0,  0,  0,  0,  0, -5, },
                {   0,  0,  0,  5,  5,  0,  0,  0, },
            }
        },
        {
            PieceType.Queen, new int[8,8] {
                { -20,-10,-10, -5, -5,-10,-10,-20, },
                { -10,  0,  0,  0,  0,  0,  0,-10, },
                { -10,  0,  5,  5,  5,  5,  0,-10, },
                {  -5,  0,  5,  5,  5,  5,  0, -5, },
                {   0,  0,  5,  5,  5,  5,  0, -5, },
                { -10,  5,  5,  5,  5,  5,  0,-10, },
                { -10,  0,  5,  0,  0,  0,  0,-10, },
                { -20,-10,-10, -5, -5,-10,-10,-20, },
            }
        },
        {
            // this "shelter" table is not good for the end-game
            PieceType.King, new int[8,8] {
                { -30,-40,-40,-50,-50,-40,-40,-30, },
                { -30,-40,-40,-50,-50,-40,-40,-30, },
                { -30,-40,-40,-50,-50,-40,-40,-30, },
                { -30,-40,-40,-50,-50,-40,-40,-30, },
                { -20,-30,-30,-40,-40,-30,-30,-20, },
                { -10,-20,-20,-20,-20,-20,-20,-10, },
                {  20, 20,  0,  0,  0,  0, 20, 20, },
                {  20, 30, 10,  0,  0, 10, 30, 20, },
            }
        },
    };
    readonly Dictionary<PieceType, int[,]> endGamePiecePositions = new()
    {
        {
            PieceType.Pawn, new int[8, 8] {
                { 90, 90, 90, 90, 90, 90, 90, 90, },
                { 70, 70, 70, 70, 70, 70, 70, 70, },
                { 20, 20, 20, 20, 20, 20, 20, 20, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
            }
        },
        {
            PieceType.Knight, new int[8, 8] {
                { -5,-5, -5, -5, -5, -5,-5, -5, },
                { -5, 0,  0,  0,  0,  0, 0, -5, },
                { -5, 0,  0, 10, 10,  0, 0, -5, },
                { -5, 0, 10, 20, 20, 10, 0, -5, },
                { -5, 0, 10, 20, 20, 10, 0, -5, },
                { -5, 0,  0, 10, 10,  0, 0, -5, },
                { -5, 0,  0,  0,  0,  0, 0, -5, },
                { -5,-5, -5, -5, -5, -5,-5, -5, },
            }
        },
        {
            PieceType.Bishop, new int[8, 8] {
                { -5,-5, -5, -5, -5, -5,-5, -5, },
                { -5, 5,  0,  0,  0,  0, 5, -5, },
                { -5, 0, 10, 10, 10, 10, 0, -5, },
                { -5, 0, 10, 20, 20, 10, 0, -5, },
                { -5, 0, 10, 20, 20, 10, 0, -5, },
                { -5, 0, 10, 10, 10, 10, 0, -5, },
                { -5, 5,  0,  0,  0,  0, 5, -5, },
                { -5,-5, -5, -5, -5, -5,-5, -5, },
            }
        },
        {
            PieceType.Rook, new int[8, 8] {
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { -15, 0, 0, 0, 0, 0, 0, -15, },
            }
        },
        {
            PieceType.Queen, new int[8, 8] {
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, },
            }
        },
        {
            PieceType.King, new int[8, 8] {
                { -50,-40,-30,-20,-20,-30,-40,-50, },
                { -30,-20,-10,  0,  0,-10,-20,-30, },
                { -30,-10, 20, 30, 30, 20,-10,-30, },
                { -30,-10, 30, 40, 40, 30,-10,-30, },
                { -30,-10, 30, 40, 40, 30,-10,-30, },
                { -30,-10, 20, 30, 30, 20,-10,-30, },
                { -30,-30,  0,  0,  0,  0,-30,-30, },
                { -50,-30,-30,-30,-30,-30,-30,-50, },
            }
        },
    };
#endif

    private ConcurrentDictionary<ulong, int> boardScoreTable = new();
    private Move moveToPlay;
    private int searchDepth = 6;
    private bool playAsBlack = true,
                 inEndGame = false,
                 stopThinking = false;

    public Move Think(Board board, Timer timer)
    {
        // this variable is used when a mate in one is found, or when we just want to stop thinking
        stopThinking = false;
        // are we the black or white player ?
        playAsBlack = !board.IsWhiteToMove;
        // adaptative search depth
        searchDepth = ChooseSearchDepth(board);
        // game stage
        inEndGame = ChooseGameStrategy(board);
        // this will be set by the negamax function
        moveToPlay = Move.NullMove;
        // search for the best move
        Negamax(board, -int.MaxValue, int.MaxValue, searchDepth);
        // Minimax(board, searchDepth - 1, int.MinValue, int.MaxValue, board.IsWhiteToMove, true);
        return moveToPlay;
    }

    private int ChooseSearchDepth(Board board) {
#if ADAPTATIVE_DEPTH
        int nbPieces = NbPiecesOnBoard(board);
        if (nbPieces > 20)
            return 4;
        if (nbPieces > 10)
            return 5;
        else
            return 6;
#else
        return 4;
#endif
    }

    private static int NbPiecesOnBoard(Board board)
    {
        int nbPieces = 0;
        foreach (PieceList pieces in board.GetAllPieceLists())
            nbPieces += pieces.Count;
        return nbPieces;
    }

    /// <summary>
    /// Choses the middle or late game strategy. True means Late game. False means Middle game.
    /// </summary>
    /// <param name="board">Board to analyze</param>
    /// <returns>Strategy</returns>
    private bool ChooseGameStrategy(Board board)
    {
        bool endGame = false;
        int nbPieces = NbPiecesOnBoard(board);
        if (nbPieces > 25)
        {
            endGame = false;
            goto ReturnEndGame;
        }
        if (nbPieces < 10)
        {
            endGame = true;
            goto ReturnEndGame;
        }
        PieceList whiteQueens = board.GetPieceList(PieceType.Queen, true);
        PieceList blackQueens = board.GetPieceList(PieceType.Queen, false);
        if (!whiteQueens.Any() && !blackQueens.Any())
        {
            endGame = true;
            goto ReturnEndGame;
        }

        ReturnEndGame:
        if (inEndGame != endGame)
            Console.WriteLine("Changing strategy from {0} to {1}", inEndGame ? "EG" : "MG", endGame ? "EG" : "MG");
        return endGame;
    }

#if PIECE_POSITION_VALUES
    private int PiecePositionValue(Piece piece, ref Dictionary<PieceType, int[,]> table)
    {
        int[,] values = table[piece.PieceType];
        int pieceY = piece.Square.Index / 8;
        int pieceX = piece.Square.Index % 8;
        if (piece.IsWhite)
            pieceY = 7 - pieceY;
        return values[pieceY, pieceX];
    }
#endif

    public int Evaluate(Board board)
    {
        int score;
        ulong boardHash = board.ZobristKey;
        if (boardScoreTable.ContainsKey(boardHash))
            score = boardScoreTable[boardHash];
        else
        {
            score = _Evaluate(board);
            boardScoreTable[boardHash] = score;
        }
        return playAsBlack ? -score : score;
    }

    private int _Evaluate(Board board)
    {
        if (board.IsDraw())
            return 0;
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? -99000 : 99000;

        int boardValue = 0;
        PieceList[] allPieces = board.GetAllPieceLists();
        foreach (PieceList pieces in allPieces) {
            int value = pieces.Count * pieceValues[(int)pieces.TypeOfPieceInList];
#if PIECE_POSITION_VALUES
            Dictionary<PieceType, int[,]> piecePositionTables;
            if (!inEndGame)
                piecePositionTables = middleGamePiecePositions;
            else
                piecePositionTables = endGamePiecePositions;
            foreach (Piece piece in pieces)
            {
                int positionValue = PiecePositionValue(piece, ref piecePositionTables);
#if VERBOSE
                if (positionValue != 0)
                    Console.WriteLine("Pawn position bonus/malus: {0}", positionValue);
#endif
                value += positionValue;
            }
#endif
            boardValue += pieces.IsWhitePieceList ? value : -value;
        }

        return boardValue;
    }

    /// <summary>
    /// NegaMax search
    /// </summary>
    /// <param name="board">Chess Board</param>
    /// <param name="alpha">Alpha for alpha-beta pruning</param>
    /// <param name="beta">Beta for alpha-beta pruning</param>
    /// <param name="depth">Depth (inverted : starting with searchDepth)</param>
    /// <returns>Evaluation of best move, starting from this board</returns>
    int Negamax(Board board, int alpha, int beta, int depth) {
        if (stopThinking)
            return 0;

        int color = board.IsWhiteToMove ? -1 : 1;
        int value = -int.MaxValue;
        bool isRoot = depth == searchDepth;

        if (depth == 0 || !board.GetLegalMoves().Any())
        {
            value = color * Evaluate(board);
            if (board.IsInCheckmate())
                value += color * depth * 100000;
            return color * value;
        }

        Move bestMove = Move.NullMove;
        /*
        Span<Move> moves = stackalloc Move[218];
        board.GetLegalMovesNonAlloc(ref moves);
        */
        Move[] moves = board.GetLegalMoves();

        foreach (Move move in moves) {
            board.MakeMove(move);

            // instantly chose move if mate in one
            if (isRoot && board.IsInCheckmate())
            {
                board.UndoMove(move);
                moveToPlay = move;
                return 0;
            }
            /*
            // don't even consider the move if it is represents a draw
            if (isRoot && board.IsDraw())
                continue;
            */

            int score = -Negamax(board, -beta, -alpha, depth - 1);
            board.UndoMove(move);
            if (score > value) {
                value = score;
#if VERBOSE
                if (value > 5999000)
                    Console.WriteLine("Chosing move for mate with a score of {0}", value);
#endif
                bestMove = move;
            }
            alpha = Math.Max(alpha, value);
            if (alpha >= beta /* || TimedOut() */ )
                break;
        }
        if (depth == searchDepth)
            moveToPlay = bestMove;
        return value;
    }

    private int Minimax(Board board, int depth, int alpha, int beta, bool isMaximizing, bool isRoot)
    {
        if (depth == 0)
            return Evaluate(board);

        if (isMaximizing)
        {
            int maxEval = int.MinValue;
            var legalMoves = board.GetLegalMoves();
            foreach (var move in legalMoves)
            {
                board.MakeMove(move);
                int eval = Minimax(board, depth - 1, alpha, beta, !isMaximizing, false);
                board.UndoMove(move);
                if (eval > maxEval)
                {
                    maxEval = eval;
                    if (isRoot)
                       moveToPlay = move;
                }
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                    break;
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            var legalMoves = board.GetLegalMoves();
            foreach (var move in legalMoves)
            {
                board.MakeMove(move);
                int eval = Minimax(board, depth - 1, alpha, beta, !isMaximizing, false);
                board.UndoMove(move);
                if (eval < minEval)
                {
                    minEval = eval;
                    if (isRoot)
                        moveToPlay = move;
                }
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                    break;
            }
            return minEval;
        }
    }

}
