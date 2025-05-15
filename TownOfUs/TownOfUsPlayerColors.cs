﻿using MiraAPI.Colors;
using UnityEngine;

namespace TownOfUs;

[RegisterCustomColors]
public static class TownOfUsPlayerColors
{
    public static CustomColor Watermelon { get; } = new("Watermelon", new Color32(168, 50, 62, byte.MaxValue), new Color32(101, 30, 37, byte.MaxValue));
    public static CustomColor Chocolate { get; } = new("Chocolate", new Color32(60, 48, 44, byte.MaxValue), new Color32(30, 24, 22, byte.MaxValue));
    public static CustomColor SkyBlue { get; } = new("Sky Blue", new Color32(61, 129, 255, byte.MaxValue), new Color32(31, 65, 128, byte.MaxValue));
    public static CustomColor Beige { get; } = new("Beige", new Color32(240, 211, 165, byte.MaxValue), new Color32(120, 106, 83, byte.MaxValue));
    public static CustomColor Magenta { get; } = new("Magenta", new Color32(255, 0, 127, byte.MaxValue), new Color32(191, 0, 95, byte.MaxValue));
    public static CustomColor SeaGreen { get; } = new("Sea Green", new Color32(61, 255, 181, byte.MaxValue), new Color32(31, 128, 91, byte.MaxValue));
    public static CustomColor Lilac { get; } = new("Lilac", new Color32(186, 161, 255, byte.MaxValue), new Color32(93, 81, 128, byte.MaxValue));
    public static CustomColor Olive { get; } = new("Olive", new Color32(97, 114, 24, byte.MaxValue), new Color32(66, 91, 15, byte.MaxValue));
    public static CustomColor Azure { get; } = new("Azure", new Color32(1, 166, 255, byte.MaxValue), new Color32(17, 104, 151, byte.MaxValue));
    public static CustomColor Plum { get; } = new("Plum", new Color32(79, 0, 127, byte.MaxValue), new Color32(55, 0, 95, byte.MaxValue));
    public static CustomColor Jungle { get; } = new("Jungle", new Color32(0, 47, 0, byte.MaxValue), new Color32(0, 23, 0, byte.MaxValue));
    public static CustomColor Mint { get; } = new("Mint", new Color32(151, 255, 151, byte.MaxValue), new Color32(109, 191, 109, byte.MaxValue));
    public static CustomColor Chartreuse { get; } = new("Chartreuse", new Color32(207, 255, 0, byte.MaxValue), new Color32(143, 191, 61, byte.MaxValue));
    public static CustomColor Macau { get; } = new("Macau", new Color32(0, 97, 93, byte.MaxValue), new Color32(0, 65, 61, byte.MaxValue));
    public static CustomColor Gold { get; } = new("Gold", new Color32(205, 63, 0, byte.MaxValue), new Color32(141, 31, 0, byte.MaxValue));
    public static CustomColor Tawny { get; } = new("Tawny", new Color32(255, 207, 0, byte.MaxValue), new Color32(191, 143, 0, byte.MaxValue));

    public static CustomColor Rainbow { get; } = new("Rainbow", new Color32(0, 0, 0, byte.MaxValue));
}
