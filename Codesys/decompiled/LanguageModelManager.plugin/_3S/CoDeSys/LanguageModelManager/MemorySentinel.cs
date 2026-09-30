using System;
using System.Diagnostics;
using System.Runtime;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000154 RID: 340
	internal static class MemorySentinel
	{
		// Token: 0x06001B97 RID: 7063 RVA: 0x0004E254 File Offset: 0x0004D254
		internal static bool CheckMinimumFreeMemory(int nMinMemory)
		{
			MemoryFailPoint memoryFailPoint = null;
			if (nMinMemory == 0)
			{
				return true;
			}
			try
			{
				memoryFailPoint = new MemoryFailPoint(nMinMemory);
			}
			catch (InsufficientMemoryException ex)
			{
				Debug.WriteLine(ex.Message);
				return false;
			}
			finally
			{
				if (memoryFailPoint != null)
				{
					memoryFailPoint.Dispose();
				}
			}
			return true;
		}

		// Token: 0x040005D2 RID: 1490
		internal static readonly int MAX_MONITORING_ELEMENTS = 16000;
	}
}
