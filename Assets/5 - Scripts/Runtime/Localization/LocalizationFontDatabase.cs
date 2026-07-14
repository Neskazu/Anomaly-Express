using TMPro;
using UnityEngine;

namespace Localization
{
    [CreateAssetMenu(menuName = "Localization/Font Database")]
    public class LocalizationFontDatabase : ScriptableObject
    {
        public TMP_FontAsset Comfortaa;
        public TMP_FontAsset Japanese;
        public TMP_FontAsset Korean;
        public TMP_FontAsset ChineseSimplified;
        public TMP_FontAsset ChineseTraditional;

        public TMP_FontAsset GetFont(string languageCode)
        {
            return languageCode switch
            {
                "ja" => Japanese,
                "ko" => Korean,
                "zh-Hans" => ChineseSimplified,
                "zh-Hant" => ChineseTraditional,
                _ => Comfortaa
            };
        }
    }
}