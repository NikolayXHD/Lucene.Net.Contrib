using System;

namespace Lucene.Net.Contrib
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

		internal Token PhraseStart { get; set; }
		internal bool PhraseHasSlop { get; set; }

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
			if (Type.IsAny(TokenType.Field))
				return $"{Position:D3}: {Value}";

			return $"{Position:D3}: {Value}    {Type} of: {ParentField}";
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
			if (!TouchesCaret(caret))
				return false;

			if (Type.IsAny(TokenType.Open | TokenType.Close | TokenType.Colon | TokenType.Quote | TokenType.RegexDelimiter))
				return false;

			if (Type.IsAny(TokenType.FieldValue))
			{
				if (Position == caret && Value[0].IsCj())
					return false;

				if (Position + Value.Length == caret && Value[Value.Length - 1].IsCj())
					return false;
			}

			return true;
		}

		public bool IsPhraseStart => PhraseStart == this && !PhraseHasSlop;
		
		public bool IsPhrase => PhraseStart != null && !PhraseHasSlop;

		public string GetPhraseText(string queryText)
		{
			if (!IsPhraseStart)
				return Value;

			int start = PhraseStart.Position;
			
			var current = PhraseStart;
			while (true)
			{
				var end = current.Position + current.Value.Length;

				if (current.Next == null || current.Next.PhraseStart != PhraseStart)
					return queryText.Substring(start, end - start);
				
				current = current.Next;
			}
		}
	}
}