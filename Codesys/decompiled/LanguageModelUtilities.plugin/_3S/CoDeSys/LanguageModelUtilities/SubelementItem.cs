using System.Diagnostics;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[DebuggerDisplay("{Name}")]
	internal class SubelementItem
	{
		internal string Name { get; private set; }

		internal IdentifierInfo Info { get; private set; }

		internal SubelementItem(string name, IdentifierInfo info)
		{
			Name = name;
			Info = info;
		}
	}
}
