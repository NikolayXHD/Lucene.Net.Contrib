using System.Collections.Generic;

namespace Lucent.Net.Contrib
{
	internal static class Extensions
	{
		public static TVal TryPeek<TVal>(this Stack<TVal> stack)
		{
			if (stack.Count == 0)
				return default(TVal);

			return stack.Peek();
		}

		public static bool ContainsString(this ICollection<char> chars, string value)
		{
			return value.Length == 1 && chars.Contains(value[0]);
		}

		public static TVal TryGetLast<TVal>(this IList<TVal> list)
		{
			if (list.Count == 0)
				return default(TVal);

			return list[list.Count - 1];
		}
	}
}
