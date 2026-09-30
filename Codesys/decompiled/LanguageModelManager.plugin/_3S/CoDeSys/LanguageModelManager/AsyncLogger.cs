using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000D8 RID: 216
	internal class AsyncLogger
	{
		// Token: 0x06000F67 RID: 3943 RVA: 0x00029850 File Offset: 0x00028850
		public AsyncLogger(string path)
		{
			AsyncLogger <>4__this = this;
			if (Path.GetInvalidPathChars().Any(new Func<char, bool>(path.Contains<char>)))
			{
				throw new InvalidOperationException(path + " contains invalid characters");
			}
			this.LogQueue = new ConcurrentQueue<string>();
			this.LogTimer = new Timer(delegate(object _)
			{
				LStringBuilder lstringBuilder = new LStringBuilder();
				int num = 10;
				string text;
				while (num-- > 0 && <>4__this.LogQueue.TryDequeue(out text))
				{
					lstringBuilder.AppendLine(text);
				}
				if (lstringBuilder.Length > 0)
				{
					AuthFile.AppendAllText(path, lstringBuilder.ToString());
				}
			}, null, default(TimeSpan), TimeSpan.FromMilliseconds(500.0));
		}

		// Token: 0x06000F68 RID: 3944 RVA: 0x000298E4 File Offset: 0x000288E4
		public void Log(string text)
		{
			this.LogQueue.Enqueue(text);
		}

		// Token: 0x04000386 RID: 902
		private readonly ConcurrentQueue<string> LogQueue;

		// Token: 0x04000387 RID: 903
		private readonly Timer LogTimer;
	}
}
