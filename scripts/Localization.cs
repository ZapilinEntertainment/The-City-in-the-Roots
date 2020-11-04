public static class Localization
{
    public enum Language : byte { English, Russian}
    public enum Label : byte { Lifepower, Money, People}
    public static Language currentLanguage;

    public static string GetLabel(Label l)
    {
        switch (currentLanguage)
        {
            default:
                {
                    switch(l)
                    {
                        case Label.Lifepower: return "Lifepower";
                        case Label.Money: return "Money";
                        case Label.People: return "People";
                        default: return string.Empty;
                    }
                    break;
                }
        }
    }
}
