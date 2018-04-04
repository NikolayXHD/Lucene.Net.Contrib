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

				if (_isRegexOpen)
				{
					// empty regex body
					if (tokenTypeNullable == TokenType.RegexDelimiter)
					{
						// close quote
						var token = addToken(TokenType.RegexDelimiter);
						_openOperators.Pop();
						updateCurrentField();
						token.NextTokenField = _currentField;
						_isRegexOpen = false;
					}
					// regex body token lasts until regex delimiter or End Of String
					else if (!_context.HasNext || TokenFilter.GetTokenType(_context.Next.Value) == TokenType.RegexDelimiter)
					{
						var token = addToken(TokenType.RegexBody);
						token.NextTokenField = _currentField;
						_isRegexOpen = false;
					}
				}
				else if (tokenTypeNullable.HasValue)
				{
					var tokenType = tokenTypeNullable.Value;

					if (tokenType.IsAny(TokenType.Open))
					{
						var token = addToken(tokenType);
						_openOperators.Push(token);
						token.NextTokenField = _currentField;
					}
					else if (tokenType.IsAny(TokenType.Close))
					{
						if (tokenType.IsLegalCloserOf(_openOperators.TryPeek()?.Type))
						{
							// close parenthesis
							var token = addToken(tokenType);
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

							var token = addToken(tokenType);
							token.NextTokenField = _currentField;
						}
					}
					else if (tokenType.IsAny(TokenType.Quote | TokenType.RegexDelimiter))
					{
						TokenType generalType;
						TokenType openType;
						TokenType closeType;
						bool isRegex;

						if (tokenType.IsAny(TokenType.Quote))
						{
							generalType = TokenType.Quote;
							openType = TokenType.OpenQuote;
							closeType = TokenType.CloseQuote;
							isRegex = false;
						}
						else
						{
							generalType = TokenType.RegexDelimiter;
							openType = TokenType.OpenRegex;
							closeType = TokenType.CloseRegex;
							isRegex = true;
						}

						if (_openOperators.Count > 0 && _openOperators.Peek().Type.IsAny(generalType))
						{
							// close quote
							var token = addToken(closeType);
							_openOperators.Pop();
							updateCurrentField();
							token.NextTokenField = _currentField;
						}
						else
						{
							var token = addToken(openType);
							_openOperators.Push(token);

							if (isRegex)
								_isRegexOpen = true;

							token.NextTokenField = _currentField;
						}
					}
					else if (tokenType.IsAny(TokenType.Modifier | TokenType.Colon) || tokenType.IsAny(TokenType.Boolean) &&
						// To avoid recognizing AND in ANDY
						(StringEscaper.SpecialChars.Contains(_substring[0]) || beforeTerminator))
					{
						var token = addToken(tokenType);
						token.NextTokenField = _currentField;
					}
					else if (tokenType.IsAny(TokenType.Wildcard))
					{
						if (tokenType.IsAny(TokenType.AnyString) && nextIsColon())
						{
							// add field
							var token = addToken(TokenType.Field);
							_currentField = Tokens[Tokens.Count - 1].ParentField;
							token.NextTokenField = _currentField;
						}
						else if (!beforeTerminator)
						{
							var previous = Tokens.TryGetLast();

							// adjacent wildcard tokens are related to the same field
							if (previous != null && previous.Position + previous.Value.Length == _start)
								_currentField = previous.ParentField;

							var token = addToken(tokenType);
							token.NextTokenField = _currentField;
						}
						else
						{
							var token = addToken(tokenType);
							updateCurrentField();
							token.NextTokenField = _currentField;
						}
					}
					else if (_openOperators.TryPeek()?.Type.IsAny(TokenType.OpenRange) == true && tokenType.IsAny(TokenType.To))
					{
						// interval extremes separator
						var token = addToken(tokenType);
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
					var token = addToken(TokenType.Field);
					_currentField = Tokens[Tokens.Count - 1].ParentField;
					token.NextTokenField = _currentField;
				}
				else if (beforeTerminator && prevIsModifier())
				{
					var token = addToken(TokenType.ModifierValue);

					updateCurrentField();
					token.NextTokenField = _currentField;
				}
				else if (beforeTerminator)
				{
					var token = addToken(TokenType.FieldValue);

					updateCurrentField();
					token.NextTokenField = _currentField;
				}
				else if (isCj())
				{
					var token = addToken(TokenType.FieldValue);
					token.NextTokenField = _currentField;
				}
			}
		}

		private bool prevIsModifier()
		{
			return Tokens.TryGetLast()?.Type.IsAny(TokenType.Modifier) == true;
		}

		private bool isCj()
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
			return _context.HasNext && TokenFilter.GetTokenType(_context.Next.Value)?.IsAny(TokenType.Colon) == true;
		}

		private Token addToken(TokenType tokenType)
		{
			string field;
			var previous = Tokens.TryGetLast();

			bool startsPhrase;

			switch (tokenType)
			{
				case TokenType.Field:
					startsPhrase = false;
					field = _substring;
					break;
				
				case TokenType.FieldValue:
					
					startsPhrase = previous?.Type.IsAny(TokenType.OpenQuote) == true;
					field = previous?.Type.IsAny(TokenType.Modifier) == true 
						? null
						: _currentField;

					break;

				default:
					startsPhrase = false;
					field = _currentField;
					break;
			}

			string value = _substring.TrimEnd();

			var result = new Token(_start, value, tokenType, field);
			result.SetPrevious(previous);
			previous?.SetNext(result);

			if (startsPhrase)
				result.PhraseStart = result;
			else if (tokenType == TokenType.FieldValue)
				result.PhraseStart = previous?.PhraseStart;
			else
				result.PhraseStart = null;

			if (tokenType.IsAny(TokenType.SlopeModifier))
			{
				if (previous?.Type.IsAny(TokenType.CloseQuote) == true)
				{
					var current = previous.Previous;

					while (current != null)
					{
						current.PhraseHasSlop = true;

						if (current.IsPhraseStart)
							break;

						current = current.Previous;
					}
				}
			}

			Tokens.Add(result);

			_substring = string.Empty;
			_start = _position;

			return result;
		}



		private readonly Stack<Token> _openOperators = new Stack<Token>();
		private bool _isRegexOpen;
		private string _currentField;
		private int _start;
		private int _position;
		private string _substring;
		private readonly ContextualEnumerator<EscapedChar> _context;
	}
}