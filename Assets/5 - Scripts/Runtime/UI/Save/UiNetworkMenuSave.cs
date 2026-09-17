using System;
using System.Net;
using UnityEngine;

namespace UI
{
    [Serializable]
    public class UiNetworkMenuSave
    {
        [SerializeField] private string username;
        [Space]
        [SerializeField] private string ip;
        [SerializeField] private int port;

        public string Username => string.IsNullOrWhiteSpace(username) ? "Player" : username;
        public string Ip => string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip;
        public int Port => port == 0 ? 25565 : port;

        public void SetIp(string ipv4)
        {
            if (string.IsNullOrWhiteSpace(ipv4))
            {
                return;
            }

            if (IPAddress.TryParse(ipv4, out var address))
            {
                ip = ipv4;
            }
        }

        public void SetPort(string stringPort)
        {
            var newPort = int.Parse(stringPort);

            if (newPort is < 0 or > ushort.MaxValue)
            {
                port = 25565;
                return;
            }

            port = newPort;
        }
    }
}