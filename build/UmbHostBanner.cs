// Included in every shipped UmbCheckout assembly by Directory.Build.targets (not in tests or the test sites).
// Decompilers show this constant verbatim; obfuscated assemblies keep it because the class is excluded from obfuscation.
namespace UmbHost;

[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
internal static class UmbHostBanner
{
#if UMBHOST_PAID
    internal const string Text = """
         _   _             _      _   _               _
        | | | | _ __ ___  | |__  | | | |  ___   ___  | |_
        | | | || '_ ` _ \ | '_ \ | |_| | / _ \ / __| | __|
        | |_| || | | | | || |_) ||  _  || (_) |\__ \ | |_
         \___/ |_| |_| |_||_.__/ |_| |_| \___/ |___/  \__|

        UmbCheckout by UmbHost Limited - https://umbhost.net
        This is a paid, licensed package. Stop being cheap - buy a licence: https://umbcheckout.net
        """;
#else
    internal const string Text = """
         _   _             _      _   _               _
        | | | | _ __ ___  | |__  | | | |  ___   ___  | |_
        | | | || '_ ` _ \ | '_ \ | |_| | / _ \ / __| | __|
        | |_| || | | | | || |_) ||  _  || (_) |\__ \ | |_
         \___/ |_| |_| |_||_.__/ |_| |_| \___/ |___/  \__|

        UmbCheckout by UmbHost Limited - https://umbhost.net
        Open source - contributions welcome: https://github.com/UmbHost/UmbCheckout
        """;
#endif
}
