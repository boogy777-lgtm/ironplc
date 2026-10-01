using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Features
{
	// Token: 0x02000287 RID: 647
	internal static class VarLenArray
	{
		// Token: 0x06002AF9 RID: 11001 RVA: 0x00072568 File Offset: 0x00071568
		internal static void AttributeVarLenArrayVariables(_ISignature sign)
		{
			if (sign.POUType == Operator.Function || sign.POUType == Operator.Method || sign.POUType == Operator.FunctionBlock)
			{
				IEnumerable<IVariable> enumerable = sign.InOuts;
				if (sign.POUType != Operator.FunctionBlock && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700)
				{
					enumerable = enumerable.Concat(sign.Inputs);
				}
				foreach (_IVariable ivariable in enumerable.Cast<_IVariable>())
				{
					if (ivariable.Type.Class == TypeClass.VarLenArray)
					{
						VarLenArray.ConfigureAndAddVariable(sign, ivariable);
					}
				}
			}
		}

		// Token: 0x06002AFA RID: 11002 RVA: 0x00072614 File Offset: 0x00071614
		private static void ConfigureAndAddVariable(_ISignature sign, _IVariable var)
		{
			string stValue = null;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				stValue = var._Type.ToString();
			}
			_IVariableLengthArrayType ivariableLengthArrayType = var.Type as _IVariableLengthArrayType;
			var._Type = new PointerType(ivariableLengthArrayType._Base);
			VarLenArray.AddOriginalVariableDeclarationScopeAttribute(var);
			var.SetFlag(VarFlag.Inout, false);
			var.SetFlag(VarFlag.Input, true);
			var.AddAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY, stValue);
			var.AddAttribute("Dimensions", ivariableLengthArrayType.Dimensions.ToString());
			_IVariable ivariable = LanguageModelBuilder.Singleton.CreateVariable(null);
			ivariable.Name = string.Format("{0}__Array__Info", var.OrgName);
			_IParser iparser = CompilerProxy.CreateParser(string.Format("ARRAY [1..{0}] OF __SYSTEM.__ARRAY__DIM__INFO;", ivariableLengthArrayType.Dimensions), true);
			ivariable.SetType(iparser.ParseType());
			ivariable.SetFlag(VarFlag.Input, true);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352100)
			{
				ivariable.SetFlag(VarFlag.Implicit, true);
			}
			ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INPUT, null);
			ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_HIDE, null);
			ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_NOINIT, null);
			sign.AddVariable(ivariable);
		}

		// Token: 0x06002AFB RID: 11003 RVA: 0x00072738 File Offset: 0x00071738
		private static void AddOriginalVariableDeclarationScopeAttribute(_IVariable var)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351800)
			{
				VarFlag varFlag = VarFlag.None;
				if (var.GetFlag(VarFlag.Inout))
				{
					varFlag = VarFlag.Inout;
				}
				else if (var.GetFlag(VarFlag.Input))
				{
					varFlag = VarFlag.Input;
				}
				var.AddAttribute("variable_length_array_original_scope", varFlag.ToString());
			}
		}

		// Token: 0x06002AFC RID: 11004 RVA: 0x0007278D File Offset: 0x0007178D
		public static bool IsVarLenArray(IVariable var)
		{
			return var != null && var.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY);
		}

		// Token: 0x06002AFD RID: 11005 RVA: 0x0007279F File Offset: 0x0007179F
		public static int GetDimensions(IVariable var)
		{
			return Convert.ToInt32(var.GetAttributeValue("Dimensions"));
		}

		// Token: 0x06002AFE RID: 11006 RVA: 0x000727B4 File Offset: 0x000717B4
		public static IVariable GetDimensionInfoVariable(ISignature declaringSign, IVariable varLenArray)
		{
			string stName = string.Format("{0}__Array__Info", varLenArray.OrgName);
			return declaringSign[stName];
		}

		// Token: 0x06002AFF RID: 11007 RVA: 0x000727D9 File Offset: 0x000717D9
		internal static bool IsVarLenArray(IVariable var, out string stOriginalType)
		{
			stOriginalType = string.Empty;
			bool flag = VarLenArray.IsVarLenArray(var);
			if (flag)
			{
				stOriginalType = var.GetAttributeValue(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY);
			}
			return flag;
		}

		// Token: 0x06002B00 RID: 11008 RVA: 0x000727F8 File Offset: 0x000717F8
		internal static VarFlag GetOriginalVariableDeclarationScope(IVariable var)
		{
			if (!VarLenArray.IsVarLenArray(var))
			{
				return VarFlag.None;
			}
			VarFlag result;
			if (var.HasAttribute("variable_length_array_original_scope") && Enum.TryParse<VarFlag>(var.GetAttributeValue("variable_length_array_original_scope"), out result))
			{
				return result;
			}
			return VarFlag.None;
		}

		// Token: 0x04000838 RID: 2104
		private const string c_DimensionsAttribute = "Dimensions";

		// Token: 0x04000839 RID: 2105
		private const string c_ArrayInfoVarNameTemplate = "{0}__Array__Info";
	}
}
