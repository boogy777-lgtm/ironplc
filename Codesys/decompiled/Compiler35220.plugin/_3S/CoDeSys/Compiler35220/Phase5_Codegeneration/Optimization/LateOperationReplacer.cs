using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u000E;
using \u000F;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002C1 RID: 705
	public class LateOperationReplacer : AbstractReplacer, IReplacer
	{
		// Token: 0x06002AFA RID: 11002 RVA: 0x0009780C File Offset: 0x00095A0C
		internal LateOperationReplacer(global::\u000E.\u0011 context)
		{
			this.\u0001 = context;
			this.\u0001 = \u0081.\u0010.\u0001(this, context);
		}

		// Token: 0x17000774 RID: 1908
		// (get) Token: 0x06002AFB RID: 11003 RVA: 0x00097828 File Offset: 0x00095A28
		// (set) Token: 0x06002AFC RID: 11004 RVA: 0x00097830 File Offset: 0x00095A30
		private _ICompiledPOU POU { get; set; }

		// Token: 0x06002AFD RID: 11005 RVA: 0x0009783C File Offset: 0x00095A3C
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.POU = cpou;
			this.\u0001.ReplaceCode(cpou);
		}

		// Token: 0x06002AFE RID: 11006 RVA: 0x00097854 File Offset: 0x00095A54
		public override _IExpression ReplaceOperatorExpression(_IOperatorExpression operatorExpression)
		{
			Func<_IOperatorExpression, global::\u000E.\u0011, _ICompiledPOU, _IExpression> func;
			if (LateOperationReplacer.\u0001.TryGetValue(operatorExpression.Code, out func))
			{
				return func(operatorExpression, this.\u0001, this.POU);
			}
			return LateOperationReplacer.\u0001(operatorExpression, this.\u0001._Scope);
		}

		// Token: 0x06002AFF RID: 11007 RVA: 0x000978A0 File Offset: 0x00095AA0
		public override _IExpression ReplaceVariableExpression(_IVariableExpression variableExpression, bool bReadAccess)
		{
			return LateOperationReplacer.\u0001(variableExpression, this.\u0001._Scope);
		}

		// Token: 0x06002B00 RID: 11008 RVA: 0x000978C4 File Offset: 0x00095AC4
		public override _IExpression ReplaceCompoAccessExpression(_ICompoAccessExpression compoAccessExpression, bool bReadAccess)
		{
			return LateOperationReplacer.\u0001(compoAccessExpression, this.\u0001._Scope);
		}

		// Token: 0x06002B01 RID: 11009 RVA: 0x000978E8 File Offset: 0x00095AE8
		public override _IExpression ReplacePartialAccessExpression(_IPartialAccessExpression partialAccessExpression, bool bReadAccess)
		{
			return LateOperationReplacer.\u0001(partialAccessExpression, this.\u0001._Scope);
		}

		// Token: 0x06002B02 RID: 11010 RVA: 0x0009790C File Offset: 0x00095B0C
		internal static _IExpression \u0001(_IExpression \u0002, IScope5 \u0003)
		{
			ILiteralValue literalValue = null;
			if (\u0002.IsConstant(\u0003, false))
			{
				literalValue = \u0002.Literal(\u0003);
			}
			TypeClass @class = \u0002.Type.Class;
			_IEnumType ienumType = \u0002.Type as _IEnumType;
			if (ienumType != null)
			{
				@class = ienumType._Base.Class;
			}
			_ILiteralExpression iliteralExpression = null;
			if (literalValue != null)
			{
				switch (literalValue.KindOf)
				{
				case KindOfLiteral.SignedInteger:
					iliteralExpression = global::\u0019.\u0003.\u0001(literalValue.SignedLong, @class);
					break;
				case KindOfLiteral.UnsignedInteger:
					iliteralExpression = global::\u0019.\u0003.\u0001(literalValue.UnsignedLong);
					break;
				case KindOfLiteral.Float:
					iliteralExpression = global::\u0019.\u0003.\u0001(literalValue.Float);
					break;
				case KindOfLiteral.Bool:
				{
					bool @bool = literalValue.Bool;
					long u = 0L;
					if (@bool)
					{
						u = 1L;
					}
					iliteralExpression = global::\u0019.\u0003.\u0001(u, @class);
					break;
				}
				}
			}
			if (iliteralExpression != null)
			{
				iliteralExpression.Type = \u0002.Type;
				iliteralExpression._Position = \u0002._Position;
				return iliteralExpression;
			}
			return \u0002;
		}

		// Token: 0x04000822 RID: 2082
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x04000823 RID: 2083
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x04000824 RID: 2084
		[CompilerGenerated]
		private _ICompiledPOU \u0001;

		// Token: 0x04000825 RID: 2085
		private static readonly Dictionary<Operator, Func<_IOperatorExpression, global::\u000E.\u0011, _ICompiledPOU, _IExpression>> \u0001 = new Dictionary<Operator, Func<_IOperatorExpression, global::\u000E.\u0011, _ICompiledPOU, _IExpression>>
		{
			{
				Operator.__IsValidRef,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _ICompiledPOU, _IExpression>(global::\u000F.\u0013.\u0001)
			}
		};
	}
}
