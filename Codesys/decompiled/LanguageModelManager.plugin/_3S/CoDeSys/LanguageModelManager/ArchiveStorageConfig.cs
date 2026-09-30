using System;
using System.Threading;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000AD RID: 173
	internal class ArchiveStorageConfig
	{
		// Token: 0x17000270 RID: 624
		// (get) Token: 0x06000A4F RID: 2639 RVA: 0x000178A8 File Offset: 0x000168A8
		// (set) Token: 0x06000A50 RID: 2640 RVA: 0x00017908 File Offset: 0x00016908
		internal ArchiveStorageFormat StorageFormat
		{
			get
			{
				Thread currentThread = Thread.CurrentThread;
				LDictionary<int, ArchiveStorageFormat> threadStorageFormatCfg = this._threadStorageFormatCfg;
				ArchiveStorageFormat result;
				lock (threadStorageFormatCfg)
				{
					ArchiveStorageFormat archiveStorageFormat;
					result = ((!this._threadStorageFormatCfg.TryGetValue(currentThread.ManagedThreadId, ref archiveStorageFormat)) ? null : archiveStorageFormat);
				}
				return result;
			}
			set
			{
				Thread currentThread = Thread.CurrentThread;
				LDictionary<int, ArchiveStorageFormat> threadStorageFormatCfg = this._threadStorageFormatCfg;
				lock (threadStorageFormatCfg)
				{
					if (value != null)
					{
						this._threadStorageFormatCfg[currentThread.ManagedThreadId] = value;
					}
					else
					{
						this._threadStorageFormatCfg.Remove(currentThread.ManagedThreadId);
					}
				}
			}
		}

		// Token: 0x17000271 RID: 625
		// (get) Token: 0x06000A51 RID: 2641 RVA: 0x00017974 File Offset: 0x00016974
		internal static ArchiveStorageConfig Singleton
		{
			get
			{
				ArchiveStorageConfig result;
				if ((result = ArchiveStorageConfig._instance) == null)
				{
					result = (ArchiveStorageConfig._instance = new ArchiveStorageConfig());
				}
				return result;
			}
		}

		// Token: 0x06000A52 RID: 2642 RVA: 0x0001798A File Offset: 0x0001698A
		private ArchiveStorageConfig()
		{
		}

		// Token: 0x0400017D RID: 381
		private readonly LDictionary<int, ArchiveStorageFormat> _threadStorageFormatCfg = new LDictionary<int, ArchiveStorageFormat>();

		// Token: 0x0400017E RID: 382
		private static ArchiveStorageConfig _instance;
	}
}
