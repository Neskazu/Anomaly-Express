using UnityEngine;

namespace Player.Input
{
    public struct PlayerPermissions
    {
        public bool CanMoveVertical;   // W, S
        public bool CanMoveHorizontal; // A, D
        public bool CanRotate;         // Mouse Look
        public bool CanJump;           // Space
        public bool CanSprint;         // Shift
    }

    public static class ControlMapper
    {
        public static PlayerPermissions GetPermissions(int playerIndex, int totalPlayers, bool isSplit)
        {
            if (!isSplit || totalPlayers <= 1)
                return new PlayerPermissions { CanMoveVertical = true, CanMoveHorizontal = true, CanRotate = true, CanJump = true, CanSprint = true };

            return totalPlayers switch
            {
                2 => playerIndex == 0
                    ? new PlayerPermissions { CanMoveVertical = true, CanMoveHorizontal = true }
                    : new PlayerPermissions { CanRotate = true, CanJump = true, CanSprint = true },

                3 => playerIndex switch
                {
                    0 => new PlayerPermissions { CanMoveVertical = true, CanSprint = true },
                    1 => new PlayerPermissions { CanMoveHorizontal = true, CanJump = true },
                    _ => new PlayerPermissions { CanRotate = true }
                },

                4 => playerIndex switch
                {
                    0 => new PlayerPermissions { CanMoveVertical = true },
                    1 => new PlayerPermissions { CanMoveHorizontal = true },
                    2 => new PlayerPermissions { CanRotate = true },
                    _ => new PlayerPermissions { CanJump = true, CanSprint = true }
                },

                _ => new PlayerPermissions { CanMoveVertical = true, CanMoveHorizontal = true, CanRotate = true, CanJump = true, CanSprint = true }
            };
        }
    }
}