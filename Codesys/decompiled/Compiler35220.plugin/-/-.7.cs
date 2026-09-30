using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0017
{
	// Token: 0x02000056 RID: 86
	internal static class \u0001
	{
		// Token: 0x06000613 RID: 1555 RVA: 0x0000CAF4 File Offset: 0x0000ACF4
		internal static void \u0001(_ISignature \u0002, IScope5 \u0003, IList<_ISignature> \u0004)
		{
			foreach (_ISignature isignature in \u0003.All.OfType<_ISignature>())
			{
				if (isignature.BaseSignatureId == \u0002.Id)
				{
					\u0004.Add(isignature);
					\u0017.\u0001.\u0001(isignature, \u0003, \u0004);
				}
				foreach (int num in isignature.InterfaceIds)
				{
					if (\u0002.Id == num)
					{
						\u0004.Add(isignature);
						\u0017.\u0001.\u0001(isignature, \u0003, \u0004);
					}
				}
			}
		}

		// Token: 0x06000614 RID: 1556 RVA: 0x0000CB90 File Offset: 0x0000AD90
		internal static void \u0002(_ISignature \u0002, IScope5 \u0003, IList<_ISignature> \u0004)
		{
			_ISignature isignature = \u0003[\u0002.BaseSignatureId] as _ISignature;
			if (isignature != null)
			{
				\u0004.Add(isignature);
				string u = \u0002.Name + " -> " + isignature.Name;
				\u0017.\u0001.\u0001(\u0002, isignature, u, \u0003, \u0004);
			}
			foreach (int nId in \u0002.InterfaceIds)
			{
				_ISignature isignature2 = \u0003[nId] as _ISignature;
				if (isignature2 != null)
				{
					\u0004.Add(isignature2);
					string u = \u0002.Name + " -> " + isignature2.Name;
					\u0017.\u0001.\u0001(\u0002, isignature2, u, \u0003, \u0004);
				}
			}
		}

		// Token: 0x06000615 RID: 1557 RVA: 0x0000CC34 File Offset: 0x0000AE34
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, string \u0004, IScope5 \u0005, IList<_ISignature> \u0006)
		{
			if (\u000E.\u0001(\u0002, \u0005))
			{
				return;
			}
			if (\u0002.Id == \u0003.Id)
			{
				\u0002.ResetBaseSignatureId();
				\u0002.AddMessage(Severity.Error, MessageId.Err_SelfInheritanceBase, new object[]
				{
					\u0004
				});
				return;
			}
			_ISignature isignature = \u0005[\u0003.BaseSignatureId] as _ISignature;
			if (isignature != null)
			{
				\u0006.Add(isignature);
				string u = \u0004 + " -> " + isignature.Name;
				\u0017.\u0001.\u0001(\u0002, isignature, u, \u0005, \u0006);
			}
			foreach (int nId in \u0003.InterfaceIds)
			{
				_ISignature isignature2 = \u0005[nId] as _ISignature;
				if (isignature2 != null)
				{
					\u0006.Add(isignature2);
					string u2 = \u0004 + " -> " + isignature2.Name;
					\u0017.\u0001.\u0001(\u0002, isignature2, u2, \u0005, \u0006);
				}
			}
		}

		// Token: 0x06000616 RID: 1558 RVA: 0x0000CD04 File Offset: 0x0000AF04
		internal static void \u0003(_ISignature \u0002, IScope5 \u0003, IList<_ISignature> \u0004)
		{
			_ISignature isignature = \u0003[\u0002.BaseSignatureId] as _ISignature;
			if (isignature != null)
			{
				\u0004.Add(isignature);
				\u0017.\u0001.\u0003(isignature, \u0003, \u0004);
			}
			foreach (int nId in \u0002.InterfaceIds)
			{
				_ISignature isignature2 = \u0003[nId] as _ISignature;
				if (isignature2 != null)
				{
					\u0004.Add(isignature2);
					\u0017.\u0001.\u0003(isignature2, \u0003, \u0004);
				}
			}
		}
	}
}
