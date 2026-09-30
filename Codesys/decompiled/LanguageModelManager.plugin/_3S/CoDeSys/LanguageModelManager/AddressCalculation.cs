using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200014D RID: 333
	public class AddressCalculation : IAddressCalculation
	{
		// Token: 0x06001B74 RID: 7028 RVA: 0x0004DD55 File Offset: 0x0004CD55
		private AddressCalculation(CompileContext comcon)
		{
			this._comcon = comcon;
		}

		// Token: 0x06001B75 RID: 7029 RVA: 0x0004DD64 File Offset: 0x0004CD64
		internal static IAddressCalculation Create(ICompileContext comconIn)
		{
			CompileContext compileContext = comconIn as CompileContext;
			CompileContext compileContext2;
			if (compileContext.DataManager.Count == 0)
			{
				compileContext2 = (compileContext.Duplicate() as CompileContext);
				compileContext2.DataManager = null;
				_IMemorySettings memorySettings = compileContext.DataManager._MemorySettings;
				LList<_IArea> llist = new LList<_IArea>(1);
				llist.AddRange((memorySettings as MemorySettings).AreaList);
				compileContext2.ConfigureMemory(memorySettings, llist, 0);
			}
			else
			{
				compileContext2 = compileContext;
			}
			return new AddressCalculation(compileContext2);
		}

		// Token: 0x06001B76 RID: 7030 RVA: 0x0004DDD1 File Offset: 0x0004CDD1
		public IDataLocation CalculateAddress(IDirectVariable dirvar, out bool bError)
		{
			return this._comcon.LocateAddress(out bError, dirvar);
		}

		// Token: 0x040005C5 RID: 1477
		private readonly CompileContext _comcon;
	}
}
