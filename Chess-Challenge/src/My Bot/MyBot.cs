using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    readonly int[] pieceValues = { 0, 100, 300, 350, 500, 900, 0 };
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
    private ConcurrentDictionary<ulong, int> boardScoreTable = new();
    private Move moveToPlay;
    private int searchDepth = 6;

    public Move Think(Board board, Timer timer)
    {
        searchDepth = ChooseSearchDepth(board);
        moveToPlay = Move.NullMove;
        int score = Search(board, -int.MaxValue, int.MaxValue, searchDepth);
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

    public int Evaluate(Board board)
    {
        ulong boardHash = board.ZobristKey;
        if (boardScoreTable.ContainsKey(boardHash))
            return boardScoreTable[boardHash];

        if (board.IsDraw() || board.IsInStalemate())
            return 0;
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? -5999000 : 5999000;

        int boardValue = 0;
        PieceList[] allPieces = board.GetAllPieceLists();
        foreach (PieceList pieces in allPieces) {
            int value = pieces.Count * pieceValues[(int)pieces.TypeOfPieceInList];
            int[,] piecePositionValues = piecePositions[pieces.TypeOfPieceInList];
            foreach (Piece piece in pieces)
            {
                int positionValue = PiecePositionValue(piece);
                //if (positionValue != 0)
                //    Console.WriteLine("Pawn position bonus/malus: {0}", positionValue);
                value += positionValue;
            }
            boardValue += board.IsWhiteToMove == pieces.IsWhitePieceList ? value : -value;
        }

        boardScoreTable[boardHash] = boardValue;
        return boardValue;
    }

    private int PiecePositionValue(Piece piece)
    {
        int[,] values = piecePositions[piece.PieceType];
        int pieceY = 7 - piece.Square.Index / 8;
        int pieceX = piece.Square.Index % 8;
        if (!piece.IsWhite)
        {
            pieceY = 7 - pieceY;
            pieceX = 7 - pieceX;
        }
        if (values[pieceY, pieceX] != 0)
        {
            //Console.WriteLine("Position Bonus: {0}", values[pieceY, pieceX]);
        }
        return values[pieceY, pieceX];
    }

    int Search (Board board, int alpha, int beta, int depth) {
        int bestScore = -int.MaxValue;
        Move bestMove = Move.NullMove;

        if (depth == 0) {
            bestScore = Evaluate(board);
            if (bestScore >= beta)
                return bestScore;
            alpha = Math.Max(alpha, bestScore);
            return alpha;
        }

        Move[] moves = board.GetLegalMoves();
        for (int i = 0; i < moves.Length; ++i) {
            board.MakeMove(moves[i]);
            int score = -Search(board, -beta, -alpha, depth - 1);
            board.UndoMove(moves[i]);
            if (score > bestScore) {
                bestScore = score;
                bestMove = moves[i];
                alpha = Math.Max(alpha, score);
                if (alpha >= beta)
                    break;
            }
        }
        if (moves.Length == 0)
            return board.IsInCheck() ? -pieceValues[6] + (searchDepth - depth) : 0;
        if (depth == searchDepth)
            moveToPlay = bestMove;
        return bestScore;
    }
}
