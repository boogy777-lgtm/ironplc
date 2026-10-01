using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0004;
using \u001F;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000F
{
	// Token: 0x0200034E RID: 846
	internal sealed class \u0016
	{
		// Token: 0x17000861 RID: 2145
		// (get) Token: 0x06003306 RID: 13062 RVA: 0x000C57E8 File Offset: 0x000C39E8
		private _ISignature Signature { get; }

		// Token: 0x17000862 RID: 2146
		// (get) Token: 0x06003307 RID: 13063 RVA: 0x000C57F0 File Offset: 0x000C39F0
		private _ICompileContext Comcon { get; }

		// Token: 0x17000863 RID: 2147
		// (get) Token: 0x06003308 RID: 13064 RVA: 0x000C57F8 File Offset: 0x000C39F8
		private _IVariable Variable { get; }

		// Token: 0x17000864 RID: 2148
		// (get) Token: 0x06003309 RID: 13065 RVA: 0x000C5800 File Offset: 0x000C3A00
		private _ISignature VariableTypeSignature { get; }

		// Token: 0x17000865 RID: 2149
		// (get) Token: 0x0600330A RID: 13066 RVA: 0x000C5808 File Offset: 0x000C3A08
		private _ISignature EffectiveTypeSignature { get; }

		// Token: 0x0600330B RID: 13067 RVA: 0x000C5810 File Offset: 0x000C3A10
		internal \u0016(_ISignature \u001C\u0002, _ICompileContext \u0001\u0002, _IVariable \u001A\u0002, _ISignature \u0086\u0005, _ISignature \u0087\u0005)
		{
			this.Signature = \u001C\u0002;
			this.Comcon = \u0001\u0002;
			this.Variable = \u001A\u0002;
			this.VariableTypeSignature = \u0086\u0005;
			this.EffectiveTypeSignature = \u0087\u0005;
		}

		// Token: 0x0600330C RID: 13068 RVA: 0x000C5840 File Offset: 0x000C3A40
		internal void \u0001(IScope5 \u0002, ExpressionTypifierWithSpecialTasks \u0003)
		{
			if (this.VariableTypeSignature != null)
			{
				this.\u0002(\u0002, \u0003);
				bool flag = this.VariableTypeSignature.POUType == Operator.Interface;
				if (this.Signature.GetFlag(SignatureFlag.External) && (this.VariableTypeSignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion) || flag) && !this.VariableTypeSignature.GetFlag(SignatureFlag.TopLevel))
				{
					this.VariableTypeSignature.SetFlag(SignatureFlag.TopLevel, true);
				}
			}
		}

		// Token: 0x0600330D RID: 13069 RVA: 0x000C58B8 File Offset: 0x000C3AB8
		private void \u0002(IScope5 \u0002, ExpressionTypifierWithSpecialTasks \u0003)
		{
			if (!this.Variable.HasFlag(VarFlag.Alias) && this.EffectiveTypeSignature != null)
			{
				_ISignature isignature = (_ISignature)this.EffectiveTypeSignature.GetSubSignature(IdentifierConstants.InitMethodName);
				if (isignature != null && this.Variable.InputAssignments != null && isignature.GetFlagInternal(SignatureFlagInternal.Overloaded))
				{
					this.\u0001(\u0003);
					isignature = \u001F.\u0010.\u0001((ICommonScope)\u0002, this.Comcon, this.Variable.InputAssignments, isignature, (_ISignature4)this.EffectiveTypeSignature).FirstOrDefault<_ISignature>();
				}
				if (isignature != null)
				{
					this.\u0001(\u0002, isignature, \u0003);
				}
			}
		}

		// Token: 0x0600330E RID: 13070 RVA: 0x000C5958 File Offset: 0x000C3B58
		private void \u0001(IScope5 \u0002, ISignature \u0003, ExpressionTypifierWithSpecialTasks \u0004)
		{
			_IVariable[] array = Helper.\u0001((_ISignature)\u0003).ToArray<_IVariable>();
			if (!this.Variable.GetFlag(VarFlag.NoInit) && (array.Length > 3 || this.Variable.InputAssignments != null))
			{
				if (this.Variable.Type is _IArrayType)
				{
					this.\u0001(\u0002, \u0003, \u0004, array);
					return;
				}
				this.\u0001(array, \u0002, \u0003, \u0004);
			}
		}

		// Token: 0x0600330F RID: 13071 RVA: 0x000C59C4 File Offset: 0x000C3BC4
		private void \u0001(IScope5 \u0002, ISignature \u0003, ExpressionTypifierWithSpecialTasks \u0004, _IVariable[] \u0005)
		{
			if (this.Variable.InputAssignments == null)
			{
				return;
			}
			if (\u0005.Length - 3 == this.Variable.InputAssignments.Length)
			{
				this.\u0001(\u0005, \u0002, \u0003, \u0004);
				this.Variable.AddAttribute("old_input_assignments", null);
				return;
			}
			this.\u0001(\u0003, \u0002, \u0005, \u0004);
		}

		// Token: 0x06003310 RID: 13072 RVA: 0x000C5A1C File Offset: 0x000C3C1C
		private void \u0001(_IVariable[] \u0002, IScope5 \u0003, ISignature \u0004, ExpressionTypifierWithSpecialTasks \u0005)
		{
			if (this.Variable.InputAssignments != null && this.Variable.InputAssignments.Length == \u0002.Length - 3)
			{
				this.\u0001(\u0004, \u0003, \u0002, \u0005);
			}
		}

		// Token: 0x06003311 RID: 13073 RVA: 0x000C5A4C File Offset: 0x000C3C4C
		private void \u0001(ISignature \u0002, IScope5 \u0003, _IVariable[] \u0004, ExpressionTypifierWithSpecialTasks \u0005)
		{
			ExpressionTypifierWithSpecialTasks expressionTypifierWithSpecialTasks = new ExpressionTypifierWithSpecialTasks(global::\u0004.\u0012.\u0001(\u0003, this.Comcon, \u0002, false), this.Comcon, true, null);
			expressionTypifierWithSpecialTasks.ContributeToCompile = true;
			int num = \u0004.Length - 3;
			for (int i = 0; i < this.Variable.InputAssignments.Length; i++)
			{
				_IAssignmentExpression iassignmentExpression = (_IAssignmentExpression)this.Variable.InputAssignments[i];
				_IExpression lvalue = iassignmentExpression._LValue;
				_IExprement rvalue = iassignmentExpression._RValue;
				lvalue.Accept(expressionTypifierWithSpecialTasks);
				ICompiledType u;
				if (lvalue is _INullExpression)
				{
					int num2 = i % num + 2;
					u = \u0004[num2].CompiledType;
				}
				else
				{
					u = lvalue.Type;
				}
				\u0005.TypeExpected = u;
				rvalue.Accept(\u0005);
			}
		}

		// Token: 0x06003312 RID: 13074 RVA: 0x000C5AF4 File Offset: 0x000C3CF4
		private void \u0001(ExpressionTypifierWithSpecialTasks \u0002)
		{
			for (int i = 0; i < this.Variable.InputAssignments.Length; i++)
			{
				((_IAssignmentExpression)this.Variable.InputAssignments[i])._RValue.Accept(\u0002);
			}
		}

		// Token: 0x040009A5 RID: 2469
		[CompilerGenerated]
		private readonly _ISignature \u0001;

		// Token: 0x040009A6 RID: 2470
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x040009A7 RID: 2471
		[CompilerGenerated]
		private readonly _IVariable \u0001;

		// Token: 0x040009A8 RID: 2472
		[CompilerGenerated]
		private readonly _ISignature \u0002;

		// Token: 0x040009A9 RID: 2473
		[CompilerGenerated]
		private readonly _ISignature \u0003;

		// Token: 0x040009AA RID: 2474
		private const int \u0001 = 3;

		// Token: 0x040009AB RID: 2475
		private const int \u0002 = 2;

		// Token: 0x040009AC RID: 2476
		private const int \u0003 = 1;
	}
}
