using System;
using \u0008;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220
{
	// Token: 0x02000008 RID: 8
	public static class ProjectDefines
	{
		// Token: 0x06000085 RID: 133 RVA: 0x000027A0 File Offset: 0x000009A0
		public static bool IsInProjectDefined(string stDefine)
		{
			string projectDefines = APEnvironmentFacade.Instance.CompileOptions.ProjectDefines;
			LDictionary<string, string> ldictionary = new LDictionary<string, string>();
			\u0005.\u0001(projectDefines, ldictionary);
			return ldictionary.ContainsKey(stDefine);
		}
	}
}
