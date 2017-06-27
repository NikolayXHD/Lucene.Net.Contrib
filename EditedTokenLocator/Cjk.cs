using System.Collections.Generic;
using System.Linq;

namespace Lucene.Net.Contrib
{
	internal static class Cjk
	{
		public static bool IsCjk(this string s)
		{
			return s.Any(IsCjk);
		}

		/// <summary>
		/// Символ является иероглифом.
		/// Аббревиатура CJK в названии расшифровывается как chinese japanese korean
		/// </summary>
		public static bool IsCjk(this char c)
		{
			var rangeIndex = CjkCharacterRanges.BinarySearchFirstIndexOf(r => r.Max >= c);

			if (rangeIndex < 0)
				return false;

			if (CjkCharacterRanges[rangeIndex].Min <= c)
				return true;

			return false;
		}

		/// <summary>
		/// Диапазоны иероглифов.
		/// Аббревиатура CJK в названии расшифровывается как chinese japanese korean.
		/// https://docs.microsoft.com/en-us/dotnet/standard/base-types/character-classes-in-regular-expressions
		/// 
		/// Диапазоны упорядочены по-возрастанию для оптимизации поиска
		/// </summary>
		private static readonly List<CharRange> CjkCharacterRanges = new List<CharRange>
		{
			new CharRange('\u1100', '\u11FF'), // IsHangulJamo
			new CharRange('\u2E80', '\u2EFF'), // IsCJKRadicalsSupplement

			// new CharRange('\u3000', '\u303F'), // IsCJKSymbolsandPunctuation
			// new CharRange('\u3040', '\u309F'), // Hiragana
			// new CharRange('\u30A0', '\u30FF'), // Katakana
			// new CharRange('\u3100', '\u312F'), // Bopomofo
			// new CharRange('\u3130', '\u318F'), // Hangul Compatibility Jamo
			// new CharRange('\u3190', '\u319F'), // Kanbun
			// new CharRange('\u31A0', '\u31BF'), // Bopomofo Extended
			new CharRange('\u3000', '\u31BF'), // слиты 7 подряд идущих диапазона выше

			//new CharRange('\u31F0', '\u31FF'), // Katakana Phonetic Extensions
			//new CharRange('\u3200', '\u32FF'), // IsEnclosedCJKLettersandMonths
			//new CharRange('\u3300', '\u33FF'), // IsCJKCompatibility
			//new CharRange('\u3400', '\u4DBF'), // IsCJKUnifiedIdeographsExtensionA
			new CharRange('\u31F0', '\u4DBF'), // слиты 4 подряд идущих диапазона выше

			new CharRange('\u4E00', '\u9FFF'), // IsCJKUnifiedIdeographs
			new CharRange('\uAC00', '\uD7AF'), // IsHangulSyllables
			new CharRange('\uF900', '\uFAFF'), // IsCJKCompatibilityIdeographs
			new CharRange('\uFE30', '\uFE4F'), // IsCJKCompatibilityForms
			new CharRange('\uFF00', '\uFFEF') // Halfwidth and Fullwidth Forms
		};

		private struct CharRange
		{
			public readonly char Min;
			public readonly char Max;

			public CharRange(char min, char max)
			{
				Min = min;
				Max = max;
			}
		}
	}
}