namespace Lucent.Net.Contrib
{
	public class EscapedChar
	{
		public EscapedChar(string value, int position)
		{
			Value = value;
			Position = position;
		}

		public int Position { get; private set; }

		public string Value { get; private set; }
	}
}