using System;
using \u000E;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x020002B6 RID: 694
	internal static class \u0011
	{
		// Token: 0x06002ABD RID: 10941 RVA: 0x0009642C File Offset: 0x0009462C
		internal static _IVariableExpression \u0001(IScope5 \u0002)
		{
			ISignature[] array;
			return \u0019.\u0003.\u0001(((_IVariable)\u0002.FindVariable(IdentifierConstants.InstancePointer, out array)[0]).VersionedName);
		}

		// Token: 0x06002ABE RID: 10942 RVA: 0x00096458 File Offset: 0x00094658
		internal static _IExpression \u0001(_IThisExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			_IVariableExpression u = \u001E.\u0011.\u0001(\u0003._Scope);
			_IVariableExpression ivariableExpression = \u0003.Generator.\u0001<_IVariableExpression>(u, \u0003._Scope, \u0003.CompiledPOU);
			\u0003.Generator.CopyPositionAndMessages(\u0002, ivariableExpression);
			return ivariableExpression;
		}

		// Token: 0x06002ABF RID: 10943 RVA: 0x000964A0 File Offset: 0x000946A0
		internal static _IExpression \u0001(_IBaseExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			_IVariableExpression u = \u001E.\u0011.\u0001(\u0003._Scope);
			_IVariableExpression ivariableExpression = \u0003.Generator.\u0001<_IVariableExpression>(u, \u0003._Scope, \u0003.CompiledPOU);
			ivariableExpression.Type = \u0002.Type;
			\u0003.Generator.CopyPositionAndMessages(\u0002, ivariableExpression);
			ivariableExpression.NoVirtual = true;
			return ivariableExpression;
		}
	}
}
