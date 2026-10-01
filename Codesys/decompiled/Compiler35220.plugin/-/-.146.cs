using System;
using System.Linq;
using \u0015;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x0200019A RID: 410
	internal static class \u0006
	{
		// Token: 0x06001D8C RID: 7564 RVA: 0x0005F984 File Offset: 0x0005DB84
		internal static _ISignature \u0001(\u0002 \u0002, IUserdefType \u0003)
		{
			bool u = \u0002.IgnoreActions;
			\u0002.IgnoreActions = true;
			_ISignature result = \u0002.FindSignature(\u0003) as _ISignature;
			\u0002.IgnoreActions = u;
			return result;
		}

		// Token: 0x06001D8D RID: 7565 RVA: 0x0005F9B4 File Offset: 0x0005DBB4
		internal static _ISignature \u0001(\u0002 \u0002, _IExpression \u0003)
		{
			bool u = \u0002.IgnoreActions;
			\u0002.IgnoreActions = true;
			ISignature[] array = \u0002.FindSignature(\u0003);
			\u0002.IgnoreActions = u;
			if (array == null)
			{
				return null;
			}
			return (_ISignature)array.FirstOrDefault<ISignature>();
		}

		// Token: 0x06001D8E RID: 7566 RVA: 0x0005F9F0 File Offset: 0x0005DBF0
		internal static void \u0001(IExpression \u0002, bool \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			\u0006.\u0001(\u0002, \u0003, \u0004, \u0005, new Action<_ISignature, _ISignature>(\u0006.\u0004));
		}

		// Token: 0x06001D8F RID: 7567 RVA: 0x0005FA08 File Offset: 0x0005DC08
		internal static void \u0002(IExpression \u0002, bool \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			\u0006.\u0001(\u0002, \u0003, \u0004, \u0005, new Action<_ISignature, _ISignature>(\u0006.\u0003));
		}

		// Token: 0x06001D90 RID: 7568 RVA: 0x0005FA20 File Offset: 0x0005DC20
		internal static void \u0001(_IVariableExpression \u0002, _IVariable \u0003, IType \u0004, \u0002 \u0005, _ISignature \u0006, _ISignature \u0007)
		{
			\u0003.AddPrecompileCrossReference(\u0006.PrecompileId);
			\u0002.PrecompileVariableId = \u0003.PrecompileId;
			\u0002.PrecompileSignatureId = \u0007.PrecompileId;
			_IUserdefType iuserdefType = \u0004 as _IUserdefType;
			if (iuserdefType != null)
			{
				_ISignature isignature = \u0006.\u0001(\u0005.\u0001(\u0007), iuserdefType);
				if (isignature != null)
				{
					_ISignature isignature2 = isignature;
					isignature2.AddPrecompileCaller(\u0006.PrecompileId);
					\u0006.AddPrecompileCallee(isignature2.PrecompileId, false);
					iuserdefType.SignatureId = isignature2.PrecompileId;
				}
			}
		}

		// Token: 0x06001D91 RID: 7569 RVA: 0x0005FA98 File Offset: 0x0005DC98
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			\u0006.\u0003(\u0002, \u0003);
		}

		// Token: 0x06001D92 RID: 7570 RVA: 0x0005FAA4 File Offset: 0x0005DCA4
		internal static void \u0002(_ISignature \u0002, _ISignature \u0003)
		{
			\u0006.\u0004(\u0002, \u0003);
		}

		// Token: 0x06001D93 RID: 7571 RVA: 0x0005FAB0 File Offset: 0x0005DCB0
		internal static void \u0001(_IUserdefType \u0002, _ISignature \u0003, \u0002 \u0004)
		{
			_ISignature isignature = (_ISignature)\u0004.FindSignature(\u0002);
			((_IExpression)\u0002.NameExpression).PrecompileSignatureId = \u0002.SignatureId;
			if (isignature != null)
			{
				isignature.AddPrecompileDeclarer(\u0003.PrecompileId);
				\u0002.SignatureId = isignature.PrecompileId;
			}
		}

		// Token: 0x06001D94 RID: 7572 RVA: 0x0005FAFC File Offset: 0x0005DCFC
		internal static void \u0001(IExpression \u0002, _ISignature \u0003)
		{
			_IVariableExpression ivariableExpression = \u0002 as _IVariableExpression;
			if (ivariableExpression != null)
			{
				ivariableExpression.PrecompileSignatureId = \u0003.PrecompileId;
				return;
			}
			_INamespaceAccessExpression inamespaceAccessExpression = \u0002 as _INamespaceAccessExpression;
			if (inamespaceAccessExpression != null)
			{
				\u0006.\u0001(inamespaceAccessExpression._Access, \u0003);
				return;
			}
			_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
			if (icompoAccessExpression == null)
			{
				return;
			}
			\u0006.\u0001(icompoAccessExpression._Right, \u0003);
		}

		// Token: 0x06001D95 RID: 7573 RVA: 0x0005FB50 File Offset: 0x0005DD50
		private static void \u0001(IExpression \u0002, bool \u0003, _ISignature \u0004, _ISignature \u0005, Action<_ISignature, _ISignature> \u0006)
		{
			_IVariableExpression ivariableExpression = \u0002 as _IVariableExpression;
			if (ivariableExpression != null)
			{
				\u0006.\u0001(ivariableExpression, \u0003, \u0004, \u0005, \u0006);
				return;
			}
			_INamespaceAccessExpression inamespaceAccessExpression = \u0002 as _INamespaceAccessExpression;
			if (inamespaceAccessExpression != null)
			{
				\u0006.\u0001(inamespaceAccessExpression._Access, \u0003, \u0004, \u0005, \u0006);
				return;
			}
			_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
			if (icompoAccessExpression == null)
			{
				return;
			}
			\u0006.\u0001(icompoAccessExpression._Right, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06001D96 RID: 7574 RVA: 0x0005FBAC File Offset: 0x0005DDAC
		private static void \u0001(_IVariableExpression \u0002, bool \u0003, _ISignature \u0004, _ISignature \u0005, Action<_ISignature, _ISignature> \u0006)
		{
			\u0002.PrecompileSignatureId = \u0005.PrecompileId;
			\u0006(\u0004, \u0005);
			\u0005.AddPrecompileCaller(\u0004.PrecompileId);
			\u0004.AddPrecompileCallee(\u0005.PrecompileId, false);
			if (\u0003)
			{
				\u0005.AddReferencer(\u0004.PrecompileId);
			}
		}

		// Token: 0x06001D97 RID: 7575 RVA: 0x0005FBEC File Offset: 0x0005DDEC
		private static void \u0003(_ISignature \u0002, _ISignature \u0003)
		{
			\u0003.AddPrecompileCaller(\u0002.PrecompileId);
			\u0002.AddPrecompileCallee(\u0003.PrecompileId, false);
		}

		// Token: 0x06001D98 RID: 7576 RVA: 0x0005FC08 File Offset: 0x0005DE08
		private static void \u0004(_ISignature \u0002, _ISignature \u0003)
		{
			\u0003.AddPrecompileDeclarer(\u0002.PrecompileId);
		}
	}
}
