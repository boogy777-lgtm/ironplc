using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0001;
using \u0003;
using \u0004;
using \u0006;
using \u0007;
using \u000E;
using \u0011;
using \u0012;
using \u0013;
using \u0014;
using \u0015;
using \u0016;
using \u0017;
using \u0018;
using \u0019;
using \u001D;
using \u001E;
using \u001F;
using _3S.CoDeSys.Compiler35220.Compile.Phase1_Typification.Code;
using _3S.CoDeSys.Compiler35220.Compile.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u007F;
using \u0081;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x0200032D RID: 813
	internal class TypifierAndCrossReferenceCollector : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2, IOperatorExpressionVisitor6, IOperatorExpressionVisitor5, IOperatorExpressionVisitor4, IOperatorExpressionVisitor3, IOperatorExpressionVisitor2, IOperatorExpressionVisitor, \u001F.\u0001, IExpressionTypifier6, IExpressionTypifier5, IExpressionTypifier4, IExpressionTypifier3, IExpressionTypifier2, IExpressionTypifier, _IExpressionTypifier
	{
		// Token: 0x06003072 RID: 12402 RVA: 0x000B82FC File Offset: 0x000B64FC
		internal TypifierAndCrossReferenceCollector()
		{
			this.\u0001 = new ObjectPool<TypifierAndCrossReferenceCollector.ETypeStackContent>(new Func<TypifierAndCrossReferenceCollector.ETypeStackContent>(TypifierAndCrossReferenceCollector.<>c.<>9.\u0001));
			this.\u0001 = new CaseInsensitiveDictionary<TypifierAndCrossReferenceCollector.\u0001>();
			base..ctor();
		}

		// Token: 0x06003073 RID: 12403 RVA: 0x000B8350 File Offset: 0x000B6550
		public TypifierAndCrossReferenceCollector(IScope5 supscope, _ICompileContext comcon, _ICompiledPOU cpou)
		{
			this.\u0001 = new ObjectPool<TypifierAndCrossReferenceCollector.ETypeStackContent>(new Func<TypifierAndCrossReferenceCollector.ETypeStackContent>(TypifierAndCrossReferenceCollector.<>c.<>9.\u0002));
			this.\u0001 = new CaseInsensitiveDictionary<TypifierAndCrossReferenceCollector.\u0001>();
			base..ctor();
			this.\u0001(supscope, null, AccessFlag.Unknown, null);
			this.\u0001 = comcon;
			this.\u0001 = cpou;
			if (supscope != null)
			{
				this.TypeChecker = new global::\u0014.\u0012(supscope as _IScope2, comcon, cpou);
			}
		}

		// Token: 0x06003074 RID: 12404 RVA: 0x000B83D4 File Offset: 0x000B65D4
		public void \u0001()
		{
			this.\u0001.Clear();
			this.\u0001 = null;
			this.\u0001 = null;
			this.\u0001 = null;
			this.\u0001 = null;
			this.TypeChecker = null;
			this.VarsToCheck = null;
		}

		// Token: 0x17000809 RID: 2057
		// (get) Token: 0x06003075 RID: 12405 RVA: 0x000B840C File Offset: 0x000B660C
		// (set) Token: 0x06003076 RID: 12406 RVA: 0x000B8414 File Offset: 0x000B6614
		internal \u001F.\u0007 TypeChecker { get; set; }

		// Token: 0x1700080A RID: 2058
		// (get) Token: 0x06003077 RID: 12407 RVA: 0x000B8420 File Offset: 0x000B6620
		// (set) Token: 0x06003078 RID: 12408 RVA: 0x000B8428 File Offset: 0x000B6628
		internal LDictionary<global::\u0017.\u0004, global::\u0017.\u0004> VarsToCheck { get; set; }

		// Token: 0x06003079 RID: 12409 RVA: 0x000B8434 File Offset: 0x000B6634
		private void \u0001(object \u0002, MessageId \u0003, params object[] \u0004)
		{
			_IExpression iexpression = \u0002 as _IExpression;
			if (iexpression != null)
			{
				this.\u0001(iexpression, \u0003, \u0004);
			}
		}

		// Token: 0x0600307A RID: 12410 RVA: 0x000B8454 File Offset: 0x000B6654
		protected virtual void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			string format = global::\u000E.\u0018.\u0001(\u0003);
			\u0002.AddError(string.Format(format, \u0004), \u0003);
		}

		// Token: 0x0600307B RID: 12411 RVA: 0x000B8478 File Offset: 0x000B6678
		private void \u0002(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			string format = global::\u000E.\u0018.\u0001(\u0003);
			\u0002.AddWarning(string.Format(format, \u0004), \u0003);
		}

		// Token: 0x0600307C RID: 12412 RVA: 0x000B849C File Offset: 0x000B669C
		private void \u0001(object \u0002, _ICompilerMessage \u0003)
		{
			_IExpression iexpression = \u0002 as _IExpression;
			if (iexpression != null)
			{
				this.\u0001(iexpression, \u0003);
			}
		}

		// Token: 0x0600307D RID: 12413 RVA: 0x000B84BC File Offset: 0x000B66BC
		private void \u0001(_IExpression \u0002, _ICompilerMessage \u0003)
		{
			\u0002.AddMessage(\u0003, false, true);
		}

		// Token: 0x0600307E RID: 12414 RVA: 0x000B84C8 File Offset: 0x000B66C8
		protected virtual void \u0001(_ICompiledPOU \u0002, CompiledPOUFlags \u0003)
		{
			if (\u0002 != null)
			{
				\u0002.SetFlag(\u0003, true);
			}
		}

		// Token: 0x0600307F RID: 12415 RVA: 0x000B84D8 File Offset: 0x000B66D8
		protected virtual void \u0001(_ISignature \u0002, SignatureFlag \u0003)
		{
			if (\u0002 != null)
			{
				\u0002.SetFlag(\u0003, true);
			}
		}

		// Token: 0x1700080B RID: 2059
		// (get) Token: 0x06003080 RID: 12416 RVA: 0x000B84E8 File Offset: 0x000B66E8
		internal TypifierAndCrossReferenceCollector.ETypeStackContent TopOfStack
		{
			get
			{
				return this.\u0001.Peek();
			}
		}

		// Token: 0x1700080C RID: 2060
		// (get) Token: 0x06003081 RID: 12417 RVA: 0x000B84F8 File Offset: 0x000B66F8
		internal IScope5 _Scope
		{
			get
			{
				return this.TopOfStack.\u0001;
			}
		}

		// Token: 0x1700080D RID: 2061
		// (get) Token: 0x06003082 RID: 12418 RVA: 0x000B8508 File Offset: 0x000B6708
		// (set) Token: 0x06003083 RID: 12419 RVA: 0x000B8518 File Offset: 0x000B6718
		public _IStatement StatementReplacement
		{
			get
			{
				return this.TopOfStack.\u0001;
			}
			set
			{
				this.TopOfStack.\u0001 = value;
			}
		}

		// Token: 0x1700080E RID: 2062
		// (get) Token: 0x06003084 RID: 12420 RVA: 0x000B8528 File Offset: 0x000B6728
		// (set) Token: 0x06003085 RID: 12421 RVA: 0x000B8538 File Offset: 0x000B6738
		public ICompiledType TypeExpected
		{
			get
			{
				return this.TopOfStack.\u0001;
			}
			set
			{
				this.TopOfStack.\u0001 = value;
			}
		}

		// Token: 0x1700080F RID: 2063
		// (get) Token: 0x06003086 RID: 12422 RVA: 0x000B8548 File Offset: 0x000B6748
		// (set) Token: 0x06003087 RID: 12423 RVA: 0x000B8550 File Offset: 0x000B6750
		public bool InterpretPragmas { get; set; }

		// Token: 0x17000810 RID: 2064
		// (get) Token: 0x06003088 RID: 12424 RVA: 0x000B855C File Offset: 0x000B675C
		// (set) Token: 0x06003089 RID: 12425 RVA: 0x000B8560 File Offset: 0x000B6760
		public virtual bool InterfaceAsInterface
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x0600308A RID: 12426 RVA: 0x000B8568 File Offset: 0x000B6768
		protected void \u0001(IScope5 \u0002, ICompiledType \u0003, AccessFlag \u0004, _IExpressionStatement \u0005)
		{
			TypifierAndCrossReferenceCollector.ETypeStackContent @object = this.\u0001.GetObject();
			@object.\u0001 = \u0003;
			@object.\u0001 = \u0002;
			@object.\u0001 = \u0004;
			@object.\u0001 = \u0005;
			@object.\u0002 = false;
			if (this.\u0001.Count > 0)
			{
				@object.\u0001 = this.TopOfStack.\u0001;
			}
			else
			{
				@object.\u0001 = true;
			}
			this.\u0001.Push(@object);
		}

		// Token: 0x0600308B RID: 12427 RVA: 0x000B85DC File Offset: 0x000B67DC
		private void \u0001(IScope5 \u0002, ICompiledType \u0003)
		{
			this.\u0001(\u0002, \u0003, this.TopOfStack.\u0001, null);
		}

		// Token: 0x0600308C RID: 12428 RVA: 0x000B85F4 File Offset: 0x000B67F4
		protected void \u0001(AccessFlag \u0002)
		{
			this.\u0001(this._Scope, this.TypeExpected, \u0002, null);
		}

		// Token: 0x0600308D RID: 12429 RVA: 0x000B860C File Offset: 0x000B680C
		private void \u0001(AccessFlag \u0002, _IExpressionStatement \u0003)
		{
			this.\u0001(this._Scope, this.TypeExpected, \u0002, \u0003);
		}

		// Token: 0x0600308E RID: 12430 RVA: 0x000B8624 File Offset: 0x000B6824
		private void \u0002()
		{
			this.\u0001(this._Scope, this.TypeExpected);
		}

		// Token: 0x0600308F RID: 12431 RVA: 0x000B8638 File Offset: 0x000B6838
		protected void \u0003()
		{
			this.\u0001.PutObject(this.\u0001.Pop());
		}

		// Token: 0x06003090 RID: 12432 RVA: 0x000B8650 File Offset: 0x000B6850
		public void \u0001(IStatement \u0002)
		{
			_IStatement istatement = \u0002 as _IStatement;
			if (istatement != null)
			{
				istatement.Accept(this);
			}
			global::\u0018.\u000E.\u0001(istatement, this.\u0001);
		}

		// Token: 0x06003091 RID: 12433 RVA: 0x000B8670 File Offset: 0x000B6870
		public void \u0001(IExpression \u0002)
		{
			_IExpression iexpression = \u0002 as _IExpression;
			if (iexpression != null)
			{
				iexpression.Accept(this);
			}
			global::\u0018.\u000E.\u0001(iexpression, this.\u0001);
		}

		// Token: 0x06003092 RID: 12434 RVA: 0x000B8690 File Offset: 0x000B6890
		public void \u0001(_ICompiledPOU \u0002)
		{
			\u0002.GetParseTree().Accept(this);
		}

		// Token: 0x06003093 RID: 12435 RVA: 0x000B86A0 File Offset: 0x000B68A0
		public virtual void \u0001(_ISequenceStatement \u0002)
		{
			IList<_IStatement> statementList = \u0002._StatementList;
			if (this.\u0001 != null)
			{
				this.\u0001.NumberOfStatements += statementList.Count;
			}
			for (int i = 0; i < statementList.Count; i++)
			{
				_IStatement istatement = statementList[i];
				this.\u0002();
				try
				{
					istatement.Accept(this);
				}
				catch (Exception ex)
				{
					string text = "Internal error in _IStatement: " + istatement.ToString();
					istatement.AddError(text);
					text = "Exception text: " + ex.ToString();
					istatement.AddMessage(text, istatement._Position, Severity.Error, istatement.PositionLength, MessageId.None);
				}
				if (this.InterpretPragmas && this.StatementReplacement != null)
				{
					\u0002.Replace(this.StatementReplacement, i);
				}
				this.\u0003();
			}
		}

		// Token: 0x06003094 RID: 12436 RVA: 0x000B877C File Offset: 0x000B697C
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Condition.Accept(this);
			this.\u0003();
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06003095 RID: 12437 RVA: 0x000B87A4 File Offset: 0x000B69A4
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
			this.\u0001(AccessFlag.Read);
			\u0002._Condition.Accept(this);
			this.\u0003();
		}

		// Token: 0x06003096 RID: 12438 RVA: 0x000B87CC File Offset: 0x000B69CC
		public void \u0001(_IForStatement \u0002)
		{
			\u0002._CounterStart.Accept(this);
			_IExpression condition = \u0002._Condition;
			if (condition != null)
			{
				condition.Accept(this);
			}
			\u0002._UpperBound.Accept(this);
			_IExpression counter = \u0002._Counter;
			if (counter != null)
			{
				counter.Accept(this);
			}
			this.\u0001(AccessFlag.Read);
			_IExpression by = \u0002._By;
			if (by != null)
			{
				by.Accept(this);
			}
			this.\u0003();
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06003097 RID: 12439 RVA: 0x000B8840 File Offset: 0x000B6A40
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x06003098 RID: 12440 RVA: 0x000B8844 File Offset: 0x000B6A44
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06003099 RID: 12441 RVA: 0x000B8848 File Offset: 0x000B6A48
		public void \u0001(_IAssignmentExpression \u0002)
		{
			this.\u0001(AccessFlag.Write);
			\u0002._LValue.Accept(this);
			this.\u0003();
			this.\u0001(this._Scope, \u0002._LValue.Type, AccessFlag.Read, null);
			\u0002._RValue.Accept(this);
			this.\u0003();
			\u0002.Type = \u0002._LValue.Type;
			_IVariable ivariable = \u0002._LValue.GetVariable(this._Scope) as _IVariable;
			if (ivariable != null && ivariable.IsProperty)
			{
				\u0002.Info = new \u001D.\u0006();
			}
		}

		// Token: 0x0600309A RID: 12442 RVA: 0x000B88D8 File Offset: 0x000B6AD8
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Condition.Accept(this);
			this.\u0003();
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				this.\u0001(AccessFlag.Read);
				ielseIf._Condition.Accept(this);
				this.\u0003();
				ielseIf._Controlled.Accept(this);
			}
			_IStatement ifElse = \u0002._IfElse;
			if (ifElse == null)
			{
				return;
			}
			ifElse.Accept(this);
		}

		// Token: 0x0600309B RID: 12443 RVA: 0x000B8978 File Offset: 0x000B6B78
		public void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001(AccessFlag.Read);
				\u0002._Condition.Accept(this);
				this.\u0003();
			}
		}

		// Token: 0x0600309C RID: 12444 RVA: 0x000B899C File Offset: 0x000B6B9C
		public void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001(AccessFlag.Read);
				\u0002._Condition.Accept(this);
				this.\u0003();
			}
		}

		// Token: 0x0600309D RID: 12445 RVA: 0x000B89C0 File Offset: 0x000B6BC0
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x0600309E RID: 12446 RVA: 0x000B89C4 File Offset: 0x000B6BC4
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x0600309F RID: 12447 RVA: 0x000B89C8 File Offset: 0x000B6BC8
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x060030A0 RID: 12448 RVA: 0x000B89CC File Offset: 0x000B6BCC
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001(AccessFlag.Read, \u0002);
			\u0002._Expr.Accept(this);
			this.\u0003();
		}

		// Token: 0x060030A1 RID: 12449 RVA: 0x000B89E8 File Offset: 0x000B6BE8
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x060030A2 RID: 12450 RVA: 0x000B89EC File Offset: 0x000B6BEC
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x060030A3 RID: 12451 RVA: 0x000B89F0 File Offset: 0x000B6BF0
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x060030A4 RID: 12452 RVA: 0x000B89F4 File Offset: 0x000B6BF4
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x060030A5 RID: 12453 RVA: 0x000B89F8 File Offset: 0x000B6BF8
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x060030A6 RID: 12454 RVA: 0x000B89FC File Offset: 0x000B6BFC
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x060030A7 RID: 12455 RVA: 0x000B8A00 File Offset: 0x000B6C00
		public void \u0001(_ICallExpression \u0002)
		{
			IPredefinedCalleeTypeProvider u = this.\u0001;
			ICompiledType compiledType = (u != null) ? u.GetType(\u0002) : null;
			if (compiledType != null)
			{
				\u0002.Type = compiledType;
				return;
			}
			if (\u0002._Condition != null)
			{
				this.\u0001(AccessFlag.Read);
				\u0002._Condition.Accept(this);
				this.\u0003();
			}
			if (\u0002.ExpectedType != null)
			{
				IEnumerable<_ICompilerMessage> enumerable;
				TypeCompiler.\u0001(\u0002.ExpectedType, this._Scope, this.\u0001, this.TypeChecker, this, \u0002.Position, out enumerable);
				foreach (_ICompilerMessage cm in enumerable)
				{
					\u0002.AddMessage(cm);
				}
			}
			this.\u0001(AccessFlag.Call);
			\u0002._Callee.Accept(this);
			bool u2 = this.TopOfStack.\u0002;
			bool u3 = this.TopOfStack.\u0003;
			this.\u0003();
			\u0002.Type = null;
			_IUserdefType iuserdefType = null;
			if (\u0002._Callee.Type != null)
			{
				iuserdefType = (\u0002._Callee.Type.DeRefType as _IUserdefType);
			}
			_ISignature isignature = ((iuserdefType != null) ? iuserdefType.GetSignature(this._Scope) : null) as _ISignature;
			if (iuserdefType != null && isignature != null)
			{
				this.\u0001(\u0002, u2, u3, ref isignature);
			}
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			ICompiledType[] array = global::\u0015.\u0003.\u0001(\u0002, isignature);
			for (int i = 0; i < paramExpressions.Count; i++)
			{
				_IExpression iexpression = paramExpressions[i];
				if (iexpression != null)
				{
					this.\u0001(AccessFlag.Read);
					this.TopOfStack.\u0001 = array[i];
					iexpression.Accept(this);
					this.\u0003();
				}
			}
			IList<_IExpression> outputExpressions = \u0002.OutputExpressions;
			ICompiledType[] array2 = global::\u0015.\u0003.\u0002(\u0002, isignature);
			for (int j = 0; j < outputExpressions.Count; j++)
			{
				_IExpression iexpression2 = outputExpressions[j];
				if (iexpression2 != null)
				{
					this.\u0001(AccessFlag.Write);
					this.TopOfStack.\u0001 = array2[j];
					iexpression2.Accept(this);
					this.\u0003();
				}
			}
			if (isignature != null)
			{
				this.\u0001(\u0002, isignature);
			}
			if (isignature != null && isignature.HasAttribute(CompileAttributes.ATTRIBUTE_CHECK_POINTER))
			{
				\u0002.Type = \u0002.ParamExpressions[0].Type;
			}
			if (\u0002.Type != null && \u0002.Type.Class == TypeClass.Reference)
			{
				this.TopOfStack.\u0002 = true;
			}
		}

		// Token: 0x060030A8 RID: 12456 RVA: 0x000B8C5C File Offset: 0x000B6E5C
		private void \u0001(_ICallExpression \u0002, bool \u0003, bool \u0004, ref _ISignature \u0005)
		{
			this.\u0001(\u0002, ref \u0005);
			this.\u0001(\u0002, \u0004, \u0005);
			IScope5 u = global::\u0004.\u0012.\u0001(this._Scope, this.\u0001, \u0005, this.InterfaceAsInterface);
			this.\u0001(u, this.TypeExpected);
			this.TopOfStack.\u0001 = false;
			foreach (_IExpression iexpression in \u0002.Inputs)
			{
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
			foreach (_IExpression iexpression2 in \u0002.Outputs)
			{
				if (iexpression2 != null)
				{
					iexpression2.Accept(this);
				}
			}
			foreach (_IExpression iexpression3 in \u0002.EmptyAssigns)
			{
				if (iexpression3 != null)
				{
					iexpression3.Accept(this);
				}
			}
			this.\u0003();
			IVariable[] outputs = \u0005.Outputs;
			if ((\u0005.POUType == Operator.Function || \u0005.POUType == Operator.Method) && outputs.Length >= 1)
			{
				\u0002.Type = outputs[0].CompiledType;
			}
			\u0003 = this.\u0001(\u0003, \u0004, \u0005);
			this.\u0001(\u0002, \u0003, \u0005);
		}

		// Token: 0x060030A9 RID: 12457 RVA: 0x000B8DC8 File Offset: 0x000B6FC8
		private bool \u0001(bool \u0002, bool \u0003, _ISignature \u0004)
		{
			if (!\u0002 && !\u0003 && \u0004.POUType == Operator.Method && !\u0004.GetFlag(SignatureFlag.Final) && !\u0004.GetFlag(SignatureFlag.Private))
			{
				ISignature signature = this._Scope.LocalSignature;
				int num = (signature != null) ? signature.Id : 0;
				while (signature != null)
				{
					if (\u0004.ParentSignatureId == signature.Id)
					{
						\u0002 = true;
						break;
					}
					if (num == signature.BaseSignatureId)
					{
						break;
					}
					signature = this._Scope[signature.BaseSignatureId];
				}
			}
			return \u0002;
		}

		// Token: 0x060030AA RID: 12458 RVA: 0x000B8E54 File Offset: 0x000B7054
		private void \u0001(_ICallExpression \u0002, bool \u0003, _ISignature \u0004)
		{
			if (\u0003 && this.TopOfStack.\u0001 != null && !this.TopOfStack.\u0001.GetFlag(StatementFlag.Implicit) && ((\u0004.Name == IdentifierConstants.InitMethodName && this.\u0001.Name == IdentifierConstants.InitMethodName) || (\u0004.Name == IdentifierConstants.ExitMethodName && this.\u0001.Name == IdentifierConstants.ExitMethodName)))
			{
				string format = global::\u000E.\u0018.\u0001(MessageId.Wrn_MethodAlreadyCalledImplicitly);
				\u0002.AddWarning(string.Format(format, \u0004.OrgName), MessageId.Wrn_MethodAlreadyCalledImplicitly);
			}
		}

		// Token: 0x060030AB RID: 12459 RVA: 0x000B8F00 File Offset: 0x000B7100
		private void \u0001(_ICallExpression \u0002, ref _ISignature \u0003)
		{
			if (!\u0003.GetFlagInternal(SignatureFlagInternal.Overloaded))
			{
				return;
			}
			_ISignature4 isignature = this.\u0001.GetSignatureById(\u0003.ParentSignatureId) as _ISignature4;
			if (isignature == null)
			{
				\u0002.AddError("Internal Error: parent signature not found for overload");
				return;
			}
			foreach (_IExpression iexpression in \u0002.ParamExpressions)
			{
				iexpression.Accept(this);
			}
			IList<_ISignature> list = global::\u001F.\u0010.\u0001(this._Scope as ICommonScope, this.\u0001, \u0002, \u0003, isignature);
			if (global::\u001F.\u0010.\u0001(\u0002, list, new global::\u000E.\u0016(this.\u0001), new global::\u0012.\u0013(this.\u0001)))
			{
				IList<_ISignature> source = global::\u001F.\u0010.\u0001(this.\u0001, isignature, \u0003.Name);
				if (!source.Any<_ISignature>())
				{
					return;
				}
				\u0003 = source.First<_ISignature>();
			}
			else
			{
				\u0003 = list[0];
			}
			_IUserdefType iuserdefType = \u0002._Callee.Type.DeRefType as _IUserdefType;
			if (iuserdefType != null)
			{
				iuserdefType.SignatureId = \u0003.Id;
			}
			global::\u001F.\u0010.\u0001(\u0002._Callee, \u0003.Id);
		}

		// Token: 0x060030AC RID: 12460 RVA: 0x000B902C File Offset: 0x000B722C
		private void \u0001(_ICallExpression \u0002, bool \u0003, ISignature \u0004)
		{
			if (this.\u0001 != null)
			{
				this.\u0001((_ISignature)\u0004);
				_ISignature isignature = this._Scope[this.\u0001.SignatureId] as _ISignature;
				if (isignature != null)
				{
					this.\u0001(isignature, \u0004.Id, \u0003);
				}
			}
			else if (this._Scope.MethodSignature != null)
			{
				this.\u0002((_ISignature)this._Scope.MethodSignature, \u0004.Id);
			}
			else if (this._Scope.LocalSignature != null)
			{
				this.\u0002((_ISignature)this._Scope.LocalSignature, \u0004.Id);
			}
			foreach (_IExpression iexpression in \u0002.Inputs)
			{
				if (iexpression != null)
				{
					_ISignature isignature2 = this._Scope[iexpression.SignatureId] as _ISignature;
					if (isignature2 != null)
					{
						_IVariable u = isignature2[iexpression.VariableId] as _IVariable;
						if (this.\u0001 != null)
						{
							this.\u0001(u, isignature2, this.\u0001.SignatureId);
						}
					}
				}
			}
			foreach (_IExpression iexpression2 in \u0002.Outputs)
			{
				_IVariable u2 = \u0004[iexpression2.VariableId] as _IVariable;
				if (this.\u0001 != null)
				{
					this.\u0001(u2, \u0004 as _ISignature, this.\u0001.SignatureId);
				}
			}
		}

		// Token: 0x060030AD RID: 12461 RVA: 0x000B91C0 File Offset: 0x000B73C0
		protected virtual void \u0001(_IVariable \u0002, _ISignature \u0003, int \u0004)
		{
			if (\u0002 != null)
			{
				\u0002.AddCrossReference(\u0004, null);
			}
		}

		// Token: 0x060030AE RID: 12462 RVA: 0x000B91D0 File Offset: 0x000B73D0
		protected virtual void \u0001(_IVariableExpression \u0002, IVariable \u0003, ISignature \u0004)
		{
			if (\u0003 != null && (\u0003 as _IVariable).IsProperty)
			{
				_ISignature isignature = null;
				_ISignature isignature2 = null;
				if (\u0004 != null)
				{
					IScope5 scope = this._Scope.CreateLocalScope(\u0004 as _ISignature);
					isignature = (scope.FindSignatureLocal(IdentifierConstants.CreateGetterName((\u0003 as _IVariable).VersionedName)) as _ISignature);
					isignature2 = (scope.FindSignatureLocal(IdentifierConstants.CreateSetterName((\u0003 as _IVariable).VersionedName)) as _ISignature);
				}
				_ISignature isignature3 = null;
				if (this.\u0001 != null)
				{
					isignature3 = (this._Scope[this.\u0001.SignatureId] as _ISignature);
				}
				else if (this.\u0001 != null)
				{
					isignature3 = this.\u0001;
				}
				if (\u0002.GetFlag(VarExprFlag.WriteAccess) && isignature2 != null)
				{
					this.\u0001(isignature2);
					if (isignature3 != null)
					{
						this.\u0002(isignature3, isignature2.Id);
						return;
					}
				}
				else if (isignature != null)
				{
					this.\u0001(isignature);
					if (isignature3 != null)
					{
						this.\u0002(isignature3, isignature.Id);
					}
				}
			}
		}

		// Token: 0x060030AF RID: 12463 RVA: 0x000B92B8 File Offset: 0x000B74B8
		protected virtual void \u0001(ISignature \u0002)
		{
			if (\u0002 != null)
			{
				if (this.\u0001 != null && this.\u0001.SignatureId != \u0002.Id)
				{
					_ISignature isignature = this._Scope[this.\u0001.SignatureId] as _ISignature;
					if (isignature == null)
					{
						return;
					}
					isignature.AddUsed(\u0002.Id);
					return;
				}
				else if (this.\u0001 == null)
				{
					if (this._Scope.MethodSignature != null)
					{
						((_ISignature)this._Scope.MethodSignature).AddUsed(\u0002.Id);
						return;
					}
					if (this._Scope.LocalSignature != null && this._Scope.LocalSignature.Id != \u0002.Id)
					{
						((_ISignature)this._Scope.LocalSignature).AddUsed(\u0002.Id);
					}
				}
			}
		}

		// Token: 0x060030B0 RID: 12464 RVA: 0x000B9384 File Offset: 0x000B7584
		protected virtual void \u0001(IVariable \u0002, ISignature \u0003)
		{
			if (\u0002 != null)
			{
				ICompiledType compiledType = global::\u0084.\u0004.\u0001(\u0002.CompiledType);
				_IPointerType ipointerType = compiledType as _IPointerType;
				_IUserdefType iuserdefType = ((ipointerType != null) ? ipointerType.BaseType : null) as _IUserdefType;
				_IUserdefType iuserdefType2;
				if ((iuserdefType2 = iuserdefType) == null)
				{
					_IReferenceType ireferenceType = compiledType as _IReferenceType;
					iuserdefType2 = (((ireferenceType != null) ? ireferenceType.BaseType : null) as _IUserdefType);
				}
				_IUserdefType iuserdefType3 = iuserdefType2;
				_ISignature isignature = ((iuserdefType3 != null) ? iuserdefType3.GetSignature(this._Scope) : null) as _ISignature;
				if (isignature != null)
				{
					if (this.\u0001 != null && this.\u0001.SignatureId != \u0003.Id)
					{
						_ISignature isignature2 = this._Scope[this.\u0001.SignatureId] as _ISignature;
						if (isignature2 != null)
						{
							isignature.AddReferencer(isignature2.Id);
							return;
						}
					}
					else if (this.\u0001 == null)
					{
						if (this._Scope.MethodSignature != null)
						{
							isignature.AddReferencer(this._Scope.MethodSignature.Id);
							return;
						}
						if (this._Scope.LocalSignature != null && this._Scope.LocalSignature.Id != \u0003.Id)
						{
							isignature.AddReferencer(this._Scope.LocalSignature.Id);
						}
					}
				}
			}
		}

		// Token: 0x060030B1 RID: 12465 RVA: 0x000B94A4 File Offset: 0x000B76A4
		protected virtual void \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			IVariable[] array = global::\u0015.\u0003.\u0001(\u0002, \u0003);
			for (int i = 0; i < paramExpressions.Count; i++)
			{
				_IExpression iexpression = paramExpressions[i];
				if (iexpression != null && array[i] != null && array[i].HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
				{
					_IArrayType iarrayType = iexpression._CompiledType as _IArrayType;
					if (iarrayType != null)
					{
						foreach (_IArrayDimension iarrayDimension in iarrayType._Dimensions)
						{
							global::\u0001.\u0004.\u0001(iarrayDimension._LowerBorder, this.\u0001.SignatureId, this._Scope);
							global::\u0001.\u0004.\u0001(iarrayDimension._UpperBorder, this.\u0001.SignatureId, this._Scope);
						}
					}
				}
			}
		}

		// Token: 0x060030B2 RID: 12466 RVA: 0x000B9584 File Offset: 0x000B7784
		private ICompiledType \u0001(ICompiledType \u0002, _IOperatorExpression \u0003)
		{
			if (global::\u001E.\u000E.\u0001(\u0003.Code))
			{
				return TypeTable.AnyNum;
			}
			if (\u0002 == null)
			{
				return null;
			}
			return \u0002;
		}

		// Token: 0x060030B3 RID: 12467 RVA: 0x000B95A0 File Offset: 0x000B77A0
		private ICompiledType \u0002(ICompiledType \u0002, _IOperatorExpression \u0003)
		{
			if (\u0002 == null)
			{
				return null;
			}
			bool flag = global::\u001D.\u0005.\u0001(\u0003);
			if (TypeTable.IsSigned(\u0002.Class) && flag)
			{
				switch (\u0002.Class)
				{
				case TypeClass.SInt:
					return TypeTable.USInt;
				case TypeClass.Int:
					return TypeTable.Int;
				case TypeClass.DInt:
					return TypeTable.DInt;
				case TypeClass.LInt:
					return TypeTable.ULInt;
				}
			}
			return \u0002;
		}

		// Token: 0x060030B4 RID: 12468 RVA: 0x000B9604 File Offset: 0x000B7804
		public void \u0002(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			bool flag = false;
			ICompiledType u = this.\u0001(this.TypeExpected, \u0002);
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression = operandsList[i];
				this.\u0001(this._Scope, u, AccessFlag.Read, null);
				iexpression.Accept(this);
				this.\u0003();
				if (iexpression.Type == null)
				{
					this.\u0001(iexpression, MessageId.Err_UnknownType, new object[]
					{
						iexpression.ToString()
					});
					flag = true;
				}
				else if (global::\u001E.\u000E.\u0001(\u0002.Code) && TypeTable.IsConcreteType(iexpression.Type.Class))
				{
					u = iexpression.Type;
				}
			}
			if (flag)
			{
				return;
			}
			this.\u0002();
			\u0002.AcceptOperatorVisitor(this);
			this.\u0003();
			\u0002._CompiledType = this.\u0002(\u0002._CompiledType, \u0002);
			this.\u0001(\u0002);
		}

		// Token: 0x060030B5 RID: 12469 RVA: 0x000B96E4 File Offset: 0x000B78E4
		protected virtual void \u0001(_IOperatorExpression \u0002)
		{
			_ISignature isignature = ImplicitFunctionCallsHandler.CheckForExternalFunctionCall(this.\u0001, \u0002, this._Scope);
			if (isignature != null)
			{
				this.\u0002(isignature);
			}
		}

		// Token: 0x060030B6 RID: 12470 RVA: 0x000B9710 File Offset: 0x000B7910
		protected virtual void \u0001(_IConversionExpression \u0002)
		{
			_ISignature isignature = ImplicitFunctionCallsHandler.CheckForExternalFunctionCall(this.\u0001, \u0002, this._Scope);
			if (isignature != null)
			{
				this.\u0002(isignature);
			}
		}

		// Token: 0x060030B7 RID: 12471 RVA: 0x000B973C File Offset: 0x000B793C
		public void \u0001(_ICastExpression \u0002)
		{
			\u0002.BaseExpression.Accept(this);
			if (\u0002.ExpWithType != null)
			{
				\u0002.ExpWithType.Accept(this);
				\u0002.Type = \u0002.ExpWithType.Type;
			}
			if (\u0002.ExplicitelySpecifiedType != null)
			{
				\u0002.Type = TypeCompiler.\u0001((_IType)\u0002.ExplicitelySpecifiedType, this._Scope, this.\u0001, this.TypeChecker, this, \u0002.Position);
			}
			if (\u0002.BaseExpression.Type != null)
			{
				\u0002.BaseExpression.Type = \u0002.Type;
			}
		}

		// Token: 0x060030B8 RID: 12472 RVA: 0x000B97D0 File Offset: 0x000B79D0
		public void \u0001(_INewExpression \u0002)
		{
			\u0002._Count.Accept(this);
			IEnumerable<_ICompilerMessage> enumerable;
			\u0002._TypeToCast = TypeCompiler.\u0001(\u0002._TypeToCast, this._Scope, this.\u0001, this.TypeChecker, this, \u0002.Position, out enumerable);
			foreach (_ICompilerMessage cm in enumerable)
			{
				\u0002.AddMessage(cm);
			}
			if (\u0002._TypeToCast.Class == TypeClass.Userdef)
			{
				_IUserdefType iuserdefType = \u0002._TypeToCast as _IUserdefType;
				ISignature signature = (iuserdefType != null) ? iuserdefType.GetSignature(this._Scope) : null;
				ISignature signature2 = null;
				if (signature != null)
				{
					signature2 = signature.GetSubSignature(IdentifierConstants.InitMethodName);
				}
				if (\u0002._FBInitParams != null && signature2 != null)
				{
					IScope5 u = global::\u0004.\u0012.\u0001(this._Scope, this.\u0001, signature2, false);
					foreach (_IAssignmentExpression iassignmentExpression in \u0002._FBInitParams.OfType<_IAssignmentExpression>())
					{
						this.\u0001(u, this.TypeExpected, AccessFlag.Write, null);
						this.TopOfStack.\u0001 = false;
						_IExprement iexprement = iassignmentExpression.LValue as _IExprement;
						if (iexprement != null)
						{
							iexprement.Accept(this);
						}
						this.\u0003();
						this.\u0002();
						this.TopOfStack.\u0001 = false;
						_IExprement iexprement2 = iassignmentExpression.RValue as _IExprement;
						if (iexprement2 != null)
						{
							iexprement2.Accept(this);
						}
						this.TypeExpected = iassignmentExpression.LValue.Type;
						_IExprement iexprement3 = iassignmentExpression.RValue as _IExprement;
						if (iexprement3 != null)
						{
							iexprement3.Accept(this);
						}
						this.\u0003();
						if (iassignmentExpression.LValue.Type is IReferenceType)
						{
							_IVariable u2 = Helper.\u0001(iassignmentExpression.LValue.ToString(), signature2.AllInputs) as _IVariable;
							this.TypeChecker.\u0001(iassignmentExpression._RValue, signature, iassignmentExpression._RValue, u2);
						}
					}
				}
			}
		}

		// Token: 0x060030B9 RID: 12473 RVA: 0x000B99E4 File Offset: 0x000B7BE4
		public void \u0001(_ITypeExpression \u0002)
		{
			IEnumerable<_ICompilerMessage> enumerable;
			\u0002._CompiledType = TypeCompiler.\u0001(\u0002._CompiledType as _IType, this._Scope, this.\u0001, this.TypeChecker, this, \u0002.Position, out enumerable);
			foreach (_ICompilerMessage cm in enumerable)
			{
				\u0002.AddMessage(cm);
			}
		}

		// Token: 0x060030BA RID: 12474 RVA: 0x000B9A60 File Offset: 0x000B7C60
		public void \u0002(_IConversionExpression \u0002)
		{
			TypeClass from;
			TypeClass to;
			this.\u0001(\u0002, out from, out to);
			TypeClass from2 = \u0002.From;
			TypeClass to2 = \u0002.To;
			\u0002.From = from;
			\u0002.To = to;
			this.\u0001(\u0002);
			\u0002.From = from2;
			\u0002.To = to2;
		}

		// Token: 0x060030BB RID: 12475 RVA: 0x000B9AAC File Offset: 0x000B7CAC
		protected virtual void \u0001(_IConversionExpression \u0002, out TypeClass \u0003, out TypeClass \u0004)
		{
			global::\u007F.\u000E.\u0001(\u0002, this.\u0001, out \u0003, out \u0004);
			if (\u0002.To == TypeClass.XWord)
			{
				if (this.\u0001.PointerSize == 8)
				{
					\u0002._CompiledType = TypeTable.XLWord;
				}
				else
				{
					\u0002._CompiledType = TypeTable.XDWord;
				}
			}
			this.\u0001(AccessFlag.Read);
			this.TopOfStack.\u0001 = TypeTable.Get(\u0003);
			\u0002._Exp.Accept(this);
			this.\u0003();
			if (\u0002._Exp.Type != null && \u0002._Exp.Type.DeRefType.Class == TypeClass.BitConst)
			{
				if (\u0003 == TypeClass.Any)
				{
					\u0002._Exp.Type = TypeTable.USInt;
				}
				else
				{
					\u0002._Exp.Type = TypeTable.Get(\u0003);
				}
			}
			if (\u0003 == TypeClass.Any && \u0002._Exp.Type != null)
			{
				if (\u0002._Exp.Type.DeRefType.Class != TypeClass.Userdef && \u0002._Exp.Type.DeRefType.Class != TypeClass.Pointer && \u0002._Exp.Type.DeRefType.Class != TypeClass.Array)
				{
					\u0003 = \u0002._Exp.Type.DeRefType.Class;
				}
				else if (\u0002._Exp.Type.DeRefType.Class == TypeClass.Pointer)
				{
					\u0003 = TypeClass.DWord;
					if (this.\u0001.PointerSize == 8)
					{
						\u0003 = TypeClass.LWord;
					}
				}
			}
			if (\u0003 == TypeClass.Reference && \u0004 == TypeClass.Pointer)
			{
				\u0002.Type = global::\u0019.\u0003.\u0001(\u0002._Exp.Type as _IType);
				return;
			}
			if (!TypeTable.IsResolvedXType(\u0002.Type))
			{
				\u0002.Type = TypeTable.Get(\u0004);
			}
		}

		// Token: 0x060030BC RID: 12476 RVA: 0x000B9C60 File Offset: 0x000B7E60
		public void \u0001(_IThisExpression \u0002)
		{
			ISignature localSignature = this._Scope.LocalSignature;
			if (localSignature != null && (localSignature.POUType == Operator.FunctionBlock || localSignature.GetFlag(SignatureFlag.Structure)) && (this.\u0001 == null || !this.\u0001.GetFlag(VarFlag.Static)))
			{
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(localSignature.Name);
				iuserdefType.SignatureId = localSignature.Id;
				_IPointerType type = global::\u0019.\u0003.\u0001(iuserdefType);
				\u0002.Type = type;
			}
		}

		// Token: 0x060030BD RID: 12477 RVA: 0x000B9CD0 File Offset: 0x000B7ED0
		public void \u0001(_IBaseExpression \u0002)
		{
			_ISignature isignature = this._Scope.LocalSignature as _ISignature;
			if (isignature != null && (this.\u0001 == null || !this.\u0001.GetFlag(VarFlag.Static)))
			{
				ISignature signature = this._Scope[isignature.BaseSignatureId];
				if (signature != null && (signature.POUType == Operator.FunctionBlock || signature.GetFlag(SignatureFlag.Structure)))
				{
					_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(signature.Name);
					iuserdefType.SignatureId = signature.Id;
					\u0002.Type = global::\u0019.\u0003.\u0001(iuserdefType);
				}
			}
		}

		// Token: 0x060030BE RID: 12478 RVA: 0x000B9D5C File Offset: 0x000B7F5C
		public void \u0001(_ILiteralExpression \u0002)
		{
			\u0002.Type = Helper.\u0001(\u0002, this.\u0001.TypeIsSupported(TypeClass.LReal), this.\u0001.TreatLRealAsReal, this.\u0001.TypeIsSupported(TypeClass.LInt), this.\u0001.TreatInt64AsInt32, this.\u0001.IsDefined("NO_UNICODE_SUPPORT"), this.\u0001.HasByteSupport());
		}

		// Token: 0x060030BF RID: 12479 RVA: 0x000B9DC0 File Offset: 0x000B7FC0
		public void \u0001(_IAddressExpression \u0002)
		{
			this.\u0001(this.\u0001, CompiledPOUFlags.ContainsDirVarAccess);
			\u0002.Type = TypeTable.GetDirectVariableSizeType(\u0002.DirectAddress.Size);
		}

		// Token: 0x060030C0 RID: 12480 RVA: 0x000B9DEC File Offset: 0x000B7FEC
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x060030C1 RID: 12481 RVA: 0x000B9DF0 File Offset: 0x000B7FF0
		public void \u0001(_IVariableExpression \u0002)
		{
			IVariable variable = null;
			ISignature signature = null;
			IScope scope = null;
			int num = this.\u0001();
			bool flag = this.\u0001(\u0002, ref variable, ref signature, ref scope);
			if (variable != null)
			{
				if (!flag)
				{
					\u0002.Type = variable.CompiledType;
				}
				this.\u0001(\u0002, variable, signature, num);
			}
			else if (signature != null)
			{
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(signature.Name);
				iuserdefType.SignatureId = signature.Id;
				\u0002.Type = iuserdefType;
			}
			else if (scope != null)
			{
				_IUserdefType iuserdefType2 = global::\u0019.\u0003.\u0001(scope.Name);
				iuserdefType2.ScopeId = scope.Id;
				\u0002.Type = iuserdefType2;
			}
			this.\u0001(\u0002, variable, signature);
			this.\u0001(signature);
			this.\u0001(variable, signature);
			this.\u0002(variable, signature);
			if (variable != null && signature != null)
			{
				IScope5 scope2 = this._Scope;
				if (((scope2 != null) ? scope2.LocalSignature : null) != null && signature.Id != num && signature.POUType == Operator.VarGlobal && signature.HasAttribute(CompileAttributes.ATTRIBUTE_OBSOLETE))
				{
					string attributeValue = signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBSOLETE);
					if (!string.IsNullOrEmpty(attributeValue))
					{
						this.\u0002(\u0002, MessageId.Wrn_Obsolete, new object[]
						{
							signature.OrgName,
							attributeValue
						});
					}
				}
			}
		}

		// Token: 0x060030C2 RID: 12482 RVA: 0x000B9F14 File Offset: 0x000B8114
		protected int \u0001()
		{
			_ICompiledPOU u = this.\u0001;
			if (u != null)
			{
				return u.SignatureId;
			}
			_ISignature u2 = this.\u0001;
			if (u2 == null)
			{
				return -1;
			}
			return u2.Id;
		}

		// Token: 0x060030C3 RID: 12483 RVA: 0x000B9F38 File Offset: 0x000B8138
		private void \u0001(_IVariableExpression \u0002, IVariable \u0003, ISignature \u0004, int \u0005)
		{
			_IVariable ivariable = \u0003 as _IVariable;
			if (ivariable != null && (this.\u0001 != null || this.\u0001 != null))
			{
				this.\u0001(ivariable, \u0004 as _ISignature, \u0005);
				if (ivariable.Address != null)
				{
					this.\u0001(this.\u0001, CompiledPOUFlags.ContainsDirVarAccess);
				}
				if (\u0003.Type != null && \u0003.Type.Class == TypeClass.Reference)
				{
					this.TopOfStack.\u0002 = true;
				}
				this.\u0001(\u0002);
			}
		}

		// Token: 0x060030C4 RID: 12484 RVA: 0x000B9FB4 File Offset: 0x000B81B4
		private bool \u0001(_IVariableExpression \u0002, ref IVariable \u0003, ref ISignature \u0004, ref IScope \u0005)
		{
			if (\u0002.Type != null && (\u0002.VariableId != Helper.InvalidId || (\u0002.SignatureId != Helper.InvalidId && \u0002.ScopeId != Helper.InvalidId)))
			{
				this.\u0002(\u0002, ref \u0003, ref \u0004, ref \u0005);
				return true;
			}
			this.\u0001(\u0002, ref \u0003, ref \u0004, out \u0005);
			return false;
		}

		// Token: 0x060030C5 RID: 12485 RVA: 0x000BA00C File Offset: 0x000B820C
		private void \u0001(_IVariableExpression \u0002, ref IVariable \u0003, ref ISignature \u0004, out IScope \u0005)
		{
			IVariable[] array = null;
			ISignature[] array2 = null;
			if (this.TopOfStack.\u0001 && this.\u0001.ContainsKey(\u0002.Name))
			{
				TypifierAndCrossReferenceCollector.\u0001 u = this.\u0001[\u0002.Name];
				array = u.\u0001;
				array2 = u.\u0001;
				\u0005 = u.\u0001;
			}
			else
			{
				this._Scope.FindDeclaration(\u0002.Name, out array, out array2, out \u0005);
				if (array == null)
				{
					array = Array.Empty<IVariable>();
				}
				if (array2 == null)
				{
					array2 = Array.Empty<ISignature>();
				}
				if (this.TopOfStack.\u0001)
				{
					TypifierAndCrossReferenceCollector.\u0001 value = new TypifierAndCrossReferenceCollector.\u0001
					{
						\u0001 = \u0005,
						\u0001 = array2,
						\u0001 = array
					};
					this.\u0001[\u0002.Name] = value;
				}
			}
			if ((this.TopOfStack.\u0001 & AccessFlag.Write) == AccessFlag.Write)
			{
				\u0002.SetFlag(VarExprFlag.WriteAccess, true);
			}
			if ((this.TopOfStack.\u0001 & AccessFlag.InitializingWrite) != AccessFlag.None)
			{
				\u0002.SetFlag(VarExprFlag.InitializingWriteAccess, true);
			}
			if (this.\u0001(\u0002, array, array2))
			{
				if (array.Length != 0)
				{
					\u0003 = array[0];
					\u0004 = array2[0];
				}
				else if (array2.Length != 0)
				{
					\u0004 = array2[0];
				}
			}
			IVariable variable = \u0003;
			\u0002.VariableId = ((variable != null) ? variable.Id : Helper.InvalidId);
			ISignature signature = \u0004;
			\u0002.SignatureId = ((signature != null) ? signature.Id : Helper.InvalidId);
			IScope scope = \u0005;
			\u0002.ScopeId = ((scope != null) ? scope.Id : Helper.InvalidId);
		}

		// Token: 0x060030C6 RID: 12486 RVA: 0x000BA178 File Offset: 0x000B8378
		private void \u0002(_IVariableExpression \u0002, ref IVariable \u0003, ref ISignature \u0004, ref IScope \u0005)
		{
			if ((this.TopOfStack.\u0001 & AccessFlag.Write) == AccessFlag.Write)
			{
				\u0002.SetFlag(VarExprFlag.WriteAccess, true);
			}
			if ((this.TopOfStack.\u0001 & AccessFlag.InitializingWrite) != AccessFlag.None)
			{
				\u0002.SetFlag(VarExprFlag.InitializingWriteAccess, true);
			}
			\u0004 = this._Scope[\u0002.SignatureId];
			if (\u0004 != null)
			{
				\u0003 = \u0004[\u0002.VariableId];
			}
			if (\u0002.ScopeId != Helper.InvalidId)
			{
				\u0005 = this._Scope.GetScopeById(\u0002.ScopeId);
			}
		}

		// Token: 0x060030C7 RID: 12487 RVA: 0x000BA200 File Offset: 0x000B8400
		private void \u0002(IVariable \u0002, ISignature \u0003)
		{
			if (\u0002 != null && \u0003 != null && (\u0002 as _IVariable).IsProperty)
			{
				for (ISignature signature = this._Scope[\u0003.BaseSignatureId]; signature != null; signature = this._Scope[signature.BaseSignatureId])
				{
					IVariable variable = signature[\u0002.Name];
					if (variable != null)
					{
						if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_SET))
						{
							((IVariable2)\u0002).AddAttribute(CompileAttributes.ATTRIBUTE_SET, string.Empty);
						}
						if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_GET))
						{
							((IVariable2)\u0002).AddAttribute(CompileAttributes.ATTRIBUTE_GET, string.Empty);
						}
					}
				}
			}
		}

		// Token: 0x060030C8 RID: 12488 RVA: 0x000BA2A4 File Offset: 0x000B84A4
		private bool \u0001(_IVariableExpression \u0002, IVariable[] \u0003, ISignature[] \u0004)
		{
			bool result = true;
			if (\u0003.Length > 1)
			{
				result = false;
				this.\u0001(\u0002, MessageId.Err_Ambiguity, new object[]
				{
					\u0002.Name
				});
				for (int i = 0; i < \u0003.Length; i++)
				{
					_ISourcePosition isourcePosition = \u0003[i].SourcePosition as _ISourcePosition;
					if (isourcePosition != null && isourcePosition.ObjectGuid == Guid.Empty)
					{
						isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0004[i].LibraryPath), \u0004[i].ObjectGuid);
					}
					string u = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
					this.TypeChecker.\u0001(\u0002, global::\u0019.\u0003.\u0001(isourcePosition, u, Severity.Information, MessageId.Inf_RelatedPosition));
				}
			}
			else if (\u0004.Length > 1)
			{
				result = false;
				this.\u0001(\u0002, MessageId.Err_Ambiguity, new object[]
				{
					\u0002.Name
				});
				foreach (ISignature signature in \u0004)
				{
					_ISourcePosition isourcePosition2 = global::\u0019.\u0003.\u0001();
					isourcePosition2.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature.LibraryPath), signature.ObjectGuid);
					string u2 = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
					\u0002.AddMessage(global::\u0019.\u0003.\u0001(isourcePosition2, u2, Severity.Information, MessageId.Inf_RelatedPosition), true);
				}
			}
			return result;
		}

		// Token: 0x060030C9 RID: 12489 RVA: 0x000BA404 File Offset: 0x000B8604
		private void \u0001(_IExpression \u0002)
		{
			IVariable variable = \u0002.GetVariable(this._Scope);
			if (variable == null)
			{
				return;
			}
			if (this.\u0001 != null && variable.GetFlag(VarFlag.TaskLocal) && !this.\u0001.GetFlagInternal(InternalCompiledPOUFlags.IsImplicitInitFunction) && !this.\u0001.Name.Contains(IdentifierConstants.GetCopyFunctionIdentification.ToUpperInvariant()) && !this.\u0001.Name.Contains(IdentifierConstants.PutCopyFunctionIdentification.ToUpperInvariant()))
			{
				_ISignature isignature = this.\u0001.GetSignatureById(this.\u0001.SignatureId) as _ISignature;
				if (isignature != null)
				{
					ITargetSettings targetSettings = this.\u0001.GetTargetSettings();
					if (global::\u0016.\u0004.MaxStackSize.GetIntValue(targetSettings) <= 0 && this.\u0001.ApplicationGuid != Guid.Empty)
					{
						this.\u0001(\u0002, MessageId.Err_UnknownMaxStackSize, Array.Empty<object>());
					}
					this._Scope.GlobalScope.FindSignature("__CurrentTaskDetection");
					isignature.SetFlagInternal(SignatureFlagInternal.ContainsAccessToCurrentTask, true);
				}
			}
		}

		// Token: 0x060030CA RID: 12490 RVA: 0x000BA518 File Offset: 0x000B8718
		public virtual void \u0001(_IIndexAccessExpression \u0002)
		{
			\u0002._Var.Accept(this);
			this.\u0001(AccessFlag.Read);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				\u0002.GetAccess(i).Accept(this);
			}
			this.\u0003();
			TypifierAndCrossReferenceCollector.\u0001(\u0002);
		}

		// Token: 0x060030CB RID: 12491 RVA: 0x000BA564 File Offset: 0x000B8764
		internal static void \u0001(_IIndexAccessExpression \u0002)
		{
			if (\u0002._Var.Type != null)
			{
				ICompiledType deRefType = \u0002._Var.Type.DeRefType;
				TypeClass @class = deRefType.Class;
				if (@class <= TypeClass.WString)
				{
					if (@class == TypeClass.String)
					{
						\u0002.Type = TypeTable.Byte;
						return;
					}
					if (@class != TypeClass.WString)
					{
						return;
					}
					\u0002.Type = TypeTable.Word;
				}
				else if (@class == TypeClass.Pointer || @class == TypeClass.Array || @class == TypeClass.__Vector)
				{
					\u0002.Type = deRefType.BaseType;
					return;
				}
			}
		}

		// Token: 0x060030CC RID: 12492 RVA: 0x000BA5DC File Offset: 0x000B87DC
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
			if (\u0002._Left.Type == null)
			{
				return;
			}
			if (\u0002._Left.Type.DeRefType.IsInteger)
			{
				\u0002._Right.Accept(this);
				\u0002.Type = TypeTable.Bit;
				return;
			}
			if (\u0002._Left.Type.DeRefType.Class == TypeClass.Userdef)
			{
				_IUserdefType iuserdefType = \u0002._Left.Type.DeRefType as _IUserdefType;
				IScope5 scope = global::\u0004.\u0012.\u0001(this._Scope, this.\u0001, iuserdefType, this.InterfaceAsInterface);
				if (scope != null)
				{
					this.\u0001(scope, this.TypeExpected);
					this.TopOfStack.\u0001 = false;
					\u0002._Right.Accept(this);
					this.\u0003();
					if (\u0002._Right.Type != null)
					{
						\u0002.Type = \u0002._Right.Type;
					}
					this.\u0001(\u0002);
					return;
				}
				this.\u0001(\u0002._Left, MessageId.Err_UnknownType, new object[]
				{
					((iuserdefType != null) ? iuserdefType.NameExpression.ToString() : null) ?? \u0002._Left.Type.DeRefType.ToString()
				});
			}
		}

		// Token: 0x060030CD RID: 12493 RVA: 0x000BA714 File Offset: 0x000B8914
		public virtual void \u0001(_IDeRefAccessExpression \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Base.Accept(this);
			this.\u0003();
			if (\u0002._Base.Type == null)
			{
				return;
			}
			_IPointerType ipointerType = \u0002._Base.Type.DeRefType as _IPointerType;
			if (ipointerType != null)
			{
				\u0002.Type = ipointerType._Base;
			}
			this.TopOfStack.\u0002 = true;
			if (\u0002._Base is _IBaseExpression)
			{
				this.TopOfStack.\u0002 = false;
				this.TopOfStack.\u0003 = true;
			}
		}

		// Token: 0x060030CE RID: 12494 RVA: 0x000BA7A0 File Offset: 0x000B89A0
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			IScope5 u = (this._Scope as global::\u0007.\u0005)._CopyScope;
			this.\u0001(u, this.TypeExpected);
			this.TopOfStack.\u0001 = false;
			\u0002._Base.Accept(this);
			this.\u0003();
			\u0002.Type = \u0002._Base.Type;
		}

		// Token: 0x060030CF RID: 12495 RVA: 0x000BA7FC File Offset: 0x000B89FC
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			IScope5 u = this._Scope.GlobalScope as IScope5;
			this.\u0001(u, this.TypeExpected);
			this.TopOfStack.\u0001 = false;
			\u0002._Base.Accept(this);
			this.\u0003();
			\u0002.Type = \u0002._Base.Type;
		}

		// Token: 0x060030D0 RID: 12496 RVA: 0x000BA858 File Offset: 0x000B8A58
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			IScope5 systemScope = this._Scope.SystemScope;
			this.\u0001(systemScope, this.TypeExpected);
			this.TopOfStack.\u0001 = false;
			\u0002._Base.Accept(this);
			this.\u0003();
			\u0002.Type = \u0002._Base.Type;
		}

		// Token: 0x060030D1 RID: 12497 RVA: 0x000BA8B0 File Offset: 0x000B8AB0
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			Debug.\u0001(this._Scope is _IScope);
			IScope5 poolScope = (this._Scope as _IScope).PoolScope;
			this.\u0001(poolScope, this.TypeExpected);
			this.TopOfStack.\u0001 = false;
			\u0002._Base.Accept(this);
			this.\u0003();
			\u0002.Type = \u0002._Base.Type;
		}

		// Token: 0x060030D2 RID: 12498 RVA: 0x000BA920 File Offset: 0x000B8B20
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			Debug.\u0001(this._Scope is _IScope);
			_IScope iscope = (this._Scope as _IScope).FindScope(\u0002._Namespace) as _IScope;
			if (iscope != null)
			{
				this.\u0001(iscope, this.TypeExpected);
				this.TopOfStack.\u0001 = false;
				\u0002._Access.Accept(this);
				this.\u0003();
				\u0002.Type = \u0002._Access.Type;
			}
		}

		// Token: 0x060030D3 RID: 12499 RVA: 0x000BA99C File Offset: 0x000B8B9C
		public virtual void \u0001(_ICurrentTaskExpression \u0002)
		{
			if (this.\u0001 != null)
			{
				this.\u0001(\u0002, MessageId.Err_OperatorNotSupported, new object[]
				{
					Scanner.GetTextOfOperator(Operator.__CurrentTask)
				});
				return;
			}
			ISignature signature = this.\u0001["__TaskSpecificInfo"];
			if (signature != null)
			{
				ITargetSettings targetSettings = this.\u0001.GetTargetSettings();
				if (global::\u0016.\u0004.MaxStackSize.GetIntValue(targetSettings) <= 0 && this.\u0001.ApplicationGuid != Guid.Empty)
				{
					this.\u0001(\u0002, MessageId.Err_UnknownMaxStackSize, Array.Empty<object>());
				}
				this._Scope.GlobalScope.FindSignature("__CurrentTaskDetection");
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(signature.Name);
				iuserdefType.SignatureId = signature.Id;
				_IPointerType type = global::\u0019.\u0003.\u0001(iuserdefType);
				\u0002.Type = type;
				_ISignature isignature = this.\u0001.GetSignatureById(this.\u0001.SignatureId) as _ISignature;
				if (isignature != null)
				{
					isignature.SetFlagInternal(SignatureFlagInternal.ContainsAccessToCurrentTask, true);
				}
				IScope5 u = this._Scope.CreateLocalScope(signature);
				this.\u0001(u, this.TypeExpected);
				\u0002._Base.Accept(this);
				this.\u0003();
			}
		}

		// Token: 0x060030D4 RID: 12500 RVA: 0x000BAAC0 File Offset: 0x000B8CC0
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x060030D5 RID: 12501 RVA: 0x000BAAC4 File Offset: 0x000B8CC4
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Low.Accept(this);
			\u0002._High.Accept(this);
			this.\u0003();
			if (\u0002._Low.Type == null || \u0002._High.Type == null)
			{
				return;
			}
			ICompiledType deRefType = \u0002._Low.Type.DeRefType;
			ICompiledType deRefType2 = \u0002._High.Type.DeRefType;
			int num = (deRefType.Size(this._Scope) > deRefType2.Size(this._Scope)) ? deRefType.Size(this._Scope) : deRefType2.Size(this._Scope);
			if (TypeTable.IsSigned(deRefType.Class) || TypeTable.IsSigned(deRefType2.Class))
			{
				switch (num)
				{
				case 1:
					\u0002.Type = TypeTable.Get(TypeClass.SInt);
					return;
				case 2:
					\u0002.Type = TypeTable.Get(TypeClass.Int);
					return;
				case 3:
					break;
				case 4:
					\u0002.Type = TypeTable.Get(TypeClass.DInt);
					return;
				default:
					if (num != 8)
					{
						return;
					}
					\u0002.Type = TypeTable.Get(TypeClass.LInt);
					return;
				}
			}
			else
			{
				switch (num)
				{
				case 1:
					\u0002.Type = TypeTable.Get(TypeClass.USInt);
					return;
				case 2:
					\u0002.Type = TypeTable.Get(TypeClass.UInt);
					return;
				case 3:
					break;
				case 4:
					\u0002.Type = TypeTable.Get(TypeClass.UDInt);
					return;
				default:
					if (num != 8)
					{
						return;
					}
					\u0002.Type = TypeTable.Get(TypeClass.ULInt);
					break;
				}
			}
		}

		// Token: 0x060030D6 RID: 12502 RVA: 0x000BAC2C File Offset: 0x000B8E2C
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
			this.\u0003();
		}

		// Token: 0x060030D7 RID: 12503 RVA: 0x000BAC84 File Offset: 0x000B8E84
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Switch.Accept(this);
			this.\u0003();
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x060030D8 RID: 12504 RVA: 0x000BAD10 File Offset: 0x000B8F10
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x060030D9 RID: 12505 RVA: 0x000BAD14 File Offset: 0x000B8F14
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x060030DA RID: 12506 RVA: 0x000BAD18 File Offset: 0x000B8F18
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x060030DB RID: 12507 RVA: 0x000BAD1C File Offset: 0x000B8F1C
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x060030DC RID: 12508 RVA: 0x000BAD20 File Offset: 0x000B8F20
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Number.Accept(this);
			\u0002._Value.Accept(this);
			this.\u0003();
			if (\u0002._Value.Type != null)
			{
				\u0002.Type = \u0002._Value.Type.DeRefType;
			}
		}

		// Token: 0x060030DD RID: 12509 RVA: 0x000BAD78 File Offset: 0x000B8F78
		public void \u0001(_IArrayInitialization \u0002)
		{
			IList<_IExpression> initValues = \u0002._InitValues;
			if (this.TypeExpected == null)
			{
				this.\u0001(\u0002, MessageId.Err_UnexpectedArrayInitialisation, Array.Empty<object>());
				return;
			}
			ICompiledType deRefType = this.TypeExpected.DeRefType;
			if (deRefType.Class != TypeClass.Array)
			{
				this.\u0001(\u0002, MessageId.Err_UnexpectedArrayInitialisation, Array.Empty<object>());
				return;
			}
			_IArrayType iarrayType = deRefType as _IArrayType;
			if (iarrayType._Base == null)
			{
				return;
			}
			this.\u0001(this._Scope, iarrayType._Base, AccessFlag.Read, null);
			for (int i = 0; i < initValues.Count; i++)
			{
				initValues[i].Accept(this);
				_IExprement iexprement = initValues[i];
				if (iexprement is IMultipleIndexInitialization)
				{
					iexprement = ((iexprement as IMultipleIndexInitialization).Value as _IExprement);
				}
				if (iarrayType._Base.DeRefType.Class == TypeClass.Array && !(iexprement is IArrayInitialization))
				{
					this.\u0001(iexprement, MessageId.Err_ArrayInitializationExpected, Array.Empty<object>());
				}
				else if (iarrayType._Base.DeRefType.Class == TypeClass.Userdef)
				{
					bool flag = iexprement is IStructureInitialization || iexprement is IVariableExpression || iexprement is ICompoAccessExpression;
					ISignature signature = (iarrayType._Base.DeRefType as _IUserdefType).GetSignature(this._Scope);
					if (signature != null && (signature.POUType == Operator.FunctionBlock || (signature.GetFlag(SignatureFlag.Structure) && !signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))) && !(iexprement is IStructureInitialization) && !flag)
					{
						this.\u0001(iexprement, MessageId.Err_StructureInitializationExpected, new object[]
						{
							iarrayType._Base
						});
					}
				}
			}
			this.\u0003();
			\u0002.Type = iarrayType;
		}

		// Token: 0x060030DE RID: 12510 RVA: 0x000BAF20 File Offset: 0x000B9120
		public void \u0001(_IStructureInitialization \u0002)
		{
			if (this.TypeExpected == null)
			{
				this.\u0001(\u0002, MessageId.Err_UnexpectedStructureInitialisation, Array.Empty<object>());
				return;
			}
			ICompiledType compiledType = this.TypeExpected;
			if (compiledType is _IImplicitReferenceType)
			{
				compiledType = ((_IImplicitReferenceType)compiledType).BaseType;
			}
			if (compiledType.Class != TypeClass.Userdef)
			{
				this.\u0001(\u0002, MessageId.Err_UnexpectedStructureInitialisation, Array.Empty<object>());
				return;
			}
			_IUserdefType iuserdefType = compiledType as _IUserdefType;
			if (iuserdefType.SignatureId == Helper.InvalidId)
			{
				TypeCompiler.\u0001(iuserdefType, this._Scope, this.\u0001, this.TypeChecker, this, iuserdefType.NameExpression.Position as _ISourcePosition);
			}
			ISignature signature = iuserdefType.GetSignature(this._Scope);
			if (signature == null)
			{
				return;
			}
			IEnumerable<_IAssignmentExpression> compoInits = \u0002._CompoInits;
			IScope5 scope = global::\u0004.\u0012.\u0001(this._Scope, this.\u0001, iuserdefType, false);
			if (scope == null)
			{
				scope = this._Scope;
			}
			AccessFlag u = AccessFlag.Read | AccessFlag.Write | AccessFlag.Type;
			AccessFlag u2 = AccessFlag.Read;
			foreach (_IAssignmentExpression iassignmentExpression in compoInits)
			{
				this.\u0001(scope, this.TopOfStack.\u0001, u, null);
				this.TopOfStack.\u0001 = false;
				iassignmentExpression._LValue.Accept(this);
				this.\u0003();
				IVariable variable = iassignmentExpression._LValue.GetVariable(scope);
				if (variable != null)
				{
					if (variable.GetFlag(VarFlag.Inout) && signature.POUType == Operator.FunctionBlock)
					{
						iassignmentExpression._LValue.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_StructInitOnlyInput, new object[]
						{
							variable.OrgName,
							signature.OrgName
						}), MessageId.Err_StructInitOnlyInput);
					}
					else if (((_IVariable)variable).IsProperty)
					{
						if (iassignmentExpression._RValue is _IArrayInitialization)
						{
							iassignmentExpression._RValue.AddError(global::\u0081.\u0002.Err_UnexpectedArrayInitialisation, MessageId.Err_UnexpectedArrayInitialisation);
						}
						else if (iassignmentExpression._RValue is _IStructureInitialization)
						{
							iassignmentExpression._RValue.AddError(global::\u0081.\u0002.Err_UnexpectedStructureInitialisation, MessageId.Err_UnexpectedStructureInitialisation);
						}
					}
				}
				this.\u0001(this._Scope, iassignmentExpression._LValue.Type, u2, null);
				iassignmentExpression._RValue.Accept(this);
				this.\u0003();
				iassignmentExpression.Type = iassignmentExpression._LValue.Type;
			}
			\u0002.Type = iuserdefType;
		}

		// Token: 0x060030DF RID: 12511 RVA: 0x000BB170 File Offset: 0x000B9370
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x060030E0 RID: 12512 RVA: 0x000BB174 File Offset: 0x000B9374
		public void \u0001(_IDefineReference \u0002)
		{
			\u0002.Value = this._Scope.IsDefined(\u0002.Define);
		}

		// Token: 0x060030E1 RID: 12513 RVA: 0x000BB190 File Offset: 0x000B9390
		public void \u0001(_IVariableReference \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002.InstancePath.Accept(this);
			this.\u0003();
			\u0002.Type = \u0002.InstancePath.Type;
		}

		// Token: 0x060030E2 RID: 12514 RVA: 0x000BB1BC File Offset: 0x000B93BC
		public void \u0001(_ITypeReference \u0002)
		{
			ISignature[] array = this._Scope.FindSignature(\u0002.InstancePath);
			ISignature u = null;
			if (array != null && array.Length != 0)
			{
				u = array[0];
			}
			this.\u0001(u);
		}

		// Token: 0x060030E3 RID: 12515 RVA: 0x000BB1F0 File Offset: 0x000B93F0
		public void \u0001(_IPouReference \u0002)
		{
			ISignature[] array = this._Scope.FindSignature(\u0002.InstancePath);
			ISignature signature = null;
			if (array != null && array.Length != 0)
			{
				signature = array[0];
			}
			if (\u0002.InstancePath is ICompoAccessExpression && signature == null)
			{
				_ICompoAccessExpression icompoAccessExpression = (_ICompoAccessExpression)\u0002.InstancePath;
				ISignature[] array2 = this._Scope.FindSignature(icompoAccessExpression.Left);
				if (array2 != null && array2.Any<ISignature>())
				{
					ISignature sign = array2.First<ISignature>();
					signature = (this._Scope.CreateLocalScope(sign) as _IScope).FindSignatureLocal(icompoAccessExpression.Right.ToString());
				}
			}
			if (signature != null && (signature.POUType == Operator.Program || signature.POUType == Operator.Function || signature.POUType == Operator.FunctionBlock || signature.POUType == Operator.Method || signature.POUType == Operator.Action || Operator.Interface == signature.POUType))
			{
				((_ISignature)this.TopOfStack.\u0001.LocalSignature).AddReferencer(signature.Id);
			}
			this.\u0001(signature);
		}

		// Token: 0x060030E4 RID: 12516 RVA: 0x000BB2E8 File Offset: 0x000B94E8
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x060030E5 RID: 12517 RVA: 0x000BB2EC File Offset: 0x000B94EC
		public void \u0001(_IResourceReference \u0002)
		{
			Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(this.\u0001.ApplicationGuid);
			string deviceName = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceName(deviceOfApplication);
			\u0002.Value = string.Equals(deviceName, \u0002.ResourceName, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x060030E6 RID: 12518 RVA: 0x000BB344 File Offset: 0x000B9544
		public void \u0001(_IDefinedExpression \u0002)
		{
			\u0002.ItemReference.Accept(this);
			\u0002.Value = (\u0002.ItemReference as _IItemReference).Value;
		}

		// Token: 0x060030E7 RID: 12519 RVA: 0x000BB368 File Offset: 0x000B9568
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			foreach (_IExpression iexpression in \u0002.Operands)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x060030E8 RID: 12520 RVA: 0x000BB3B4 File Offset: 0x000B95B4
		public virtual void \u0001(_IPragmaAssertion \u0002)
		{
			\u0002.Condition.Accept(this);
		}

		// Token: 0x060030E9 RID: 12521 RVA: 0x000BB3C4 File Offset: 0x000B95C4
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x060030EA RID: 12522 RVA: 0x000BB3C8 File Offset: 0x000B95C8
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
		}

		// Token: 0x060030EB RID: 12523 RVA: 0x000BB3CC File Offset: 0x000B95CC
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x060030EC RID: 12524 RVA: 0x000BB3D0 File Offset: 0x000B95D0
		protected void \u0001(_IPragmaIfStatement \u0002, bool \u0003)
		{
			_IStatement istatement = global::\u0011.\u0013.\u0001(\u0002, this._Scope, this.\u0001, \u0003);
			if (istatement == null)
			{
				this.\u0001.SetFlagInternal(InternalCompiledPOUFlags.RequiresRetypification, true);
				\u0002.IfThen.Accept(this);
				foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
				{
					ipragmaElseIf.Controlled.Accept(this);
				}
				if (\u0002.IfElse != null)
				{
					\u0002.IfElse.Accept(this);
					return;
				}
			}
			else
			{
				this.StatementReplacement = istatement;
				istatement.Accept(this);
			}
		}

		// Token: 0x060030ED RID: 12525 RVA: 0x000BB478 File Offset: 0x000B9678
		public virtual void \u0001(_IPragmaIfStatement \u0002)
		{
			\u0002.Condition.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				ipragmaElseIf.Condition.Accept(this);
			}
			this.\u0001(\u0002, true);
		}

		// Token: 0x060030EE RID: 12526 RVA: 0x000BB4DC File Offset: 0x000B96DC
		public void \u0001(_IDefineStatement \u0002)
		{
			if (\u0002.Define)
			{
				this._Scope.Define(\u0002.Ident, \u0002.Value);
				return;
			}
			this._Scope.Undefine(\u0002.Ident);
		}

		// Token: 0x060030EF RID: 12527 RVA: 0x000BB510 File Offset: 0x000B9710
		public void \u0001(_IXRefExpression \u0002)
		{
			if (\u0002.XRef != null)
			{
				\u0002.XRef.Accept(this);
			}
			if (\u0002.XRefFrom != null)
			{
				\u0002.XRefFrom.Accept(this);
			}
		}

		// Token: 0x060030F0 RID: 12528 RVA: 0x000BB53C File Offset: 0x000B973C
		public void \u0001(_IHasTypeExpression \u0002)
		{
			if (\u0002.Variable != null)
			{
				\u0002.Variable.Accept(this);
			}
			\u0002.ReferencedType = TypeCompiler.\u0001(\u0002.ReferencedType as _IType, this._Scope, this.\u0001, this.TypeChecker, this, \u0002.Position as _ISourcePosition);
			\u0002.Type = \u0002.ReferencedType;
		}

		// Token: 0x060030F1 RID: 12529 RVA: 0x000BB5A0 File Offset: 0x000B97A0
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
			\u0002.Value = false;
			\u0002.ReferencedType = TypeCompiler.\u0001(\u0002.ReferencedType as _IType, this._Scope, this.\u0001, this.TypeChecker, this, \u0002.Position as _ISourcePosition);
			\u0002.Type = \u0002.ReferencedType;
		}

		// Token: 0x060030F2 RID: 12530 RVA: 0x000BB5F4 File Offset: 0x000B97F4
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x060030F3 RID: 12531 RVA: 0x000BB604 File Offset: 0x000B9804
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x060030F4 RID: 12532 RVA: 0x000BB608 File Offset: 0x000B9808
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
			_IHasConstantValueExpression2 ihasConstantValueExpression = \u0002 as _IHasConstantValueExpression2;
			if (ihasConstantValueExpression != null)
			{
				this.\u0001(ihasConstantValueExpression.Constant);
				this.\u0001(ihasConstantValueExpression.ConstantValueExpression);
			}
		}

		// Token: 0x060030F5 RID: 12533 RVA: 0x000BB638 File Offset: 0x000B9838
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
			this.\u0001(\u0002.Constant);
			if (\u0002.Type != null)
			{
				\u0002.Type = TypeCompiler.\u0001(\u0002.Type as _IType, this._Scope, this.\u0001, this.TypeChecker, this, \u0002.Position as _ISourcePosition);
			}
			_IVariable ivariable = \u0002._Constant.GetVariable(this._Scope) as _IVariable;
			\u0002.Value = (ivariable != null && ivariable.GetFlag(VarFlag.ReplacedConstant) == \u0002._ConstantTypeReplaced);
			\u0002.ValueStillUndecided = false;
		}

		// Token: 0x060030F6 RID: 12534 RVA: 0x000BB6C8 File Offset: 0x000B98C8
		protected virtual void \u0001(_ISignature \u0002)
		{
			int nId = this.\u0001();
			\u0002.AddCaller(nId);
		}

		// Token: 0x060030F7 RID: 12535 RVA: 0x000BB6E4 File Offset: 0x000B98E4
		protected virtual void \u0002(_ISignature \u0002, int \u0003)
		{
			\u0002.AddCallee(\u0003, false);
		}

		// Token: 0x060030F8 RID: 12536 RVA: 0x000BB6F0 File Offset: 0x000B98F0
		protected virtual void \u0001(_ISignature \u0002, int \u0003)
		{
			\u0002.AddDeclarer(\u0003);
		}

		// Token: 0x060030F9 RID: 12537 RVA: 0x000BB6FC File Offset: 0x000B98FC
		protected virtual void \u0001(_ISignature \u0002, int \u0003, bool \u0004)
		{
			\u0002.AddCallee(\u0003, \u0004);
		}

		// Token: 0x060030FA RID: 12538 RVA: 0x000BB708 File Offset: 0x000B9908
		internal void \u0002(_ISignature \u0002)
		{
			if (this.\u0001 != null)
			{
				this.\u0001(\u0002);
				_ISignature isignature = (_ISignature)this._Scope[this.\u0001.SignatureId];
				if (isignature != null)
				{
					this.\u0002(isignature, \u0002.Id);
				}
			}
			if (this.\u0001 == null)
			{
				if (this._Scope.MethodSignature != null)
				{
					this.\u0002((_ISignature)this._Scope.MethodSignature, \u0002.Id);
					return;
				}
				if (this._Scope.LocalSignature != null)
				{
					this.\u0002((_ISignature)this._Scope.LocalSignature, \u0002.Id);
				}
			}
		}

		// Token: 0x060030FB RID: 12539 RVA: 0x000BB7AC File Offset: 0x000B99AC
		private void \u0003(_ISignature \u0002)
		{
			this.\u0001(\u0002, SignatureFlag.TopLevel);
			_ICompiledPOU icompiledPOU = this.\u0001.GetCompiledPOUById(\u0002.Id) as _ICompiledPOU;
			if (icompiledPOU != null)
			{
				this.\u0001(icompiledPOU, CompiledPOUFlags.TopLevel);
			}
		}

		// Token: 0x060030FC RID: 12540 RVA: 0x000BB7E8 File Offset: 0x000B99E8
		public void \u0001(IStatement \u0002, out IEnumerable<IMessage> \u0003)
		{
			this.\u0001(\u0002);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			_IStatement istatement = \u0002 as _IStatement;
			if (istatement != null)
			{
				istatement.Accept(errorVisitor);
			}
			IStatementTraverser ivisit = new global::\u0013.\u0001(new \u0084.\u0008());
			istatement.Accept(ivisit);
			\u0003 = errorVisitor.Messages;
		}

		// Token: 0x060030FD RID: 12541 RVA: 0x000BB830 File Offset: 0x000B9A30
		public void \u0002(IStatement \u0002, out IEnumerable<IMessage> \u0003)
		{
			this.\u0001(\u0002);
			IStatementTraverser ivisit = new global::\u0013.\u0001(new \u0084.\u0008());
			_IStatement istatement = \u0002 as _IStatement;
			istatement.Accept(ivisit);
			TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(this._Scope, this.\u0001, false, this.\u0001, false);
			istatement.Accept(ivisit2);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			if (istatement != null)
			{
				istatement.Accept(errorVisitor);
			}
			\u0003 = errorVisitor.Messages;
		}

		// Token: 0x060030FE RID: 12542 RVA: 0x000BB898 File Offset: 0x000B9A98
		public void \u0001(IExpression \u0002, out IEnumerable<IMessage> \u0003)
		{
			this.\u0001(\u0002);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			_IExpression iexpression = \u0002 as _IExpression;
			if (iexpression != null)
			{
				iexpression.Accept(errorVisitor);
			}
			\u0003 = errorVisitor.Messages;
		}

		// Token: 0x060030FF RID: 12543 RVA: 0x000BB8CC File Offset: 0x000B9ACC
		public void \u0001(IExpression \u0002, IPredefinedCalleeTypeProvider \u0003)
		{
			this.\u0001 = \u0003;
			_IExpression iexpression = (_IExpression)\u0002;
			this.\u0001(iexpression);
			this.\u0001 = null;
			ConstantFolder.ReplaceInnerFoldedConstants(iexpression, this.\u0001, this._Scope, this.\u0001);
			TypeCheckerVisitor ivisit = new TypeCheckerVisitor(this._Scope, this.\u0001, false, this.\u0001, false);
			iexpression.Accept(ivisit);
		}

		// Token: 0x06003100 RID: 12544 RVA: 0x000BB930 File Offset: 0x000B9B30
		public void \u0002(IExpression \u0002, out IEnumerable<IMessage> \u0003)
		{
			this.\u0001(\u0002);
			TypeCheckerVisitor ivisit = new TypeCheckerVisitor(this._Scope, this.\u0001, false, this.\u0001, false);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			_IExpression iexpression = \u0002 as _IExpression;
			if (iexpression != null)
			{
				iexpression.Accept(ivisit);
				iexpression.Accept(errorVisitor);
			}
			\u0003 = errorVisitor.Messages;
		}

		// Token: 0x06003101 RID: 12545 RVA: 0x000BB984 File Offset: 0x000B9B84
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			this.\u0002();
			\u0002._Left.Accept(this);
			bool compiledType = \u0002._Left._CompiledType != null;
			this.\u0003();
			if (compiledType)
			{
				\u0002._CompiledType = TypeTable.GetDirectVariableSizeType(\u0002.PartSize);
				return;
			}
			\u0002._CompiledType = null;
		}

		// Token: 0x06003102 RID: 12546 RVA: 0x000BB9C4 File Offset: 0x000B9BC4
		public void \u0001(_ITryCatchStatement \u0002)
		{
			this.\u0001.SetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch, true);
			ISignature[] array = this._Scope.SystemScope.FindSignature("trycatch");
			Debug.\u0001(array.Length == 1);
			this.\u0001((_ISignature)array[0]);
			if (this._Scope.MethodSignature != null)
			{
				this.\u0002((_ISignature)this._Scope.MethodSignature, array[0].Id);
			}
			else if (this._Scope.LocalSignature != null)
			{
				this.\u0002((_ISignature)this._Scope.LocalSignature, array[0].Id);
			}
			array = this._Scope.SystemScope.FindSignature("ExceptionFlags");
			Debug.\u0001(array.Length == 1);
			this.\u0001((_ISignature)array[0]);
			if (this._Scope.MethodSignature != null)
			{
				this.\u0002((_ISignature)this._Scope.MethodSignature, array[0].Id);
			}
			else if (this._Scope.LocalSignature != null)
			{
				this.\u0002((_ISignature)this._Scope.LocalSignature, array[0].Id);
			}
			\u0002.DefaultTraverse(this);
		}

		// Token: 0x06003103 RID: 12547 RVA: 0x000BBAF4 File Offset: 0x000B9CF4
		public void \u0003(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DWord;
		}

		// Token: 0x06003104 RID: 12548 RVA: 0x000BBB04 File Offset: 0x000B9D04
		public void \u0004(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count == 0)
			{
				\u0002.Type = TypeTable.Bool;
				return;
			}
			\u0002.Type = operandsList[0].Type.DeRefType;
		}

		// Token: 0x06003105 RID: 12549 RVA: 0x000BBB44 File Offset: 0x000B9D44
		public void \u0005(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count == 0)
			{
				\u0002.Type = TypeTable.DInt;
				return;
			}
			\u0002.Type = operandsList[0].Type.DeRefType;
		}

		// Token: 0x06003106 RID: 12550 RVA: 0x000BBB84 File Offset: 0x000B9D84
		public void \u0006(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DInt;
		}

		// Token: 0x06003107 RID: 12551 RVA: 0x000BBB94 File Offset: 0x000B9D94
		public void \u0007(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Int;
		}

		// Token: 0x06003108 RID: 12552 RVA: 0x000BBBA4 File Offset: 0x000B9DA4
		public void \u0008(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06003109 RID: 12553 RVA: 0x000BBBB4 File Offset: 0x000B9DB4
		public void \u000E(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
			Debug.\u0001(this._Scope.SystemScope.FindSignature("throwex").Length == 1);
		}

		// Token: 0x0600310A RID: 12554 RVA: 0x000BBBE0 File Offset: 0x000B9DE0
		public void \u000F(_IOperatorExpression \u0002)
		{
			if (\u0002._OperandsList.Count == 3)
			{
				\u0002.Type = TypeTable.Bool;
				return;
			}
			\u0002.Type = TypeTable.Int;
		}

		// Token: 0x0600310B RID: 12555 RVA: 0x000BBC08 File Offset: 0x000B9E08
		public void \u0010(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DWord;
		}

		// Token: 0x0600310C RID: 12556 RVA: 0x000BBC18 File Offset: 0x000B9E18
		public void \u0011(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x0600310D RID: 12557 RVA: 0x000BBC28 File Offset: 0x000B9E28
		public void \u0012(_IOperatorExpression \u0002)
		{
			\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.DWord);
		}

		// Token: 0x0600310E RID: 12558 RVA: 0x000BBC3C File Offset: 0x000B9E3C
		public void \u0013(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.ULInt;
		}

		// Token: 0x0600310F RID: 12559 RVA: 0x000BBC4C File Offset: 0x000B9E4C
		public void \u0014(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.UDInt;
		}

		// Token: 0x06003110 RID: 12560 RVA: 0x000BBC5C File Offset: 0x000B9E5C
		public void \u0015(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DInt;
		}

		// Token: 0x06003111 RID: 12561 RVA: 0x000BBC6C File Offset: 0x000B9E6C
		public void \u0016(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DWord;
		}

		// Token: 0x06003112 RID: 12562 RVA: 0x000BBC7C File Offset: 0x000B9E7C
		public void \u0017(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06003113 RID: 12563 RVA: 0x000BBC8C File Offset: 0x000B9E8C
		public void \u0018(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			\u0002.Type = TypeTable.Bool;
			this.\u0001(this._Scope, this.TypeExpected, AccessFlag.Write, null);
			operandsList[0].Accept(this);
			this.\u0003();
			_IType itype = operandsList[0].Type as _IType;
			if (itype != null && itype.BaseType.Class != TypeClass.Userdef)
			{
				return;
			}
			_IUserdefType iuserdefType = itype.BaseType as _IUserdefType;
			ISignature signature = (iuserdefType != null) ? iuserdefType.GetSignature(this._Scope) : null;
			ISignature signature2 = (signature != null) ? signature.GetSubSignature("FB_EXIT") : null;
			if (signature2 == null)
			{
				return;
			}
			this.\u0001(signature2 as _ISignature);
		}

		// Token: 0x06003114 RID: 12564 RVA: 0x000BBD34 File Offset: 0x000B9F34
		public void \u0019(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			\u0002.Type = TypeTable.Bool;
			this.\u0001(this._Scope, this.TypeExpected, AccessFlag.Write, null);
			if (2 == operandsList.Count)
			{
				operandsList[1].Accept(this);
			}
			this.\u0003();
			IList<ISignature> list = this._Scope.SystemScope["__CheckedPointerCast"];
			Debug.\u0001(list != null && list.Count == 1);
			ISignature signature = list[0];
			if (signature != null)
			{
				if (this.\u0001 != null && this.\u0001.SignatureId != signature.Id)
				{
					this.\u0001((_ISignature)signature);
				}
				if (this._Scope.MethodSignature != null)
				{
					this.\u0002((_ISignature)this._Scope.MethodSignature, signature.Id);
					return;
				}
				if (this._Scope.LocalSignature != null && this._Scope.LocalSignature.Id != signature.Id)
				{
					this.\u0002((_ISignature)this._Scope.LocalSignature, signature.Id);
				}
			}
		}

		// Token: 0x06003115 RID: 12565 RVA: 0x000BBE50 File Offset: 0x000BA050
		public void \u001A(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			\u0002.Type = TypeTable.Bool;
			this.\u0001(this._Scope, this.TypeExpected, AccessFlag.Write, null);
			operandsList[1].Accept(this);
			this.\u0003();
			IList<ISignature> list = this._Scope.SystemScope["__CheckedInterfaceCast"];
			Debug.\u0001(list != null && list.Count == 1);
			ISignature signature = list[0];
			if (signature != null)
			{
				if (this.\u0001 != null && this.\u0001.SignatureId != signature.Id)
				{
					this.\u0001((_ISignature)signature);
				}
				if (this._Scope.MethodSignature != null)
				{
					this.\u0002((_ISignature)this._Scope.MethodSignature, signature.Id);
					return;
				}
				if (this._Scope.LocalSignature != null && this._Scope.LocalSignature.Id != signature.Id)
				{
					this.\u0002((_ISignature)this._Scope.LocalSignature, signature.Id);
				}
			}
		}

		// Token: 0x06003116 RID: 12566 RVA: 0x000BBF60 File Offset: 0x000BA160
		public void \u001B(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
			_IVariable ivariable = \u0002._OperandsList[0].GetVariable(this._Scope) as _IVariable;
			if (ivariable != null && ivariable.IsProperty)
			{
				ISignature signatureEx = \u0002._OperandsList[0].GetSignatureEx(this._Scope);
				if (signatureEx != null)
				{
					IScope5 scope = this._Scope.CreateLocalScope(signatureEx as _ISignature);
					_ISignature isignature = scope.FindSignatureLocal(IdentifierConstants.CreateGetterName(ivariable.VersionedName)) as _ISignature;
					_ISignature isignature2 = scope.FindSignatureLocal(IdentifierConstants.CreateSetterName(ivariable.VersionedName)) as _ISignature;
					if (isignature != null)
					{
						this.\u0003(isignature);
					}
					if (isignature2 != null)
					{
						this.\u0003(isignature2);
					}
				}
			}
		}

		// Token: 0x06003117 RID: 12567 RVA: 0x000BC010 File Offset: 0x000BA210
		public void \u001C(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList[0]._CompiledType is _IUserdefType)
			{
				ISignature signature = (operandsList[0]._CompiledType as _IUserdefType).GetSignature(this._Scope);
				if (signature != null && signature.Outputs.Length != 0)
				{
					\u0002.Type = signature.Outputs[0].CompiledType;
				}
			}
		}

		// Token: 0x06003118 RID: 12568 RVA: 0x000BC074 File Offset: 0x000BA274
		public void \u001D(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Int;
			ICompiledType compiledType = this.TypeExpected;
			if (compiledType != null && compiledType.Class == TypeClass.Userdef)
			{
				_IUserdefType iuserdefType = compiledType as _IUserdefType;
				if (!(((iuserdefType != null) ? iuserdefType.NameExpression.ToString() : null) != "__SYSTEM.VAR_INFO"))
				{
					\u0002.Type = compiledType;
					return;
				}
			}
			_ISystemScopeExpression isystemScopeExpression = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001("TYPE_CLASS"));
			isystemScopeExpression.Accept(this);
			_IUserdefType iuserdefType2 = isystemScopeExpression.Type as _IUserdefType;
			int u = (iuserdefType2 != null) ? iuserdefType2.SignatureId : -1;
			_IEnumType ienumType = global::\u0019.\u0003.\u0001("TYPE_CLASS", u);
			ienumType._Base = TypeTable.Byte;
			\u0002.Type = ienumType;
		}

		// Token: 0x06003119 RID: 12569 RVA: 0x000BC118 File Offset: 0x000BA318
		public void \u001E(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x0600311A RID: 12570 RVA: 0x000BC128 File Offset: 0x000BA328
		public void \u001F(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
			ICompiledType compiledType = this.TypeExpected;
			if (compiledType == null || compiledType.Class != TypeClass.Userdef || (compiledType as _IUserdefType).NameExpression.ToString() != "__SYSTEM.VAR_INFO")
			{
				\u0002.Type = global::\u0019.\u0003.\u0001("__SYSTEM.VAR_INFO");
				return;
			}
			\u0002.Type = compiledType;
		}

		// Token: 0x0600311B RID: 12571 RVA: 0x000BC188 File Offset: 0x000BA388
		public void \u007F(_IOperatorExpression \u0002)
		{
			this.\u0080(\u0002);
		}

		// Token: 0x0600311C RID: 12572 RVA: 0x000BC194 File Offset: 0x000BA394
		public void \u0080(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count > 0)
			{
				if (\u0002.Code == Operator.__RefAdr && operandsList[0].Type.Class == TypeClass.Reference)
				{
					\u0002.Type = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(operandsList[0].Type as _IType));
				}
				else
				{
					\u0002.Type = global::\u0019.\u0003.\u0001(operandsList[0].Type as _IType);
				}
			}
			else
			{
				\u0002.Type = TypeTable.DInt;
			}
			if (1 == operandsList.Count)
			{
				_ISignature isignature = operandsList[0].GetSignature(this._Scope) as _ISignature;
				if (isignature != null)
				{
					this.\u0003(isignature);
				}
			}
		}

		// Token: 0x0600311D RID: 12573 RVA: 0x000BC24C File Offset: 0x000BA44C
		public void \u0081(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			_ILiteralValue iliteralValue = \u0002.Literal(this._Scope) as _ILiteralValue;
			if (iliteralValue != null)
			{
				bool flag;
				long anyLong = iliteralValue.GetAnyLong(out flag);
				if (anyLong > (long)((ulong)-1))
				{
					\u0002.Type = TypeTable.ULInt;
				}
				else if (anyLong > 65535L)
				{
					\u0002.Type = TypeTable.UDInt;
				}
				else if (anyLong > 255L)
				{
					\u0002.Type = TypeTable.UInt;
				}
				else
				{
					\u0002.Type = TypeTable.USInt;
				}
			}
			else if (this.TypeExpected != null && TypeTable.IsConcreteType(this.TypeExpected.Class) && TypeTable.IsInteger(this.TypeExpected.Class))
			{
				\u0002.Type = this.TypeExpected;
			}
			else if (this.TypeExpected != null && this.TypeExpected.Class == TypeClass.Pointer)
			{
				if (TypeTable.GetSize(TypeClass.Pointer, this._Scope) == 2)
				{
					\u0002.Type = TypeTable.UInt;
				}
				else
				{
					\u0002.Type = TypeTable.UDInt;
				}
			}
			else if (this.TypeExpected != null && TypeTable.IsReal(this.TypeExpected.Class))
			{
				\u0002.Type = TypeTable.UDInt;
			}
			else
			{
				\u0002.Type = TypeTable.UInt;
			}
			this.\u0001(operandsList);
		}

		// Token: 0x0600311E RID: 12574 RVA: 0x000BC38C File Offset: 0x000BA58C
		private void \u0001(IList<_IExpression> \u0002)
		{
			_ISignature isignature = \u0002[0].GetSignature(this._Scope) as _ISignature;
			if (isignature == null && \u0002[0] is IDeRefAccessExpression)
			{
				_IUserdefType iuserdefType = \u0002[0].Type as _IUserdefType;
				if (iuserdefType != null)
				{
					isignature = (iuserdefType.GetSignature(this._Scope) as _ISignature);
				}
			}
			if (isignature != null)
			{
				this.\u0001(isignature, true);
			}
		}

		// Token: 0x0600311F RID: 12575 RVA: 0x000BC3F4 File Offset: 0x000BA5F4
		private void \u0001(_ISignature \u0002, bool \u0003)
		{
			int num = this.\u0001();
			if (num != -1 && num != \u0002.Id)
			{
				this.\u0001(\u0002);
				if (\u0003)
				{
					this.\u0001(\u0002, num);
				}
			}
			_ISignature isignature = this._Scope.MethodSignature as _ISignature;
			if (isignature != null)
			{
				this.\u0002(isignature, \u0002.Id);
				return;
			}
			_ISignature isignature2 = this._Scope.LocalSignature as _ISignature;
			if (isignature2 != null && this._Scope.LocalSignature.Id != \u0002.Id)
			{
				this.\u0002(isignature2, \u0002.Id);
			}
		}

		// Token: 0x06003120 RID: 12576 RVA: 0x000BC484 File Offset: 0x000BA684
		public void \u0082(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.ResolveUXIntType(this.\u0001.PointerSize);
			this.\u0001(\u0002._OperandsList);
		}

		// Token: 0x06003121 RID: 12577 RVA: 0x000BC4A8 File Offset: 0x000BA6A8
		public void \u0083(_IOperatorExpression \u0002)
		{
			ISignature localSignature = this._Scope.LocalSignature;
			if (localSignature != null && (localSignature.POUType == Operator.FunctionBlock || localSignature.GetFlag(SignatureFlag.Structure)))
			{
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(localSignature.Name);
				iuserdefType.SignatureId = localSignature.Id;
				_IPointerType type = global::\u0019.\u0003.\u0001(iuserdefType);
				\u0002.Type = type;
			}
		}

		// Token: 0x06003122 RID: 12578 RVA: 0x000BC4FC File Offset: 0x000BA6FC
		public void \u0084(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			\u0002.Type = TypeTable.Int;
			_ISignature isignature = operandsList[0].GetSignature(this._Scope) as _ISignature;
			if (isignature != null)
			{
				this.\u0001(isignature, false);
			}
		}

		// Token: 0x06003123 RID: 12579 RVA: 0x000BC53C File Offset: 0x000BA73C
		public void \u0086(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			\u0002.Type = TypeTable.Bool;
			_ISignature isignature = operandsList[0].GetSignature(this._Scope) as _ISignature;
			if (isignature != null)
			{
				this.\u0001(isignature, false);
			}
		}

		// Token: 0x06003124 RID: 12580 RVA: 0x000BC57C File Offset: 0x000BA77C
		public void \u0087(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06003125 RID: 12581 RVA: 0x000BC58C File Offset: 0x000BA78C
		public void \u0088(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DInt;
			IVariable variable = \u0002._OperandsList[0].GetVariable(this._Scope);
			if (variable != null && variable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
			{
				string stName = string.Format("{0}__Array__Info", \u0002._OperandsList[0]);
				ISignature signature;
				_IVariable ivariable = this._Scope.FindVariableLocal(stName, out signature) as _IVariable;
				if (ivariable != null)
				{
					this.\u0001(ivariable, (_ISignature)signature, this.\u0001());
				}
			}
		}

		// Token: 0x06003126 RID: 12582 RVA: 0x000BC610 File Offset: 0x000BA810
		public void \u0089(_IOperatorExpression \u0002)
		{
			\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.Byte);
		}

		// Token: 0x06003127 RID: 12583 RVA: 0x000BC624 File Offset: 0x000BA824
		public void \u008A(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Time;
		}

		// Token: 0x06003128 RID: 12584 RVA: 0x000BC634 File Offset: 0x000BA834
		public void \u008B(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.LTime;
		}

		// Token: 0x06003129 RID: 12585 RVA: 0x000BC644 File Offset: 0x000BA844
		public void \u008C(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count == 0)
			{
				\u0002.Type = TypeTable.Bool;
				return;
			}
			if (operandsList[0] is ILiteralExpression && !TypeTable.IsConcreteType((operandsList[0] as ILiteralExpression).ConstantType))
			{
				ICompiledType compiledType = TypeTable.UDInt;
				if (this.TypeExpected != null && TypeTable.IsConcreteType(this.TypeExpected.Class) && TypeTable.IsInteger(this.TypeExpected.Class))
				{
					compiledType = this.TypeExpected;
				}
				if (TypeTable.IsInteger(compiledType.Class) && (operandsList[0] as _ILiteralExpression).ConstantType == TypeClass.AnyInt)
				{
					(operandsList[0] as _ILiteralExpression).ConstantType = compiledType.Class;
					operandsList[0].Type = compiledType;
				}
				\u0002.Type = compiledType;
			}
			else
			{
				\u0002.Type = operandsList[0].Type.DeRefType;
			}
			if (!TypeTable.IsInteger(\u0002.Type.Class) && \u0002.Type.Class != TypeClass.Pointer)
			{
				this.\u0001(operandsList[0], MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					\u0002.Code,
					\u0002.Type
				});
			}
		}

		// Token: 0x0600312A RID: 12586 RVA: 0x000BC784 File Offset: 0x000BA984
		public void \u008D(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DInt;
		}

		// Token: 0x0600312B RID: 12587 RVA: 0x000BC794 File Offset: 0x000BA994
		public void \u008E(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x0600312C RID: 12588 RVA: 0x000BC7A4 File Offset: 0x000BA9A4
		public void \u008F(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x0600312D RID: 12589 RVA: 0x000BC7B4 File Offset: 0x000BA9B4
		public void \u0090(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.UDInt;
			_ISignature isignature = null;
			if (this.\u0001 != null)
			{
				isignature = (this._Scope[this.\u0001.SignatureId] as _ISignature);
				if (isignature.ParentSignatureId != Helper.InvalidId && isignature.Name == "__MAIN")
				{
					isignature = (this._Scope[isignature.ParentSignatureId] as _ISignature);
				}
			}
			else if (this.\u0001 != null)
			{
				isignature = this.\u0001;
			}
			bool flag = true;
			try
			{
				_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(isignature) as _IPreCompileContext;
				if (ipreCompileContext != null)
				{
					if (string.IsNullOrEmpty(ipreCompileContext.LibraryId))
					{
						flag = false;
					}
					else
					{
						string u = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() >= new Version(3, 5, 17, 0)) ? "3SLicense, * (CODESYS)" : "3SLicense, * (3S - Smart Software Solutions GmbH)";
						_IPreCompileContext ipreCompileContext2 = global::\u0014.\u0002.\u0001(this.\u0001, ipreCompileContext, u);
						if (ipreCompileContext2 != null)
						{
							IExpression qne = global::\u0011.\u0006.\u0001(Helper.\u0001(this.\u0001, ipreCompileContext2, ipreCompileContext, new Dictionary<_IPreCompileContext, _IPreCompileContext>()));
							IScope5 scope = this._Scope.FindScope(qne) as IScope5;
							_ISignature isignature2 = scope.FindFirstSignature("SysTargetGetId") as _ISignature;
							if (isignature2 != null)
							{
								this.\u0001(isignature2, false);
							}
							_ISignature isignature3 = scope.FindFirstSignature("SysTargetGetType") as _ISignature;
							if (isignature3 != null)
							{
								this.\u0001(isignature3, false);
							}
							ISignature[] array = null;
							IVariable[] array2 = scope.FindVariableGlobal("g_olm", out array);
							if (isignature3 != null && array2 != null && array2.Length != 0)
							{
								flag = false;
							}
						}
					}
				}
			}
			catch
			{
				flag = true;
			}
			finally
			{
				if (flag)
				{
					this.\u0001(\u0002, MessageId.Err_CheckLicenseNeedsSysTarget, Array.Empty<object>());
				}
			}
		}

		// Token: 0x0600312E RID: 12590 RVA: 0x000BC988 File Offset: 0x000BAB88
		public void \u0091(_IOperatorExpression \u0002)
		{
			this.\u0090(\u0002);
			\u0002.Type = TypeTable.Int;
		}

		// Token: 0x0600312F RID: 12591 RVA: 0x000BC99C File Offset: 0x000BAB9C
		public void \u0092(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			ITargetSettings targetSettings = this.\u0001.GetTargetSettings();
			if (global::\u0016.\u0004.LRealDataType.GetBoolValue(targetSettings))
			{
				\u0002.Type = TypeTable.Real;
				if (this.\u0001.TreatLRealAsReal)
				{
					\u0002.Type = TypeTable.Real;
					return;
				}
				using (IEnumerator<_IExpression> enumerator = operandsList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_IExpression iexpression = enumerator.Current;
						if (iexpression.Type == null || iexpression.Type.DeRefType.Class != TypeClass.Real)
						{
							\u0002.Type = TypeTable.LReal;
							break;
						}
					}
					return;
				}
			}
			\u0002.Type = TypeTable.Real;
		}

		// Token: 0x06003130 RID: 12592 RVA: 0x000BCA58 File Offset: 0x000BAC58
		public void \u0093(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Operator code = \u0002.Code;
			if (code - Operator.Eq <= 1 || code - Operator.Equal <= 1)
			{
				\u0002.Type = TypeTable.Bool;
				if (operandsList.Count == 2 && global::\u0006.\u0011.\u0001(operandsList[0].Type, this._Scope) && global::\u0006.\u0011.\u0001(operandsList[1].Type, this._Scope))
				{
					IList<ISignature> list = this._Scope.SystemScope["__CompareInterfaces"];
					Debug.\u0001(list != null && list.Count == 1);
					_ISignature isignature = ((list != null) ? list[0] : null) as _ISignature;
					if (isignature != null)
					{
						this.\u0001(isignature, false);
						return;
					}
				}
			}
			else
			{
				\u0002.Type = TypeTable.Bool;
			}
		}

		// Token: 0x06003131 RID: 12593 RVA: 0x000BCB28 File Offset: 0x000BAD28
		public void \u0094(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			bool u = false;
			Operator code = \u0002.Code;
			if (code <= Operator.Not)
			{
				if (code - Operator.And > 5)
				{
					if (code != Operator.Not)
					{
						goto IL_1AF;
					}
					if (operandsList.Count != 0 && operandsList[0] is ILiteralExpression && !TypeTable.IsConcreteType((operandsList[0] as ILiteralExpression).ConstantType) && this.TypeExpected != null && TypeTable.IsConcreteType(this.TypeExpected.Class))
					{
						(operandsList[0] as _ILiteralExpression).ConstantType = this.TypeExpected.Class;
						\u0002.Type = this.TypeExpected;
						return;
					}
				}
			}
			else if (code - Operator.Ampersand > 1 && code - Operator.And_Then > 1)
			{
				goto IL_1AF;
			}
			u = true;
			bool flag = true;
			bool flag2 = false;
			ICompiledType compiledType = null;
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression = operandsList[i];
				if (iexpression.Type.DeRefType.Class == TypeClass.Bool || iexpression.Type.DeRefType.Class == TypeClass.Bit || iexpression.Type.DeRefType.Class == TypeClass.BitConst)
				{
					flag2 = true;
					if (compiledType == null || iexpression.Type.DeRefType.Class == TypeClass.Bool)
					{
						compiledType = iexpression.Type.DeRefType;
					}
				}
				if (iexpression.Type.DeRefType.Class != TypeClass.Bool && iexpression.Type.DeRefType.Class != TypeClass.Bit && iexpression.Type.DeRefType.Class != TypeClass.BitConst)
				{
					flag = false;
					break;
				}
			}
			if (flag && flag2)
			{
				if (compiledType == null)
				{
					\u0002.Type = TypeTable.Get(TypeClass.Bool);
					return;
				}
				\u0002.Type = compiledType;
				return;
			}
			IL_1AF:
			this.\u0001(\u0002, 0, u, operandsList);
		}

		// Token: 0x06003132 RID: 12594 RVA: 0x000BCCF0 File Offset: 0x000BAEF0
		public void \u0095(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			ICompiledType type = null;
			if (TimeOperationTypeCalculator.\u0001(\u0002, this._Scope as ICommonScope, out type))
			{
				\u0002.Type = type;
				return;
			}
			this.\u0001(\u0002, 0, false, operandsList);
		}

		// Token: 0x06003133 RID: 12595 RVA: 0x000BCD30 File Offset: 0x000BAF30
		public void \u0096(_IOperatorExpression \u0002)
		{
			_ICompiledPOU u = this.\u0001;
			if (u != null)
			{
				u.SetFlagInternal(InternalCompiledPOUFlags.ContainsVectorOperation, true);
			}
			IList<_IExpression> operandsList = \u0002._OperandsList;
			switch (\u0002.Code)
			{
			case Operator.__vcAdd:
			case Operator.__vcSub:
			case Operator.__vcMul:
			case Operator.__vcDiv:
			case Operator.__vcMin:
			case Operator.__vcMax:
				if (operandsList[0].Type.DeRefType.Class == TypeClass.__Vector)
				{
					\u0002.Type = operandsList[0].Type.DeRefType;
					return;
				}
				if (operandsList[1].Type.DeRefType.Class == TypeClass.__Vector)
				{
					\u0002.Type = operandsList[1].Type.DeRefType;
					return;
				}
				\u0002.Type = operandsList[0].Type.DeRefType;
				return;
			case Operator.__vcDot:
				if (operandsList[0].Type.DeRefType.Class == TypeClass.__Vector && operandsList[0].Type.DeRefType.Class == TypeClass.__Vector)
				{
					\u0002.Type = operandsList[0].Type.DeRefType.BaseType;
					return;
				}
				\u0002.Type = TypeTable.Get(TypeClass.LReal);
				return;
			case Operator.__vcSqrt:
				if (operandsList[0].Type.DeRefType.Class != TypeClass.__Vector)
				{
					\u0002.Type = operandsList[0].Type.DeRefType;
					return;
				}
				\u0002.Type = operandsList[0].Type.DeRefType;
				return;
			case Operator.__vcSetReal:
				\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.Get(Operator.Real), global::\u0019.\u0003.\u0001((long)operandsList.Count, TypeClass.Int));
				return;
			case Operator.__vcSetLReal:
				\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.Get(Operator.LReal), global::\u0019.\u0003.\u0001((long)operandsList.Count, TypeClass.Int));
				return;
			case Operator.__vcLoadReal:
				\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.Get(TypeClass.Real), operandsList[0]);
				return;
			case Operator.__vcLoadLReal:
				\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.Get(TypeClass.LReal), operandsList[0]);
				return;
			case Operator.__vcStore:
				if (operandsList[1].Type.DeRefType.Class != TypeClass.__Vector)
				{
					\u0002.Type = operandsList[1].Type;
					return;
				}
				\u0002.Type = operandsList[1].Type;
				return;
			default:
				return;
			}
		}

		// Token: 0x06003134 RID: 12596 RVA: 0x000BCF7C File Offset: 0x000BB17C
		internal void \u0001(_IOperatorExpression \u0002, int \u0003, bool \u0004, IList<_IExpression> \u0005)
		{
			if (\u0002.Type == null)
			{
				\u0002.Type = global::\u001D.\u0005.\u0001(\u0002.Code, \u0004, \u0003, \u0005, this.TypeExpected, this._Scope, this.\u0001);
			}
			if (\u0002.Type == null)
			{
				if (\u0005.Count == 0)
				{
					\u0002.Type = TypeTable.Get(TypeClass.Int);
					return;
				}
				\u0002.Type = \u0005[0].Type.DeRefType;
			}
		}

		// Token: 0x06003135 RID: 12597 RVA: 0x000BCFF0 File Offset: 0x000BB1F0
		public void \u0097(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			switch (\u0002.Code)
			{
			case Operator.Limit:
				this.\u0001(\u0002, operandsList);
				return;
			case Operator.Min:
			case Operator.Max:
				TypifierAndCrossReferenceCollector.\u0001(this._Scope, \u0002, operandsList);
				break;
			case Operator.Mux:
			case Operator.Sel:
			{
				int i = 1;
				while (i < operandsList.Count)
				{
					_IExpression iexpression = operandsList[i];
					bool flag = false;
					TypeClass @class = iexpression.Type.DeRefType.Class;
					if (@class <= TypeClass.Userdef)
					{
						if (@class == TypeClass.Bool)
						{
							goto IL_2E2;
						}
						switch (@class)
						{
						case TypeClass.String:
						{
							_IStringType istringType = \u0002.Type as _IStringType;
							flag = true;
							if (istringType == null)
							{
								\u0002.Type = iexpression.Type.DeRefType;
							}
							else
							{
								int num = 0;
								if (istringType.Length == null)
								{
									num = 80;
								}
								else
								{
									_ILiteralValue iliteralValue = istringType.Length.Literal(this._Scope, true) as _ILiteralValue;
									if (iliteralValue == null)
									{
										\u0002.Type = iexpression.Type.DeRefType;
										break;
									}
									if (!iliteralValue.GetInt(out num))
									{
										\u0002.Type = iexpression.Type.DeRefType;
										break;
									}
								}
								_IStringType istringType2 = iexpression.Type.DeRefType as _IStringType;
								int num2 = 0;
								if (istringType2.Length == null)
								{
									num2 = 80;
								}
								else
								{
									_ILiteralValue iliteralValue2 = istringType2.Length.Literal(this._Scope, true) as _ILiteralValue;
									if (iliteralValue2 == null || !iliteralValue2.GetInt(out num2))
									{
										break;
									}
								}
								if (num2 > num)
								{
									\u0002.Type = iexpression.Type.DeRefType;
								}
							}
							break;
						}
						case TypeClass.WString:
						{
							_IWStringType iwstringType = \u0002.Type as _IWStringType;
							flag = true;
							if (iwstringType == null)
							{
								\u0002.Type = iexpression.Type.DeRefType;
							}
							else
							{
								int num3 = 0;
								if (iwstringType.Length == null)
								{
									num3 = 80;
								}
								else
								{
									_ILiteralValue iliteralValue3 = iwstringType.Length.Literal(this._Scope, true) as _ILiteralValue;
									if (iliteralValue3 == null)
									{
										\u0002.Type = iexpression.Type.DeRefType;
										break;
									}
									if (!iliteralValue3.GetInt(out num3))
									{
										\u0002.Type = iexpression.Type.DeRefType;
										break;
									}
								}
								_IWStringType iwstringType2 = iexpression.Type.DeRefType as _IWStringType;
								int num4 = 0;
								if (iwstringType2.Length == null)
								{
									num4 = 80;
								}
								else
								{
									_ILiteralValue iliteralValue4 = iwstringType2.Length.Literal(this._Scope, true) as _ILiteralValue;
									if (iliteralValue4 == null || !iliteralValue4.GetInt(out num4))
									{
										break;
									}
								}
								if (num4 > num3)
								{
									\u0002.Type = iexpression.Type.DeRefType;
								}
							}
							break;
						}
						case TypeClass.Time:
						case TypeClass.Date:
						case TypeClass.DateAndTime:
						case TypeClass.TimeOfDay:
							goto IL_2E2;
						case TypeClass.Pointer:
						case TypeClass.Array:
						case TypeClass.Userdef:
							\u0002.Type = iexpression.Type.DeRefType;
							break;
						}
					}
					else if (@class == TypeClass.LTime || @class - TypeClass.LDate <= 2)
					{
						goto IL_2E2;
					}
					IL_312:
					if (\u0002.Type == null || flag)
					{
						i++;
						continue;
					}
					break;
					IL_2E2:
					\u0002.Type = TypeTable.Get(iexpression.Type.DeRefType.Class);
					goto IL_312;
				}
				break;
			}
			}
			int u = 0;
			if (\u0002.Code == Operator.Mux || \u0002.Code == Operator.Sel)
			{
				u = 1;
			}
			this.\u0001(\u0002, u, false, operandsList);
		}

		// Token: 0x06003136 RID: 12598 RVA: 0x000BD350 File Offset: 0x000BB550
		private static void \u0001(IScope5 \u0002, _IOperatorExpression \u0003, IList<_IExpression> \u0004)
		{
			_IEnumType type;
			if (global::\u001D.\u0005.\u0001(\u0002, \u0004, 0, out type))
			{
				\u0003.Type = type;
				return;
			}
			foreach (_IExpression iexpression in \u0004)
			{
				TypeClass @class = iexpression.Type.DeRefType.Class;
				if (@class <= TypeClass.TimeOfDay)
				{
					if (@class == TypeClass.Bool || @class - TypeClass.String <= 5)
					{
						goto IL_52;
					}
				}
				else if (@class == TypeClass.LTime || @class - TypeClass.LDate <= 2)
				{
					goto IL_52;
				}
				IL_6D:
				if (\u0003.Type != null)
				{
					break;
				}
				continue;
				IL_52:
				\u0003.Type = TypeTable.Get(iexpression.Type.DeRefType.Class);
				goto IL_6D;
			}
		}

		// Token: 0x06003137 RID: 12599 RVA: 0x000BD3F8 File Offset: 0x000BB5F8
		private void \u0001(_IOperatorExpression \u0002, IList<_IExpression> \u0003)
		{
			if (\u0003.Count < 3)
			{
				\u0002.Type = TypeTable.Real;
				return;
			}
			\u0002.Type = \u0003[1].Type.DeRefType;
			if (TypeTable.IsInteger(\u0002.Type.Class))
			{
				ICompiledType compiledType = global::\u001D.\u0005.\u0001(\u0002.Code, false, 0, \u0003, this.TypeExpected, this._Scope, this.\u0001);
				if (compiledType != null)
				{
					\u0002.Type = compiledType;
				}
			}
		}

		// Token: 0x0400093D RID: 2365
		private readonly Stack<TypifierAndCrossReferenceCollector.ETypeStackContent> \u0001 = new Stack<TypifierAndCrossReferenceCollector.ETypeStackContent>();

		// Token: 0x0400093E RID: 2366
		private readonly ObjectPool<TypifierAndCrossReferenceCollector.ETypeStackContent> \u0001;

		// Token: 0x0400093F RID: 2367
		protected _ICompileContext \u0001;

		// Token: 0x04000940 RID: 2368
		protected _ICompiledPOU \u0001;

		// Token: 0x04000941 RID: 2369
		protected _ISignature \u0001;

		// Token: 0x04000942 RID: 2370
		protected _IVariable \u0001;

		// Token: 0x04000943 RID: 2371
		private readonly ICaseInsensitiveDictionary<TypifierAndCrossReferenceCollector.\u0001> \u0001;

		// Token: 0x04000944 RID: 2372
		private IPredefinedCalleeTypeProvider \u0001;

		// Token: 0x04000945 RID: 2373
		[CompilerGenerated]
		private \u001F.\u0007 \u0001;

		// Token: 0x04000946 RID: 2374
		[CompilerGenerated]
		private LDictionary<global::\u0017.\u0004, global::\u0017.\u0004> \u0001;

		// Token: 0x04000947 RID: 2375
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x0200032E RID: 814
		internal sealed class ETypeStackContent : \u001F.\u0001
		{
			// Token: 0x06003138 RID: 12600 RVA: 0x000BD470 File Offset: 0x000BB670
			internal ETypeStackContent()
			{
			}

			// Token: 0x06003139 RID: 12601 RVA: 0x000BD478 File Offset: 0x000BB678
			public void \u0001()
			{
				this.\u0001 = null;
				this.\u0001 = null;
				this.\u0001 = null;
				this.\u0001 = AccessFlag.Unknown;
				this.\u0001 = null;
				this.\u0001 = false;
				this.\u0002 = false;
				this.\u0003 = false;
			}

			// Token: 0x04000948 RID: 2376
			public ICompiledType \u0001;

			// Token: 0x04000949 RID: 2377
			public IScope5 \u0001;

			// Token: 0x0400094A RID: 2378
			public _IStatement \u0001;

			// Token: 0x0400094B RID: 2379
			public AccessFlag \u0001;

			// Token: 0x0400094C RID: 2380
			public _IExpressionStatement \u0001;

			// Token: 0x0400094D RID: 2381
			public bool \u0001;

			// Token: 0x0400094E RID: 2382
			public bool \u0002;

			// Token: 0x0400094F RID: 2383
			public bool \u0003;
		}

		// Token: 0x0200032F RID: 815
		private struct \u0001
		{
			// Token: 0x04000950 RID: 2384
			public IVariable[] \u0001;

			// Token: 0x04000951 RID: 2385
			public ISignature[] \u0001;

			// Token: 0x04000952 RID: 2386
			public IScope \u0001;
		}
	}
}
