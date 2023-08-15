using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    readonly int[] pieceValues = { 0, 100, 300, 350, 500, 900, 0 };
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
                { -20, 0, 30, 0, 0, 30, 0, -20 },
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
                { -50, -50, -50, -50, -50, -50, -50, -50, },
                { -40, -40, -40, -40, -40, -40, -40, -40, },
                { -30, -30, -30, -30, -30, -30, -30, -30, },
                { -20, -20, -20, -20, -20, -20, -20, -20, },    
                { -10, -10, -10, -10, -10, -10, -10, -10, },
                { -10, -10, -10, -10, -10, -10, -10, -10, },
                { -10, -10, -10, -10, -10, -10, -10, -10, },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
            }
        },
    };

    int searchDepth = 6;
    bool isWhite = true;
    bool isBlack = false;
    Move moveToPlay;

    public Move Think(Board board, Timer timer)
    {
        isBlack = board.IsWhiteToMove;
        isWhite = !isBlack;

        searchDepth = chooseSearchDepth(board);
        moveToPlay = Move.NullMove;
        int score = Search(board, -int.MaxValue, int.MaxValue, searchDepth);
        return moveToPlay;
    }

    private int chooseSearchDepth(Board board) {
        int nbPieces = nbPiecesOnBoard(board);
        if (nbPieces > 20)
            return 4;
        if (nbPieces > 10)
            return 5;
        else
            return 6;
    }

    private int nbPiecesOnBoard(Board board)
    {
        int nbPieces = 0;
        foreach (PieceList pieces in board.GetAllPieceLists())
            nbPieces += pieces.Count;
        return nbPieces;
    }

    public int Evaluate(Board board)
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
                int positionValue = piecePositionValue(piece);
                if (positionValue != 0)
                    Console.WriteLine("Pawn position bonus/malus: {0}", positionValue);
                value += positionValue;
            }
            boardValue += board.IsWhiteToMove == pieces.IsWhitePieceList ? value : -value;
        }
        return boardValue;
    }

    private int piecePositionValue(Piece piece)
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
            Console.WriteLine("Position Bonus: {0}", values[pieceY, pieceX]);
        }
        return values[pieceY, pieceX];
    }

    private int evalMove(Move move)
    {
        return 0;
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
            score += evalMove(moves[i]);
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
