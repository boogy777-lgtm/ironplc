using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000143 RID: 323
	public class DirVarLocationTable : _IDirvarLocationTable
	{
		// Token: 0x06001B28 RID: 6952 RVA: 0x0004D410 File Offset: 0x0004C410
		public bool TryGetLocationInfo(IDirectVariable _dirvar, IVariable2 _var, out _IDirectLocationInfo dirlocinfo)
		{
			return this._dicDirvarLocation.TryGetValue(new DirectVariableWrapper(_dirvar, _var), ref dirlocinfo);
		}

		// Token: 0x06001B29 RID: 6953 RVA: 0x0004D425 File Offset: 0x0004C425
		public void AddLocationInfo(IDirectVariable _dirvar, IVariable2 _var, IDataLocation datloc, IMessage message, bool bError)
		{
			this._dicDirvarLocation[new DirectVariableWrapper(_dirvar, _var)] = new DirectLocationInfo(datloc, message, bError);
		}

		// Token: 0x06001B2A RID: 6954 RVA: 0x0004D444 File Offset: 0x0004C444
		public IDirectVariable FindDirectVariable(IAbsoluteAddressInfo addressInfo)
		{
			if (addressInfo != null)
			{
				foreach (KeyValuePair<DirectVariableWrapper, _IDirectLocationInfo> keyValuePair in this._dicDirvarLocation)
				{
					if ((int)keyValuePair.Value.DatLoc.Area == addressInfo.Area && keyValuePair.Value.DatLoc.BitNr == addressInfo.BitOffset && keyValuePair.Value.DatLoc.Offset == addressInfo.Offset)
					{
						return keyValuePair.Key._dirvar;
					}
				}
			}
			return null;
		}

		// Token: 0x040005B3 RID: 1459
		private readonly LDictionary<DirectVariableWrapper, _IDirectLocationInfo> _dicDirvarLocation = new LDictionary<DirectVariableWrapper, _IDirectLocationInfo>();
	}
}
