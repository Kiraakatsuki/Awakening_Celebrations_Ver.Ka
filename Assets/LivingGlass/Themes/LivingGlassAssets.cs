using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Awakening Celebrations/Living Glass Assets")]
public sealed class LivingGlassAssets : ScriptableObject
{
    public Texture2D[] portraits = new Texture2D[6];
    public Texture2D[] activation = new Texture2D[6];
    public Texture2D[] living = new Texture2D[6];
    public Font bodyEnglish;
    public Font displayEnglish;
    public Font traditionalChinese;
    public StyleSheet styles;
    public bool reducedMotion;
    public bool reducedEffects;

    public static Color Hex(string value) { ColorUtility.TryParseHtmlString(value, out Color result); return result; }
    public static readonly Color Neutral = Hex("#8BD6E5");
    public static readonly Color[] Accents = { Hex("#5BD8ED"), Hex("#C0AA91"), Hex("#E0B0C8"), Hex("#C5E9FA"), Hex("#FF956A"), Hex("#F1C57A") };
    public static readonly Color[] Atmospheres = { Hex("#137F91"), Hex("#756453"), Hex("#8B557C"), Hex("#6A839A"), Hex("#9B4B2B"), Hex("#98713B") };
    public static readonly string[] ThemeEnglish = { "A living current", "The weight of stillness", "An unfolding renewal", "A quiet protection", "An ember within", "A place of belonging" };
    public static readonly string[] ThemeChinese = { "流動的生命", "靜止的重量", "逐漸展開的新生", "寧靜的守護", "內在的餘燼", "心之歸所" };
}
