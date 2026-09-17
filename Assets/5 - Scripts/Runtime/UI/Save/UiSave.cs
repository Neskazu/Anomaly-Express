using System;
using UnityEngine;

namespace UI
{
    [Serializable]
    public class UiSave
    {
        [SerializeField] private UiNetworkMenuSave networkMenu;

        public UiNetworkMenuSave NetworkMenu => networkMenu;
    }
}