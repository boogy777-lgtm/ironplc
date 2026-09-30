using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Compile.Phase1_Typification.Code
{
	// Token: 0x02000301 RID: 769
	public static class ImplicitFunctionCallsHandler
	{
		// Token: 0x06002EEC RID: 12012 RVA: 0x000B0BCC File Offset: 0x000AEDCC
		public static _ISignature CheckForExternalFunctionCall(_ICompileContext compileContext, _IOperatorExpression op, IScope5 scope)
		{
			_ISignature result = null;
			if (compileContext.Codegenerator != null)
			{
				string empty = string.Empty;
				TypeClass typeClass = TypeClass.None;
				Operator code = op.Code;
				if (op.Code == Operator.TruncInt)
				{
					op.Code = Operator.Trunc;
				}
				if (ImplicitFunctionCallsHandler.\u0001(compileContext, op, op.Type, ref empty, ref typeClass))
				{
					IList<ISignature> list = scope[empty];
					if (list != null && list.Count == 1)
					{
						result = (_ISignature)list[0];
					}
				}
				op.Code = code;
			}
			return result;
		}

		// Token: 0x06002EED RID: 12013 RVA: 0x000B0C48 File Offset: 0x000AEE48
		public static _ISignature CheckForExternalFunctionCall(_ICompileContext compileContext, _IConversionExpression conv, IScope5 scope)
		{
			_ISignature result = null;
			if (compileContext.Codegenerator != null)
			{
				string empty = string.Empty;
				TypeClass typeClass = TypeClass.None;
				if (ImplicitFunctionCallsHandler.\u0001(compileContext, conv, scope, ref empty, ref typeClass))
				{
					IList<ISignature> list = scope[empty];
					if (list != null && list.Count == 1)
					{
						result = (_ISignature)list[0];
					}
					string text = ImplicitFunctionCallsHandler.\u0001(empty);
					if (text != null)
					{
						IList<ISignature> list2 = scope[text];
					}
				}
			}
			return result;
		}

		// Token: 0x06002EEE RID: 12014 RVA: 0x000B0CB0 File Offset: 0x000AEEB0
		private static string \u0001(string \u0002)
		{
			if (\u0002 == "any32__to__string")
			{
				return "any64__to__string";
			}
			if (\u0002 == "any64__to_string")
			{
				return "any32__to__string";
			}
			return null;
		}

		// Token: 0x06002EEF RID: 12015 RVA: 0x000B0CDC File Offset: 0x000AEEDC
		internal static bool \u0001(_ICompileContext \u0002, IConversionExpression \u0003, IScope5 \u0004, ref string \u0005, ref TypeClass \u0006)
		{
			if (!ImplicitFunctionCallsHandler.\u0001(\u0002, \u0003.From) || !ImplicitFunctionCallsHandler.\u0001(\u0002, \u0003.To))
			{
				return false;
			}
			IExpression exp = \u0003.Exp;
			ICompiledType compiledType = (exp != null) ? exp.Type : null;
			_IReferenceType ireferenceType = compiledType as _IReferenceType;
			if (ireferenceType != null)
			{
				compiledType = ireferenceType._Base;
			}
			return ImplicitFunctionCallsHandler.\u0001(\u0003, ref \u0005, compiledType, \u0004) || ((!TypeTable.IsReal(\u0003.From) || !TypeTable.IsReal(\u0003.To) || !\u0002.TreatLRealAsReal) && (ImplicitFunctionCallsHandler.\u0001(\u0002, \u0003, ref \u0005) || \u0002.Codegenerator.NeedsExternalFunctionCall(\u0003, ref \u0005, ref \u0006)));
		}

		// Token: 0x06002EF0 RID: 12016 RVA: 0x000B0D78 File Offset: 0x000AEF78
		internal static bool \u0001(_ICompileContext \u0002, IOperatorExpression \u0003, ICompiledType \u0004, ref string \u0005, ref TypeClass \u0006)
		{
			return \u0004 != null && ImplicitFunctionCallsHandler.\u0001(\u0002, \u0004.Class) && \u0003.Code != Operator.__Copy && \u0003.Code != Operator.__CRC && \u0003.Code != Operator.__Init && \u0003.Code != Operator.__LocalOffset && \u0003.Code != Operator.__MaxOffset && \u0003.Code != Operator.__Reloc && \u0003.Code != Operator.__TypeOf && \u0003.Code != Operator.__IsValidRef && \u0003.Code != Operator.__QueryInterface && \u0003.Code != Operator.__FCall && \u0003.Code != Operator.__PropertyInfo && \u0003.Code != Operator.__QueryPointer && \u0003.Code != Operator.__Delete && \u0003.Code != Operator.__AdrInst && \u0003.Code != Operator.__VarInfo && \u0003.Code != Operator.__CheckLicense && \u0003.Code != Operator.__CheckLicenseBit && \u0003.Code != Operator.__CallInitFunction && \u0003.Code != Operator.__LateCompiledExpr && \u0003.Code != Operator.__MemoryBarrier && \u0003.Code != Operator.__vcStore && \u0003.Code != Operator.__CurrentTask && \u0003.Code != Operator.XSizeOf && \u0002.Codegenerator.NeedsExternalFunctionCall(\u0003, \u0003.Type, ref \u0005, ref \u0006);
		}

		// Token: 0x06002EF1 RID: 12017 RVA: 0x000B0EFC File Offset: 0x000AF0FC
		private static bool \u0001(IConversionExpression \u0002, ref string \u0003, ICompiledType \u0004, IScope5 \u0005)
		{
			if (\u0004 != null && \u0004.Class == TypeClass.Enum)
			{
				_IEnumType ienumType = (_IEnumType)\u0004;
				ISignature signature = \u0005[ienumType.SignatureId];
				if (signature != null)
				{
					if (\u0002.To == TypeClass.String && signature.HasAttribute(CompileAttributes.ATTRIBUTE_TO_STRING_FUNCTION))
					{
						\u0003 = signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_TO_STRING_FUNCTION);
						return true;
					}
					if (\u0002.To == TypeClass.WString && signature.HasAttribute(CompileAttributes.ATTRIBUTE_TO_WSTRING_FUNCTION))
					{
						\u0003 = signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_TO_WSTRING_FUNCTION);
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002EF2 RID: 12018 RVA: 0x000B0F7C File Offset: 0x000AF17C
		private static bool \u0001(_ICompileContext \u0002, IConversionExpression \u0003, ref string \u0004)
		{
			if (((_ICompileContext6)\u0002).ExternalRealStringConversions && ((TypeTable.IsReal(\u0003.From) && TypeTable.IsString(\u0003.To)) || (TypeTable.IsString(\u0003.From) && TypeTable.IsReal(\u0003.To))))
			{
				string str = ImplicitFunctionCallsHandler.\u0001(\u0003.From);
				string str2 = ImplicitFunctionCallsHandler.\u0001(\u0003.To);
				\u0004 = str + "__to__" + str2 + "__ext";
				return true;
			}
			return false;
		}

		// Token: 0x06002EF3 RID: 12019 RVA: 0x000B0FF8 File Offset: 0x000AF1F8
		private static bool \u0001(_ICompileContext \u0002, TypeClass \u0003)
		{
			return ((_ICompileContext5)\u0002).TypeIsSupported(\u0003);
		}

		// Token: 0x06002EF4 RID: 12020 RVA: 0x000B1008 File Offset: 0x000AF208
		private static string \u0001(TypeClass \u0002)
		{
			if (\u0002 == TypeClass.Real)
			{
				return "real32";
			}
			if (\u0002 != TypeClass.LReal)
			{
				return \u0002.ToString().ToLower();
			}
			return "real64";
		}
	}
}
