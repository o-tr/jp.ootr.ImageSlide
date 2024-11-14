using jp.ootr.common;
using TMPro;
using UnityEngine;
using VRC.SDKBase;

namespace jp.ootr.ImageSlide.Viewer
{
    public enum L10nLanguage
    {
        EN,
        JA
    }
    
    public class L10n : TransformLock {
        [SerializeField] private string[] l10nKeys;
        [SerializeField] private string[] l10nValues;
        
        [SerializeField] private TextMeshProUGUI[] l10nTexts;
        [SerializeField] private string[] l10nTextKeys;

        public override void InitImageSlide()
        {
            base.InitImageSlide();
            ApplyL10n();
        }

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);
            ApplyL10n();
        }

        protected L10nLanguage GetLanguage()
        {
            var lang = VRCPlayerApi.GetCurrentLanguage();
            switch (lang)
            {
                case "ja":
                    return L10nLanguage.JA;
                default:
                    return L10nLanguage.EN;
            }
        }

        protected string GetText(string key, string lang)
        {
            var langKey = $"{lang}.{key}";
            if (l10nKeys.Has(langKey, out var index))
            {
                return l10nValues[index];
            }

            var defaultKey = $"en.{key}";
            if (l10nKeys.Has(defaultKey, out var defaultIndex))
            {
                return l10nValues[defaultIndex];
            }

            return key;
        }
        
        private void ApplyL10n()
        {
            var lang = GetLanguage().ToStr();
            for (var i = 0; i < l10nTexts.Length; i++)
            {
                var text = l10nTexts[i];
                var key = l10nTextKeys[i];
                ConsoleDebug($"l10n: {key} {lang}");
                text.text = GetText(key, lang);
            }
        }
    }
    
    public static class L10nExtensions
    {
        public static string ToStr(this L10nLanguage lang)
        {
            switch (lang)
            {
                case L10nLanguage.JA:
                    return "ja";
                default:
                    return "en";
            }
        }
    }
}
