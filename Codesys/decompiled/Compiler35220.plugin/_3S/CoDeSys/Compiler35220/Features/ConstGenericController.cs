using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0007;
using \u000E;
using \u0010;
using \u0011;
using \u0018;
using \u0019;
using \u001A;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Features
{
	// Token: 0x020001BE RID: 446
	internal sealed class ConstGenericController
	{
		// Token: 0x17000605 RID: 1541
		// (get) Token: 0x0600205E RID: 8286 RVA: 0x0006DE18 File Offset: 0x0006C018
		private \u001B CompileInformation { get; }

		// Token: 0x17000606 RID: 1542
		// (get) Token: 0x0600205F RID: 8287 RVA: 0x0006DE20 File Offset: 0x0006C020
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x17000607 RID: 1543
		// (get) Token: 0x06002060 RID: 8288 RVA: 0x0006DE30 File Offset: 0x0006C030
		private _ICompileContext ComconOld
		{
			get
			{
				return this.CompileInformation.ComconOld;
			}
		}

		// Token: 0x06002061 RID: 8289 RVA: 0x0006DE40 File Offset: 0x0006C040
		internal ConstGenericController(\u001B ci)
		{
			this.CompileInformation = ci;
		}

		// Token: 0x17000608 RID: 1544
		// (get) Token: 0x06002062 RID: 8290 RVA: 0x0006DE68 File Offset: 0x0006C068
		private Stack<_ISignature> CurrentlyProcessedGenericSignature { get; } = new Stack<_ISignature>();

		// Token: 0x17000609 RID: 1545
		// (get) Token: 0x06002063 RID: 8291 RVA: 0x0006DE70 File Offset: 0x0006C070
		private Dictionary<_ISignature, _ISignature> ProcessedGenericSignature { get; } = new Dictionary<_ISignature, _ISignature>();

		// Token: 0x06002064 RID: 8292 RVA: 0x0006DE78 File Offset: 0x0006C078
		public _ISignature \u0001(_ISignature \u0002, IGenericUserdefType \u0003, global::\u000E.\u0017 \u0004)
		{
			if (!this.CurrentlyProcessedGenericSignature.Contains(\u0002))
			{
				ConstGenericController.SpecificConstGenericGenerator specificConstGenericGenerator = new ConstGenericController.SpecificConstGenericGenerator(\u0002, this.ComconNew, \u0003.GenericConstantsInitializations, \u0004);
				this.CurrentlyProcessedGenericSignature.Push(\u0002);
				try
				{
					specificConstGenericGenerator.\u0001(this.ComconOld, this.CompileInformation.InterfaceCompiler, this);
				}
				finally
				{
					this.CurrentlyProcessedGenericSignature.Pop();
				}
				return specificConstGenericGenerator.SpecificSignature;
			}
			_ISignature isignature = this.ProcessedGenericSignature[\u0002];
			if (isignature != null)
			{
				return isignature;
			}
			return \u0002;
		}

		// Token: 0x06002065 RID: 8293 RVA: 0x0006DF04 File Offset: 0x0006C104
		public bool \u0001(_ISignature \u0002)
		{
			return new ConstGenericController.ConstGenericAdapter(this.CompileInformation, \u0002).\u0002();
		}

		// Token: 0x06002066 RID: 8294 RVA: 0x0006DF18 File Offset: 0x0006C118
		public bool \u0002(_ISignature \u0002)
		{
			return new ConstGenericController.ConstGenericAdapter(this.CompileInformation, \u0002).\u0003();
		}

		// Token: 0x04000546 RID: 1350
		[CompilerGenerated]
		private readonly \u001B \u0001;

		// Token: 0x04000547 RID: 1351
		[CompilerGenerated]
		private readonly Stack<_ISignature> \u0001;

		// Token: 0x04000548 RID: 1352
		[CompilerGenerated]
		private readonly Dictionary<_ISignature, _ISignature> \u0001;

		// Token: 0x020001BF RID: 447
		private sealed class SpecificConstGenericGenerator
		{
			// Token: 0x1700060A RID: 1546
			// (get) Token: 0x06002067 RID: 8295 RVA: 0x0006DF2C File Offset: 0x0006C12C
			private global::\u000E.\u0017 Context { get; }

			// Token: 0x1700060B RID: 1547
			// (get) Token: 0x06002068 RID: 8296 RVA: 0x0006DF34 File Offset: 0x0006C134
			private _ISignature GenericSignature { get; }

			// Token: 0x1700060C RID: 1548
			// (get) Token: 0x06002069 RID: 8297 RVA: 0x0006DF3C File Offset: 0x0006C13C
			private _ICompileContext CompileContext { get; }

			// Token: 0x1700060D RID: 1549
			// (get) Token: 0x0600206A RID: 8298 RVA: 0x0006DF44 File Offset: 0x0006C144
			private IEnumerable<_IExpression> GenericValues { get; }

			// Token: 0x1700060E RID: 1550
			// (get) Token: 0x0600206B RID: 8299 RVA: 0x0006DF4C File Offset: 0x0006C14C
			// (set) Token: 0x0600206C RID: 8300 RVA: 0x0006DF54 File Offset: 0x0006C154
			internal _ISignature SpecificSignature { get; private set; }

			// Token: 0x0600206D RID: 8301 RVA: 0x0006DF60 File Offset: 0x0006C160
			internal SpecificConstGenericGenerator(_ISignature sign, _ICompileContext compileContext, IEnumerable<_IExpression> genericValues, global::\u000E.\u0017 context)
			{
				this.GenericSignature = sign;
				this.CompileContext = compileContext;
				this.GenericValues = genericValues;
				this.Context = context;
			}

			// Token: 0x0600206E RID: 8302 RVA: 0x0006DF88 File Offset: 0x0006C188
			private void \u0002(_ISignature \u0002)
			{
				string name = global::\u0018.\u0003.\u0001(\u0002.OrgName, this.GenericValues);
				\u0002.Name = name;
				foreach (_IVariable ivariable in \u0002.All.Where(new Func<IVariable, bool>(ConstGenericController.SpecificConstGenericGenerator.<>c.<>9.\u0001)).OfType<_IVariable>())
				{
					ivariable.SetFlag(VarFlag.Generic, false);
					ivariable.SetFlag(VarFlag.ReplacedConstant, true);
					ivariable.SetFlag(VarFlag.Local, true);
					ivariable.SetFlag(VarFlag.Constant, true);
					ivariable.SetFlag(VarFlag.GenericConstant, true);
				}
			}

			// Token: 0x0600206F RID: 8303 RVA: 0x0006E04C File Offset: 0x0006C24C
			private _ISignature \u0001(_ISignature \u0002, out ISignature[] \u0003, out _IPreCompileContext \u0004)
			{
				return Helper.\u0001(this.CompileContext, \u0002, APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(this.CompileContext.ApplicationGuid) as _IPreCompileContext, APEnvironmentFacade.Instance.LanguageModelMgr.Pool, out \u0003, out \u0004);
			}

			// Token: 0x06002070 RID: 8304 RVA: 0x0006E08C File Offset: 0x0006C28C
			private static string \u0001(string \u0002, _ISignature \u0003, _ICompileContext \u0004)
			{
				string[] array = \u0003.GetSearchName(\u0004).Split(new char[]
				{
					'.'
				});
				int num = array.Length - 1;
				if (string.Compare(array[num], \u0003.OrgName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					array[num] = \u0002;
				}
				return string.Join(".", array);
			}

			// Token: 0x06002071 RID: 8305 RVA: 0x0006E0D8 File Offset: 0x0006C2D8
			internal void \u0001(_ICompileContext \u0002, \u001A.\u0013 \u0003, ConstGenericController \u0004)
			{
				string text = ConstGenericController.SpecificConstGenericGenerator.\u0001(global::\u0018.\u0003.\u0001(this.GenericSignature.OrgName, this.GenericValues), this.GenericSignature, this.CompileContext);
				this.SpecificSignature = this.CompileContext[text];
				if (this.SpecificSignature != null)
				{
					return;
				}
				ISignature[] source;
				_IPreCompileContext ipreCompileContext;
				_ISignature isignature = this.\u0001(this.GenericSignature, out source, out ipreCompileContext);
				_ISignature isignature2 = ((\u0002 != null) ? \u0002.GetSignature(text) : null) as _ISignature;
				this.SpecificSignature = isignature.CreateCompiledSignature(isignature2, true);
				this.SpecificSignature.SetBaseSignatureId(this.GenericSignature.BaseSignatureId);
				this.\u0002(this.SpecificSignature);
				this.SpecificSignature.AddAttribute("search_name", text);
				this.SpecificSignature.SetFlag(SignatureFlag.Generated, true);
				this.SpecificSignature.ObjectGuid = ((isignature2 != null) ? isignature2.ObjectGuid : Guid.NewGuid());
				List<_ISignature> list = new List<_ISignature>();
				list.AddRange(source.OfType<_ISignature>());
				((_ISignature4)this.GenericSignature).CreateOverloadPlaceholderSignatures(list);
				List<Tuple<_ISignature, _ISignature>> list2 = new List<Tuple<_ISignature, _ISignature>>();
				foreach (_ISignature isignature3 in list)
				{
					_ISignature isignature4 = this.SpecificSignature.GetSubSignature(isignature3.Name) as _ISignature;
					if (isignature4 != null)
					{
						this.CompileContext.RemoveSignature(isignature4);
						this.SpecificSignature.RemoveSubSignature(isignature4);
					}
					_ISignature signOld = ((isignature2 != null) ? isignature2.GetSubSignature(isignature3.Name) : null) as _ISignature;
					_ISignature isignature5 = isignature3.CreateCompiledSignature(signOld, true);
					this.SpecificSignature.AddSubSignature(isignature5);
					list2.Add(new Tuple<_ISignature, _ISignature>(isignature5, isignature3));
				}
				this.CompileContext.AddSignature(this.SpecificSignature, isignature2, \u0002);
				\u0004.ProcessedGenericSignature[this.GenericSignature] = this.SpecificSignature;
				_ICompiledPOU icompiledPOU = ipreCompileContext.GetCompiledPOU(isignature.ObjectGuid) as _ICompiledPOU;
				if (icompiledPOU != null)
				{
					_ICompiledPOU icompiledPOU2 = ConstGenericController.SpecificConstGenericGenerator.\u0001(icompiledPOU);
					icompiledPOU2.ObjectGuid = this.SpecificSignature.ObjectGuid;
					icompiledPOU2.SetFlag(CompiledPOUFlags.NotForUpToDate, true);
					this.CompileContext.AddCompiledPOU(icompiledPOU2, this.SpecificSignature, null);
				}
				foreach (Tuple<_ISignature, _ISignature> tuple in list2)
				{
					_ISignature item = tuple.Item1;
					_ISignature item2 = tuple.Item2;
					_ISignature isignature6 = ((isignature2 != null) ? isignature2.GetSubSignature(item.Name) : null) as _ISignature;
					item.ParentSignatureId = this.SpecificSignature.Id;
					item.ObjectGuid = ((isignature6 != null) ? isignature6.ObjectGuid : Guid.NewGuid());
					this.CompileContext.AddSignature(item, isignature6, \u0002, true);
					_ICompiledPOU icompiledPOU3 = ipreCompileContext.GetCompiledPOU(item2.ObjectGuid) as _ICompiledPOU;
					if (icompiledPOU3 != null)
					{
						_ICompiledPOU icompiledPOU4 = ConstGenericController.SpecificConstGenericGenerator.\u0001(icompiledPOU3);
						icompiledPOU4.SignatureId = item.Id;
						this.CompileContext.AddCompiledPOU(icompiledPOU4, item, true, \u0002);
						item.SetFlag(SignatureFlag.Generated, true);
						icompiledPOU4.ObjectGuid = item.ObjectGuid;
						icompiledPOU4.SetFlag(CompiledPOUFlags.NotForUpToDate, true);
					}
					this.SpecificSignature.AddSubSignature(item);
				}
				this.\u0001(this.SpecificSignature, \u0003, ipreCompileContext);
			}

			// Token: 0x06002072 RID: 8306 RVA: 0x0006E450 File Offset: 0x0006C650
			private static _ICompiledPOU \u0001(_ICompiledPOU \u0002)
			{
				_ICompiledPOU icompiledPOU = \u0002.CreateCompiledPOU();
				if (icompiledPOU.GetParseTree() is IEmptyStatement)
				{
					_IStatement parseTree = \u0002.GetParseTree();
					icompiledPOU.SetParseTree(parseTree);
				}
				return icompiledPOU;
			}

			// Token: 0x06002073 RID: 8307 RVA: 0x0006E480 File Offset: 0x0006C680
			private IScope5 \u0001(_ISignature \u0002, _IPreCompileContext \u0003)
			{
				\u0081.\u0008 u = this.Context.Scope as \u0081.\u0008;
				if (u != null)
				{
					return u.\u0001(\u0002, \u0003);
				}
				return global::\u0007.\u0005.\u0001(this.Context.Comcon, \u0002.Id);
			}

			// Token: 0x06002074 RID: 8308 RVA: 0x0006E4C0 File Offset: 0x0006C6C0
			private void \u0001(_ISignature \u0002, \u001A.\u0013 \u0003, _IPreCompileContext \u0004)
			{
				\u0002.SetFlag(SignatureFlag.Temp, true);
				IScope5 scope = this.\u0001(\u0002, \u0004);
				\u0003.\u0004(\u0002, scope);
				\u0002.SetFlag(SignatureFlag.Typified, true);
				foreach (_ISignature isignature in \u0002.SubSignatures.OfType<_ISignature>())
				{
					if (!isignature.GetFlag(SignatureFlag.Typified))
					{
						scope.MethodSignature = isignature;
						isignature.SetFlag(SignatureFlag.Temp, true);
						\u0003.\u0004(isignature, scope);
						isignature.SetFlag(SignatureFlag.Typified, true);
						isignature.SetFlag(SignatureFlag.Temp, false);
					}
				}
				\u0002.SetFlag(SignatureFlag.Temp, false);
			}

			// Token: 0x04000549 RID: 1353
			[CompilerGenerated]
			private readonly global::\u000E.\u0017 \u0001;

			// Token: 0x0400054A RID: 1354
			[CompilerGenerated]
			private readonly _ISignature \u0001;

			// Token: 0x0400054B RID: 1355
			[CompilerGenerated]
			private readonly _ICompileContext \u0001;

			// Token: 0x0400054C RID: 1356
			[CompilerGenerated]
			private readonly IEnumerable<_IExpression> \u0001;

			// Token: 0x0400054D RID: 1357
			[CompilerGenerated]
			private _ISignature \u0002;
		}

		// Token: 0x020001C1 RID: 449
		private sealed class ConstGenericAdapter
		{
			// Token: 0x1700060F RID: 1551
			// (get) Token: 0x06002078 RID: 8312 RVA: 0x0006E5B4 File Offset: 0x0006C7B4
			private _ICompileContext ComconNew
			{
				get
				{
					return this.CompileInformation.ComconNew;
				}
			}

			// Token: 0x17000610 RID: 1552
			// (get) Token: 0x06002079 RID: 8313 RVA: 0x0006E5C4 File Offset: 0x0006C7C4
			private _ICompileContext ComconOld
			{
				get
				{
					return this.CompileInformation.ComconOld;
				}
			}

			// Token: 0x17000611 RID: 1553
			// (get) Token: 0x0600207A RID: 8314 RVA: 0x0006E5D4 File Offset: 0x0006C7D4
			private _ISignature SignWithDeclarations { get; }

			// Token: 0x17000612 RID: 1554
			// (get) Token: 0x0600207B RID: 8315 RVA: 0x0006E5DC File Offset: 0x0006C7DC
			// (set) Token: 0x0600207C RID: 8316 RVA: 0x0006E5E4 File Offset: 0x0006C7E4
			private _IVariable CurrentVariable { get; set; }

			// Token: 0x17000613 RID: 1555
			// (get) Token: 0x0600207D RID: 8317 RVA: 0x0006E5F0 File Offset: 0x0006C7F0
			// (set) Token: 0x0600207E RID: 8318 RVA: 0x0006E5F8 File Offset: 0x0006C7F8
			private bool ErrorOccured { get; set; }

			// Token: 0x17000614 RID: 1556
			// (get) Token: 0x0600207F RID: 8319 RVA: 0x0006E604 File Offset: 0x0006C804
			private \u001B CompileInformation { get; }

			// Token: 0x06002080 RID: 8320 RVA: 0x0006E60C File Offset: 0x0006C80C
			internal ConstGenericAdapter(\u001B ci, _ISignature signWithDeclarations)
			{
				this.CompileInformation = ci;
				this.SignWithDeclarations = signWithDeclarations;
				this.CurrentVariable = null;
				this.ErrorOccured = false;
			}

			// Token: 0x06002081 RID: 8321 RVA: 0x0006E630 File Offset: 0x0006C830
			internal bool \u0002()
			{
				IEnumerable<_IVariable> enumerable = this.SignWithDeclarations.AllVariables.OfType<_IVariable>();
				_IScope u = global::\u0007.\u0005.\u0001(this.ComconNew, this.SignWithDeclarations.Id) as _IScope;
				foreach (_IVariable ivariable in enumerable)
				{
					this.CurrentVariable = ivariable;
					_IType itype = this.\u0001(u, ivariable.CompiledType as _IType);
					if (itype != null)
					{
						ivariable.SetType(itype);
					}
					this.CurrentVariable = null;
				}
				if (this.SignWithDeclarations.BaseExpression != null)
				{
					_IType itype2 = this.\u0001(u, this.SignWithDeclarations.BaseExpression.Type as _IType);
					if (itype2 != null)
					{
						((_IExpression)this.SignWithDeclarations.BaseExpression)._CompiledType = itype2;
					}
				}
				return !this.ErrorOccured;
			}

			// Token: 0x06002082 RID: 8322 RVA: 0x0006E718 File Offset: 0x0006C918
			private void \u0001(_ISignature \u0002, _ISignature \u0003)
			{
				if (this.CompileInformation.OnlineChange && \u0003 != null)
				{
					_IScope scope = global::\u0007.\u0005.\u0001(this.ComconNew, \u0002.Id) as _IScope;
					_IScope scope2 = global::\u0007.\u0005.\u0001(this.ComconOld, \u0003.Id) as _IScope;
					_IVariable[] array = \u0002.\u0001().ToArray<_IVariable>();
					_IVariable[] array2 = \u0003.\u0001().ToArray<_IVariable>();
					for (int i = 0; i < array.Length; i++)
					{
						_IVariable ivariable = array[i];
						_IVariable ivariable2 = array2[i];
						_ILiteralValue iliteralValue = (_ILiteralValue)ivariable.Initial.Literal(scope);
						_ILiteralValue iliteralValue2 = (_ILiteralValue)ivariable2.Initial.Literal(scope2);
						if (iliteralValue != null && iliteralValue2 != null && !iliteralValue.IsValueEqual(iliteralValue2))
						{
							if (this.CurrentVariable != null)
							{
								this.SignWithDeclarations.AddMessage(this.CurrentVariable._SourcePosition, Severity.Error, MessageId.Err_NoOnlineChangeOnGenericConstantType, new object[]
								{
									this.CurrentVariable.OrgName,
									array[i].OrgName,
									ivariable2.Initial.ToString(),
									ivariable.Initial.ToString()
								});
							}
							else
							{
								this.SignWithDeclarations.\u0001(((_IExpression)this.SignWithDeclarations.BaseExpression).Position, Severity.Error, MessageId.Err_NoOnlineChangeOnGenericConstantBaseType, new object[]
								{
									this.SignWithDeclarations.BaseExpression.ToString(),
									array[i].OrgName,
									ivariable2.Initial.ToString(),
									ivariable.Initial.ToString()
								});
							}
							this.ErrorOccured = true;
						}
					}
				}
			}

			// Token: 0x06002083 RID: 8323 RVA: 0x0006E8C0 File Offset: 0x0006CAC0
			internal bool \u0003()
			{
				IEnumerable<_IVariable> enumerable = this.SignWithDeclarations.AllVariables.OfType<_IVariable>();
				_IScope u = global::\u0007.\u0005.\u0001(this.ComconNew, this.SignWithDeclarations.Id) as _IScope;
				foreach (_IVariable ivariable in enumerable)
				{
					if (ivariable.Type is IPointerType || ivariable.Type is IReferenceType)
					{
						_IType itype = this.\u0002(u, ivariable.CompiledType as _IType);
						if (itype != null)
						{
							ivariable.SetType(itype);
						}
					}
				}
				return true;
			}

			// Token: 0x06002084 RID: 8324 RVA: 0x0006E964 File Offset: 0x0006CB64
			private void \u0001(_ISignature \u0002)
			{
				foreach (_ISignature isignature in \u0002.SubSignatures.OfType<_ISignature>())
				{
					_IVariable ivariable = isignature[IdentifierConstants.InstancePointer] as _IVariable;
					if (ivariable != null)
					{
						((_IUserdefType)((_IPointerType)ivariable.Type).Base).NameExpression = global::\u0019.\u0003.\u0001(\u0002.OrgName);
					}
				}
			}

			// Token: 0x06002085 RID: 8325 RVA: 0x0006E9E8 File Offset: 0x0006CBE8
			private void \u0001(_ISignature \u0002, _IScope \u0003, IEnumerable<_IExpression> \u0004)
			{
				LList<_ILiteralValue> llist = new LList<_ILiteralValue>();
				this.\u0001(\u0003, llist, \u0004);
				string name = global::\u0018.\u0003.\u0001(\u0002.OrgName, llist);
				\u0002.Name = name;
				IEnumerable<_IVariable> enumerable = \u0002.All.Where(new Func<IVariable, bool>(ConstGenericController.ConstGenericAdapter.<>c.<>9.\u0001)).OfType<_IVariable>();
				_IVariable[] array = (enumerable as _IVariable[]) ?? enumerable.ToArray<_IVariable>();
				Debug.\u0001(array.Count<_IVariable>() == llist.Count);
				int num = 0;
				foreach (_IVariable ivariable in array)
				{
					_IExpression initial = global::\u0010.\u0004.\u0001(llist[num], ivariable);
					ivariable.SetInitial(initial);
					num++;
				}
				this.\u0001(\u0002);
			}

			// Token: 0x06002086 RID: 8326 RVA: 0x0006EAB0 File Offset: 0x0006CCB0
			private void \u0001(_IScope \u0002, IList<_ILiteralValue> \u0003, IEnumerable<_IExpression> \u0004)
			{
				foreach (_IExpression iexpression in \u0004)
				{
					bool flag;
					_ILiteralValue iliteralValue = iexpression.LiteralWithRecursionCheck(\u0002, new Dictionary<IVariable, IVariable>(), true, out flag) as _ILiteralValue;
					Debug.\u0001(iliteralValue != null);
					Debug.\u0001(!flag);
					\u0003.Add(iliteralValue);
				}
			}

			// Token: 0x06002087 RID: 8327 RVA: 0x0006EB20 File Offset: 0x0006CD20
			private _IType \u0001(_IScope \u0002, _IType \u0003)
			{
				IGenericUserdefType genericUserdefType = \u0003 as IGenericUserdefType;
				if (genericUserdefType != null)
				{
					return this.\u0002(\u0002, genericUserdefType);
				}
				_IArrayType iarrayType = \u0003 as _IArrayType;
				if (iarrayType == null)
				{
					return \u0003;
				}
				return this.\u0002(\u0002, iarrayType);
			}

			// Token: 0x06002088 RID: 8328 RVA: 0x0006EB58 File Offset: 0x0006CD58
			private _IType \u0002(_IScope \u0002, _IType \u0003)
			{
				IGenericUserdefType genericUserdefType = \u0003 as IGenericUserdefType;
				if (genericUserdefType != null)
				{
					return this.\u0001(\u0002, genericUserdefType);
				}
				_IArrayType iarrayType = \u0003 as _IArrayType;
				if (iarrayType != null)
				{
					return this.\u0001(\u0002, iarrayType);
				}
				_IPointerType ipointerType = \u0003 as _IPointerType;
				if (ipointerType != null)
				{
					return this.\u0001(\u0002, ipointerType);
				}
				_IReferenceType ireferenceType = \u0003 as _IReferenceType;
				if (ireferenceType == null)
				{
					return \u0003;
				}
				return this.\u0001(\u0002, ireferenceType);
			}

			// Token: 0x06002089 RID: 8329 RVA: 0x0006EBB4 File Offset: 0x0006CDB4
			private _IType \u0001(_IScope \u0002, _IReferenceType \u0003)
			{
				_IType itype = this.\u0002(\u0002, \u0003._Base);
				if (itype != null)
				{
					\u0003._Base = itype;
				}
				return \u0003;
			}

			// Token: 0x0600208A RID: 8330 RVA: 0x0006EBDC File Offset: 0x0006CDDC
			private _IType \u0001(_IScope \u0002, _IPointerType \u0003)
			{
				_IType itype = this.\u0002(\u0002, \u0003._Base);
				if (itype != null)
				{
					\u0003._Base = itype;
				}
				return \u0003;
			}

			// Token: 0x0600208B RID: 8331 RVA: 0x0006EC04 File Offset: 0x0006CE04
			private _IType \u0001(_IScope \u0002, _IArrayType \u0003)
			{
				_IType itype = this.\u0002(\u0002, \u0003._Base);
				if (itype != null)
				{
					\u0003._Base = itype;
				}
				return \u0003;
			}

			// Token: 0x0600208C RID: 8332 RVA: 0x0006EC2C File Offset: 0x0006CE2C
			private _IType \u0002(_IScope \u0002, _IArrayType \u0003)
			{
				_IType itype = this.\u0001(\u0002, \u0003._Base);
				if (itype != null)
				{
					\u0003._Base = itype;
				}
				return \u0003;
			}

			// Token: 0x0600208D RID: 8333 RVA: 0x0006EC54 File Offset: 0x0006CE54
			private _IType \u0001(_IScope \u0002, IGenericUserdefType \u0003)
			{
				_ISignature isignature = (_ISignature)this.ComconNew.GetSignatureById(\u0003.SignatureId);
				return this.\u0002(\u0002, \u0003);
			}

			// Token: 0x0600208E RID: 8334 RVA: 0x0006EC78 File Offset: 0x0006CE78
			private _IType \u0002(_IScope \u0002, IGenericUserdefType \u0003)
			{
				_ISignature isignature = (_ISignature)this.ComconNew.GetSignatureById(\u0003.SignatureId);
				if (!isignature.GetFlag(SignatureFlag.Located))
				{
					this.\u0001(isignature, \u0002, \u0003.GenericConstantsInitializations);
					if (isignature.GetFlagInternal(SignatureFlagInternal.ContainsGenericInstanceVar) && !new ConstGenericController.ConstGenericAdapter(this.CompileInformation, isignature).\u0002())
					{
						return null;
					}
					_ICompileContext icompileContext = this.ComconOld;
					_ISignature isignature2 = ((icompileContext != null) ? icompileContext.GetSignatureById(isignature.Id) : null) as _ISignature;
					this.\u0001(isignature, isignature2);
					if (!Locator.\u0001(this.ComconNew.DataManager, this.ComconNew, this.ComconOld, isignature, isignature2))
					{
						return null;
					}
				}
				string u = \u0003.ToString();
				IGenericUserdefType2 genericUserdefType = \u0003 as IGenericUserdefType2;
				if (genericUserdefType != null)
				{
					u = genericUserdefType.OriginalDeclaration;
				}
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(u);
				iuserdefType.SignatureId = isignature.Id;
				isignature.AddDeclarer(this.SignWithDeclarations.Id);
				return iuserdefType;
			}

			// Token: 0x04000550 RID: 1360
			[CompilerGenerated]
			private readonly _ISignature \u0001;

			// Token: 0x04000551 RID: 1361
			[CompilerGenerated]
			private _IVariable \u0001;

			// Token: 0x04000552 RID: 1362
			[CompilerGenerated]
			private bool \u0001;

			// Token: 0x04000553 RID: 1363
			[CompilerGenerated]
			private readonly \u001B \u0001;
		}
	}
}
