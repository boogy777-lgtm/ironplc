using System;

namespace _3S.CoDeSys.Compiler35220.Tools
{
	// Token: 0x0200005C RID: 92
	public class FatalCompileErrorException : Exception
	{
		// Token: 0x0600067D RID: 1661 RVA: 0x0000DBE4 File Offset: 0x0000BDE4
		internal FatalCompileErrorException()
		{
		}

		// Token: 0x0600067E RID: 1662 RVA: 0x0000DBEC File Offset: 0x0000BDEC
		internal FatalCompileErrorException(string msg) : base(msg)
		{
		}
	}
}
