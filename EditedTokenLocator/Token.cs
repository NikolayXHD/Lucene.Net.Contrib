namespace Lucent.Net.Contrib
{
	public class Token
	{
		public Token(int position, string value, TokenType type, string parentField)
		{
			Position = position;
			Type = type;
			ParentField = parentField;
			Value = value;
		}

		public int Position { get; }
		public string ParentField { get; }
		public string NextTokenField { get; internal set; }

		public TokenType Type { get; }
		public string Value { get; }

		public Token Next { get; private set; }
		public Token Previous { get; private set; }

		internal void SetNext(Token value)
		{
			Next = value;
		}

		internal void SetPrevious(Token value)
		{
			Previous = value;
		}

		public override string ToString()
		{
			if (Type == TokenType.Field)
				return $"{Position:D3}: {Value}";

			return $"{Position:D3}: {Value}\t{Type} of: {ParentField}";
		}

		public bool IsLeftToCaret(int caret)
		{
			return Position + Value.Length <= caret;
		}

		public bool IsRightToCaret(int caret)
		{
			return Position >= caret;
		}

		public bool OverlapsCaret(int caret)
		{
			return Position + Value.Length > caret && Position < caret;
		}

		public bool TouchesCaret(int caret)
		{
			return Position == caret || Position + Value.Length == caret;
		}

		public bool IsConnectedToCaret(int caret)
		{
			return TouchesCaret(caret) && !Type.Is(TokenType.Open | TokenType.Close | TokenType.Colon | TokenType.Quote);
		}
	}
}