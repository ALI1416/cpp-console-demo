﻿namespace ConsoleDemo.Model
{

    /// <summary>
    /// 版本(编码模式 BYTE)
    /// </summary>
    public class Version
    {

        /// <summary>
        /// 版本号
        /// <para>[1,40]</para>
        /// </summary>
        public readonly int VersionNumber;
        /// <summary>
        /// 尺寸
        /// <para>尺寸 = (版本号 - 1) * 4 + 21</para>
        /// <para>[21,177]</para>
        /// </summary>
        public readonly int Dimension;
        /// <summary>
        /// 内容字节数
        /// </summary>
        public readonly int ContentBytes;
        /// <summary>
        /// 数据和纠错bit数
        /// <para>数据bit数+纠错bit数</para>
        /// </summary>
        public readonly int DataAndEcBits;
        /// <summary>
        /// `内容字节数`bit数
        /// <para>8或16</para>
        /// </summary>
        public readonly int ContentBytesBits;
        /// <summary>
        /// 数据bit数
        /// <para>编码模式(4bit)+`内容字节数`bit数+内容bit数+结束符(4bit)</para>
        /// </summary>
        public readonly int DataBits;
        /// <summary>
        /// 纠错字节数
        /// </summary>
        public readonly int EcBytes;
        /// <summary>
        /// 纠错
        /// <para>[纠错块,(块数量,纠错码)]</para>
        /// </summary>
        public readonly int[,] Ec;
        /// <summary>
        /// 纠错块数
        /// </summary>
        public readonly int EcBlocks;

        /// <summary>
        /// 构造版本
        /// <para>`内容字节数`过长`VersionNumber`值为`0`</para>
        /// </summary>
        /// <param name="length">
        /// 内容字节数
        /// </param>
        /// <param name="level">
        /// 纠错等级
        /// <para>0 L 7%</para>
        /// <para>1 M 15%</para>
        /// <para>2 Q 25%</para>
        /// <para>3 H 30%</para>
        /// </param>
        public Version(int length, int level)
        {
            for (int i = 1; i < 41; i++)
            {
                if (length <= CONTENT_BYTES[i - 1, level])
                {
                    ContentBytes = CONTENT_BYTES[i - 1, level];
                    VersionNumber = i;
                    break;
                }
            }
            Dimension = (VersionNumber - 1) * 4 + 21;
            // `内容字节数`bit数 1-9版本8bit 10-40版本16bit
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 3 -> Byte mode列
            ContentBytesBits = VersionNumber < 10 ? 8 : 16;
            DataAndEcBits = DATA_AND_EC_BITS[VersionNumber - 1];
            // 编码模式(4bit)+`内容字节数`bit数+内容bit数+结束符(4bit)
            DataBits = 4 + ContentBytesBits + ContentBytes * 8 + 4;
            EcBytes = (DataAndEcBits - DataBits) / 8;
            Ec = EC[VersionNumber - 1, level];
            for (int i = 0; i < Ec.GetLength(0); i++)
            {
                EcBlocks += Ec[i, 0];
            }
        }

        /// <summary>
        /// 数据bit数+纠错bit数
        /// <para>索引[版本号]:40</para>
        /// <para>数据来源 ISO/IEC 18004-2015 -> 7.1 -> Table 1 -> Data modules except(C)列</para>
        /// </summary>
        private static readonly int[] DATA_AND_EC_BITS =
        {
              208,   359,   567,   807,  1079, // 1-5
             1383,  1568,  1936,  2336,  2768, // 6-10
             3232,  3728,  4256,  4651,  5243, // 11-15
             5867,  6523,  7211,  7931,  8683, // 16-20
             9252, 10068, 10916, 11796, 12708, // 21-25
            13652, 14628, 15371, 16411, 17483, // 26-30
            18587, 19723, 20891, 22091, 23008, // 31-35
            24272, 25568, 26896, 28256, 29648, // 36-40
        };

        /// <summary>
        /// 内容字节数
        /// <para>索引[版本号,纠错等级]:40x4</para>
        /// <para>数据来源 ISO/IEC 18004-2015 -> 7.4.10 -> Table 7 -> Data capacity列 -> Byte列</para>
        /// </summary>
        private static readonly int[,] CONTENT_BYTES =
        {
            {   17,   14,   11,    7 }, {   32,   26,   20,   14 }, {   53,   42,   32,   24 }, {   78,   62,   46,   34 }, {  106,   84,   60,   44 }, // 1-5
            {  134,  106,   74,   58 }, {  154,  122,   86,   64 }, {  192,  152,  108,   84 }, {  230,  180,  130,   98 }, {  271,  213,  151,  119 }, // 6-10
            {  321,  251,  177,  137 }, {  367,  287,  203,  155 }, {  425,  331,  241,  177 }, {  458,  362,  258,  194 }, {  520,  412,  292,  220 }, // 11-15
            {  586,  450,  322,  250 }, {  644,  504,  364,  280 }, {  718,  560,  394,  310 }, {  792,  624,  442,  338 }, {  858,  666,  482,  382 }, // 16-20
            {  929,  711,  509,  403 }, { 1003,  779,  565,  439 }, { 1091,  857,  611,  461 }, { 1171,  911,  661,  511 }, { 1273,  997,  715,  535 }, // 21-25
            { 1367, 1059,  751,  593 }, { 1465, 1125,  805,  625 }, { 1528, 1190,  868,  658 }, { 1628, 1264,  908,  698 }, { 1732, 1370,  982,  742 }, // 26-30
            { 1840, 1452, 1030,  790 }, { 1952, 1538, 1112,  842 }, { 2068, 1628, 1168,  898 }, { 2188, 1722, 1228,  958 }, { 2303, 1809, 1283,  983 }, // 31-35
            { 2431, 1911, 1351, 1051 }, { 1591, 2563, 1989, 1423 }, { 2699, 2099, 1499, 1139 }, { 2809, 2213, 1579, 1219 }, { 2953, 2331, 1663, 1273 }, // 36-40
        };

        /// <summary>
        /// 纠错
        /// <para>索引[版本号,纠错等级][纠错块,(块数量,纠错码)]:40x4x?x2</para>
        /// <para>数据来源 ISO/IEC 18004-2015 -> 7.5.1 -> Table 9 -> Number of error correction blocks列 和 Error correction code per block列的k</para>
        /// </summary>
        private static readonly int[,][,] EC =
        {
            // 1
            {
                new int[,] { {  1,  19 } },
                new int[,] { {  1,  16 } },
                new int[,] { {  1,  13 } },
                new int[,] { {  1,   9 } },
            },
            // 2
            {
                new int[,] { {  1,  34 } },
                new int[,] { {  1,  28 } },
                new int[,] { {  1,  22 } },
                new int[,] { {  1,  16 } },
            },
            // 3
            {
                new int[,] { {  1,  55 } },
                new int[,] { {  1,  44 } },
                new int[,] { {  2,  17 } },
                new int[,] { {  2,  13 } },
            },
            // 4
            {
                new int[,] { {  1,  80 } },
                new int[,] { {  2,  32 } },
                new int[,] { {  2,  24 } },
                new int[,] { {  4,   9 } },
            },
            // 5
            {
                new int[,] { {  1, 108 } },
                new int[,] { {  2,  43 } },
                new int[,] { {  2,  15 }, {  2,  16 } },
                new int[,] { {  2,  11 }, {  2,  12 } },
            },
            // 6
            {
                new int[,] { {  2,  68 } },
                new int[,] { {  4,  27 } },
                new int[,] { {  4,  19 } },
                new int[,] { {  4,  15 } },
            },
            // 7
            {
                new int[,] { {  2,  78 } },
                new int[,] { {  4,  31 } },
                new int[,] { {  2,  14 }, {  4,  15 } },
                new int[,] { {  4,  13 }, {  1,  14 } },
            },
            // 8
            {
                new int[,] { {  2,  97 } },
                new int[,] { {  2,  38 }, {  2,  39 } },
                new int[,] { {  4,  18 }, {  2,  19 } },
                new int[,] { {  4,  14 }, {  2,  15 } },
            },
            // 9
            {
                new int[,] { {  2, 116 } },
                new int[,] { {  3,  36 }, {  2,  37 } },
                new int[,] { {  4,  16 }, {  4,  17 } },
                new int[,] { {  4,  12 }, {  4,  13 } },
            },
            // 10
            {
                new int[,] { {  2,  68 }, {  2,  69 } },
                new int[,] { {  4,  43 }, {  1,  44 } },
                new int[,] { {  6,  19 }, {  2,  20 } },
                new int[,] { {  6,  15 }, {  2,  16 } },
            },
            // 11
            {
                new int[,] { {  4,  81 } },
                new int[,] { {  1,  50 }, {  4,  51 } },
                new int[,] { {  4,  22 }, {  4,  23 } },
                new int[,] { {  3,  12 }, {  8,  13 } },
            },
            // 12
            {
                new int[,] { {  2,  92 }, {  2,  93 } },
                new int[,] { {  6,  36 }, {  2,  37 } },
                new int[,] { {  4,  20 }, {  6,  21 } },
                new int[,] { {  7,  14 }, {  4,  15 } },
            },
            // 13
            {
                new int[,] { {  4, 107 } },
                new int[,] { {  8,  37 }, {  1,  38 } },
                new int[,] { {  8,  20 }, {  4,  21 } },
                new int[,] { { 12,  11 }, {  4,  12 } },
            },
            // 14
            {
                new int[,] { {  3, 115 }, {  1, 116 } },
                new int[,] { {  4,  40 }, {  5,  41 } },
                new int[,] { { 11,  16 }, {  5,  17 } },
                new int[,] { { 11,  12 }, {  5,  13 } },
            },
            // 15
            {
                new int[,] { {  5,  87 }, {  1,  88 } },
                new int[,] { {  5,  41 }, {  5,  42 } },
                new int[,] { {  5,  24 }, {  7,  25 } },
                new int[,] { { 11,  12 }, {  7,  13 } },
            },
            // 16
            {
                new int[,] { {  5,  98 }, {  1,  99 } },
                new int[,] { {  7,  45 }, {  3,  46 } },
                new int[,] { { 15,  19 }, {  2,  20 } },
                new int[,] { {  3,  15 }, { 13,  16 } },
            },
            // 17
            {
                new int[,] { {  1, 107 }, {  5, 108 } },
                new int[,] { { 10,  46 }, {  1,  47 } },
                new int[,] { {  1,  22 }, { 15,  23 } },
                new int[,] { {  2,  14 }, { 17,  15 } },
            },
            // 18
            {
                new int[,] { {  5, 120 }, {  1, 121 } },
                new int[,] { {  9,  43 }, {  4,  44 } },
                new int[,] { { 17,  22 }, {  1,  23 } },
                new int[,] { {  2,  14 }, { 19,  15 } },
            },
            // 19
            {
                new int[,] { {  3, 113 }, {  4, 114 } },
                new int[,] { {  3,  44 }, { 11,  45 } },
                new int[,] { { 17,  21 }, {  4,  22 } },
                new int[,] { {  9,  13 }, { 16,  14 } },
            },
            // 20
            {
                new int[,] { {  3, 107 }, {  5, 108 } },
                new int[,] { {  3,  41 }, { 13,  42 } },
                new int[,] { { 15,  24 }, {  5,  25 } },
                new int[,] { { 15,  15 }, { 10,  16 } },
            },
            // 21
            {
                new int[,] { {  4, 116 }, {  4, 117 } },
                new int[,] { { 17,  42 } },
                new int[,] { { 17,  22 }, {  6,  23 } },
                new int[,] { { 19,  16 }, {  6,  17 } },
            },
            // 22
            {
                new int[,] { {  2, 111 }, {  7, 112 } },
                new int[,] { { 17,  46 } },
                new int[,] { {  7,  24 }, { 16,  25 } },
                new int[,] { { 34,  13 } },
            },
            // 23
            {
                new int[,] { {  4, 121 }, {  5, 122 } },
                new int[,] { {  4,  47 }, { 14,  48 } },
                new int[,] { { 11,  24 }, { 14,  25 } },
                new int[,] { { 16,  15 }, { 14,  16 } },
            },
            // 24
            {
                new int[,] { {  6, 117 }, {  4, 118 } },
                new int[,] { {  6,  45 }, { 14,  46 } },
                new int[,] { { 11,  24 }, { 16,  25 } },
                new int[,] { { 30,  16 }, {  2,  17 } },
            },
            // 25
            {
                new int[,] { {  8, 106 }, {  4, 107 } },
                new int[,] { {  8,  47 }, { 13,  48 } },
                new int[,] { {  7,  24 }, { 22,  25 } },
                new int[,] { { 22,  15 }, { 13,  16 } },
            },
            // 26
            {
                new int[,] { { 10, 114 }, {  2, 115 } },
                new int[,] { { 19,  46 }, {  4,  47 } },
                new int[,] { { 28,  22 }, {  6,  23 } },
                new int[,] { { 33,  16 }, {  4,  17 } },
            },
            // 27
            {
                new int[,] { {  8, 122 }, {  4, 123 } },
                new int[,] { { 22,  45 }, {  3,  46 } },
                new int[,] { {  8,  23 }, { 26,  24 } },
                new int[,] { { 12,  15 }, { 28,  16 } },
            },
            // 28
            {
                new int[,] { {  3, 117 }, { 10, 118 } },
                new int[,] { {  3,  45 }, { 23,  46 } },
                new int[,] { {  4,  24 }, { 31,  25 } },
                new int[,] { { 11,  15 }, { 31,  16 } },
            },
            // 29
            {
                new int[,] { {  7, 116 }, {  7, 117 } },
                new int[,] { { 21,  45 }, {  7,  46 } },
                new int[,] { {  1,  23 }, { 37,  24 } },
                new int[,] { { 19,  15 }, { 26,  16 } },
            },
            // 30
            {
                new int[,] { {  5, 115 }, { 10, 116 } },
                new int[,] { { 19,  47 }, { 10,  48 } },
                new int[,] { { 15,  24 }, { 25,  25 } },
                new int[,] { { 23,  15 }, { 25,  16 } },
            },
            // 31
            {
                new int[,] { { 13, 115 }, {  3, 116 } },
                new int[,] { {  2,  46 }, { 29,  47 } },
                new int[,] { { 42,  24 }, {  1,  25 } },
                new int[,] { { 23,  15 }, { 28,  16 } },
            },
            // 32
            {
                new int[,] { { 17, 115 } },
                new int[,] { { 10,  46 }, { 23,  47 } },
                new int[,] { { 10,  24 }, { 35,  25 } },
                new int[,] { { 19,  15 }, { 35,  16 } },
            },
            // 33
            {
                new int[,] { { 17, 115 }, {  1, 116 } },
                new int[,] { { 14,  46 }, { 21,  47 } },
                new int[,] { { 29,  24 }, { 19,  25 } },
                new int[,] { { 11,  15 }, { 46,  16 } },
            },
            // 34
            {
                new int[,] { { 13, 115 }, {  6, 116 } },
                new int[,] { { 14,  46 }, { 23,  47 } },
                new int[,] { { 44,  24 }, {  7,  25 } },
                new int[,] { { 59,  16 }, {  1,  17 } },
            },
            // 35
            {
                new int[,] { { 12, 121 }, {  7, 122 } },
                new int[,] { { 12,  47 }, { 26,  48 } },
                new int[,] { { 39,  24 }, { 14,  25 } },
                new int[,] { { 22,  15 }, { 41,  16 } },
            },
            // 36
            {
                new int[,] { {  6, 121 }, { 14, 122 } },
                new int[,] { {  6,  47 }, { 34,  48 } },
                new int[,] { { 46,  24 }, { 10,  25 } },
                new int[,] { {  2,  15 }, { 64,  16 } },
            },
            // 37
            {
                new int[,] { { 17, 122 }, {  4, 123 } },
                new int[,] { { 29,  46 }, { 14,  47 } },
                new int[,] { { 49,  24 }, { 10,  25 } },
                new int[,] { { 24,  15 }, { 46,  16 } },
            },
            // 38
            {
                new int[,] { {  4, 122 }, { 18, 123 } },
                new int[,] { { 13,  46 }, { 32,  47 } },
                new int[,] { { 48,  24 }, { 14,  25 } },
                new int[,] { { 42,  15 }, { 32,  16 } },
            },
            // 39
            {
                new int[,] { { 20, 117 }, {  4, 118 } },
                new int[,] { { 40,  47 }, {  7,  48 } },
                new int[,] { { 43,  24 }, { 22,  25 } },
                new int[,] { { 10,  15 }, { 67,  16 } },
            },
            // 40
            {
                new int[,] { { 19, 118 }, {  6, 119 } },
                new int[,] { { 18,  47 }, { 31,  48 } },
                new int[,] { { 34,  24 }, { 34,  25 } },
                new int[,] { { 20,  15 }, { 61,  16 } },
            },
       };

    }
}
