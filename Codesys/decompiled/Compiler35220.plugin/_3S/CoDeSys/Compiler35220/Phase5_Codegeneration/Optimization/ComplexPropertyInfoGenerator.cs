using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0001;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u007F;
using \u0083;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002B0 RID: 688
	internal sealed class ComplexPropertyInfoGenerator
	{
		// Token: 0x17000765 RID: 1893
		// (get) Token: 0x06002AA9 RID: 10921 RVA: 0x000958F0 File Offset: 0x00093AF0
		private IScope5 Scope { get; }

		// Token: 0x17000766 RID: 1894
		// (get) Token: 0x06002AAA RID: 10922 RVA: 0x000958F8 File Offset: 0x00093AF8
		private _ICompileContext Comcon { get; }

		// Token: 0x06002AAB RID: 10923 RVA: 0x00095900 File Offset: 0x00093B00
		internal ComplexPropertyInfoGenerator(IScope5 scope, _ICompileContext comcon)
		{
			this.Scope = scope;
			this.Comcon = comcon;
		}

		// Token: 0x06002AAC RID: 10924 RVA: 0x00095918 File Offset: 0x00093B18
		public _ISequenceStatement \u0001(string \u0002, _IExpression \u0003, _IExpression \u0004, global::\u000E.\u0011 \u0005, bool \u0006, bool \u0007, bool \u0008)
		{
			\u007F.\u0008 u = new \u007F.\u0008(this.Comcon);
			_IExpression iexpression = \u0005.Generator.\u0001<_IExpression>((_IExpression)\u0003.Duplicate(), \u0005._Scope, \u0005.CompiledPOU);
			_IExpression iexpression2 = \u0005.Generator.\u0001<_IExpression>((_IExpression)\u0004.Duplicate(), \u0005._Scope, \u0005.CompiledPOU);
			if (\u0008)
			{
				iexpression = ((_IDeRefAccessExpression)iexpression)._Base;
			}
			_IType itype = (_IType)iexpression.Type.BaseType;
			_ISignature isignature = (_ISignature)((_IUserdefType)itype.DeRefType).GetSignature(this.Scope);
			_ISignature u2 = isignature;
			if (isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				_IUserdefType iuserdefType = isignature["__Interface"].CompiledType.BaseType as _IUserdefType;
				if (iuserdefType != null)
				{
					u2 = (_ISignature)iuserdefType.GetSignature(this.Scope);
				}
			}
			if (iexpression.Type.Class == TypeClass.Reference)
			{
				iexpression.Type = \u0019.\u0003.\u0001(itype);
			}
			_ISignature isignature2 = (_ISignature)((_IUserdefType)iexpression2.Type).GetSignature(this.Scope);
			_ISignature isignature3 = (_ISignature)this.Scope[isignature2.BaseSignatureId];
			_IVariable ivariable = (_IVariable)isignature3.All.First(new Func<IVariable, bool>(ComplexPropertyInfoGenerator.<>c.<>9.\u0001));
			_ISignature u4;
			_IVariable u3 = this.\u0001((_IType)iexpression.Type, this.Scope, out u4);
			_IExpression iexpression3 = u.\u0001(u3, u4);
			_IExpression u5;
			_IExpression iexpression4;
			this.\u0001(isignature, itype, iexpression3, out u5, out iexpression4);
			\u0083.\u0005 u6 = global::\u0001.\u000E.\u0001(this.Scope, \u0002, u2, isignature);
			List<IStatement> list = new List<IStatement>();
			list.Add(u.\u0001((_IExpression)iexpression3.Duplicate(), (_IExpression)iexpression.Duplicate()));
			_IVariable ivariable2 = (_IVariable)isignature2.All.First(new Func<IVariable, bool>(ComplexPropertyInfoGenerator.<>c.<>9.\u0002));
			_IVariable ivariable3 = (_IVariable)isignature2.All.First(new Func<IVariable, bool>(ComplexPropertyInfoGenerator.<>c.<>9.\u0003));
			_IVariable ivariable4 = (_IVariable)isignature2.All.First(new Func<IVariable, bool>(ComplexPropertyInfoGenerator.<>c.<>9.\u0004));
			_IType u7 = (_IType)iexpression4._CompiledType.BaseType;
			_IExpression iexpression5;
			if (!u6.HasGetter)
			{
				iexpression5 = u.\u0001(ivariable2._Type);
			}
			else
			{
				_IExpression iexpression6 = u.\u0001((_IExpression)iexpression4.Duplicate(), u.\u0001((long)u6.\u0002), u7);
				iexpression5 = iexpression6;
			}
			_IExpression iexpression7 = iexpression5;
			_IExpression iexpression8;
			if (!u6.HasSetter)
			{
				iexpression8 = u.\u0001(ivariable3._Type);
			}
			else
			{
				_IExpression iexpression6 = u.\u0001((_IExpression)iexpression4.Duplicate(), u.\u0001((long)u6.\u0001), u7);
				iexpression8 = iexpression6;
			}
			_IExpression iexpression9 = iexpression8;
			if (this.Comcon.NewVFTable)
			{
				iexpression7 = u.\u0001(Operator.Adr, iexpression7, ivariable2._Type);
				iexpression9 = u.\u0001(Operator.Adr, iexpression9, ivariable3._Type);
			}
			List<IStatement> u8 = new List<IStatement>
			{
				u.\u0001(iexpression2, isignature2, ivariable2, iexpression7),
				u.\u0001(iexpression2, isignature2, ivariable3, iexpression9),
				u.\u0001(iexpression2, isignature2, ivariable4, u5)
			};
			List<IStatement> u9 = new List<IStatement>
			{
				u.\u0001(iexpression2, isignature2, ivariable2, u.\u0001(ivariable2._Type)),
				u.\u0001(iexpression2, isignature2, ivariable3, u.\u0001(ivariable3._Type)),
				u.\u0001(iexpression2, isignature2, ivariable4, u.\u0001(ivariable4._Type))
			};
			IExpression u11;
			if (\u0006)
			{
				_IExpression u10 = (_IExpression)iexpression3.Duplicate();
				if (\u0007)
				{
					u10 = u.\u0001(u10, itype);
				}
				_IVariable ivariable5 = (_IVariable)isignature.All.First(new Func<IVariable, bool>(ComplexPropertyInfoGenerator.<>c.<>9.\u0005));
				u11 = u.\u0001(Operator.NotEqual, u.\u0001(ivariable5._Type), u.\u0001(u10, u.\u0001(ivariable5, isignature), ivariable5._Type), \u0019.\u0003.\u0001());
			}
			else
			{
				u11 = u.\u0001(Operator.NotEqual, u.\u0001(iexpression3._CompiledType), (_IExpression)iexpression3.Duplicate(), \u0019.\u0003.\u0001());
			}
			_IIfStatement item = \u0019.\u0003.\u0001(u11, \u0019.\u0003.\u0001(u8), \u0019.\u0003.\u0001(u9));
			list.Add(item);
			list.Add(u.\u0001(iexpression2, isignature3, ivariable, u.\u0001(ivariable._Type)));
			return \u0019.\u0003.\u0001(list);
		}

		// Token: 0x06002AAD RID: 10925 RVA: 0x00095E10 File Offset: 0x00094010
		private _IVariable \u0001(_IType \u0002, IScope5 \u0003, out _ISignature \u0004)
		{
			\u0004 = (_ISignature)this.Comcon.GetSignatureById(\u0003.MostLocalSignatureId);
			if (\u0004.POUType == Operator.FunctionBlock)
			{
				\u0004 = (_ISignature)\u0004.GetSubSignature(IdentifierConstants.MainSignatureName);
			}
			string pouuniqueLabel = ArrayInitialisationCodeGenerator.GetPOUUniqueLabel("__HelpForPropertyInfo__{0}");
			\u0019.\u0001.\u0001(this.Comcon, \u0004, null, pouuniqueLabel, \u0002._Duplicate(true));
			Locator.\u0001(this.Comcon.DataManager, this.Comcon, null, \u0004, null);
			return (_IVariable)\u0004[pouuniqueLabel];
		}

		// Token: 0x06002AAE RID: 10926 RVA: 0x00095EA0 File Offset: 0x000940A0
		private void \u0001(_ISignature \u0002, _IType \u0003, _IExpression \u0004, out _IExpression \u0005, out _IExpression \u0006)
		{
			\u007F.\u0008 u = new \u007F.\u0008(this.Comcon);
			_IVariable ivariable = (_IVariable)\u0002.All.First(new Func<IVariable, bool>(ComplexPropertyInfoGenerator.<>c.<>9.\u0006));
			_IDeRefAccessExpression u2 = u.\u0001((_IExpression)\u0004.Duplicate(), \u0003);
			_IVariableExpression u3 = u.\u0001(ivariable, \u0002);
			_IType itype = (_IType)ivariable._Type.BaseType;
			_IType itype2 = (_IType)itype.BaseType;
			_IType itype3 = (_IType)itype2.BaseType;
			if (!\u0002.HasFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				\u0005 = (_IExpression)\u0004.Duplicate();
				\u0006 = u.\u0001(u.\u0001(u2, u3, ivariable._Type), itype);
				return;
			}
			_ICompoAccessExpression icompoAccessExpression = u.\u0001(u2, u3, ivariable._Type);
			_IDeRefAccessExpression ideRefAccessExpression = u.\u0001(u.\u0001((_IExpression)icompoAccessExpression.Duplicate(), itype), itype2);
			\u0006 = ideRefAccessExpression;
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE) || \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL))
			{
				\u0005 = (_IExpression)icompoAccessExpression.Duplicate();
				return;
			}
			if (TypeTable.GetSize(TypeClass.Pointer, this.Scope) == 8)
			{
				\u0005 = u.\u0001(Operator.Minus, (_IExpression)icompoAccessExpression.Duplicate(), u.\u0001(itype3, \u0019.\u0003.\u0001(), u.\u0001((_IExpression)\u0006.Duplicate(), u.\u0001(0L), itype3)), itype3);
				return;
			}
			\u0005 = u.\u0001(Operator.Minus, (_IExpression)icompoAccessExpression.Duplicate(), u.\u0001((_IExpression)\u0006.Duplicate(), u.\u0001(0L), itype3), itype3);
		}

		// Token: 0x04000808 RID: 2056
		[CompilerGenerated]
		private readonly IScope5 \u0001;

		// Token: 0x04000809 RID: 2057
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;
	}
}
