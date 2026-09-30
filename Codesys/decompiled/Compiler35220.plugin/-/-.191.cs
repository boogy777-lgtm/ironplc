using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using \u0003;
using \u0007;
using \u0011;
using \u0014;
using \u0017;
using \u0018;
using \u0019;
using \u001D;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u007F;
using \u0084;

namespace \u001E
{
	// Token: 0x02000226 RID: 550
	internal sealed class \u000E : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2, IExprementVisitorAdapter, ICodeAdapter6, ICodeAdapter5, ICodeAdapter4, ICodeAdapter3, ICodeAdapter2, ICodeAdapter
	{
		// Token: 0x06002448 RID: 9288 RVA: 0x0007C6D0 File Offset: 0x0007A8D0
		public \u000E(IScope5 \u009B\u0002, bool \u0014\u0004, _ICompileContext \u0001\u0002)
		{
			this.Scope = \u009B\u0002;
			this.\u0001 = \u0014\u0004;
			this.\u0001 = \u0001\u0002.DataManager;
			this.TargetSettings = \u0001\u0002.GetTargetSettings();
			this.\u0001 = \u0001\u0002;
			this.\u0002 = \u0001\u0002.IsDefined(CompileAttributes.ATTRIBUTE_GENERATE_EXCEPTIONINFO);
		}

		// Token: 0x170006A0 RID: 1696
		// (get) Token: 0x06002449 RID: 9289 RVA: 0x0007C774 File Offset: 0x0007A974
		// (set) Token: 0x0600244A RID: 9290 RVA: 0x0007C77C File Offset: 0x0007A97C
		public ICodegenerator Codegen
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
				this.\u0004 = !this.\u0001(CodegeneratorProperties.SupportsVectorOperations);
			}
		}

		// Token: 0x170006A1 RID: 1697
		// (get) Token: 0x0600244B RID: 9291 RVA: 0x0007C798 File Offset: 0x0007A998
		public ICodegenerator7 Codegen7
		{
			get
			{
				return this.\u0001 as ICodegenerator7;
			}
		}

		// Token: 0x170006A2 RID: 1698
		// (get) Token: 0x0600244C RID: 9292 RVA: 0x0007C7A8 File Offset: 0x0007A9A8
		public ICodegenerator9 Codegen9
		{
			get
			{
				return this.\u0001 as ICodegenerator9;
			}
		}

		// Token: 0x170006A3 RID: 1699
		// (get) Token: 0x0600244D RID: 9293 RVA: 0x0007C7B8 File Offset: 0x0007A9B8
		public ICodegenerator12 Codegen12
		{
			get
			{
				return this.\u0001 as ICodegenerator12;
			}
		}

		// Token: 0x0600244E RID: 9294 RVA: 0x0007C7C8 File Offset: 0x0007A9C8
		public bool \u0001(CodegeneratorProperties \u0002)
		{
			ICodegenerator3 codegenerator = this.\u0001 as ICodegenerator3;
			return codegenerator != null && codegenerator.GetProperty(\u0002);
		}

		// Token: 0x170006A4 RID: 1700
		// (get) Token: 0x0600244F RID: 9295 RVA: 0x0007C7F0 File Offset: 0x0007A9F0
		// (set) Token: 0x06002450 RID: 9296 RVA: 0x0007C7F8 File Offset: 0x0007A9F8
		public IScope5 Scope { get; private set; }

		// Token: 0x06002451 RID: 9297 RVA: 0x0007C804 File Offset: 0x0007AA04
		public void \u0001(bool \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06002452 RID: 9298 RVA: 0x0007C814 File Offset: 0x0007AA14
		public void \u0001()
		{
			this.\u0001.Pop();
		}

		// Token: 0x170006A5 RID: 1701
		// (get) Token: 0x06002453 RID: 9299 RVA: 0x0007C824 File Offset: 0x0007AA24
		public bool TopOfStack_ForceScalarType
		{
			get
			{
				return this.\u0001.Count > 0 && this.\u0001.Peek();
			}
		}

		// Token: 0x170006A6 RID: 1702
		// (get) Token: 0x06002454 RID: 9300 RVA: 0x0007C844 File Offset: 0x0007AA44
		public IDataManager DataManager
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170006A7 RID: 1703
		// (get) Token: 0x06002455 RID: 9301 RVA: 0x0007C84C File Offset: 0x0007AA4C
		public ITargetSettings TargetSettings { get; }

		// Token: 0x06002456 RID: 9302 RVA: 0x0007C854 File Offset: 0x0007AA54
		public void \u0001(IExprement \u0002)
		{
			this.\u0001.Push(\u0002);
			_IExprement iexprement = \u0002 as _IExprement;
			Debug.\u0001(iexprement != null);
			if (iexprement == null)
			{
				this.\u0001.Pop();
				return;
			}
			iexprement.Accept(this);
			this.\u0001.Pop();
		}

		// Token: 0x170006A8 RID: 1704
		// (get) Token: 0x06002457 RID: 9303 RVA: 0x0007C8A0 File Offset: 0x0007AAA0
		public IExprement CurrentExpression
		{
			get
			{
				if (this.\u0001.Count <= 0)
				{
					return null;
				}
				return this.\u0001.Peek();
			}
		}

		// Token: 0x170006A9 RID: 1705
		// (get) Token: 0x06002458 RID: 9304 RVA: 0x0007C8C0 File Offset: 0x0007AAC0
		public ICompiledPOU CurrentPOU
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x06002459 RID: 9305 RVA: 0x0007C8C8 File Offset: 0x0007AAC8
		public bool \u0001(string \u0002)
		{
			if (this.\u0001.Count > 0)
			{
				_IExprement iexprement = this.\u0001.Peek() as _IExprement;
				if (iexprement._Position != null && iexprement.LengthIntern > 0)
				{
					this.\u0001(iexprement, \u0002);
					return true;
				}
				IExprement[] array = this.\u0001.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					iexprement = (array[i] as _IExprement);
					if (iexprement._Position != null && iexprement.LengthIntern > 0)
					{
						this.\u0001(iexprement, \u0002);
						return true;
					}
				}
			}
			_ICompiledPOU u = this.\u0001;
			if (((u != null) ? u.GetParseTree() : null) != null && this.\u0001.GetParseTree()._Position != null)
			{
				this.\u0001(this.\u0001.GetParseTree(), \u0002);
				return true;
			}
			return false;
		}

		// Token: 0x0600245A RID: 9306 RVA: 0x0007C988 File Offset: 0x0007AB88
		private void \u0001(_IExprement \u0002, string \u0003)
		{
			int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(this.\u0001[this.\u0001.SignatureId].LibraryPath);
			Guid messageGuid = this.\u0001.MessageGuid;
			IMinimalPosition position = \u0002._Position;
			long u = (position != null) ? position.EditorPosition : -1L;
			IMinimalPosition position2 = \u0002._Position;
			_ICompilerMessage message = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(projectHandle, messageGuid, u, (position2 != null) ? position2.PositionOffset : 0, \u0002.LengthIntern), \u0003, Severity.FatalError, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
		}

		// Token: 0x0600245B RID: 9307 RVA: 0x0007CA24 File Offset: 0x0007AC24
		public void \u0001(TypeClass \u0002, TypeClass \u0003, params IExpression[] \u0004)
		{
			Debug.\u0001(false);
		}

		// Token: 0x0600245C RID: 9308 RVA: 0x0007CA2C File Offset: 0x0007AC2C
		public void \u0001(string \u0002, params IExpression[] \u0003)
		{
			Debug.\u0001(false);
		}

		// Token: 0x0600245D RID: 9309 RVA: 0x0007CA34 File Offset: 0x0007AC34
		public void \u0001(string \u0002, TypeClass \u0003, params IExpression[] \u0004)
		{
			Debug.\u0001(false);
		}

		// Token: 0x0600245E RID: 9310 RVA: 0x0007CA3C File Offset: 0x0007AC3C
		public ISignature \u0001(int \u0002)
		{
			return this.Scope[\u0002];
		}

		// Token: 0x0600245F RID: 9311 RVA: 0x0007CA4C File Offset: 0x0007AC4C
		public bool \u0001(Operator \u0002)
		{
			if (\u0002 <= Operator.Plus)
			{
				if (\u0002 - Operator.Min > 1)
				{
					switch (\u0002)
					{
					case Operator.Add:
					case Operator.Mul:
					case Operator.And:
					case Operator.AndN:
					case Operator.Or:
					case Operator.OrN:
					case Operator.Xor:
					case Operator.XorN:
					case Operator.Eq:
					case Operator.Ne:
						break;
					case Operator.Sub:
					case Operator.Div:
					case Operator.Mod:
					case Operator.Not:
						return false;
					default:
						if (\u0002 != Operator.Plus)
						{
							return false;
						}
						break;
					}
				}
			}
			else if (\u0002 != Operator.Times && \u0002 - Operator.Equal > 1)
			{
				switch (\u0002)
				{
				case Operator.__vcAdd:
				case Operator.__vcMul:
				case Operator.__vcDot:
				case Operator.__vcMin:
				case Operator.__vcMax:
					break;
				case Operator.__vcSub:
				case Operator.__vcDiv:
				case Operator.__vcSqrt:
					return false;
				default:
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002460 RID: 9312 RVA: 0x0007CAFC File Offset: 0x0007ACFC
		public static bool \u0001(Operator \u0002)
		{
			return \u0002 - Operator.Eq <= 5 || \u0002 - Operator.Less <= 5;
		}

		// Token: 0x06002461 RID: 9313 RVA: 0x0007CB18 File Offset: 0x0007AD18
		public bool \u0002(Operator \u0002)
		{
			return \u001E.\u000E.\u0001(\u0002);
		}

		// Token: 0x06002462 RID: 9314 RVA: 0x0007CB20 File Offset: 0x0007AD20
		public Operator \u0001(Operator \u0002)
		{
			switch (\u0002)
			{
			case Operator.Eq:
				return Operator.Ne;
			case Operator.Ne:
				return Operator.Eq;
			case Operator.Ge:
				return Operator.Lt;
			case Operator.Gt:
				return Operator.Le;
			case Operator.Le:
				return Operator.Gt;
			case Operator.Lt:
				return Operator.Ge;
			default:
				switch (\u0002)
				{
				case Operator.Less:
					return Operator.GreaterEqual;
				case Operator.Greater:
					return Operator.LessEqual;
				case Operator.LessEqual:
					return Operator.Greater;
				case Operator.GreaterEqual:
					return Operator.Less;
				case Operator.Equal:
					return Operator.NotEqual;
				case Operator.NotEqual:
					return Operator.Equal;
				default:
					Debug.\u0001(false);
					return Operator.None;
				}
				break;
			}
		}

		// Token: 0x06002463 RID: 9315 RVA: 0x0007CBC8 File Offset: 0x0007ADC8
		public int \u0001(TypeClass \u0002)
		{
			return TypeTable.GetSize(\u0002, this.Scope);
		}

		// Token: 0x06002464 RID: 9316 RVA: 0x0007CBD8 File Offset: 0x0007ADD8
		public bool \u0001(TypeClass \u0002)
		{
			return TypeTable.IsSigned(\u0002);
		}

		// Token: 0x06002465 RID: 9317 RVA: 0x0007CBE0 File Offset: 0x0007ADE0
		public bool \u0002(TypeClass \u0002)
		{
			return TypeTable.IsLType(\u0002);
		}

		// Token: 0x06002466 RID: 9318 RVA: 0x0007CBE8 File Offset: 0x0007ADE8
		public bool \u0003(TypeClass \u0002)
		{
			return \u0002 == TypeClass.LTime || \u0002 - TypeClass.LDate <= 2 || TypeTable.IsLInteger(\u0002, this.Scope);
		}

		// Token: 0x06002467 RID: 9319 RVA: 0x0007CC04 File Offset: 0x0007AE04
		public bool \u0001(ICompiledType \u0002)
		{
			return \u0084.\u0004.\u0001(\u0002, this.Scope, this.Codegen);
		}

		// Token: 0x06002468 RID: 9320 RVA: 0x0007CC18 File Offset: 0x0007AE18
		public bool \u0004(TypeClass \u0002)
		{
			if (\u0002 <= TypeClass.Array)
			{
				if (\u0002 - TypeClass.String > 1 && \u0002 != TypeClass.Array)
				{
					return true;
				}
			}
			else if (\u0002 != TypeClass.Userdef)
			{
				if (\u0002 != TypeClass.__Vector)
				{
					return true;
				}
				return !this.\u0004;
			}
			return false;
		}

		// Token: 0x06002469 RID: 9321 RVA: 0x0007CC48 File Offset: 0x0007AE48
		public bool \u0005(TypeClass \u0002)
		{
			return TypeTable.IsReal(\u0002);
		}

		// Token: 0x0600246A RID: 9322 RVA: 0x0007CC50 File Offset: 0x0007AE50
		public bool \u0006(TypeClass \u0002)
		{
			return TypeTable.IsString(\u0002);
		}

		// Token: 0x0600246B RID: 9323 RVA: 0x0007CC58 File Offset: 0x0007AE58
		public string \u0001(Operator \u0002)
		{
			return this.\u0001.GetOperatorText(\u0002);
		}

		// Token: 0x0600246C RID: 9324 RVA: 0x0007CC68 File Offset: 0x0007AE68
		public string \u0001(string \u0002)
		{
			this.\u0001++;
			return "@" + \u0002 + "_" + this.\u0001.ToString();
		}

		// Token: 0x0600246D RID: 9325 RVA: 0x0007CC94 File Offset: 0x0007AE94
		public int \u0001(ISignature \u0002)
		{
			if (\u0002 == null || \u0002.POUType != Operator.Method)
			{
				return -1;
			}
			IVariable variable = \u0002[IdentifierConstants.InstancePointer];
			if (variable == null)
			{
				return -1;
			}
			return variable.DataLocation.Offset + this.Codegen.StackDisplacement;
		}

		// Token: 0x0600246E RID: 9326 RVA: 0x0007CCD8 File Offset: 0x0007AED8
		public IJumpTable[] \u0001(ICaseStatement \u0002, int \u0003)
		{
			LList<\u001E.\u000E.\u0001.\u0001> llist = new LList<\u001E.\u000E.\u0001.\u0001>();
			TypeClass @class = \u0002.Switch.Type.Class;
			ICase[] cases = \u0002.Cases;
			for (int i = 0; i < cases.Length; i++)
			{
				_ICaseLabelStatement icaseLabelStatement = (_ICaseLabelStatement)cases[i].Label;
				for (int j = 0; j < icaseLabelStatement._cases.Count; j++)
				{
					if (icaseLabelStatement._cases[j].IsLiteral && icaseLabelStatement._cases[j].Type.Class == @class)
					{
						ILiteralValue literalValue = icaseLabelStatement._cases[j].Literal(this.Scope);
						KindOfLiteral kindOf = literalValue.KindOf;
						long u0011_u;
						if (kindOf != KindOfLiteral.SignedInteger)
						{
							if (kindOf != KindOfLiteral.UnsignedInteger)
							{
								goto IL_D2;
							}
							u0011_u = (long)literalValue.UnsignedLong;
						}
						else
						{
							u0011_u = literalValue.SignedLong;
						}
						llist.Add(new \u001E.\u000E.\u0001.\u0001(u0011_u, icaseLabelStatement._cases[j]));
					}
					IL_D2:;
				}
			}
			llist.Sort(new \u001E.\u000E.\u0001.\u0002());
			LList<IJumpTable> llist2 = new LList<IJumpTable>();
			\u001E.\u000E.\u0001 u = null;
			for (int k = 0; k < llist.Count; k++)
			{
				\u001E.\u000E.\u0001.\u0001 u2 = llist[k];
				if (u == null)
				{
					u = new \u001E.\u000E.\u0001(u2.Value);
					u.\u0001(u2);
				}
				else if (u.End == u2.Value || u.End + 1L == u2.Value)
				{
					u.\u0001(u2);
				}
				else
				{
					if (u.Count >= \u0003)
					{
						llist2.Add(u);
					}
					u = new \u001E.\u000E.\u0001(u2.Value);
					u.\u0001(u2);
				}
			}
			if (u != null && u.Count >= \u0003)
			{
				llist2.Add(u);
			}
			return llist2.ToArray();
		}

		// Token: 0x0600246F RID: 9327 RVA: 0x0007CE9C File Offset: 0x0007B09C
		public byte[] \u0001(string \u0002, TypeClass \u0003, bool \u0004, int \u0005)
		{
			StringEncoding stringEncoding = this.\u0001(\u0002, \u0003);
			if (stringEncoding != StringEncoding.Default && stringEncoding != StringEncoding.UTF8)
			{
				this.\u0001("Internal Error: Unknown string encoding.");
			}
			ByteOrder u = \u0004 ? ByteOrder.Motorola : ByteOrder.Intel;
			return global::\u0017.\u0003.Singleton.\u0001(u, \u0003, \u0005, \u0002, stringEncoding);
		}

		// Token: 0x06002470 RID: 9328 RVA: 0x0007CEE0 File Offset: 0x0007B0E0
		private StringEncoding \u0001(string \u0002, TypeClass \u0003)
		{
			StringEncoding result = StringEncoding.Default;
			_ILiteralExpression iliteralExpression = this.\u0001.Peek() as _ILiteralExpression;
			TypeClass @class = iliteralExpression.Type.DeRefType.Class;
			if (@class == TypeClass.String)
			{
				_ILiteralValue iliteralValue = iliteralExpression.LiteralValue as _ILiteralValue;
				if (iliteralValue.KindOf != KindOfLiteral.String || @class != \u0003 || iliteralValue.String != \u0002)
				{
					this.\u0001("Internal Error: Literal missmatch.");
				}
				else
				{
					result = (iliteralExpression as _IStringLiteralExpression2).StringEncoding;
				}
			}
			return result;
		}

		// Token: 0x06002471 RID: 9329 RVA: 0x0007CF58 File Offset: 0x0007B158
		public object \u0001(ExpressionProperties \u0002, IExpression \u0003)
		{
			if (\u0002 == ExpressionProperties.IsIntermediateResult)
			{
				return global::\u0014.\u000E.\u0001(\u0003 as _IExpression);
			}
			return null;
		}

		// Token: 0x06002472 RID: 9330 RVA: 0x0007CF70 File Offset: 0x0007B170
		public bool \u0001(ISignature \u0002)
		{
			if (\u0002.GetFlag(SignatureFlag.External))
			{
				if (this.\u0002)
				{
					return true;
				}
				if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_GENERATE_EXCEPTIONINFO))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002473 RID: 9331 RVA: 0x0007CF98 File Offset: 0x0007B198
		public void \u0001(_ITryCatchStatement \u0002)
		{
			\u0002.DefaultTraverse(this);
		}

		// Token: 0x06002474 RID: 9332 RVA: 0x0007CFA4 File Offset: 0x0007B1A4
		public void \u0001(_ICompiledPOU \u0002)
		{
			this.\u0001 = 0;
			_ISequenceStatement isequenceStatement = \u0002.ParseTree as _ISequenceStatement;
			List<_ISubRoutineStatement> list = new List<_ISubRoutineStatement>();
			_ISequenceStatement seqMainRoutine = null;
			if (this.Codegen is ISubroutineCodegenerator && \u0002.GetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch))
			{
				List<IStatement> list2 = new List<IStatement>();
				foreach (IStatement statement in isequenceStatement.StatementList)
				{
					if (statement is _ISubRoutineStatement)
					{
						list.Add(statement as _ISubRoutineStatement);
					}
					else
					{
						list2.Add(statement);
					}
				}
				seqMainRoutine = global::\u0019.\u0003.\u0001(list2);
			}
			this.\u0001 = 0;
			this.\u0001 = \u0002;
			this.\u0001.ClearBitWriteAccesses();
			bool flag = false;
			ISignature signature = this.Scope[this.\u0001.SignatureId];
			if (signature != null && signature.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INIT_FUN))
			{
				flag = true;
			}
			ICodegenerator3 codegenerator = this.Codegen as ICodegenerator3;
			if (!flag && codegenerator != null && codegenerator.GetProperty(CodegeneratorProperties.CheckConcurrentBitAccess))
			{
				this.\u0003 = true;
			}
			else
			{
				this.\u0003 = false;
			}
			this.\u0001.Clear();
			if (this.Codegen is ISubroutineCodegenerator && \u0002.GetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch))
			{
				(this.Codegen as ISubroutineCodegenerator).Generate(\u0002, seqMainRoutine, this.\u0001, list);
			}
			else
			{
				this.Codegen.Generate(\u0002, this.\u0001);
			}
			this.\u0001 = null;
		}

		// Token: 0x06002475 RID: 9333 RVA: 0x0007D11C File Offset: 0x0007B31C
		public void \u0001(_ISequenceStatement \u0002)
		{
			if (\u0002 is _ISubRoutineStatement && this.Codegen is ICodegenerator6)
			{
				this.\u0005 = true;
				this.\u0001 += 1;
				(this.Codegen as ICodegenerator6).GenerateTrySubroutine(this.\u0001, \u0002);
				this.\u0005 = false;
				return;
			}
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x06002476 RID: 9334 RVA: 0x0007D180 File Offset: 0x0007B380
		public void \u0001(_IWhileStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x06002477 RID: 9335 RVA: 0x0007D190 File Offset: 0x0007B390
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x06002478 RID: 9336 RVA: 0x0007D1A0 File Offset: 0x0007B3A0
		public void \u0001(_IForStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x06002479 RID: 9337 RVA: 0x0007D1B0 File Offset: 0x0007B3B0
		public void \u0001(_IExitStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x0600247A RID: 9338 RVA: 0x0007D1C0 File Offset: 0x0007B3C0
		public void \u0001(_IContinueStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x0600247B RID: 9339 RVA: 0x0007D1D0 File Offset: 0x0007B3D0
		public void \u0001(_IAssignmentExpression \u0002)
		{
			bool flag = false;
			ICompiledType type = null;
			ICompiledType type2 = null;
			ICompiledType compiledType = null;
			if (!this.\u0004(\u0002.Type.Class) && \u0084.\u0004.\u0001(\u0002.Type, this.Scope, this.\u0001))
			{
				type = \u0002._LValue.Type;
				type2 = \u0002._RValue.Type;
				compiledType = \u0002.Type;
				if ((this.Codegen as ICodegenerator3).RegisterSize == 8)
				{
					\u0002._LValue.Type = TypeTable.ULInt;
					\u0002._RValue.Type = TypeTable.ULInt;
					\u0002._CompiledType = TypeTable.ULInt;
				}
				else
				{
					\u0002._LValue.Type = TypeTable.UDInt;
					\u0002._RValue.Type = TypeTable.UDInt;
					\u0002._CompiledType = TypeTable.UDInt;
				}
				flag = true;
				this.\u0001(true);
			}
			else
			{
				this.\u0001(false);
			}
			this.Codegen.Generate(\u0002);
			this.\u0001();
			if (flag)
			{
				\u0002._LValue.Type = type;
				\u0002._RValue.Type = type2;
				\u0002._CompiledType = compiledType;
			}
		}

		// Token: 0x0600247C RID: 9340 RVA: 0x0007D2E8 File Offset: 0x0007B4E8
		public void \u0001(_IIfStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x0600247D RID: 9341 RVA: 0x0007D2F8 File Offset: 0x0007B4F8
		public void \u0001(_IReturnStatement \u0002)
		{
			Debug.\u0001(false, "Return found in adapter");
		}

		// Token: 0x0600247E RID: 9342 RVA: 0x0007D308 File Offset: 0x0007B508
		public void \u0001(_IJumpStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x0600247F RID: 9343 RVA: 0x0007D318 File Offset: 0x0007B518
		public void \u0001(_ILabelStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x06002480 RID: 9344 RVA: 0x0007D328 File Offset: 0x0007B528
		public void \u0001(_ICommentStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x06002481 RID: 9345 RVA: 0x0007D338 File Offset: 0x0007B538
		public void \u0001(_IPragmaStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x06002482 RID: 9346 RVA: 0x0007D348 File Offset: 0x0007B548
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x06002483 RID: 9347 RVA: 0x0007D358 File Offset: 0x0007B558
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002484 RID: 9348 RVA: 0x0007D35C File Offset: 0x0007B55C
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06002485 RID: 9349 RVA: 0x0007D360 File Offset: 0x0007B560
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002486 RID: 9350 RVA: 0x0007D364 File Offset: 0x0007B564
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002487 RID: 9351 RVA: 0x0007D368 File Offset: 0x0007B568
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002488 RID: 9352 RVA: 0x0007D36C File Offset: 0x0007B56C
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06002489 RID: 9353 RVA: 0x0007D370 File Offset: 0x0007B570
		public void \u0001(_ICallInstanceExpression \u0002)
		{
			Debug.\u0001(this.Codegen is ICodegenerator4);
			(this.Codegen as ICodegenerator4).GenerateCallInstanceAccess(\u0002);
		}

		// Token: 0x0600248A RID: 9354 RVA: 0x0007D398 File Offset: 0x0007B598
		public void \u0001(_IProgramCounterExpression \u0002)
		{
		}

		// Token: 0x0600248B RID: 9355 RVA: 0x0007D39C File Offset: 0x0007B59C
		public void \u0001(_IFramePointerExpression \u0002)
		{
			Debug.\u0001(this.Codegen is ICodegenerator6);
			(this.Codegen as ICodegenerator6).GenerateFramePointer();
		}

		// Token: 0x0600248C RID: 9356 RVA: 0x0007D3C4 File Offset: 0x0007B5C4
		public void \u0001(_ICallExpression \u0002)
		{
			ICallExprInfo callInfo = \u0002.CallInfo;
			ISignature signToCall = this.Scope[callInfo.IdCalledSignature];
			KindOfCall kindOfCall = callInfo.KindOfCall;
			if (kindOfCall > KindOfCall.InterfaceCall)
			{
				Debug.\u0001(false);
				return;
			}
			if (this.Codegen is ICodegenerator4)
			{
				(this.Codegen as ICodegenerator4).GenerateCall2(\u0002, signToCall, callInfo.KindOfCall, callInfo.ExpLoadTargetAddress, callInfo.InstanceAssignment);
				return;
			}
			this.Codegen.GenerateCall(\u0002, signToCall, callInfo.KindOfCall, callInfo.ExpLoadTargetAddress);
		}

		// Token: 0x0600248D RID: 9357 RVA: 0x0007D448 File Offset: 0x0007B648
		public void \u0001(_IOperatorExpression \u0002)
		{
			if (\u0002.Code == Operator.__FCall)
			{
				this.\u0001(\u0002._OperandsList[\u0002._OperandsList.Count - 1]);
				return;
			}
			int[] array = new int[\u0002._OperandsList.Count];
			for (int i = 0; i < \u0002._OperandsList.Count; i++)
			{
				if ((\u0002.Info != null && this.\u0001.\u0001(\u0002, this.Scope)) || (\u0002.Info != null && \u0002.Info.HasSideEffect))
				{
					array[i] = \u0002._OperandsList.Count - i;
				}
				else if (\u0002._OperandsList[i].Info != null)
				{
					array[i] = \u0002._OperandsList[i].Info.NestingDepth;
				}
				else
				{
					array[i] = 0;
				}
			}
			if (\u0002.Code == Operator.Adr || \u0002.Code == Operator.__RefAdr)
			{
				this.\u0001(\u0002._OperandsList[0]);
				return;
			}
			this.Codegen.Generate(\u0002, array, \u0002.Type);
		}

		// Token: 0x0600248E RID: 9358 RVA: 0x0007D560 File Offset: 0x0007B760
		public void \u0001(_ICastExpression \u0002)
		{
			this.\u0001(\u0002.BaseExpression);
		}

		// Token: 0x0600248F RID: 9359 RVA: 0x0007D570 File Offset: 0x0007B770
		public void \u0001(_INewExpression \u0002)
		{
			Debug.\u0001(false);
		}

		// Token: 0x06002490 RID: 9360 RVA: 0x0007D578 File Offset: 0x0007B778
		public void \u0001(_ITypeExpression \u0002)
		{
			Debug.\u0001(false);
		}

		// Token: 0x06002491 RID: 9361 RVA: 0x0007D580 File Offset: 0x0007B780
		public void \u0001(_IConversionExpression \u0002)
		{
			if (\u0002.From == TypeClass.Reference && \u0002.To == TypeClass.Pointer)
			{
				this.\u0001(\u0002._Exp);
				return;
			}
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x06002492 RID: 9362 RVA: 0x0007D5B0 File Offset: 0x0007B7B0
		public void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x06002493 RID: 9363 RVA: 0x0007D5B4 File Offset: 0x0007B7B4
		public void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x06002494 RID: 9364 RVA: 0x0007D5B8 File Offset: 0x0007B7B8
		public void \u0001(_ILiteralExpression \u0002)
		{
			this.Codegen.GenerateLiteral(\u0002);
		}

		// Token: 0x06002495 RID: 9365 RVA: 0x0007D5C8 File Offset: 0x0007B7C8
		public void \u0001(_IAddressExpression \u0002)
		{
			global::\u0018.\u0004 u = \u0002.Info as global::\u0018.\u0004;
			if (u == null)
			{
				Debug.\u0001(false);
			}
			int num = u.CompiledType.Size(this.Scope);
			if (!u.DoGeneration)
			{
				return;
			}
			ICodeGeneratorAttributes codeGeneratorAttributes = this.\u0001(\u0002);
			if (codeGeneratorAttributes != null && codeGeneratorAttributes.OverwriteAM != AccessModeFlags.Unknown)
			{
				u.AccessMode.SetAccessMode((AccessModeFlags)4294967295U, false);
				u.AccessMode.SetAccessMode(codeGeneratorAttributes.OverwriteAM, true);
				if (u.AccessMode.GetAccessMode(AccessModeFlags.WriteAddress) || u.AccessMode.GetAccessMode(AccessModeFlags.ReadAddress))
				{
					u.CompiledType = global::\u0019.\u0003.\u0001(u.CompiledType as _IType);
				}
			}
			if (u.DataLocation.IsBitLocation && !u.AccessMode.GetAccessMode(AccessModeFlags.ReadAddress))
			{
				this.Codegen.GenerateBitAccess(u.WithoutBit, u.CompiledType, u.AccessMode, (int)u.DataLocation.BitNr);
				if (this.\u0003)
				{
					Debug.\u0001(num == 1);
					if (u.DataLocation.IsBitLocation && (u.\u0002(AccessModeFlags.Write) || u.\u0002(AccessModeFlags.Set) || u.\u0002(AccessModeFlags.Reset)))
					{
						IBitWriteAccess bwa = global::\u0019.\u0003.\u0001(this.\u0001.SignatureId, (int)u.DataLocation.Area, u.DataLocation.Offset, u.DataLocation.BitNr, \u0002.PositionIntern, \u0002.ToString());
						this.\u0001.AddBitWriteAccess(bwa);
						return;
					}
				}
			}
			else
			{
				this.Codegen.GenerateDirectAddress(\u0002, (int)u.DataLocation.Area, u.DataLocation.Offset, u.CompiledType, num, u.AccessMode);
			}
		}

		// Token: 0x06002496 RID: 9366 RVA: 0x0007D770 File Offset: 0x0007B970
		public static _ILiteralValue \u0001(TypeClass \u0002)
		{
			if (\u0002 <= TypeClass.AnyBit)
			{
				switch (\u0002)
				{
				case TypeClass.Bool:
				case TypeClass.Bit:
					goto IL_8D;
				case TypeClass.Byte:
				case TypeClass.Word:
				case TypeClass.DWord:
				case TypeClass.LWord:
				case TypeClass.USInt:
				case TypeClass.UInt:
				case TypeClass.UDInt:
				case TypeClass.ULInt:
					break;
				case TypeClass.SInt:
				case TypeClass.Int:
				case TypeClass.DInt:
				case TypeClass.LInt:
					goto IL_94;
				case TypeClass.Real:
				case TypeClass.LReal:
					goto IL_6B;
				case TypeClass.String:
				case TypeClass.WString:
					goto IL_82;
				default:
					if (\u0002 != TypeClass.AnyBit)
					{
						goto IL_94;
					}
					break;
				}
				return global::\u0019.\u0003.\u0001(0UL);
			}
			if (\u0002 != TypeClass.AnyReal)
			{
				if (\u0002 == TypeClass.BitConst)
				{
					goto IL_8D;
				}
				if (\u0002 != TypeClass.AnyString)
				{
					goto IL_94;
				}
				goto IL_82;
			}
			IL_6B:
			return global::\u0019.\u0003.\u0001(0.0);
			IL_82:
			return global::\u0019.\u0003.\u0001(string.Empty);
			IL_8D:
			return global::\u0019.\u0003.\u0001(false);
			IL_94:
			return global::\u0019.\u0003.\u0001(0L);
		}

		// Token: 0x06002497 RID: 9367 RVA: 0x0007D818 File Offset: 0x0007BA18
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06002498 RID: 9368 RVA: 0x0007D81C File Offset: 0x0007BA1C
		public void \u0001(_IVariableExpression \u0002)
		{
			\u001D.\u0007 u = \u0002.VarInfo as \u001D.\u0007;
			if (u == null)
			{
				Debug.\u0001(false);
			}
			if (!u.DoGeneration)
			{
				return;
			}
			int num = u.CompiledType.Size(this.Scope);
			ICodeGeneratorAttributes codeGeneratorAttributes = this.\u0001(\u0002);
			if (codeGeneratorAttributes != null && codeGeneratorAttributes.OverwriteAM != AccessModeFlags.Unknown)
			{
				u.AccessMode.SetAccessMode((AccessModeFlags)4294967295U, false);
				u.AccessMode.SetAccessMode(codeGeneratorAttributes.OverwriteAM, true);
				if (u.AccessMode.GetAccessMode(AccessModeFlags.WriteAddress) || u.AccessMode.GetAccessMode(AccessModeFlags.ReadAddress))
				{
					u.CompiledType = global::\u0019.\u0003.\u0001(u.CompiledType as _IType);
				}
			}
			ICompiledType compiledType = u.CompiledType;
			if (u.CompiledType.Class == TypeClass.Subrange || u.CompiledType.Class == TypeClass.Enum)
			{
				compiledType = u.CompiledType.DeRefType;
				num = compiledType.Size(this.Scope);
			}
			ICompiledType u2 = compiledType;
			TypeClass @class = compiledType.Class;
			if (@class <= TypeClass.Array)
			{
				if (@class - TypeClass.String > 1)
				{
					if (@class != TypeClass.Array)
					{
						goto IL_1BE;
					}
					compiledType = compiledType.BaseType;
				}
			}
			else if (@class != TypeClass.Userdef)
			{
				if (@class != TypeClass.__Vector || !this.\u0004)
				{
					goto IL_1BE;
				}
				compiledType = compiledType.BaseType;
			}
			if (\u0084.\u0004.\u0001(u2, this.Scope, this.Codegen) && this.TopOfStack_ForceScalarType)
			{
				if ((this.Codegen as ICodegenerator3).RegisterSize == 8)
				{
					compiledType = TypeTable.ULInt;
				}
				else
				{
					compiledType = TypeTable.UDInt;
				}
			}
			else if (u.AccessMode.GetAccessMode(AccessModeFlags.ReadAddress) || u.AccessMode.GetAccessMode(AccessModeFlags.WriteAddress))
			{
				compiledType = global::\u0019.\u0003.\u0001(compiledType as _IType);
			}
			else if (u.AccessMode.HasAccessMode(AccessModeFlags.Read | AccessModeFlags.Write))
			{
				u.AccessMode.SetAccessMode(AccessModeFlags.Write, false);
				u.AccessMode.SetAccessMode(AccessModeFlags.ReadAddress, true);
				compiledType = global::\u0019.\u0003.\u0001(compiledType as _IType);
			}
			IL_1BE:
			if (compiledType is ISpecialSizeType)
			{
				compiledType = (compiledType as ISpecialSizeType).CodegeneratorType;
			}
			_IVariable ivariable = null;
			ISignature signature = this.Scope[u.SignatureId];
			if (signature != null)
			{
				ivariable = (signature[u.VariableId] as _IVariable);
			}
			int num2 = -1;
			if (u.IsBit)
			{
				num2 = (int)u.BitNr;
			}
			if (ivariable == null)
			{
				this.\u0001(string.Format(global::\u0011.\u0001.UnresolvedVariable, \u0002.Name));
				return;
			}
			if (!ivariable.GetFlag(VarFlag.ReplacedConstant))
			{
				if (num2 != -1 && !u.AccessMode.GetAccessMode(AccessModeFlags.ReadAddress))
				{
					u.BitNr = byte.MaxValue;
					global::\u0011.\u0008 u3 = u._AccessMode.\u0001() as global::\u0011.\u0008;
					u.AccessMode.SetAccessMode((AccessModeFlags)4294967295U, false);
					u.AccessMode.SetAccessMode(AccessModeFlags.Read, true);
					this.Codegen.GenerateBitAccess(\u0002, compiledType, u3, num2);
					u.BitNr = (byte)num2;
					u._AccessMode = u3;
					if (this.\u0003 && ivariable.Address != null && ivariable.Address.Size == DirectVariableSize.X && (u.\u0002(AccessModeFlags.Write) || u.\u0002(AccessModeFlags.Set) || u.\u0002(AccessModeFlags.Reset)))
					{
						Debug.\u0001(u.CompiledType.BaseType.Size(this.Scope) == 1);
						Debug.\u0001(num2 <= 7);
						IBitWriteAccess bwa = global::\u0019.\u0003.\u0001(this.\u0001.SignatureId, u.Area, u.Address + u.Offset, (byte)num2, \u0002.PositionIntern, \u0002.ToString());
						this.\u0001.AddBitWriteAccess(bwa);
						return;
					}
				}
				else if (ivariable.GetFlag(VarFlag.Absolut))
				{
					if (!this.\u0001(u.PackMode, num))
					{
						this.Codegen.GenerateVarAbsolut(\u0002, u.Area, u.Address + u.Offset, u.IndexInfo, compiledType, num, u.AccessMode);
						return;
					}
					if (this.Codegen12 != null)
					{
						this.Codegen12.GenerateVarAbsolutMisaligned(\u0002, u.Area, u.Address + u.Offset, u.IndexInfo, compiledType, num, u.AccessMode);
						return;
					}
					MethodInfo method = this.Codegen.GetType().GetMethod("GenerateVarAbsolutMisaligned", new Type[]
					{
						typeof(_IVariableExpression),
						typeof(int),
						typeof(int),
						typeof(global::\u0017.\u0011),
						typeof(ICompiledType),
						typeof(int),
						typeof(IAccessMode)
					});
					if (method != null)
					{
						method.Invoke(this.Codegen, new object[]
						{
							\u0002,
							u.Area,
							u.Address + u.Offset,
							u.IndexInfo,
							compiledType,
							num,
							u.AccessMode
						});
						return;
					}
					this.Codegen.GenerateVarAbsolut(\u0002, u.Area, u.Address + u.Offset, u.IndexInfo, compiledType, num, u.AccessMode);
					return;
				}
				else
				{
					int num3 = u.Address + u.Offset;
					IAccessMode accessMode = u.AccessMode;
					bool flag = this.\u0001(CodegeneratorProperties.PositiveStackGrow);
					if (!accessMode.GetAccessMode(AccessModeFlags.Parameter) && ((!flag && num3 >= 0) || (flag && num3 < 0)))
					{
						num3 += this.Codegen.StackDisplacement;
					}
					else if (accessMode.GetAccessMode(AccessModeFlags.Parameter))
					{
						num3 += this.Codegen.ParameterDisplacement;
					}
					if (this.Codegen9 != null)
					{
						bool bMisaligned = this.\u0001(u.PackMode, num);
						this.Codegen9.GenerateVarRelative(\u0002, num3, u.IndexInfo, compiledType, num, u.AccessMode, bMisaligned);
						return;
					}
					this.Codegen.GenerateVarRelative(\u0002, num3, u.IndexInfo, compiledType, num, u.AccessMode);
				}
				return;
			}
			if (ivariable.Initial == null)
			{
				this.Codegen.GenerateVarConstant(\u0002, \u001E.\u000E.\u0001(compiledType.Class));
				return;
			}
			ILiteralValue literal = ((_IExpression)ivariable.Initial).Literal(this.Scope, true);
			this.Codegen.GenerateVarConstant(\u0002, literal);
		}

		// Token: 0x06002499 RID: 9369 RVA: 0x0007DE24 File Offset: 0x0007C024
		private bool \u0001(int \u0002, int \u0003)
		{
			return \u0002 >= 0 && \u0003 > 1;
		}

		// Token: 0x0600249A RID: 9370 RVA: 0x0007DE30 File Offset: 0x0007C030
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			if (\u0002.Info == null || \u0002.Info.DoGeneration)
			{
				Debug.\u0001(false, "CodeAdapter - _IIndexAccessExpression");
				return;
			}
			this.\u0001(\u0002._Var);
		}

		// Token: 0x0600249B RID: 9371 RVA: 0x0007DE60 File Offset: 0x0007C060
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			global::\u0003.\u000F u000F = \u0002.Info as global::\u0003.\u000F;
			Debug.\u0001(u000F != null && !u000F.DoGeneration);
			if (u000F.GenerateRight)
			{
				this.\u0001(\u0002._Right);
				return;
			}
			this.\u0001(\u0002._Left);
			if (this.\u0003)
			{
				_IVariableExpression ivariableExpression = \u0002.Left as _IVariableExpression;
				if (ivariableExpression != null)
				{
					IDataLocation dataLocation = \u0002.DataLocation(this.Scope);
					if (dataLocation != null)
					{
						\u001D.\u0007 u = ivariableExpression.VarInfo as \u001D.\u0007;
						if (dataLocation.IsBitLocation && dataLocation.Area != 65535 && (u.\u0002(AccessModeFlags.Write) || u.\u0002(AccessModeFlags.Set) || u.\u0002(AccessModeFlags.Reset)))
						{
							Debug.\u0001(u.CompiledType.BaseType.Size(this.Scope) == 1);
							IBitWriteAccess bwa = global::\u0019.\u0003.\u0001(this.\u0001.SignatureId, (int)dataLocation.Area, dataLocation.Offset, dataLocation.BitNr, \u0002.PositionIntern, \u0002.ToString());
							this.\u0001.AddBitWriteAccess(bwa);
						}
					}
				}
			}
		}

		// Token: 0x0600249C RID: 9372 RVA: 0x0007DF7C File Offset: 0x0007C17C
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			global::\u0014.\u000F u000F = \u0002.DeRefInfo as global::\u0014.\u000F;
			int num = -1;
			if (u000F.IsBit)
			{
				num = (int)u000F.BitNr;
			}
			if (u000F.CompiledType.Class == TypeClass.Subrange || u000F.CompiledType.Class == TypeClass.Enum)
			{
				u000F.CompiledType = u000F.CompiledType.DeRefType;
			}
			ICodeGeneratorAttributes codeGeneratorAttributes = this.\u0001(\u0002);
			if (codeGeneratorAttributes != null && codeGeneratorAttributes.OverwriteAM != AccessModeFlags.Unknown)
			{
				u000F.AccessMode.SetAccessMode((AccessModeFlags)4294967295U, false);
				u000F.AccessMode.SetAccessMode(codeGeneratorAttributes.OverwriteAM, true);
				if (u000F.AccessMode.GetAccessMode(AccessModeFlags.WriteAddress) || u000F.AccessMode.GetAccessMode(AccessModeFlags.ReadAddress))
				{
					u000F.CompiledType = global::\u0019.\u0003.\u0001(u000F.CompiledType as _IType);
				}
			}
			bool accessMode = u000F.AccessMode.GetAccessMode(AccessModeFlags.ReadAddress);
			if (num != -1 && !accessMode)
			{
				u000F.BitNr = byte.MaxValue;
				global::\u0011.\u0008 u = u000F._AccessMode.\u0001() as global::\u0011.\u0008;
				u000F.AccessMode.SetAccessMode((AccessModeFlags)4294967295U, false);
				u000F.AccessMode.SetAccessMode(AccessModeFlags.Read, true);
				this.Codegen.GenerateBitAccess(\u0002, u000F.CompiledType, u, num);
				u000F.BitNr = (byte)num;
				u000F._AccessMode = u;
				return;
			}
			this.\u0001(\u0002, u000F);
		}

		// Token: 0x0600249D RID: 9373 RVA: 0x0007E0B4 File Offset: 0x0007C2B4
		private void \u0001(_IDeRefAccessExpression \u0002, global::\u0014.\u000F \u0003)
		{
			this.\u0001(\u0003);
			if (\u0003.InstanceAccess)
			{
				this.\u0002(\u0002, \u0003);
				return;
			}
			ICompiledType compiledType = this.\u0001(\u0003.CompiledType);
			if (this.Codegen7 != null)
			{
				int u = compiledType.Size(this.Scope);
				bool bMisaligned = this.\u0001(\u0003.PackMode, u);
				this.Codegen7.Generate(\u0002, \u0003.Offset, \u0003.IndexInfo, compiledType, \u0003.AccessMode, bMisaligned);
				return;
			}
			this.Codegen.Generate(\u0002, \u0003.Offset, \u0003.IndexInfo, compiledType, \u0003.AccessMode);
		}

		// Token: 0x0600249E RID: 9374 RVA: 0x0007E14C File Offset: 0x0007C34C
		private void \u0001(global::\u0014.\u000F \u0002)
		{
			TypeClass @class = \u0002.CompiledType.Class;
			if (@class <= TypeClass.Array)
			{
				if (@class - TypeClass.String > 1 && @class != TypeClass.Array)
				{
					return;
				}
			}
			else if (@class != TypeClass.Userdef && @class != TypeClass.__Vector)
			{
				return;
			}
			if (\u0002.CompiledType.Class != TypeClass.__Vector || this.\u0004)
			{
				if (\u0084.\u0004.\u0001(\u0002.CompiledType, this.Scope, this.Codegen) && this.TopOfStack_ForceScalarType)
				{
					if ((this.Codegen as ICodegenerator3).RegisterSize == 8)
					{
						\u0002.CompiledType = TypeTable.ULInt;
						return;
					}
					\u0002.CompiledType = TypeTable.UDInt;
					return;
				}
				else
				{
					if (\u0002.AccessMode.GetAccessMode(AccessModeFlags.ReadAddress) || \u0002.AccessMode.GetAccessMode(AccessModeFlags.WriteAddress))
					{
						\u0002.CompiledType = global::\u0019.\u0003.\u0001(\u0002.CompiledType as _IType);
						return;
					}
					if (\u0002.AccessMode.HasAccessMode(AccessModeFlags.Read | AccessModeFlags.Write))
					{
						\u0002.AccessMode.SetAccessMode(AccessModeFlags.Write, false);
						\u0002.AccessMode.SetAccessMode(AccessModeFlags.ReadAddress, true);
						\u0002.CompiledType = global::\u0019.\u0003.\u0001(\u0002.CompiledType as _IType);
					}
				}
			}
		}

		// Token: 0x0600249F RID: 9375 RVA: 0x0007E25C File Offset: 0x0007C45C
		private void \u0002(_IDeRefAccessExpression \u0002, global::\u0014.\u000F \u0003)
		{
			if (\u0003.CompiledType.Class == TypeClass.Subrange || \u0003.CompiledType.Class == TypeClass.Enum)
			{
				ICompiledType compiledType = this.\u0001(\u0003.CompiledType.DeRefType);
				if (this.Codegen7 != null)
				{
					int u = compiledType.Size(this.Scope);
					bool bMisaligned = this.\u0001(\u0003.PackMode, u);
					this.Codegen7.GenerateInstanceAccess(\u0002, \u0003.Offset, \u0003.IndexInfo, compiledType, \u0003.AccessMode, bMisaligned);
					return;
				}
				this.Codegen.GenerateInstanceAccess(\u0002, \u0003.Offset, \u0003.IndexInfo, compiledType, \u0003.AccessMode);
				return;
			}
			else
			{
				ICompiledType compiledType2 = this.\u0001(\u0003.CompiledType);
				if (this.Codegen7 != null)
				{
					int u2 = compiledType2.Size(this.Scope);
					bool bMisaligned2 = this.\u0001(\u0003.PackMode, u2);
					this.Codegen7.GenerateInstanceAccess(\u0002, \u0003.Offset, \u0003.IndexInfo, compiledType2, \u0003.AccessMode, bMisaligned2);
					return;
				}
				this.Codegen.GenerateInstanceAccess(\u0002, \u0003.Offset, \u0003.IndexInfo, compiledType2, \u0003.AccessMode);
				return;
			}
		}

		// Token: 0x060024A0 RID: 9376 RVA: 0x0007E378 File Offset: 0x0007C578
		private ICompiledType \u0001(ICompiledType \u0002)
		{
			if (\u0002 is ISpecialSizeType)
			{
				return (\u0002 as ISpecialSizeType).CodegeneratorType;
			}
			return \u0002;
		}

		// Token: 0x060024A1 RID: 9377 RVA: 0x0007E390 File Offset: 0x0007C590
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			IScope5 u = this.Scope;
			this.Scope = (this.Scope as global::\u0007.\u0005)._CopyScope;
			this.\u0001(\u0002._Base);
			this.Scope = u;
		}

		// Token: 0x060024A2 RID: 9378 RVA: 0x0007E3D0 File Offset: 0x0007C5D0
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			this.\u0001(\u0002._Base);
		}

		// Token: 0x060024A3 RID: 9379 RVA: 0x0007E3E0 File Offset: 0x0007C5E0
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			this.\u0001(\u0002._Base);
		}

		// Token: 0x060024A4 RID: 9380 RVA: 0x0007E3F0 File Offset: 0x0007C5F0
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			this.\u0001(\u0002._Base);
		}

		// Token: 0x060024A5 RID: 9381 RVA: 0x0007E400 File Offset: 0x0007C600
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			this.\u0001(\u0002._Access);
		}

		// Token: 0x060024A6 RID: 9382 RVA: 0x0007E410 File Offset: 0x0007C610
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			this.\u0001(\u0002._Base);
		}

		// Token: 0x060024A7 RID: 9383 RVA: 0x0007E420 File Offset: 0x0007C620
		public void \u0001(_IEmptyStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x060024A8 RID: 9384 RVA: 0x0007E430 File Offset: 0x0007C630
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x060024A9 RID: 9385 RVA: 0x0007E440 File Offset: 0x0007C640
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x060024AA RID: 9386 RVA: 0x0007E450 File Offset: 0x0007C650
		public void \u0001(_ICaseStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x060024AB RID: 9387 RVA: 0x0007E460 File Offset: 0x0007C660
		public void \u0001(_IBreakPointStatement \u0002)
		{
			this.Codegen.Generate(\u0002);
		}

		// Token: 0x060024AC RID: 9388 RVA: 0x0007E470 File Offset: 0x0007C670
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x060024AD RID: 9389 RVA: 0x0007E474 File Offset: 0x0007C674
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x060024AE RID: 9390 RVA: 0x0007E478 File Offset: 0x0007C678
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x060024AF RID: 9391 RVA: 0x0007E47C File Offset: 0x0007C67C
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x060024B0 RID: 9392 RVA: 0x0007E480 File Offset: 0x0007C680
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
		}

		// Token: 0x060024B1 RID: 9393 RVA: 0x0007E484 File Offset: 0x0007C684
		public void \u0001(_IArrayInitialization \u0002)
		{
		}

		// Token: 0x060024B2 RID: 9394 RVA: 0x0007E488 File Offset: 0x0007C688
		public void \u0001(_IStructureInitialization \u0002)
		{
		}

		// Token: 0x060024B3 RID: 9395 RVA: 0x0007E48C File Offset: 0x0007C68C
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x060024B4 RID: 9396 RVA: 0x0007E490 File Offset: 0x0007C690
		public void \u0001(_IVariableReference \u0002)
		{
		}

		// Token: 0x060024B5 RID: 9397 RVA: 0x0007E494 File Offset: 0x0007C694
		public void \u0001(_ITypeReference \u0002)
		{
		}

		// Token: 0x060024B6 RID: 9398 RVA: 0x0007E498 File Offset: 0x0007C698
		public void \u0001(_IPouReference \u0002)
		{
		}

		// Token: 0x060024B7 RID: 9399 RVA: 0x0007E49C File Offset: 0x0007C69C
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x060024B8 RID: 9400 RVA: 0x0007E4A0 File Offset: 0x0007C6A0
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x060024B9 RID: 9401 RVA: 0x0007E4A4 File Offset: 0x0007C6A4
		public void \u0001(_IDefinedExpression \u0002)
		{
		}

		// Token: 0x060024BA RID: 9402 RVA: 0x0007E4A8 File Offset: 0x0007C6A8
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x060024BB RID: 9403 RVA: 0x0007E4AC File Offset: 0x0007C6AC
		public void \u0001(_IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x060024BC RID: 9404 RVA: 0x0007E4B0 File Offset: 0x0007C6B0
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x060024BD RID: 9405 RVA: 0x0007E4B4 File Offset: 0x0007C6B4
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x060024BE RID: 9406 RVA: 0x0007E4B8 File Offset: 0x0007C6B8
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x060024BF RID: 9407 RVA: 0x0007E4BC File Offset: 0x0007C6BC
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x060024C0 RID: 9408 RVA: 0x0007E4C0 File Offset: 0x0007C6C0
		public void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x060024C1 RID: 9409 RVA: 0x0007E4C4 File Offset: 0x0007C6C4
		public void \u0001(_IHasTypeExpression \u0002)
		{
		}

		// Token: 0x060024C2 RID: 9410 RVA: 0x0007E4C8 File Offset: 0x0007C6C8
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x060024C3 RID: 9411 RVA: 0x0007E4CC File Offset: 0x0007C6CC
		public void \u0001(_IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x060024C4 RID: 9412 RVA: 0x0007E4D0 File Offset: 0x0007C6D0
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x060024C5 RID: 9413 RVA: 0x0007E4D4 File Offset: 0x0007C6D4
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x060024C6 RID: 9414 RVA: 0x0007E4D8 File Offset: 0x0007C6D8
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
		}

		// Token: 0x060024C7 RID: 9415 RVA: 0x0007E4DC File Offset: 0x0007C6DC
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
		}

		// Token: 0x060024C8 RID: 9416 RVA: 0x0007E4EC File Offset: 0x0007C6EC
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
		}

		// Token: 0x060024C9 RID: 9417 RVA: 0x0007E4F0 File Offset: 0x0007C6F0
		public IBreakpoint \u0001(IExprement \u0002, int \u0003, byte \u0004)
		{
			short sTryCatchId = -1;
			if (this.\u0005)
			{
				sTryCatchId = this.\u0001;
			}
			IBreakpoint breakpoint = (\u0002 as _IExprement2).CreateBreakpoint(\u0003, \u0004, sTryCatchId);
			if (!this.\u0001.ContainsKey(\u0002))
			{
				this.\u0001.Add(\u0002, breakpoint);
			}
			return breakpoint;
		}

		// Token: 0x060024CA RID: 9418 RVA: 0x0007E53C File Offset: 0x0007C73C
		public IBreakpoint \u0001(IExprement \u0002)
		{
			IBreakpoint result = null;
			this.\u0001.TryGetValue(\u0002, ref result);
			return result;
		}

		// Token: 0x060024CB RID: 9419 RVA: 0x0007E55C File Offset: 0x0007C75C
		public void \u0001(IExprement \u0002, ICodeGeneratorAttributes \u0003)
		{
			if (!this.\u0001.ContainsKey(\u0002) && \u0003 != null)
			{
				this.\u0001.Add(\u0002, \u0003);
			}
		}

		// Token: 0x060024CC RID: 9420 RVA: 0x0007E57C File Offset: 0x0007C77C
		public ICodeGeneratorAttributes \u0001(IExprement \u0002)
		{
			ICodeGeneratorAttributes result = null;
			this.\u0001.TryGetValue(\u0002, ref result);
			return result;
		}

		// Token: 0x060024CD RID: 9421 RVA: 0x0007E59C File Offset: 0x0007C79C
		public void \u0002()
		{
			this.\u0001.Clear();
			this.\u0001.Clear();
		}

		// Token: 0x060024CE RID: 9422 RVA: 0x0007E5B4 File Offset: 0x0007C7B4
		private void \u0001(_ICompileContext \u0002, List<IArea> \u0003)
		{
			if (\u0002.ParentContext != null)
			{
				this.\u0001(\u0002.ParentContext, \u0003);
			}
			IArea[] areas = \u0002.DataManager.Areas;
			int i = 0;
			while (i < areas.Length)
			{
				_IArea iarea = (_IArea)areas[i];
				if (!iarea.Dynamic || !iarea.GetAreaFlag(AreaFlags.OnlineChange))
				{
					goto IL_8D;
				}
				_IDataSegment idataSegment = null;
				foreach (_IDataSegment idataSegment2 in \u0002.DataManager.AreaSegments)
				{
					if ((int)idataSegment2.Area == iarea.Index)
					{
						idataSegment = idataSegment2;
						break;
					}
				}
				Debug.\u0001(idataSegment != null);
				if (idataSegment.SizeAllocated != 0)
				{
					goto IL_8D;
				}
				IL_94:
				i++;
				continue;
				IL_8D:
				\u0003.Add(iarea);
				goto IL_94;
			}
		}

		// Token: 0x060024CF RID: 9423 RVA: 0x0007E664 File Offset: 0x0007C864
		public IArea[] \u0001()
		{
			List<IArea> list = new List<IArea>();
			this.\u0001(this.\u0001, list);
			return list.ToArray();
		}

		// Token: 0x04000675 RID: 1653
		private ICodegenerator \u0001;

		// Token: 0x04000676 RID: 1654
		private int \u0001;

		// Token: 0x04000677 RID: 1655
		private readonly bool \u0001;

		// Token: 0x04000678 RID: 1656
		private readonly Stack<IExprement> \u0001 = new Stack<IExprement>();

		// Token: 0x04000679 RID: 1657
		private _ICompiledPOU \u0001;

		// Token: 0x0400067A RID: 1658
		private readonly IDataManager3 \u0001;

		// Token: 0x0400067B RID: 1659
		private readonly IScanner \u0001 = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner();

		// Token: 0x0400067C RID: 1660
		private readonly _ICompileContext \u0001;

		// Token: 0x0400067D RID: 1661
		private readonly bool \u0002;

		// Token: 0x0400067E RID: 1662
		private bool \u0003;

		// Token: 0x0400067F RID: 1663
		private bool \u0004 = true;

		// Token: 0x04000680 RID: 1664
		private readonly \u007F.\u000F \u0001 = new \u007F.\u000F();

		// Token: 0x04000681 RID: 1665
		private readonly LDictionary<IExprement, IBreakpoint> \u0001 = new LDictionary<IExprement, IBreakpoint>();

		// Token: 0x04000682 RID: 1666
		private readonly LDictionary<IExprement, ICodeGeneratorAttributes> \u0001 = new LDictionary<IExprement, ICodeGeneratorAttributes>();

		// Token: 0x04000683 RID: 1667
		private readonly Stack<bool> \u0001 = new Stack<bool>();

		// Token: 0x04000684 RID: 1668
		private bool \u0005;

		// Token: 0x04000685 RID: 1669
		private short \u0001;

		// Token: 0x04000686 RID: 1670
		[CompilerGenerated]
		private IScope5 \u0001;

		// Token: 0x04000687 RID: 1671
		[CompilerGenerated]
		private readonly ITargetSettings \u0001;

		// Token: 0x02000227 RID: 551
		private sealed class \u0001 : IJumpTable
		{
			// Token: 0x060024D0 RID: 9424 RVA: 0x0007E68C File Offset: 0x0007C88C
			public \u0001(long \u0003\u0005)
			{
				this.Start = \u0003\u0005;
			}

			// Token: 0x060024D1 RID: 9425 RVA: 0x0007E6A8 File Offset: 0x0007C8A8
			public void \u0001(\u001E.\u000E.\u0001.\u0001 \u0002)
			{
				this.End = \u0002.Value;
				this.\u0001.Add(\u0002);
			}

			// Token: 0x170006AA RID: 1706
			public IExpression this[int \u0002]
			{
				get
				{
					return this.\u0001[\u0002].Label;
				}
			}

			// Token: 0x170006AB RID: 1707
			// (get) Token: 0x060024D3 RID: 9427 RVA: 0x0007E6D8 File Offset: 0x0007C8D8
			public long Start { get; }

			// Token: 0x170006AC RID: 1708
			// (get) Token: 0x060024D4 RID: 9428 RVA: 0x0007E6E0 File Offset: 0x0007C8E0
			// (set) Token: 0x060024D5 RID: 9429 RVA: 0x0007E6E8 File Offset: 0x0007C8E8
			public long End { get; private set; }

			// Token: 0x170006AD RID: 1709
			// (get) Token: 0x060024D6 RID: 9430 RVA: 0x0007E6F4 File Offset: 0x0007C8F4
			public int Count
			{
				get
				{
					return this.\u0001.Count;
				}
			}

			// Token: 0x04000688 RID: 1672
			private readonly LList<\u001E.\u000E.\u0001.\u0001> \u0001 = new LList<\u001E.\u000E.\u0001.\u0001>();

			// Token: 0x04000689 RID: 1673
			[CompilerGenerated]
			private readonly long \u0001;

			// Token: 0x0400068A RID: 1674
			[CompilerGenerated]
			private long \u0002;

			// Token: 0x02000228 RID: 552
			internal sealed class \u0001
			{
				// Token: 0x060024D7 RID: 9431 RVA: 0x0007E704 File Offset: 0x0007C904
				public \u0001(long \u0011\u0005, IExpression \u0012\u0005)
				{
					this.Value = \u0011\u0005;
					this.Label = \u0012\u0005;
				}

				// Token: 0x170006AE RID: 1710
				// (get) Token: 0x060024D8 RID: 9432 RVA: 0x0007E71C File Offset: 0x0007C91C
				public long Value { get; }

				// Token: 0x170006AF RID: 1711
				// (get) Token: 0x060024D9 RID: 9433 RVA: 0x0007E724 File Offset: 0x0007C924
				public IExpression Label { get; }

				// Token: 0x0400068B RID: 1675
				[CompilerGenerated]
				private readonly long \u0001;

				// Token: 0x0400068C RID: 1676
				[CompilerGenerated]
				private readonly IExpression \u0001;
			}

			// Token: 0x02000229 RID: 553
			internal sealed class \u0002 : IComparer<\u001E.\u000E.\u0001.\u0001>
			{
				// Token: 0x060024DA RID: 9434 RVA: 0x0007E72C File Offset: 0x0007C92C
				public int \u0001(\u001E.\u000E.\u0001.\u0001 \u0002, \u001E.\u000E.\u0001.\u0001 \u0003)
				{
					if (\u0002.Value > \u0003.Value)
					{
						return 1;
					}
					if (\u0002.Value < \u0003.Value)
					{
						return -1;
					}
					return 0;
				}
			}
		}
	}
}
