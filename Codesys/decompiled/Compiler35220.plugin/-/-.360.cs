using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0019;
using \u001A;
using \u001C;
using \u001D;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace \u0001
{
	// Token: 0x020003AA RID: 938
	internal sealed class \u0012 : EmptyVisitor351900
	{
		// Token: 0x06003624 RID: 13860 RVA: 0x000D9690 File Offset: 0x000D7890
		private \u0012(bool \u0089\u0003)
		{
			this.ImplicitSignature = \u0089\u0003;
		}

		// Token: 0x06003625 RID: 13861 RVA: 0x000D96AC File Offset: 0x000D78AC
		internal static void \u0001(_IStatement \u0002, IScope5 \u0003, _ISignature \u0004)
		{
			\u001D.\u0001 ivisit = new \u001D.\u0001(new global::\u0001.\u0012(APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(\u0004, GUIHidingFlags.AllCommon))
			{
				\u0001 = \u0003
			});
			\u0002.Accept(ivisit);
		}

		// Token: 0x06003626 RID: 13862 RVA: 0x000D96EC File Offset: 0x000D78EC
		internal static void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			global::\u0001.\u0012.\u0001(global::\u0001.\u0012.\u0001.\u0001(\u0002, \u0003), \u0003, \u0002);
		}

		// Token: 0x170008CF RID: 2255
		// (get) Token: 0x06003627 RID: 13863 RVA: 0x000D96FC File Offset: 0x000D78FC
		private bool ImplicitSignature { get; }

		// Token: 0x170008D0 RID: 2256
		// (get) Token: 0x06003628 RID: 13864 RVA: 0x000D9704 File Offset: 0x000D7904
		// (set) Token: 0x06003629 RID: 13865 RVA: 0x000D970C File Offset: 0x000D790C
		internal bool InCall { get; set; }

		// Token: 0x0600362A RID: 13866 RVA: 0x000D9718 File Offset: 0x000D7918
		public override void visit(_IEmptyStatement empty)
		{
			_IVarInitialEmptyStatement ivarInitialEmptyStatement = empty as _IVarInitialEmptyStatement;
			if (ivarInitialEmptyStatement != null)
			{
				this.\u0001(ivarInitialEmptyStatement);
			}
		}

		// Token: 0x0600362B RID: 13867 RVA: 0x000D9738 File Offset: 0x000D7938
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			bool flag = (access & (AccessFlag.Write | AccessFlag.Address)) == (AccessFlag.Write | AccessFlag.Address);
			bool u = this.InCall && flag;
			IVariable variable2 = variable.GetVariable(this.\u0001);
			this.\u0001(variable2, access, u);
			if (!this.ImplicitSignature)
			{
				this.\u0001(variable2, flag, u);
			}
		}

		// Token: 0x0600362C RID: 13868 RVA: 0x000D9780 File Offset: 0x000D7980
		public override void visit(_IPragmaStatement pragma)
		{
			_IWarningDisableRestorePragmaStatement iwarningDisableRestorePragmaStatement = pragma as _IWarningDisableRestorePragmaStatement;
			if (iwarningDisableRestorePragmaStatement != null)
			{
				if (iwarningDisableRestorePragmaStatement.Restore && this.\u0001.ContainsKey(iwarningDisableRestorePragmaStatement.Id))
				{
					this.\u0001.Remove(iwarningDisableRestorePragmaStatement.Id);
				}
				if (!iwarningDisableRestorePragmaStatement.Restore && !this.\u0001.ContainsKey(iwarningDisableRestorePragmaStatement.Id))
				{
					this.\u0001.Add(iwarningDisableRestorePragmaStatement.Id, iwarningDisableRestorePragmaStatement.Id);
				}
			}
		}

		// Token: 0x170008D1 RID: 2257
		// (get) Token: 0x0600362D RID: 13869 RVA: 0x000D97F8 File Offset: 0x000D79F8
		private static string NOINITWARNING_ID
		{
			get
			{
				return string.Format("{0}{1:d4}", global::\u0001.\u0012.\u0001.Prefix, global::\u0001.\u0012.\u0001.Number);
			}
		}

		// Token: 0x0600362E RID: 13870 RVA: 0x000D9820 File Offset: 0x000D7A20
		private void \u0001(IVariable \u0002, bool \u0003, bool \u0004)
		{
			Severity u = this.\u0001.ContainsKey(global::\u0001.\u0012.NOINITWARNING_ID) ? Severity.SuppressedWarning : Severity.Warning;
			if (\u0003 && !\u0004 && \u0002 != null && this.\u0001 != null && global::\u0001.\u0012.\u0001(\u0002))
			{
				_IVariable variable = this.\u0001.GetVariable();
				_ISignature signature = this.\u0001.GetSignature();
				string u2 = global::\u0003.\u0006.\u0001(global::\u0001.\u0012.\u0001.MessageId, new object[]
				{
					\u0002.OrgName,
					variable.OrgName
				});
				signature.AddMessage(\u0019.\u0003.\u0001(variable.SourcePosition, u2, u, global::\u0001.\u0012.\u0001.MessageId));
			}
		}

		// Token: 0x0600362F RID: 13871 RVA: 0x000D98BC File Offset: 0x000D7ABC
		private static bool \u0001(IVariable \u0002)
		{
			return \u0002.GetFlag(VarFlag.Absolut) && !\u0002.GetFlag(VarFlag.NoInit) && !\u0002.GetFlag(VarFlag.Initialized) && !\u0002.GetFlag(VarFlag.ReplacedConstant) && !\u0002.HasAttribute(CompileAttributes.GET_ACCESS);
		}

		// Token: 0x06003630 RID: 13872 RVA: 0x000D9914 File Offset: 0x000D7B14
		private void \u0001(IVariable \u0002, AccessFlag \u0003, bool \u0004)
		{
			if (((\u0003 & AccessFlag.Read) == AccessFlag.Read || (\u0003 & AccessFlag.Call) == AccessFlag.Call || \u0004) && \u0002 != null && this.\u0001 != null && global::\u0001.\u0012.\u0001(\u0002))
			{
				_IVariable variable = this.\u0001.GetVariable();
				_ISignature signature = this.\u0001.GetSignature();
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_UninitialisedVariableUsedInInitialisation, new object[]
				{
					\u0002.OrgName,
					variable.OrgName
				});
				signature.AddError(\u0019.\u0003.\u0001(variable.SourcePosition, u, Severity.Error, MessageId.Err_UninitialisedVariableUsedInInitialisation));
			}
		}

		// Token: 0x06003631 RID: 13873 RVA: 0x000D999C File Offset: 0x000D7B9C
		private void \u0001(_IVarInitialEmptyStatement \u0002)
		{
			_IVariable variable = \u0002.GetVariable();
			_ISignature signature = \u0002.GetSignature();
			variable.SetFlag(VarFlag.Initialized, true);
			this.\u0001 = \u0002;
			this.\u0001(variable, signature);
		}

		// Token: 0x06003632 RID: 13874 RVA: 0x000D99D8 File Offset: 0x000D7BD8
		private void \u0001(_IVariable \u0002, _ISignature \u0003)
		{
			foreach (ICrossReference crossReference in \u0002.CrossReferences)
			{
				ISignature signature = this.\u0001[crossReference.CodeId];
				if (global::\u0001.\u0012.\u0001(signature))
				{
					_ISignature isignature = this.\u0001[signature.ParentSignatureId] as _ISignature;
					_IVariable ivariable;
					if (isignature != null && this.\u0001(\u0002, \u0003, isignature, out ivariable))
					{
						this.\u0001(isignature, \u0003, \u0002, ivariable._SourcePosition);
					}
				}
			}
		}

		// Token: 0x06003633 RID: 13875 RVA: 0x000D9A54 File Offset: 0x000D7C54
		private static bool \u0001(ISignature \u0002)
		{
			return \u0002 != null && \u0002.POUType == Operator.Method && \u0002.Name == IdentifierConstants.InitMethodName;
		}

		// Token: 0x06003634 RID: 13876 RVA: 0x000D9A78 File Offset: 0x000D7C78
		private bool \u0001(_IVariable \u0002, _ISignature \u0003, _ISignature \u0004, out _IVariable \u0005)
		{
			foreach (_IVariable ivariable in \u0004.AllVariables)
			{
				_IExprement iexprement = ivariable.Initial as _IExprement;
				if (iexprement != null)
				{
					using (IEnumerator<ICodePosition> enumerator2 = \u001A.\u0004.\u0001(iexprement, \u0003.Id, \u0002.Id, AccessFlag.Read).GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (enumerator2.Current.Access == AccessFlag.Read)
							{
								\u0005 = ivariable;
								return true;
							}
						}
					}
				}
			}
			\u0005 = null;
			return false;
		}

		// Token: 0x06003635 RID: 13877 RVA: 0x000D9B28 File Offset: 0x000D7D28
		private void \u0001(_ISignature \u0002, _ISignature \u0003, _IVariable \u0004, _ISourcePosition \u0005)
		{
			HashSet<_ISignature> hashSet = new HashSet<_ISignature>();
			Stack<_ISignature> stack = new Stack<_ISignature>();
			stack.Push(\u0002);
			Severity u = Severity.Error;
			MessageId messageId = MessageId.Err_UninitialisedVariableUsedInInitialisation;
			while (stack.Count > 0)
			{
				\u0002 = stack.Pop();
				if (hashSet.Add(\u0002))
				{
					IVariable[] array;
					ISignature[] array2;
					((_ICompileContext)this.\u0001.ApplicationContext).InstancePaths(\u0002, out array, out array2, false, false, true);
					for (int i = 0; i < array.Length; i++)
					{
						IVariable variable = array[i];
						_ISignature isignature = (_ISignature)array2[i];
						if (variable.GetFlag(VarFlag.Initialized))
						{
							string u2 = global::\u0003.\u0006.\u0001(messageId, new object[]
							{
								this.\u0001(\u0003, \u0004),
								variable.OrgName
							});
							isignature.AddError(\u0019.\u0003.\u0001(variable.SourcePosition, u2, u, messageId));
							_ISourcePosition sourcepos = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0003.LibraryPath), \u0003.ObjectGuid, \u0005.Position, \u0005.PositionOffset, \u0005.Length);
							isignature.AddMessage(sourcepos, Severity.Information, MessageId.Inf_RelatedPosition, Array.Empty<object>());
						}
						else if ((isignature.POUType == Operator.Type || isignature.POUType == Operator.FunctionBlock) && !hashSet.Contains(isignature))
						{
							stack.Push(isignature);
						}
					}
					u = Severity.Warning;
					messageId = MessageId.Wrn_UninitialisedVariableUsedInInitialisation;
				}
			}
		}

		// Token: 0x06003636 RID: 13878 RVA: 0x000D9C90 File Offset: 0x000D7E90
		private string \u0001(ISignature \u0002, IVariable \u0003)
		{
			string text = \u0002.OrgName + "." + \u0003.OrgName;
			if (!string.IsNullOrEmpty(\u0002.LibraryPath))
			{
				return text + "[" + \u0002.LibraryPath + "]";
			}
			return text;
		}

		// Token: 0x04000A8C RID: 2700
		private IScope5 \u0001;

		// Token: 0x04000A8D RID: 2701
		private _IVarInitialEmptyStatement \u0001;

		// Token: 0x04000A8E RID: 2702
		[CompilerGenerated]
		private readonly bool \u0001;

		// Token: 0x04000A8F RID: 2703
		private readonly LDictionary<string, string> \u0001 = new LDictionary<string, string>();

		// Token: 0x04000A90 RID: 2704
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x04000A91 RID: 2705
		private static readonly _ICompilerMessage \u0001 = \u0019.\u0003.\u0001(null, "", Severity.Warning, MessageId.Wrn_ReferenceToUninitializedVariable);

		// Token: 0x020003AB RID: 939
		private static class \u0001
		{
			// Token: 0x06003638 RID: 13880 RVA: 0x000D9CF4 File Offset: 0x000D7EF4
			public static _ISequenceStatement \u0001(_ISignature \u0002, IScope5 \u0003)
			{
				_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
				global::\u0001.\u0012.\u0001.\u0001(\u0002, \u0003, isequenceStatement, \u0002.AllRetains);
				global::\u0001.\u0012.\u0001.\u0001(\u0002, \u0003, isequenceStatement, \u0002.AllNonRetains);
				return isequenceStatement;
			}

			// Token: 0x06003639 RID: 13881 RVA: 0x000D9D24 File Offset: 0x000D7F24
			private static bool \u0001(_IVariable \u0002)
			{
				return !\u0002.HasFlag(VarFlag.Inout | VarFlag.External | VarFlag.ReplacedConstant | VarFlag.NoInit) && (\u0002.Address == null || \u0002.Address.Location != DirectVariableLocation.Input) && \u0002.HasFlag(VarFlag.Absolut) && !\u0002.IsProperty && !\u0002.GetFlag(VarFlag.Constant);
			}

			// Token: 0x0600363A RID: 13882 RVA: 0x000D9D84 File Offset: 0x000D7F84
			private static void \u0001(_ISignature \u0002, IScope5 \u0003, _ISequenceStatement \u0004, IList<IVariable> \u0005)
			{
				foreach (_IVariable ivariable in \u0005.OfType<_IVariable>().Where(new Func<_IVariable, bool>(global::\u0001.\u0012.\u0001.\u0001)))
				{
					global::\u0001.\u0012.\u0001.\u0001(\u0004, ivariable);
					ICompiledType compiledType = ivariable.CompiledType;
					if (compiledType.DeRefType.Class == TypeClass.Array)
					{
						_IArrayType iarrayType = (_IArrayType)compiledType.DeRefType;
						compiledType = \u0084.\u0004.\u0001(iarrayType);
						if (iarrayType.GetNumOfElements(\u0003) == 0)
						{
							continue;
						}
					}
					IExpression expression = global::\u0001.\u0012.\u0001.\u0001(\u0002, \u0003, ivariable, compiledType);
					_IVarInitialEmptyStatement sm = \u0019.\u0003.\u0001(ivariable, \u0002);
					\u0004.Add(sm);
					if (ivariable.InputAssignments != null)
					{
						foreach (_IAssignmentExpression iassignmentExpression in ivariable.InputAssignments.OfType<_IAssignmentExpression>())
						{
							_IAssignmentExpression iassignmentExpression2 = iassignmentExpression.Duplicate() as _IAssignmentExpression;
							\u001C.\u0013.\u0001(iassignmentExpression2, \u0003);
							_IExpressionStatement sm2 = \u0019.\u0003.\u0001((_IExpression)iassignmentExpression2.RValue, Token.Empty);
							\u0004.Add(sm2);
						}
					}
					if (expression != null)
					{
						_IExpressionStatement sm3 = \u0019.\u0003.\u0001((_IExpression)expression, Token.Empty);
						\u0004.Add(sm3);
					}
					global::\u0001.\u0012.\u0001.\u0002(\u0004, ivariable);
				}
			}

			// Token: 0x0600363B RID: 13883 RVA: 0x000D9ED0 File Offset: 0x000D80D0
			private static void \u0001(_ISequenceStatement \u0002, _IVariable \u0003)
			{
				global::\u0001.\u0012.\u0001.\u0001(\u0002, false, \u0003);
			}

			// Token: 0x0600363C RID: 13884 RVA: 0x000D9EDC File Offset: 0x000D80DC
			private static void \u0002(_ISequenceStatement \u0002, _IVariable \u0003)
			{
				global::\u0001.\u0012.\u0001.\u0001(\u0002, true, \u0003);
			}

			// Token: 0x0600363D RID: 13885 RVA: 0x000D9EE8 File Offset: 0x000D80E8
			private static void \u0001(_ISequenceStatement \u0002, bool \u0003, _IVariable \u0004)
			{
				foreach (string text in \u0004.Attributes)
				{
					if (text.StartsWith("suppress_warning"))
					{
						\u0002.Add(\u0019.\u0003.\u0001(Token.Empty, \u0003, \u0004.GetAttributeValue(text)));
					}
				}
				if (APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(\u0004, GUIHidingFlags.AllCommon))
				{
					\u0002.Add(\u0019.\u0003.\u0001(Token.Empty, \u0003, global::\u0001.\u0012.NOINITWARNING_ID));
				}
			}

			// Token: 0x0600363E RID: 13886 RVA: 0x000D9F64 File Offset: 0x000D8164
			private static IExpression \u0001(_ISignature \u0002, IScope5 \u0003, _IVariable \u0004, ICompiledType \u0005)
			{
				_IExpression iexpression = \u0004._Initial;
				if (\u0005.DeRefType.Class == TypeClass.Userdef)
				{
					_IUserdefType iuserdefType = (_IUserdefType)\u0005.DeRefType;
					_ISignature isignature = \u0003[iuserdefType.SignatureId] as _ISignature;
					if (isignature != null && isignature.POUType == Operator.Type && isignature.GetFlag(SignatureFlag.Alias) && iexpression == null)
					{
						return \u0002.AllVariables[0].Initial as _IExpression;
					}
				}
				if (iexpression != null)
				{
					iexpression = (_IExpression)iexpression.Duplicate();
					\u001C.\u0013.\u0001(iexpression, \u0003);
				}
				return iexpression;
			}
		}
	}
}
