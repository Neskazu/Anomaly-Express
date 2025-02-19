using Session.Base;
using Unity.Netcode;

namespace Session.Validators
{
    /// <summary>
    /// Устанавливает максимально кол-во игроков в сессии.
    /// </summary>
    public class PlayerLimit : IConnectionValidator
    {
        private readonly int playerLimit;

        public PlayerLimit(int limit)
        {
            playerLimit = limit;
        }

        public bool Validate(in NetworkManager.ConnectionApprovalRequest request, out string reason)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count >= playerLimit)
            {
                // TODO: Добавить возможность локализации
                reason = "Game is full";
                return false;
            }

            reason = null;
            return true;
        }
    }
}