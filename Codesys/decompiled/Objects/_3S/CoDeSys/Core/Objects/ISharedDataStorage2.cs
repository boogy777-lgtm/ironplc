using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000034 RID: 52
	[ReleasedInterface]
	public interface ISharedDataStorage2 : ISharedDataStorage
	{
		// Token: 0x060000DA RID: 218
		void CompactTables();

		// Token: 0x060000DB RID: 219
		void GetStatistics(out int totalStringTableEntries, out int totalStringTableCharacters, out int unusedStringTableEntries, out int unusedStringTableCharacters, out int totalSchemaTableEntries, out int unusedSchemaTableEntries, out bool willBeCompact);
	}
}
