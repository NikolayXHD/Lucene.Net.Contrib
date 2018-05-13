using System.Collections.Generic;
using System.Linq;

namespace Lucene.Net.Contrib
{
	public static class EditedTokenLocator
	{
		public static Token GetEditedToken(this TolerantTokenizer tokenizer, int caret)
		{
			tokenizer.Parse();

			var tokens = tokenizer.Tokens;

			var overllapingToken = tokens.FirstOrDefault(_ => _.OverlapsCaret(caret));

			if (overllapingToken != null)
				return overllapingToken;

			var leftToken = tokens.LastOrDefault(_ => _.IsLeftToCaret(caret));
			var rightToken = tokens.FirstOrDefault(_ => _.IsRightToCaret(caret));

			if (leftToken?.IsConnectedToCaret(caret) != true && rightToken?.IsConnectedToCaret(caret) == true)
				return rightToken;

			if (leftToken?.Type.IsAny(TokenType.Modifier) == true && leftToken.TouchesCaret(caret))
				return new Token(caret, string.Empty, TokenType.ModifierValue, leftToken.ParentField);

			if (leftToken?.TouchesCaret(caret) == true)
			{
				if (leftToken.Type.IsAny(TokenType.Field | TokenType.FieldValue | TokenType.Modifier | TokenType.Wildcard))
					return leftToken;

				if (leftToken.Type.IsAny(TokenType.Boolean) && leftToken.Value.Length > 1)
					return leftToken;

				return tokenOnEmptyInput(tokens, caret);
			}

			return tokenOnEmptyInput(tokens, caret);
		}

		public static Token GetTokenForArbitraryInsertion(this TolerantTokenizer tokenizer, int caret)
		{
			tokenizer.Parse();

			var tokens = tokenizer.Tokens;

			var overllapingToken = tokens.FirstOrDefault(_ => _.OverlapsCaret(caret));

			if (overllapingToken != null)
				return tokenOnEmptyInput(tokens, overllapingToken.Position + overllapingToken.Value.Length);

			var leftToken = tokens.LastOrDefault(_ => _.IsLeftToCaret(caret));

			if (leftToken == null)
				return tokenOnEmptyInput(tokens, 0);

			return tokenOnEmptyInput(tokens, leftToken.Position + leftToken.Value.Length);
		}

		public static Token GetTokenForTermInsertion(this TolerantTokenizer tokenizer, int caret)
		{
			tokenizer.Parse();

			var tokens = tokenizer.Tokens;

			var token = 
				tokens.FirstOrDefault(_ => _.OverlapsCaret(caret)) ??
				tokens.LastOrDefault(_ => _.IsLeftToCaret(caret));

			if (token == null)
				return tokenOnEmptyInput(tokens, 0);

			var current = token;

			while (true)
			{
				if (!current.IsPhrase && string.IsNullOrEmpty(current.NextTokenField))
					return tokenOnEmptyInput(tokens, current.Position + current.Value.Length);

				current = current.Next;

				if (current == null)
					return tokenOnEmptyInput(tokens, 0);
			}
		}

		private static Token tokenOnEmptyInput(List<Token> tokens, int caret)
		{
			var leftToken = tokens.LastOrDefault(_ => _.Position + _.Value.Length <= caret);
			var rightToken = tokens.FirstOrDefault(_ => _.Position >= caret);

			var field = leftToken?.NextTokenField;

			var lastQuote = tokens.LastOrDefault(_ => _.Position < caret && _.Type.IsAny(TokenType.Quote | TokenType.RegexDelimiter));

			Token result;

			if (lastQuote?.Type.IsAny(TokenType.OpenQuote | TokenType.OpenRegex) == true || !string.IsNullOrEmpty(field))
				result = new Token(caret, string.Empty, TokenType.FieldValue, field);
			else
				result = new Token(caret, string.Empty, TokenType.Field, field);

			result.SetPrevious(leftToken);
			result.SetNext(rightToken);

			result.PhraseStart = result.Previous?.PhraseStart;
			result.PhraseHasSlop = result.Previous?.PhraseHasSlop == true;
			result.IsPhraseComplex = result.Previous?.IsPhraseComplex == true;

			return result;
		}
	}
}