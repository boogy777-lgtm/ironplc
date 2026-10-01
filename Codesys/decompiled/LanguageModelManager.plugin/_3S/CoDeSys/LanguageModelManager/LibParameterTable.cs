using System;
using System.Collections;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000116 RID: 278
	internal class LibParameterTable : ILibParameterTable2, ILibParameterTable
	{
		// Token: 0x170005BB RID: 1467
		// (get) Token: 0x060014C1 RID: 5313 RVA: 0x0003C7AE File Offset: 0x0003B7AE
		internal CaseInsensitiveHashtable Table
		{
			get
			{
				return this._cih;
			}
		}

		// Token: 0x170005BC RID: 1468
		// (get) Token: 0x060014C2 RID: 5314 RVA: 0x0003C7AE File Offset: 0x0003B7AE
		public IDictionary ParameterTable
		{
			get
			{
				return this._cih;
			}
		}

		// Token: 0x060014C3 RID: 5315 RVA: 0x0003C7B6 File Offset: 0x0003B7B6
		public void AddParameter(string stName, IExpression expression)
		{
			if (this._cih == null)
			{
				this._cih = new CaseInsensitiveHashtable();
			}
			this._cih.Add(stName, expression);
		}

		// Token: 0x060014C4 RID: 5316 RVA: 0x0003C7D8 File Offset: 0x0003B7D8
		public void AddParameter(string stName, string stValue)
		{
			IExpression expression;
			if (stValue == null)
			{
				expression = new ErrorExpression();
			}
			else
			{
				_IParser iparser = CompilerProxy.CreateParser(stValue);
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500)
				{
					expression = iparser.ParseInitialisation();
					if (expression == null)
					{
						expression = new ErrorExpression();
					}
				}
				else
				{
					expression = iparser.ParseExpression();
				}
			}
			this.AddParameter(stName, expression);
		}

		// Token: 0x040004CD RID: 1229
		private CaseInsensitiveHashtable _cih;
	}
}
