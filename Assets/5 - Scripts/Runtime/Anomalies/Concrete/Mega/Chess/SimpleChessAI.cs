using ChessDotNet;
using ChessDotNet.Pieces;
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

            int boardValue = Minimax(branch, 2, int.MinValue, int.MaxValue, aiPlayer != ChessDotNet.Player.White);

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
        if (depth == 0) return ChessEvaluator.Evaluate(game);

        var moves = game.GetValidMoves(game.WhoseTurn);
        if (isMaximizing)
        {
            int maxEval = int.MinValue;
            foreach (var move in moves)
            {
                game.MakeMove(move, true);
                int eval = Minimax(game, depth - 1, alpha, beta, false);
                game.Undo();
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha) break;
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var move in moves)
            {
                game.MakeMove(move, true);
                int eval = Minimax(game, depth - 1, alpha, beta, true);
                game.Undo();
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha) break;
            }
            return minEval;
        }
    }
}