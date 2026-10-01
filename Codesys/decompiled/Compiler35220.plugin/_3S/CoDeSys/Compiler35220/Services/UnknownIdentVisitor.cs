using System;
using System.Collections.Generic;
using System.Linq;
using \u0002;
using \u0005;
using \u0011;
using \u0015;
using \u0016;
using \u0017;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x02000105 RID: 261
	internal sealed class UnknownIdentVisitor : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x06001355 RID: 4949 RVA: 0x00035388 File Offset: 0x00033588
		public UnknownIdentVisitor(IPrecompileScope scope, \u0018.\u0001[] resolvedLazies)
		{
			this.\u0001(null, scope, AccessFlag.Unknown, VarFlag.Local);
			this.\u0001 = false;
			this.\u0001 = resolvedLazies;
		}

		// Token: 0x06001356 RID: 4950 RVA: 0x000353C0 File Offset: 0x000335C0
		public UnknownIdentVisitor(IPrecompileScope scope, bool bSearchLazies, _ICompileContext comcon, LList<IVariable> varLazies)
		{
			this.\u0001(null, scope, AccessFlag.Unknown, VarFlag.Local);
			this.\u0001 = bSearchLazies;
			this.\u0001 = comcon;
			if (varLazies != null)
			{
				this.\u0001 = new LDictionary<string, string>();
				foreach (IVariable variable in varLazies)
				{
					this.\u0001[variable.Name] = variable.Name;
				}
			}
		}

		// Token: 0x06001357 RID: 4951 RVA: 0x00035460 File Offset: 0x00033660
		internal void \u0001(IPrecompileScope2 \u0002)
		{
			this.TopOfStack.Scope = \u0002;
		}

		// Token: 0x06001358 RID: 4952 RVA: 0x00035470 File Offset: 0x00033670
		internal static IType \u0001(IOperatorExpression \u0002, IPrecompileScope \u0003, IType \u0004)
		{
			ILiteralValue literalValue = \u0002.Literal(\u0003);
			IType result;
			if (literalValue != null)
			{
				if (literalValue.UnsignedLong > (ulong)-1)
				{
					result = TypeTable.ULInt;
				}
				else if (literalValue.UnsignedLong > 65535UL)
				{
					result = TypeTable.UDInt;
				}
				else if (literalValue.UnsignedLong > 255UL)
				{
					result = TypeTable.UInt;
				}
				else
				{
					result = TypeTable.USInt;
				}
			}
			else if (\u0004 != null && TypeTable.IsConcreteType(\u0004.Class))
			{
				result = \u0004;
			}
			else
			{
				result = TypeTable.UInt;
			}
			return result;
		}

		// Token: 0x06001359 RID: 4953 RVA: 0x000354EC File Offset: 0x000336EC
		private void \u0001(IType \u0002, IPrecompileScope \u0003, AccessFlag \u0004, VarFlag \u0005)
		{
			global::\u0017.\u0005 u = new global::\u0017.\u0005();
			u.DerivedType = \u0002;
			u.Scope = (\u0003 as IPrecompileScope2);
			u.DerivedScope = null;
			u.Access = \u0004;
			u.VarFlag = \u0005;
			this.\u0001.Push(u);
		}

		// Token: 0x0600135A RID: 4954 RVA: 0x00035534 File Offset: 0x00033734
		private void \u0001()
		{
			this.\u0001.Pop();
		}

		// Token: 0x170004D0 RID: 1232
		// (get) Token: 0x0600135B RID: 4955 RVA: 0x00035544 File Offset: 0x00033744
		public IType DerivedType
		{
			get
			{
				if (this.\u0001 == null)
				{
					return null;
				}
				return this.TopOfStack.DerivedTypeNoDeref;
			}
		}

		// Token: 0x170004D1 RID: 1233
		// (get) Token: 0x0600135C RID: 4956 RVA: 0x0003555C File Offset: 0x0003375C
		public ISignature DerivedSignature
		{
			get
			{
				if (this.\u0001 == null)
				{
					return null;
				}
				return this.TopOfStack.CurrentSignature;
			}
		}

		// Token: 0x170004D2 RID: 1234
		// (get) Token: 0x0600135D RID: 4957 RVA: 0x00035574 File Offset: 0x00033774
		public IVariable DerivedVariable
		{
			get
			{
				if (this.\u0001 == null)
				{
					return null;
				}
				return this.TopOfStack.CurrentVariable;
			}
		}

		// Token: 0x170004D3 RID: 1235
		// (get) Token: 0x0600135E RID: 4958 RVA: 0x0003558C File Offset: 0x0003378C
		public IPrecompileScope DerivedScope
		{
			get
			{
				if (this.\u0001 == null)
				{
					return null;
				}
				return this.TopOfStack.DerivedScope;
			}
		}

		// Token: 0x170004D4 RID: 1236
		// (get) Token: 0x0600135F RID: 4959 RVA: 0x000355A4 File Offset: 0x000337A4
		public \u0018.\u0001[] DeclarationInfo
		{
			get
			{
				\u0018.\u0001[] array = new \u0018.\u0001[this.\u0001.Count];
				if (!this.\u0001)
				{
					foreach (\u0018.\u0001 u in this.\u0001)
					{
						if (!this.\u0001(u.DerivedType))
						{
							u.DerivedType = this.\u0001(u.DerivedType);
						}
					}
				}
				this.\u0001.CopyTo(array);
				return array;
			}
		}

		// Token: 0x170004D5 RID: 1237
		// (get) Token: 0x06001360 RID: 4960 RVA: 0x00035630 File Offset: 0x00033830
		private global::\u0017.\u0005 TopOfStack
		{
			get
			{
				return this.\u0001.Peek();
			}
		}

		// Token: 0x170004D6 RID: 1238
		// (get) Token: 0x06001361 RID: 4961 RVA: 0x00035640 File Offset: 0x00033840
		private IType TopOfStack_DerivedType
		{
			get
			{
				IType result = this.TopOfStack.DerivedType;
				IType type = this.TopOfStack.DerivedType;
				_IArrayType iarrayType = type as _IArrayType;
				if (iarrayType != null)
				{
					type = iarrayType.Base;
				}
				if (type is _IUserdefType)
				{
					ISignature signature = this.TopOfStack.Scope.FindSignatureGlobal((type as _IUserdefType).NameExpression);
					if (signature == null)
					{
						goto IL_181;
					}
					if (signature.GetFlag(SignatureFlag.Alias) && signature.All.Length != 0)
					{
						IType type2 = signature.All[0].Type;
						if (type2 == null)
						{
							return null;
						}
						if (type2.Class != TypeClass.Userdef)
						{
							if (iarrayType != null)
							{
								iarrayType = (iarrayType.Duplicate as _IArrayType);
								iarrayType._Base = (type2 as _IType);
								type2 = iarrayType;
							}
							return type2;
						}
						if (!string.IsNullOrEmpty(signature.LibraryPath))
						{
							try
							{
								if ((type as _IUserdefType).NameExpression is ICompoAccessExpression)
								{
									type = this.\u0003(signature, type2);
								}
								goto IL_181;
							}
							catch
							{
								goto IL_181;
							}
						}
						this.TopOfStack.DerivedType = type2;
						type = this.TopOfStack_DerivedType;
						goto IL_181;
					}
					else
					{
						if (string.IsNullOrEmpty(signature.LibraryPath))
						{
							goto IL_181;
						}
						try
						{
							if ((type as _IUserdefType).NameExpression is IVariableExpression)
							{
								type = this.\u0003(signature, type);
							}
							goto IL_181;
						}
						catch
						{
							goto IL_181;
						}
					}
				}
				if (type is _IEnumType)
				{
					ISignature signature2 = this.TopOfStack.Scope.FindSignatureGlobal((type as _IEnumType).Name);
					if (signature2 != null)
					{
						if (!string.IsNullOrEmpty(signature2.LibraryPath))
						{
							type = this.\u0003(signature2, type);
						}
						if (type is _IEnumType)
						{
							type = \u0019.\u0003.\u0001(signature2.Name);
						}
					}
				}
				IL_181:
				if (iarrayType != null)
				{
					iarrayType = (iarrayType.Duplicate as _IArrayType);
					iarrayType._Base = (type as _IType);
				}
				else
				{
					result = type;
				}
				return result;
			}
		}

		// Token: 0x06001362 RID: 4962 RVA: 0x0003580C File Offset: 0x00033A0C
		public void \u0001(_ICompiledPOU \u0002)
		{
			\u0002.GetParseTree().Accept(this);
		}

		// Token: 0x06001363 RID: 4963 RVA: 0x0003581C File Offset: 0x00033A1C
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001(TypeTable.Bool, this.TopOfStack.Scope, AccessFlag.Write, this.TopOfStack.VarFlag);
			\u0002._Condition.Accept(this);
			this.\u0001();
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06001364 RID: 4964 RVA: 0x0003586C File Offset: 0x00033A6C
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
			this.\u0001(TypeTable.Bool, this.TopOfStack.Scope, AccessFlag.Write, this.TopOfStack.VarFlag);
			\u0002._Condition.Accept(this);
			this.\u0001();
		}

		// Token: 0x06001365 RID: 4965 RVA: 0x000358BC File Offset: 0x00033ABC
		public void \u0001(_IForStatement \u0002)
		{
			\u0002._CounterStart.Accept(this);
			this.\u0001(TypeTable.Int, this.TopOfStack.Scope, AccessFlag.Write, this.TopOfStack.VarFlag);
			\u0002._UpperBound.Accept(this);
			this.\u0001();
			this.\u0001(TypeTable.Int, this.TopOfStack.Scope, AccessFlag.Write, this.TopOfStack.VarFlag);
			\u0002._By.Accept(this);
			this.\u0001();
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06001366 RID: 4966 RVA: 0x0003594C File Offset: 0x00033B4C
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x06001367 RID: 4967 RVA: 0x00035950 File Offset: 0x00033B50
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06001368 RID: 4968 RVA: 0x00035954 File Offset: 0x00033B54
		public void \u0001(_ISequenceStatement \u0002)
		{
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				try
				{
					this.\u0001(TypeTable.Any, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.None);
					if (this.\u0001 && this.\u0001 != null)
					{
						if (istatement is _IPragmaStatement)
						{
							istatement.Accept(this);
						}
						if (!this.\u0002 && global::\u0002.\u0004.\u0001(istatement, this.\u0001, true))
						{
							istatement.Accept(this);
						}
					}
					else
					{
						istatement.Accept(this);
					}
					this.\u0001();
				}
				catch
				{
				}
			}
		}

		// Token: 0x06001369 RID: 4969 RVA: 0x00035A14 File Offset: 0x00033C14
		private void \u0001(IExpression \u0002, IType \u0003, out long \u0004, out long \u0005)
		{
			\u0005 = 0L;
			\u0004 = 0L;
			if (\u0002 is _ILiteralExpression)
			{
				\u0005 = Math.Max(\u0005, ((_ILiteralExpression)\u0002).LongValue);
				\u0004 = Math.Min(\u0004, ((_ILiteralExpression)\u0002).LongValue);
				return;
			}
			if (\u0003 is _IRangeAwareAnyIntType)
			{
				\u0005 = ((_IRangeAwareAnyIntType)\u0003).MaxValue;
				\u0004 = ((_IRangeAwareAnyIntType)\u0003).MinValue;
			}
		}

		// Token: 0x0600136A RID: 4970 RVA: 0x00035A80 File Offset: 0x00033C80
		public void \u0001(_IAssignmentExpression \u0002)
		{
			this.\u0001(this.TopOfStack_DerivedType, this.TopOfStack.Scope, AccessFlag.Write, this.TopOfStack.VarFlag);
			int count = this.\u0001.Count;
			\u0002._LValue.Accept(this);
			int count2 = this.\u0001.Count;
			IType type = this.TopOfStack_DerivedType;
			this.\u0001();
			this.\u0001(type, this.TopOfStack.Scope, AccessFlag.Read, this.TopOfStack.VarFlag);
			\u0002._RValue.Accept(this);
			IType type2 = this.TopOfStack_DerivedType;
			this.\u0001();
			IType type3;
			if (this.\u0001(type, type2) || (!this.\u0001(type) && this.\u0001(type2)))
			{
				this.\u0001.RemoveRange(count, count2 - count);
				this.\u0001(type2, this.TopOfStack.Scope, AccessFlag.Read, this.TopOfStack.VarFlag);
				\u0002._LValue.Accept(this);
				this.\u0001();
				type3 = type2;
			}
			else
			{
				type3 = type;
			}
			if (type3 is _IRangeAwareAnyIntType)
			{
				long val;
				long val2;
				this.\u0001(\u0002.LValue, type, out val, out val2);
				long val3;
				long val4;
				this.\u0001(\u0002.RValue, type2, out val3, out val4);
				long minValue = Math.Min(val, val3);
				long maxValue = Math.Max(val2, val4);
				((_IRangeAwareAnyIntType)type3).MinValue = minValue;
				((_IRangeAwareAnyIntType)type3).MaxValue = maxValue;
			}
			this.TopOfStack.DerivedType = type3;
		}

		// Token: 0x0600136B RID: 4971 RVA: 0x00035BE8 File Offset: 0x00033DE8
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001(TypeTable.Bool, this.TopOfStack.Scope, AccessFlag.Write, this.TopOfStack.VarFlag);
			\u0002._Condition.Accept(this);
			this.\u0001();
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				this.\u0001(TypeTable.Bool, this.TopOfStack.Scope, AccessFlag.Write, this.TopOfStack.VarFlag);
				ielseIf._Condition.Accept(this);
				this.\u0001();
				ielseIf._Controlled.Accept(this);
			}
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x0600136C RID: 4972 RVA: 0x00035CC0 File Offset: 0x00033EC0
		public void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001(TypeTable.Bool, this.TopOfStack.Scope, AccessFlag.Write, this.TopOfStack.VarFlag);
				\u0002._Condition.Accept(this);
				this.\u0001();
			}
		}

		// Token: 0x0600136D RID: 4973 RVA: 0x00035D00 File Offset: 0x00033F00
		public void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001(TypeTable.Bool, this.TopOfStack.Scope, AccessFlag.Write, this.TopOfStack.VarFlag);
				\u0002._Condition.Accept(this);
				this.\u0001();
			}
		}

		// Token: 0x0600136E RID: 4974 RVA: 0x00035D40 File Offset: 0x00033F40
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x0600136F RID: 4975 RVA: 0x00035D44 File Offset: 0x00033F44
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06001370 RID: 4976 RVA: 0x00035D48 File Offset: 0x00033F48
		public void \u0001(_IPragmaStatement \u0002)
		{
			if (string.IsNullOrEmpty(\u0002.Text))
			{
				return;
			}
			string[] array = \u0002.Text.Split(new char[]
			{
				' '
			});
			if (array != null && array.Length == 2 && array[0] == "lazytypeinference")
			{
				if (array[1] == "off")
				{
					this.\u0002 = true;
					return;
				}
				if (array[1] == "on")
				{
					this.\u0002 = false;
				}
			}
		}

		// Token: 0x06001371 RID: 4977 RVA: 0x00035DC0 File Offset: 0x00033FC0
		public void \u0001(_IExpressionStatement \u0002)
		{
			\u0002._Expr.Accept(this);
		}

		// Token: 0x06001372 RID: 4978 RVA: 0x00035DD0 File Offset: 0x00033FD0
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001373 RID: 4979 RVA: 0x00035DD4 File Offset: 0x00033FD4
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06001374 RID: 4980 RVA: 0x00035DD8 File Offset: 0x00033FD8
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001375 RID: 4981 RVA: 0x00035DDC File Offset: 0x00033FDC
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001376 RID: 4982 RVA: 0x00035DE0 File Offset: 0x00033FE0
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001377 RID: 4983 RVA: 0x00035DE4 File Offset: 0x00033FE4
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06001378 RID: 4984 RVA: 0x00035DE8 File Offset: 0x00033FE8
		public void \u0001(_ICallExpression \u0002)
		{
			this.\u0001(TypeTable.Bool, this.TopOfStack.Scope, AccessFlag.Write, this.TopOfStack.VarFlag);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			this.\u0001();
			this.\u0001(this.TopOfStack_DerivedType, this.TopOfStack.Scope, AccessFlag.Call, VarFlag.Local);
			\u0002._Callee.Accept(this);
			ISignature signature;
			if (this.TopOfStack_DerivedType != null && this.TopOfStack_DerivedType.Class == TypeClass.Userdef)
			{
				_IUserdefType iuserdefType = this.TopOfStack_DerivedType as _IUserdefType;
				signature = this.TopOfStack.Scope.FindSignatureLocal(iuserdefType.NameExpression.ToString());
				if (signature == null)
				{
					signature = this.TopOfStack.Scope.FindSignatureGlobal(iuserdefType.NameExpression);
				}
			}
			else
			{
				signature = this.TopOfStack.CurrentSignature;
			}
			this.\u0001();
			IPrecompileScope u = this.TopOfStack.Scope;
			IList<_IExpression> inputs = \u0002.Inputs;
			IList<_IExpression> outputs = \u0002.Outputs;
			IType[] array = new IType[inputs.Count];
			IType[] array2 = new IType[outputs.Count];
			if (signature != null)
			{
				u = this.TopOfStack.Scope.NewLocalScope(signature);
				for (int i = 0; i < inputs.Count; i++)
				{
					_IExpression iexpression = inputs[i];
					if (iexpression != null)
					{
						this.\u0001(null, u, AccessFlag.Write, VarFlag.Input);
						iexpression.Accept(this);
						array[i] = this.TopOfStack_DerivedType;
						this.\u0001();
						array[i] = this.\u0002(signature, array[i]);
						this.TopOfStack.DerivedType = array[i];
					}
				}
				for (int j = 0; j < outputs.Count; j++)
				{
					_IExpression iexpression2 = outputs[j];
					if (iexpression2 != null)
					{
						this.\u0001(null, u, AccessFlag.Write, VarFlag.Output);
						iexpression2.Accept(this);
						array2[j] = this.TopOfStack_DerivedType;
						this.\u0001();
						array2[j] = this.\u0002(signature, array2[j]);
						this.TopOfStack.DerivedType = array2[j];
					}
				}
			}
			else
			{
				for (int k = 0; k < array.Length; k++)
				{
					array[k] = null;
				}
				for (int l = 0; l < array2.Length; l++)
				{
					array2[l] = null;
				}
			}
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			for (int m = 0; m < paramExpressions.Count; m++)
			{
				_IExpression iexpression3 = paramExpressions[m];
				if (iexpression3 != null)
				{
					IType u2 = null;
					if (m < array.Length)
					{
						u2 = array[m];
					}
					else if (signature != null && signature.Inputs.Length > m)
					{
						u2 = signature.Inputs[m].Type;
					}
					this.\u0001(u2, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
					iexpression3.Accept(this);
					IType type = this.TopOfStack_DerivedType;
					this.\u0001();
					if (type != null && array.Length > m && array[m] == null && inputs.Count > m && signature != null && inputs[m] != null)
					{
						this.\u0001(type, u, AccessFlag.Write, VarFlag.Input);
						inputs[m].Accept(this);
						this.\u0001();
					}
				}
			}
			IList<_IExpression> outputExpressions = \u0002.OutputExpressions;
			for (int n = 0; n < outputExpressions.Count; n++)
			{
				_IExpression iexpression4 = outputExpressions[n];
				if (iexpression4 != null)
				{
					IType u3 = null;
					if (array2.Length > n)
					{
						u3 = array2[n];
					}
					this.\u0001(u3, this.TopOfStack.Scope, AccessFlag.Write, VarFlag.Local);
					iexpression4.Accept(this);
					IType type2 = this.TopOfStack_DerivedType;
					this.\u0001();
					if (type2 != null && array2.Length > n && array2[n] == null && signature != null && outputs.Count > n && outputs[n] != null)
					{
						this.\u0001(type2, u, AccessFlag.Read, VarFlag.Output);
						outputs[n].Accept(this);
						this.\u0001();
					}
				}
			}
			if (signature != null && (signature.POUType == Operator.Function || signature.POUType == Operator.Method))
			{
				IVariable[] outputs2 = signature.Outputs;
				if (outputs2.Length != 0)
				{
					this.TopOfStack.DerivedType = this.\u0002(signature, outputs2[0].Type);
				}
			}
		}

		// Token: 0x06001379 RID: 4985 RVA: 0x000361F8 File Offset: 0x000343F8
		private string \u0001(ISignature \u0002)
		{
			IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0002.LibraryPath);
			if (libraryContext == null)
			{
				return string.Empty;
			}
			IPreCompileContext preCompileContext = null;
			if (this.TopOfStack.Scope.LocalSignature != null)
			{
				preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(this.TopOfStack.Scope.LocalSignature.LibraryPath);
			}
			if (preCompileContext != null && libraryContext != null && string.Equals(preCompileContext.LibraryPath, libraryContext.LibraryPath, StringComparison.OrdinalIgnoreCase))
			{
				return string.Empty;
			}
			string text = null;
			if (this.\u0001 != null)
			{
				if (preCompileContext == null)
				{
					text = Helper.\u0001(this.\u0001, libraryContext as _IPreCompileContext);
				}
				else
				{
					text = Helper.\u0001(this.\u0001, libraryContext as _IPreCompileContext, preCompileContext as _IPreCompileContext, new Dictionary<_IPreCompileContext, _IPreCompileContext>());
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				text = APEnvironmentFacade.Instance.LanguageModelMgr.Pool.GetNameOfLibrary(libraryContext as _IPreCompileContext, this.\u0001);
			}
			if (string.IsNullOrEmpty(text))
			{
				text = libraryContext.Namespace;
			}
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return string.Empty;
		}

		// Token: 0x0600137A RID: 4986 RVA: 0x00036304 File Offset: 0x00034504
		private _IExpression \u0001(ISignature \u0002, _IExpression \u0003, out bool \u0004)
		{
			string text = this.\u0001(\u0002);
			if (string.IsNullOrWhiteSpace(text))
			{
				\u0004 = false;
				return \u0003;
			}
			if (\u0003 is IVariableExpression || \u0003 is ICompoAccessExpression)
			{
				_IExpression result = new global::\u0011.\u0006(text + "." + \u0003.ToString()).\u0002() as _IExpression;
				\u0004 = true;
				return result;
			}
			if (!(\u0003 is IOperatorExpression))
			{
				\u0004 = false;
				return \u0003;
			}
			_IOperatorExpression ioperatorExpression = (_IOperatorExpression)((_IOperatorExpression)\u0003).Duplicate();
			\u0004 = false;
			for (int i = 0; i < ioperatorExpression._OperandsList.Count; i++)
			{
				bool flag;
				_IExpression value = this.\u0001(\u0002, ioperatorExpression._OperandsList[i], out flag);
				ioperatorExpression._OperandsList[i] = value;
				\u0004 = (\u0004 || flag);
			}
			if (\u0004)
			{
				return ioperatorExpression;
			}
			return \u0003;
		}

		// Token: 0x0600137B RID: 4987 RVA: 0x000363C4 File Offset: 0x000345C4
		private IType \u0001(ISignature \u0002, IType \u0003)
		{
			_IArrayType iarrayType = \u0003 as _IArrayType;
			IList<Tuple<_IExpression, _IExpression>> list = new LList<Tuple<_IExpression, _IExpression>>();
			bool flag = false;
			foreach (_IArrayDimension iarrayDimension in iarrayType._Dimensions)
			{
				_IExpression iexpression = iarrayDimension._LowerBorder;
				bool flag2;
				iexpression = this.\u0001(\u0002, iexpression, out flag2);
				flag = (flag || flag2);
				_IExpression iexpression2 = iarrayDimension._UpperBorder;
				iexpression2 = this.\u0001(\u0002, iexpression2, out flag2);
				flag = (flag || flag2);
				list.Add(new Tuple<_IExpression, _IExpression>(iexpression, iexpression2));
			}
			if (!flag)
			{
				return \u0003;
			}
			_IArrayType iarrayType2 = \u0019.\u0003.\u0001();
			iarrayType2._Base = (this.\u0002(\u0002, iarrayType.Base) as _IType);
			foreach (Tuple<_IExpression, _IExpression> tuple in list)
			{
				iarrayType2.AddDimension(tuple.Item1, tuple.Item2);
			}
			return iarrayType2;
		}

		// Token: 0x0600137C RID: 4988 RVA: 0x000364D4 File Offset: 0x000346D4
		private IType \u0002(ISignature \u0002, IType \u0003)
		{
			if (\u0003 == null)
			{
				return null;
			}
			string text = this.\u0001(\u0002);
			if (string.IsNullOrEmpty(text))
			{
				return \u0003;
			}
			TypeClass @class = \u0003.Class;
			if (@class != TypeClass.String)
			{
				if (@class != TypeClass.WString)
				{
					switch (@class)
					{
					case TypeClass.Subrange:
					{
						ISubrangeType2 subrangeType = \u0003 as ISubrangeType2;
						_IExpression iexpression = subrangeType.LowerBorder as _IExpression;
						_IExpression u = subrangeType.UpperBorder as _IExpression;
						bool flag;
						iexpression = this.\u0001(\u0002, iexpression, out flag);
						bool flag2;
						u = this.\u0001(\u0002, u, out flag2);
						if (flag || flag2)
						{
							_ISubrangeType isubrangeType = \u0019.\u0003.\u0001(iexpression, u);
							isubrangeType._Base = (this.\u0002(\u0002, subrangeType.BaseType) as _IType);
							return isubrangeType;
						}
						return \u0003;
					}
					case TypeClass.Array:
						return this.\u0001(\u0002, \u0003);
					case TypeClass.Userdef:
					{
						_ISystemScopeExpression isystemScopeExpression = ((IUserdefType2)\u0003).NameExpression as _ISystemScopeExpression;
						if (isystemScopeExpression != null)
						{
							return \u0019.\u0003.\u0001(isystemScopeExpression);
						}
						return \u0019.\u0003.\u0001(global::\u0011.\u0006.\u0001(text + "." + ((\u0003 != null) ? \u0003.ToString() : null)) as _IExpression);
					}
					}
					return \u0003;
				}
				_IExpression u2 = (\u0003 as IWStringType).LengthExpression as _IExpression;
				bool flag3;
				_IExpression iexpression2 = this.\u0001(\u0002, u2, out flag3);
				if (flag3)
				{
					_IWStringType iwstringType = \u0019.\u0003.\u0001();
					iwstringType.Length = iexpression2;
					return iwstringType;
				}
				return \u0003;
			}
			else
			{
				_IExpression iexpression2 = (\u0003 as IStringType).LengthExpression as _IExpression;
				bool flag4;
				iexpression2 = this.\u0001(\u0002, iexpression2, out flag4);
				if (flag4)
				{
					_IStringType istringType = \u0019.\u0003.\u0001();
					istringType.Length = iexpression2;
					return istringType;
				}
				return \u0003;
			}
		}

		// Token: 0x0600137D RID: 4989 RVA: 0x0003664C File Offset: 0x0003484C
		private IType \u0003(ISignature \u0002, IType \u0003)
		{
			IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0002.LibraryPath);
			IPreCompileContext preCompileContext = null;
			if (this.TopOfStack.Scope.LocalSignature != null)
			{
				preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(this.TopOfStack.Scope.LocalSignature.LibraryPath);
			}
			if (preCompileContext != null && libraryContext != null && string.Equals(preCompileContext.LibraryPath, libraryContext.LibraryPath, StringComparison.OrdinalIgnoreCase))
			{
				return \u0003;
			}
			string text = null;
			if (this.\u0001 != null)
			{
				if (preCompileContext == null)
				{
					text = Helper.\u0001(this.\u0001, libraryContext as _IPreCompileContext);
				}
				else
				{
					text = Helper.\u0001(this.\u0001, libraryContext as _IPreCompileContext, preCompileContext as _IPreCompileContext, new Dictionary<_IPreCompileContext, _IPreCompileContext>());
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				text = APEnvironmentFacade.Instance.LanguageModelMgr.Pool.GetNameOfLibrary(libraryContext as _IPreCompileContext, this.\u0001);
			}
			if (string.IsNullOrEmpty(text))
			{
				text = libraryContext.Namespace;
			}
			if (!string.IsNullOrEmpty(text))
			{
				text = text + "." + ((\u0003 != null) ? \u0003.ToString() : null);
				_IExpression iexpression = new global::\u0011.\u0006(text).\u0002() as _IExpression;
				if (iexpression != null && (iexpression is ICompoAccessExpression || iexpression is IVariableExpression))
				{
					return \u0019.\u0003.\u0001(iexpression);
				}
			}
			return \u0003;
		}

		// Token: 0x170004D7 RID: 1239
		// (get) Token: 0x0600137E RID: 4990 RVA: 0x00036788 File Offset: 0x00034988
		public bool LRealSupported
		{
			get
			{
				if (this.\u0001 != null)
				{
					ITargetSettings targetSettings = this.\u0001.GetTargetSettings();
					return global::\u0016.\u0004.LRealDataType.GetBoolValue(targetSettings);
				}
				return true;
			}
		}

		// Token: 0x170004D8 RID: 1240
		// (get) Token: 0x0600137F RID: 4991 RVA: 0x000367B8 File Offset: 0x000349B8
		public bool Int64Supported
		{
			get
			{
				if (this.\u0001 != null)
				{
					ITargetSettings targetSettings = this.\u0001.GetTargetSettings();
					return global::\u0016.\u0004.LintDataTypes.GetBoolValue(targetSettings);
				}
				return true;
			}
		}

		// Token: 0x06001380 RID: 4992 RVA: 0x000367E8 File Offset: 0x000349E8
		public void \u0001(_IOperatorExpression \u0002)
		{
			IType type = this.TopOfStack_DerivedType;
			IList<_IExpression> operandsList = \u0002._OperandsList;
			int num = 0;
			IType type2 = null;
			Operator code = \u0002.Code;
			if (code <= Operator.__PropertyInfo)
			{
				if (code <= Operator.TruncInt)
				{
					switch (code)
					{
					case Operator.Adr:
						goto IL_357;
					case Operator.BitAdr:
					case Operator.IndexOf:
						goto IL_408;
					case Operator.SizeOf:
						this.TopOfStack.DerivedType = UnknownIdentVisitor.\u0001(\u0002, this.TopOfStack.Scope, type);
						return;
					case Operator.Ini:
						goto IL_917;
					case Operator.Abs:
						for (int i = 0; i < operandsList.Count; i++)
						{
							_IExprement iexprement = operandsList[i];
							this.\u0001(TypeTable.AnyInt, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
							iexprement.Accept(this);
							type2 = this.TopOfStack_DerivedType;
							this.\u0001();
						}
						this.TopOfStack.DerivedType = type2;
						return;
					case Operator.Limit:
					case Operator.Min:
					case Operator.Max:
						goto IL_71A;
					case Operator.Trunc:
						break;
					case Operator.Mux:
						if (operandsList.Count > 0)
						{
							this.\u0001(TypeTable.Int, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
							operandsList[0].Accept(this);
							this.\u0001();
							num = 1;
							goto IL_917;
						}
						goto IL_917;
					case Operator.Sel:
						if (operandsList.Count > 0)
						{
							this.\u0001(TypeTable.Bool, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
							operandsList[0].Accept(this);
							this.\u0001();
							num = 1;
							goto IL_917;
						}
						goto IL_917;
					case Operator.Rol:
					case Operator.Ror:
					case Operator.Shl:
					case Operator.Shr:
						for (int j = 0; j < operandsList.Count; j++)
						{
							_IExprement iexprement2 = operandsList[j];
							if (j == 0)
							{
								this.\u0001(TypeTable.AnyBit, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
							}
							else
							{
								this.\u0001(TypeTable.AnyInt, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
							}
							iexprement2.Accept(this);
							if (j == 0)
							{
								type2 = this.TopOfStack_DerivedType;
							}
							this.\u0001();
						}
						this.TopOfStack.DerivedType = type2;
						return;
					case Operator.Exp:
					case Operator.Expt:
					case Operator.Sqrt:
					case Operator.Ln:
					case Operator.Log:
					case Operator.Sin:
					case Operator.Cos:
					case Operator.Tan:
					case Operator.ASin:
					case Operator.ACos:
					case Operator.ATan:
						for (int k = 0; k < operandsList.Count; k++)
						{
							_IExprement iexprement3 = operandsList[k];
							if (type == null)
							{
								this.\u0001(TypeTable.AnyReal, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
							}
							else
							{
								this.\u0001(type, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
							}
							iexprement3.Accept(this);
							this.\u0001();
						}
						if (this.LRealSupported)
						{
							this.TopOfStack.DerivedType = TypeTable.LReal;
							return;
						}
						this.TopOfStack.DerivedType = TypeTable.Real;
						return;
					default:
						switch (code)
						{
						case Operator.Add:
						case Operator.Mul:
						case Operator.Div:
						case Operator.Mod:
						case Operator.Plus:
						case Operator.Times:
						case Operator.Power:
						case Operator.Divide:
							goto IL_71A;
						case Operator.Sub:
						case Operator.Minus:
							if (operandsList.Count == 2 && operandsList[0] is ILiteralExpression && operandsList[0].Literal(this.TopOfStack.Scope) != null && Helper.\u0001(operandsList[0].Literal(this.TopOfStack.Scope)) == 0L)
							{
								num = 1;
								goto IL_71A;
							}
							goto IL_71A;
						case Operator.And:
						case Operator.AndN:
						case Operator.Or:
						case Operator.OrN:
						case Operator.Xor:
						case Operator.XorN:
						case Operator.Not:
						case Operator.Ampersand:
						case Operator.VerticalLine:
							if (type == null)
							{
								type = TypeTable.AnyBitButBoolIsPreferred;
								goto IL_917;
							}
							goto IL_917;
						case Operator.Eq:
						case Operator.Ne:
						case Operator.Ge:
						case Operator.Gt:
						case Operator.Le:
						case Operator.Lt:
						case Operator.Less:
						case Operator.Greater:
						case Operator.LessEqual:
						case Operator.GreaterEqual:
						case Operator.Equal:
						case Operator.NotEqual:
							type2 = TypeTable.Bool;
							type = TypeTable.AnyNum;
							goto IL_917;
						case Operator.Cal:
						case Operator.CalC:
						case Operator.CalCN:
						case Operator.Jmp:
						case Operator.JmpC:
						case Operator.JmpCN:
						case Operator.Ret:
						case Operator.RetC:
						case Operator.RetCN:
						case Operator.Ld:
						case Operator.LdN:
						case Operator.St:
						case Operator.StN:
						case Operator.R:
						case Operator.S:
						case Operator.Period:
						case Operator.Colon:
						case Operator.Assign:
						case Operator.SetAssign:
						case Operator.ResetAssign:
						case Operator.LeftParenthesis:
						case Operator.RightParenthesis:
						case Operator.LeftBracket:
						case Operator.RightBracket:
						case Operator.Comma:
						case Operator.Semicolon:
						case Operator.Range:
						case Operator.AssignOut:
						case Operator.DeRef:
						case Operator.Conversion:
						case Operator.RefAssign:
						case Operator.Reference:
						case Operator.Property:
							goto IL_917;
						case Operator.Move:
							for (int l = 0; l < operandsList.Count; l++)
							{
								_IExprement iexprement4 = operandsList[l];
								this.\u0001(type, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
								iexprement4.Accept(this);
								type2 = this.TopOfStack_DerivedType;
								this.\u0001();
							}
							this.TopOfStack.DerivedType = type2;
							return;
						case Operator.TestAndSet:
							for (int m = 0; m < operandsList.Count; m++)
							{
								_IExprement iexprement5 = operandsList[m];
								this.\u0001(TypeTable.Bool, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
								iexprement5.Accept(this);
								type2 = this.TopOfStack_DerivedType;
								this.\u0001();
							}
							this.TopOfStack.DerivedType = TypeTable.Bool;
							return;
						case Operator.TruncInt:
							break;
						default:
							goto IL_917;
						}
						break;
					}
					for (int n = 0; n < operandsList.Count; n++)
					{
						_IExprement iexprement6 = operandsList[n];
						this.\u0001(TypeTable.AnyReal, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
						iexprement6.Accept(this);
						this.\u0001();
					}
					if (\u0002.Code == Operator.TruncInt)
					{
						this.TopOfStack.DerivedType = TypeTable.Int;
						return;
					}
					this.TopOfStack.DerivedType = TypeTable.DInt;
					return;
					IL_71A:
					if (type == null)
					{
						type = TypeTable.AnyNum;
						goto IL_917;
					}
					goto IL_917;
				}
				else if (code != Operator.__TypeOf)
				{
					switch (code)
					{
					case Operator.__QueryInterface:
					case Operator.__QueryPointer:
					case Operator.__Delete:
						goto IL_8C8;
					case Operator.__New:
					case Operator.__Cast:
					case Operator.__AdrInst:
						goto IL_917;
					case Operator.__RefAdr:
						break;
					default:
						switch (code)
						{
						case Operator.__BitOffset:
							goto IL_408;
						case Operator.__FCall:
						{
							this.\u0001(type, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
							operandsList[0].Accept(this);
							_ISignature isignature = this.TopOfStack.CurrentSignature as _ISignature;
							this.\u0001();
							if (isignature != null)
							{
								int num2 = 2;
								while (num2 < operandsList.Count && num2 - 2 < isignature.AllInputs.Length)
								{
									this.\u0001(isignature.AllInputs[num2 - 2].Type, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
									operandsList[num2].Accept(this);
									this.\u0001();
									num2++;
								}
								if (isignature.Outputs.Length != 0)
								{
									this.TopOfStack.DerivedType = isignature.Outputs[0].Type;
								}
							}
							return;
						}
						case Operator.__PropertyInfo:
							if (this.\u0001 && operandsList.Count == 2)
							{
								this.\u0001(type, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
								operandsList[0].Accept(this);
								_IVariable ivariable = this.TopOfStack.CurrentVariable as _IVariable;
								this.\u0001();
								IType u;
								if (ivariable != null && ivariable.IsProperty)
								{
									u = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(\u0019.\u0003.\u0001("__PropertyInfoStruct")));
								}
								else
								{
									u = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(\u0019.\u0003.\u0001("__BasePropertyInfo")));
								}
								this.\u0001(u, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
								operandsList[1].Accept(this);
								this.\u0001();
								return;
							}
							goto IL_917;
						default:
							goto IL_917;
						}
						break;
					}
				}
				else
				{
					if (this.\u0001 && operandsList.Count == 1 && operandsList[0] is IVariableExpression)
					{
						return;
					}
					goto IL_917;
				}
				IL_357:
				for (int num3 = 0; num3 < operandsList.Count; num3++)
				{
					_IExprement iexprement7 = operandsList[num3];
					IType type3 = type;
					if (type3 is _IPointerType)
					{
						type3 = (type3 as _IPointerType).Base;
					}
					this.\u0001(type3, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
					iexprement7.Accept(this);
					type2 = this.TopOfStack_DerivedType;
					this.\u0001();
				}
				this.TopOfStack.DerivedType = \u0019.\u0003.\u0001(type2 as _IType);
				return;
				IL_408:
				for (int num4 = 0; num4 < operandsList.Count; num4++)
				{
					_IExprement iexprement8 = operandsList[num4];
					this.\u0001(type, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
					iexprement8.Accept(this);
					this.\u0001();
				}
				this.TopOfStack.DerivedType = TypeTable.Int;
				return;
			}
			if (code <= Operator.__CheckLicense)
			{
				if (code - Operator.And_Then > 1)
				{
					if (code != Operator.__CheckLicense)
					{
						goto IL_917;
					}
				}
				else
				{
					if (type == null)
					{
						type = TypeTable.Bool;
						goto IL_917;
					}
					goto IL_917;
				}
			}
			else
			{
				switch (code)
				{
				case Operator.__CheckLicenseBit:
				case Operator.__MemoryBarrier:
					break;
				case Operator.__XAdd:
					this.TopOfStack.DerivedType = TypeTable.DInt;
					return;
				case Operator.__CurrentTask:
					goto IL_917;
				case Operator.__CompareAndSwap:
					this.TopOfStack.DerivedType = TypeTable.Bool;
					return;
				default:
					if (code == Operator.XSizeOf)
					{
						this.TopOfStack.DerivedType = TypeTable.ResolveUXIntType(this.\u0001.PointerSize);
						return;
					}
					if (code - Operator.__PouName > 1)
					{
						goto IL_917;
					}
					break;
				}
			}
			IL_8C8:
			for (int num5 = 0; num5 < operandsList.Count; num5++)
			{
				_IExprement iexprement9 = operandsList[num5];
				this.\u0001(type, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
				iexprement9.Accept(this);
				this.\u0001();
			}
			this.TopOfStack.DerivedType = TypeTable.Bool;
			return;
			IL_917:
			IType[] array = new IType[operandsList.Count];
			for (int num6 = num; num6 < operandsList.Count; num6++)
			{
				_IExprement iexprement10 = operandsList[num6];
				this.\u0001(type, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
				iexprement10.Accept(this);
				if (!this.\u0001(type) && this.\u0001(this.TopOfStack_DerivedType))
				{
					type = this.TopOfStack_DerivedType;
					array[num6] = type;
					num6 = num - 1;
				}
				else
				{
					if (this.\u0001(type) && this.\u0001(this.TopOfStack_DerivedType) && TypeTable.IsInteger(type.Class) && TypeTable.IsReal(this.TopOfStack_DerivedType.Class))
					{
						type = this.TopOfStack_DerivedType;
					}
					else if (this.\u0001(type) && this.\u0001(this.TopOfStack_DerivedType) && TypeTable.IsInteger(type.Class) && TypeTable.IsInteger(this.TopOfStack_DerivedType.Class) && TypeTable.GetSize(this.TopOfStack_DerivedType.Class, null) > TypeTable.GetSize(type.Class, null))
					{
						type = this.TopOfStack_DerivedType;
					}
					if (type != null && this.TopOfStack_DerivedType != null && TypeTable.IsString(type.Class) && TypeTable.IsString(this.TopOfStack_DerivedType.Class))
					{
						int num7 = 0;
						int num8 = 0;
						bool flag = false;
						if (type.Class == TypeClass.String)
						{
							num7 = CompilerConstants.StringTypeDefaultSize;
							if ((type as _IStringType).Length != null)
							{
								ILiteralValue literalValue = (type as _IStringType).Length.Literal(this.TopOfStack.Scope);
								if (literalValue != null)
								{
									bool flag2;
									num7 = literalValue.GetInt(out flag2);
								}
							}
						}
						if (this.TopOfStack_DerivedType.Class == TypeClass.String)
						{
							num8 = CompilerConstants.StringTypeDefaultSize;
							if ((this.TopOfStack_DerivedType as _IStringType).Length != null)
							{
								ILiteralValue literalValue2 = (this.TopOfStack_DerivedType as _IStringType).Length.Literal(this.TopOfStack.Scope);
								if (literalValue2 != null)
								{
									num8 = literalValue2.GetInt(out flag);
								}
							}
						}
						if (type.Class == TypeClass.WString)
						{
							num7 = CompilerConstants.WStringTypeDefaultSize;
							if ((type as _IWStringType).Length != null)
							{
								ILiteralValue literalValue3 = (type as _IWStringType).Length.Literal(this.TopOfStack.Scope);
								if (literalValue3 != null)
								{
									bool flag2;
									num7 = literalValue3.GetInt(out flag2);
								}
							}
						}
						if (this.TopOfStack_DerivedType.Class == TypeClass.WString)
						{
							num8 = CompilerConstants.WStringTypeDefaultSize;
							if ((this.TopOfStack_DerivedType as _IWStringType).Length != null)
							{
								ILiteralValue literalValue4 = (this.TopOfStack_DerivedType as _IWStringType).Length.Literal(this.TopOfStack.Scope);
								if (literalValue4 != null)
								{
									num8 = literalValue4.GetInt(out flag);
								}
							}
						}
						if (flag && num8 > num7)
						{
							type = this.TopOfStack_DerivedType;
						}
					}
					array[num6] = type;
				}
				this.\u0001();
			}
			if (type2 == null)
			{
				type2 = this.\u0001(type, \u0002, array);
			}
			this.TopOfStack.DerivedType = type2;
		}

		// Token: 0x06001381 RID: 4993 RVA: 0x000373DC File Offset: 0x000355DC
		public static _IType \u0001(_IOperatorExpression \u0002, IType[] \u0003)
		{
			for (int i = 0; i < \u0003.Length; i++)
			{
				if (\u0003[i] == null)
				{
					return null;
				}
			}
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count != \u0003.Length || operandsList.Count == 0)
			{
				return null;
			}
			if (\u0003.Where(new Func<IType, bool>(UnknownIdentVisitor.<>c.<>9.\u0001)).Count<IType>() == 0)
			{
				return null;
			}
			Operator code = \u0002.Code;
			if (code <= Operator.Sub)
			{
				if (code != Operator.Add)
				{
					if (code != Operator.Sub)
					{
						goto IL_15F;
					}
					goto IL_119;
				}
			}
			else if (code != Operator.Plus)
			{
				if (code != Operator.Minus)
				{
					goto IL_15F;
				}
				goto IL_119;
			}
			if (\u0003.Length < 2)
			{
				goto IL_15F;
			}
			TypeClass typeClass = TypeClass.None;
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			foreach (_IType itype in \u0003)
			{
				TypeClass @class = itype.Class;
				if (@class != TypeClass.LTime)
				{
					if (@class - TypeClass.LDate > 2)
					{
						flag2 = true;
					}
					else
					{
						typeClass = itype.Class;
						num++;
					}
				}
				else
				{
					if (typeClass == TypeClass.None)
					{
						typeClass = TypeClass.LTime;
					}
					flag = true;
				}
			}
			if (num == 1 && flag && !flag2)
			{
				return TypeTable.Get(typeClass);
			}
			goto IL_15F;
			IL_119:
			if (\u0003.Length == 2)
			{
				TypeClass class2 = \u0003[0].Class;
				TypeClass class3 = \u0003[1].Class;
				if (class2 == TypeClass.LDate || class2 == TypeClass.LDateAndTime || class2 == TypeClass.LTimeOfDay)
				{
					if (class3 == TypeClass.LTime)
					{
						return TypeTable.Get(class2);
					}
					if (class3 == class2)
					{
						return TypeTable.LTime;
					}
				}
			}
			IL_15F:
			return null;
		}

		// Token: 0x06001382 RID: 4994 RVA: 0x0003754C File Offset: 0x0003574C
		public IType \u0001(IType \u0002, _IOperatorExpression \u0003, IType[] \u0004)
		{
			for (int i = 0; i < \u0004.Length; i++)
			{
				if (\u0004[i] == null)
				{
					return \u0002;
				}
			}
			IList<_IExpression> operandsList = \u0003._OperandsList;
			if (operandsList.Count != \u0004.Length || operandsList.Count == 0)
			{
				return \u0002;
			}
			IType type = UnknownIdentVisitor.\u0001(\u0003, \u0004);
			if (type != null)
			{
				return type;
			}
			Operator code = \u0003.Code;
			switch (code)
			{
			case Operator.Add:
				break;
			case Operator.Sub:
				goto IL_194;
			case Operator.Mul:
				goto IL_228;
			case Operator.Div:
				goto IL_2D4;
			default:
				switch (code)
				{
				case Operator.Plus:
					break;
				case Operator.Minus:
					goto IL_194;
				case Operator.Times:
					goto IL_228;
				case Operator.Power:
					return \u0002;
				case Operator.Divide:
					goto IL_2D4;
				default:
					return \u0002;
				}
				break;
			}
			if (\u0004.Length < 2)
			{
				return \u0002;
			}
			TypeClass typeClass = TypeClass.None;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			_IPointerType ipointerType = null;
			int num = 0;
			foreach (_IType itype in \u0004)
			{
				TypeClass @class = itype.Class;
				switch (@class)
				{
				case TypeClass.Time:
					if (typeClass == TypeClass.None)
					{
						typeClass = TypeClass.Time;
					}
					flag = true;
					break;
				case TypeClass.Date:
				case TypeClass.DateAndTime:
				case TypeClass.TimeOfDay:
					typeClass = itype.Class;
					num++;
					break;
				case TypeClass.Pointer:
					ipointerType = (itype as _IPointerType);
					num++;
					break;
				default:
					if (@class == TypeClass.LTime)
					{
						if (typeClass == TypeClass.None)
						{
							typeClass = TypeClass.LTime;
						}
						flag2 = true;
					}
					else
					{
						flag3 = true;
					}
					break;
				}
			}
			if (num == 1 && flag && !flag2 && !flag3)
			{
				return TypeTable.Get(typeClass);
			}
			if (num == 0 && flag && !flag2 && !flag3)
			{
				return TypeTable.Get(TypeClass.Time);
			}
			if (num == 0 && !flag && flag2 && !flag3)
			{
				return TypeTable.Get(TypeClass.LTime);
			}
			if (ipointerType != null && num == 1)
			{
				return ipointerType;
			}
			return \u0002;
			IL_194:
			if (\u0004.Length != 2)
			{
				return \u0002;
			}
			TypeClass class2 = \u0004[0].Class;
			TypeClass class3 = \u0004[1].Class;
			if (class2 == TypeClass.Date || class2 == TypeClass.DateAndTime || class2 == TypeClass.TimeOfDay || class2 == TypeClass.Time)
			{
				if (class3 == TypeClass.Time)
				{
					return TypeTable.Get(class2);
				}
				if (class3 == class2)
				{
					return TypeTable.Time;
				}
				return \u0002;
			}
			else
			{
				if (class2 == TypeClass.LTime && class3 == TypeClass.LTime)
				{
					return TypeTable.LTime;
				}
				if (class2 == TypeClass.Pointer && TypeTable.IsInteger(class3))
				{
					return \u0004[0] as _IType;
				}
				if (class2 == TypeClass.Pointer && class3 == TypeClass.Pointer)
				{
					return TypeTable.DWord;
				}
				return \u0002;
			}
			IL_228:
			if (\u0004.Length < 2)
			{
				return \u0002;
			}
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			int num2 = 0;
			foreach (_IType itype2 in \u0004)
			{
				TypeClass @class = itype2.Class;
				if (@class != TypeClass.Time)
				{
					if (@class == TypeClass.LTime)
					{
						flag5 = true;
						num2++;
					}
					else
					{
						if (!TypeTable.IsInteger(itype2.Class))
						{
							flag6 = true;
						}
						if (TypeTable.IsLInteger(itype2.Class, null))
						{
							flag7 = true;
						}
					}
				}
				else
				{
					flag4 = true;
					num2++;
				}
			}
			if (num2 == 1 && flag5 && !flag6)
			{
				return TypeTable.LTime;
			}
			if (num2 == 1 && flag4 && !flag6 && !flag7)
			{
				return TypeTable.Time;
			}
			return \u0002;
			IL_2D4:
			if (operandsList.Count == 2)
			{
				TypeClass class4 = \u0004[0].Class;
				TypeClass class5 = \u0004[1].Class;
				if (class4 == TypeClass.Time && TypeTable.IsInteger(class5) && !TypeTable.IsLInteger2(class5, null))
				{
					return TypeTable.Time;
				}
				if (class4 == TypeClass.LTime && TypeTable.IsInteger(class5))
				{
					return TypeTable.LTime;
				}
			}
			return \u0002;
		}

		// Token: 0x06001383 RID: 4995 RVA: 0x00037880 File Offset: 0x00035A80
		public IType \u0001(IType \u0002)
		{
			if (\u0002 == null)
			{
				return TypeTable.Int;
			}
			if (typeof(_IAnyBitButBoolIsPreferred).IsAssignableFrom(\u0002.GetType()))
			{
				return TypeTable.Bool;
			}
			switch (\u0002.Class)
			{
			case TypeClass.None:
			case TypeClass.Any:
			case TypeClass.AnyInt:
			case TypeClass.AnyNum:
			case TypeClass.Lazy:
				return TypeTable.Int;
			case TypeClass.AnyBit:
				return TypeTable.Word;
			case TypeClass.AnyDate:
				return TypeTable.Date;
			case TypeClass.AnyReal:
				return TypeTable.Real;
			default:
				return \u0002;
			}
		}

		// Token: 0x06001384 RID: 4996 RVA: 0x00037900 File Offset: 0x00035B00
		public bool \u0001(IType \u0002)
		{
			return \u0002 != null && TypeTable.IsConcreteType(\u0002.Class);
		}

		// Token: 0x06001385 RID: 4997 RVA: 0x00037914 File Offset: 0x00035B14
		public bool \u0001(IType \u0002, IType \u0003)
		{
			if (\u0003 == null)
			{
				return false;
			}
			if (\u0002 == null)
			{
				return true;
			}
			bool flag = \u0002 is _IRangeAwareAnyIntType;
			bool flag2 = \u0002 is _IRangeAwareAnyIntType;
			return (!flag || flag2) && ((flag2 && !flag) || TypeTable.IsConcreterType(\u0002.Class, \u0003.Class));
		}

		// Token: 0x06001386 RID: 4998 RVA: 0x00037960 File Offset: 0x00035B60
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x06001387 RID: 4999 RVA: 0x00037964 File Offset: 0x00035B64
		public void \u0001(_INewExpression \u0002)
		{
			this.\u0001(TypeTable.Int, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
			\u0002._Count.Accept(this);
			IType type = this.TopOfStack.DerivedType;
			this.\u0001();
			this.TopOfStack.DerivedType = \u0019.\u0003.\u0001(type as _IType);
			if (\u0002._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in \u0002._FBInitParams)
				{
					((_IAssignmentExpression)assignmentExpression).Accept(this);
				}
			}
		}

		// Token: 0x06001388 RID: 5000 RVA: 0x00037A0C File Offset: 0x00035C0C
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x06001389 RID: 5001 RVA: 0x00037A10 File Offset: 0x00035C10
		public void \u0001(_IConversionExpression \u0002)
		{
			this.\u0001(TypeTable.Get(\u0002.From), this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
			\u0002._Exp.Accept(this);
			IType type = this.TopOfStack_DerivedType;
			IPrecompileScope2 u = this.TopOfStack.Scope;
			this.\u0001();
			IType type2 = TypeTable.Get(\u0002.To);
			if (type != null && TypeTable.IsString(type.Class) && TypeTable.IsString(type2.Class))
			{
				type2 = this.\u0001(type2, this.TopOfStack.Scope, type, u);
			}
			this.TopOfStack.DerivedType = type2;
		}

		// Token: 0x0600138A RID: 5002 RVA: 0x00037AAC File Offset: 0x00035CAC
		private IType \u0001(IType \u0002, IPrecompileScope2 \u0003, IType \u0004, IPrecompileScope2 \u0005)
		{
			\u0004 = global::\u0005.\u0004.\u0001((global::\u0015.\u0002)\u0003, (global::\u0015.\u0002)\u0005, (_IType)\u0004);
			_IExpression iexpression = null;
			if (\u0004.Class == TypeClass.String)
			{
				iexpression = (\u0004 as _IStringType).Length;
				if (iexpression != null)
				{
					iexpression = (iexpression.Duplicate() as _IExpression);
				}
			}
			else if (\u0004.Class == TypeClass.WString)
			{
				iexpression = (\u0004 as _IWStringType).Length;
				if (iexpression != null)
				{
					iexpression = (iexpression.Duplicate() as _IExpression);
				}
			}
			if (iexpression != null)
			{
				if (\u0002.Class == TypeClass.String)
				{
					_IStringType istringType = \u0019.\u0003.\u0001();
					istringType.Length = iexpression;
					\u0002 = istringType;
				}
				else if (\u0002.Class == TypeClass.WString)
				{
					_IWStringType iwstringType = \u0019.\u0003.\u0001();
					iwstringType.Length = iexpression;
					\u0002 = iwstringType;
				}
			}
			return \u0002;
		}

		// Token: 0x0600138B RID: 5003 RVA: 0x00037B58 File Offset: 0x00035D58
		public void \u0001(_IThisExpression \u0002)
		{
			ISignature localSignature = this.TopOfStack.Scope.LocalSignature;
			if (localSignature != null)
			{
				string name = localSignature.Name;
				this.TopOfStack.DerivedType = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(name));
			}
		}

		// Token: 0x0600138C RID: 5004 RVA: 0x00037B98 File Offset: 0x00035D98
		public void \u0001(_IBaseExpression \u0002)
		{
			ISignature2 signature = this.TopOfStack.Scope.LocalSignature as ISignature2;
			if (((signature != null) ? signature.BaseExpression : null) != null)
			{
				string u = signature.BaseExpression.ToString();
				this.TopOfStack.DerivedType = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(u));
			}
		}

		// Token: 0x0600138D RID: 5005 RVA: 0x00037BEC File Offset: 0x00035DEC
		public void \u0001(_ILiteralExpression \u0002)
		{
			ICompiledType compiledType = null;
			if (\u0002.ConstantType == TypeClass.AnyInt)
			{
				if (this.\u0001)
				{
					this.TopOfStack.DerivedType = \u0019.\u0003.\u0001();
					return;
				}
				if (\u0002.Negative)
				{
					long longValue = \u0002.LongValue;
					if (longValue >= -32768L && longValue <= 32767L)
					{
						compiledType = TypeTable.Int;
					}
					else if (longValue >= -2147483648L && longValue <= 2147483647L)
					{
						compiledType = TypeTable.DInt;
					}
					else
					{
						compiledType = TypeTable.LInt;
					}
				}
				else
				{
					ulong ulongValue = \u0002.ULongValue;
					if (ulongValue <= 32767UL)
					{
						compiledType = TypeTable.Int;
					}
					else if (ulongValue <= 2147483647UL)
					{
						compiledType = TypeTable.DInt;
					}
					else if (ulongValue <= 9223372036854775807UL)
					{
						compiledType = TypeTable.LInt;
					}
					else
					{
						compiledType = TypeTable.ULInt;
					}
				}
			}
			if (compiledType == null)
			{
				bool u = false;
				bool u2 = true;
				if (this.\u0001 != null)
				{
					u2 = this.\u0001.HasByteSupport();
					if (this.\u0001.IsDefined("NO_UNICODE_SUPPORT"))
					{
						u = true;
					}
				}
				compiledType = Helper.\u0001(\u0002, this.LRealSupported, false, this.Int64Supported, false, u, u2);
			}
			this.TopOfStack.DerivedType = compiledType;
		}

		// Token: 0x0600138E RID: 5006 RVA: 0x00037D04 File Offset: 0x00035F04
		public void \u0001(_IAddressExpression \u0002)
		{
			this.TopOfStack.DerivedType = TypeTable.GetDirectVariableSizeType(\u0002.DirectAddress.Size);
		}

		// Token: 0x0600138F RID: 5007 RVA: 0x00037D24 File Offset: 0x00035F24
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06001390 RID: 5008 RVA: 0x00037D28 File Offset: 0x00035F28
		public void \u0001(_IVariableExpression \u0002)
		{
			IVariable variable = null;
			ISignature signature = null;
			IPrecompileScope precompileScope = null;
			if (this.\u0001 != null)
			{
				foreach (\u0018.\u0001 u2 in this.\u0001)
				{
					if (string.Equals(\u0002.Name, u2.Name, StringComparison.OrdinalIgnoreCase) && this.\u0001(u2.DerivedType))
					{
						this.TopOfStack.DerivedType = u2.DerivedType;
						return;
					}
				}
			}
			this.TopOfStack.Scope.FindDeclaration(\u0002.Name, out variable, out signature, out precompileScope);
			this.TopOfStack.CurrentSignature = signature;
			if ((variable == null && !this.\u0001 && signature == null && precompileScope == null) || (variable != null && variable.GetFlag(VarFlag.Lazy) && this.\u0001 && (variable.Type == null || !TypeTable.IsConcreteType(variable.Type.Class))))
			{
				Guid guid = Guid.Empty;
				string u0081_u = string.Empty;
				if (this.TopOfStack.Scope.LocalSignature != null)
				{
					guid = this.TopOfStack.Scope.LocalSignature.ObjectGuid;
					u0081_u = ((_ISignature)this.TopOfStack.Scope.LocalSignature).LibraryPath;
				}
				if (guid != Guid.Empty)
				{
					\u0018.\u0001 u3 = new \u0018.\u0001(\u0002.Name, \u0002.Position, guid, this.TopOfStack_DerivedType, this.TopOfStack.Access, this.TopOfStack.VarFlag, u0081_u, null, Guid.Empty);
					bool flag = true;
					for (int j = 0; j < this.\u0001.Count; j++)
					{
						\u0018.\u0001 u4 = this.\u0001[j];
						if (u4.Name == u3.Name && u4.ObjectGuid == u3.ObjectGuid)
						{
							IType u5 = u4.DerivedType;
							if (!this.\u0001(u4.DerivedType) && this.\u0001(u4.DerivedType, this.TopOfStack_DerivedType))
							{
								u5 = this.TopOfStack_DerivedType;
							}
							else if (!this.\u0001(this.TopOfStack_DerivedType))
							{
								u5 = u4.DerivedType;
							}
							if (u4.Access == AccessFlag.Unknown)
							{
								u4.Access = u3.Access;
							}
							u4.DerivedType = u5;
							this.TopOfStack.DerivedType = u5;
							if (u4.VarFlag == VarFlag.Local)
							{
								u4.VarFlag = u3.VarFlag;
							}
							flag = false;
						}
					}
					if (flag)
					{
						this.\u0001.Add(u3);
					}
				}
			}
			if (variable != null)
			{
				this.TopOfStack.CurrentVariable = variable;
				if (this.\u0001(variable.Type))
				{
					this.TopOfStack.DerivedType = variable.Type;
					this.TopOfStack.DerivedType = this.TopOfStack_DerivedType;
					this.TopOfStack.Scope = (this.TopOfStack.Scope.NewLocalScope(signature) as IPrecompileScope2);
					return;
				}
			}
			else
			{
				if (signature != null && (signature.POUType == Operator.Function || signature.POUType == Operator.Method))
				{
					this.TopOfStack.DerivedType = null;
					return;
				}
				if (signature != null && (signature.POUType == Operator.VarGlobal || signature.POUType == Operator.Program || signature.POUType == Operator.FunctionBlock || signature.POUType == Operator.Type))
				{
					string u6 = signature.Name;
					if (!string.IsNullOrEmpty(signature.LibraryPath))
					{
						_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(signature.LibraryPath);
						_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
						Guid applicationGuid = (this.TopOfStack.Scope as _IPrecompileScope).ApplicationGuid;
						if (applicationGuid != Guid.Empty)
						{
							ipreCompileContext = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(applicationGuid) as _IPreCompileContext);
						}
						string namespaceOfLibrary = ipreCompileContext._GetLibraryTable(applicationGuid).GetNamespaceOfLibrary(ipreCompileContext, libraryContext.LibraryId);
						if (!string.IsNullOrEmpty(namespaceOfLibrary))
						{
							u6 = namespaceOfLibrary + "." + signature.Name;
						}
					}
					_IExpression u7 = global::\u0011.\u0006.\u0001(u6) as _IExpression;
					this.TopOfStack.DerivedType = \u0019.\u0003.\u0001(u7);
					return;
				}
				if (precompileScope != null)
				{
					this.TopOfStack.Scope = (precompileScope as IPrecompileScope2);
					this.TopOfStack.DerivedScope = (precompileScope as IPrecompileScope2);
				}
			}
		}

		// Token: 0x06001391 RID: 5009 RVA: 0x00038170 File Offset: 0x00036370
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
			if (this.TopOfStack.CurrentVariable != null && this.\u0001(this.TopOfStack.CurrentVariable.Type) && this.TopOfStack_DerivedType != null)
			{
				this.TopOfStack.DerivedType = TypeTable.GetDirectVariableSizeType(\u0002.PartSize);
			}
		}

		// Token: 0x06001392 RID: 5010 RVA: 0x000381CC File Offset: 0x000363CC
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			ICollection<_IExpression> accesses = \u0002._Accesses;
			foreach (_IExprement iexprement in accesses)
			{
				this.\u0001(TypeTable.Int, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Local);
				iexprement.Accept(this);
				this.\u0001();
			}
			_IArrayType iarrayType = null;
			if (this.TopOfStack_DerivedType != null)
			{
				iarrayType = \u0019.\u0003.\u0001(this.TopOfStack_DerivedType as _IType);
				foreach (_IExpression iexpression in accesses)
				{
					ILiteralValue literalValue = iexpression.Literal(this.TopOfStack.Scope);
					int num = 0;
					if (literalValue == null || !literalValue.GetInt(out num))
					{
						iarrayType.AddDimension(\u0019.\u0003.\u0001(0L), \u0019.\u0003.\u0001(0L));
					}
					else if (num < 0)
					{
						iarrayType.AddDimension(\u0019.\u0003.\u0001((long)num), \u0019.\u0003.\u0001(0L));
					}
					else
					{
						iarrayType.AddDimension(\u0019.\u0003.\u0001(0L), \u0019.\u0003.\u0001((long)num));
					}
				}
			}
			this.\u0001(iarrayType, this.TopOfStack.Scope, this.TopOfStack.Access, this.TopOfStack.VarFlag);
			\u0002._Var.Accept(this);
			IType type = this.TopOfStack_DerivedType;
			IPrecompileScope2 u = this.TopOfStack.Scope;
			ISignature u2 = this.TopOfStack.CurrentSignature;
			IVariable u3 = this.TopOfStack.CurrentVariable;
			IPrecompileScope2 u4 = this.TopOfStack.DerivedScope;
			this.\u0001();
			this.TopOfStack.DerivedType = type;
			this.TopOfStack.CurrentSignature = u2;
			this.TopOfStack.CurrentVariable = u3;
			this.TopOfStack.DerivedScope = u4;
			IArrayType arrayType = type as _IArrayType;
			if (arrayType != null)
			{
				this.TopOfStack.DerivedType = arrayType.Base;
			}
			else if (type is _IPointerType)
			{
				this.TopOfStack.DerivedType = (type as _IPointerType).Base;
			}
			this.TopOfStack.Scope = u;
		}

		// Token: 0x06001393 RID: 5011 RVA: 0x000383F0 File Offset: 0x000365F0
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			ISignature u = this.TopOfStack.CurrentSignature;
			IVariable u2 = this.TopOfStack.CurrentVariable;
			IPrecompileScope2 precompileScope = null;
			this.\u0001(null, this.TopOfStack.Scope, this.TopOfStack.Access, this.TopOfStack.VarFlag);
			\u0002._Left.Accept(this);
			IType type = this.TopOfStack_DerivedType;
			IType type2 = type;
			IPrecompileScope2 precompileScope2 = this.TopOfStack.DerivedScope;
			if (precompileScope2 == null)
			{
				IVariable variable = this.TopOfStack.CurrentVariable;
				IUserdefType2 userdefType = ((variable != null) ? variable.Type : null) as IUserdefType2;
				if (userdefType != null && Helper.InvalidId != userdefType.SignatureId)
				{
					ISignature signatureForPrecompileID = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetSignatureForPrecompileID(userdefType.SignatureId);
					precompileScope2 = (this.TopOfStack.Scope.NewLocalScope(signatureForPrecompileID) as IPrecompileScope2);
				}
			}
			this.\u0001();
			if (precompileScope2 == null)
			{
				precompileScope2 = this.TopOfStack.Scope;
			}
			if (type != null)
			{
				_IUserdefType iuserdefType = type as _IUserdefType;
				if (iuserdefType != null)
				{
					ISignature signature = precompileScope2.FindSignatureGlobal(iuserdefType.NameExpression);
					if (signature == null && this.\u0001 != null)
					{
						signature = new CheckerScope(this.\u0001.ApplicationGuid, null, APEnvironmentFacade.Instance.LanguageModelMgr.Pool, null).\u0001(iuserdefType.NameExpression);
					}
					precompileScope2 = (precompileScope2.NewLocalScope(signature) as IPrecompileScope2);
				}
				if (TypeTable.IsNumber(type.Class))
				{
					this.TopOfStack.DerivedType = TypeTable.Bit;
					this.TopOfStack.CurrentSignature = u;
					this.TopOfStack.CurrentVariable = u2;
					this.TopOfStack.DerivedScope = precompileScope;
					return;
				}
			}
			if (precompileScope2 == null)
			{
				return;
			}
			if (precompileScope2.LocalSignature != null && precompileScope2.LocalSignature.POUType == Operator.FunctionBlock && \u0002._Right.ToString() == "FB_Init")
			{
				this.TopOfStack.DerivedType = null;
				this.TopOfStack.CurrentSignature = null;
				this.TopOfStack.CurrentVariable = null;
				return;
			}
			if (precompileScope2.LocalSignature != null && precompileScope2.LocalSignature.POUType == Operator.VarGlobal)
			{
				this.\u0001(this.TopOfStack_DerivedType, precompileScope2, this.TopOfStack.Access, VarFlag.Global);
			}
			else if (this.TopOfStack.Access == AccessFlag.Write)
			{
				this.\u0001(this.TopOfStack_DerivedType, precompileScope2, this.TopOfStack.Access, VarFlag.Input);
			}
			else
			{
				this.\u0001(this.TopOfStack_DerivedType, precompileScope2, this.TopOfStack.Access, VarFlag.Output);
			}
			\u0002._Right.Accept(this);
			type = this.TopOfStack_DerivedType;
			u = this.TopOfStack.CurrentSignature;
			u2 = this.TopOfStack.CurrentVariable;
			precompileScope = this.TopOfStack.DerivedScope;
			IType type3 = type;
			this.\u0001();
			_IType itype = type3 as _IType;
			_IArrayType iarrayType = type3 as _IArrayType;
			if (iarrayType != null)
			{
				itype = iarrayType._Base;
			}
			if (type2 is _IUserdefType && itype is _IUserdefType)
			{
				_IUserdefType iuserdefType2 = type2 as _IUserdefType;
				_IUserdefType iuserdefType3 = itype as _IUserdefType;
				if (iuserdefType2.NameExpression is _ICompoAccessExpression)
				{
					_IExpression iexpression = new global::\u0011.\u0006((iuserdefType2.NameExpression as _ICompoAccessExpression)._Left.ToString() + "." + iuserdefType3.NameExpression.ToString()).\u0002() as _IExpression;
					if (iexpression != null)
					{
						itype = \u0019.\u0003.\u0001(iexpression);
					}
				}
				if (iarrayType != null)
				{
					type3 = iarrayType.Duplicate;
					(type3 as _IArrayType)._Base = itype;
				}
				else
				{
					type3 = itype;
				}
			}
			type = type3;
			this.TopOfStack.DerivedType = type;
			this.TopOfStack.CurrentSignature = u;
			this.TopOfStack.CurrentVariable = u2;
			this.TopOfStack.DerivedScope = precompileScope;
			if (precompileScope != null)
			{
				this.TopOfStack.Scope = precompileScope;
				return;
			}
			this.TopOfStack.Scope = precompileScope2;
		}

		// Token: 0x06001394 RID: 5012 RVA: 0x000387CC File Offset: 0x000369CC
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			_IPointerType u = \u0019.\u0003.\u0001(this.TopOfStack_DerivedType as _IType);
			this.\u0001(u, this.TopOfStack.Scope, this.TopOfStack.Access, this.TopOfStack.VarFlag);
			\u0002._Base.Accept(this);
			IType type = this.TopOfStack_DerivedType;
			IPrecompileScope2 u2 = this.TopOfStack.Scope;
			this.\u0001();
			if (type is _IPointerType)
			{
				this.TopOfStack.DerivedType = (type as _IPointerType).Base;
			}
			this.TopOfStack.Scope = u2;
		}

		// Token: 0x06001395 RID: 5013 RVA: 0x00038864 File Offset: 0x00036A64
		public void \u0001(_ICopyScopeExpression \u0002)
		{
		}

		// Token: 0x06001396 RID: 5014 RVA: 0x00038868 File Offset: 0x00036A68
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			IPrecompileScope precompileScope = this.TopOfStack.Scope;
			precompileScope = precompileScope.GlobalScope();
			this.\u0001(this.TopOfStack_DerivedType, precompileScope, this.TopOfStack.Access, VarFlag.Global);
			\u0002._Base.Accept(this);
			IType u = this.TopOfStack_DerivedType;
			this.\u0001();
			this.TopOfStack.DerivedType = u;
		}

		// Token: 0x06001397 RID: 5015 RVA: 0x000388CC File Offset: 0x00036ACC
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			IPrecompileScope2 precompileScope = (this.TopOfStack.Scope as _IPrecompileScope2).CreateSystemScope();
			this.\u0001(this.TopOfStack_DerivedType, precompileScope, this.TopOfStack.Access, VarFlag.Global);
			\u0002._Base.Accept(this);
			IType type = this.TopOfStack_DerivedType;
			this.\u0001();
			if (type == null)
			{
				this.TopOfStack.DerivedScope = precompileScope;
				return;
			}
			this.TopOfStack.DerivedType = type;
		}

		// Token: 0x06001398 RID: 5016 RVA: 0x00038944 File Offset: 0x00036B44
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			IPrecompileScope2 precompileScope = (this.TopOfStack.Scope as _IPrecompileScope2).CreatePoolScope();
			this.\u0001(this.TopOfStack_DerivedType, precompileScope, this.TopOfStack.Access, VarFlag.Global);
			\u0002._Base.Accept(this);
			IType type = this.TopOfStack_DerivedType;
			this.\u0001();
			if (type == null)
			{
				this.TopOfStack.DerivedScope = precompileScope;
				return;
			}
			this.TopOfStack.DerivedType = type;
		}

		// Token: 0x06001399 RID: 5017 RVA: 0x000389BC File Offset: 0x00036BBC
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			IPrecompileScope2 precompileScope = (this.TopOfStack.Scope as IPrecompileScope7).FindScope(\u0002._Namespace) as IPrecompileScope2;
			if (precompileScope != null)
			{
				this.\u0001(this.TopOfStack_DerivedType, precompileScope, this.TopOfStack.Access, VarFlag.Global);
				\u0002._Access.Accept(this);
				IType type = this.TopOfStack_DerivedType;
				this.\u0001();
				if (type == null)
				{
					this.TopOfStack.DerivedScope = precompileScope;
					return;
				}
				this.TopOfStack.DerivedType = type;
			}
		}

		// Token: 0x0600139A RID: 5018 RVA: 0x00038A40 File Offset: 0x00036C40
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x0600139B RID: 5019 RVA: 0x00038A44 File Offset: 0x00036C44
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			this.\u0001(this.TopOfStack_DerivedType, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Constant);
			\u0002._High.Accept(this);
			this.\u0001();
			this.\u0001(this.TopOfStack_DerivedType, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Constant);
			\u0002._Low.Accept(this);
			this.\u0001();
		}

		// Token: 0x0600139C RID: 5020 RVA: 0x00038AAC File Offset: 0x00036CAC
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.\u0001(this.TopOfStack_DerivedType, this.TopOfStack.Scope, AccessFlag.Read, VarFlag.Constant);
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
			this.\u0001();
		}

		// Token: 0x0600139D RID: 5021 RVA: 0x00038B18 File Offset: 0x00036D18
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001(TypeTable.Int, this.TopOfStack.Scope, AccessFlag.Read, this.TopOfStack.VarFlag);
			\u0002._Switch.Accept(this);
			IType u = this.TopOfStack_DerivedType;
			this.\u0001();
			foreach (_ICase icase in \u0002._Cases)
			{
				this.\u0001(u, this.TopOfStack.Scope, AccessFlag.Read, this.TopOfStack.VarFlag);
				icase._Label.Accept(this);
				this.\u0001();
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x0600139E RID: 5022 RVA: 0x00038BE8 File Offset: 0x00036DE8
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x0600139F RID: 5023 RVA: 0x00038BEC File Offset: 0x00036DEC
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x060013A0 RID: 5024 RVA: 0x00038BF0 File Offset: 0x00036DF0
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x060013A1 RID: 5025 RVA: 0x00038BF4 File Offset: 0x00036DF4
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x060013A2 RID: 5026 RVA: 0x00038BF8 File Offset: 0x00036DF8
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
		}

		// Token: 0x060013A3 RID: 5027 RVA: 0x00038BFC File Offset: 0x00036DFC
		public void \u0001(_IArrayInitialization \u0002)
		{
		}

		// Token: 0x060013A4 RID: 5028 RVA: 0x00038C00 File Offset: 0x00036E00
		public void \u0001(_IStructureInitialization \u0002)
		{
		}

		// Token: 0x060013A5 RID: 5029 RVA: 0x00038C04 File Offset: 0x00036E04
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x060013A6 RID: 5030 RVA: 0x00038C08 File Offset: 0x00036E08
		public void \u0001(_IVariableReference \u0002)
		{
		}

		// Token: 0x060013A7 RID: 5031 RVA: 0x00038C0C File Offset: 0x00036E0C
		public void \u0001(_ITypeReference \u0002)
		{
		}

		// Token: 0x060013A8 RID: 5032 RVA: 0x00038C10 File Offset: 0x00036E10
		public void \u0001(_IPouReference \u0002)
		{
		}

		// Token: 0x060013A9 RID: 5033 RVA: 0x00038C14 File Offset: 0x00036E14
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x060013AA RID: 5034 RVA: 0x00038C18 File Offset: 0x00036E18
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x060013AB RID: 5035 RVA: 0x00038C1C File Offset: 0x00036E1C
		public void \u0001(_IDefinedExpression \u0002)
		{
		}

		// Token: 0x060013AC RID: 5036 RVA: 0x00038C20 File Offset: 0x00036E20
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x060013AD RID: 5037 RVA: 0x00038C24 File Offset: 0x00036E24
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x060013AE RID: 5038 RVA: 0x00038C28 File Offset: 0x00036E28
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x060013AF RID: 5039 RVA: 0x00038C2C File Offset: 0x00036E2C
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			\u0002.Condition.Accept(this);
			\u0002.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				ipragmaElseIf.Condition.Accept(this);
				ipragmaElseIf.Controlled.Accept(this);
			}
			_IStatement ifElse = \u0002.IfElse;
			if (ifElse == null)
			{
				return;
			}
			ifElse.Accept(this);
		}

		// Token: 0x060013B0 RID: 5040 RVA: 0x00038CB4 File Offset: 0x00036EB4
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
		}

		// Token: 0x060013B1 RID: 5041 RVA: 0x00038CB8 File Offset: 0x00036EB8
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x060013B2 RID: 5042 RVA: 0x00038CBC File Offset: 0x00036EBC
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x060013B3 RID: 5043 RVA: 0x00038CC0 File Offset: 0x00036EC0
		public void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x060013B4 RID: 5044 RVA: 0x00038CC4 File Offset: 0x00036EC4
		public void \u0001(_IHasTypeExpression \u0002)
		{
		}

		// Token: 0x060013B5 RID: 5045 RVA: 0x00038CC8 File Offset: 0x00036EC8
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x060013B6 RID: 5046 RVA: 0x00038CCC File Offset: 0x00036ECC
		public void \u0001(_IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x060013B7 RID: 5047 RVA: 0x00038CD0 File Offset: 0x00036ED0
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x060013B8 RID: 5048 RVA: 0x00038CD4 File Offset: 0x00036ED4
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x060013B9 RID: 5049 RVA: 0x00038CD8 File Offset: 0x00036ED8
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
		}

		// Token: 0x060013BA RID: 5050 RVA: 0x00038CDC File Offset: 0x00036EDC
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x060013BB RID: 5051 RVA: 0x00038CE0 File Offset: 0x00036EE0
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
		}

		// Token: 0x04000342 RID: 834
		private LStack<global::\u0017.\u0005> \u0001 = new LStack<global::\u0017.\u0005>();

		// Token: 0x04000343 RID: 835
		private LList<\u0018.\u0001> \u0001 = new LList<\u0018.\u0001>();

		// Token: 0x04000344 RID: 836
		private bool \u0001;

		// Token: 0x04000345 RID: 837
		private bool \u0002;

		// Token: 0x04000346 RID: 838
		private _ICompileContext \u0001;

		// Token: 0x04000347 RID: 839
		private \u0018.\u0001[] \u0001;

		// Token: 0x04000348 RID: 840
		private LDictionary<string, string> \u0001;
	}
}
