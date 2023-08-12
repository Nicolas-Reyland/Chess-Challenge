using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    readonly int[] pieceValues = { 0, 100, 300, 350, 500, 900, 1 };
    readonly Dictionary<PieceType, int[,]> piecePositions = new()
    {
        {
            PieceType.Pawn, new int[8,8] {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
            }
        },
        {
            PieceType.Knight, new int[8,8] {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
            }
        },
        {
            PieceType.Bishop, new int[8,8] {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
            }
        },
        {
            PieceType.Rook, new int[8,8] {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
            }
        },
        {
            PieceType.Queen, new int[8,8] {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
            }
        },
        {
            PieceType.King, new int[8,8] {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
            }
        },
    };

    int searchDepth = 6;
    Move moveToPlay;

    public Move Think(Board board, Timer timer)
    {
        searchDepth = chooseSearchDepth(board);
        moveToPlay = Move.NullMove;
        int score = Search(board, -int.MaxValue, int.MaxValue, searchDepth);
        return moveToPlay;
    }

    private int chooseSearchDepth(Board board) {
        int nbPieces = 0;
        foreach (PieceList pieces in board.GetAllPieceLists())
            nbPieces += pieces.Count;
        return nbPieces > 20 ? 4 : 6;
    }

    private int Evaluate(Board board)
    {
        if (board.IsDraw() || board.IsInStalemate())
            return 0;
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? int.MinValue : int.MaxValue;

        int boardValue = 0;
        PieceList[] allPieces = board.GetAllPieceLists();
        foreach (PieceList pieces in allPieces) {
            int value = pieces.Count * pieceValues[(int)pieces.TypeOfPieceInList];
            int[,] piecePositionValues = piecePositions[pieces.TypeOfPieceInList];
            foreach (Piece piece in pieces)
            {
                int pieceY = piece.Square.Index / 8;
                int pieceX = piece.Square.Index % 8;
                int piecePositionValue = piecePositionValues[pieceY, pieceX];
                /*
                if (piecePositionValue != 0)
                    Console.WriteLine("Pawn position bonus/malus: {0}", piecePositionValue);
                */
                value += piecePositionValue;
            }
            boardValue += board.IsWhiteToMove == pieces.IsWhitePieceList ? value : -value;
        }
        return boardValue;
    }

    int Search (Board board, int alpha, int beta, int depth){
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
