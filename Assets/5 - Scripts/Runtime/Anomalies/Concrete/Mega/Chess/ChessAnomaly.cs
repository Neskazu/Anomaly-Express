using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using ChessDotNet;
using Unity.Collections;
using Train;
using System.Threading.Tasks;
using ChessDotNet.Pieces;

namespace Anomalies
{
    public enum ChessPieceType { Pawn, Knight, Bishop, Rook, Queen, King }

    [System.Serializable]
    public struct PieceModelMapping
    {
        public ChessPieceType type;
        public ChessDotNet.Player color;
        public GameObject prefab;
    }

    public class ChessAnomaly : AnomalyBase
    {
        [Header("Settings")]
        [SerializeField] private Transform _boardRoot;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private PieceModelMapping[] _mappings;

        [SerializeField] private DoorController door;

        public System.Action<ChessDotNet.Player> OnGameOver;

        private NetworkVariable<FixedString128Bytes> _currentBoardFen = new NetworkVariable<FixedString128Bytes>();
        private NetworkVariable<ulong> _whitePlayerId = new NetworkVariable<ulong>(999);

        private ChessGame _gameLogic;
        private ChessPiece[,] _activePieces = new ChessPiece[8, 8];
        private Vector2Int? _selectedCoord = null;

        private PlayerAnimator _cachedLocalAnimator;

        private SimpleChessAI _ai;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _currentBoardFen.OnValueChanged += OnBoardChanged;

            if (IsServer && _gameLogic == null)
            {
                _gameLogic = new ChessGame();
                _currentBoardFen.Value = _gameLogic.GetFen();
            }
        }
        private void Start()
        {
            if (IsServer) Activate();
        }
        protected override void OnActivate()
        {
            if (!IsServer) return;

            _gameLogic = new ChessGame();
            _currentBoardFen.Value = _gameLogic.GetFen();

            var clients = NetworkManager.Singleton.ConnectedClientsIds;
            if (clients.Count > 0) _whitePlayerId.Value = clients[0];

            //door locked until win
            door.SetLockServerRpc(true);
            //add ai
            _ai = new SimpleChessAI(depth: 1);
        }

        protected override void OnDeactivate()
        {
            if (IsServer) _whitePlayerId.Value = 999;
            _cachedLocalAnimator = null;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (_activePieces[x, y] != null) Destroy(_activePieces[x, y].gameObject);
                }
            }
        }


        private void Update()
        {
            if (NetworkManager.Singleton.LocalClientId != _whitePlayerId.Value) return;
            if (Input.GetMouseButtonDown(0)) HandleInput();
        }

        private void HandleInput()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                int x, y;

                ChessPiece clickedPiece = hit.collider.GetComponentInParent<ChessPiece>();

                if (clickedPiece != null)
                {
                    Vector3 localPiecePos = _boardRoot.InverseTransformPoint(clickedPiece.transform.position);
                    x = Mathf.RoundToInt(localPiecePos.x / _cellSize);
                    y = Mathf.RoundToInt(localPiecePos.z / _cellSize);
                }
                else
                {
                    Vector3 localPos = _boardRoot.InverseTransformPoint(hit.point);
                    x = Mathf.RoundToInt(localPos.x / _cellSize);
                    y = Mathf.RoundToInt(localPos.z / _cellSize);
                }

                if (x < 0 || x > 7 || y < 0 || y > 7)
                {
                    return;
                }

                if (_selectedCoord == null)
                {
                    if (_activePieces[x, y] != null)
                    {
                        char pieceChar = _activePieces[x, y].name[0];

                        if (!char.IsUpper(pieceChar))
                        {
                            return;
                        }
                        _selectedCoord = new Vector2Int(x, y);
                        _activePieces[x, y].SetSelected(true);
                    }
                }
                else
                {
                    _activePieces[_selectedCoord.Value.x, _selectedCoord.Value.y].SetSelected(false);

                    string moveFrom = CoordToAlgebraic(_selectedCoord.Value);
                    string moveTo = CoordToAlgebraic(new Vector2Int(x, y));
                    PlayAnimationAndSendMove(moveFrom, moveTo);
                    _selectedCoord = null;
                }
            }
        }
        private void PlayAnimationAndSendMove(string moveFrom, string moveTo)
        {
            if (_cachedLocalAnimator == null)
            {
                var localClient = NetworkManager.Singleton.LocalClient;
                if (localClient != null && localClient.PlayerObject != null)
                {
                    _cachedLocalAnimator = localClient.PlayerObject.GetComponentInChildren<PlayerAnimator>();
                }
            }

            if (_cachedLocalAnimator != null)
            {
                _cachedLocalAnimator.TriggerInteract();
            }

            // 3. Отправляем ход на сервер
            MakeMoveServerRpc(moveFrom, moveTo);
        }
        [ServerRpc(RequireOwnership = false)]
        public void MakeMoveServerRpc(string moveFrom, string moveTo, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;
            if (senderId != _whitePlayerId.Value) return;
            ChessDotNet.Player player = _gameLogic.WhoseTurn;
            Piece pieceAtFrom = _gameLogic.GetPieceAt(new Position(moveFrom));
            bool isPawnMoving = pieceAtFrom != null && pieceAtFrom is Pawn && pieceAtFrom.Owner == player;
            bool isReachingLastRank = (player == ChessDotNet.Player.White && moveTo[1] == '8') ||
                                     (player == ChessDotNet.Player.Black && moveTo[1] == '1');

            Move move;
            if (isPawnMoving && isReachingLastRank)
            {
                move = new Move(moveFrom, moveTo, player, player == ChessDotNet.Player.White ? 'Q' : 'q');
            }
            else
            {
                move = new Move(moveFrom, moveTo, player);
            }

            bool isValid = _gameLogic.IsValidMove(move);
            if (isValid)
            {
                _gameLogic.MakeMove(move, true);
                _currentBoardFen.Value = _gameLogic.GetFen();

                if (!CheckGameOver())
                {
                    StartCoroutine(DelayedAIMoveRoutine());
                }
            }
            else
            {
                InvalidMoveClientRpc(moveFrom, rpcParams.Receive.SenderClientId);
            }
        }
        [ClientRpc]
        private void InvalidMoveClientRpc(string moveFrom, ulong targetClientId)
        {
            if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

            int x = moveFrom[0] - 'a';
            int y = moveFrom[1] - '1';

            if (_activePieces[x, y] != null)
            {
                _activePieces[x, y].FlashError();
            }
        }
        private IEnumerator DelayedAIMoveRoutine()
        {
            yield return new WaitForSeconds(0.6f);

            string currentFen = _gameLogic.GetFen();

            Task<Move> aiTask = Task.Run(() =>
            {
                ChessGame threadSafeGame = new ChessGame(currentFen);
                return _ai.GetBestMove(threadSafeGame);
            });

            yield return new WaitUntil(() => aiTask.IsCompleted);

            Move bestMove = aiTask.Result;

            if (bestMove != null)
            {
                _gameLogic.MakeMove(bestMove, true);
            }
            else
            {
                var moves = _gameLogic.GetValidMoves(ChessDotNet.Player.Black);
                if (moves.Count > 0)
                {
                    _gameLogic.MakeMove(moves[0], true);
                }
            }

            _currentBoardFen.Value = _gameLogic.GetFen();
            CheckGameOver();
        }

        private void OnBoardChanged(FixedString128Bytes oldFen, FixedString128Bytes newFen)
        {
            UpdateBoardVisuals(newFen.ToString());
        }

        private void UpdateBoardVisuals(string fen)
        {
            char[,] newBoardState = ParseFen(fen);
            ChessPiece[,] nextActivePieces = new ChessPiece[8, 8];

            Vector2Int moveFrom = new Vector2Int(-1, -1);
            Vector2Int moveTo = new Vector2Int(-1, -1);

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    char newChar = newBoardState[x, y];
                    ChessPiece oldPiece = _activePieces[x, y];
                    char oldChar = oldPiece != null ? oldPiece.name[0] : ' ';

                    if (newChar == ' ' && oldChar != ' ') moveFrom = new Vector2Int(x, y);
                    if (newChar != ' ' && newChar != oldChar) moveTo = new Vector2Int(x, y);
                }
            }

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    char newChar = newBoardState[x, y];
                    if (x == moveTo.x && y == moveTo.y && moveFrom.x != -1)
                    {
                        if (_activePieces[x, y] != null) _activePieces[x, y].PlayCaptureAndDestroy();

                        ChessPiece movingPiece = _activePieces[moveFrom.x, moveFrom.y];

                        char oldCharFrom = movingPiece != null ? movingPiece.name[0] : ' ';

                        if (char.ToLower(oldCharFrom) == 'p' && char.ToLower(newChar) != 'p')
                        {
                            movingPiece.PlayCaptureAndDestroy();
                            nextActivePieces[x, y] = SpawnPiece(x, y, newChar);
                        }
                        else if (movingPiece != null)
                        {
                            
                            movingPiece.MoveTo(new Vector3(x * _cellSize, 0, y * _cellSize));
                            movingPiece.name = $"{newChar}_{x}_{y}";
                            nextActivePieces[x, y] = movingPiece;
                        }
                    }
                    else if (newChar != ' ' && _activePieces[x, y] != null && _activePieces[x, y].name[0] == newChar)
                    {
                        nextActivePieces[x, y] = _activePieces[x, y];
                    }
                    else if (newChar != ' ')
                    {
                        nextActivePieces[x, y] = SpawnPiece(x, y, newChar);
                    }
                }
            }

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (_activePieces[x, y] != null)
                    {
                        bool stillExists = false;
                        foreach (var p in nextActivePieces) if (p == _activePieces[x, y]) stillExists = true;
                        if (!stillExists) _activePieces[x, y].PlayCaptureAndDestroy();
                    }
                }
            }
            _activePieces = nextActivePieces;
        }

        private char[,] ParseFen(string fen)
        {
            char[,] board = new char[8, 8];
            for (int i = 0; i < 8; i++) for (int j = 0; j < 8; j++) board[i, j] = ' ';
            string boardPart = fen.Split(' ')[0];
            string[] rows = boardPart.Split('/');
            for (int r = 0; r < 8; r++)
            {
                int col = 0;
                foreach (char c in rows[r])
                {
                    if (char.IsDigit(c)) col += (int)char.GetNumericValue(c);
                    else { board[col, 7 - r] = c; col++; }
                }
            }
            return board;
        }

        private ChessPiece SpawnPiece(int x, int y, char fenChar)
        {
            ChessDotNet.Player color = char.IsUpper(fenChar) ? ChessDotNet.Player.White : ChessDotNet.Player.Black;
            ChessPieceType type = GetTypeFromChar(fenChar);
            var mapping = _mappings.FirstOrDefault(m => m.type == type && m.color == color);
            if (mapping.prefab == null) return null;

            GameObject obj = Instantiate(mapping.prefab, _boardRoot);
            obj.transform.localPosition = new Vector3(x * _cellSize, 0, y * _cellSize);
            obj.name = $"{fenChar}_{x}_{y}";
            return obj.GetComponent<ChessPiece>();
        }

        private string CoordToAlgebraic(Vector2Int coord) => $"{(char)('a' + coord.x)}{coord.y + 1}";

        private ChessPieceType GetTypeFromChar(char c)
        {
            return char.ToLower(c) switch
            {
                'p' => ChessPieceType.Pawn,
                'n' => ChessPieceType.Knight,
                'b' => ChessPieceType.Bishop,
                'r' => ChessPieceType.Rook,
                'q' => ChessPieceType.Queen,
                'k' => ChessPieceType.King,
                _ => ChessPieceType.Pawn
            };
        }
        [ClientRpc]
        private void NotifyGameOverClientRpc(ChessDotNet.Player winner)
        {
            OnGameOver?.Invoke(winner);
        }
        private bool CheckGameOver()
        {
            if (_gameLogic.IsCheckmated(ChessDotNet.Player.Black))
            {
                door.SetLockServerRpc(false);

                NotifyGameOverClientRpc(ChessDotNet.Player.White);
                Deactivate();
                return true;
            }
            else if (_gameLogic.IsCheckmated(ChessDotNet.Player.White) ||
                     _gameLogic.IsStalemated(ChessDotNet.Player.White) ||
                     _gameLogic.IsStalemated(ChessDotNet.Player.Black))
            {
                ResetMatch();
                return true;
            }

            return false;
        }

        private void ResetMatch()
        {
            _gameLogic = new ChessGame();
            _currentBoardFen.Value = _gameLogic.GetFen();

            door.SetLockServerRpc(true);
        }
        private void OnDrawGizmos()
        {
            if (_boardRoot == null) return;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Vector3 localCenter = new Vector3(x * _cellSize, 0, y * _cellSize);

                    Vector3 worldCenter = _boardRoot.TransformPoint(localCenter);

                    Gizmos.color = (x + y) % 2 == 0 ? Color.green : Color.yellow;
                    Gizmos.DrawWireCube(worldCenter, new Vector3(_cellSize, 0.01f, _cellSize));
                }
            }
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_boardRoot.position, 0.05f);
        }
    }
}