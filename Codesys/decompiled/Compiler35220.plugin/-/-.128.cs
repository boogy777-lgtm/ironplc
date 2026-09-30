using System;
using System.Text;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u001C
{
	// Token: 0x02000179 RID: 377
	internal static class \u000E
	{
		// Token: 0x060019B5 RID: 6581 RVA: 0x0005012C File Offset: 0x0004E32C
		internal static void \u0001(_ISignature \u0002, _IPreCompileContext \u0003)
		{
			\u000E.\u0002(\u0002, \u0003);
		}

		// Token: 0x060019B6 RID: 6582 RVA: 0x00050138 File Offset: 0x0004E338
		public static IImplicitEnumerationType \u0001(_IVariable \u0002)
		{
			IImplicitEnumerationType implicitEnumerationType = \u0002.Type as IImplicitEnumerationType;
			if (implicitEnumerationType == null && \u0002.Type is _IArrayType)
			{
				implicitEnumerationType = (\u0002._Type.BaseType as IImplicitEnumerationType);
			}
			return implicitEnumerationType;
		}

		// Token: 0x060019B7 RID: 6583 RVA: 0x00050174 File Offset: 0x0004E374
		private static void \u0002(_ISignature \u0002, _IPreCompileContext \u0003)
		{
			if (\u0002.HasAttribute("contains_implicit_enum"))
			{
				LDictionary<string, IImplicitEnumerationType> ldictionary = new LDictionary<string, IImplicitEnumerationType>();
				foreach (_IVariable ivariable in \u0002.All)
				{
					if (!ivariable.GetFlag(VarFlag.Inout) && !ivariable.GetFlag(VarFlag.Output))
					{
						IImplicitEnumerationType implicitEnumerationType = \u000E.\u0001(ivariable);
						if (implicitEnumerationType != null)
						{
							string text = \u000E.\u0001(ldictionary, implicitEnumerationType);
							if (text == null)
							{
								StringBuilder stringBuilder = new StringBuilder();
								text = string.Format("Implicit_Enum__{0}__{1}", \u0002.OrgName, ivariable.OrgName);
								stringBuilder.AppendLine("TYPE " + text + ":");
								stringBuilder.Append(implicitEnumerationType.ToString());
								stringBuilder.AppendLine(";");
								stringBuilder.AppendLine("END_TYPE");
								stringBuilder.AppendLine();
								_ISignature isignature = new CompilerServicesInternal().\u0001(stringBuilder.ToString(), true).ParseInterface() as _ISignature;
								isignature.SetFlag(SignatureFlag.Generated, true);
								if (!isignature.GetFlag(SignatureFlag.Global))
								{
									isignature.AddAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY, "");
								}
								ISignature signature = \u0003[text];
								if (signature != null)
								{
									isignature.ObjectGuid = signature.ObjectGuid;
								}
								else
								{
									isignature.ObjectGuid = Guid.NewGuid();
								}
								APEnvironmentFacade.Instance.LanguageModelMgr.AddRelatedObject(Guid.Empty, \u0002.ObjectGuid, isignature.ObjectGuid);
								\u0003.AddSignature(isignature, false);
								\u0003.AddRelatedSignature(\u0002, isignature);
								ldictionary.Add(text, implicitEnumerationType);
							}
							_IUserdefType iuserdefType = \u0003.\u0001(text);
							if (ivariable._Type is _IArrayType)
							{
								(ivariable._Type as _IArrayType)._Base = iuserdefType;
							}
							else
							{
								ivariable._Type = iuserdefType;
							}
							ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_ENUM_TYPE, "");
						}
					}
				}
			}
		}

		// Token: 0x060019B8 RID: 6584 RVA: 0x00050350 File Offset: 0x0004E550
		internal static string \u0001(LDictionary<string, IImplicitEnumerationType> \u0002, IImplicitEnumerationType \u0003)
		{
			foreach (string text in \u0002.Keys)
			{
				if (\u0002[text].IsEqual(\u0003))
				{
					return text;
				}
			}
			return null;
		}
	}
}
