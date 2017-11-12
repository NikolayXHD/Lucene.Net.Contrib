using System.Collections.Generic;

namespace Lucene.Net.Contrib
{
	public class TolerantTokenizer
	{
		public List<Token> Tokens { get; } = new List<Token>();
		public List<string> SyntaxErrors { get; } = new List<string>();

		public TolerantTokenizer(string queryStr)
		{
			_context = new ContextualEnumerator<EscapedChar>(new StringEscaper(queryStr));
		}

		public void Parse()
		{
			_substring = string.Empty;

			while (_context.MoveNext())
			{
				_position = _context.Current.Position;
				_substring += _context.Current.Value;

				bool beforeTerminator = nextIsTerminator();

				var tokenTypeNullable = TokenFilter.GetTokenType(_substring);
				if (tokenTypeNullable.HasValue)
				{
					var tokenType = tokenTypeNullable.Value;

					if (tokenType.Is(TokenType.Open))
					{
						var token = createToken(tokenType);
						_openOperators.Push(token);
						token.NextTokenField = _currentField;
					}
					else if (tokenType.Is(TokenType.Close))
					{
						if (tokenType.IsLegalCloserOf(_openOperators.TryPeek()?.Type))
						{
							// close parenthesis
							var token = createToken(tokenType);
							_openOperators.Pop();
							updateCurrentField();
							token.NextTokenField = _currentField;
						}
						else
						{
							if (_openOperators.Count == 0)
								SyntaxErrors.Add($"Unmatched {_substring} at {_start}");
							else
								SyntaxErrors.Add($"Unexpected {_substring} at {_start} closing {_openOperators.Peek().Value} at {_openOperators.Peek().Position}");

							var token = createToken(tokenType);
							token.NextTokenField = _currentField;
						}
					}
					else if (tokenType.Is(TokenType.Quote))
					{
						if (_openOperators.Count > 0 && _openOperators.Peek().Type.Is(TokenType.Quote))
						{
							// close quote
							var token = createToken(TokenType.CloseQuote);
							_openOperators.Pop();
							updateCurrentField();
							token.NextTokenField = _currentField;
						}
						else
						{
							var token = createToken(TokenType.OpeningQuote);
							_openOperators.Push(token);
							token.NextTokenField = _currentField;
						}
					}
					else if (tokenType.Is(TokenType.Modifier | TokenType.Wildcard | TokenType.Colon) ||
						tokenType.Is(TokenType.Boolean) && 
						// To avoid recognizing AND in ANDY
						(StringEscaper.SpecialChars.Contains(_substring[0]) || beforeTerminator))
					{
						var previous = Tokens.TryGetLast();

						// adjacent wildcard tokens are related to the same field
						if (tokenType.Is(TokenType.Wildcard) && previous != null && previous.Position + previous.Value.Length == _start)
							_currentField = previous.ParentField;

						var token = createToken(tokenType);

						token.NextTokenField = _currentField;
					}
					else if (_openOperators.TryPeek()?.Type.Is(TokenType.OpenRange) == true && tokenType.Is(TokenType.To))
					{
						// interval extremes separator
						var token = createToken(tokenType);
						token.NextTokenField = _currentField;
					}
				}
				else if (TokenFilter.IsWhitespace(_substring))
				{
					// ignore whitespace token
					_start = _position;
					_substring = string.Empty;
				}
				else if (nextIsColon())
				{
					// add field
					var token = createToken(TokenType.Field);
					_currentField = Tokens[Tokens.Count - 1].ParentField;
					token.NextTokenField = _currentField;
				}
				else if (beforeTerminator && prevIsModifier())
				{
					var token = createToken(TokenType.ModifierValue);

					updateCurrentField();
					token.NextTokenField = _currentField;
				}
				else if (beforeTerminator)
				{
					var token = createToken(TokenType.FieldValue);

					updateCurrentField();
					token.NextTokenField = _currentField;
				}
				else if (isCjk())
				{
					var token = createToken(TokenType.FieldValue);
					token.NextTokenField = _currentField;
				}
			}
		}

		private bool prevIsModifier()
		{
			return Tokens.TryGetLast()?.Type.Is(TokenType.Modifier) == true;
		}

		private bool isCjk()
		{
			if (_context.Current.Value.Length == 0)
				return false;

			if (_context.Current.Value[_context.Current.Value.Length - 1].IsCj())
				return true;

			return false;
		}

		private void updateCurrentField()
		{
			_currentField = getCurrentField();
		}

		private string getCurrentField()
		{
			return _openOperators.TryPeek()?.ParentField;
		}

		private bool nextIsTerminator()
		{
			if (!_context.HasNext)
				return true;

			if (TokenFilter.IsWhitespace(_context.Next.Value))
				return true;

			bool nextIsSpecial = StringEscaper.SpecialChars.ContainsString(_context.Next.Value);
			bool currentIsSpecial = StringEscaper.SpecialChars.ContainsString(_context.Current.Value);

			if (!nextIsSpecial && !currentIsSpecial)
				return false;

			if (_context.Next.Value != _context.Current.Value)
				// There are no operators constructed from different special chars
				return true;

			if (StringEscaper.TwoSymbolOperators.ContainsString(_context.Current.Value))
				return false;

			return true;
		}

		private bool nextIsColon()
		{
			return _context.HasNext && TokenFilter.GetTokenType(_context.Next.Value)?.Is(TokenType.Colon) == true;
		}

		private Token createToken(TokenType tokenType)
		{
			string field;
			var previous = Tokens.TryGetLast();

			switch (tokenType)
			{
				case TokenType.Field:
					field = _substring;
					break;
				case TokenType.FieldValue:
					if (previous?.Type.Is(TokenType.Modifier) == true)
						field = null;
					else
						field = _currentField;
					break;

				default:
					field = _currentField;
					break;
			}

			string value = _substring.TrimEnd();

			var result = new Token(_start, value, tokenType, field);
			result.SetPrevious(previous);
			previous?.SetNext(result);

			Tokens.Add(result);

			_substring = string.Empty;
			_start = _position;

			return result;
		}



		private readonly Stack<Token> _openOperators = new Stack<Token>();

		private string _currentField;
		private int _start;
		private int _position;
		private string _substring;
		private readonly ContextualEnumerator<EscapedChar> _context;
	}
}