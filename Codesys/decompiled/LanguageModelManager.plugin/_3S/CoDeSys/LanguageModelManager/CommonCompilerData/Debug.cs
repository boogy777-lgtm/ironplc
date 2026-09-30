using System;
using System.Diagnostics;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001CB RID: 459
	public class Debug
	{
		// Token: 0x06002056 RID: 8278 RVA: 0x000597E2 File Offset: 0x000587E2
		internal static void Assert(bool bAssert)
		{
			if (!bAssert)
			{
				Debug.Fail(string.Empty);
			}
		}

		// Token: 0x06002057 RID: 8279 RVA: 0x000597F1 File Offset: 0x000587F1
		internal static void Assert(bool bAssert, string stMessage)
		{
			if (!bAssert)
			{
				Debug.Fail(stMessage);
			}
		}

		// Token: 0x06002058 RID: 8280 RVA: 0x000597FC File Offset: 0x000587FC
		internal static void AssertAsException(bool bAssert)
		{
			if (!bAssert)
			{
				throw new InvalidStateCompilerException();
			}
		}

		// Token: 0x06002059 RID: 8281 RVA: 0x00059807 File Offset: 0x00058807
		internal static void LateCompileErrorDetected(bool bError)
		{
			if (bError)
			{
				throw new LateCompileErrorException();
			}
		}

		// Token: 0x17000878 RID: 2168
		// (get) Token: 0x0600205A RID: 8282 RVA: 0x00059812 File Offset: 0x00058812
		// (set) Token: 0x0600205B RID: 8283 RVA: 0x00059819 File Offset: 0x00058819
		public static Debug Singleton { get; set; } = new Debug();

		// Token: 0x0600205C RID: 8284 RVA: 0x00059821 File Offset: 0x00058821
		internal static void Fail(string text)
		{
			Debug.Singleton.FailMockable(text);
		}

		// Token: 0x0600205D RID: 8285 RVA: 0x0005982E File Offset: 0x0005882E
		public virtual void FailMockable(string text)
		{
			if (Debugger.IsAttached)
			{
				Debugger.Break();
				return;
			}
			Debug.Assert(false, text);
		}

		// Token: 0x0600205E RID: 8286 RVA: 0x00059844 File Offset: 0x00058844
		internal static void WriteLine(string text)
		{
			Debug.WriteLine(text);
		}
	}
}
