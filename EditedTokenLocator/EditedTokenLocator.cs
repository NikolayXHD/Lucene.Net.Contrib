using System.Collections.Generic;
using System.Linq;

namespace Lucene.Net.Contrib
{
	public static class EditedTokenLocator
	{
		public static Token GetEditedToken(string query, int caret)
		{
			var tokenizer = new TolerantTokenizer(query);
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
				if (leftToken.Type.IsAny(TokenType.Field | TokenType.FieldValue | TokenType.Modifier))
					return leftToken;

				if (leftToken.Type.IsAny(TokenType.Boolean) && leftToken.Value.Length > 1)
					return leftToken;

				return tokenOnEmptyInput(tokens, caret, leftToken.NextTokenField);
			}

			return tokenOnEmptyInput(tokens, caret, leftToken?.NextTokenField);
		}

		private static Token tokenOnEmptyInput(List<Token> tokens, int caret, string field = null)
		{
			var lastQuote = tokens.LastOrDefault(_ => _.Position < caret && _.Type.IsAny(TokenType.Quote | TokenType.RegexDelimiter));

			Token result;

			if (lastQuote?.Type.IsAny(TokenType.OpenQuote | TokenType.OpenRegex) == true || !string.IsNullOrEmpty(field))
				result = new Token(caret, string.Empty, TokenType.FieldValue, field);
			else
				result = new Token(caret, string.Empty, TokenType.Field, field);

			result.SetPrevious(tokens.LastOrDefault(_ => _.Position + _.Value.Length <= result.Position));
			result.SetNext(tokens.FirstOrDefault(_ => _.Position >= result.Position + result.Value.Length));

			return result;
		}
	}
}