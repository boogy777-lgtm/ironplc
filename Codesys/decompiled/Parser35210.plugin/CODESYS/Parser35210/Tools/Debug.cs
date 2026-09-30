using System.Diagnostics;

namespace CODESYS.Parser35210.Tools
{
	public class Debug
	{
		public static Debug Singleton { get; set; } = new Debug();


		internal static void Assert(bool bAssert)
		{
			if (!bAssert)
			{
				Fail(string.Empty);
			}
		}

		internal static void Fail(string text)
		{
			Singleton.FailMockable(text);
		}

		public virtual void FailMockable(string text)
		{
			if (Debugger.IsAttached)
			{
				Debugger.Break();
			}
		}
	}
}
