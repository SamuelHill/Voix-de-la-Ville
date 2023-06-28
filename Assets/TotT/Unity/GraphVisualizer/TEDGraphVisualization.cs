﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TED.Interpreter;
using TED;
using TED.Utilities;
using TotT.Unity;
using TotT.Utilities;
using UnityEngine;

namespace GraphVisualization {
    // ReSharper disable once InconsistentNaming
    public class TEDGraphVisualization : Graph {
        private static readonly Dictionary<string, Color> NonStandardColorNames;
        private static readonly DotColor[] DotColors = {
            new("aliceblue", 147, 15, 255),
            new("antiquewhite", 24, 35, 250),
            new("antiquewhite1", 23, 36, 255),
            new("antiquewhite2", 23, 36, 238),
            new("antiquewhite3", 23, 36, 205),
            new("antiquewhite4", 24, 34, 139),
            new("aquamarine", 113, 128, 255),
            new("aquamarine1", 113, 128, 255),
            new("aquamarine2", 113, 128, 238),
            new("aquamarine3", 113, 128, 205),
            new("aquamarine4", 113, 128, 139),
            new("azure", 127, 15, 255),
            new("azure1", 127, 15, 255),
            new("azure2", 127, 15, 238),
            new("azure3", 127, 14, 205),
            new("azure4", 127, 14, 139),
            new("beige", 42, 26, 245),
            new("bisque", 23, 58, 255),
            new("bisque1", 23, 58, 255),
            new("bisque2", 23, 58, 238),
            new("bisque3", 22, 58, 205),
            new("bisque4", 23, 58, 139),
            new("black", 0, 0, 0),
            new("blanchedalmond", 25, 49, 255),
            new("blue", 170, 255, 255),
            new("blue1", 170, 255, 255),
            new("blue2", 170, 255, 238),
            new("blue3", 170, 255, 205),
            new("blue4", 170, 255, 139),
            new("blueviolet", 192, 206, 226),
            new("brown", 0, 190, 165),
            new("brown1", 0, 191, 255),
            new("brown2", 0, 191, 238),
            new("brown3", 0, 191, 205),
            new("brown4", 0, 190, 139),
            new("burlywood", 23, 99, 222),
            new("burlywood1", 23, 100, 255),
            new("burlywood2", 23, 99, 238),
            new("burlywood3", 23, 99, 205),
            new("burlywood4", 23, 99, 139),
            new("cadetblue", 128, 103, 160),
            new("cadetblue1", 131, 103, 255),
            new("cadetblue2", 131, 102, 238),
            new("cadetblue3", 131, 103, 205),
            new("cadetblue4", 131, 102, 139),
            new("chartreuse", 63, 255, 255),
            new("chartreuse1", 63, 255, 255),
            new("chartreuse2", 63, 255, 238),
            new("chartreuse3", 63, 255, 205),
            new("chartreuse4", 63, 255, 139),
            new("chocolate", 17, 218, 210),
            new("chocolate1", 17, 219, 255),
            new("chocolate2", 17, 219, 238),
            new("chocolate3", 17, 218, 205),
            new("chocolate4", 17, 220, 139),
            new("coral", 11, 175, 255),
            new("coral1", 7, 169, 255),
            new("coral2", 6, 169, 238),
            new("coral3", 6, 169, 205),
            new("coral4", 6, 168, 139),
            new("cornflowerblue", 154, 147, 237),
            new("cornsilk", 33, 34, 255),
            new("cornsilk1", 33, 34, 255),
            new("cornsilk2", 34, 35, 238),
            new("cornsilk3", 34, 34, 205),
            new("cornsilk4", 35, 34, 139),
            new("crimson", 246, 231, 220),
            new("cyan", 127, 255, 255),
            new("cyan1", 127, 255, 255),
            new("cyan2", 127, 255, 238),
            new("cyan3", 127, 255, 205),
            new("cyan4", 127, 255, 139),
            new("darkgoldenrod", 30, 239, 184),
            new("darkgoldenrod1", 30, 240, 255),
            new("darkgoldenrod2", 30, 240, 238),
            new("darkgoldenrod3", 30, 240, 205),
            new("darkgoldenrod4", 30, 240, 139),
            new("darkgreen", 85, 255, 100),
            new("darkkhaki", 39, 110, 189),
            new("darkolivegreen", 58, 142, 107),
            new("darkolivegreen1", 58, 143, 255),
            new("darkolivegreen2", 58, 143, 238),
            new("darkolivegreen3", 58, 143, 205),
            new("darkolivegreen4", 58, 143, 139),
            new("darkorange", 23, 255, 255),
            new("darkorange1", 21, 255, 255),
            new("darkorange2", 21, 255, 238),
            new("darkorange3", 21, 255, 205),
            new("darkorange4", 21, 255, 139),
            new("darkorchid", 198, 192, 204),
            new("darkorchid1", 198, 193, 255),
            new("darkorchid2", 198, 192, 238),
            new("darkorchid3", 198, 192, 205),
            new("darkorchid4", 198, 192, 139),
            new("darksalmon", 10, 121, 233),
            new("darkseagreen", 85, 61, 188),
            new("darkseagreen1", 85, 62, 255),
            new("darkseagreen2", 85, 62, 238),
            new("darkseagreen3", 85, 62, 205),
            new("darkseagreen4", 85, 62, 139),
            new("darkslateblue", 175, 143, 139),
            new("darkslategray", 127, 103, 79),
            new("darkslategray1", 127, 104, 255),
            new("darkslategray2", 127, 103, 238),
            new("darkslategray3", 127, 104, 205),
            new("darkslategray4", 127, 104, 139),
            new("darkslategrey", 127, 103, 79),
            new("darkturquoise", 128, 255, 209),
            new("darkviolet", 199, 255, 211),
            new("deeppink", 232, 235, 255),
            new("deeppink1", 232, 235, 255),
            new("deeppink2", 232, 235, 238),
            new("deeppink3", 232, 235, 205),
            new("deeppink4", 231, 236, 139),
            new("deepskyblue", 138, 255, 255),
            new("deepskyblue1", 138, 255, 255),
            new("deepskyblue2", 138, 255, 238),
            new("deepskyblue3", 138, 255, 205),
            new("deepskyblue4", 138, 255, 139),
            new("dimgray", 0, 0, 105),
            new("dimgrey", 0, 0, 105),
            new("dodgerblue", 148, 225, 255),
            new("dodgerblue1", 148, 225, 255),
            new("dodgerblue2", 148, 225, 238),
            new("dodgerblue3", 148, 225, 205),
            new("dodgerblue4", 148, 225, 139),
            new("firebrick", 0, 206, 178),
            new("firebrick1", 0, 207, 255),
            new("firebrick2", 0, 207, 238),
            new("firebrick3", 0, 207, 205),
            new("firebrick4", 0, 207, 139),
            new("floralwhite", 28, 15, 255),
            new("forestgreen", 85, 192, 139),
            new("gainsboro", 0, 0, 220),
            new("ghostwhite", 170, 7, 255),
            new("gold", 35, 255, 255),
            new("gold1", 35, 255, 255),
            new("gold2", 35, 255, 238),
            new("gold3", 35, 255, 205),
            new("gold4", 35, 255, 139),
            new("goldenrod", 30, 217, 218),
            new("goldenrod1", 30, 218, 255),
            new("goldenrod2", 30, 218, 238),
            new("goldenrod3", 30, 218, 205),
            new("goldenrod4", 30, 218, 139),
            new("gray", 0, 0, 192),
            new("gray0", 0, 0, 0),
            new("gray1", 0, 0, 3),
            new("gray10", 0, 0, 26),
            new("gray100", 0, 0, 255),
            new("gray11", 0, 0, 28),
            new("gray12", 0, 0, 31),
            new("gray13", 0, 0, 33),
            new("gray14", 0, 0, 36),
            new("gray15", 0, 0, 38),
            new("gray16", 0, 0, 41),
            new("gray17", 0, 0, 43),
            new("gray18", 0, 0, 46),
            new("gray19", 0, 0, 48),
            new("gray2", 0, 0, 5),
            new("gray20", 0, 0, 51),
            new("gray21", 0, 0, 54),
            new("gray22", 0, 0, 56),
            new("gray23", 0, 0, 59),
            new("gray24", 0, 0, 61),
            new("gray25", 0, 0, 64),
            new("gray26", 0, 0, 66),
            new("gray27", 0, 0, 69),
            new("gray28", 0, 0, 71),
            new("gray29", 0, 0, 74),
            new("gray3", 0, 0, 8),
            new("gray30", 0, 0, 77),
            new("gray31", 0, 0, 79),
            new("gray32", 0, 0, 82),
            new("gray33", 0, 0, 84),
            new("gray34", 0, 0, 87),
            new("gray35", 0, 0, 89),
            new("gray36", 0, 0, 92),
            new("gray37", 0, 0, 94),
            new("gray38", 0, 0, 97),
            new("gray39", 0, 0, 99),
            new("gray4", 0, 0, 10),
            new("gray40", 0, 0, 102),
            new("gray41", 0, 0, 105),
            new("gray42", 0, 0, 107),
            new("gray43", 0, 0, 110),
            new("gray44", 0, 0, 112),
            new("gray45", 0, 0, 115),
            new("gray46", 0, 0, 117),
            new("gray47", 0, 0, 120),
            new("gray48", 0, 0, 122),
            new("gray49", 0, 0, 125),
            new("gray5", 0, 0, 13),
            new("gray50", 0, 0, 127),
            new("gray51", 0, 0, 130),
            new("gray52", 0, 0, 133),
            new("gray53", 0, 0, 135),
            new("gray54", 0, 0, 138),
            new("gray55", 0, 0, 140),
            new("gray56", 0, 0, 143),
            new("gray57", 0, 0, 145),
            new("gray58", 0, 0, 148),
            new("gray59", 0, 0, 150),
            new("gray6", 0, 0, 15),
            new("gray60", 0, 0, 153),
            new("gray61", 0, 0, 156),
            new("gray62", 0, 0, 158),
            new("gray63", 0, 0, 161),
            new("gray64", 0, 0, 163),
            new("gray65", 0, 0, 166),
            new("gray66", 0, 0, 168),
            new("gray67", 0, 0, 171),
            new("gray68", 0, 0, 173),
            new("gray69", 0, 0, 176),
            new("gray7", 0, 0, 18),
            new("gray70", 0, 0, 179),
            new("gray71", 0, 0, 181),
            new("gray72", 0, 0, 184),
            new("gray73", 0, 0, 186),
            new("gray74", 0, 0, 189),
            new("gray75", 0, 0, 191),
            new("gray76", 0, 0, 194),
            new("gray77", 0, 0, 196),
            new("gray78", 0, 0, 199),
            new("gray79", 0, 0, 201),
            new("gray8", 0, 0, 20),
            new("gray80", 0, 0, 204),
            new("gray81", 0, 0, 207),
            new("gray82", 0, 0, 209),
            new("gray83", 0, 0, 212),
            new("gray84", 0, 0, 214),
            new("gray85", 0, 0, 217),
            new("gray86", 0, 0, 219),
            new("gray87", 0, 0, 222),
            new("gray88", 0, 0, 224),
            new("gray89", 0, 0, 227),
            new("gray9", 0, 0, 23),
            new("gray90", 0, 0, 229),
            new("gray91", 0, 0, 232),
            new("gray92", 0, 0, 235),
            new("gray93", 0, 0, 237),
            new("gray94", 0, 0, 240),
            new("gray95", 0, 0, 242),
            new("gray96", 0, 0, 245),
            new("gray97", 0, 0, 247),
            new("gray98", 0, 0, 250),
            new("gray99", 0, 0, 252),
            new("green", 85, 255, 255),
            new("green1", 85, 255, 255),
            new("green2", 85, 255, 238),
            new("green3", 85, 255, 205),
            new("green4", 85, 255, 139),
            new("greenyellow", 59, 208, 255),
            new("grey", 0, 0, 192),
            new("grey0", 0, 0, 0),
            new("grey1", 0, 0, 3),
            new("grey10", 0, 0, 26),
            new("grey100", 0, 0, 255),
            new("grey11", 0, 0, 28),
            new("grey12", 0, 0, 31),
            new("grey13", 0, 0, 33),
            new("grey14", 0, 0, 36),
            new("grey15", 0, 0, 38),
            new("grey16", 0, 0, 41),
            new("grey17", 0, 0, 43),
            new("grey18", 0, 0, 46),
            new("grey19", 0, 0, 48),
            new("grey2", 0, 0, 5),
            new("grey20", 0, 0, 51),
            new("grey21", 0, 0, 54),
            new("grey22", 0, 0, 56),
            new("grey23", 0, 0, 59),
            new("grey24", 0, 0, 61),
            new("grey25", 0, 0, 64),
            new("grey26", 0, 0, 66),
            new("grey27", 0, 0, 69),
            new("grey28", 0, 0, 71),
            new("grey29", 0, 0, 74),
            new("grey3", 0, 0, 8),
            new("grey30", 0, 0, 77),
            new("grey31", 0, 0, 79),
            new("grey32", 0, 0, 82),
            new("grey33", 0, 0, 84),
            new("grey34", 0, 0, 87),
            new("grey35", 0, 0, 89),
            new("grey36", 0, 0, 92),
            new("grey37", 0, 0, 94),
            new("grey38", 0, 0, 97),
            new("grey39", 0, 0, 99),
            new("grey4", 0, 0, 10),
            new("grey40", 0, 0, 102),
            new("grey41", 0, 0, 105),
            new("grey42", 0, 0, 107),
            new("grey43", 0, 0, 110),
            new("grey44", 0, 0, 112),
            new("grey45", 0, 0, 115),
            new("grey46", 0, 0, 117),
            new("grey47", 0, 0, 120),
            new("grey48", 0, 0, 122),
            new("grey49", 0, 0, 125),
            new("grey5", 0, 0, 13),
            new("grey50", 0, 0, 127),
            new("grey51", 0, 0, 130),
            new("grey52", 0, 0, 133),
            new("grey53", 0, 0, 135),
            new("grey54", 0, 0, 138),
            new("grey55", 0, 0, 140),
            new("grey56", 0, 0, 143),
            new("grey57", 0, 0, 145),
            new("grey58", 0, 0, 148),
            new("grey59", 0, 0, 150),
            new("grey6", 0, 0, 15),
            new("grey60", 0, 0, 153),
            new("grey61", 0, 0, 156),
            new("grey62", 0, 0, 158),
            new("grey63", 0, 0, 161),
            new("grey64", 0, 0, 163),
            new("grey65", 0, 0, 166),
            new("grey66", 0, 0, 168),
            new("grey67", 0, 0, 171),
            new("grey68", 0, 0, 173),
            new("grey69", 0, 0, 176),
            new("grey7", 0, 0, 18),
            new("grey70", 0, 0, 179),
            new("grey71", 0, 0, 181),
            new("grey72", 0, 0, 184),
            new("grey73", 0, 0, 186),
            new("grey74", 0, 0, 189),
            new("grey75", 0, 0, 191),
            new("grey76", 0, 0, 194),
            new("grey77", 0, 0, 196),
            new("grey78", 0, 0, 199),
            new("grey79", 0, 0, 201),
            new("grey8", 0, 0, 20),
            new("grey80", 0, 0, 204),
            new("grey81", 0, 0, 207),
            new("grey82", 0, 0, 209),
            new("grey83", 0, 0, 212),
            new("grey84", 0, 0, 214),
            new("grey85", 0, 0, 217),
            new("grey86", 0, 0, 219),
            new("grey87", 0, 0, 222),
            new("grey88", 0, 0, 224),
            new("grey89", 0, 0, 227),
            new("grey9", 0, 0, 23),
            new("grey90", 0, 0, 229),
            new("grey91", 0, 0, 232),
            new("grey92", 0, 0, 235),
            new("grey93", 0, 0, 237),
            new("grey94", 0, 0, 240),
            new("grey95", 0, 0, 242),
            new("grey96", 0, 0, 245),
            new("grey97", 0, 0, 247),
            new("grey98", 0, 0, 250),
            new("grey99", 0, 0, 252),
            new("honeydew", 85, 15, 255),
            new("honeydew1", 85, 15, 255),
            new("honeydew2", 85, 15, 238),
            new("honeydew3", 85, 14, 205),
            new("honeydew4", 85, 14, 139),
            new("hotpink", 233, 150, 255),
            new("hotpink1", 234, 145, 255),
            new("hotpink2", 235, 141, 238),
            new("hotpink3", 236, 135, 205),
            new("hotpink4", 234, 148, 139),
            new("indianred", 0, 140, 205),
            new("indianred1", 0, 148, 255),
            new("indianred2", 0, 148, 238),
            new("indianred3", 0, 149, 205),
            new("indianred4", 0, 148, 139),
            new("indigo", 194, 255, 130),
            new("ivory", 42, 15, 255),
            new("ivory1", 42, 15, 255),
            new("ivory2", 42, 15, 238),
            new("ivory3", 42, 14, 205),
            new("ivory4", 42, 14, 139),
            new("khaki", 38, 106, 240),
            new("khaki1", 39, 112, 255),
            new("khaki2", 39, 112, 238),
            new("khaki3", 39, 111, 205),
            new("khaki4", 39, 111, 139),
            new("lavender", 170, 20, 250),
            new("lavenderblush", 240, 15, 255),
            new("lavenderblush1", 240, 15, 255),
            new("lavenderblush2", 239, 15, 238),
            new("lavenderblush3", 240, 14, 205),
            new("lavenderblush4", 239, 14, 139),
            new("lawngreen", 64, 255, 252),
            new("lemonchiffon", 38, 49, 255),
            new("lemonchiffon1", 38, 49, 255),
            new("lemonchiffon2", 37, 50, 238),
            new("lemonchiffon3", 38, 49, 205),
            new("lemonchiffon4", 39, 49, 139),
            new("lightblue", 137, 63, 230),
            new("lightblue1", 138, 64, 255),
            new("lightblue2", 138, 64, 238),
            new("lightblue3", 138, 63, 205),
            new("lightblue4", 137, 64, 139),
            new("lightcoral", 0, 119, 240),
            new("lightcyan", 127, 31, 255),
            new("lightcyan1", 127, 31, 255),
            new("lightcyan2", 127, 31, 238),
            new("lightcyan3", 127, 31, 205),
            new("lightcyan4", 127, 31, 139),
            new("lightgoldenrod", 35, 115, 238),
            new("lightgoldenrod1", 35, 116, 255),
            new("lightgoldenrod2", 35, 115, 238),
            new("lightgoldenrod3", 35, 115, 205),
            new("lightgoldenrod4", 35, 115, 139),
            new("lightgoldenrodyellow", 42, 40, 250),
            new("lightgray", 0, 0, 211),
            new("lightgrey", 0, 0, 211),
            new("lightpink", 248, 73, 255),
            new("lightpink1", 249, 81, 255),
            new("lightpink2", 248, 81, 238),
            new("lightpink3", 249, 80, 205),
            new("lightpink4", 249, 80, 139),
            new("lightsalmon", 12, 132, 255),
            new("lightsalmon1", 12, 132, 255),
            new("lightsalmon2", 11, 132, 238),
            new("lightsalmon3", 12, 133, 205),
            new("lightsalmon4", 12, 133, 139),
            new("lightseagreen", 125, 209, 178),
            new("lightskyblue", 143, 117, 250),
            new("lightskyblue1", 143, 79, 255),
            new("lightskyblue2", 143, 79, 238),
            new("lightskyblue3", 142, 79, 205),
            new("lightskyblue4", 143, 78, 139),
            new("lightslateblue", 175, 143, 255),
            new("lightslategray", 148, 56, 153),
            new("lightslategrey", 148, 56, 153),
            new("lightsteelblue", 151, 52, 222),
            new("lightsteelblue1", 151, 53, 255),
            new("lightsteelblue2", 151, 53, 238),
            new("lightsteelblue3", 151, 53, 205),
            new("lightsteelblue4", 150, 53, 139),
            new("lightyellow", 42, 31, 255),
            new("lightyellow1", 42, 31, 255),
            new("lightyellow2", 42, 31, 238),
            new("lightyellow3", 42, 31, 205),
            new("lightyellow4", 42, 31, 139),
            new("limegreen", 85, 192, 205),
            new("linen", 21, 20, 250),
            new("magenta", 212, 255, 255),
            new("magenta1", 212, 255, 255),
            new("magenta2", 212, 255, 238),
            new("magenta3", 212, 255, 205),
            new("magenta4", 212, 255, 139),
            new("maroon", 239, 185, 176),
            new("maroon1", 228, 203, 255),
            new("maroon2", 228, 203, 238),
            new("maroon3", 228, 204, 205),
            new("maroon4", 228, 203, 139),
            new("mediumaquamarine", 113, 128, 205),
            new("mediumblue", 170, 255, 205),
            new("mediumorchid", 204, 152, 211),
            new("mediumorchid1", 203, 153, 255),
            new("mediumorchid2", 203, 153, 238),
            new("mediumorchid3", 203, 153, 205),
            new("mediumorchid4", 203, 154, 139),
            new("mediumpurple", 183, 124, 219),
            new("mediumpurple1", 183, 125, 255),
            new("mediumpurple2", 183, 125, 238),
            new("mediumpurple3", 183, 125, 205),
            new("mediumpurple4", 183, 124, 139),
            new("mediumseagreen", 103, 169, 179),
            new("mediumslateblue", 176, 143, 238),
            new("mediumspringgreen", 111, 255, 250),
            new("mediumturquoise", 125, 167, 209),
            new("mediumvioletred", 228, 228, 199),
            new("midnightblue", 170, 198, 112),
            new("mintcream", 106, 9, 255),
            new("mistyrose", 4, 30, 255),
            new("mistyrose1", 4, 30, 255),
            new("mistyrose2", 4, 30, 238),
            new("mistyrose3", 3, 29, 205),
            new("mistyrose4", 5, 29, 139),
            new("moccasin", 26, 73, 255),
            new("navajowhite", 25, 81, 255),
            new("navajowhite1", 25, 81, 255),
            new("navajowhite2", 25, 82, 238),
            new("navajowhite3", 25, 82, 205),
            new("navajowhite4", 25, 82, 139),
            new("navy", 170, 255, 128),
            new("navyblue", 170, 255, 128),
            new("oldlace", 27, 23, 253),
            new("olivedrab", 56, 192, 142),
            new("olivedrab1", 56, 193, 255),
            new("olivedrab2", 56, 192, 238),
            new("olivedrab3", 56, 192, 205),
            new("olivedrab4", 56, 192, 139),
            new("orange", 27, 255, 255),
            new("orange1", 27, 255, 255),
            new("orange2", 27, 255, 238),
            new("orange3", 27, 255, 205),
            new("orange4", 27, 255, 139),
            new("orangered", 11, 255, 255),
            new("orangered1", 11, 255, 255),
            new("orangered2", 11, 255, 238),
            new("orangered3", 11, 255, 205),
            new("orangered4", 11, 255, 139),
            new("orchid", 214, 123, 218),
            new("orchid1", 214, 124, 255),
            new("orchid2", 214, 124, 238),
            new("orchid3", 214, 124, 205),
            new("orchid4", 213, 124, 139),
            new("palegoldenrod", 38, 72, 238),
            new("palegreen", 85, 100, 251),
            new("palegreen1", 85, 101, 255),
            new("palegreen2", 85, 100, 238),
            new("palegreen3", 85, 100, 205),
            new("palegreen4", 85, 100, 139),
            new("paleturquoise", 127, 67, 238),
            new("paleturquoise1", 127, 68, 255),
            new("paleturquoise2", 127, 68, 238),
            new("paleturquoise3", 127, 68, 205),
            new("paleturquoise4", 127, 67, 139),
            new("palevioletred", 241, 124, 219),
            new("palevioletred1", 241, 125, 255),
            new("palevioletred2", 241, 125, 238),
            new("palevioletred3", 241, 125, 205),
            new("palevioletred4", 241, 124, 139),
            new("papayawhip", 26, 41, 255),
            new("peachpuff", 20, 70, 255),
            new("peachpuff1", 20, 70, 255),
            new("peachpuff2", 19, 69, 238),
            new("peachpuff3", 19, 69, 205),
            new("peachpuff4", 20, 69, 139),
            new("peru", 20, 176, 205),
            new("pink", 247, 63, 255),
            new("pink1", 245, 73, 255),
            new("pink2", 245, 73, 238),
            new("pink3", 245, 74, 205),
            new("pink4", 245, 73, 139),
            new("plum", 212, 70, 221),
            new("plum1", 212, 68, 255),
            new("plum2", 212, 68, 238),
            new("plum3", 212, 68, 205),
            new("plum4", 212, 67, 139),
            new("powderblue", 132, 59, 230),
            new("purple", 196, 221, 240),
            new("purple1", 191, 207, 255),
            new("purple2", 192, 207, 238),
            new("purple3", 192, 207, 205),
            new("purple4", 192, 207, 139),
            new("red", 0, 255, 255),
            new("red1", 0, 255, 255),
            new("red2", 0, 255, 238),
            new("red3", 0, 255, 205),
            new("red4", 0, 255, 139),
            new("rosybrown", 0, 61, 188),
            new("rosybrown1", 0, 62, 255),
            new("rosybrown2", 0, 62, 238),
            new("rosybrown3", 0, 62, 205),
            new("rosybrown4", 0, 62, 139),
            new("royalblue", 159, 181, 225),
            new("royalblue1", 159, 183, 255),
            new("royalblue2", 159, 183, 238),
            new("royalblue3", 159, 182, 205),
            new("royalblue4", 159, 183, 139),
            new("saddlebrown", 17, 220, 139),
            new("salmon", 4, 138, 250),
            new("salmon1", 9, 150, 255),
            new("salmon2", 9, 150, 238),
            new("salmon3", 9, 150, 205),
            new("salmon4", 9, 150, 139),
            new("sandybrown", 19, 154, 244),
            new("seagreen", 103, 170, 139),
            new("seagreen1", 103, 171, 255),
            new("seagreen2", 103, 171, 238),
            new("seagreen3", 103, 171, 205),
            new("seagreen4", 103, 170, 139),
            new("seashell", 17, 16, 255),
            new("seashell1", 17, 16, 255),
            new("seashell2", 18, 17, 238),
            new("seashell3", 18, 17, 205),
            new("seashell4", 18, 16, 139),
            new("sienna", 13, 183, 160),
            new("sienna1", 13, 184, 255),
            new("sienna2", 13, 184, 238),
            new("sienna3", 13, 184, 205),
            new("sienna4", 13, 185, 139),
            new("skyblue", 139, 108, 235),
            new("skyblue1", 144, 120, 255),
            new("skyblue2", 144, 120, 238),
            new("skyblue3", 144, 120, 205),
            new("skyblue4", 145, 119, 139),
            new("slateblue", 175, 143, 205),
            new("slateblue1", 175, 144, 255),
            new("slateblue2", 175, 144, 238),
            new("slateblue3", 175, 144, 205),
            new("slateblue4", 175, 144, 139),
            new("slategray", 148, 56, 144),
            new("slategray1", 149, 56, 255),
            new("slategray2", 149, 56, 238),
            new("slategray3", 148, 57, 205),
            new("slategray4", 149, 56, 139),
            new("slategrey", 148, 56, 144),
            new("snow", 0, 5, 255),
            new("snow1", 0, 5, 255),
            new("snow2", 0, 5, 238),
            new("snow3", 0, 4, 205),
            new("snow4", 0, 3, 139),
            new("springgreen", 106, 255, 255),
            new("springgreen1", 106, 255, 255),
            new("springgreen2", 106, 255, 238),
            new("springgreen3", 106, 255, 205),
            new("springgreen4", 106, 255, 139),
            new("steelblue", 146, 155, 180),
            new("steelblue1", 146, 156, 255),
            new("steelblue2", 146, 156, 238),
            new("steelblue3", 146, 156, 205),
            new("steelblue4", 147, 155, 139),
            new("tan", 24, 84, 210),
            new("tan1", 20, 176, 255),
            new("tan2", 20, 176, 238),
            new("tan3", 20, 176, 205),
            new("tan4", 20, 176, 139),
            new("thistle", 212, 29, 216),
            new("thistle1", 212, 30, 255),
            new("thistle2", 212, 30, 238),
            new("thistle3", 212, 29, 205),
            new("thistle4", 212, 29, 139),
            new("tomato", 6, 184, 255),
            new("tomato1", 6, 184, 255),
            new("tomato2", 6, 184, 238),
            new("tomato3", 6, 184, 205),
            new("tomato4", 6, 185, 139),
            new("turquoise", 123, 182, 224),
            new("turquoise1", 129, 255, 255),
            new("turquoise2", 129, 255, 238),
            new("turquoise3", 129, 255, 205),
            new("turquoise4", 129, 255, 139),
            new("violet", 212, 115, 238),
            new("violetred", 227, 215, 208),
            new("violetred1", 235, 193, 255),
            new("violetred2", 235, 192, 238),
            new("violetred3", 235, 192, 205),
            new("violetred4", 235, 192, 139),
            new("wheat", 27, 68, 245),
            new("wheat1", 27, 69, 255),
            new("wheat2", 27, 68, 238),
            new("wheat3", 27, 68, 205),
            new("wheat4", 27, 67, 139),
            new("white", 0, 0, 255),
            new("whitesmoke", 0, 0, 245),
            new("yellow", 42, 255, 255),
            new("yellow1", 42, 255, 255),
            new("yellow2", 42, 255, 238),
            new("yellow3", 42, 255, 205),
            new("yellow4", 42, 255, 139),
            new("yellowgreen", 56, 192, 205)
        };

        public static TEDGraphVisualization Current;

        private readonly DictionaryWithDefault<string, EdgeStyle> _edgeStyles;
        private readonly DictionaryWithDefault<string, NodeStyle> _nodeStyles;

        static TEDGraphVisualization() => NonStandardColorNames = new Dictionary<string, Color>(
                                              DotColors.Select(c => new KeyValuePair<string, Color>(c.Name, c.Color)));
        public TEDGraphVisualization() {
            _nodeStyles = new DictionaryWithDefault<string, NodeStyle>(MakeNodeStyle);
            _edgeStyles = new DictionaryWithDefault<string, EdgeStyle>(MakeEdgeStyle);
            Current = this;
        }

        private NodeStyle MakeNodeStyle(string colorName) {
            var s = NodeStyles[0].Clone();
            var f = typeof(Color).GetField(
                colorName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (f == null) {
                if (NonStandardColorNames.TryGetValue(colorName, out var namedColor)) s.Color = namedColor;
                else Debug.Log($"No such color as {colorName}");
            } else s.Color = (Color)f.GetValue(null);
            return s;
        }

        private EdgeStyle MakeEdgeStyle(string colorName) {
            var s = EdgeStyles[0].Clone();
            var f = typeof(Color).GetField(colorName, BindingFlags.Static | BindingFlags.IgnoreCase);
            if (f == null) {
                if (NonStandardColorNames.TryGetValue(colorName, out var namedColor)) s.Color = namedColor;
                else Debug.Log($"No such color as {colorName}");
            } else s.Color = (Color)f.GetValue(null);
            return s;
        }

        private void SetGraph<T>(GraphViz<T> g) {
            GUIManager.ChangeTable = false; // hide buttons
            Clear();
            EnsureEdgeNodesInGraph(g);
            if (g.Nodes.Count == 0) return;

            foreach (var n in g.Nodes) {
                var attrs = g.NodeAttributes[n];
                var nodeStyle = NodeStyles[0];
                if (attrs != null && attrs.TryGetValue("fillcolor", out var attr)) nodeStyle = _nodeStyles[(string)attr];
                if (attrs != null && attrs.TryGetValue("rgbcolor", out var rgbcolor)) nodeStyle = (Color)rgbcolor;
                AddNode(n, n.ToString(), nodeStyle);
            }
            foreach (var e in g.Edges) {
                var attrs = e.Attributes;
                var edgeStyle = EdgeStyles[0];
                if (attrs != null && attrs.TryGetValue("color", out var attr)) edgeStyle = _edgeStyles[(string)attr];
                if (attrs != null && attrs.TryGetValue("rgbcolor", out var rgbcolor)) edgeStyle = (Color)rgbcolor;
                AddEdge(e.StartNode, e.EndNode, e.Label, edgeStyle);
            }
            UpdateTopologyStats();
            PlaceComponents();
            RepopulateMesh();
        }

        private static void EnsureEdgeNodesInGraph<T>(GraphViz<T> g) {
            foreach (var e in g.Edges) {
                if (!g.Nodes.Contains(e.StartNode)) g.AddNode(e.StartNode);
                if (!g.Nodes.Contains(e.EndNode)) g.AddNode(e.EndNode);
            }
        }

        public static void ShowGraph<T>(GraphViz<T> g) => FindObjectOfType<TEDGraphVisualization>().SetGraph(g);

        public static void SetTableDescription() => SetDescriptionMethod<TablePredicate>(TableDescription);

        private static string TableDescription(TablePredicate p) {
            var b = new StringBuilder();
            b.Append("<b>");
            b.AppendLine(p.DefaultGoal.ToString().Replace("[", "</b>["));
            b.AppendFormat("{0} rows\n", p.Length);
            b.Append("<size=16>");
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (p.UpdateMode) {
                case UpdateMode.BaseTable:
                    b.Append("Base table");
                    break;
                case UpdateMode.Operator:
                    b.Append("Operator result");
                    break;
                default:
                    if (p.Rules != null)
                        foreach (var r in p.Rules) b.AppendLine(r.ToString());
                    break;
            }
            return b.ToString();
        }

        public static GraphViz<TGraph> TraceToDepth<TGraph, T>(int maxDepth, T start, Func<T, IEnumerable<(
                                                                   T neighbor, string label, string color)>> edges) where T : TGraph {
            var g = new GraphViz<TGraph>();

            void Walk(T node, int depth) {
                if (depth > maxDepth || g.Nodes.Contains(node)) return;
                g.AddNode(node);
                foreach (var edge in edges(node)) {
                    Walk(edge.neighbor, depth + 1);
                    g.AddEdge(new GraphViz<TGraph>.Edge(node, edge.neighbor, true, edge.label,
                                                        new Dictionary<string, object> { { "color", edge.color } }));
                }
            }

            Walk(start, 0);
            return g;
        }

        private readonly struct DotColor {
            public readonly string Name;
            public readonly Color Color;

            public DotColor(string name, int h, int s, int v) {
                Name = name;
                Color = Color.HSVToRGB(h / 255f, s / 255f, v / 255f);
            }
        }
    }
}
