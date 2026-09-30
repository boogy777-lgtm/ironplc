using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u0017;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002CD RID: 717
	public class TaskLocalAccessReplacer : AbstractReplacer, IReplacer
	{
		// Token: 0x06002B51 RID: 11089 RVA: 0x000986C8 File Offset: 0x000968C8
		private TaskLocalAccessReplacer(global::\u000E.\u0011 context)
		{
			this.\u0001 = context;
			this.\u0001 = \u0081.\u0010.\u0001(this, context);
		}

		// Token: 0x06002B52 RID: 11090 RVA: 0x000986E4 File Offset: 0x000968E4
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002)
		{
			return new ReplacerController(new TaskLocalAccessReplacer(\u0002), new global::\u0017.\u0013(\u0002));
		}

		// Token: 0x17000787 RID: 1927
		// (get) Token: 0x06002B53 RID: 11091 RVA: 0x000986F8 File Offset: 0x000968F8
		// (set) Token: 0x06002B54 RID: 11092 RVA: 0x00098700 File Offset: 0x00096900
		private _ICompiledPOU POU { get; set; }

		// Token: 0x06002B55 RID: 11093 RVA: 0x0009870C File Offset: 0x0009690C
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.POU = cpou;
			this.\u0001.ReplaceCode(cpou);
		}

		// Token: 0x06002B56 RID: 11094 RVA: 0x00098724 File Offset: 0x00096924
		public override _IExpression ReplaceVariableExpression(_IVariableExpression variableExpression, bool bReadAccess)
		{
			_IVariable u = variableExpression.GetVariable(this.\u0001._Scope) as _IVariable;
			_IExpression result;
			if (this.\u0001(variableExpression, u, bReadAccess, out result))
			{
				return result;
			}
			return variableExpression;
		}

		// Token: 0x06002B57 RID: 11095 RVA: 0x0009875C File Offset: 0x0009695C
		public override _IExpression ReplaceCompoAccessExpression(_ICompoAccessExpression compoAccessExpression, bool bReadAccess)
		{
			_IVariable u = compoAccessExpression._Right.GetVariable(this.\u0001._Scope) as _IVariable;
			_IExpression result;
			if (this.\u0001(compoAccessExpression._Right, u, bReadAccess, out result))
			{
				return result;
			}
			return compoAccessExpression;
		}

		// Token: 0x06002B58 RID: 11096 RVA: 0x000987A0 File Offset: 0x000969A0
		private bool \u0001(_IExpression \u0002, _IVariable \u0003, bool \u0004, out _IExpression \u0005)
		{
			\u0005 = null;
			if (\u0003 != null && \u0003.GetFlag(VarFlag.TaskLocal) && this.POU != null && !this.POU.GetFlagInternal(InternalCompiledPOUFlags.IsImplicitInitFunction) && !this.POU.Name.Contains(IdentifierConstants.GetCopyFunctionIdentification.ToUpperInvariant()) && !this.POU.Name.Contains(IdentifierConstants.PutCopyFunctionIdentification.ToUpperInvariant()))
			{
				if (!\u0004)
				{
					_ISignature isignature = (_ISignature)this.\u0001._Scope[this.POU.SignatureId];
					ITaskInfo[] tasksReferencingSignature = this.\u0001.Comcon.GetTasksReferencingSignature(isignature);
					ISignature signature = this.\u0001._Scope[\u0002.SignatureId];
					string attributeValue = signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_TASKLOCALGVL);
					if (tasksReferencingSignature.Length > 1 || (tasksReferencingSignature.Length == 1 && attributeValue.ToUpperInvariant() != tasksReferencingSignature[0].TaskName.ToUpperInvariant()))
					{
						string u = string.Format(\u0081.\u0002.Err_TaskLocalVariablesAccessNotAllowed, signature.OrgName, attributeValue);
						IMessage cm = global::\u0019.\u0003.\u0001(\u0002.Position, u, Severity.Error, MessageId.Err_TaskLocalVariablesAccessNotAllowed);
						isignature.AddError(cm);
					}
				}
				_IExpression u2 = this.\u0001(\u0002);
				\u0005 = this.\u0001.Generator.\u0001<_IExpression>(u2, this.\u0001._Scope, this.POU);
				\u0005._Position = \u0002._Position;
				return true;
			}
			return false;
		}

		// Token: 0x06002B59 RID: 11097 RVA: 0x00098930 File Offset: 0x00096B30
		private _IExpression \u0001(_IExpression \u0002)
		{
			_ISignature signGVL = \u0002.GetSignatureEx(this.\u0001._Scope) as _ISignature;
			_IVariable var = \u0002.GetVariable(this.\u0001._Scope) as _IVariable;
			string taskLocalVariablesGVLName = IdentifierConstants.GetTaskLocalVariablesGVLName(signGVL);
			string taskLocalVariablesArrayName = IdentifierConstants.GetTaskLocalVariablesArrayName(signGVL);
			string taskLocalVariablesComponentName = IdentifierConstants.GetTaskLocalVariablesComponentName(var);
			IExpression u = global::\u0019.\u0003.\u0001(taskLocalVariablesGVLName);
			_IVariableExpression u2 = global::\u0019.\u0003.\u0001(taskLocalVariablesArrayName);
			IExpression u3 = global::\u0019.\u0003.\u0001(u, u2);
			_ICurrentTaskExpression u4 = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001("TaskIndex"), Token.Empty);
			IExpression u5 = global::\u0019.\u0003.\u0001(u3, u4);
			_IVariableExpression u6 = global::\u0019.\u0003.\u0001(taskLocalVariablesComponentName);
			return global::\u0019.\u0003.\u0001(u5, u6);
		}

		// Token: 0x04000843 RID: 2115
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x04000844 RID: 2116
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x04000845 RID: 2117
		[CompilerGenerated]
		private _ICompiledPOU \u0001;
	}
}
