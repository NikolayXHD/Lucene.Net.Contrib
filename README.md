# Lucene.Net.Contrib

## Lucene.Net.Contrib.EditedTokenLocator

A class library to facilitate implementing a code editor supporting Lucene.NET query syntax. The implemented syntax is specified [here](https://lucene.apache.org/core/2_9_4/queryparsersyntax.html).

When it comes to implementing intellisense normal query parsing is not enough. You need to be able to interpret partial input as well as input with some not-so-much-breaking errors.

EditedTokenLocator uses an internal class TolerantParser to parse the entire input as a (possibly) incomplete query written in Lucene language.

Then according to caret position (user input marker whithin text box) it tells which token is edited, it's type and the field it is assosiated with.

Based on this an end-user application can correctly decide what strings should intellisense suggest to end-user.

Example usage from my hobby project [Mtgdb.Gui](https://www.slightlymagic.net/forum/viewtopic.php?f=62&t=19299)

![Implementation example screenshot](https://github.com/NikolayXHD/Mtgdb/raw/master/out/help/l/search_intellisense.jpg)

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucent.Net.Contrib;
using SpellChecker.Net.Search.Spell;

namespace Mtgdb.Dal.Index
{
	public class SuggestSource
	{
		public TokenSuggest Suggest(string query, int caret, string language, int maxCount)
		{
			var token = EditedTokenLocator.GetEditedToken(query, caret);

			if (token == null || token.Type.Is(TokenType.ModifierValue))
				return new TokenSuggest(null, new string[0]);

			string value = token.Value.Substring(0, caret - token.Position);

			if (token.Type.Is(TokenType.FieldValue))
				return new TokenSuggest(token, SuggestValues(value.ToLowerInvariant(), token.ParentField, language, maxCount));

			if (token.Type.Is(TokenType.Field))
				return new TokenSuggest(token, suggestFields(value));

			return new TokenSuggest(token, new[] { value });
		}
		
		// ... some other code
	}
}
```
