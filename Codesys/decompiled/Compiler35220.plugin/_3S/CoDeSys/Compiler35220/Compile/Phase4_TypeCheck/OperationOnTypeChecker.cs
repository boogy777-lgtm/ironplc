using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Compile.Phase4_TypeCheck
{
	// Token: 0x02000300 RID: 768
	public class OperationOnTypeChecker
	{
		// Token: 0x06002EDD RID: 11997 RVA: 0x000B074C File Offset: 0x000AE94C
		private OperationOnTypeChecker(IScope5 scope, IErrorAdder adder)
		{
			this.Scope = scope;
			this.ErrorAdder = adder;
			this.Error = false;
		}

		// Token: 0x170007D8 RID: 2008
		// (get) Token: 0x06002EDE RID: 11998 RVA: 0x000B076C File Offset: 0x000AE96C
		private IScope5 Scope { get; }

		// Token: 0x170007D9 RID: 2009
		// (get) Token: 0x06002EDF RID: 11999 RVA: 0x000B0774 File Offset: 0x000AE974
		private IErrorAdder ErrorAdder { get; }

		// Token: 0x170007DA RID: 2010
		// (get) Token: 0x06002EE0 RID: 12000 RVA: 0x000B077C File Offset: 0x000AE97C
		// (set) Token: 0x06002EE1 RID: 12001 RVA: 0x000B0784 File Offset: 0x000AE984
		private bool Error { get; set; }

		// Token: 0x06002EE2 RID: 12002 RVA: 0x000B0790 File Offset: 0x000AE990
		public static bool CheckForValidOperationOnType(_IOperatorExpression op, IScope5 scope, IErrorAdder adder = null)
		{
			OperationOnTypeChecker operationOnTypeChecker = new OperationOnTypeChecker(scope, adder);
			operationOnTypeChecker.\u0001(op, op._OperandsList);
			return !operationOnTypeChecker.Error;
		}

		// Token: 0x06002EE3 RID: 12003 RVA: 0x000B07B0 File Offset: 0x000AE9B0
		private void \u0001(_IOperatorExpression \u0002, IList<_IExpression> \u0003)
		{
			foreach (_IExpression iexpression in \u0003)
			{
				if (\u0002.Code == Operator.__Delete && !this.\u0001(iexpression))
				{
					break;
				}
				bool flag = iexpression is _ICallExpression;
				bool flag2;
				if (!(iexpression is _IThisExpression))
				{
					_IDeRefAccessExpression ideRefAccessExpression = iexpression as _IDeRefAccessExpression;
					flag2 = (ideRefAccessExpression != null && ideRefAccessExpression._Base is _IThisExpression);
				}
				else
				{
					flag2 = true;
				}
				bool flag3 = flag2;
				if (!flag && !flag3)
				{
					_ISignature isignature = iexpression.GetSignature(this.Scope) as _ISignature;
					if (\u0002.Code == Operator.Adr)
					{
						this.\u0001(iexpression);
					}
					isignature = this.\u0001(\u0002, isignature);
					if (this.\u0001(\u0002, iexpression, isignature))
					{
						break;
					}
				}
			}
		}

		// Token: 0x06002EE4 RID: 12004 RVA: 0x000B0884 File Offset: 0x000AEA84
		private bool \u0001(_IOperatorExpression \u0002, _IExpression \u0003, _ISignature \u0004)
		{
			bool result = false;
			if (\u0004 != null)
			{
				string text;
				bool flag;
				OperationOnTypeChecker.\u0001(\u0002, \u0004, this.Scope, out text, out flag);
				if (text == null)
				{
					text = \u0003.Type.ToString();
				}
				if (!flag)
				{
					this.\u0001(\u0003, MessageId.Err_OperationNotPossibleOnType, new object[]
					{
						\u0002.Code,
						text
					});
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06002EE5 RID: 12005 RVA: 0x000B08E0 File Offset: 0x000AEAE0
		private static void \u0001(_IOperatorExpression \u0002, _ISignature \u0003, IScope5 \u0004, out string \u0005, out bool \u0006)
		{
			\u0005 = null;
			\u0006 = false;
			Operator code = \u0002.Code;
			if (code <= Operator.__MaxOffset)
			{
				switch (code)
				{
				case Operator.Adr:
				case Operator.IndexOf:
				{
					Operator poutype = \u0003.POUType;
					if (poutype - Operator.Function <= 1 || poutype == Operator.Program)
					{
						\u0006 = true;
						return;
					}
					if (poutype != Operator.Method)
					{
						return;
					}
					ISignature signature = \u0004[\u0003.ParentSignatureId];
					if (signature == null || signature.POUType != Operator.Interface)
					{
						\u0006 = true;
						return;
					}
					\u0005 = signature.OrgName + "." + \u0003.OrgName;
					return;
				}
				case Operator.BitAdr:
					return;
				case Operator.SizeOf:
					goto IL_5B;
				default:
					if (code - Operator.__CRC > 1)
					{
						return;
					}
					break;
				}
			}
			else if (code != Operator.__FCall)
			{
				if (code == Operator.__CallInitFunction)
				{
					\u0006 = (\u0003.POUType == Operator.Program || \u0003.POUType == Operator.VarGlobal);
					return;
				}
				if (code != Operator.XSizeOf)
				{
					return;
				}
				goto IL_5B;
			}
			\u0006 = true;
			return;
			IL_5B:
			bool flag = \u0003.POUType == Operator.FunctionBlock && !\u0003.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants);
			bool flag2 = \u0003.GetFlag(SignatureFlag.Enum) && \u0003.AllVariables.Count != 0;
			if (flag || \u0003.GetFlag(SignatureFlag.Structure) || \u0003.HasAttribute("subsequent") || flag2)
			{
				\u0006 = true;
				return;
			}
		}

		// Token: 0x06002EE6 RID: 12006 RVA: 0x000B0A14 File Offset: 0x000AEC14
		private _ISignature \u0001(_IOperatorExpression \u0002, _ISignature \u0003)
		{
			if (\u0003 != null && \u0003.GetFlag(SignatureFlag.Alias) && Helper.\u0001(\u0002))
			{
				ICompiledType compiledType = \u0003.AllVariables.First<_IVariable>().CompiledType;
				if (compiledType.Class == TypeClass.Userdef)
				{
					\u0003 = (this.Scope.ApplicationContext.GetSignatureById(((IUserdefType)compiledType).SignatureId) as _ISignature);
				}
				else
				{
					\u0003 = null;
				}
			}
			return \u0003;
		}

		// Token: 0x06002EE7 RID: 12007 RVA: 0x000B0A7C File Offset: 0x000AEC7C
		private bool \u0001(_IExpression \u0002)
		{
			if (\u0002 is _IThisExpression || \u0002 is _IBaseExpression)
			{
				this.\u0001(\u0002, MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					Operator.__Delete,
					\u0002.ToString()
				});
				return false;
			}
			return true;
		}

		// Token: 0x06002EE8 RID: 12008 RVA: 0x000B0AB8 File Offset: 0x000AECB8
		private void \u0001(_IExpression \u0002)
		{
			_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
			_IVariable ivariable = \u0002.GetVariable(this.Scope) as _IVariable;
			if (ivariable != null && !ivariable.HasFlag(VarFlag.Absolut | VarFlag.Static) && icompoAccessExpression != null)
			{
				_IExpression iexpression = this.\u0001(icompoAccessExpression);
				ISignature signature = iexpression.GetSignature(this.Scope);
				if (iexpression.IsPOUReference && signature != null && !OperationOnTypeChecker.\u0001(signature))
				{
					this.\u0001(\u0002, MessageId.Err_AddressOfNonInstanceVar, new object[]
					{
						\u0002
					});
				}
			}
		}

		// Token: 0x06002EE9 RID: 12009 RVA: 0x000B0B30 File Offset: 0x000AED30
		private _IExpression \u0001(_ICompoAccessExpression \u0002)
		{
			_ICompoAccessExpression icompoAccessExpression = \u0002._Left as _ICompoAccessExpression;
			if (icompoAccessExpression == null)
			{
				return \u0002._Left;
			}
			ISignature signature = ((_IExpression)icompoAccessExpression.Right).GetSignature(this.Scope);
			if (signature != null && !OperationOnTypeChecker.\u0001(signature))
			{
				return (_IExpression)icompoAccessExpression.Right;
			}
			return this.\u0001(icompoAccessExpression);
		}

		// Token: 0x06002EEA RID: 12010 RVA: 0x000B0B88 File Offset: 0x000AED88
		private static bool \u0001(ISignature \u0002)
		{
			return Operator.Program == \u0002.POUType || (Operator.VarGlobal == \u0002.POUType && !\u0002.GetFlag(SignatureFlag.Enum));
		}

		// Token: 0x06002EEB RID: 12011 RVA: 0x000B0BB0 File Offset: 0x000AEDB0
		private void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			this.Error = true;
			IErrorAdder errorAdder = this.ErrorAdder;
			if (errorAdder == null)
			{
				return;
			}
			errorAdder.AddError(\u0002, \u0003, \u0004);
		}

		// Token: 0x040008ED RID: 2285
		[CompilerGenerated]
		private readonly IScope5 \u0001;

		// Token: 0x040008EE RID: 2286
		[CompilerGenerated]
		private readonly IErrorAdder \u0001;

		// Token: 0x040008EF RID: 2287
		[CompilerGenerated]
		private bool \u0001;
	}
}
