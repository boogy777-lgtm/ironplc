using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200023D RID: 573
	internal static class GreenVariableFactory
	{
		// Token: 0x06002608 RID: 9736 RVA: 0x0005E0C8 File Offset: 0x0005D0C8
		private static AbstractGreenVariable Create(_IVariable var, out LDictionary<string, string> attributes, out string comment, out GreenVariableProperies gtFlags, out bool isDocuComment)
		{
			GreenVariableFactory.GetVarAttributes(var, out attributes, out comment, out gtFlags, out isDocuComment);
			if (var.Address != null || var.InputAssignments != null)
			{
				GreenVariableFactory.ConvertFlagsToAttributes(attributes, ref comment, ref gtFlags);
				return new GreenVariableFull();
			}
			if (attributes.Any<KeyValuePair<string, string>>())
			{
				GreenVariableFactory.ConvertFlagsToAttributes(attributes, ref comment, ref gtFlags);
				return new GreenVariableWithInitValueAndCommentAndAttributes();
			}
			if (gtFlags != GreenVariableProperies.None)
			{
				return new GreenVariableWithInitValueAndCommentAndFlags(isDocuComment);
			}
			if (!string.IsNullOrWhiteSpace(comment) && var.Initial != null)
			{
				return new GreenVariableWithInitValueAndComment(isDocuComment);
			}
			if (var.Initial != null)
			{
				return new GreenVariableWithInitValue();
			}
			if (!string.IsNullOrWhiteSpace(comment))
			{
				return new GreenVariableWithComment(isDocuComment);
			}
			return new GreenVariableMinimal();
		}

		// Token: 0x06002609 RID: 9737 RVA: 0x0005E164 File Offset: 0x0005D164
		private static void AddInternalAttribute(_IVariable var, LDictionary<string, string> attributes, string stAttribute)
		{
			string text = string.Intern(stAttribute);
			string text2 = var.GetAttributeValue(stAttribute);
			if (text2 != null)
			{
				text2 = string.Intern(text2);
			}
			attributes[text] = text2;
		}

		// Token: 0x0600260A RID: 9738 RVA: 0x0005E194 File Offset: 0x0005D194
		private static void GetVarAttributes(_IVariable var, out LDictionary<string, string> attributes, out string comment, out GreenVariableProperies gtFlags, out bool isDocuComment)
		{
			attributes = new LDictionary<string, string>();
			comment = string.Empty;
			gtFlags = GreenVariableProperies.None;
			isDocuComment = false;
			if (var is AbstractGreenVariable)
			{
				AbstractGreenVariable abstractGreenVariable = var as AbstractGreenVariable;
				comment = abstractGreenVariable.Comment;
				gtFlags = abstractGreenVariable.GTFlags;
			}
			if (var.Attributes == null || var.Attributes.Length == 0)
			{
				return;
			}
			foreach (string stAttribute in var.Attributes)
			{
				GreenVariableFactory.SetAttributeFlags(var, attributes, ref comment, ref gtFlags, ref isDocuComment, stAttribute);
			}
		}

		// Token: 0x0600260B RID: 9739 RVA: 0x0005E210 File Offset: 0x0005D210
		private static void SetAttributeFlags(_IVariable var, LDictionary<string, string> attributes, ref string comment, ref GreenVariableProperies gtFlags, ref bool isDocuComment, string stAttribute)
		{
			if (stAttribute == CompileAttributes.ATTRIBUTE_COMMENT)
			{
				comment = string.Intern(var.GetAttributeValue(stAttribute));
				isDocuComment = false;
				return;
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_DOCUCOMMENT)
			{
				if (!string.IsNullOrEmpty(comment))
				{
					GreenVariableFactory.AddInternalAttribute(var, attributes, CompileAttributes.ATTRIBUTE_COMMENT);
				}
				comment = string.Intern(var.GetAttributeValue(stAttribute));
				isDocuComment = true;
				return;
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_PROPERTY)
			{
				gtFlags |= GreenVariableProperies.Property;
				return;
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_HIDE)
			{
				gtFlags |= GreenVariableProperies.Hide;
				return;
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_BLOBINITCONST)
			{
				gtFlags |= GreenVariableProperies.BlobInitConst;
				return;
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_NOINIT)
			{
				gtFlags |= GreenVariableProperies.NoInit;
				return;
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_NO_PRECOMPILE_CHECKS)
			{
				gtFlags |= GreenVariableProperies.NoPrecompileChecks;
				return;
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_INIT_ON_ONLCHANGE)
			{
				gtFlags |= GreenVariableProperies.InitOnOnlChange;
				return;
			}
			GreenVariableFactory.AddInternalAttribute(var, attributes, stAttribute);
		}

		// Token: 0x0600260C RID: 9740 RVA: 0x0005E300 File Offset: 0x0005D300
		private static void ConvertFlagsToAttributes(LDictionary<string, string> attributes, ref string comment, ref GreenVariableProperies gtFlags)
		{
			if (!string.IsNullOrEmpty(comment))
			{
				if (attributes.ContainsKey(CompileAttributes.ATTRIBUTE_COMMENT))
				{
					attributes.Add(CompileAttributes.ATTRIBUTE_DOCUCOMMENT, comment);
				}
				else
				{
					attributes.Add(CompileAttributes.ATTRIBUTE_COMMENT, comment);
				}
				comment = string.Empty;
			}
			if (gtFlags == GreenVariableProperies.None)
			{
				return;
			}
			if ((gtFlags & GreenVariableProperies.Hide) != GreenVariableProperies.None)
			{
				attributes.Add(CompileAttributes.ATTRIBUTE_HIDE, null);
			}
			if ((gtFlags & GreenVariableProperies.Property) != GreenVariableProperies.None)
			{
				attributes.Add(CompileAttributes.ATTRIBUTE_PROPERTY, null);
			}
			if ((gtFlags & GreenVariableProperies.BlobInitConst) != GreenVariableProperies.None)
			{
				attributes.Add(CompileAttributes.ATTRIBUTE_BLOBINITCONST, null);
			}
			if ((gtFlags & GreenVariableProperies.NoInit) != GreenVariableProperies.None)
			{
				attributes.Add(CompileAttributes.ATTRIBUTE_NOINIT, null);
			}
			if ((gtFlags & GreenVariableProperies.NoPrecompileChecks) != GreenVariableProperies.None)
			{
				attributes.Add(CompileAttributes.ATTRIBUTE_NO_PRECOMPILE_CHECKS, null);
			}
			if ((gtFlags & GreenVariableProperies.InitOnOnlChange) != GreenVariableProperies.None)
			{
				attributes.Add(CompileAttributes.ATTRIBUTE_INIT_ON_ONLCHANGE, null);
			}
			gtFlags = GreenVariableProperies.None;
		}

		// Token: 0x0600260D RID: 9741 RVA: 0x0005E3BC File Offset: 0x0005D3BC
		public static AbstractGreenVariable CreateGreenVariable(_IVariable var)
		{
			AbstractGreenVariable result;
			try
			{
				LDictionary<string, string> ldictionary;
				string text;
				GreenVariableProperies greenVariableProperies;
				bool flag;
				AbstractGreenVariable abstractGreenVariable = GreenVariableFactory.Create(var, out ldictionary, out text, out greenVariableProperies, out flag);
				if (var.Address != null)
				{
					abstractGreenVariable.Address = var.Address;
				}
				if (!string.IsNullOrWhiteSpace(text))
				{
					if (flag)
					{
						abstractGreenVariable.DocuComment = text;
					}
					else
					{
						abstractGreenVariable.Comment = text;
					}
				}
				if (var.Initial != null)
				{
					CompactedPrecompileParseTreeInformation compactedPrecompileParseTreeInformation = null;
					_IExpression iexpression = GreenTreeContext.Singleton.ConvertInitialValueToGreenTrees((IVariableWithCompactedInitialValue)var, out compactedPrecompileParseTreeInformation);
					if (iexpression != null)
					{
						Debug.Assert(compactedPrecompileParseTreeInformation != null);
						abstractGreenVariable.InitialWithCompactedInformation = new ExpressionWithCompactedInformation(iexpression, compactedPrecompileParseTreeInformation);
					}
					else
					{
						abstractGreenVariable.Initial = var.Initial;
					}
				}
				if (var.InputAssignments != null && var.InputAssignments.Any<IAssignmentExpression>())
				{
					abstractGreenVariable.InputAssignments = var.InputAssignments;
				}
				abstractGreenVariable.OrgName = var.OrgName;
				if (var._Type != null)
				{
					abstractGreenVariable._Type = var._Type;
				}
				abstractGreenVariable.Flags = var.Flags;
				abstractGreenVariable._SourcePosition = var._SourcePosition;
				if (ldictionary.Any<KeyValuePair<string, string>>())
				{
					abstractGreenVariable.SetAttributes(ldictionary);
				}
				if (greenVariableProperies != GreenVariableProperies.None)
				{
					abstractGreenVariable.GTFlags = greenVariableProperies;
				}
				abstractGreenVariable.PrecompileId = var.PrecompileId;
				result = abstractGreenVariable;
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x0600260E RID: 9742 RVA: 0x0005E508 File Offset: 0x0005D508
		public static _IVariable CreateRedVariable(_IVariable2 var)
		{
			_IVariable result;
			try
			{
				_IVariable2 ivariable = LanguageModelBuilder.Singleton.CreateVariable(var._SourcePosition) as _IVariable2;
				if (var.Address != null)
				{
					ivariable.Address = DirectVariable.CopyFrom(var.Address);
				}
				IVariableWithCompactedInitialValue variableWithCompactedInitialValue = (IVariableWithCompactedInitialValue)var;
				if (variableWithCompactedInitialValue.OriginalInitial != null)
				{
					if (variableWithCompactedInitialValue.OriginalInitial is IGreenTreeExprement)
					{
						ivariable._Initial = var._Initial;
					}
					else
					{
						ivariable._Initial = (variableWithCompactedInitialValue.OriginalInitial.Duplicate() as _IExpression);
					}
				}
				if (var.InputAssignments != null && var.InputAssignments.Any<IAssignmentExpression>())
				{
					IAssignmentExpression[] array = new IAssignmentExpression[var.InputAssignments.Length];
					for (int i = 0; i < var.InputAssignments.Length; i++)
					{
						array[i] = ((var.InputAssignments[i] as _IAssignmentExpression).Duplicate() as IAssignmentExpression);
					}
					ivariable.InputAssignments = array;
				}
				ivariable.Name = var.OrgName;
				if (var._Type != null)
				{
					ivariable._Type = var._Type._Duplicate(false);
				}
				ivariable.Flags = var.Flags;
				if (var.Attributes != null)
				{
					foreach (string stAttribute in var.Attributes)
					{
						ivariable.SetAttributeValue(stAttribute, var.GetAttributeValue(stAttribute));
					}
				}
				result = ivariable;
			}
			catch
			{
				result = null;
			}
			return result;
		}
	}
}
