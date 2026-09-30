using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0080
{
	// Token: 0x020000B6 RID: 182
	internal sealed class \u0004
	{
		// Token: 0x17000439 RID: 1081
		// (get) Token: 0x06000E41 RID: 3649 RVA: 0x00026A0C File Offset: 0x00024C0C
		internal IEnumerable<_ISignature> Interfaces
		{
			get
			{
				foreach (int nId in this.\u0001)
				{
					yield return this.\u0001[nId] as _ISignature;
				}
				HashSet<int>.Enumerator enumerator = default(HashSet<int>.Enumerator);
				yield break;
				yield break;
			}
		}

		// Token: 0x1700043A RID: 1082
		// (get) Token: 0x06000E42 RID: 3650 RVA: 0x00026A1C File Offset: 0x00024C1C
		internal IEnumerable<_ISignature> InterfaceMethods
		{
			get
			{
				return this.\u0001.Values;
			}
		}

		// Token: 0x06000E43 RID: 3651 RVA: 0x00026A2C File Offset: 0x00024C2C
		internal \u0004(_ISignature \u001C\u0002, IScope5 \u009B\u0002, Func<_ISignature, bool> \u0013\u0008 = null)
		{
			this.\u0001 = \u001C\u0002;
			this.\u0001 = \u009B\u0002;
			this.\u0001 = \u0013\u0008;
		}

		// Token: 0x06000E44 RID: 3652 RVA: 0x00026A60 File Offset: 0x00024C60
		internal void \u0001()
		{
			this.\u0001(this.\u0001);
		}

		// Token: 0x06000E45 RID: 3653 RVA: 0x00026A70 File Offset: 0x00024C70
		private void \u0001(_ISignature \u0002)
		{
			foreach (int num in \u0002.InterfaceIds)
			{
				if (this.\u0001.Add(num))
				{
					_ISignature u = (_ISignature)this.\u0001[num];
					this.\u0002(u);
				}
			}
		}

		// Token: 0x06000E46 RID: 3654 RVA: 0x00026AC0 File Offset: 0x00024CC0
		private void \u0002(_ISignature \u0002)
		{
			if (\u0002.BaseSignatureId != Helper.InvalidId && this.\u0001.Add(\u0002.BaseSignatureId))
			{
				_ISignature u = (_ISignature)this.\u0001[\u0002.BaseSignatureId];
				this.\u0002(u);
			}
			foreach (object obj in \u0002._SubSignatures)
			{
				_ISignature isignature = (_ISignature)obj;
				if (!this.\u0001.ContainsKey(isignature.Name) && this.\u0001(isignature))
				{
					this.\u0001.Add(isignature.Name, isignature);
				}
			}
			this.\u0001(\u0002);
		}

		// Token: 0x04000261 RID: 609
		private readonly _ISignature \u0001;

		// Token: 0x04000262 RID: 610
		private readonly IScope5 \u0001;

		// Token: 0x04000263 RID: 611
		private readonly HashSet<int> \u0001 = new HashSet<int>();

		// Token: 0x04000264 RID: 612
		private readonly IDictionary<string, _ISignature> \u0001 = new Dictionary<string, _ISignature>();

		// Token: 0x04000265 RID: 613
		private readonly Func<_ISignature, bool> \u0001;
	}
}
