#define PIECE_POSITION_VALUES

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
    readonly Dictionary<PieceType, int[,]> piecePositions = new()
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
#endif

    private ConcurrentDictionary<ulong, int> boardScoreTable = new();
    private Move moveToPlay;
    private int searchDepth = 6;
    private bool playAsBlack = true,
                 inEndGame = false;

    public Move Think(Board board, Timer timer)
    {
        playAsBlack = !board.IsWhiteToMove;
        searchDepth = ChooseSearchDepth(board);
        inEndGame = ReachedEndGame(board);
        moveToPlay = Move.NullMove;
        Negamax(board, -int.MaxValue, int.MaxValue, searchDepth);
        // Minimax(board, searchDepth - 1, int.MinValue, int.MaxValue, board.IsWhiteToMove, true);
        return moveToPlay;
    }

    private int ChooseSearchDepth(Board board) {
        int nbPieces = NbPiecesOnBoard(board);
        if (nbPieces > 20)
            return 4;
        if (nbPieces > 10)
            return 5;
        else
            return 6;
    }

    private static int NbPiecesOnBoard(Board board)
    {
        int nbPieces = 0;
        foreach (PieceList pieces in board.GetAllPieceLists())
            nbPieces += pieces.Count;
        return nbPieces;
    }

    private static bool ReachedEndGame(Board board)
    {
        int nbPieces = NbPiecesOnBoard(board);
        if (nbPieces > 25)
            return false;
        if (nbPieces < 10)
            return true;
        PieceList whiteQueens = board.GetPieceList(PieceType.Queen, true);
        PieceList blackQueens = board.GetPieceList(PieceType.Queen, false);
        if (!whiteQueens.Any() && !blackQueens.Any())
            return true;
        return false;
    }

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
        if (board.IsDraw() || board.IsInStalemate())
            return 0;
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? -5999000 : 5999000;

        int boardValue = 0;
        PieceList[] allPieces = board.GetAllPieceLists();
        foreach (PieceList pieces in allPieces) {
            int value = pieces.Count * pieceValues[(int)pieces.TypeOfPieceInList];
#if PIECE_POSITION_VALUES
            if (!inEndGame)
            {
                int[,] piecePositionValues = piecePositions[pieces.TypeOfPieceInList];
                foreach (Piece piece in pieces)
                {
                    int positionValue = PiecePositionValue(piece);
#if VERBOSE
                if (positionValue != 0)
                    Console.WriteLine("Pawn position bonus/malus: {0}", positionValue);
#endif
                    value += positionValue;
                }
            }
#endif
            boardValue += pieces.IsWhitePieceList ? value : -value;
        }

        return boardValue;
    }

#if PIECE_POSITION_VALUES
    private int PiecePositionValue(Piece piece)
    {
        int[,] values = piecePositions[piece.PieceType];
        int pieceY = piece.Square.Index / 8;
        int pieceX = piece.Square.Index % 8;
        if (piece.IsWhite)
            pieceY = 7 - pieceY;
        return values[pieceY, pieceX];
    }
#endif

    /// <summary>
    /// NegaMax search
    /// </summary>
    /// <param name="board">Chess Board</param>
    /// <param name="alpha">Alpha for alpha-beta pruning</param>
    /// <param name="beta">Beta for alpha-beta pruning</param>
    /// <param name="depth">Depth (inverted : starting with searchDepth)</param>
    /// <returns>Evaluation of best move, starting from this board</returns>
    int Negamax(Board board, int alpha, int beta, int depth) {
        int color = board.IsWhiteToMove ? -1 : 1;
        int value = -int.MaxValue;

        if (depth == 0 || !board.GetLegalMoves().Any())
        {
            value = color * Evaluate(board);
            if (board.IsInCheckmate())
                value += color * depth * 100000;
            return color * value;
        }

        Move bestMove = Move.NullMove;
        Move[] moves = board.GetLegalMoves();

        foreach (Move move in moves) {
            board.MakeMove(move);
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
