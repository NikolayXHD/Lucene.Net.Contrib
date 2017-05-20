using System;
using System.Collections.Generic;

namespace Lucent.Net.Contrib
{
	[Flags]
	public enum TokenType
	{
		Open = OpenOpenRange | OpenClosedRange | OpenGroup,
		OpenRange = OpenOpenRange | OpenClosedRange,
		OpenOpenRange = 1 << 0,
		OpenClosedRange = 1 << 1,
		OpenGroup = 1 << 2,

		Close = CloseOpenRange | CloseClosedRange | CloseGroup,
		CloseOpenRange = 1 << 5,
		CloseClosedRange = 1 << 6,
		CloseGroup = 1 << 7,

		To = 1 << 10,

		Quote = 1 << 11,

		Boolean = And | Or | Not,
		And = 1 << 14,
		Or = 1 << 15,
		Not = 1 << 16,

		Wildcard = AnyChar | AnyString,
		AnyChar = 1 << 19,
		AnyString = 1 << 20,

		Modifier = BoostModifier | SlopeModifier,
		BoostModifier = 1 << 23,
		SlopeModifier = 1 << 24,

		Field = 1 << 27,
		Colon = 1 << 28,

		FieldValue = 1 << 29,
		ModifierValue = 1 << 30
	}

	public static class TokenTypeExtension
	{
		public static bool Is(this TokenType value, TokenType kind)
		{
			return (value & kind) == value;
		}

		public static bool IsLegalCloserOf(this TokenType closer, TokenType? opener)
		{
			return opener.HasValue && closer.Is(TokenType.Close) && LegalOpenersByCloser[closer].Contains(opener.Value);
		}

		private static readonly Dictionary<TokenType, List<TokenType>> LegalOpenersByCloser = new Dictionary<TokenType, List<TokenType>>
		{
			{ TokenType.CloseOpenRange, new List<TokenType> { TokenType.OpenOpenRange, TokenType.OpenClosedRange } },
			{ TokenType.CloseClosedRange, new List<TokenType> { TokenType.OpenClosedRange, TokenType.OpenOpenRange } },
			{ TokenType.CloseGroup, new List<TokenType> { TokenType.OpenGroup } }
		};
	}
}