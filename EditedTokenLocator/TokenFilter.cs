using System.Collections.Generic;

namespace Lucene.Net.Contrib
{
	internal static class TokenFilter
	{
		private static readonly Dictionary<string, TokenType> TokensDictionary = new Dictionary<string, TokenType>
		{
			{"AND", TokenType.And },
			{"&&", TokenType.And },
			{"+", TokenType.And },

			{"OR", TokenType.Or },
			{"||", TokenType.Or },

			{"NOT", TokenType.Not },
			{"!", TokenType.Not },
			{"-", TokenType.Not },

			{"^", TokenType.BoostModifier },
			{"~", TokenType.SlopeModifier },

			{"?", TokenType.AnyChar },
			{"*", TokenType.AnyString },

			{"(", TokenType.OpenGroup },
			{"{", TokenType.OpenOpenRange },
			{"[", TokenType.OpenClosedRange },

			{")", TokenType.CloseGroup },
			{"}", TokenType.CloseOpenRange },
			{"]", TokenType.CloseClosedRange },

			{"TO", TokenType.To },
			{":", TokenType.Colon },
			{"\"", TokenType.Quote }
		};

		public static TokenType? GetTokenType(string s)
		{
			TokenType result;
			if (TokensDictionary.TryGetValue(s, out result))
				return result;

			return null;
		}

		public static bool IsWhitespace(string s)
		{
			if (s == string.Empty)
				return true;

			for (int i = 0; i < s.Length; i++)
			{
				var c = s[i];
				if (!char.IsWhiteSpace(c))
					return false;
			}

			return true;
		}
	}
}