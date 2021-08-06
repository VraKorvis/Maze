using UnityEngine;
using System.Collections;

public enum WallType {
    Empty = 0,
    Bot=1,
    Right=0,
    Left=2,
    Top=11,
    BotRight=0,
    BotLeft=2,
    BotTop=19,
    RightLeft=1,
    RightTop=21,
    LeftTop=20,
    BotRightLeft=1,
    BotRightTop=5,
    BotLeftTop=3,
    RightLeftTop=11,
    All = 6
}

public sealed class AutotileID {
    private AutotileID() { }
    public const string Empty = "EEEE";
    public const string Bot = "EWEE";
    public const string Right = "EEEW";
    public const string Left = "WEEE";
    public const string Top = "EEWE";
    public const string BotRight = "EWEW";
    public const string BotLeft = "WWEE";
    public const string BotTop = "EWWE";
    public const string RightLeft = "WEEW";
    public const string RightTop = "EEWW";
    public const string LeftTop = "WEWE";
    public const string BotRightLeft = "WWEW";
    public const string BotRightTop = "EWWW";
    public const string BotLeftTop = "WWWE";
    public const string RightLeftTop = "WEWW";
    public const string All = "WWWW";
}
