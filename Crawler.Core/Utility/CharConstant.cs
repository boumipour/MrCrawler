using System.Collections.Generic;

namespace Utility.Extensions
{
    public static class CharConstant
    {
        /// <summary>
        /// ؠ.
        /// </summary>
        public const char ArabicYeWithOneDotBelow = (char)1568;

        /// <summary>
        /// ؽ.
        /// </summary>
        public const char ArabicYeWithInvertedV = (char)1597;

        /// <summary>
        /// ؾ.
        /// </summary>
        public const char ArabicYeWithTwoDotsAbove = (char)1598;

        /// <summary>
        /// ؿ.
        /// </summary>
        public const char ArabicYeWithThreeDotsAbove = (char)1599;

        /// <summary>
        /// ٸ.
        /// </summary>
        public const char ArabicYeWithHighHamzeYeh = (char)1656;

        /// <summary>
        /// ې.
        /// </summary>
        public const char ArabicYeWithFinalForm = (char)1744;

        /// <summary>
        /// ۑ.
        /// </summary>
        public const char ArabicYeWithThreeDotsBelow = (char)1745;

        /// <summary>
        /// ۍ.
        /// </summary>
        public const char ArabicYeWithTail = (char)1741;

        /// <summary>
        /// ێ.
        /// </summary>
        public const char ArabicYeSmallV = (char)1742;

        /// <summary>
        /// Arabic Ke Char \u0643 = ARABIC LETTER KAF.
        /// </summary>
        public const char ArabicKeChar = '\u0643';

        /// <summary>
        /// Arabic Ye Char \u0649 = ARABIC LETTER ALEF MAKSURA.
        /// </summary>
        public const char ArabicYeChar1 = '\u0649';

        /// <summary>
        /// Arabic Ye Char \u064A = ARABIC LETTER YEH.
        /// </summary>
        public const char ArabicYeChar2 = '\u064A';

        /// <summary>
        /// Persian Ke Char \u06A9 = ARABIC LETTER KEHEH.
        /// </summary>
        public const char PersianKeChar = '\u06A9';

        public const char PersianYeChar = '\u06CC';

        public const char Latin0 = '\u0030';
        public const char Latin1 = '\u0031';
        public const char Latin2 = '\u0032';
        public const char Latin3 = '\u0033';
        public const char Latin4 = '\u0034';
        public const char Latin5 = '\u0035';
        public const char Latin6 = '\u0036';
        public const char Latin7 = '\u0037';
        public const char Latin8 = '\u0038';
        public const char Latin9 = '\u0039';

        public const char Farsi0 = '\u06F0';
        public const char Farsi1 = '\u06F1';
        public const char Farsi2 = '\u06F2';
        public const char Farsi3 = '\u06F3';
        public const char Farsi4 = '\u06F4';
        public const char Farsi5 = '\u06F5';
        public const char Farsi6 = '\u06F6';
        public const char Farsi7 = '\u06F7';
        public const char Farsi8 = '\u06F8';
        public const char Farsi9 = '\u06F9';

        public const char Arabic0 = '\u0660';
        public const char Arabic1 = '\u0661';
        public const char Arabic2 = '\u0662';
        public const char Arabic3 = '\u0663';
        public const char Arabic4 = '\u0664';
        public const char Arabic5 = '\u0665';
        public const char Arabic6 = '\u0666';
        public const char Arabic7 = '\u0667';
        public const char Arabic8 = '\u0668';
        public const char Arabic9 = '\u0669';

        public static Dictionary<char, char> DigitCharMaps => new()
        {
            { Farsi0, Latin0 },
            { Farsi1, Latin1 },
            { Farsi2, Latin2 },
            { Farsi3, Latin3 },
            { Farsi4, Latin4 },
            { Farsi5, Latin5 },
            { Farsi6, Latin6 },
            { Farsi7, Latin7 },
            { Farsi8, Latin8 },
            { Farsi9, Latin9 },
            { Arabic0, Latin0 },
            { Arabic1, Latin1 },
            { Arabic2, Latin2 },
            { Arabic3, Latin3 },
            { Arabic4, Latin4 },
            { Arabic5, Latin5 },
            { Arabic6, Latin6 },
            { Arabic7, Latin7 },
            { Arabic8, Latin8 },
            { Arabic9, Latin9 },
        };

        public static Dictionary<char, char> YeKeCharMaps => new()
        {
            { ArabicYeChar1, PersianYeChar },
            { ArabicYeChar2, PersianYeChar },
            { ArabicYeWithOneDotBelow, PersianYeChar },
            { ArabicYeWithInvertedV, PersianYeChar },
            { ArabicYeWithTwoDotsAbove, PersianYeChar },
            { ArabicYeWithThreeDotsAbove, PersianYeChar },
            { ArabicYeWithHighHamzeYeh, PersianYeChar },
            { ArabicYeWithFinalForm, PersianYeChar },
            { ArabicYeWithThreeDotsBelow, PersianYeChar },
            { ArabicYeWithTail, PersianYeChar },
            { ArabicYeSmallV, PersianYeChar },
            { ArabicKeChar, PersianKeChar },
        };
    }
}