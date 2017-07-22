using System.Collections.Generic;
using System.Linq;

namespace Lucene.Net.Contrib
{
	public static class Cj
	{
		/// <summary>
		/// Символ является иероглифом.
		/// Аббревиатура CJ в названии расшифровывается как chinese japanese
		/// </summary>
		public static bool IsCj(this char c)
		{
			var rangeIndex = CjCharacterRanges.BinarySearchFirstIndexOf(r => r.Max >= c);

			if (rangeIndex < 0)
				return false;

			if (CjCharacterRanges[rangeIndex].Min <= c)
				return true;

			return false;
		}

		/// <summary>
		/// Диапазоны иероглифов.
		/// Аббревиатура CJ в названии расшифровывается как chinese japanese.
		/// https://en.wikipedia.org/wiki/Unicode_block
		/// 
		/// Диапазоны упорядочены по-возрастанию для оптимизации поиска
		/// </summary>
		private static readonly List<CharRange> CjCharacterRanges = new List<CharRange>
		{
			new CharRange('\u3040', '\u312f'),
			//new CharRange('\u3040', '\u309F'), // Hiragana
			//new CharRange('\u30A0', '\u30FF'), // Katakana
			//new CharRange('\u3100', '\u312f'), // Bopomofo

			new CharRange('\u31F0', '\u31FF'), // Katakana Phonetic Extensions
			new CharRange('\u3300', '\u337f'), // CJK Compatibility (Non Korean)
			new CharRange('\u3400', '\u4dbf'), // CJK Unified Ideographs ExtensionA
			new CharRange('\u4e00', '\u9fff'), // CJK Unified Ideographs
			new CharRange('\uf900', '\ufaff'), // CJK Compatibility Ideographs
			new CharRange('\uff65', '\uff9f'), // Halfwidth and Fullwidth Forms (Non Korean)
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