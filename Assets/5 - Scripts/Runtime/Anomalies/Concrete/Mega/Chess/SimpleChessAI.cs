using ChessDotNet;
using System;
using UnityEngine;

public class SimpleChessAI
{
    private int _maxDepth;

    public SimpleChessAI(int depth = 3) => _maxDepth = depth;

    public Move GetBestMove(ChessGame game)
    {
        Move bestMove = null;
        ChessDotNet.Player aiPlayer = game.WhoseTurn;
        int bestValue = (aiPlayer == ChessDotNet.Player.White) ? int.MinValue : int.MaxValue;

        var validMoves = game.GetValidMoves(aiPlayer);

        foreach (var move in validMoves)
        {
            ChessGame branch = new ChessGame(game.GetFen());
            branch.MakeMove(move, true);

            int boardValue = Minimax(branch, _maxDepth - 1, int.MinValue, int.MaxValue, aiPlayer != ChessDotNet.Player.White);

            if (aiPlayer == ChessDotNet.Player.White)
            {
                if (boardValue > bestValue) { bestValue = boardValue; bestMove = move; }
            }
            else
            {
                if (boardValue < bestValue) { bestValue = boardValue; bestMove = move; }
            }
        }
        return bestMove;
    }

    private int Minimax(ChessGame game, int depth, int alpha, int beta, bool isMaximizing)
    {
        if (depth <= 0 || game.IsCheckmated(game.WhoseTurn) || game.IsStalemated(game.WhoseTurn))
        {
            return ChessEvaluator.Evaluate(game);
        }

        var moves = game.GetValidMoves(game.WhoseTurn);

        if (isMaximizing)
        {
            int maxEval = int.MinValue;
            foreach (var move in moves)
            {
                ChessGame nextStepGame = new ChessGame(game.GetFen());
                nextStepGame.MakeMove(move, true);

                int eval = Minimax(nextStepGame, depth - 1, alpha, beta, false);

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) break;
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var move in moves)
            {
                ChessGame nextStepGame = new ChessGame(game.GetFen());
                nextStepGame.MakeMove(move, true);

                int eval = Minimax(nextStepGame, depth - 1, alpha, beta, true);

                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break;
            }
            return minEval;
        }
    }
}