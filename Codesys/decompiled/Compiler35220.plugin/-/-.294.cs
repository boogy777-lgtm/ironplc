using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using \u0019;
using \u001A;
using \u001C;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000F
{
	// Token: 0x02000316 RID: 790
	internal sealed class \u0015
	{
		// Token: 0x170007E5 RID: 2021
		// (get) Token: 0x06002F7C RID: 12156 RVA: 0x000B28CC File Offset: 0x000B0ACC
		private ConcurrentQueue<ICompiledPOUWithParseTreeProvider> POUSToLoad { get; } = new ConcurrentQueue<ICompiledPOUWithParseTreeProvider>();

		// Token: 0x170007E6 RID: 2022
		// (get) Token: 0x06002F7D RID: 12157 RVA: 0x000B28D4 File Offset: 0x000B0AD4
		private Thread[] LoaderThreads { get; }

		// Token: 0x170007E7 RID: 2023
		// (get) Token: 0x06002F7E RID: 12158 RVA: 0x000B28DC File Offset: 0x000B0ADC
		// (set) Token: 0x06002F7F RID: 12159 RVA: 0x000B28E4 File Offset: 0x000B0AE4
		private bool EndThread { get; set; }

		// Token: 0x170007E8 RID: 2024
		// (get) Token: 0x06002F80 RID: 12160 RVA: 0x000B28F0 File Offset: 0x000B0AF0
		// (set) Token: 0x06002F81 RID: 12161 RVA: 0x000B28F8 File Offset: 0x000B0AF8
		internal int ParseTreesLoaded { get; set; }

		// Token: 0x170007E9 RID: 2025
		// (get) Token: 0x06002F82 RID: 12162 RVA: 0x000B2904 File Offset: 0x000B0B04
		// (set) Token: 0x06002F83 RID: 12163 RVA: 0x000B290C File Offset: 0x000B0B0C
		internal long TimeToLoad { get; set; }

		// Token: 0x170007EA RID: 2026
		// (get) Token: 0x06002F84 RID: 12164 RVA: 0x000B2918 File Offset: 0x000B0B18
		// (set) Token: 0x06002F85 RID: 12165 RVA: 0x000B2920 File Offset: 0x000B0B20
		internal long TimeForRedTree { get; set; }

		// Token: 0x06002F86 RID: 12166 RVA: 0x000B292C File Offset: 0x000B0B2C
		public \u0015()
		{
			this.LoaderThreads = new Thread[global::\u000F.\u0015.\u0002];
			for (int i = 0; i < this.LoaderThreads.Length; i++)
			{
				this.LoaderThreads[i] = new Thread(new ThreadStart(this.\u0002));
			}
		}

		// Token: 0x06002F87 RID: 12167 RVA: 0x000B299C File Offset: 0x000B0B9C
		public void \u0001(ICompiledPOUWithParseTreeProvider \u0002)
		{
			this.POUSToLoad.Enqueue(\u0002);
			object u = this.\u0001;
			lock (u)
			{
				Monitor.Pulse(this.\u0001);
			}
		}

		// Token: 0x06002F88 RID: 12168 RVA: 0x000B29F0 File Offset: 0x000B0BF0
		public void \u0002(ICompiledPOUWithParseTreeProvider \u0002)
		{
			IParseTreeProvider parseTreeProvider = \u0002.ParseTreeProvider;
			lock (parseTreeProvider)
			{
				\u001A.\u0012 u = \u0002.ParseTreeProvider as \u001A.\u0012;
				if (u != null && !u.Done)
				{
					try
					{
						if (\u0002.ParseTreeRaw is _IEmptyStatement && !string.IsNullOrEmpty((\u0002 as _ICompiledPOU).LibraryPath))
						{
							long ticks = DateTime.Now.Ticks;
							_IStatement istatement = \u001C.\u0004.\u0001(\u0002 as _ICompiledPOU);
							if (istatement != null)
							{
								this.TimeToLoad += DateTime.Now.Ticks - ticks;
								int num = this.ParseTreesLoaded;
								this.ParseTreesLoaded = num + 1;
								\u0002.ParseTreeProvider.SetParseTreeDirectly(istatement);
							}
						}
						if (global::\u000F.\u0015.\u0002)
						{
							long ticks2 = DateTime.Now.Ticks;
							u.\u0001();
							this.TimeForRedTree += DateTime.Now.Ticks - ticks2;
						}
					}
					finally
					{
						u.Done = true;
					}
				}
			}
		}

		// Token: 0x06002F89 RID: 12169 RVA: 0x000B2B14 File Offset: 0x000B0D14
		public void \u0001()
		{
			this.EndThread = false;
			for (int i = 0; i < this.LoaderThreads.Length; i++)
			{
				this.LoaderThreads[i].Start();
			}
			object u = this.\u0001;
			lock (u)
			{
				Monitor.PulseAll(this.\u0001);
			}
		}

		// Token: 0x06002F8A RID: 12170 RVA: 0x000B2B80 File Offset: 0x000B0D80
		public void \u0001(_ICompileContext \u0002)
		{
			this.EndThread = true;
			object u = this.\u0001;
			lock (u)
			{
				Monitor.PulseAll(this.\u0001);
			}
			for (int i = 0; i < this.LoaderThreads.Length; i++)
			{
				this.LoaderThreads[i].Join();
			}
			for (int j = 0; j < \u0002.GetAllSignaturesFlatInvariant().Count<_ISignature>(); j++)
			{
				_ISignature isignature = \u0002.GetAllSignaturesFlatInvariant()[j];
				_ICompiledPOU icompiledPOU = \u0002.GetCompiledPOUById(isignature.Id) as _ICompiledPOU;
				if (icompiledPOU != null && (icompiledPOU as ICompiledPOUWithParseTreeProvider).ParseTreeProvider is \u001A.\u0012)
				{
					\u001A.\u0012 u2 = (icompiledPOU as ICompiledPOUWithParseTreeProvider).ParseTreeProvider as \u001A.\u0012;
					(icompiledPOU as ICompiledPOUWithParseTreeProvider).ParseTreeProvider = u2.Original;
				}
			}
			foreach (string u3 in this.\u0001)
			{
				_ICompilerMessage message = \u0019.\u0003.\u0001(null, u3, Severity.FatalError, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
			}
		}

		// Token: 0x06002F8B RID: 12171 RVA: 0x000B2CC4 File Offset: 0x000B0EC4
		private void \u0002()
		{
			ICompiledPOUWithParseTreeProvider compiledPOUWithParseTreeProvider = null;
			try
			{
				while (!this.EndThread || !this.POUSToLoad.IsEmpty)
				{
					if (this.POUSToLoad.TryDequeue(out compiledPOUWithParseTreeProvider))
					{
						this.\u0002(compiledPOUWithParseTreeProvider);
					}
					else
					{
						object u = this.\u0001;
						lock (u)
						{
							Monitor.Wait(this.\u0001, 1000);
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (compiledPOUWithParseTreeProvider != null)
				{
					this.\u0001.Add("Internal error in late loading of parse tree for POU " + (compiledPOUWithParseTreeProvider as _ICompiledPOU).Name + ": " + ex.Message);
				}
				else
				{
					this.\u0001.Add("Internal error in late loading of parse trees: " + ex.Message);
				}
			}
		}

		// Token: 0x0400090C RID: 2316
		[CompilerGenerated]
		private readonly ConcurrentQueue<ICompiledPOUWithParseTreeProvider> \u0001;

		// Token: 0x0400090D RID: 2317
		[CompilerGenerated]
		private readonly Thread[] \u0001;

		// Token: 0x0400090E RID: 2318
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x0400090F RID: 2319
		private readonly object \u0001 = new object();

		// Token: 0x04000910 RID: 2320
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x04000911 RID: 2321
		[CompilerGenerated]
		private long \u0001;

		// Token: 0x04000912 RID: 2322
		[CompilerGenerated]
		private long \u0002;

		// Token: 0x04000913 RID: 2323
		private static readonly int \u0002 = Math.Max(Environment.ProcessorCount - 1, 1);

		// Token: 0x04000914 RID: 2324
		private static readonly bool \u0002 = Environment.Is64BitProcess;

		// Token: 0x04000915 RID: 2325
		private readonly ConcurrentBag<string> \u0001 = new ConcurrentBag<string>();
	}
}
