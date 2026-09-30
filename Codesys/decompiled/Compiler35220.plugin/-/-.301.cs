using System;
using \u0002;
using \u000E;
using \u001E;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0083;
using \u0084;

namespace \u0014
{
	// Token: 0x02000320 RID: 800
	internal static class \u0013
	{
		// Token: 0x06002FDD RID: 12253 RVA: 0x000B4C50 File Offset: 0x000B2E50
		internal static bool \u0001(_ISignature \u0002, IScope5 \u0003, _ICompileContext \u0004)
		{
			return global::\u000E.\u001B.\u0001(\u0004.ApplicationGuid).\u0001(\u0002, \u0003);
		}

		// Token: 0x06002FDE RID: 12254 RVA: 0x000B4C64 File Offset: 0x000B2E64
		internal static bool \u0002(_ISignature \u0002, IScope5 \u0003, _ICompileContext \u0004)
		{
			return global::\u000E.\u001B.\u0001(\u0004.ApplicationGuid).\u0003(\u0002, \u0003);
		}

		// Token: 0x06002FDF RID: 12255 RVA: 0x000B4C78 File Offset: 0x000B2E78
		internal static bool \u0003(_ISignature \u0002, IScope5 \u0003, _ICompileContext \u0004)
		{
			return global::\u000E.\u001B.\u0001(\u0004.ApplicationGuid).\u0004(\u0002, \u0003);
		}

		// Token: 0x06002FE0 RID: 12256 RVA: 0x000B4C8C File Offset: 0x000B2E8C
		internal static bool \u0004(_ISignature \u0002, IScope5 \u0003, _ICompileContext \u0004)
		{
			return global::\u000E.\u001B.\u0001(\u0004.ApplicationGuid).\u0005(\u0002, \u0003);
		}

		// Token: 0x06002FE1 RID: 12257 RVA: 0x000B4CA0 File Offset: 0x000B2EA0
		internal static bool \u0005(_ISignature \u0002, IScope5 \u0003, _ICompileContext \u0004)
		{
			return new SignatureChecker(\u001E.\u001A.\u0001(\u0004.ApplicationGuid, false, false, false)).\u0001(\u0002, \u0003);
		}

		// Token: 0x06002FE2 RID: 12258 RVA: 0x000B4CBC File Offset: 0x000B2EBC
		internal static bool \u0006(_ISignature \u0002, IScope5 \u0003, _ICompileContext \u0004)
		{
			global::\u000E.\u001B u0082_u = \u001E.\u001A.\u0001(\u0004.ApplicationGuid, false, false, false);
			return new \u0083.\u0008(\u0002, u0082_u).\u0001(\u0003);
		}

		// Token: 0x06002FE3 RID: 12259 RVA: 0x000B4CE8 File Offset: 0x000B2EE8
		internal static bool \u0007(_ISignature \u0002, IScope5 \u0003, _ICompileContext \u0004)
		{
			bool flag = true;
			ErrorVisitor errorVisitor = new ErrorVisitor();
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				errorVisitor.\u0001();
				flag = (global::\u0002.\u000E.\u0001(ivariable._Type, \u0003, errorVisitor) && flag);
				\u0002.AddMessages(errorVisitor.Messages);
			}
			return flag;
		}

		// Token: 0x06002FE4 RID: 12260 RVA: 0x000B4D58 File Offset: 0x000B2F58
		internal static void \u0001(_IVariable \u0002, ErrorVisitor \u0003)
		{
			int num = 0;
			for (;;)
			{
				string stAttribute = string.Format("suppress_warning_{0}", num++);
				if (!\u0002.HasAttribute(stAttribute))
				{
					break;
				}
				\u0003.\u0001(\u0002.GetAttributeValue(stAttribute));
			}
		}

		// Token: 0x06002FE5 RID: 12261 RVA: 0x000B4D94 File Offset: 0x000B2F94
		internal static bool \u0001(_IPreCompileContext \u0002, _ISignature \u0003, _ICompiledPOU \u0004)
		{
			return \u001C.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06002FE6 RID: 12262 RVA: 0x000B4DA0 File Offset: 0x000B2FA0
		internal static string \u0001(IVariable \u0002, int \u0003)
		{
			string attributeValue = \u0002.GetAttributeValue(CompileAttributes.DEVICE_PARAMETER);
			if (\u0003 >= 0)
			{
				return string.Format("ParamGetBit({0}, {1})", attributeValue, \u0003);
			}
			TypeClass @class = \u0002.CompiledType.DeRefType.Class;
			ICompiledType compiledType;
			switch (@class)
			{
			case TypeClass.USInt:
				compiledType = TypeTable.Get(TypeClass.Byte);
				break;
			case TypeClass.UInt:
				compiledType = TypeTable.Get(TypeClass.Word);
				break;
			case TypeClass.UDInt:
				compiledType = TypeTable.Get(TypeClass.DWord);
				break;
			case TypeClass.ULInt:
				compiledType = TypeTable.Get(TypeClass.LWord);
				break;
			default:
				compiledType = \u0002.CompiledType.DeRefType;
				break;
			}
			switch (@class)
			{
			case TypeClass.Bool:
			case TypeClass.Byte:
			case TypeClass.Word:
			case TypeClass.DWord:
			case TypeClass.LWord:
			case TypeClass.SInt:
			case TypeClass.Int:
			case TypeClass.DInt:
			case TypeClass.LInt:
			case TypeClass.USInt:
			case TypeClass.UInt:
			case TypeClass.UDInt:
			case TypeClass.ULInt:
			case TypeClass.Real:
			case TypeClass.LReal:
			case TypeClass.String:
			case TypeClass.WString:
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
			case TypeClass.LTime:
			case TypeClass.LDate:
			case TypeClass.LDateAndTime:
			case TypeClass.LTimeOfDay:
				return string.Format("ParamGet{0}({1})", compiledType.ToString(), attributeValue);
			}
			return string.Empty;
		}

		// Token: 0x06002FE7 RID: 12263 RVA: 0x000B4F10 File Offset: 0x000B3110
		internal static string \u0001(IVariable \u0002, int \u0003, string \u0004)
		{
			string attributeValue = \u0002.GetAttributeValue(CompileAttributes.DEVICE_PARAMETER);
			if (\u0003 >= 0)
			{
				return string.Format("ParamSetBit({0}, {1}, {2})", attributeValue, \u0003, \u0004);
			}
			switch (\u0002.CompiledType.DeRefType.Class)
			{
			case TypeClass.Bool:
			case TypeClass.Byte:
			case TypeClass.Word:
			case TypeClass.SInt:
			case TypeClass.Int:
			case TypeClass.DInt:
			case TypeClass.USInt:
			case TypeClass.UInt:
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
			case TypeClass.Enum:
				return string.Format("ParamSet4Byte({0}, {1}_TO_DWORD({2}))", attributeValue, \u0002.CompiledType.DeRefType.ToString(), \u0004);
			case TypeClass.DWord:
			case TypeClass.UDInt:
				return string.Format("ParamSet4Byte({0}, {1})", attributeValue, \u0004);
			case TypeClass.LWord:
				return string.Format("ParamSet8Byte({0}, {1})", attributeValue, \u0004);
			case TypeClass.LInt:
			case TypeClass.ULInt:
			case TypeClass.LTime:
			case TypeClass.LDate:
			case TypeClass.LDateAndTime:
			case TypeClass.LTimeOfDay:
				return string.Format("ParamSet8Byte({0}, {1}_TO_LWORD({2}))", attributeValue, \u0002.CompiledType.DeRefType.ToString(), \u0004);
			case TypeClass.Real:
				return string.Format("ParamSetReal({0}, {1})", attributeValue, \u0004);
			case TypeClass.LReal:
				return string.Format("ParamSetLReal({0}, {1})", attributeValue, \u0004);
			case TypeClass.String:
				return string.Format("ParamSetString({0}, {1})", attributeValue, \u0004);
			case TypeClass.WString:
				return string.Format("ParamSetWString({0}, {1})", attributeValue, \u0004);
			}
			return string.Empty;
		}
	}
}
