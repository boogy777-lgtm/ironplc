using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0006;
using \u000E;
using \u0011;
using \u0012;
using \u0016;
using \u0019;
using \u001F;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u007F;
using \u0080;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x02000322 RID: 802
	internal sealed class SignatureChecker
	{
		// Token: 0x06002FEA RID: 12266 RVA: 0x000B52F8 File Offset: 0x000B34F8
		public SignatureChecker(global::\u000E.\u001B ci)
		{
			this.CompileInformation = ci;
			this.FBInheritanceChecker = new \u007F.\u0011(ci);
			this.VariableChecker = new VariableChecker(ci);
			this.MethodChecker = new \u0080.\u0015();
			this.CPPCompatibilityChecker = new global::\u0006.\u0010();
			this.TaskLocalVariablesChecker = new global::\u0019.\u0011(ci);
			this.AttributeChecker = new \u001F.\u0011();
		}

		// Token: 0x170007F3 RID: 2035
		// (get) Token: 0x06002FEB RID: 12267 RVA: 0x000B5358 File Offset: 0x000B3558
		private global::\u000E.\u001B CompileInformation { get; }

		// Token: 0x170007F4 RID: 2036
		// (get) Token: 0x06002FEC RID: 12268 RVA: 0x000B5360 File Offset: 0x000B3560
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x170007F5 RID: 2037
		// (get) Token: 0x06002FED RID: 12269 RVA: 0x000B5370 File Offset: 0x000B3570
		private \u007F.\u0011 FBInheritanceChecker { get; }

		// Token: 0x170007F6 RID: 2038
		// (get) Token: 0x06002FEE RID: 12270 RVA: 0x000B5378 File Offset: 0x000B3578
		private VariableChecker VariableChecker { get; }

		// Token: 0x170007F7 RID: 2039
		// (get) Token: 0x06002FEF RID: 12271 RVA: 0x000B5380 File Offset: 0x000B3580
		private \u0080.\u0015 MethodChecker { get; }

		// Token: 0x170007F8 RID: 2040
		// (get) Token: 0x06002FF0 RID: 12272 RVA: 0x000B5388 File Offset: 0x000B3588
		private global::\u0006.\u0010 CPPCompatibilityChecker { get; }

		// Token: 0x170007F9 RID: 2041
		// (get) Token: 0x06002FF1 RID: 12273 RVA: 0x000B5390 File Offset: 0x000B3590
		private global::\u0019.\u0011 TaskLocalVariablesChecker { get; }

		// Token: 0x170007FA RID: 2042
		// (get) Token: 0x06002FF2 RID: 12274 RVA: 0x000B5398 File Offset: 0x000B3598
		private \u001F.\u0011 AttributeChecker { get; }

		// Token: 0x06002FF3 RID: 12275 RVA: 0x000B53A0 File Offset: 0x000B35A0
		internal bool \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			this.\u0003(\u0002, \u0003);
			this.FBInheritanceChecker.\u0001(\u0002);
			\u007F.\u0010.\u0001(\u0002, this.ComconNew);
			this.MethodChecker.\u0002(\u0002, \u0003);
			this.\u0001(\u0002);
			this.MethodChecker.\u0001(\u0002, \u0003);
			this.FBInheritanceChecker.\u0001(\u0002, \u0003);
			this.VariableChecker.\u0001(\u0002, \u0003);
			this.AttributeChecker.\u0001(\u0002, \u0003);
			this.\u0002(\u0002);
			this.\u0003(\u0002);
			this.CPPCompatibilityChecker.\u0001(\u0002, \u0003);
			global::\u0016.\u0013.\u0001(\u0002, \u0003, this.ComconNew);
			this.TaskLocalVariablesChecker.\u0001(\u0002, \u0003);
			global::\u0012.\u0014.\u0001(\u0002);
			this.\u0004(\u0002);
			return true;
		}

		// Token: 0x06002FF4 RID: 12276 RVA: 0x000B5458 File Offset: 0x000B3658
		internal void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			this.VariableChecker.\u0003(\u0002, \u0003);
		}

		// Token: 0x06002FF5 RID: 12277 RVA: 0x000B5468 File Offset: 0x000B3668
		internal void \u0002(_ISignature \u0002, IScope5 \u0003)
		{
			this.VariableChecker.\u0002(\u0002, \u0003);
		}

		// Token: 0x06002FF6 RID: 12278 RVA: 0x000B5478 File Offset: 0x000B3678
		private void \u0003(_ISignature \u0002, IScope5 \u0003)
		{
			if (!\u0002.GetFlag(SignatureFlag.External))
			{
				if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_C_CALLING_CONVENTION))
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_AttributeNotValidForNonExternalPOU, new object[]
					{
						CompileAttributes.ATTRIBUTE_C_CALLING_CONVENTION
					});
				}
				return;
			}
			bool flag = false;
			if (\u0002.POUType == Operator.Program || (\u0002.POUType == Operator.Type && !\u0002.GetFlag(SignatureFlag.Structure)))
			{
				flag = true;
			}
			else if (\u0002.POUType == Operator.VarGlobal && \u0002.AllConstants.Count <= 0)
			{
				flag = true;
			}
			if (flag)
			{
				\u0002.AddMessage(Severity.Warning, MessageId.Wrn_ExternalReferenceIgnored, new object[]
				{
					Scanner.GetTextOfOperator(\u0002.POUType),
					\u0002.OrgName
				});
				return;
			}
			this.\u0004(\u0002, \u0003);
		}

		// Token: 0x06002FF7 RID: 12279 RVA: 0x000B552C File Offset: 0x000B372C
		private void \u0004(_ISignature \u0002, IScope5 \u0003)
		{
			if (this.ComconNew.GetCodegeneratorProperty(CodegeneratorProperties.CCallingConvention) || \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_C_CALLING_CONVENTION))
			{
				for (int i = 0; i < 2; i++)
				{
					IVariable[] u;
					if (i == 0)
					{
						u = \u0002.Inputs;
					}
					else
					{
						u = \u0002.Outputs;
					}
					this.\u0001(\u0002, \u0003, u);
				}
			}
		}

		// Token: 0x06002FF8 RID: 12280 RVA: 0x000B5580 File Offset: 0x000B3780
		private void \u0001(_ISignature \u0002, IScope5 \u0003, IVariable[] \u0004)
		{
			foreach (_IVariable ivariable in \u0004.OfType<_IVariable>())
			{
				TypeClass tc = ivariable.CompiledType.DeRefType.Class;
				if (ivariable.CompiledType.Class == TypeClass.Reference)
				{
					tc = TypeClass.Reference;
				}
				if (TypeTable.IsBlock(tc) && !\u0084.\u0004.\u0001(ivariable.CompiledType, \u0003, this.ComconNew.Codegenerator))
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_StructuredValueTypeInExternalCall, new object[]
					{
						ivariable.Type.ToString()
					});
				}
			}
		}

		// Token: 0x06002FF9 RID: 12281 RVA: 0x000B562C File Offset: 0x000B382C
		private void \u0001(_ISignature \u0002)
		{
			if (\u0002.POUType == Operator.Interface)
			{
				using (IEnumerator<_IVariable> enumerator = \u0002.AllVariables.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (!enumerator.Current.IsProperty)
						{
							\u0002.AddMessage(Severity.Error, MessageId.Err_NoVarsinInterface, Array.Empty<object>());
							break;
						}
					}
				}
			}
		}

		// Token: 0x06002FFA RID: 12282 RVA: 0x000B5698 File Offset: 0x000B3898
		private void \u0002(_ISignature \u0002)
		{
			bool isCompiledLibraryObject = \u0002.IsCompiledLibraryObject;
			if (Scanner.IsReservedUnusedKeyword(\u0002.Name))
			{
				Severity severity = Messages.\u0001(\u0002, MessageId.Wrn_ReservedUnusedKeyword);
				if (severity == Severity.Warning && isCompiledLibraryObject)
				{
					severity = Severity.Information;
				}
				\u0002.AddMessage(severity, MessageId.Wrn_ReservedUnusedKeyword, new object[]
				{
					\u0002.Name
				});
			}
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				Operator operatorFromText = Scanner.GetOperatorFromText(ivariable.Name);
				if (isCompiledLibraryObject && operatorFromText != Operator.None && !Scanner.IsContextualOperator(operatorFromText))
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_OperatorNoValidVariableName, new object[]
					{
						ivariable.Name
					});
				}
				if (Scanner.IsReservedUnusedKeyword(ivariable.Name))
				{
					Severity severity2 = Messages.\u0001(ivariable, MessageId.Wrn_ReservedUnusedKeyword);
					if (severity2 == Severity.Warning && isCompiledLibraryObject)
					{
						severity2 = Severity.Information;
					}
					\u0002.\u0001(ivariable.SourcePosition, severity2, MessageId.Wrn_ReservedUnusedKeyword, new object[]
					{
						ivariable.Name
					});
				}
			}
		}

		// Token: 0x06002FFB RID: 12283 RVA: 0x000B57AC File Offset: 0x000B39AC
		private void \u0003(_ISignature \u0002)
		{
			ITargetSettings targetSettings = this.ComconNew.GetTargetSettings();
			if (!global::\u0016.\u0004.SupportUserCheckFunctions.GetBoolValue(targetSettings) && string.IsNullOrEmpty(\u0002.LibraryId) && APEnvironmentFacade.Instance.LanguageModelMgr.AttributeManager.IsCheckFunction(\u0002))
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_UserCheckFunctionsNotSupported, Array.Empty<object>());
				\u0002.AddError(global::\u0019.\u0003.\u0001(\u0002.NameExpression.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_UserCheckFunctionsNotSupported));
			}
		}

		// Token: 0x06002FFC RID: 12284 RVA: 0x000B5828 File Offset: 0x000B3A28
		private static bool \u0001(IEnumerable<IVariable> \u0002, string \u0003)
		{
			SignatureChecker.\u0001 u = new SignatureChecker.\u0001();
			u.\u0001 = \u0003;
			return \u0002.Any(new Func<IVariable, bool>(u.\u0001));
		}

		// Token: 0x06002FFD RID: 12285 RVA: 0x000B5854 File Offset: 0x000B3A54
		private void \u0004(_ISignature \u0002)
		{
			SignatureChecker.\u0002 u = new SignatureChecker.\u0002();
			u.\u0001 = \u0002;
			if (u.\u0001.HasAttribute("call_after_global_init_slot"))
			{
				IVariable[] array = u.\u0001.AllInputs.Where(new Func<IVariable, bool>(SignatureChecker.<>c.<>9.\u0001)).ToArray<IVariable>();
				bool flag = SignatureChecker.\u0001(array, "BINITRETAINS");
				bool flag2 = u.\u0001.Name == "IOGLOBALINIT__POU" && SignatureChecker.\u0001(array, "__BNOIOMGRUPDATEMAPPING");
				bool flag3 = array.Length == 0 || (array.Length == 1 && (flag || flag2));
				IVariable[] array2 = u.\u0001.AllOutputs.Where(new Func<IVariable, bool>(SignatureChecker.<>c.<>9.\u0002)).ToArray<IVariable>();
				bool flag4 = array2.Any(new Func<IVariable, bool>(u.\u0001));
				bool flag5 = array2.Length == 0 || (flag4 && array2.Length == 1);
				if (!flag3 || !flag5)
				{
					string u2 = global::\u000E.\u0018.\u0001(MessageId.Err_WrongCallAfterGlobalInitSlotSignature, Array.Empty<object>());
					u.\u0001.AddError(global::\u0019.\u0003.\u0001(u.\u0001.NameExpression.Position, u2, Severity.Error, MessageId.Err_WrongCallAfterGlobalInitSlotSignature));
				}
			}
		}

		// Token: 0x04000924 RID: 2340
		[CompilerGenerated]
		private readonly global::\u000E.\u001B \u0001;

		// Token: 0x04000925 RID: 2341
		[CompilerGenerated]
		private readonly \u007F.\u0011 \u0001;

		// Token: 0x04000926 RID: 2342
		[CompilerGenerated]
		private readonly VariableChecker \u0001;

		// Token: 0x04000927 RID: 2343
		[CompilerGenerated]
		private readonly \u0080.\u0015 \u0001;

		// Token: 0x04000928 RID: 2344
		[CompilerGenerated]
		private readonly global::\u0006.\u0010 \u0001;

		// Token: 0x04000929 RID: 2345
		[CompilerGenerated]
		private readonly global::\u0019.\u0011 \u0001;

		// Token: 0x0400092A RID: 2346
		[CompilerGenerated]
		private readonly \u001F.\u0011 \u0001;

		// Token: 0x02000323 RID: 803
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06002FFF RID: 12287 RVA: 0x000B59A8 File Offset: 0x000B3BA8
			internal bool \u0001(IVariable \u0002)
			{
				return \u0002.Name == this.\u0001 && \u0002.Type.Class == TypeClass.Bool;
			}

			// Token: 0x0400092B RID: 2347
			public string \u0001;
		}

		// Token: 0x02000324 RID: 804
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x06003001 RID: 12289 RVA: 0x000B59D8 File Offset: 0x000B3BD8
			internal bool \u0001(IVariable \u0002)
			{
				return \u0002.Name == this.\u0001.Name && \u0002.HasFlag(VarFlag.Output);
			}

			// Token: 0x0400092C RID: 2348
			public _ISignature \u0001;
		}
	}
}
