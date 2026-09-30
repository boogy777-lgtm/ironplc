using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using \u0011;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification.MessageSuppression;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000284 RID: 644
	public class LateCodeGenerator
	{
		// Token: 0x060028B3 RID: 10419 RVA: 0x0008E78C File Offset: 0x0008C98C
		public LateCodeGenerator(_ICompileContext comcon)
		{
			this.\u0001 = comcon;
		}

		// Token: 0x060028B4 RID: 10420 RVA: 0x0008E838 File Offset: 0x0008CA38
		public static LateCodeGenerator CreateNormalGenerator(_ICompileContext comcon)
		{
			return new LateCodeGenerator(comcon);
		}

		// Token: 0x060028B5 RID: 10421 RVA: 0x0008E840 File Offset: 0x0008CA40
		public static LateCodeGenerator CreateGeneratorAfterInterfaceReplacement(_ICompileContext comcon)
		{
			return new LateCodeGeneratorAfterInterfaceReplacement(comcon);
		}

		// Token: 0x060028B6 RID: 10422 RVA: 0x0008E848 File Offset: 0x0008CA48
		public static LateCodeGenerator CreateGeneratorAfterInterfaceAndReferenceReplacement(_ICompileContext comcon)
		{
			return new LateCodeGeneratorAfterInterfaceAndReferenceReplacement(comcon);
		}

		// Token: 0x060028B7 RID: 10423 RVA: 0x0008E850 File Offset: 0x0008CA50
		public _IExpression GenerateExpression(string stInput, IScope5 scopeIn, _ICompiledPOU cpou)
		{
			bool flag;
			_IExpression u = this.\u0001(stInput, out flag);
			Debug.\u0001(!flag);
			return this.\u0001<_IExpression>(u, scopeIn, cpou);
		}

		// Token: 0x060028B8 RID: 10424 RVA: 0x0008E87C File Offset: 0x0008CA7C
		internal _IStatement \u0001(string \u0002, IScope5 \u0003, _ICompiledPOU \u0004)
		{
			this.\u0001.UsedScanner.Initialize(\u0002 + ";");
			_IStatement u = this.\u0001.\u0001();
			return this.\u0001<_IStatement>(u, \u0003, \u0004);
		}

		// Token: 0x060028B9 RID: 10425 RVA: 0x0008E8BC File Offset: 0x0008CABC
		internal _IExpression \u0001(string \u0002, out bool \u0003)
		{
			this.\u0001.UsedScanner.Initialize(\u0002);
			return this.\u0001.\u0001(out \u0003);
		}

		// Token: 0x060028BA RID: 10426 RVA: 0x0008E8DC File Offset: 0x0008CADC
		internal \u0001 \u0001<\u0001>(\u0001 \u0002, IScope5 \u0003, _ICompiledPOU \u0004) where \u0001 : _IExprement
		{
			ExpressionTypifierWithSpecialTasks @object = this.\u0001.GetObject();
			@object.\u0001(\u0003, this.\u0001, false, \u0004);
			@object.NoCrossReferences = true;
			TypeCheckerVisitor object2 = this.\u0001.GetObject();
			object2.\u0001(\u0003, this.\u0001, true);
			object2.ConvertAllTypeMismatches = true;
			object2.MessageSuppressionController = IgnoreWarnings.Instance;
			object2.InImplicitCode = true;
			this.\u0001(@object, object2);
			ErrorVisitor object3 = this.\u0001.GetObject();
			\u0002.Accept(@object);
			\u0018.\u000E.\u0001(\u0002, this.\u0001);
			\u0002.Accept(object2);
			\u0002.Accept(object3);
			this.\u0001(object3);
			this.\u0001.PutObject(@object);
			this.\u0001.PutObject(object2);
			this.\u0001.PutObject(object3);
			return \u0002;
		}

		// Token: 0x060028BB RID: 10427 RVA: 0x0008E9BC File Offset: 0x0008CBBC
		[ExcludeFromCodeCoverage]
		private bool \u0001(_ICompilerMessage \u0002)
		{
			return (\u0002.Severity == Severity.Error || \u0002.Severity == Severity.FatalError) && !LateCodeGenerator.\u0001.Contains(\u0002.MessageId);
		}

		// Token: 0x060028BC RID: 10428 RVA: 0x0008E9E8 File Offset: 0x0008CBE8
		private void \u0001(ErrorVisitor \u0002)
		{
			if (\u0002.MessageList.Any(new Func<_ICompilerMessage, bool>(this.\u0001)))
			{
				throw new LateCompileErrorException(\u0002.MessageList.First(new Func<_ICompilerMessage, bool>(this.\u0001)).Text);
			}
		}

		// Token: 0x060028BD RID: 10429 RVA: 0x0008EA28 File Offset: 0x0008CC28
		public void CopyPositionAndMessages(_IExprement source, _IExprement target)
		{
			target._Position = source._Position;
			target.SetPositionIntern(source._Position);
			LateCodeGenerator.\u0001(source, target);
		}

		// Token: 0x060028BE RID: 10430 RVA: 0x0008EA4C File Offset: 0x0008CC4C
		[ExcludeFromCodeCoverage]
		private static void \u0001(_IExprement \u0002, _IExprement \u0003)
		{
			if (\u0002.MessagesList != null)
			{
				foreach (_ICompilerMessage cm in \u0002.MessagesList)
				{
					\u0003.AddMessage(cm);
				}
			}
		}

		// Token: 0x060028BF RID: 10431 RVA: 0x0008EAA4 File Offset: 0x0008CCA4
		public _IStatement DisableFlowBPForallExceptFirst(_IStatement st, IMinimalPosition position)
		{
			_ISequenceStatement isequenceStatement = st as _ISequenceStatement;
			if (isequenceStatement != null)
			{
				bool flag = false;
				using (IEnumerator<_IStatement> enumerator = isequenceStatement._StatementList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_IStatement istatement = enumerator.Current;
						if (!flag && !(istatement is _IPragmaStatement) && !(istatement is _IEmptyStatement))
						{
							istatement._Position = position;
							istatement.SetFlag(StatementFlag.GenerateBP, true);
							istatement.SetFlag(StatementFlag.GenerateFlow, true);
							flag = true;
						}
						else
						{
							istatement.SetFlag(StatementFlag.GenerateBP, false);
							istatement.SetFlag(StatementFlag.GenerateFlow, false);
						}
					}
					return st;
				}
			}
			st._Position = position;
			return st;
		}

		// Token: 0x060028C0 RID: 10432 RVA: 0x0008EB40 File Offset: 0x0008CD40
		internal static _IVariableExpression \u0001(int \u0002)
		{
			return \u0019.\u0003.\u0001(IdentifierConstants.GetFPAddressVarName(\u0002));
		}

		// Token: 0x060028C1 RID: 10433 RVA: 0x0008EB50 File Offset: 0x0008CD50
		internal virtual void \u0001(ExpressionTypifierWithSpecialTasks \u0002, TypeCheckerVisitor \u0003)
		{
		}

		// Token: 0x060028C2 RID: 10434 RVA: 0x0008EB54 File Offset: 0x0008CD54
		public _IExpression Pass2GenerateExpression(string stInput, IScope5 scope, _ICompiledPOU cpou, bool bIgnoreErrors)
		{
			this.\u0001.UsedScanner.Initialize(stInput);
			bool flag;
			_IExpression iexpression = this.\u0001.\u0001(out flag);
			if (flag)
			{
				throw new InvalidOperationException("Invalid generated code in optimizer pass 2");
			}
			ExpressionTypifierWithSpecialTasks @object = this.\u0001.GetObject();
			@object.\u0001(scope, this.\u0001, false, cpou);
			@object.TreatReferenceAsPointer = true;
			@object.InterfaceAsInterface = true;
			@object.NoCrossReferences = true;
			TypeCheckerVisitor object2 = this.\u0001.GetObject();
			object2.\u0001(scope, this.\u0001, true);
			object2.TreatReferenceAsPointer = true;
			object2.InterfaceAsInterface = true;
			object2.ConvertAllTypeMismatches = true;
			object2.MessageSuppressionController = (bIgnoreErrors ? IgnoreWarningsAndErrors.Instance : NoSuppressions.Instance);
			ErrorVisitor object3 = this.\u0001.GetObject();
			iexpression.Accept(@object);
			iexpression.Accept(object2);
			iexpression.Accept(object3);
			this.\u0001.PutObject(@object);
			this.\u0001.PutObject(object2);
			this.\u0001.PutObject(object3);
			return iexpression;
		}

		// Token: 0x060028C3 RID: 10435 RVA: 0x0008EC48 File Offset: 0x0008CE48
		public void Pass2AttributeExprement(_IExprement expInput, IScope5 _Scope, _ICompiledPOU Cpou, bool bIgnoreErrors, bool bTreatReferenceAsPointer, bool bInterfaceAsInterface)
		{
			ExpressionTypifierWithSpecialTasks @object = this.\u0001.GetObject();
			@object.\u0001(_Scope, this.\u0001, false, Cpou);
			@object.TreatReferenceAsPointer = bTreatReferenceAsPointer;
			@object.InterfaceAsInterface = bInterfaceAsInterface;
			@object.NoCrossReferences = true;
			@object.Optimize = true;
			TypeCheckerVisitor object2 = this.\u0001.GetObject();
			object2.\u0001(_Scope, this.\u0001, true);
			object2.TreatReferenceAsPointer = true;
			object2.InterfaceAsInterface = true;
			object2.ConvertAllTypeMismatches = true;
			object2.MessageSuppressionController = (bIgnoreErrors ? IgnoreWarningsAndErrors.Instance : NoSuppressions.Instance);
			object2.InImplicitCode = true;
			ErrorVisitor object3 = this.\u0001.GetObject();
			expInput.Accept(@object);
			expInput.Accept(object2);
			expInput.Accept(object3);
			this.\u0001.PutObject(@object);
			this.\u0001.PutObject(object2);
			this.\u0001.PutObject(object3);
		}

		// Token: 0x0400077A RID: 1914
		private readonly _ICompileContext \u0001;

		// Token: 0x0400077B RID: 1915
		private readonly global::\u0011.\u0006 \u0001 = new global::\u0011.\u0006("", true);

		// Token: 0x0400077C RID: 1916
		internal readonly ObjectPool<ExpressionTypifierWithSpecialTasks> \u0001 = new ObjectPool<ExpressionTypifierWithSpecialTasks>(new Func<ExpressionTypifierWithSpecialTasks>(LateCodeGenerator.<>c.<>9.\u0001));

		// Token: 0x0400077D RID: 1917
		internal readonly ObjectPool<TypeCheckerVisitor> \u0001 = new ObjectPool<TypeCheckerVisitor>(new Func<TypeCheckerVisitor>(LateCodeGenerator.<>c.<>9.\u0001));

		// Token: 0x0400077E RID: 1918
		internal readonly ObjectPool<ErrorVisitor> \u0001 = new ObjectPool<ErrorVisitor>(new Func<ErrorVisitor>(LateCodeGenerator.<>c.<>9.\u0001));

		// Token: 0x0400077F RID: 1919
		private static readonly MessageId[] \u0001 = new MessageId[]
		{
			MessageId.Err_StrictEnumNotAMember,
			MessageId.Err_StrictEnumNoArithmeticAllowed
		};
	}
}
