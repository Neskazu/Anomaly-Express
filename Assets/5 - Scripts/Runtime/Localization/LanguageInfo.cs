using System;

namespace Localization
{
    [Serializable]
    public class LanguageInfo
    {
        public string Name;
        public string NativeName;
        public string Code;
        public string Author;
        public int Version = 1;
        public string Flag;
    }
}