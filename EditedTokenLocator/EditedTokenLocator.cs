using System.Linq;

namespace Lucent.Net.Contrib
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

			if (leftToken?.Type.Is(TokenType.Modifier) == true && leftToken.TouchesCaret(caret))
				return new Token(caret, string.Empty, TokenType.ModifierValue, leftToken.ParentField);

			if (leftToken?.TouchesCaret(caret) == true)
			{
				if (leftToken.Type.Is(TokenType.Field | TokenType.FieldValue | TokenType.Modifier))
					return leftToken;

				if (leftToken.Type.Is(TokenType.Boolean) && leftToken.Value.Length > 1)
					return leftToken;

				return tokenOnEmptyInput(caret, leftToken.NextTokenField);
			}

			return tokenOnEmptyInput(caret, leftToken?.NextTokenField);
		}

		private static Token tokenOnEmptyInput(int caret, string field = null)
		{
			if (!string.IsNullOrEmpty(field))
				return new Token(caret, string.Empty, TokenType.FieldValue, field);

			return new Token(caret, string.Empty, TokenType.Field, field);
		}
	}
}