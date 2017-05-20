using System.Collections;
using System.Collections.Generic;

namespace Lucent.Net.Contrib
{
	/// <summary>
	/// Выполняет отделение Escape-последовательностей
	/// </summary>
	internal class StringEscaper : IEnumerator<EscapedChar>
	{
		public static readonly HashSet<char> SpecialChars = new HashSet<char>
		{
			'&', '|', '+', '-', '!', '(', ')', '{', '}', '[', ']', '^' , '\"', '~', '*', '?', ':', '\\'
		};

		public static readonly HashSet<char> TwoSymbolOperators = new HashSet<char>
		{
			'&', '|'
		};

		public StringEscaper(string query)
		{
			_query = query;
		}
		
		public bool MoveNext()
		{
			if (Position >= _query.Length)
			{
				Substring = null;
				return false;
			}

			int s = getSpacesCount();
			if (s > 0)
			{
				var result = new string(_query[Position], s);
				Position += s;
				Substring = result;
				return true;
			}

			if (_query[Position] != EscapeCharacter)
			{
				var result = new string(_query[Position], 1);
				Position++;
				Substring = result;
				return true;
			}

			Position++;
			if (Position < _query.Length)
			{
				if (SpecialChars.Contains(_query[Position]))
				{
					var result = _query.Substring(Position - 1, 2);
					Position ++;
					Substring = result;
					return true;
				}
			}

			Substring = EscapeString;
			return true;
		}

		private int getSpacesCount()
		{
			for (int s = 0; Position + s < _query.Length; s++)
				if (!char.IsWhiteSpace(_query[Position + s]))
					return s;

			return _query.Length - Position;
		}



		object IEnumerator.Current => Substring;

		public void Reset()
		{
			Position = 0;
			Substring = null;
		}

		public void Dispose()
		{
		}

		public EscapedChar Current => new EscapedChar(Substring, Position);

		private int Position { get; set; }
		private string Substring { get; set; }

		private readonly string _query;
		private const char EscapeCharacter = '\\';
		private static readonly string EscapeString = new string(EscapeCharacter, 1);
	}
}