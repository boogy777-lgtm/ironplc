using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0004;
using \u0006;
using \u0007;
using \u000E;
using \u0011;
using \u0014;
using \u0016;
using \u0017;
using \u0019;
using \u001D;
using \u001F;
using _3S.CoDeSys.Compiler35220.Compile.Phase1_Typification.Code;
using _3S.CoDeSys.Compiler35220.Compile.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;
using \u0082;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase4_TypeCheck
{
	// Token: 0x020002E1 RID: 737
	internal sealed class TypeCheckerVisitor : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2, IOperatorExpressionVisitor6, IOperatorExpressionVisitor5, IOperatorExpressionVisitor4, IOperatorExpressionVisitor3, IOperatorExpressionVisitor2, IOperatorExpressionVisitor, \u001F.\u0001, ITypeChecker, global::\u0017.\u0010, IErrorAdder
	{
		// Token: 0x1700079E RID: 1950
		// (get) Token: 0x06002C2A RID: 11306 RVA: 0x0009AF44 File Offset: 0x00099144
		// (set) Token: 0x06002C2B RID: 11307 RVA: 0x0009AF4C File Offset: 0x0009914C
		private \u001F.\u0007 TypeCheckerInternal { get; set; }

		// Token: 0x1700079F RID: 1951
		// (get) Token: 0x06002C2C RID: 11308 RVA: 0x0009AF58 File Offset: 0x00099158
		// (set) Token: 0x06002C2D RID: 11309 RVA: 0x0009AF60 File Offset: 0x00099160
		public bool TreatReferenceAsPointer { get; set; }

		// Token: 0x170007A0 RID: 1952
		// (get) Token: 0x06002C2E RID: 11310 RVA: 0x0009AF6C File Offset: 0x0009916C
		// (set) Token: 0x06002C2F RID: 11311 RVA: 0x0009AF74 File Offset: 0x00099174
		public bool InterfaceAsInterface { get; set; }

		// Token: 0x170007A1 RID: 1953
		// (get) Token: 0x06002C30 RID: 11312 RVA: 0x0009AF80 File Offset: 0x00099180
		// (set) Token: 0x06002C31 RID: 11313 RVA: 0x0009AF90 File Offset: 0x00099190
		public bool ConvertAllTypeMismatches
		{
			get
			{
				return this.TypeCheckerInternal.ConvertAllTypeMismatches;
			}
			set
			{
				this.TypeCheckerInternal.ConvertAllTypeMismatches = value;
			}
		}

		// Token: 0x170007A2 RID: 1954
		// (get) Token: 0x06002C32 RID: 11314 RVA: 0x0009AFA0 File Offset: 0x000991A0
		// (set) Token: 0x06002C33 RID: 11315 RVA: 0x0009AFB0 File Offset: 0x000991B0
		public IMessageSuppressionController MessageSuppressionController
		{
			get
			{
				return this.TypeCheckerInternal.MessageSuppressionController;
			}
			set
			{
				this.TypeCheckerInternal.MessageSuppressionController = value;
			}
		}

		// Token: 0x170007A3 RID: 1955
		// (get) Token: 0x06002C34 RID: 11316 RVA: 0x0009AFC0 File Offset: 0x000991C0
		// (set) Token: 0x06002C35 RID: 11317 RVA: 0x0009AFD0 File Offset: 0x000991D0
		public Guid MessageGuid
		{
			get
			{
				return this.TypeCheckerInternal.MessageGuid;
			}
			set
			{
				this.TypeCheckerInternal.MessageGuid = value;
			}
		}

		// Token: 0x170007A4 RID: 1956
		// (get) Token: 0x06002C36 RID: 11318 RVA: 0x0009AFE0 File Offset: 0x000991E0
		// (set) Token: 0x06002C37 RID: 11319 RVA: 0x0009AFF0 File Offset: 0x000991F0
		public bool InImplicitCode
		{
			get
			{
				return this.TypeCheckerInternal.InImplicitCode;
			}
			set
			{
				this.TypeCheckerInternal.InImplicitCode = value;
			}
		}

		// Token: 0x170007A5 RID: 1957
		// (get) Token: 0x06002C38 RID: 11320 RVA: 0x0009B000 File Offset: 0x00099200
		// (set) Token: 0x06002C39 RID: 11321 RVA: 0x0009B010 File Offset: 0x00099210
		public bool AddCrossReferences
		{
			get
			{
				return this.TypeCheckerInternal.AddCrossReferences;
			}
			set
			{
				this.TypeCheckerInternal.AddCrossReferences = value;
			}
		}

		// Token: 0x170007A6 RID: 1958
		// (get) Token: 0x06002C3A RID: 11322 RVA: 0x0009B020 File Offset: 0x00099220
		// (set) Token: 0x06002C3B RID: 11323 RVA: 0x0009B028 File Offset: 0x00099228
		public bool CheckingInitialValue { get; internal set; }

		// Token: 0x170007A7 RID: 1959
		// (get) Token: 0x06002C3C RID: 11324 RVA: 0x0009B034 File Offset: 0x00099234
		// (set) Token: 0x06002C3D RID: 11325 RVA: 0x0009B03C File Offset: 0x0009923C
		public ISignature InitialValueContextSignature { get; internal set; }

		// Token: 0x06002C3E RID: 11326 RVA: 0x0009B048 File Offset: 0x00099248
		internal TypeCheckerVisitor()
		{
			this.\u0001 = new ObjectPool<TypeCheckerVisitor.ETypeStackContent>(new Func<TypeCheckerVisitor.ETypeStackContent>(TypeCheckerVisitor.<>c.<>9.\u0001));
			base..ctor();
		}

		// Token: 0x06002C3F RID: 11327 RVA: 0x0009B088 File Offset: 0x00099288
		public TypeCheckerVisitor(IScope5 scope, _ICompileContext comcon)
		{
			this.\u0001 = new ObjectPool<TypeCheckerVisitor.ETypeStackContent>(new Func<TypeCheckerVisitor.ETypeStackContent>(TypeCheckerVisitor.<>c.<>9.\u0002));
			base..ctor();
			this.\u0001(scope, comcon);
		}

		// Token: 0x06002C40 RID: 11328 RVA: 0x0009B0D8 File Offset: 0x000992D8
		public TypeCheckerVisitor(IScope5 scope, _ICompileContext comcon, bool bWriteConstants)
		{
			this.\u0001 = new ObjectPool<TypeCheckerVisitor.ETypeStackContent>(new Func<TypeCheckerVisitor.ETypeStackContent>(TypeCheckerVisitor.<>c.<>9.\u0003));
			base..ctor();
			this.\u0001(scope, comcon, bWriteConstants);
		}

		// Token: 0x06002C41 RID: 11329 RVA: 0x0009B12C File Offset: 0x0009932C
		public TypeCheckerVisitor(IScope5 scope, _ICompileContext comcon, bool bWriteConstants, _ICompiledPOU cpou, bool bAddCrossReferences)
		{
			this.\u0001 = new ObjectPool<TypeCheckerVisitor.ETypeStackContent>(new Func<TypeCheckerVisitor.ETypeStackContent>(TypeCheckerVisitor.<>c.<>9.\u0004));
			base..ctor();
			this.\u0001(scope, comcon, bWriteConstants, cpou, bAddCrossReferences);
		}

		// Token: 0x06002C42 RID: 11330 RVA: 0x0009B184 File Offset: 0x00099384
		internal void \u0001(IScope5 \u0002, _ICompileContext \u0003)
		{
			this.\u0001(\u0002, \u0003, false, null, false);
		}

		// Token: 0x06002C43 RID: 11331 RVA: 0x0009B194 File Offset: 0x00099394
		internal void \u0001(IScope5 \u0002, _ICompileContext \u0003, bool \u0004)
		{
			this.\u0001(\u0002, \u0003, \u0004, null, false);
		}

		// Token: 0x06002C44 RID: 11332 RVA: 0x0009B1A4 File Offset: 0x000993A4
		internal void \u0001(IScope5 \u0002, _ICompileContext \u0003, bool \u0004, _ICompiledPOU \u0005, bool \u0006)
		{
			this.\u0001(\u0002);
			this.\u0001 = \u0003;
			this.\u0001 = (\u0002.LocalSignature as _ISignature);
			if (\u0002.MethodSignature != null)
			{
				this.\u0001 = (this.Scope.MethodSignature as _ISignature);
			}
			this.\u0002 = \u0004;
			this.\u0001 = \u0005;
			this.TypeCheckerInternal = new global::\u0014.\u0012(\u0002 as _IScope2, \u0003, \u0005);
			this.AddCrossReferences = \u0006;
		}

		// Token: 0x06002C45 RID: 11333 RVA: 0x0009B21C File Offset: 0x0009941C
		public void \u0001()
		{
			this.\u0001.Clear();
			this.\u0001 = null;
			this.\u0001 = false;
			this.\u0002 = false;
			this.\u0001 = null;
			this.AddCrossReferences = false;
			this.\u0001 = null;
			this.TreatReferenceAsPointer = false;
			this.InterfaceAsInterface = false;
			this.ConvertAllTypeMismatches = false;
			this.InImplicitCode = false;
			this.CheckingInitialValue = false;
			this.InitialValueContextSignature = null;
		}

		// Token: 0x170007A8 RID: 1960
		// (get) Token: 0x06002C46 RID: 11334 RVA: 0x0009B288 File Offset: 0x00099488
		private TypeCheckerVisitor.ETypeStackContent TopOfStack
		{
			get
			{
				return this.\u0001.Peek();
			}
		}

		// Token: 0x170007A9 RID: 1961
		// (get) Token: 0x06002C47 RID: 11335 RVA: 0x0009B298 File Offset: 0x00099498
		private IScope5 Scope
		{
			get
			{
				return this.TopOfStack.\u0001;
			}
		}

		// Token: 0x170007AA RID: 1962
		// (get) Token: 0x06002C48 RID: 11336 RVA: 0x0009B2A8 File Offset: 0x000994A8
		public Guid ApplicationGuid
		{
			get
			{
				if (this.\u0001 == null)
				{
					return Guid.Empty;
				}
				return this.\u0001.ApplicationGuid;
			}
		}

		// Token: 0x170007AB RID: 1963
		// (get) Token: 0x06002C49 RID: 11337 RVA: 0x0009B2C4 File Offset: 0x000994C4
		public bool TreatLRealAsReal
		{
			get
			{
				return this.\u0001 != null && this.\u0001.TreatLRealAsReal;
			}
		}

		// Token: 0x170007AC RID: 1964
		// (get) Token: 0x06002C4A RID: 11338 RVA: 0x0009B2DC File Offset: 0x000994DC
		public bool TreatInt64AsInt32
		{
			get
			{
				return this.\u0001 != null && this.\u0001.TreatInt64AsInt32;
			}
		}

		// Token: 0x06002C4B RID: 11339 RVA: 0x0009B2F4 File Offset: 0x000994F4
		public bool \u0001(_IImplicitConversionExpression \u0002)
		{
			bool result = false;
			if (this.\u0001 != null && this.\u0001.Codegenerator != null)
			{
				string empty = string.Empty;
				TypeClass typeClass = TypeClass.None;
				if (ImplicitFunctionCallsHandler.\u0001(this.\u0001, \u0002, this.Scope, ref empty, ref typeClass))
				{
					result = true;
					IList<ISignature> list = this.Scope[empty];
					if (this.\u0001.ApplicationGuid != Guid.Empty)
					{
						Debug.\u0001(list != null && list.Count == 1);
						ISignature signature = list[0];
						if (this.\u0001 != null && this.\u0001.SignatureId != signature.Id)
						{
							this.\u0001((_ISignature)signature);
						}
						if (this.Scope.MethodSignature != null)
						{
							this.\u0001((_ISignature)this.Scope.MethodSignature, signature.Id);
						}
						else if (this.Scope.LocalSignature != null && this.Scope.LocalSignature.Id != signature.Id)
						{
							this.\u0001((_ISignature)this.Scope.LocalSignature, signature.Id);
						}
					}
				}
			}
			return result;
		}

		// Token: 0x170007AD RID: 1965
		// (get) Token: 0x06002C4C RID: 11340 RVA: 0x0009B424 File Offset: 0x00099624
		public bool NoConversionChecks
		{
			get
			{
				return this.\u0001 != null && this.\u0001.IsDefined("NO_3_0_CONVERSION_CHECKS");
			}
		}

		// Token: 0x06002C4D RID: 11341 RVA: 0x0009B440 File Offset: 0x00099640
		private void \u0001(IScope5 \u0002, bool \u0003)
		{
			TypeCheckerVisitor.ETypeStackContent @object = this.\u0001.GetObject();
			@object.\u0001 = \u0002;
			@object.\u0001 = \u0003;
			if (this.\u0001.Count > 0)
			{
				@object.\u0003 = this.TopOfStack.\u0003;
				@object.\u0001 = this.TopOfStack.\u0001;
			}
			else
			{
				@object.\u0003 = false;
			}
			@object.\u0004 = false;
			this.\u0001.Push(@object);
		}

		// Token: 0x06002C4E RID: 11342 RVA: 0x0009B4B4 File Offset: 0x000996B4
		private void \u0007(bool \u0002)
		{
			TypeCheckerVisitor.ETypeStackContent @object = this.\u0001.GetObject();
			@object.\u0001 = this.TopOfStack.\u0001;
			@object.\u0001 = this.TopOfStack.\u0001;
			@object.\u0003 = this.TopOfStack.\u0003;
			@object.\u0004 = false;
			@object.\u0006 = \u0002;
			@object.\u0001 = this.TopOfStack.\u0001;
			this.\u0001.Push(@object);
		}

		// Token: 0x06002C4F RID: 11343 RVA: 0x0009B52C File Offset: 0x0009972C
		private void \u0002()
		{
			TypeCheckerVisitor.ETypeStackContent @object = this.\u0001.GetObject();
			@object.\u0001 = this.TopOfStack.\u0001;
			@object.\u0001 = this.TopOfStack.\u0001;
			@object.\u0003 = this.TopOfStack.\u0003;
			@object.\u0006 = this.TopOfStack.\u0006;
			@object.\u0004 = false;
			@object.\u0001 = this.TopOfStack.\u0001;
			this.\u0001.Push(@object);
		}

		// Token: 0x06002C50 RID: 11344 RVA: 0x0009B5B0 File Offset: 0x000997B0
		private void \u0001(IScope5 \u0002)
		{
			this.\u0001(\u0002, true);
		}

		// Token: 0x06002C51 RID: 11345 RVA: 0x0009B5BC File Offset: 0x000997BC
		private void \u0003()
		{
			this.\u0001.PutObject(this.\u0001.Pop());
		}

		// Token: 0x06002C52 RID: 11346 RVA: 0x0009B5D4 File Offset: 0x000997D4
		public void AddError(_IExprement exp, MessageId mid, params object[] args)
		{
			this.TypeCheckerInternal.\u0001(exp, mid, args);
		}

		// Token: 0x06002C53 RID: 11347 RVA: 0x0009B5E4 File Offset: 0x000997E4
		public void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			Severity severity = Severity.Warning;
			MessageHandling messageHandling = this.MessageSuppressionController.HandleMessage(\u0003, Severity.Warning);
			if (messageHandling != MessageHandling.Suppress)
			{
				if (messageHandling == MessageHandling.Ignore)
				{
					return;
				}
			}
			else
			{
				severity = Severity.SuppressedWarning;
			}
			string format = global::\u000E.\u0018.\u0001(\u0003);
			\u0002.AddMessage(string.Format(format, \u0004), \u0002._Position, severity, \u0002.LengthIntern, \u0003);
		}

		// Token: 0x06002C54 RID: 11348 RVA: 0x0009B630 File Offset: 0x00099830
		public void \u0001(_IExprement \u0002, _ICompilerMessage \u0003)
		{
			this.TypeCheckerInternal.\u0001(\u0002, \u0003);
		}

		// Token: 0x06002C55 RID: 11349 RVA: 0x0009B640 File Offset: 0x00099840
		private bool \u0001(_IExpression \u0002, ICompiledType \u0003)
		{
			if (\u0002.Type == null)
			{
				string text = global::\u0003.\u0006.\u0001(MessageId.Err_UnknownType, new object[]
				{
					\u0002.ToString()
				});
				this.AddError(\u0002, MessageId.Err_TypeMismatch, new object[]
				{
					text,
					\u0003.ToString()
				});
				return false;
			}
			return true;
		}

		// Token: 0x06002C56 RID: 11350 RVA: 0x0009B68C File Offset: 0x0009988C
		private bool \u0001(Operator \u0002)
		{
			if (this.\u0001 != null)
			{
				return !this.\u0001.Contains(\u0002);
			}
			this.\u0001 = new HashSet<Operator>();
			ITargetSettings targetSettings = this.\u0001.GetTargetSettings();
			if (targetSettings == null)
			{
				return true;
			}
			string stringValue = global::\u0016.\u0004.UnsupportedOperators.GetStringValue(targetSettings);
			if (!string.IsNullOrEmpty(stringValue))
			{
				string[] array = stringValue.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					Operator operatorFromText = Scanner.GetOperatorFromText(array[i]);
					if (!this.\u0001.Contains(operatorFromText))
					{
						this.\u0001.Add(operatorFromText);
					}
				}
			}
			string stringValue2 = global::\u0016.\u0004.SupportedSystemOperators.GetStringValue(targetSettings);
			if (!string.IsNullOrEmpty(stringValue2))
			{
				string[] array = stringValue2.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					Operator operatorFromText2 = Scanner.GetOperatorFromText(array[i]);
					if (this.\u0001.Contains(operatorFromText2))
					{
						this.\u0001.Remove(operatorFromText2);
					}
				}
			}
			return this.\u0001(\u0002);
		}

		// Token: 0x06002C57 RID: 11351 RVA: 0x0009B794 File Offset: 0x00099994
		internal void \u0001(IScope5 \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005)
		{
			global::\u000E.\u000F.\u0001(this, \u0002 as ICommonScope, \u0003, \u0004, \u0005);
		}

		// Token: 0x06002C58 RID: 11352 RVA: 0x0009B7A8 File Offset: 0x000999A8
		internal bool \u0001(_IExpression \u0002, IScope5 \u0003)
		{
			if (!typeof(_IVariableExpression).IsAssignableFrom(\u0002.GetType()) && !typeof(_ICompoAccessExpression).IsAssignableFrom(\u0002.GetType()) && !typeof(_IGlobalScopeExpression).IsAssignableFrom(\u0002.GetType()))
			{
				return true;
			}
			if (typeof(_IUserdefType).IsAssignableFrom(\u0002._CompiledType.GetType()) || typeof(_IEnumType).IsAssignableFrom(\u0002._CompiledType.GetType()))
			{
				ICompiledType compiledType = global::\u0006.\u0011.\u0001(\u0002._CompiledType, \u0003);
				if (compiledType != null && typeof(_IEnumType).IsAssignableFrom(compiledType.GetType()) && \u0002.GetVariable(\u0003) == null)
				{
					this.AddError(\u0002, MessageId.Err_UnexpectedTypeName, new object[]
					{
						\u0002.ToString()
					});
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002C59 RID: 11353 RVA: 0x0009B884 File Offset: 0x00099A84
		internal static bool \u0001(TypeCheckerVisitor \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005, IScope5 \u0006, ref _IExpression \u0007)
		{
			bool flag;
			return TypeCheckerVisitor.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, ref \u0007, out flag);
		}

		// Token: 0x06002C5A RID: 11354 RVA: 0x0009B8A0 File Offset: 0x00099AA0
		internal static bool \u0001(TypeCheckerVisitor \u0002, _IExpression \u0003, ICompiledType \u0004, ICompiledType \u0005, _ICompileContext \u0006, IScope5 \u0007)
		{
			return global::\u0014.\u0012.\u0001(\u0002.TypeCheckerInternal, \u0003, \u0004, \u0005, \u0006, \u0007);
		}

		// Token: 0x06002C5B RID: 11355 RVA: 0x0009B8B4 File Offset: 0x00099AB4
		internal static bool \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004)
		{
			TypeClass @class = \u0004.Class;
			switch (@class)
			{
			case TypeClass.Any:
				return true;
			case TypeClass.AnyBit:
				return TypeTable.IsBit(\u0003.DeRefType.Class);
			case TypeClass.AnyDate:
				return TypeTable.IsTimeOrDateType(\u0003.DeRefType.Class);
			case TypeClass.AnyInt:
				return TypeTable.IsInteger(\u0003.DeRefType.Class);
			case TypeClass.AnyNum:
				return TypeTable.IsNumber(\u0003.DeRefType.Class);
			case TypeClass.AnyReal:
				return TypeTable.IsReal(\u0003.DeRefType.Class);
			default:
				if (@class != TypeClass.AnyString)
				{
					Debug.\u0001(false);
					return false;
				}
				return TypeTable.IsString(\u0003.DeRefType.Class);
			}
		}

		// Token: 0x06002C5C RID: 11356 RVA: 0x0009B960 File Offset: 0x00099B60
		internal static bool \u0001(TypeCheckerVisitor \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005, IScope5 \u0006, ref _IExpression \u0007, out bool \u0008)
		{
			return \u0002.\u0001(\u0003, \u0004, \u0005, \u0006, ref \u0007, out \u0008);
		}

		// Token: 0x06002C5D RID: 11357 RVA: 0x0009B974 File Offset: 0x00099B74
		public bool \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005, ref _IExpression \u0006)
		{
			return this.TypeCheckerInternal.\u0001(\u0002, \u0003, \u0004, \u0005, ref \u0006);
		}

		// Token: 0x06002C5E RID: 11358 RVA: 0x0009B988 File Offset: 0x00099B88
		public bool \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005, ref _IExpression \u0006, out bool \u0007)
		{
			return this.TypeCheckerInternal.\u0001(\u0002, \u0003, \u0004, \u0005, ref \u0006, out \u0007);
		}

		// Token: 0x06002C5F RID: 11359 RVA: 0x0009B9A0 File Offset: 0x00099BA0
		private void \u0001(_ISignature \u0002)
		{
			if (this.AddCrossReferences)
			{
				\u0002.AddCaller(this.\u0001.SignatureId);
			}
		}

		// Token: 0x06002C60 RID: 11360 RVA: 0x0009B9BC File Offset: 0x00099BBC
		private void \u0001(_ISignature \u0002, int \u0003)
		{
			if (this.AddCrossReferences)
			{
				\u0002.AddCallee(\u0003, false);
			}
		}

		// Token: 0x06002C61 RID: 11361 RVA: 0x0009B9D0 File Offset: 0x00099BD0
		internal static bool \u0001(TypeCheckerVisitor \u0002, _IExprement \u0003, ICompiledType \u0004, TypeClass \u0005, IScope5 \u0006, ref _IExpression \u0007)
		{
			_IExpression iexpression = \u0007;
			bool result = \u0002.\u0001(\u0003, \u0004, TypeTable.Get(\u0005), \u0006, ref \u0007);
			if (\u0002.CheckingInitialValue)
			{
				\u0007 = iexpression;
			}
			return result;
		}

		// Token: 0x06002C62 RID: 11362 RVA: 0x0009BA00 File Offset: 0x00099C00
		public bool \u0001(_IExprement \u0002, ICompiledType \u0003, TypeClass \u0004, IScope5 \u0005, ref _IExpression \u0006)
		{
			return this.TypeCheckerInternal.\u0001(\u0002, \u0003, \u0004, \u0005, ref \u0006);
		}

		// Token: 0x06002C63 RID: 11363 RVA: 0x0009BA14 File Offset: 0x00099C14
		private void \u0001(_IExprement \u0002)
		{
			if (this.\u0001)
			{
				return;
			}
			IList<_ICompilerMessage> messagesList = \u0002.MessagesList;
			if (messagesList == null || messagesList.Count == 0)
			{
				return;
			}
			for (int i = 0; i < messagesList.Count; i++)
			{
				if (messagesList[i].Severity == Severity.Error)
				{
					this.\u0001 = true;
					return;
				}
			}
		}

		// Token: 0x06002C64 RID: 11364 RVA: 0x0009BA68 File Offset: 0x00099C68
		public void \u0001(_ICompiledPOU \u0002)
		{
			\u0002.GetParseTree().Accept(this);
		}

		// Token: 0x06002C65 RID: 11365 RVA: 0x0009BA78 File Offset: 0x00099C78
		public void \u0001(_ISequenceStatement \u0002)
		{
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				try
				{
					if (istatement is ICaseLabelStatement && !this.TopOfStack.\u0006)
					{
						this.AddError(istatement, MessageId.Err_CaseLabelOutsideOfCase, Array.Empty<object>());
					}
					this.\u0007(false);
					istatement.Accept(this);
					this.\u0003();
				}
				catch (Exception ex)
				{
					string text = "Internal error in _IStatement: " + istatement.ToString();
					istatement.AddError(text, MessageId.None);
					text = "Exception text: " + ex.ToString();
					istatement.AddMessage(text, istatement._Position, Severity.Error, istatement.PositionLength, MessageId.None);
				}
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002C66 RID: 11366 RVA: 0x0009BB58 File Offset: 0x00099D58
		public void \u0001(_IWhileStatement \u0002)
		{
			\u0002._Condition.Accept(this);
			\u0002._Controlled.Accept(this);
			if (!this.\u0001(\u0002._Condition, TypeTable.Bool))
			{
				return;
			}
			_IExpression condition = \u0002._Condition;
			TypeCheckerVisitor.\u0001(this, \u0002, \u0002._Condition.Type, TypeClass.Bool, this.Scope, ref condition);
			\u0002._Condition = condition;
			this.\u0001(\u0002);
		}

		// Token: 0x06002C67 RID: 11367 RVA: 0x0009BBC4 File Offset: 0x00099DC4
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
			\u0002._Condition.Accept(this);
			if (!this.\u0001(\u0002._Condition, TypeTable.Get(TypeClass.Bool)))
			{
				return;
			}
			_IExpression condition = \u0002._Condition;
			TypeCheckerVisitor.\u0001(this, \u0002, \u0002._Condition.Type, TypeClass.Bool, this.Scope, ref condition);
			\u0002._Condition = condition;
			this.\u0001(\u0002);
		}

		// Token: 0x06002C68 RID: 11368 RVA: 0x0009BC30 File Offset: 0x00099E30
		public void \u0001(_IForStatement \u0002)
		{
			\u0002._CounterStart.Accept(this);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			\u0002._UpperBound.Accept(this);
			this.\u0001(\u0002._UpperBound, \u0002._Counter.Type);
			if (\u0002._Counter != null)
			{
				\u0002._Counter.Accept(this);
			}
			if (\u0002._By != null)
			{
				\u0002._By.Accept(this);
			}
			this.\u0001(\u0002._By, \u0002._Counter.Type);
			\u0002._Controlled.Accept(this);
			bool flag = false;
			bool flag2 = true;
			checked
			{
				try
				{
					if (\u0002._By != null)
					{
						ILiteralValue literalValue = \u0002._By.Literal(this.Scope);
						if (literalValue == null)
						{
							goto IL_1BA;
						}
						bool flag3;
						int @int = literalValue.GetInt(out flag3);
						if (!flag3)
						{
							goto IL_1BA;
						}
						flag2 = (@int > 0);
					}
					ILiteralValue literalValue2 = \u0002._UpperBound.Literal(this.Scope);
					if (literalValue2 != null)
					{
						if (\u0002._Counter != null)
						{
							if (\u0002._Counter._CompiledType != null)
							{
								bool flag4 = false;
								bool flag5 = false;
								ulong unsignedLong = literalValue2.GetUnsignedLong(out flag4);
								long num = 0L;
								if (!flag4)
								{
									num = literalValue2.GetSignedLong(out flag5);
								}
								if (flag5 || flag4)
								{
									if (flag2)
									{
										ulong typeRangeHigh = TypeTable.GetTypeRangeHigh(\u0002._Counter._CompiledType.Class);
										if (flag4 && unsignedLong == typeRangeHigh)
										{
											flag = true;
										}
										if (flag5 && typeRangeHigh <= 9223372036854775807UL && num == (long)typeRangeHigh)
										{
											flag = true;
										}
									}
									else
									{
										long typeRangeLow = TypeTable.GetTypeRangeLow(\u0002._Counter._CompiledType.Class);
										if (flag5 && num == typeRangeLow)
										{
											flag = true;
										}
										if (flag4 && typeRangeLow >= 0L && unsignedLong == (ulong)typeRangeLow)
										{
											flag = true;
										}
									}
								}
							}
						}
					}
				}
				catch
				{
					flag = false;
				}
				IL_1BA:
				if (flag)
				{
					_IExpression iexpression = \u0002._CounterStart;
					if (iexpression is _IAssignmentExpression)
					{
						iexpression = (iexpression as _IAssignmentExpression)._LValue;
					}
					Operator op;
					if (flag2)
					{
						op = Operator.Greater;
					}
					else
					{
						op = Operator.Less;
					}
					this.\u0001(\u0002._UpperBound, MessageId.Wrn_LoopExitConditionConstant, new object[]
					{
						iexpression,
						Scanner.GetTextOfOperator(op),
						\u0002._UpperBound
					});
				}
				_IAssignmentExpression iassignmentExpression = \u0002.CounterStart as _IAssignmentExpression;
				if (iassignmentExpression != null)
				{
					_IExpression iexpression2 = iassignmentExpression._LValue;
					TypeCheckerVisitor.\u0001(this, iassignmentExpression._LValue, iassignmentExpression._LValue.Type, TypeClass.AnyInt, this.Scope, ref iexpression2);
					iassignmentExpression._LValue = iexpression2;
					iexpression2 = iassignmentExpression._RValue;
					TypeCheckerVisitor.\u0001(this, iassignmentExpression._RValue, iassignmentExpression._RValue.Type, iassignmentExpression._LValue.Type, this.Scope, ref iexpression2);
					iassignmentExpression._RValue = iexpression2;
					if (\u0002.UpperBound != null)
					{
						iexpression2 = \u0002._UpperBound;
						TypeCheckerVisitor.\u0001(this, \u0002._UpperBound, \u0002._UpperBound.Type, iassignmentExpression._LValue.Type, this.Scope, ref iexpression2);
						\u0002._UpperBound = iexpression2;
					}
					if (\u0002.By != null)
					{
						iexpression2 = \u0002._By;
						TypeCheckerVisitor.\u0001(this, \u0002._By, \u0002._By.Type, iassignmentExpression._LValue.Type.BaseType, this.Scope, ref iexpression2);
						\u0002._By = iexpression2;
					}
				}
				this.\u0001(\u0002);
			}
		}

		// Token: 0x06002C69 RID: 11369 RVA: 0x0009BF88 File Offset: 0x0009A188
		public void \u0001(_IExitStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002C6A RID: 11370 RVA: 0x0009BF94 File Offset: 0x0009A194
		public void \u0001(_IContinueStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002C6B RID: 11371 RVA: 0x0009BFA0 File Offset: 0x0009A1A0
		public bool \u0001(_IAssignmentExpression \u0002, bool \u0003)
		{
			if (\u0002.RValue is _IAssignmentExpression && (\u0002.RValue as _IAssignmentExpression).RValue is _INewExpression)
			{
				this.AddError(\u0002, MessageId.Err_MultipleAssignmentsNotAllowedForOperator, new object[]
				{
					Operator.__New.ToString()
				});
			}
			if (\u0002._LValue.Type is _IUserdefType && \u0002._RValue.Type is _IUserdefType && \u0002.KindOf != Operator.RefAssign)
			{
				_ISignature isignature = ((_IUserdefType)\u0002._LValue.Type).GetSignature(this.Scope) as _ISignature;
				_ISignature isignature2 = ((_IUserdefType)\u0002._RValue.Type).GetSignature(this.Scope) as _ISignature;
				if (isignature != null && isignature2 != null && (isignature.POUType == Operator.Interface || isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion)) && isignature2.POUType != Operator.Interface && !isignature2.GetFlag(SignatureFlag.ImplicitInterfaceUnion) && !\u0002._RValue.IsLValue(this.Scope, this.\u0002))
				{
					this.AddError(\u0002, MessageId.Err_RefAssignNeedsLValue, new object[]
					{
						\u0002._RValue.ToString()
					});
				}
			}
			if (\u0002._LValue.Type.DeRefType is _IUserdefType && \u0002._RValue.Type.DeRefType is _IUserdefType)
			{
				_ISignature isignature3 = ((_IUserdefType)\u0002._LValue.Type.DeRefType).GetSignature(this.Scope) as _ISignature;
				_ISignature isignature4 = ((_IUserdefType)\u0002._RValue.Type.DeRefType).GetSignature(this.Scope) as _ISignature;
				if (\u0002.KindOf != Operator.RefAssign)
				{
					if (isignature3 != null && (isignature3.POUType == Operator.Interface || isignature3.GetFlag(SignatureFlag.ImplicitInterfaceUnion)) && \u0002.RValue is _IAssignmentExpression)
					{
						this.\u0002(\u0002);
					}
					if (isignature3 != null && isignature3.POUType == Operator.FunctionBlock && \u0002.RValue is _IAssignmentExpression)
					{
						this.\u0001(\u0002, isignature3);
						this.\u0003(\u0002);
					}
				}
				if (isignature3 != null && isignature3.POUType == Operator.FunctionBlock && isignature3.GetFlag(SignatureFlag.Abstract))
				{
					this.AddError(\u0002, MessageId.Err_AbstractFunctionBlockAssigned, new object[]
					{
						isignature3.NameExpression.ToString()
					});
				}
				if (\u0003 && isignature3 != null && isignature3.POUType == Operator.FunctionBlock)
				{
					int? num = (isignature3 != null) ? new int?(isignature3.Id) : null;
					int? num2 = (isignature4 != null) ? new int?(isignature4.Id) : null;
					if (!(num.GetValueOrDefault() == num2.GetValueOrDefault() & num != null == (num2 != null)))
					{
						this.\u0001(this.Scope, \u0002._LValue, \u0002._RValue.Type.DeRefType, \u0002._LValue.Type.DeRefType);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002C6C RID: 11372 RVA: 0x0009C2B4 File Offset: 0x0009A4B4
		public void \u0001(_IAssignmentExpression \u0002, _ISignature \u0003)
		{
			_IAssignmentExpression iassignmentExpression = \u0002.RValue as _IAssignmentExpression;
			if (iassignmentExpression._LValue.Type.DeRefType is _IUserdefType && iassignmentExpression._RValue.Type.DeRefType is _IUserdefType)
			{
				_ISignature isignature = ((_IUserdefType)iassignmentExpression._LValue.Type.DeRefType).GetSignature(this.Scope) as _ISignature;
				_ISignature isignature2 = ((_IUserdefType)iassignmentExpression._RValue.Type.DeRefType).GetSignature(this.Scope) as _ISignature;
				if (\u0003 != null && isignature != null && isignature2 != null && (\u0003.Id != isignature.Id || isignature.Id != isignature2.Id))
				{
					this.AddError(iassignmentExpression, MessageId.Err_MultipleAssignmentWithChangingValueTypes, Array.Empty<object>());
				}
			}
		}

		// Token: 0x06002C6D RID: 11373 RVA: 0x0009C384 File Offset: 0x0009A584
		public void \u0001(_IAssignmentExpression \u0002)
		{
			if (\u0002._LValue.Type.DeRefType is _IUserdefType && \u0002._RValue.Type.DeRefType is _IUserdefType)
			{
				_ISignature isignature = ((_IUserdefType)\u0002._LValue.Type.DeRefType).GetSignature(this.Scope) as _ISignature;
				_ISignature isignature2 = ((_IUserdefType)\u0002._RValue.Type.DeRefType).GetSignature(this.Scope) as _ISignature;
				if (isignature != null && isignature2 != null && ((isignature.POUType != Operator.Interface && !isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion)) || (isignature2.POUType != Operator.Interface && !isignature2.GetFlag(SignatureFlag.ImplicitInterfaceUnion))))
				{
					this.AddError(\u0002, MessageId.Err_MultipleAssignmentsToInterfaceVariables, Array.Empty<object>());
					return;
				}
			}
			else
			{
				this.AddError(\u0002, MessageId.Err_MultipleAssignmentsToInterfaceVariables, Array.Empty<object>());
			}
		}

		// Token: 0x06002C6E RID: 11374 RVA: 0x0009C46C File Offset: 0x0009A66C
		public void \u0002(_IAssignmentExpression \u0002)
		{
			_IAssignmentExpression u = \u0002.RValue as _IAssignmentExpression;
			this.\u0001(u);
		}

		// Token: 0x06002C6F RID: 11375 RVA: 0x0009C48C File Offset: 0x0009A68C
		public void \u0003(_IAssignmentExpression \u0002)
		{
			_IAssignmentExpression iassignmentExpression = \u0002.RValue as _IAssignmentExpression;
			if (iassignmentExpression._LValue.Type.DeRefType is _IUserdefType && iassignmentExpression._RValue.Type.DeRefType is _IUserdefType && iassignmentExpression._RValue.Type is _IReferenceType)
			{
				this.AddError(iassignmentExpression, MessageId.Err_MultipleAssignmentWithReferences, Array.Empty<object>());
			}
		}

		// Token: 0x06002C70 RID: 11376 RVA: 0x0009C4F8 File Offset: 0x0009A6F8
		private bool \u0001(ICompiledType \u0002)
		{
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			if (iuserdefType == null)
			{
				return false;
			}
			ISignature signature = iuserdefType.GetSignature(this.Scope);
			return signature != null && signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion);
		}

		// Token: 0x06002C71 RID: 11377 RVA: 0x0009C530 File Offset: 0x0009A730
		private void \u0001(_IAssignmentExpression \u0002, ref _IExpression \u0003)
		{
			bool u = true;
			int num = 0;
			if (\u0002._LValue.Type.Class != TypeClass.Reference)
			{
				this.AddError(\u0002._LValue, MessageId.Err_LValueNoRefType, Array.Empty<object>());
				return;
			}
			if (\u0002._RValue.Type.DeRefType.Class == TypeClass.Userdef)
			{
				this.\u0005(\u0002);
				return;
			}
			if (\u0002._LValue.Type.DeRefType.Class == TypeClass.String && \u0002._LValue.Type.DeRefType.Class == TypeClass.WString)
			{
				\u0003 = \u0002._RValue;
				TypeCheckerVisitor.\u0001(this, \u0002._LValue, \u0002._RValue.Type, \u0002._LValue.Type, this.Scope, ref \u0003);
				\u0002._RValue = \u0003;
				return;
			}
			if (\u0002._RValue is ILiteralExpression && (\u0002._RValue as ILiteralExpression).LiteralValue.GetInt(out num) && num == 0)
			{
				this.\u0001(\u0002._RValue);
				return;
			}
			if (!global::\u0006.\u0011.\u0001(\u0002._LValue.Type.DeRefType, \u0002._RValue.Type.DeRefType, this.Scope, this.Scope, u))
			{
				this.AddError(\u0002, MessageId.Err_TypeMismatch, new object[]
				{
					\u0002._RValue.Type.ToString(),
					\u0002._LValue.Type.ToString()
				});
				return;
			}
			if (!\u0002._RValue.IsLValue(this.Scope, this.\u0002))
			{
				this.\u0004(\u0002);
				return;
			}
			if (\u0002._RValue.Type != null && (\u0002._RValue.Type.Class == TypeClass.Bit || \u0002._RValue.Type is _IDirectAddressBitType))
			{
				this.AddError(\u0002, MessageId.Err_NoReferenceToBits, Array.Empty<object>());
				return;
			}
			if (!\u0002._RValue.IsVarInOutInput(this.Scope, this.\u0002, false) && \u0002._RValue.Type != null && \u0002._RValue.Type.Class != TypeClass.Reference)
			{
				this.AddError(\u0002, MessageId.Err_LValueForReference, Array.Empty<object>());
			}
		}

		// Token: 0x06002C72 RID: 11378 RVA: 0x0009C74C File Offset: 0x0009A94C
		private void \u0004(_IAssignmentExpression \u0002)
		{
			_IVariable ivariable = \u0002._RValue.GetVariable(this.Scope) as _IVariable;
			bool flag = ivariable != null;
			bool flag2 = flag && ivariable.Address != null && ivariable.Address.Location == DirectVariableLocation.Input;
			bool flag3 = flag && ivariable.IsProperty;
			bool flag4 = \u0002._RValue is _ICallExpression;
			bool flag5 = \u0002._RValue._CompiledType is _IReferenceType && (flag3 || flag4);
			if (flag2)
			{
				this.AddError(\u0002, MessageId.Err_ReferenceToInput, Array.Empty<object>());
				return;
			}
			if (!flag5)
			{
				this.AddError(\u0002, MessageId.Err_LValueForReference, Array.Empty<object>());
			}
		}

		// Token: 0x06002C73 RID: 11379 RVA: 0x0009C7F4 File Offset: 0x0009A9F4
		private void \u0005(_IAssignmentExpression \u0002)
		{
			bool flag = !\u0002._RValue.IsLValue(this.Scope, this.\u0002);
			_ICallExpression icallExpression = \u0002._RValue as _ICallExpression;
			bool flag2 = icallExpression != null && icallExpression._CompiledType is _IReferenceType;
			_IVariable ivariable = \u0002._RValue.GetVariable(this.Scope) as _IVariable;
			bool flag3 = ivariable != null && ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY) && ivariable.CompiledType is _IReferenceType;
			if (flag && !flag2 && !flag3)
			{
				this.AddError(\u0002, MessageId.Err_RefAssignNeedsLValue, new object[]
				{
					\u0002._RValue.ToString()
				});
			}
			if (this.\u0001(\u0002._LValue.Type.DeRefType))
			{
				if (!global::\u0006.\u0011.\u0001(\u0002._RValue.Type.DeRefType, \u0002._LValue.Type.DeRefType, this.Scope))
				{
					this.AddError(\u0002, MessageId.Err_TypeMismatch, new object[]
					{
						\u0002._LValue.Type.ToString(),
						\u0002._RValue.Type.ToString()
					});
					return;
				}
			}
			else
			{
				if (!global::\u0006.\u0011.\u0002(\u0002._RValue.Type, \u0002._LValue.Type, this.Scope))
				{
					this.AddError(\u0002, MessageId.Err_TypeMismatch, new object[]
					{
						\u0002._RValue.Type.ToString(),
						\u0002._LValue.Type.ToString()
					});
					return;
				}
				if (!\u0002._RValue.IsVarInOutInput(this.Scope, this.\u0002, false) && \u0002._RValue.Type != null && \u0002._RValue.Type.Class != TypeClass.Reference)
				{
					this.AddError(\u0002, MessageId.Err_LValueForReference, Array.Empty<object>());
				}
			}
		}

		// Token: 0x06002C74 RID: 11380 RVA: 0x0009C9C8 File Offset: 0x0009ABC8
		private void \u0002(_IAssignmentExpression \u0002, ref _IExpression \u0003)
		{
			if (!(\u0002._RValue is IAssignmentExpression))
			{
				\u0003 = \u0002._RValue;
				\u0002.Type = \u0002._RValue.Type;
				TypeCheckerVisitor.\u0001(this, \u0002._LValue, \u0002._RValue.Type, \u0002._LValue.Type, this.Scope, ref \u0003);
				\u0002._RValue = \u0003;
				return;
			}
			IAssignmentExpression assignmentExpression = \u0002._RValue as IAssignmentExpression;
			\u0003 = \u0002._RValue;
			if (TypeCheckerVisitor.\u0001(this, \u0002._LValue, \u0002._RValue.Type, \u0002._LValue.Type, this.Scope, ref \u0003))
			{
				\u0003 = \u0002._RValue;
				bool u = this.ConvertAllTypeMismatches;
				this.ConvertAllTypeMismatches = true;
				TypeCheckerVisitor.\u0001(this, \u0002._LValue, assignmentExpression.LValue.Type, \u0002._LValue.Type, this.Scope, ref \u0003);
				this.ConvertAllTypeMismatches = u;
				\u0002.Type = \u0002._RValue.Type;
				\u0002._RValue = \u0003;
				return;
			}
			\u0002.Type = \u0002._RValue.Type;
			\u0002._RValue = \u0003;
		}

		// Token: 0x06002C75 RID: 11381 RVA: 0x0009CAEC File Offset: 0x0009ACEC
		private void \u0006(_IAssignmentExpression \u0002)
		{
			if (\u0002._RValue is _INewExpression && \u0002._RValue.Type.Class == TypeClass.Pointer && \u0002._RValue.Type.BaseType.Class == TypeClass.Userdef)
			{
				if (\u0002._LValue.Type.Class == TypeClass.Pointer)
				{
					if (!global::\u0006.\u0011.\u0001(\u0002._LValue.Type.BaseType, \u0002._RValue.Type.BaseType, this.Scope, this.Scope, false))
					{
						this.AddError(\u0002, MessageId.Err_TypeMismatch, new object[]
						{
							\u0002._RValue.Type.ToString(),
							\u0002._LValue.Type.ToString()
						});
						return;
					}
				}
				else
				{
					this.AddError(\u0002, MessageId.Err_TypeMismatch, new object[]
					{
						\u0002._RValue.Type.ToString(),
						\u0002._LValue.Type.ToString()
					});
				}
			}
		}

		// Token: 0x06002C76 RID: 11382 RVA: 0x0009CBF0 File Offset: 0x0009ADF0
		private void \u0002(_IAssignmentExpression \u0002, _ISignature \u0003)
		{
			if (\u0003 != null && \u0002._RValue is _IOperatorExpression && ((\u0002._RValue as _IOperatorExpression).Code == Operator.Sel || (\u0002._RValue as _IOperatorExpression).Code == Operator.Mux))
			{
				_IOperatorExpression ioperatorExpression = \u0002._RValue as _IOperatorExpression;
				if (ioperatorExpression[1].Type.DeRefType is _IArrayType && ioperatorExpression[2].Type.DeRefType is _IArrayType)
				{
					return;
				}
				_IUserdefType iuserdefType = ioperatorExpression[1].Type.DeRefType as _IUserdefType;
				_IUserdefType iuserdefType2 = ioperatorExpression[2].Type.DeRefType as _IUserdefType;
				if (iuserdefType == null || iuserdefType2 == null || iuserdefType.SignatureId != iuserdefType2.SignatureId || iuserdefType.SignatureId != \u0003.Id)
				{
					this.AddError(\u0002, MessageId.Err_SelMuxOnlyEqualUserDefTypes, Array.Empty<object>());
				}
			}
		}

		// Token: 0x06002C77 RID: 11383 RVA: 0x0009CCE4 File Offset: 0x0009AEE4
		public void \u0007(_IAssignmentExpression \u0002)
		{
			this.\u0007(this.TopOfStack.\u0006);
			this.TopOfStack.\u0001 = \u0002.KindOf;
			this.TopOfStack.\u0001 |= TypeCheckerVisitor.\u0001.\u0002;
			\u0002._LValue.Accept(this);
			this.TopOfStack.\u0001 ^= TypeCheckerVisitor.\u0001.\u0002;
			this.\u0003();
			_IVariable ivariable = \u0002._LValue.GetVariable(this.Scope) as _IVariable;
			bool flag = ivariable != null && ivariable.IsProperty;
			if (!this.TopOfStack.\u0002 && \u0002._RValue is _INewExpression)
			{
				(\u0002._RValue as _INewExpression).PositionOK = true;
			}
			if (!this.TopOfStack.\u0002 && \u0002._RValue is _IOperatorExpression && ((\u0002._RValue as _IOperatorExpression).Code == Operator.__CheckLicense || (\u0002._RValue as _IOperatorExpression).Code == Operator.__CheckLicenseBit))
			{
				(\u0002._RValue as _IOperatorExpression).PositionOK = true;
			}
			if (flag && \u0002._RValue is _IOperatorExpression && (\u0002._RValue as _IOperatorExpression).Code == Operator.__VarInfo)
			{
				this.AddError(\u0002._LValue, MessageId.Err_IsNoLValue, new object[]
				{
					\u0002._LValue
				});
			}
			this.\u0002();
			this.TopOfStack.\u0001 |= TypeCheckerVisitor.\u0001.\u0002;
			\u0002._RValue.Accept(this);
			this.TopOfStack.\u0001 ^= TypeCheckerVisitor.\u0001.\u0002;
			this.\u0003();
			if (!\u0002._LValue.IsLValue(this.Scope, this.\u0002, false, \u0002.KindOf == Operator.RefAssign) || \u0002._LValue.Type == null)
			{
				this.AddError(\u0002._LValue, MessageId.Err_IsNoLValue, new object[]
				{
					\u0002._LValue
				});
			}
			else if (\u0002._RValue.Type == null)
			{
				bool flag2 = false;
				if (\u0002.RValue is _IAssignmentExpression)
				{
					_IAssignmentExpression iassignmentExpression = (_IAssignmentExpression)\u0002.RValue;
					_IVariable ivariable2 = iassignmentExpression._LValue.GetVariable(this.Scope) as _IVariable;
					if (ivariable2 != null && ivariable2.IsProperty)
					{
						this.AddError(iassignmentExpression._LValue, MessageId.Err_MultipleAssignmentWithProperty, Array.Empty<object>());
						flag2 = true;
					}
				}
				if (!flag2)
				{
					this.AddError(\u0002, MessageId.Err_TypeMismatch, new object[]
					{
						global::\u0003.\u0006.\u0001(MessageId.Err_UnknownType, new object[]
						{
							\u0002._RValue.ToString()
						}),
						\u0002._LValue.Type,
						MessageId.Err_UnknownType
					});
				}
			}
			else
			{
				_IExpression rvalue = \u0002._RValue;
				Operator kindOf = \u0002.KindOf;
				if (kindOf - Operator.SetAssign > 1)
				{
					if (kindOf != Operator.RefAssign)
					{
						if (kindOf != Operator.FupAssign)
						{
							if (!this.\u0001(\u0002, flag))
							{
								this.\u0001(\u0002._LValue, this.Scope);
								rvalue = \u0002._RValue;
								TypeCheckerVisitor.\u0001(this, \u0002._LValue, \u0002._RValue.Type, \u0002._LValue.Type, this.Scope, ref rvalue);
								\u0002._RValue = rvalue;
							}
						}
						else
						{
							this.\u0002(\u0002, ref rvalue);
						}
					}
					else
					{
						this.\u0001(\u0002, ref rvalue);
					}
				}
				else
				{
					rvalue = \u0002._RValue;
					TypeCheckerVisitor.\u0001(this, \u0002._RValue, \u0002._RValue.Type, TypeTable.Bool, this.Scope, ref rvalue);
					rvalue = \u0002._LValue;
					TypeCheckerVisitor.\u0001(this, \u0002._LValue, \u0002._LValue.Type, TypeTable.Bool, this.Scope, ref rvalue);
				}
			}
			if (\u0002.KindOf != Operator.FupAssign)
			{
				\u0002.Type = \u0002._LValue.Type;
			}
			this.\u0006(\u0002);
			this.\u0008(\u0002);
			this.\u0001(\u0002);
		}

		// Token: 0x06002C78 RID: 11384 RVA: 0x0009D0B4 File Offset: 0x0009B2B4
		private void \u0008(_IAssignmentExpression \u0002)
		{
			_IUserdefType iuserdefType = null;
			_IUserdefType iuserdefType2 = null;
			if (this.\u0001(\u0002, out iuserdefType, out iuserdefType2))
			{
				_ISignature isignature = iuserdefType.GetSignature(this.Scope) as _ISignature;
				_ISignature isignature2 = iuserdefType2.GetSignature(this.Scope) as _ISignature;
				if (isignature != null && isignature2 != null && isignature.POUType == Operator.FunctionBlock && isignature2.POUType == Operator.FunctionBlock && \u0002._LValue is IDeRefAccessExpression)
				{
					this.\u0001(\u0002, MessageId.Wrn_ValueAssignViaPointerMayChangeVFTable, new object[]
					{
						\u0002._LValue,
						isignature.OrgName
					});
				}
				this.\u0001(isignature, \u0002);
				this.\u0002(\u0002, isignature);
			}
		}

		// Token: 0x06002C79 RID: 11385 RVA: 0x0009D154 File Offset: 0x0009B354
		private void \u0001(_ISignature \u0002, _IAssignmentExpression \u0003)
		{
			if (\u0002 != null && (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN) || \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN_WARNING)) && !(\u0003._RValue is IStructureInitialization) && !(\u0003._RValue is IArrayInitialization))
			{
				if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN))
				{
					this.AddError(\u0003._LValue, MessageId.Err_NoAssign, new object[]
					{
						\u0002.OrgName
					});
					return;
				}
				if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN_WARNING))
				{
					this.\u0001(\u0003._LValue, MessageId.Err_NoAssign, new object[]
					{
						\u0002.OrgName
					});
				}
			}
		}

		// Token: 0x06002C7A RID: 11386 RVA: 0x0009D1F8 File Offset: 0x0009B3F8
		private bool \u0001(_IAssignmentExpression \u0002, out _IUserdefType \u0003, out _IUserdefType \u0004)
		{
			\u0003 = null;
			\u0004 = null;
			if (\u0002.KindOf == Operator.RefAssign)
			{
				return false;
			}
			ICompiledType compiledType = \u0002._RValue.Type;
			ICompiledType compiledType2 = \u0002._LValue.Type;
			if (compiledType == null || compiledType2 == null)
			{
				return false;
			}
			if (compiledType.Class == TypeClass.Reference)
			{
				compiledType = compiledType.DeRefType;
			}
			if (compiledType2.Class == TypeClass.Reference)
			{
				compiledType2 = compiledType2.DeRefType;
			}
			if (compiledType.Class == TypeClass.Array && compiledType2.Class == TypeClass.Array)
			{
				compiledType = global::\u0084.\u0004.\u0001(compiledType);
				compiledType2 = global::\u0084.\u0004.\u0001(compiledType2);
			}
			if (compiledType == null || compiledType2 == null)
			{
				return false;
			}
			if (compiledType.Class == TypeClass.Userdef && compiledType2.Class == TypeClass.Userdef)
			{
				\u0003 = (compiledType2 as _IUserdefType);
				\u0004 = (compiledType as _IUserdefType);
				return true;
			}
			return false;
		}

		// Token: 0x06002C7B RID: 11387 RVA: 0x0009D2B0 File Offset: 0x0009B4B0
		public void \u0001(_IIfStatement \u0002)
		{
			\u0002._Condition.Accept(this);
			\u0002._IfThen.Accept(this);
			IList<_IElseIf> elseIf = \u0002._ElseIf;
			if (elseIf != null)
			{
				foreach (_IElseIf ielseIf in elseIf)
				{
					ielseIf._Condition.Accept(this);
					ielseIf._Controlled.Accept(this);
				}
			}
			if (\u0002.IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
			if (\u0002._Condition.Type == null)
			{
				this.AddError(\u0002._Condition, MessageId.Err_SpecialTypeExpected, new object[]
				{
					TypeTable.Bool
				});
				return;
			}
			_IExpression condition = \u0002._Condition;
			TypeCheckerVisitor.\u0001(this, \u0002._Condition, \u0002._Condition.Type, TypeClass.Bool, this.Scope, ref condition);
			\u0002._Condition = condition;
			foreach (_IElseIf ielseIf2 in \u0002._ElseIf)
			{
				if (ielseIf2._Condition.Type == null)
				{
					this.AddError(ielseIf2._Condition, MessageId.Err_SpecialTypeExpected, new object[]
					{
						TypeTable.Bool
					});
				}
				else
				{
					condition = ielseIf2._Condition;
					TypeCheckerVisitor.\u0001(this, ielseIf2._Condition, ielseIf2._Condition.Type, TypeClass.Bool, this.Scope, ref condition);
					ielseIf2._Condition = condition;
				}
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002C7C RID: 11388 RVA: 0x0009D434 File Offset: 0x0009B634
		public void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
				if (\u0002._Condition.Type == null)
				{
					this.AddError(\u0002._Condition, MessageId.Err_SpecialTypeExpected, new object[]
					{
						TypeTable.Bool
					});
					return;
				}
				_IExpression condition = \u0002._Condition;
				TypeCheckerVisitor.\u0001(this, \u0002._Condition, \u0002._Condition.Type, TypeClass.Bool, this.Scope, ref condition);
				\u0002._Condition = condition;
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002C7D RID: 11389 RVA: 0x0009D4B8 File Offset: 0x0009B6B8
		public void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
				if (\u0002._Condition.Type == null)
				{
					this.AddError(\u0002._Condition, MessageId.Err_SpecialTypeExpected, new object[]
					{
						TypeTable.Bool
					});
					return;
				}
				_IExpression condition = \u0002._Condition;
				TypeCheckerVisitor.\u0001(this, \u0002._Condition, \u0002._Condition.Type, TypeClass.Bool, this.Scope, ref condition);
				\u0002._Condition = condition;
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002C7E RID: 11390 RVA: 0x0009D53C File Offset: 0x0009B73C
		public void \u0001(_ILabelStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002C7F RID: 11391 RVA: 0x0009D548 File Offset: 0x0009B748
		public void \u0001(_ICommentStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002C80 RID: 11392 RVA: 0x0009D554 File Offset: 0x0009B754
		public void \u0001(_IPragmaStatement \u0002)
		{
			_IImplicitCodeSectionPragma iimplicitCodeSectionPragma = \u0002 as _IImplicitCodeSectionPragma;
			if (iimplicitCodeSectionPragma != null)
			{
				this.InImplicitCode = iimplicitCodeSectionPragma.ImplicitOn;
				this.\u0002 = this.InImplicitCode;
			}
			else
			{
				string text = \u0002.Text;
				if (text.Contains("implicit"))
				{
					this.InImplicitCode = !text.Contains("implicit off");
					this.\u0002 = this.InImplicitCode;
				}
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002C81 RID: 11393 RVA: 0x0009D5C0 File Offset: 0x0009B7C0
		public void \u0001(_IExpressionStatement \u0002)
		{
			\u0002._Expr.Accept(this);
			this.\u0001(\u0002);
			if (typeof(_IVariableExpression).IsAssignableFrom(\u0002._Expr.GetType()) || typeof(_ICompoAccessExpression).IsAssignableFrom(\u0002._Expr.GetType()) || typeof(_IDeRefAccessExpression).IsAssignableFrom(\u0002._Expr.GetType()) || typeof(_IIndexAccessExpression).IsAssignableFrom(\u0002._Expr.GetType()))
			{
				ICompiledType type = \u0002._Expr.Type;
				if (type != null && type.Class == TypeClass.Userdef)
				{
					ISignature signature = (type as _IUserdefType).GetSignature(this.Scope);
					if (signature != null && (signature.POUType == Operator.Function || signature.POUType == Operator.Method || signature.POUType == Operator.Action || signature.POUType == Operator.Program))
					{
						if (signature.GetFlag(SignatureFlag.Action))
						{
							this.AddError(\u0002, MessageId.Err_MissingParameterList, new object[]
							{
								Scanner.GetTextOfOperator(Operator.Action),
								\u0002._Expr
							});
							return;
						}
						this.AddError(\u0002, MessageId.Err_MissingParameterList, new object[]
						{
							Scanner.GetTextOfOperator(signature.POUType),
							\u0002._Expr
						});
					}
				}
			}
		}

		// Token: 0x06002C82 RID: 11394 RVA: 0x0009D710 File Offset: 0x0009B910
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002C83 RID: 11395 RVA: 0x0009D714 File Offset: 0x0009B914
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06002C84 RID: 11396 RVA: 0x0009D718 File Offset: 0x0009B918
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002C85 RID: 11397 RVA: 0x0009D71C File Offset: 0x0009B91C
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002C86 RID: 11398 RVA: 0x0009D720 File Offset: 0x0009B920
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002C87 RID: 11399 RVA: 0x0009D724 File Offset: 0x0009B924
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06002C88 RID: 11400 RVA: 0x0009D728 File Offset: 0x0009B928
		public void \u0001(TypeCheckerVisitor \u0002, _IExprement \u0003, IVariable \u0004, ICompiledType \u0005, ICompiledType \u0006, IScope5 \u0007, ref _IExpression \u0008, out bool \u000E)
		{
			\u000E = false;
			if (\u0005.DeRefType.Class == TypeClass.Userdef && \u0006.DeRefType.Class == TypeClass.Userdef)
			{
				ISignature signature = (\u0006.DeRefType as _IUserdefType).GetSignature(this.Scope);
				if (!global::\u0006.\u0011.\u0001((\u0005.DeRefType as _IUserdefType).GetSignature(this.Scope), signature, this.Scope, this.Scope))
				{
					this.AddError(\u0003, MessageId.Err_OutParamNotEqual, new object[]
					{
						\u0005,
						\u0006,
						\u0004.OrgName
					});
					return;
				}
			}
			TypeCheckerVisitor.\u0001(\u0002, \u0003, \u0005, \u0006, \u0007, ref \u0008, out \u000E);
		}

		// Token: 0x06002C89 RID: 11401 RVA: 0x0009D7D4 File Offset: 0x0009B9D4
		private bool \u0001(ref _IExpression \u0002, ISignature \u0003, IVariable \u0004, _IExpression \u0005)
		{
			bool flag = (\u0004.CompiledType != null && \u0004.CompiledType.Class == TypeClass.Reference) || \u0004.GetFlag(VarFlag.Inout);
			bool result = false;
			if (flag)
			{
				result = true;
				if (\u0004.CompiledType.DeRefType.Class == TypeClass.Userdef && \u0002.Type.DeRefType.Class == TypeClass.Userdef)
				{
					_IUserdefType iuserdefType = \u0004.CompiledType.DeRefType as _IUserdefType;
					ISignature u = (iuserdefType != null) ? iuserdefType.GetSignature(this.Scope) : null;
					_IUserdefType iuserdefType2 = \u0002.Type.DeRefType as _IUserdefType;
					if (!global::\u0006.\u0011.\u0001((iuserdefType2 != null) ? iuserdefType2.GetSignature(this.Scope) : null, u, this.Scope, this.Scope))
					{
						this.AddError(\u0002, MessageId.Err_InOutParamNotEqual, new object[]
						{
							\u0002.Type.DeRefType,
							\u0004.CompiledType.DeRefType,
							\u0004.OrgName
						});
					}
				}
				else if (\u0004.CompiledType.DeRefType.Class != TypeClass.String && \u0004.CompiledType.DeRefType.Class != TypeClass.WString)
				{
					int num;
					if (\u0002 is ILiteralExpression && ((ILiteralExpression)\u0002).LiteralValue.GetInt(out num))
					{
						if (num == 0)
						{
							this.\u0001(\u0002);
						}
						else
						{
							this.AddError(\u0002, MessageId.Err_LValueForReference, Array.Empty<object>());
						}
					}
					else
					{
						if (\u0004.CompiledType.DeRefType.Class == TypeClass.Bool && \u0002._CompiledType is _IDirectAddressBitType)
						{
							this.AddError(\u0002, MessageId.Err_InOutParamNotEqual, new object[]
							{
								Scanner.GetTextOfOperator(Operator.Bit),
								\u0004.CompiledType.DeRefType,
								\u0004.OrgName
							});
						}
						if (!global::\u0006.\u0011.\u0001(\u0002.Type.DeRefType, \u0004.CompiledType.DeRefType, this.Scope, this.Scope, true))
						{
							if (!TypeTable.IsBlock(\u0002.Type.DeRefType.Class) && TypeTable.IsEquivalent(\u0002.Type.DeRefType.Class, \u0004.CompiledType.DeRefType.Class))
							{
								return true;
							}
							if (\u0002.Type.DeRefType.Class == TypeClass.Pointer && TypeTable.IsLikePointer(\u0004.CompiledType.DeRefType, this.\u0001.PointerSize))
							{
								return true;
							}
							if (\u0002.Type.DeRefType.Class == TypeClass.Pointer && this.TreatReferenceAsPointer)
							{
								return true;
							}
							this.AddError(\u0002, MessageId.Err_InOutParamNotEqual, new object[]
							{
								\u0002.Type.DeRefType,
								\u0004.CompiledType.DeRefType,
								\u0004.OrgName
							});
						}
						if (this.\u0003(\u0002))
						{
							this.AddError(\u0002, MessageId.Err_LValueForReference, Array.Empty<object>());
						}
						result = this.TypeCheckerInternal.\u0001(\u0002.Type, \u0004.CompiledType, this.Scope, \u0002);
					}
				}
				else
				{
					result = false;
				}
				if (\u0004.HasAttribute(CompileAttributes.ATTRIBUTE_NOCONST) && \u0003 != null && !\u0002.IsLValue(this.Scope, this.\u0002))
				{
					this.AddError(\u0002, MessageId.Err_LValueForVarinout, new object[]
					{
						\u0004.OrgName,
						\u0003.OrgName
					});
				}
			}
			return result;
		}

		// Token: 0x06002C8A RID: 11402 RVA: 0x0009DB24 File Offset: 0x0009BD24
		private void \u0001(ref bool \u0002, _IExpression \u0003, ISignature \u0004, IVariable \u0005)
		{
			ICompiledType compiledType = \u0005.CompiledType;
			if (compiledType is _IUserdefType && \u0005.HasAttribute(CompileAttributes.ATTRIBUTE_ANYTYPECLASS))
			{
				compiledType = new global::\u0011.\u0006(\u0005.GetAttributeValue(CompileAttributes.ATTRIBUTE_ANYTYPECLASS)).\u0001();
				bool flag = TypeTable.IsFunctionalAnyType(\u0003._CompiledType);
				bool flag2 = false;
				if (!\u0003.IsLValue(this.Scope, false))
				{
					flag2 = !flag;
				}
				else
				{
					_IVariable ivariable = \u0003.GetVariable(this.Scope) as _IVariable;
					if (ivariable != null && ivariable.IsProperty)
					{
						flag2 = !flag;
					}
				}
				if (flag2)
				{
					this.AddError(\u0003, MessageId.Err_LValueForAnyVar, new object[]
					{
						\u0005.OrgName,
						\u0004.OrgName
					});
				}
				if (TypeTable.IsAnyType(compiledType.Class))
				{
					if (!TypeCheckerVisitor.\u0001(\u0003, \u0003.Type, compiledType))
					{
						this.AddError(\u0003, MessageId.Err_TypeMismatch, new object[]
						{
							\u0003.Type,
							compiledType
						});
					}
					\u0002 = false;
				}
			}
		}

		// Token: 0x06002C8B RID: 11403 RVA: 0x0009DC18 File Offset: 0x0009BE18
		private IVariable \u0001(IVariable \u0002, ISignature \u0003)
		{
			string stName = \u0002.Name + "__Array__Info";
			IVariable variable = \u0003[stName];
			if (variable == null)
			{
				ISignature signature;
				for (int baseSignatureId = \u0003.BaseSignatureId; baseSignatureId != Helper.InvalidId; baseSignatureId = signature.BaseSignatureId)
				{
					signature = this.Scope[baseSignatureId];
					variable = signature[stName];
					if (variable != null)
					{
						break;
					}
				}
			}
			return variable;
		}

		// Token: 0x06002C8C RID: 11404 RVA: 0x0009DC74 File Offset: 0x0009BE74
		private void \u0001(ref ICompiledType \u0002, _IExpression \u0003, ISignature \u0004, IVariable \u0005)
		{
			if (!\u0005.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
			{
				return;
			}
			\u0002 = global::\u0019.\u0003.\u0001(\u0003.Type as _IType);
			IVariable variable = this.\u0001(\u0005, \u0004);
			if (variable != null)
			{
				_IArrayType iarrayType = variable.Type as _IArrayType;
				if (iarrayType != null)
				{
					bool flag;
					int num = iarrayType._Dimensions[0].UpperBorderInt(out flag, this.Scope);
					_IVariableLengthArrayType ivariableLengthArrayType = global::\u0019.\u0003.\u0001();
					ivariableLengthArrayType._Base = ((_IPointerType)\u0005.Type)._Base;
					ivariableLengthArrayType.Dimensions = num;
					IVariable variable2 = \u0003.GetVariable(this.Scope);
					if (variable2 != null && variable2.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
					{
						this.\u0001(\u0003, num, ivariableLengthArrayType, variable2);
						return;
					}
					if (!(\u0003.Type.DeRefType is _IArrayType))
					{
						this.\u0001(this.Scope, \u0003, \u0003.Type, ivariableLengthArrayType);
						return;
					}
					if ((\u0003.Type.DeRefType as _IArrayType)._Dimensions.Count != num)
					{
						this.\u0001(this.Scope, \u0003, \u0003.Type, ivariableLengthArrayType);
						return;
					}
					if (!global::\u0006.\u0011.\u0001(\u0003.Type.DeRefType.BaseType, ivariableLengthArrayType._Base, this.Scope, this.Scope, true))
					{
						this.\u0001(this.Scope, \u0003, \u0003.Type, ivariableLengthArrayType);
					}
					return;
				}
			}
		}

		// Token: 0x06002C8D RID: 11405 RVA: 0x0009DDC0 File Offset: 0x0009BFC0
		private void \u0001(_IExpression \u0002, int \u0003, _IVariableLengthArrayType \u0004, IVariable \u0005)
		{
			IVariableExpression variableExpression = \u0002 as IVariableExpression;
			if (variableExpression == null)
			{
				return;
			}
			if (Helper.InvalidId == variableExpression.SignatureId)
			{
				return;
			}
			IVariable variable = this.\u0001[variableExpression.SignatureId][\u0005.Name + "__Array__Info"];
			_IArrayType iarrayType = ((variable != null) ? variable.Type : null) as _IArrayType;
			if (iarrayType == null)
			{
				return;
			}
			bool flag;
			if (iarrayType._Dimensions[0].UpperBorderInt(out flag, this.Scope) != \u0003)
			{
				this.\u0001(this.Scope, \u0002, \u0002.Type, \u0004);
			}
			_IType @base = ((_IPointerType)\u0005.Type)._Base;
			_IType base2 = \u0004._Base;
			_IPointerType ipointerType = @base as _IPointerType;
			if (ipointerType != null)
			{
				_IPointerType ipointerType2 = base2 as _IPointerType;
				if (ipointerType2 != null)
				{
					@base = ipointerType._Base;
					base2 = ipointerType2._Base;
				}
			}
			if (!global::\u0006.\u0011.\u0001(@base, base2, this.Scope))
			{
				ICompiledType u = global::\u0011.\u0006.\u0001(\u0005.GetAttributeValue(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY));
				this.\u0001(this.Scope, \u0002, u, \u0004);
			}
		}

		// Token: 0x06002C8E RID: 11406 RVA: 0x0009DEC8 File Offset: 0x0009C0C8
		private void \u0001(ref _IExpression \u0002, ISignature \u0003, IVariable \u0004)
		{
			bool flag = true;
			if (\u0002.Type != null && \u0004.CompiledType != null && \u0002.Type.Class == TypeClass.Pointer && \u0004.CompiledType.Class == TypeClass.Reference && this.TreatReferenceAsPointer)
			{
				flag = false;
			}
			this.\u0001(ref flag, \u0002, \u0003, \u0004);
			ICompiledType compiledType = \u0004.CompiledType;
			ICompiledType type = \u0002.Type;
			this.\u0001(ref type, \u0002, \u0003, \u0004);
			this.\u0001(\u0002, \u0004);
			int num = 0;
			if (\u0004.CompiledType.Class == TypeClass.Reference && !\u0004.GetFlag(VarFlag.Inout) && \u0002 is ILiteralExpression && (\u0002 as ILiteralExpression).LiteralValue.GetInt(out num) && num == 0)
			{
				flag = false;
			}
			this.\u0001(\u0002, compiledType);
			if (flag && !TypeCheckerVisitor.\u0001(this, \u0002, type, compiledType, this.Scope, ref \u0002) && \u0002.Type == null)
			{
				this.AddError(\u0002, MessageId.Err_TypeMismatch, new object[]
				{
					global::\u0003.\u0006.\u0001(MessageId.Err_UnknownType, new object[]
					{
						\u0002
					}),
					\u0004.CompiledType
				});
			}
		}

		// Token: 0x06002C8F RID: 11407 RVA: 0x0009DFD8 File Offset: 0x0009C1D8
		private void \u0001(_IExpression \u0002, ICompiledType \u0003)
		{
			if (\u0002 is _IAssignmentExpression && \u0003 is _IUserdefType)
			{
				_ISignature isignature = ((_IUserdefType)\u0003).GetSignature(this.Scope) as _ISignature;
				if (isignature != null && (isignature.POUType == Operator.Interface || isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion)))
				{
					this.\u0001(\u0002 as _IAssignmentExpression);
				}
			}
		}

		// Token: 0x06002C90 RID: 11408 RVA: 0x0009E034 File Offset: 0x0009C234
		private void \u0001(_IExpression \u0002, IVariable \u0003)
		{
			if (\u0003.CompiledType.Class != TypeClass.Reference && !\u0003.GetFlag(VarFlag.Inout) && !global::\u0006.\u0011.\u0001(\u0003.CompiledType, this.Scope) && !(\u0002 is IStructureInitialization) && \u0002.Type.DeRefType.Class == TypeClass.Userdef && \u0003.CompiledType.DeRefType.Class == TypeClass.Userdef)
			{
				ISignature signature = (\u0002.Type.DeRefType as _IUserdefType).GetSignature(this.Scope);
				if (signature != null)
				{
					if (signature.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN))
					{
						this.AddError(\u0002, MessageId.Err_NoAssign, new object[]
						{
							signature.OrgName
						});
						return;
					}
					if (signature.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN_WARNING))
					{
						this.\u0001(\u0002, MessageId.Err_NoAssign, new object[]
						{
							signature.OrgName
						});
					}
				}
			}
		}

		// Token: 0x06002C91 RID: 11409 RVA: 0x0009E11C File Offset: 0x0009C31C
		public void \u0001(_IExpression \u0002, ref _IExpression \u0003, ISignature \u0004, IVariable \u0005, _IExpression \u0006)
		{
			if (\u0005 == null || !\u0005.HasFlag(VarFlag.Input | VarFlag.Inout))
			{
				this.AddError(\u0002, MessageId.Err_IsNoInput, new object[]
				{
					\u0006,
					\u0004.Name
				});
				return;
			}
			if (\u0003.Type == null)
			{
				this.AddError(\u0003, MessageId.Err_TypeMismatch, new object[]
				{
					global::\u0003.\u0006.\u0001(MessageId.Err_UnknownType, new object[]
					{
						\u0003.ToString()
					}),
					\u0005.Type,
					MessageId.Err_UnknownType
				});
				return;
			}
			if (\u0003 is _IOperatorExpression && (\u0003 as _IOperatorExpression).Code == Operator.__VarInfo)
			{
				this.AddError(\u0003, MessageId.Err_OperatorNotAllowedAtPosition, new object[]
				{
					"__VARINFO"
				});
				return;
			}
			if (!this.\u0001(ref \u0003, \u0004, \u0005, \u0006))
			{
				this.\u0001(ref \u0003, \u0004, \u0005);
			}
		}

		// Token: 0x06002C92 RID: 11410 RVA: 0x0009E1F0 File Offset: 0x0009C3F0
		public void \u0001(_ICallExpression \u0002, ISignature \u0003, int \u0004, IVariable \u0005, _IExpression \u0006)
		{
			_IExpression value = \u0002.ParamExpressions[\u0004];
			this.\u0001(\u0002, ref value, \u0003, \u0005, \u0006);
			\u0002[\u0004] = value;
		}

		// Token: 0x06002C93 RID: 11411 RVA: 0x0009E220 File Offset: 0x0009C420
		private void \u0001(ICallExpression \u0002, IList<_IVariable> \u0003)
		{
			LDictionary<string, int> ldictionary = new LDictionary<string, int>();
			HashSet<string> hashSet = new HashSet<string>();
			for (int i = 0; i < \u0003.Count; i++)
			{
				_IVariable ivariable = \u0003[i];
				if (ivariable != null)
				{
					string name = ivariable.Name;
					if (ldictionary.ContainsKey(name))
					{
						if (!hashSet.Contains(name))
						{
							this.AddError(\u0002 as _IExprement, MessageId.Err_MultipleAssignsToSameInputInCall, new object[]
							{
								ivariable.OrgName
							});
							hashSet.Add(name);
						}
					}
					else
					{
						ldictionary.Add(ivariable.Name, i);
					}
				}
			}
		}

		// Token: 0x06002C94 RID: 11412 RVA: 0x0009E2AC File Offset: 0x0009C4AC
		private void \u0001(_ICallExpression \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
				_IExpression condition = \u0002._Condition;
				TypeCheckerVisitor.\u0001(this, \u0002._Condition, \u0002._Condition.Type, TypeClass.Bool, this.Scope, ref condition);
				\u0002._Condition = condition;
			}
		}

		// Token: 0x06002C95 RID: 11413 RVA: 0x0009E2FC File Offset: 0x0009C4FC
		private void \u0001(_ICallExpression \u0002, ref ISignature \u0003, _IUserdefType \u0004)
		{
			if (\u0002.ExpectedType != null && \u0004 != null && \u0002.ExpectedType is _IUserdefType)
			{
				ISignature signature = (\u0002.ExpectedType as _IUserdefType).GetSignature(this.Scope);
				string arg = \u0002.Callee.ToString();
				if (\u0003 != null && (\u0003.POUType == Operator.Method || \u0003.POUType == Operator.Action))
				{
					\u0003 = this.Scope[\u0003.ParentSignatureId];
					if (\u0002.Callee is _ICompoAccessExpression)
					{
						arg = (\u0002.Callee as _ICompoAccessExpression).Left.ToString();
					}
				}
				if (\u0003 != null && signature != null && signature.Id != \u0003.Id)
				{
					\u0002.AddMessage(string.Format(global::\u0081.\u0002.Err_NotAnInstanceOf, arg, \u0002.ExpectedType), \u0002._Position, Severity.Error, \u0002.PositionLength, MessageId.Err_NotAnInstanceOf);
				}
			}
		}

		// Token: 0x06002C96 RID: 11414 RVA: 0x0009E3E0 File Offset: 0x0009C5E0
		private ISignature \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			ISignature signature = null;
			if (this.\u0001 != null)
			{
				if (this.\u0001 != null)
				{
					signature = this.Scope[this.\u0001.SignatureId];
				}
				if (signature != null && signature.ParentSignatureId != Helper.InvalidId)
				{
					signature = this.Scope[signature.ParentSignatureId];
				}
				if (\u0003 != null && \u0003.GetFlag(SignatureFlag.Private) && signature != null && signature.Id != \u0003.ParentSignatureId && signature.Id != \u0003.Id)
				{
					ISignature signature2 = this.Scope[\u0003.ParentSignatureId];
					string text = (signature2 == null) ? "???" : signature2.OrgName;
					this.AddError(\u0002, MessageId.Err_CallOfPrivateMethod, new object[]
					{
						text,
						\u0003.OrgName
					});
				}
				if (\u0003 != null && \u0003.GetFlag(SignatureFlag.Protected))
				{
					bool flag = false;
					for (ISignature signature3 = signature; signature3 != null; signature3 = this.Scope[signature3.BaseSignatureId])
					{
						if (signature3.Id == \u0003.ParentSignatureId || signature3.Id == \u0003.Id)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						ISignature signature4 = this.Scope[\u0003.ParentSignatureId];
						string text2 = (signature4 == null) ? "???" : signature4.OrgName;
						this.AddError(\u0002, MessageId.Err_CallOfProtectedMethod, new object[]
						{
							text2,
							\u0003.OrgName
						});
					}
				}
			}
			return signature;
		}

		// Token: 0x06002C97 RID: 11415 RVA: 0x0009E554 File Offset: 0x0009C754
		private void \u0001(IList<_IExpression> \u0002)
		{
			foreach (_IExpression iexpression in \u0002)
			{
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
				if (!iexpression.IsLValue(this.Scope, this.\u0002))
				{
					this.AddError(iexpression, MessageId.Err_IsNoLValue, new object[]
					{
						iexpression
					});
				}
				else if (iexpression.GetVariable(this.Scope) != null && (iexpression.GetVariable(this.Scope) as _IVariable).IsProperty)
				{
					this.AddError(iexpression, MessageId.Err_NoPropertiesInOutAssignment, new object[]
					{
						iexpression
					});
				}
			}
		}

		// Token: 0x06002C98 RID: 11416 RVA: 0x0009E608 File Offset: 0x0009C808
		private bool \u0001(_ICallExpression \u0002, out _IUserdefType \u0003)
		{
			\u0003 = null;
			bool result = false;
			if (\u0002._Callee.Type != null && \u0002._Callee.Type.DeRefType.Class == TypeClass.Userdef)
			{
				\u0003 = (\u0002._Callee.Type.DeRefType as _IUserdefType);
				ISignature signature = \u0003.GetSignature(this.Scope);
				if (\u0002._Callee.Type.Class == TypeClass.Reference && signature.POUType == Operator.FunctionBlock)
				{
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06002C99 RID: 11417 RVA: 0x0009E688 File Offset: 0x0009C888
		private void \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			for (int i = 0; i < \u0002.Outputs.Count; i++)
			{
				_IVariableExpression ivariableExpression = \u0002.Outputs[i] as _IVariableExpression;
				if (ivariableExpression != null)
				{
					IVariable variable = \u0003[ivariableExpression.VariableId];
					if (variable == null || !variable.GetFlag(VarFlag.Output))
					{
						this.AddError(\u0002, MessageId.Err_IsNoOutput, new object[]
						{
							ivariableExpression,
							\u0003.Name
						});
					}
					else
					{
						_IExpression iexpression = \u0002.Outputs[i];
						bool flag = false;
						this.\u0001(this, \u0002.Outputs[i], variable, \u0002.Outputs[i].Type, \u0002.OutputExpressions[i].Type, this.Scope, ref iexpression, out flag);
						if (flag)
						{
							this.\u0001(this.Scope, \u0002.Outputs[i], \u0002.Outputs[i].Type, \u0002.OutputExpressions[i].Type);
						}
					}
				}
			}
		}

		// Token: 0x06002C9A RID: 11418 RVA: 0x0009E794 File Offset: 0x0009C994
		private bool \u0001(_ICallExpression \u0002, IList<_IVariable> \u0003, int \u0004, int \u0005, _ISignature \u0006, out bool \u0007, out IVariable \u0008, out _IArrayType \u000E)
		{
			\u0007 = false;
			\u0008 = null;
			\u000E = null;
			if (\u0003.Count > 0)
			{
				\u0008 = \u0003[\u0003.Count - 1];
			}
			if (\u0008 != null && \u0008.Name == IdentifierConstants.InstancePointer && \u0003.Count > 1)
			{
				\u0008 = \u0003[\u0003.Count - 2];
			}
			if (\u0008 != null && \u0008.GetFlag(VarFlag.ImplicitParamsStruct))
			{
				_IUserdefType iuserdefType = \u0008.CompiledType as _IUserdefType;
				\u0007 = true;
				if (iuserdefType == null || iuserdefType.GetNumOfElements(this.Scope) == 0)
				{
					return false;
				}
				\u000E = (iuserdefType.GetComponent(1, this.Scope) as _IArrayType);
			}
			bool result = true;
			if (\u0003.Count != \u0004 && !\u0007)
			{
				result = global::\u0082.\u0007.\u0001(\u0002, \u0004, \u0005, \u0006, new \u0082.\u0007.\u0001(this.AddError), new \u0082.\u0007.\u0002(this.\u0002));
			}
			return result;
		}

		// Token: 0x06002C9B RID: 11419 RVA: 0x0009E880 File Offset: 0x0009CA80
		private bool \u0001(_ICallExpression \u0002)
		{
			bool flag = \u0002.Inputs[0] == null;
			for (int i = 1; i < \u0002.Inputs.Count; i++)
			{
				if (\u0002.Inputs[i] == null != flag)
				{
					flag = true;
					break;
				}
			}
			return flag;
		}

		// Token: 0x06002C9C RID: 11420 RVA: 0x0009E8CC File Offset: 0x0009CACC
		private void \u0001(_ICallExpression \u0002, ISignature \u0003, IVariable \u0004, _IArrayType \u0005, int \u0006, bool \u0007)
		{
			if (\u0007)
			{
				string attributeValue = \u0004.GetAttributeValue(CompileAttributes.ATTRIBUTE_PARAMS_MINIMAL_NUMBER);
				int num;
				if (attributeValue != null && int.TryParse(attributeValue, out num) && num > \u0006)
				{
					this.AddError(\u0002, MessageId.Err_TooFewParametersForExtensibleFunction, new object[]
					{
						\u0003.OrgName,
						num,
						\u0004.OrgName
					});
					return;
				}
				IScope5 scope = this.Scope.CreateLocalScope(\u0003);
				bool flag;
				int num2 = \u0005._Dimensions[0].Range(out flag, scope);
				if (flag && num2 < \u0006)
				{
					this.AddError(\u0002, MessageId.Err_TooManyParametersForExtensibleFunction, new object[]
					{
						\u0003.OrgName,
						num2,
						\u0004.OrgName
					});
				}
			}
		}

		// Token: 0x06002C9D RID: 11421 RVA: 0x0009E988 File Offset: 0x0009CB88
		private void \u0001(_ICallExpression \u0002, IList<_IVariable> \u0003, int \u0004, int \u0005, ISignature \u0006)
		{
			bool flag = false;
			IVariable u = null;
			_IArrayType iarrayType = null;
			if (!this.\u0001(\u0002, \u0003, \u0004, \u0005, (_ISignature)\u0006, out flag, out u, out iarrayType))
			{
				return;
			}
			this.\u0001(\u0002, \u0006);
			if (\u0002.Inputs.Count != 0)
			{
				if (this.\u0001(\u0002))
				{
					IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
					ICompiledType compiledType = null;
					int index = -1;
					_IVariableExpression ivariableExpression = null;
					for (int i = 0; i < paramExpressions.Count; i++)
					{
						if (compiledType != null || (i < \u0003.Count && \u0003[i].CompiledType != null))
						{
							if (paramExpressions[i].Type == null)
							{
								if (\u0003[i].CompiledType == null)
								{
									goto IL_2FA;
								}
								this.AddError(paramExpressions[i], MessageId.Err_TypeMismatch, new object[]
								{
									global::\u0003.\u0006.\u0001(MessageId.Err_UnknownType, new object[]
									{
										paramExpressions[i].ToString()
									}),
									\u0003[i].CompiledType
								});
							}
							if (compiledType == null && \u0003[i].GetFlag(VarFlag.ImplicitParamsStruct))
							{
								compiledType = global::\u0019.\u0003.\u0001(iarrayType.BaseType as _IType, iarrayType._Dimensions[0]._UpperBorder);
								index = i;
							}
							if (compiledType != null)
							{
								_IExpression iexpression = paramExpressions[i];
								TypeCheckerVisitor.\u0001(this, paramExpressions[i], paramExpressions[i].Type, compiledType.BaseType, this.Scope, ref iexpression);
							}
							else
							{
								_IExpression iexpression = paramExpressions[i];
								bool flag2 = APEnvironmentFacade.Instance.LanguageModelMgr.AttributeManager.IsCheckFunction(\u0006);
								bool u2 = this.InImplicitCode;
								if (flag2)
								{
									this.InImplicitCode = true;
								}
								if (iexpression is _IAssignmentExpression)
								{
									this.AddError(\u0002, MessageId.Err_IsNoInput, new object[]
									{
										(iexpression as _IAssignmentExpression).LValue,
										\u0006.Name
									});
								}
								else
								{
									this.\u0001(\u0002, \u0006, i, \u0003[i], paramExpressions[i]);
								}
								this.InImplicitCode = u2;
							}
							if (compiledType == null)
							{
								_IVariableExpression ivariableExpression2 = global::\u0019.\u0003.\u0001(\u0003[i].OrgName);
								ivariableExpression2.VariableId = \u0003[i].Id;
								ivariableExpression2.SignatureId = \u0006.Id;
								ivariableExpression2.Type = \u0003[i]._Type;
								if (\u0002.Inputs[i] != null && \u0002.Inputs[i].VariableId != ivariableExpression2.VariableId)
								{
									this.AddError(\u0002.Inputs[i], MessageId.Err_WrongFormalParameter, new object[]
									{
										ivariableExpression2
									});
								}
								else
								{
									\u0002.SetFormalParam(ivariableExpression2, i);
								}
							}
							else
							{
								if (ivariableExpression == null)
								{
									ivariableExpression = global::\u0019.\u0003.\u0001(\u0003[index].OrgName);
									ivariableExpression.VariableId = \u0003[index].Id;
									ivariableExpression.SignatureId = \u0006.Id;
									ivariableExpression.Type = compiledType.BaseType;
									ivariableExpression.SetFlag(VarExprFlag.FormalParamsParameter, true);
								}
								\u0002.SetFormalParam(ivariableExpression, i);
							}
						}
						IL_2FA:;
					}
				}
				else if (flag)
				{
					this.AddError(\u0002, MessageId.Err_NoFormalParamsCallsForExtensibleFunction, new object[]
					{
						\u0006.OrgName
					});
				}
				else if (!APEnvironmentFacade.Instance.LanguageModelMgr.AttributeManager.IsCheckFunction(\u0006))
				{
					IList<_IExpression> inputs = \u0002.Inputs;
					for (int j = 0; j < inputs.Count; j++)
					{
						_IVariableExpression ivariableExpression3 = inputs[j] as _IVariableExpression;
						if (ivariableExpression3 != null)
						{
							IVariable u3 = null;
							if (ivariableExpression3.SignatureId == \u0006.Id)
							{
								u3 = \u0006[ivariableExpression3.VariableId];
							}
							this.\u0001(\u0002, \u0006, j, u3, ivariableExpression3);
						}
					}
				}
				IList<_IExpression> inputs2 = \u0002.Inputs;
				IList<_IExpression> paramExpressions2 = \u0002.ParamExpressions;
				IList<_IVariable> list = new LList<_IVariable>(inputs2.Count);
				for (int k = 0; k < inputs2.Count; k++)
				{
					_IVariableExpression ivariableExpression4 = inputs2[k] as _IVariableExpression;
					if (ivariableExpression4 != null)
					{
						_IVariable ivariable = \u0006[ivariableExpression4.VariableId] as _IVariable;
						list.Add(ivariable);
						if (ivariable != null && ivariable.HasFlag(VarFlag.Input | VarFlag.Inout))
						{
							this.\u0001(\u0002, \u0006, paramExpressions2[k], ivariable);
						}
					}
				}
				this.\u0001(\u0002, list);
			}
			this.\u0001(\u0002, \u0006, u, iarrayType, \u0004, flag);
		}

		// Token: 0x06002C9E RID: 11422 RVA: 0x0009EDE4 File Offset: 0x0009CFE4
		private void \u0001(_ICallExpression \u0002, ISignature \u0003, ISignature \u0004, bool \u0005)
		{
			TypeCheckerVisitor.\u0004 u = new TypeCheckerVisitor.\u0004();
			u.\u0001 = \u0003;
			for (int i = 0; i < \u0002.Inputs.Count; i++)
			{
				_IVariableExpression ivariableExpression = \u0002.Inputs[i] as _IVariableExpression;
				if (ivariableExpression == null)
				{
					this.AddError(\u0002, MessageId.Err_InputMissing, new object[]
					{
						\u0002.ParamExpressions[i],
						u.\u0001.Name
					});
				}
				else if (ivariableExpression.SignatureId == Helper.InvalidId || ivariableExpression.VariableId == Helper.InvalidId)
				{
					this.AddError(\u0002, MessageId.Err_IsNoInput, new object[]
					{
						ivariableExpression,
						u.\u0001.Name
					});
				}
				else
				{
					_IVariable u2 = this.Scope[ivariableExpression.SignatureId][ivariableExpression.VariableId] as _IVariable;
					this.\u0001(\u0002, u.\u0001, i, u2, ivariableExpression);
				}
			}
			for (int j = 0; j < \u0002.Outputs.Count; j++)
			{
				_IVariableExpression ivariableExpression2 = \u0002.Outputs[j] as _IVariableExpression;
				if (ivariableExpression2 != null)
				{
					if (ivariableExpression2.SignatureId == Helper.InvalidId || ivariableExpression2.VariableId == Helper.InvalidId)
					{
						this.AddError(\u0002, MessageId.Err_IsNoOutput, new object[]
						{
							ivariableExpression2,
							u.\u0001.Name
						});
					}
					else
					{
						_IVariable ivariable = this.Scope[ivariableExpression2.SignatureId][ivariableExpression2.VariableId] as _IVariable;
						if (ivariable == null || !ivariable.GetFlag(VarFlag.Output))
						{
							this.AddError(\u0002, MessageId.Err_IsNoOutput, new object[]
							{
								ivariableExpression2,
								u.\u0001.Name
							});
						}
						else
						{
							_IExpression iexpression = \u0002.Outputs[j];
							bool flag;
							this.\u0001(this, \u0002.Outputs[j], ivariable, \u0002.Outputs[j].Type, \u0002.OutputExpressions[j].Type, this.Scope, ref iexpression, out flag);
						}
					}
				}
			}
			bool flag2 = !\u0005 || (\u0004 != null && TypeCheckerVisitor.\u0001(\u0004, this.Scope, true).All(new Func<ISignature, bool>(u.\u0001)) && !(\u0002._Callee is _IVariableExpression));
			foreach (ISignature signature in TypeCheckerVisitor.\u0001(u.\u0001, this.Scope, true))
			{
				foreach (IVariable variable in signature.All.Where(new Func<IVariable, bool>(TypeCheckerVisitor.<>c.<>9.\u0001)))
				{
					bool flag3 = false;
					for (int k = 0; k < \u0002.Inputs.Count; k++)
					{
						_IVariableExpression ivariableExpression3 = \u0002.Inputs[k] as _IVariableExpression;
						bool flag4 = true;
						if (ivariableExpression3 != null)
						{
							flag4 = (ivariableExpression3.SignatureId == signature.Id);
						}
						if (ivariableExpression3 != null && ivariableExpression3.VariableId == variable.Id && flag4)
						{
							this.\u0001(\u0002, signature, \u0002.ParamExpressions[k], variable as _IVariable);
							flag3 = true;
							break;
						}
					}
					if (flag3 && !flag2)
					{
						this.AddError(\u0002, MessageId.Err_InOutAssignedInActionCall, new object[]
						{
							variable.OrgName,
							u.\u0001.OrgName
						});
					}
					else if (!flag3 && flag2)
					{
						this.AddError(\u0002, MessageId.Err_InOutNotAssigned, new object[]
						{
							variable.OrgName,
							u.\u0001.OrgName
						});
					}
				}
			}
		}

		// Token: 0x06002C9F RID: 11423 RVA: 0x0009F1FC File Offset: 0x0009D3FC
		private static IEnumerable<ISignature> \u0001(ISignature \u0002, IScope \u0003, bool \u0004)
		{
			ISignature signature = \u0002;
			LDictionary<ISignature, ISignature> ldictionary = new LDictionary<ISignature, ISignature>();
			bool flag = true;
			do
			{
				ldictionary.Add(signature, signature);
				if ((flag && \u0004) || !flag)
				{
					yield return signature;
				}
				flag = false;
				signature = \u0003[signature.BaseSignatureId];
			}
			while (signature != null && !ldictionary.ContainsKey(signature));
			yield break;
		}

		// Token: 0x06002CA0 RID: 11424 RVA: 0x0009F21C File Offset: 0x0009D41C
		public void \u0002(_ICallExpression \u0002)
		{
			this.\u0001(\u0002);
			ISignature signature = null;
			_IUserdefType iuserdefType = null;
			if (\u0002.Callee.Type is _IUserdefType)
			{
				iuserdefType = (\u0002.Callee.Type as _IUserdefType);
				signature = iuserdefType.GetSignature(this.Scope);
			}
			this.\u0001(\u0002, ref signature, iuserdefType);
			ISignature u = this.\u0001(\u0002, signature);
			\u0002._Callee.Accept(this);
			IEnumerable<_IExpression> paramExpressions = \u0002.ParamExpressions;
			this.\u0002();
			this.TopOfStack.\u0001 |= TypeCheckerVisitor.\u0001.\u0004;
			foreach (_IExpression iexpression in paramExpressions)
			{
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
			this.TopOfStack.\u0001 ^= TypeCheckerVisitor.\u0001.\u0004;
			this.\u0003();
			IList<_IExpression> list = \u0002.OutputExpressions;
			this.\u0001(list);
			_IUserdefType iuserdefType2 = null;
			bool flag = this.\u0001(\u0002, out iuserdefType2);
			if (((iuserdefType2 != null) ? iuserdefType2.GetSignature(this.Scope) : null) == null)
			{
				this.AddError(\u0002._Callee, MessageId.Err_CalleeInvalidType, new object[]
				{
					\u0002._Callee
				});
				IVariable variable = \u0002._Callee.GetVariable(this.Scope);
				ISignature signature2 = this.Scope[\u0002._Callee.SignatureId];
				if (variable != null && signature2 != null)
				{
					string u2 = global::\u0081.\u0002.Inf_RelatedPosition;
					_ISourcePosition isourcePosition = variable.SourcePosition as _ISourcePosition;
					isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature2.LibraryPath), signature2.ObjectGuid);
					this.\u0001(\u0002._Callee, global::\u0019.\u0003.\u0001(isourcePosition, u2, Severity.Information, MessageId.Inf_RelatedPosition));
				}
			}
			else if (\u0002._Callee is ICallExpression || (this.\u0002(\u0002._Callee) && !flag))
			{
				if (\u0002._Callee.Type.Class != TypeClass.Reference)
				{
					this.AddError(\u0002._Callee, MessageId.Err_NoCallInInstancePath, new object[]
					{
						\u0002._Callee
					});
				}
			}
			else
			{
				IScope5 u3 = global::\u0004.\u0012.\u0001(this.Scope, this.\u0001, iuserdefType2, this.InterfaceAsInterface);
				this.\u0001(u3);
				this.TopOfStack.\u0002 = true;
				this.TopOfStack.\u0001 = AccessFlag.Write;
				foreach (_IExpression iexpression2 in \u0002.Inputs)
				{
					if (iexpression2 != null)
					{
						iexpression2.Accept(this);
					}
				}
				this.TopOfStack.\u0001 = AccessFlag.Read;
				list = \u0002.Outputs;
				foreach (_IExpression iexpression3 in list)
				{
					if (iexpression3 != null)
					{
						iexpression3.Accept(this);
					}
				}
				foreach (_IExpression iexpression4 in \u0002.EmptyAssigns)
				{
					if (iexpression4 != null)
					{
						iexpression4.Accept(this);
					}
				}
				this.\u0003();
				ISignature signature3 = iuserdefType2.GetSignature(this.Scope);
				if (signature3.POUType != Operator.Function && signature3.POUType != Operator.FunctionBlock && signature3.POUType != Operator.Program && signature3.POUType != Operator.Method && signature3.POUType != Operator.Action)
				{
					this.AddError(\u0002, MessageId.Err_WrongObjectType, new object[]
					{
						Scanner.GetTextOfOperator(signature3.POUType)
					});
				}
				this.\u0002(\u0002, signature3);
				string attributeValue = signature3.GetAttributeValue("no_explicit_call");
				if (!string.IsNullOrEmpty(attributeValue) && !this.InImplicitCode)
				{
					this.AddError(\u0002, MessageId.Err_NoExplicitCall, new object[]
					{
						signature3.OrgName,
						attributeValue
					});
				}
				bool flag2 = signature3.GetFlag(SignatureFlag.Action);
				if (flag2)
				{
					_ISignature isignature = signature3 as _ISignature;
					ISignature signature4 = this.Scope[isignature.ParentSignatureId];
					if (signature4 != null)
					{
						signature3 = signature4;
					}
				}
				if (signature3 == null)
				{
					return;
				}
				global::\u0084.\u001D.\u0001(\u0002, signature3, signature, this.\u0001, this.Scope);
				this.\u0002(\u0002, u, signature3, flag2);
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CA1 RID: 11425 RVA: 0x0009F674 File Offset: 0x0009D874
		private void \u0002(_ICallExpression \u0002, ISignature \u0003, ISignature \u0004, bool \u0005)
		{
			IList<_IVariable> list = Helper.\u0001(\u0004 as _ISignature);
			int num = \u0002.ParamExpressions.Count;
			int num2 = list.Count;
			Operator poutype = \u0004.POUType;
			if (poutype != Operator.Function)
			{
				if (poutype != Operator.Method)
				{
					this.\u0001(\u0002, \u0004, \u0003, \u0005);
					return;
				}
				if (\u0004[IdentifierConstants.InstancePointer] != null)
				{
					num++;
					num2--;
				}
			}
			this.\u0001(\u0002, list, num, num2, \u0004);
		}

		// Token: 0x06002CA2 RID: 11426 RVA: 0x0009F6E0 File Offset: 0x0009D8E0
		private void \u0002(_ICallExpression \u0002, ISignature \u0003)
		{
			string attributeValue = \u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBSOLETE);
			if (!string.IsNullOrEmpty(attributeValue) && !this.InImplicitCode)
			{
				string stWarning = string.Format(global::\u0081.\u0002.Wrn_Obsolete, \u0003.OrgName, attributeValue);
				\u0002.AddWarning(stWarning, MessageId.Wrn_Obsolete);
			}
		}

		// Token: 0x06002CA3 RID: 11427 RVA: 0x0009F728 File Offset: 0x0009D928
		internal void \u0001(_IExprement \u0002, ISignature \u0003, _IExpression \u0004, _IVariable \u0005)
		{
			ICompiledType type = \u0004.Type;
			if (type != null && type.Class == TypeClass.Pointer && this.TreatReferenceAsPointer)
			{
				return;
			}
			this.TypeCheckerInternal.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06002CA4 RID: 11428 RVA: 0x0009F75C File Offset: 0x0009D95C
		public void \u0001(_IOperatorExpression \u0002)
		{
			if (!this.\u0001(\u0002.Code))
			{
				this.AddError(\u0002, MessageId.Err_OperatorNotSupported, new object[]
				{
					Scanner.GetTextOfOperator(\u0002.Code)
				});
				return;
			}
			if (!\u0002.PositionOK)
			{
				this.AddError(\u0002, MessageId.Err_OperatorNotAllowedAtPosition, new object[]
				{
					\u0002.Code
				});
				return;
			}
			IEnumerable<_IExpression> operandsList = \u0002._OperandsList;
			this.\u0001(this.Scope, !Helper.\u0001(\u0002) && \u0002.Code != Operator.IndexOf && \u0002.Code != Operator.__LocalOffset && \u0002.Code != Operator.__CRC && \u0002.Code != Operator.__MaxOffset && \u0002.Code != Operator.__FCall && \u0002.Code != Operator.Adr);
			this.TopOfStack.\u0003 = (\u0002.Code == Operator.__VarInfo || \u0002.Code == Operator.__LocalOffset || \u0002.Code == Operator.__TypeOf);
			this.TopOfStack.\u0001 |= TypeCheckerVisitor.\u0001.\u0003;
			foreach (_IExpression iexpression in operandsList)
			{
				iexpression.Accept(this);
				this.TopOfStack.\u0003 = false;
			}
			this.TopOfStack.\u0001 ^= TypeCheckerVisitor.\u0001.\u0003;
			this.\u0003();
			if (\u0002.Type == null)
			{
				return;
			}
			OperationOnTypeChecker.CheckForValidOperationOnType(\u0002, this.Scope, this);
			\u0002.AcceptOperatorVisitor(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06002CA5 RID: 11429 RVA: 0x0009F8F4 File Offset: 0x0009DAF4
		public void \u0001(_ICastExpression \u0002)
		{
			\u0002.BaseExpression.Accept(this);
		}

		// Token: 0x06002CA6 RID: 11430 RVA: 0x0009F904 File Offset: 0x0009DB04
		private static bool \u0001(TypeCheckerVisitor.\u0001 \u0002, TypeCheckerVisitor.\u0001 \u0003)
		{
			return (\u0003 & \u0002) > TypeCheckerVisitor.\u0001.\u0001;
		}

		// Token: 0x06002CA7 RID: 11431 RVA: 0x0009F90C File Offset: 0x0009DB0C
		private bool \u0001(TypeCheckerVisitor.\u0001 \u0002)
		{
			TypeCheckerVisitor.ETypeStackContent[] array = this.\u0001.ToArray();
			return 2 <= array.Length && TypeCheckerVisitor.\u0001(array[0].\u0001, TypeCheckerVisitor.\u0001.\u0002) && TypeCheckerVisitor.\u0001(array[1].\u0001, \u0002);
		}

		// Token: 0x06002CA8 RID: 11432 RVA: 0x0009F950 File Offset: 0x0009DB50
		public void \u0001(_INewExpression \u0002)
		{
			if (!this.\u0001.SupportDynamicMemory)
			{
				this.AddError(\u0002, MessageId.Err_DynamicMemoryNotSupported, new object[]
				{
					APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationNameByGuid(this.\u0001.ApplicationGuid)
				});
			}
			if (this.\u0001(TypeCheckerVisitor.\u0001.\u0003) || this.\u0001(TypeCheckerVisitor.\u0001.\u0004))
			{
				this.AddError(\u0002, MessageId.Err_NoNewAssignmentInOtherExpression, Array.Empty<object>());
			}
			\u0002._Count.Accept(this);
			if (\u0002._TypeToCast is _IUserdefType)
			{
				if ((\u0002._TypeToCast as _IUserdefType).SignatureId == Helper.InvalidId)
				{
					this.AddError(\u0002, MessageId.Err_UnknownType, new object[]
					{
						\u0002._TypeToCast.ToString()
					});
				}
				bool flag;
				if (\u0002.ElementCount(out flag) != 1 || !flag)
				{
					this.AddError(\u0002, MessageId.Err_NewArrayOnUserdefNotAllowed, Array.Empty<object>());
				}
				ISignature signature = (\u0002._TypeToCast as _IUserdefType).GetSignature(this.Scope);
				if (signature != null && signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
				{
					this.AddError(\u0002, MessageId.Err_NewOnInterfaceNotPossible, Array.Empty<object>());
				}
				else if (signature != null && string.IsNullOrEmpty(signature.LibraryPath) && !signature.HasAttribute(CompileAttributes.ATTRIBUTE_ENABLE_DYNAMIC_CREATION))
				{
					this.AddError(\u0002, MessageId.Err_NewOnlyWithAttribute, Array.Empty<object>());
					string u = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
					_ISourcePosition isourcePosition = global::\u0019.\u0003.\u0001();
					isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature.LibraryPath), signature.ObjectGuid);
					this.\u0001(\u0002, global::\u0019.\u0003.\u0001(isourcePosition, u, Severity.Information, MessageId.Inf_RelatedPosition));
				}
				this.\u0001(\u0002, signature);
			}
			if (!\u0002.PositionOK)
			{
				this.AddError(\u0002, MessageId.Err_NewPositionNotOK, Array.Empty<object>());
			}
			_IExpression count = \u0002._Count;
			_IExpression count2 = count;
			TypeCheckerVisitor.\u0001(this, count, count.Type, TypeTable.UDInt, this.Scope, ref count2);
			\u0002._Count = count2;
		}

		// Token: 0x06002CA9 RID: 11433 RVA: 0x0009FB3C File Offset: 0x0009DD3C
		private void \u0001(_INewExpression \u0002, ISignature \u0003)
		{
			ISignature signature = (\u0003 != null) ? \u0003.GetSubSignature(IdentifierConstants.InitMethodName) : null;
			if (signature == null)
			{
				return;
			}
			IVariable[] inputs = signature.Inputs;
			if (inputs.Length <= 3 && \u0002._FBInitParams == null)
			{
				return;
			}
			if (\u0002._FBInitParams == null || \u0002._FBInitParams.Count != inputs.Length - 3)
			{
				this.AddError(\u0002, MessageId.Err_NoMatchingInitMethodFound, new object[]
				{
					\u0003.OrgName
				});
				return;
			}
			this.\u0001(\u0002, \u0003, inputs);
			IScope5 u = global::\u0004.\u0012.\u0001(this.Scope, this.\u0001, signature, false);
			foreach (IAssignmentExpression assignmentExpression in \u0002._FBInitParams)
			{
				this.\u0001(u);
				this.TopOfStack.\u0002 = true;
				(assignmentExpression.LValue as _IExprement).Accept(this);
				this.\u0003();
				(assignmentExpression.RValue as _IExprement).Accept(this);
				_IExpression rvalue = assignmentExpression.RValue as _IExpression;
				TypeCheckerVisitor.\u0001(this, assignmentExpression.RValue as _IExpression, assignmentExpression.RValue.Type, assignmentExpression.LValue.Type, this.Scope, ref rvalue);
				(assignmentExpression as _IAssignmentExpression)._RValue = rvalue;
			}
		}

		// Token: 0x06002CAA RID: 11434 RVA: 0x0009FC94 File Offset: 0x0009DE94
		private void \u0001(_INewExpression \u0002, ISignature \u0003, IVariable[] \u0004)
		{
			for (int i = 2; i < \u0004.Length - 1; i++)
			{
				bool flag = false;
				string name = \u0004[i].Name;
				using (IEnumerator<IAssignmentExpression> enumerator = \u0002._FBInitParams.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.LValue.ToString().ToUpperInvariant() == name)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					this.AddError(\u0002, MessageId.Err_NoMatchingInitMethodFound, new object[]
					{
						\u0003.OrgName
					});
				}
			}
		}

		// Token: 0x06002CAB RID: 11435 RVA: 0x0009FD30 File Offset: 0x0009DF30
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x06002CAC RID: 11436 RVA: 0x0009FD34 File Offset: 0x0009DF34
		public void \u0001(_IConversionExpression \u0002)
		{
			\u0002._Exp.Accept(this);
			if (!this.\u0001.TypeIsSupported(\u0002.From) && TypeTable.Get(\u0002.From) != null)
			{
				this.AddError(\u0002, MessageId.Err_UnsupportedType, new object[]
				{
					TypeTable.Get(\u0002.From).ToString()
				});
			}
			if (!this.\u0001.TypeIsSupported(\u0002.To) && TypeTable.Get(\u0002.To) != null)
			{
				this.AddError(\u0002, MessageId.Err_UnsupportedType, new object[]
				{
					TypeTable.Get(\u0002.To).ToString()
				});
			}
			if (\u0002._Exp.Type == null)
			{
				this.AddError(\u0002._Exp, MessageId.Err_TypeMismatch, new object[]
				{
					global::\u0003.\u0006.\u0001(MessageId.Err_UnknownType, new object[]
					{
						\u0002._Exp.ToString()
					}),
					TypeTable.Get(\u0002.From).ToString(),
					MessageId.Err_UnknownType
				});
			}
			_IExpression exp = \u0002._Exp;
			this.TypeCheckerInternal.ExplicitConversion = !\u0002.Implicit;
			if (\u0002.Implicit && \u0002._Exp.Type != null)
			{
				\u0002.From = \u0002._Exp.Type.DeRefType.Class;
			}
			TypeCheckerVisitor.\u0001(this, \u0002._Exp, \u0002._Exp.Type, \u0002.From, this.Scope, ref exp);
			this.TypeCheckerInternal.ExplicitConversion = false;
			\u0002._Exp = exp;
			this.\u0001(\u0002);
		}

		// Token: 0x06002CAD RID: 11437 RVA: 0x0009FEB8 File Offset: 0x0009E0B8
		public void \u0001(_IThisExpression \u0002)
		{
			if (\u0002.Type == null)
			{
				this.AddError(\u0002, MessageId.Err_ThisNotAllowed, Array.Empty<object>());
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CAE RID: 11438 RVA: 0x0009FED8 File Offset: 0x0009E0D8
		public void \u0001(_IBaseExpression \u0002)
		{
			if (\u0002.Type == null)
			{
				this.AddError(\u0002, MessageId.Err_BaseNotAllowed, Array.Empty<object>());
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CAF RID: 11439 RVA: 0x0009FEF8 File Offset: 0x0009E0F8
		public void \u0001(_ILiteralExpression \u0002)
		{
			if (\u0002.ConstantType == TypeClass.AnyReal)
			{
				double realValue = \u0002.RealValue;
				double num = (realValue < 0.0) ? (-realValue) : realValue;
				if ((realValue > 3.4028234663852886E+38 || realValue < -3.4028234663852886E+38 || num < 1.4012984643248171E-45) && num != 0.0)
				{
					\u0002.ConstantType = TypeClass.LReal;
					if (!this.\u0001.TypeIsSupported(TypeClass.LReal))
					{
						\u0002.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ConstantOverflow, new object[]
						{
							\u0002.ToString(),
							TypeTable.Get(Operator.Real).ToString()
						}), MessageId.Err_ConstantOverflow);
					}
				}
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CB0 RID: 11440 RVA: 0x0009FFA4 File Offset: 0x0009E1A4
		public void \u0001(_IAddressExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CB1 RID: 11441 RVA: 0x0009FFB0 File Offset: 0x0009E1B0
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06002CB2 RID: 11442 RVA: 0x0009FFB4 File Offset: 0x0009E1B4
		private bool \u0001(ISignature \u0002, ISignature \u0003)
		{
			if (\u0002.Id == \u0003.Id)
			{
				return true;
			}
			int num = \u0002.Id;
			if (\u0002.Name == "__MAIN" || \u0002.GetFlag(SignatureFlag.Action))
			{
				num = \u0002.ParentSignatureId;
			}
			LHashSet<int> lhashSet = new LHashSet<int>();
			while (num != Helper.InvalidId && !lhashSet.Contains(num))
			{
				if (num == \u0003.Id)
				{
					return true;
				}
				lhashSet.Add(num);
				ISignature signature = this.Scope[num];
				num = ((signature != null) ? signature.BaseSignatureId : Helper.InvalidId);
			}
			return false;
		}

		// Token: 0x06002CB3 RID: 11443 RVA: 0x000A004C File Offset: 0x0009E24C
		private void \u0001(_IVariableExpression \u0002, _ISignature \u0003)
		{
			bool flag = !this.TopOfStack.\u0004 && !this.TopOfStack.\u0005;
			if (flag)
			{
				flag = (\u0002.VariableId == Helper.InvalidId && \u0002.SignatureId != Helper.InvalidId);
			}
			if (flag)
			{
				flag = (\u0003 != null);
			}
			if (flag)
			{
				flag = (\u0003.GetFlag(SignatureFlag.Enum) || \u0003.GetFlag(SignatureFlag.Alias));
			}
			if (flag)
			{
				flag = this.TopOfStack.\u0001;
			}
			if (flag)
			{
				bool flag2 = true;
				if (\u0003.GetFlag(SignatureFlag.Alias))
				{
					flag2 = (Helper.\u0001(\u0003, this.\u0001) != null);
				}
				if (flag2)
				{
					this.AddError(\u0002, MessageId.Err_UnexpectedTypeName, new object[]
					{
						\u0003.OrgName
					});
				}
			}
		}

		// Token: 0x06002CB4 RID: 11444 RVA: 0x000A010C File Offset: 0x0009E30C
		public void \u0001(_IVariableExpression \u0002)
		{
			if (\u0002.Type == null)
			{
				this.AddError(\u0002, MessageId.Err_IdentNotDefined, new object[]
				{
					\u0002.Name
				});
			}
			_ISignature isignature = \u0002.GetSignature(this.Scope) as _ISignature;
			IVariable variable = \u0002.GetVariable(this.Scope);
			ISignature signature = null;
			if (variable != null)
			{
				signature = this.Scope[\u0002.SignatureId];
			}
			this.\u0001(\u0002, variable, signature);
			this.\u0003(\u0002, variable, signature);
			this.\u0004(\u0002, variable, signature);
			if (!this.\u0002 && !this.TopOfStack.\u0002 && !this.TopOfStack.\u0003 && signature != null && this.\u0001 != null && this.\u0001.Id != signature.Id && this.\u0001.ParentSignatureId != signature.Id && (signature.POUType == Operator.Function || signature.POUType == Operator.Method || variable.GetFlag(VarFlag.Temp)))
			{
				if (variable.GetFlag(VarFlag.Input))
				{
					this.AddError(\u0002, MessageId.Err_AccessOutsideOfCall, new object[]
					{
						variable.OrgName,
						signature.OrgName
					});
				}
				else if (variable.GetFlag(VarFlag.AllocateInInstance))
				{
					this.AddError(\u0002, MessageId.Err_AccessVarInstFromOutsideTheDeclaringMethod, new object[]
					{
						variable.OrgName,
						signature.OrgName
					});
				}
				else
				{
					this.AddError(\u0002, MessageId.Err_IsNoInput, new object[]
					{
						variable.OrgName,
						signature.OrgName
					});
				}
			}
			this.\u0001(\u0002, isignature);
			if (isignature != null && (isignature.POUType == Operator.FunctionBlock || isignature.POUType == Operator.Type) && this.TopOfStack.\u0001)
			{
				if (variable == null)
				{
					bool flag = false;
					if (isignature.POUType == Operator.FunctionBlock)
					{
						this.AddError(\u0002, MessageId.Err_FunctionBlockNeedsInstance, new object[]
						{
							isignature.OrgName
						});
						flag = true;
					}
					else if (isignature.POUType != Operator.Type || !isignature.GetFlag(SignatureFlag.Alias) || isignature.AllVariables.First<_IVariable>().CompiledType.Class != TypeClass.Enum)
					{
						this.AddError(\u0002, MessageId.Err_UnexpectedTypeName, new object[]
						{
							isignature.OrgName
						});
						flag = true;
					}
					if (flag)
					{
						_ISourcePosition isourcePosition = global::\u0019.\u0003.\u0002();
						isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(isignature.LibraryPath), isignature.ObjectGuid);
						string u = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
						this.\u0001(\u0002, global::\u0019.\u0003.\u0001(isourcePosition, u, Severity.Information, MessageId.Inf_RelatedPosition));
					}
				}
			}
			else if (isignature != null && isignature.POUType == Operator.Interface && this.TopOfStack.\u0001 && variable == null)
			{
				this.AddError(\u0002, MessageId.Err_InterfaceNeedsInstance, new object[]
				{
					isignature.OrgName
				});
			}
			this.\u0001(\u0002);
			ISignature signature2 = null;
			if (this.\u0001 != null)
			{
				signature2 = this.Scope[this.\u0001.SignatureId];
			}
			else if (this.CheckingInitialValue)
			{
				signature2 = this.InitialValueContextSignature;
			}
			if (signature2 != null && signature2.POUType == Operator.Method)
			{
				signature2 = this.Scope[signature2.ParentSignatureId];
			}
			this.\u0001(\u0002, isignature, signature2, variable, signature);
			this.\u0002(\u0002, variable, signature2);
		}

		// Token: 0x06002CB5 RID: 11445 RVA: 0x000A0458 File Offset: 0x0009E658
		private void \u0001(_IVariableExpression \u0002, IVariable \u0003, ISignature \u0004)
		{
			if (\u0003 != null && \u0003.GetFlag(VarFlag.Inout))
			{
				if (\u0004 != null && this.\u0001 != null && !this.\u0001.HasAttribute(CompileAttributes.ATTRIBUTE_ALLOW_EXTERNAL_VAR_IN_OUT_ACCESS) && !this.TopOfStack.\u0002 && !this.\u0001(this.\u0001, \u0004))
				{
					this.\u0001(\u0002, MessageId.Wrn_NonLocalAccessToVarInOut, new object[]
					{
						\u0003.OrgName,
						\u0004.OrgName,
						this.\u0001.OrgName
					});
				}
				if (this.CheckingInitialValue && AccessFlag.Write != this.TopOfStack.\u0001)
				{
					this.\u0001(\u0002, MessageId.Wrn_VarInOutUnitializedInInitialValue, Array.Empty<object>());
				}
			}
		}

		// Token: 0x06002CB6 RID: 11446 RVA: 0x000A050C File Offset: 0x0009E70C
		private void \u0002(_IVariableExpression \u0002, IVariable \u0003, ISignature \u0004)
		{
			if (\u0003 != null && (\u0003 as _IVariable).IsProperty)
			{
				_ISignature isignature = this.Scope[\u0002.SignatureId] as _ISignature;
				ISignature signature = null;
				ISignature signature2 = null;
				if (isignature != null)
				{
					IScope5 scope = this.Scope.CreateLocalScope(isignature);
					signature = scope.FindSignatureLocal(IdentifierConstants.CreateGetterName((\u0003 as _IVariable).VersionedName));
					signature2 = scope.FindSignatureLocal(IdentifierConstants.CreateSetterName((\u0003 as _IVariable).VersionedName));
				}
				if ((!\u0002.GetFlag(VarExprFlag.WriteAccess) || (\u0003.Type.Class == TypeClass.Reference && this.TopOfStack.\u0001 != Operator.RefAssign)) && !\u0003.HasAttribute(CompileAttributes.GET_ACCESS) && !\u0003.HasAttribute(CompileAttributes.DEVICE_PARAMETER))
				{
					isignature = (this.Scope[\u0002.SignatureId] as _ISignature);
					if (isignature != null && signature == null)
					{
						this.AddError(\u0002, MessageId.Err_NoReadAccessToProperty, new object[]
						{
							\u0002.ToString()
						});
					}
				}
				ISignature signature3 = signature;
				if (\u0002.GetFlag(VarExprFlag.WriteAccess) && \u0003.Type.Class != TypeClass.Reference)
				{
					signature3 = signature2;
				}
				if (\u0004 != null && signature3 != null && signature3.GetFlag(SignatureFlag.Private) && \u0004.Id != signature3.ParentSignatureId && \u0004.Id != signature3.Id)
				{
					ISignature signature4 = this.Scope[signature3.ParentSignatureId];
					this.\u0002(\u0002, MessageId.Err_CallOfPrivateProperty, new object[]
					{
						signature4.OrgName,
						\u0003.OrgName
					});
				}
				if (\u0004 != null && signature3 != null && signature3.GetFlag(SignatureFlag.Internal) && !string.Equals(\u0004.LibraryPath, signature3.LibraryPath, StringComparison.OrdinalIgnoreCase))
				{
					ISignature signature5 = this.Scope[signature3.ParentSignatureId];
					string text = signature5.OrgName + "." + \u0003.OrgName;
					this.\u0002(\u0002, MessageId.Err_AccessToInternalProperty, new object[]
					{
						text,
						signature5.LibraryPath
					});
				}
				if (\u0004 != null && signature3 != null && signature3.GetFlag(SignatureFlag.Protected))
				{
					ISignature signature6 = \u0004;
					bool flag = false;
					while (signature6 != null)
					{
						if (signature6.Id == signature3.ParentSignatureId || signature6.Id == signature3.Id)
						{
							flag = true;
						}
						signature6 = this.Scope[signature6.BaseSignatureId];
					}
					if (!flag)
					{
						ISignature signature7 = this.Scope[signature3.ParentSignatureId];
						this.\u0002(\u0002, MessageId.Err_CallOfProtectedProperty, new object[]
						{
							signature7.OrgName,
							\u0003.OrgName
						});
					}
				}
			}
		}

		// Token: 0x06002CB7 RID: 11447 RVA: 0x000A079C File Offset: 0x0009E99C
		private void \u0002(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			if (this.CheckingInitialValue)
			{
				this.\u0001(\u0002, TypeCheckerVisitor.\u0001[\u0003], \u0004);
				return;
			}
			this.AddError(\u0002, \u0003, \u0004);
		}

		// Token: 0x06002CB8 RID: 11448 RVA: 0x000A07C4 File Offset: 0x0009E9C4
		private void \u0001(_IVariableExpression \u0002, _ISignature \u0003, ISignature \u0004, IVariable \u0005, ISignature \u0006)
		{
			if (\u0003 != null && \u0003.GetFlag(SignatureFlag.Internal))
			{
				if (\u0004 != null && !string.Equals(\u0004.LibraryPath, \u0003.LibraryPath, StringComparison.OrdinalIgnoreCase))
				{
					ISignature signature = this.Scope[\u0003.ParentSignatureId];
					if (\u0005 != null && \u0003 != null)
					{
						string text = \u0003.OrgName + "." + \u0005.OrgName;
						this.\u0002(\u0002, MessageId.Err_AccessToInternalVariable, new object[]
						{
							text,
							\u0003.LibraryPath
						});
						return;
					}
					if (\u0003 != null && signature != null)
					{
						string text2 = signature.OrgName + "." + \u0003.OrgName;
						this.\u0002(\u0002, MessageId.Err_AccessToInternalObject, new object[]
						{
							text2,
							\u0003.LibraryPath
						});
						return;
					}
					if (\u0003 != null)
					{
						this.\u0002(\u0002, MessageId.Err_AccessToInternalObject, new object[]
						{
							\u0003.OrgName,
							\u0003.LibraryPath
						});
						return;
					}
				}
			}
			else if (\u0005 != null && (\u0005.GetFlag(VarFlag.Internal) || (\u0006 != null && \u0006.GetFlag(SignatureFlag.Internal))) && \u0006 != null && \u0004 != null && !string.Equals(\u0004.LibraryPath, \u0006.LibraryPath, StringComparison.OrdinalIgnoreCase))
			{
				string text3 = \u0006.OrgName + "." + \u0005.OrgName;
				this.\u0002(\u0002, MessageId.Err_AccessToInternalVariable, new object[]
				{
					text3,
					\u0006.LibraryPath
				});
			}
		}

		// Token: 0x06002CB9 RID: 11449 RVA: 0x000A0944 File Offset: 0x0009EB44
		private void \u0003(_IVariableExpression \u0002, IVariable \u0003, ISignature \u0004)
		{
			if (!this.\u0002 && \u0003 != null && !(\u0003 as _IVariable).IsProperty && (\u0003.GetFlag(VarFlag.Output) || \u0003.GetFlag(VarFlag.Local)) && \u0002.GetFlag(VarExprFlag.WriteAccess) && !\u0002.GetFlag(VarExprFlag.InitializingWriteAccess) && \u0004 != null && (\u0004.POUType == Operator.FunctionBlock || \u0004.POUType == Operator.Program))
			{
				bool flag = false;
				if (this.\u0001 != null)
				{
					flag = (this.\u0001.Id == \u0002.SignatureId || this.\u0001.ParentSignatureId == \u0002.SignatureId);
					if (!flag)
					{
						int nId = this.\u0001.Id;
						if (this.\u0001.ParentSignatureId != Helper.InvalidId)
						{
							nId = this.\u0001.ParentSignatureId;
						}
						for (ISignature signature = this.Scope[nId]; signature != null; signature = this.Scope[signature.BaseSignatureId])
						{
							flag = (signature.Id == \u0002.SignatureId);
							if (flag)
							{
								break;
							}
						}
					}
				}
				if (!flag)
				{
					this.AddError(\u0002, MessageId.Err_IsNoInput, new object[]
					{
						\u0003.OrgName,
						\u0004.OrgName
					});
				}
			}
		}

		// Token: 0x06002CBA RID: 11450 RVA: 0x000A0A7C File Offset: 0x0009EC7C
		private void \u0004(_IVariableExpression \u0002, IVariable \u0003, ISignature \u0004)
		{
			if (\u0003 != null && \u0003.GetFlag(VarFlag.GenericConstant) && this.\u0001 != null && this.\u0001.Id != \u0002.SignatureId && this.\u0001.ParentSignatureId != \u0002.SignatureId)
			{
				this.AddError(\u0002, MessageId.Err_NoOutsideAccessToGenericVariable, new object[]
				{
					\u0003.OrgName,
					\u0004.OrgName
				});
			}
		}

		// Token: 0x06002CBB RID: 11451 RVA: 0x000A0AF4 File Offset: 0x0009ECF4
		internal bool \u0001(_IExpression \u0002)
		{
			if (\u0002 is IDeRefAccessExpression)
			{
				return false;
			}
			IVariable variable = \u0002.GetVariable(this.Scope);
			return variable != null && ((variable as _IVariable).HasAttribute(CompileAttributes.GET_BITACCESS) || (variable as _IVariable).HasAttribute(CompileAttributes.SET_BITACCESS) || (variable as _IVariable).HasAttribute(CompileAttributes.DEVICE_PARAMETER));
		}

		// Token: 0x06002CBC RID: 11452 RVA: 0x000A0B54 File Offset: 0x0009ED54
		internal bool \u0002(_IExpression \u0002)
		{
			if (\u0002 is IDeRefAccessExpression)
			{
				return false;
			}
			IVariable variable = \u0002.GetVariable(this.Scope);
			return variable != null && (variable as _IVariable).IsProperty;
		}

		// Token: 0x06002CBD RID: 11453 RVA: 0x000A0B88 File Offset: 0x0009ED88
		private bool \u0001(_IIndexAccessExpression \u0002, _IArrayType \u0003)
		{
			IList<_IArrayDimension> dimensions = \u0003._Dimensions;
			if (\u0002.NumAccesses != dimensions.Count)
			{
				this.AddError(\u0002, MessageId.Err_ArrayIndexNumWrong, new object[]
				{
					dimensions.Count
				});
				return true;
			}
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				if (this.\u0001(\u0002, i))
				{
					ILiteralValue literalValue = \u0002[i].Literal(this.Scope);
					int num;
					if (literalValue != null && literalValue.GetInt(out num))
					{
						bool flag;
						int num2 = dimensions[i].LowerBorderInt(out flag, this.Scope);
						bool flag2;
						int num3 = dimensions[i].UpperBorderInt(out flag2, this.Scope);
						if ((num < num2 || num > num3) && flag && flag2)
						{
							this.AddError(\u0002[i], MessageId.Err_ConstantIndexOutOfRange, new object[]
							{
								\u0002[i],
								num2,
								num3
							});
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06002CBE RID: 11454 RVA: 0x000A0C84 File Offset: 0x0009EE84
		private bool \u0001(_IIndexAccessExpression \u0002, int \u0003)
		{
			_IExpression value = \u0002[\u0003];
			if (\u0002[\u0003].Type == null)
			{
				this.AddError(\u0002[\u0003], MessageId.Err_TypeMismatch, new object[]
				{
					global::\u0003.\u0006.\u0001(MessageId.Err_UnknownType, new object[]
					{
						\u0002[\u0003].ToString()
					}),
					TypeClass.AnyInt.ToString()
				});
				return false;
			}
			TypeCheckerVisitor.\u0001(this, \u0002[\u0003], \u0002[\u0003].Type.DeRefType, TypeClass.AnyInt, this.Scope, ref value);
			\u0002[\u0003] = value;
			return true;
		}

		// Token: 0x06002CBF RID: 11455 RVA: 0x000A0D24 File Offset: 0x0009EF24
		private bool \u0001(_IIndexAccessExpression \u0002, ICompiledType \u0003)
		{
			if (\u0003.Class == TypeClass.Pointer)
			{
				IVariable variable = (\u0002.Var as _IExpression).GetVariable(this.Scope);
				if (variable != null && variable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
				{
					int num;
					if (int.TryParse(variable.GetAttributeValue("Dimensions"), out num) && num != \u0002.NumAccesses)
					{
						this.AddError(\u0002, MessageId.Err_IndexNumWrong, new object[]
						{
							\u0003
						});
					}
					for (int i = 0; i < \u0002.NumAccesses; i++)
					{
						this.\u0001(\u0002, i);
					}
					return false;
				}
			}
			if (\u0002.NumAccesses != 1)
			{
				this.AddError(\u0002, MessageId.Err_IndexNumWrong, new object[]
				{
					\u0003
				});
				return true;
			}
			_IExpression value = \u0002[0];
			if (\u0002[0].Type == null)
			{
				return false;
			}
			TypeCheckerVisitor.\u0001(this, \u0002[0], \u0002[0].Type.DeRefType, TypeClass.AnyInt, this.Scope, ref value);
			\u0002[0] = value;
			this.\u0001(\u0002, \u0003);
			return false;
		}

		// Token: 0x06002CC0 RID: 11456 RVA: 0x000A0E20 File Offset: 0x0009F020
		private void \u0001(_IIndexAccessExpression \u0002, ICompiledType \u0003)
		{
			int num = 0;
			if (\u0003.Class == TypeClass.Pointer)
			{
				num = int.MinValue;
			}
			int num2 = int.MaxValue;
			TypeClass @class = \u0003.Class;
			if (@class != TypeClass.String)
			{
				if (@class != TypeClass.WString)
				{
					if (@class == TypeClass.__Vector)
					{
						bool flag;
						num2 = (\u0003 as _IVectorType).DimensionInt(this.Scope, out flag) - 1;
					}
				}
				else
				{
					num2 = (\u0003 as _IWStringType).Size(this.Scope) - 1;
				}
			}
			else
			{
				num2 = (\u0003 as _IStringType).Size(this.Scope) - 1;
			}
			ILiteralValue literalValue = \u0002[0].Literal(this.Scope);
			int num3;
			if (literalValue != null && literalValue.GetInt(out num3) && (num3 < num || num3 > num2))
			{
				this.AddError(\u0002[0], MessageId.Err_ConstantIndexOutOfRange, new object[]
				{
					\u0002[0],
					num,
					num2
				});
			}
		}

		// Token: 0x06002CC1 RID: 11457 RVA: 0x000A0EFC File Offset: 0x0009F0FC
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			\u0002._Var.Accept(this);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				this.\u0002();
				this.TopOfStack.\u0004 = false;
				\u0002.GetAccess(i).Accept(this);
				this.\u0003();
			}
			if (\u0002._Var.Type == null)
			{
				return;
			}
			if ((\u0002._Var is ICallExpression || this.\u0002(\u0002._Var)) && \u0002._Var.Type.Class != TypeClass.Reference)
			{
				this.AddError(\u0002._Var, MessageId.Err_NoCallInInstancePath, new object[]
				{
					\u0002._Var
				});
			}
			ICompiledType deRefType = \u0002._Var.Type.DeRefType;
			TypeClass @class = deRefType.Class;
			if (@class <= TypeClass.Pointer)
			{
				if (@class - TypeClass.String > 1 && @class != TypeClass.Pointer)
				{
					goto IL_F1;
				}
			}
			else if (@class != TypeClass.Array)
			{
				if (@class != TypeClass.__Vector)
				{
					goto IL_F1;
				}
			}
			else
			{
				if (this.\u0001(\u0002, deRefType as _IArrayType))
				{
					return;
				}
				goto IL_109;
			}
			if (this.\u0001(\u0002, deRefType))
			{
				return;
			}
			goto IL_109;
			IL_F1:
			this.AddError(\u0002._Var, MessageId.Err_IndexingInvalid, new object[]
			{
				deRefType
			});
			IL_109:
			this.\u0001(\u0002);
		}

		// Token: 0x06002CC2 RID: 11458 RVA: 0x000A101C File Offset: 0x0009F21C
		private void \u0001(_IExpression \u0002)
		{
			_ILiteralExpression iliteralExpression = \u0002 as _ILiteralExpression;
			if (this.\u0001.GetCodegeneratorProperty(CodegeneratorProperties.LWordPointer))
			{
				iliteralExpression.ConstantType = TypeClass.LWord;
				iliteralExpression._CompiledType = TypeTable.LWord;
				return;
			}
			iliteralExpression.ConstantType = TypeClass.DWord;
			iliteralExpression._CompiledType = TypeTable.DWord;
		}

		// Token: 0x06002CC3 RID: 11459 RVA: 0x000A1064 File Offset: 0x0009F264
		private bool \u0001(IExpression \u0002, out long \u0003, out bool \u0004)
		{
			bool flag = false;
			\u0004 = true;
			\u0003 = -1L;
			_IExpression iexpression = (_IExpression)\u0002;
			ILiteralValue literalValue = iexpression.Literal(this.Scope, true);
			if (literalValue != null && literalValue.KindOf == KindOfLiteral.SignedInteger)
			{
				\u0003 = literalValue.GetSignedLong(out flag);
			}
			if (literalValue != null && literalValue.KindOf == KindOfLiteral.UnsignedInteger)
			{
				\u0003 = (long)literalValue.GetUnsignedLong(out flag);
				\u0004 = false;
			}
			return flag && iexpression.Type.DeRefType.IsInteger;
		}

		// Token: 0x06002CC4 RID: 11460 RVA: 0x000A10D4 File Offset: 0x0009F2D4
		private bool \u0001(_ICompoAccessExpression \u0002)
		{
			if (\u0002._Right.IsLiteral && !\u0002._Left.Type.DeRefType.IsInteger)
			{
				this.AddError(\u0002._Right, MessageId.Err_BitAccessOnlyOnInt, Array.Empty<object>());
			}
			if (!\u0002._Left.Type.DeRefType.IsInteger || \u0002._Right.Type == null)
			{
				return false;
			}
			bool flag = false;
			int num = -1;
			if (\u0002._Right.Literal(this.Scope) != null)
			{
				num = \u0002._Right.Literal(this.Scope).GetInt(out flag);
			}
			if (!flag || !\u0002._Right.Type.DeRefType.IsInteger)
			{
				this.AddError(\u0002._Right, MessageId.Err_Bitaccessnoconst, Array.Empty<object>());
			}
			if ((\u0002.Left is ICallExpression || this.\u0002(\u0002._Left)) && !this.\u0001(\u0002._Left))
			{
				this.AddError(\u0002._Left, MessageId.Err_NoBitAccessOnFunctionCall, Array.Empty<object>());
				return true;
			}
			int num2 = \u0002.Left.Type.DeRefType.Size(this.Scope) * 8;
			if (num >= num2)
			{
				this.AddError(\u0002._Right, MessageId.Err_BitNrOverflow, new object[]
				{
					\u0002._Right.ToString(),
					\u0002.Left.ToString()
				});
			}
			return true;
		}

		// Token: 0x06002CC5 RID: 11461 RVA: 0x000A1234 File Offset: 0x0009F434
		private void \u0001(_ICompoAccessExpression \u0002, _IUserdefType \u0003)
		{
			IVariable variable = \u0002._Left.GetVariable(this.Scope);
			if ((variable == null || !(variable as _IVariable).IsProperty || variable.Type.Class != TypeClass.Reference) && \u0002._Left.Type.Class != TypeClass.Reference && (\u0003 == null || \u0003.GetSignature(this.Scope) == null || !\u0003.GetSignature(this.Scope).GetFlag(SignatureFlag.ImplicitInterfaceUnion)))
			{
				this.AddError(\u0002._Left, MessageId.Err_NoCallInInstancePath, new object[]
				{
					\u0002._Left
				});
			}
		}

		// Token: 0x06002CC6 RID: 11462 RVA: 0x000A12D8 File Offset: 0x0009F4D8
		private void \u0002(_ICompoAccessExpression \u0002, _IUserdefType \u0003)
		{
			ISignature signature = \u0003.GetSignature(this.Scope);
			IScope5 scope = (IScope5)\u0003.GetScope(this.Scope);
			\u0002._Right.ClearMessages();
			if (signature != null)
			{
				this.AddError(\u0002._Right, MessageId.Err_NoComponentOf, new object[]
				{
					\u0002._Right,
					signature.OrgName
				});
				string u = global::\u0081.\u0002.Inf_RelatedPosition;
				_ISourcePosition isourcePosition = global::\u0019.\u0003.\u0001();
				isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature.LibraryPath), signature.ObjectGuid);
				this.\u0001(\u0002._Right, global::\u0019.\u0003.\u0001(isourcePosition, u, Severity.Information, MessageId.Inf_RelatedPosition));
				return;
			}
			if (scope != null)
			{
				this.AddError(\u0002._Right, MessageId.Err_ContainsNoDefinition, new object[]
				{
					scope.DisplayName,
					\u0002._Right
				});
			}
		}

		// Token: 0x06002CC7 RID: 11463 RVA: 0x000A13AC File Offset: 0x0009F5AC
		private void \u0001(_ICompoAccessExpression \u0002)
		{
			if (\u0002._Right.Type.Class == TypeClass.Reference)
			{
				IVariable variable = \u0002._Right.GetVariable(this.Scope);
				if (variable != null && (this.Scope.LocalSignature == null || !this.Scope.LocalSignature.HasAttribute(CompileAttributes.ATTRIBUTE_ALLOW_EXTERNAL_VAR_IN_OUT_ACCESS)) && variable.GetFlag(VarFlag.Inout))
				{
					this.\u0001(\u0002, variable);
					return;
				}
			}
			else if (\u0002._Right.Type.Class == TypeClass.Pointer)
			{
				IVariable variable2 = \u0002._Right.GetVariable(this.Scope);
				if (variable2 != null && variable2.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
				{
					this.\u0001(\u0002, variable2);
				}
			}
		}

		// Token: 0x06002CC8 RID: 11464 RVA: 0x000A145C File Offset: 0x0009F65C
		private void \u0003(_ICompoAccessExpression \u0002, _IUserdefType \u0003)
		{
			ISignature signature = null;
			if (\u0003 != null)
			{
				signature = \u0003.GetSignature(this.Scope);
			}
			if (signature != null && signature.POUType == Operator.Method && !this.TopOfStack.\u0003 && !this.TopOfStack.\u0002)
			{
				IVariable variable = \u0002._Right.GetVariable(this.Scope);
				if (variable != null && variable.GetFlag(VarFlag.AllocateInInstance))
				{
					this.AddError(\u0002._Right, MessageId.Err_AccessVarInstFromOutsideTheDeclaringMethod, new object[]
					{
						variable.OrgName,
						signature.OrgName
					});
					return;
				}
				this.AddError(\u0002._Right, MessageId.Err_IsNoInput, new object[]
				{
					\u0002.Right.ToString(),
					signature.OrgName
				});
			}
		}

		// Token: 0x06002CC9 RID: 11465 RVA: 0x000A1528 File Offset: 0x0009F728
		public void \u0002(_ICompoAccessExpression \u0002)
		{
			Operator u = this.TopOfStack.\u0001;
			this.\u0002();
			this.TopOfStack.\u0004 = true;
			this.TopOfStack.\u0001 = u;
			\u0002._Left.Accept(this);
			this.\u0003();
			if (\u0002._Left.Type == null)
			{
				if (\u0002._Left.MessagesList == null || \u0002._Left.MessagesList.Count == 0)
				{
					this.AddError(\u0002._Left, MessageId.Err_CompoAccessNoStruct, new object[]
					{
						\u0002._Left
					});
				}
				return;
			}
			_IUserdefType iuserdefType = \u0002._Left.Type.DeRefType as _IUserdefType;
			if (iuserdefType != null)
			{
				IScope5 scope = global::\u0004.\u0012.\u0001(this.Scope, this.\u0001, iuserdefType, this.InterfaceAsInterface);
				if (scope != null)
				{
					bool u2 = this.TopOfStack.\u0001;
					bool u3 = this.TopOfStack.\u0004;
					u = this.TopOfStack.\u0001;
					this.\u0001(scope);
					this.TopOfStack.\u0001 = u2;
					this.TopOfStack.\u0004 = u3;
					this.TopOfStack.\u0001 = u;
					\u0002._Right.Accept(this);
					this.\u0003();
				}
			}
			if (this.\u0001(\u0002))
			{
				return;
			}
			if (iuserdefType == null)
			{
				this.AddError(\u0002._Left, MessageId.Err_CompoAccessNoStruct, new object[]
				{
					\u0002._Left
				});
				IVariable variable = \u0002._Left.GetVariable(this.Scope);
				ISignature signature = this.Scope[\u0002._Left.SignatureId];
				if (variable != null && signature != null)
				{
					string u4 = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
					_ISourcePosition isourcePosition = variable.SourcePosition as _ISourcePosition;
					isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature.LibraryPath), signature.ObjectGuid);
					this.\u0001(\u0002, global::\u0019.\u0003.\u0001(isourcePosition, u4, Severity.Information, MessageId.Inf_RelatedPosition));
				}
			}
			else if (\u0002._Left is ICallExpression || this.\u0002(\u0002._Left))
			{
				this.\u0001(\u0002, iuserdefType);
			}
			else if (\u0002._Right.Type == null)
			{
				this.\u0002(\u0002, iuserdefType);
			}
			else
			{
				this.\u0001(\u0002);
			}
			this.\u0003(\u0002, iuserdefType);
			this.\u0001(\u0002);
		}

		// Token: 0x06002CCA RID: 11466 RVA: 0x000A176C File Offset: 0x0009F96C
		private void \u0001(_ICompoAccessExpression \u0002, IVariable \u0003)
		{
			ISignature signature = this.Scope[\u0002._Right.SignatureId];
			if (signature != null && signature != this.Scope.MethodSignature)
			{
				bool flag = false;
				for (ISignature signature2 = this.Scope.LocalSignature; signature2 != null; signature2 = this.Scope[signature2.BaseSignatureId])
				{
					if (signature == signature2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.AddError(\u0002._Right, MessageId.Err_InOutAccessOutside, new object[]
					{
						\u0003.OrgName,
						signature.Name
					});
				}
			}
		}

		// Token: 0x06002CCB RID: 11467 RVA: 0x000A17FC File Offset: 0x0009F9FC
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			\u0002._Base.Accept(this);
			if (\u0002._Base.Type == null)
			{
				return;
			}
			if (!(\u0002._Base.Type.DeRefType is _IPointerType))
			{
				if (this.TreatReferenceAsPointer)
				{
					if (!(\u0002._Base.Type is _IReferenceType))
					{
						this.AddError(\u0002._Base, MessageId.Err_DerefNoPointer, Array.Empty<object>());
					}
				}
				else
				{
					this.AddError(\u0002._Base, MessageId.Err_DerefNoPointer, Array.Empty<object>());
				}
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CCC RID: 11468 RVA: 0x000A1884 File Offset: 0x0009FA84
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			IScope5 u = (this.Scope as global::\u0007.\u0005)._CopyScope;
			this.\u0001(u);
			\u0002._Base.Accept(this);
			this.\u0003();
			if (\u0002._Base.Type == null)
			{
				\u0002._Base.ClearMessages();
				this.AddError(\u0002._Base, MessageId.Err_NoGlobalDefine, new object[]
				{
					\u0002._Base
				});
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CCD RID: 11469 RVA: 0x000A18F8 File Offset: 0x0009FAF8
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			IScope5 scope = this.Scope.GlobalScope as IScope5;
			this.\u0001(scope, false);
			\u0002._Base.Accept(this);
			this.\u0003();
			if (\u0002._Base.Type == null)
			{
				\u0002._Base.ClearMessages();
				this.AddError(\u0002._Base, MessageId.Err_NoGlobalDefine, new object[]
				{
					\u0002._Base
				});
			}
			else if (!this.TopOfStack.\u0004 && (\u0002._Base.Type is _IUserdefType || \u0002._Base.Type is _IEnumType) && global::\u0006.\u0011.\u0001(\u0002._Base.Type, scope) is _IEnumType && \u0002._Base.GetVariable(scope) == null)
			{
				this.AddError(\u0002._Base, MessageId.Err_UnexpectedTypeName, new object[]
				{
					\u0002._Base.ToString()
				});
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CCE RID: 11470 RVA: 0x000A19EC File Offset: 0x0009FBEC
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			IScope5 systemScope = this.Scope.SystemScope;
			this.\u0001(systemScope);
			\u0002._Base.Accept(this);
			this.\u0003();
			if (\u0002._Base.Type == null)
			{
				\u0002._Base.ClearMessages();
				this.AddError(\u0002._Base, MessageId.Err_NoSystemDefine, new object[]
				{
					\u0002._Base
				});
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CCF RID: 11471 RVA: 0x000A1A60 File Offset: 0x0009FC60
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			Debug.\u0001(this.Scope is _IScope);
			IScope5 poolScope = (this.Scope as _IScope).PoolScope;
			this.\u0001(poolScope);
			this.TopOfStack.\u0005 = true;
			\u0002._Base.Accept(this);
			this.\u0003();
			this.\u0001(\u0002);
		}

		// Token: 0x06002CD0 RID: 11472 RVA: 0x000A1AC0 File Offset: 0x0009FCC0
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			IScope5 scope = this.Scope.FindScope(\u0002._Namespace) as IScope5;
			if (scope == null)
			{
				this.AddError(\u0002._Namespace, MessageId.Err_NoNamespace, new object[]
				{
					\u0002._Namespace
				});
				return;
			}
			bool u = this.TopOfStack.\u0001;
			Operator u2 = this.TopOfStack.\u0001;
			bool u3 = this.TopOfStack.\u0004;
			this.\u0001(scope);
			this.TopOfStack.\u0001 = u;
			this.TopOfStack.\u0001 = u2;
			this.TopOfStack.\u0004 = u3;
			\u0002._Access.Accept(this);
			this.\u0003();
			this.\u0001(\u0002);
		}

		// Token: 0x06002CD1 RID: 11473 RVA: 0x000A1B70 File Offset: 0x0009FD70
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			IScope5 u = this.Scope.CreateLocalScope(this.\u0001["__TaskSpecificInfo"]);
			this.\u0001(u);
			\u0002._Base.Accept(this);
			this.\u0003();
			this.\u0001(\u0002);
		}

		// Token: 0x06002CD2 RID: 11474 RVA: 0x000A1BBC File Offset: 0x0009FDBC
		public void \u0001(_IEmptyStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CD3 RID: 11475 RVA: 0x000A1BC8 File Offset: 0x0009FDC8
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			\u0002._Low.Accept(this);
			\u0002._High.Accept(this);
		}

		// Token: 0x06002CD4 RID: 11476 RVA: 0x000A1BE4 File Offset: 0x0009FDE4
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CD5 RID: 11477 RVA: 0x000A1C38 File Offset: 0x0009FE38
		private bool \u0001(_IExpression \u0002, out long \u0003)
		{
			bool result = false;
			\u0003 = 0L;
			if (\u0002.Type != null)
			{
				bool flag;
				result = this.\u0001(\u0002, out \u0003, out flag);
			}
			return result;
		}

		// Token: 0x06002CD6 RID: 11478 RVA: 0x000A1C60 File Offset: 0x0009FE60
		private bool \u0001(TypeCheckerVisitor.\u0002 \u0002, bool \u0003, SortedList \u0004)
		{
			object key = \u0002.\u0001;
			object value = \u0002;
			if (!\u0003)
			{
				TypeCheckerVisitor.\u0003 u = new TypeCheckerVisitor.\u0003();
				u.\u0001 = \u0002.\u0001;
				u.\u0001 = (ulong)\u0002.\u0001;
				u.\u0002 = (ulong)\u0002.\u0002;
				u.\u0001 = \u0002.\u0001;
				key = u.\u0001;
				value = u;
			}
			if (\u0004.ContainsKey(key))
			{
				return false;
			}
			\u0004.Add(key, value);
			return true;
		}

		// Token: 0x06002CD7 RID: 11479 RVA: 0x000A1CD4 File Offset: 0x0009FED4
		public void \u0001(_ICaseStatement \u0002)
		{
			\u0002._Switch.Accept(this);
			IList<_ICase> cases = \u0002._Cases;
			foreach (_ICase icase in cases)
			{
				icase._Label.Accept(this);
				this.\u0007(true);
				icase._Controlled.Accept(this);
				this.\u0003();
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
			if (!this.\u0001(\u0002._Switch, TypeTable.AnyInt))
			{
				return;
			}
			_IExpression iexpression = \u0002._Switch;
			TypeCheckerVisitor.\u0001(this, \u0002._Switch, \u0002._Switch.Type, TypeTable.AnyInt, this.Scope, ref iexpression);
			\u0002._Switch = iexpression;
			bool flag = false;
			if (\u0002._Switch.Type != null && TypeTable.IsSigned(\u0002._Switch.Type.Class))
			{
				flag = true;
			}
			SortedList sortedList = new SortedList();
			foreach (_ICase icase2 in cases)
			{
				IList<_IExpression> cases2 = icase2._Label._cases;
				for (int i = 0; i < cases2.Count; i++)
				{
					if (icase2._Label[i].Type == null)
					{
						this.AddError(icase2._Label[i], MessageId.Err_CaseLabelNoConstant, Array.Empty<object>());
					}
					if (icase2._Label[i].Type != null)
					{
						if (icase2._Label[i] is _ICaseRangeExpression)
						{
							_ICaseRangeExpression icaseRangeExpression = icase2._Label[i] as _ICaseRangeExpression;
							if (icaseRangeExpression._Low.Type == null || icaseRangeExpression._High.Type == null || icaseRangeExpression.Type == null)
							{
								if (icaseRangeExpression._Low.Type == null)
								{
									this.AddError(icaseRangeExpression._Low, MessageId.Err_CaseLabelNoConstant, Array.Empty<object>());
								}
								if (icaseRangeExpression._High.Type == null)
								{
									this.AddError(icaseRangeExpression._Low, MessageId.Err_CaseLabelNoConstant, Array.Empty<object>());
								}
							}
							else
							{
								iexpression = icaseRangeExpression._Low;
								TypeCheckerVisitor.\u0001(this, icaseRangeExpression._Low, icaseRangeExpression._Low.Type, \u0002._Switch.Type, this.Scope, ref iexpression);
								icaseRangeExpression._Low = iexpression;
								iexpression = icaseRangeExpression._High;
								TypeCheckerVisitor.\u0001(this, icaseRangeExpression._High, icaseRangeExpression._High.Type, \u0002._Switch.Type, this.Scope, ref iexpression);
								icaseRangeExpression._High = iexpression;
								this.\u0001(icaseRangeExpression);
								TypeCheckerVisitor.\u0002 u = new TypeCheckerVisitor.\u0002();
								u.\u0001 = icaseRangeExpression;
								bool flag2 = false;
								if (!this.\u0001(icaseRangeExpression._Low, out u.\u0001))
								{
									this.AddError(icaseRangeExpression._Low, MessageId.Err_CaseLabelNoConstant, Array.Empty<object>());
									flag2 = true;
								}
								if (!this.\u0001(icaseRangeExpression._High, out u.\u0002))
								{
									this.AddError(icaseRangeExpression._High, MessageId.Err_CaseLabelNoConstant, Array.Empty<object>());
									flag2 = true;
								}
								if (!flag2 && u.\u0001 > u.\u0002)
								{
									this.AddError(icaseRangeExpression, MessageId.Err_LowerGreaterUpperBorder, Array.Empty<object>());
									flag2 = true;
								}
								if (!flag2)
								{
									u.\u0001 = true;
									if (!this.\u0001(u, flag, sortedList))
									{
										this.AddError(icaseRangeExpression._Low, MessageId.Err_CaseLabelDuplicate, Array.Empty<object>());
									}
								}
							}
						}
						else
						{
							iexpression = icase2._Label[i];
							TypeCheckerVisitor.\u0001(this, icase2._Label[i], icase2._Label[i].Type, \u0002._Switch.Type, this.Scope, ref iexpression);
							icase2._Label[i] = iexpression;
							TypeCheckerVisitor.\u0002 u2 = new TypeCheckerVisitor.\u0002();
							u2.\u0001 = icase2._Label[i];
							if (!this.\u0001(icase2._Label[i], out u2.\u0001))
							{
								this.AddError(icase2._Label[i], MessageId.Err_CaseLabelNoConstant, Array.Empty<object>());
							}
							else
							{
								u2.\u0001 = false;
								u2.\u0002 = u2.\u0001;
								if (!this.\u0001(u2, flag, sortedList))
								{
									this.AddError(icase2._Label[i], MessageId.Err_CaseLabelDuplicate, Array.Empty<object>());
								}
							}
						}
					}
				}
			}
			for (int j = 0; j < sortedList.Count - 1; j++)
			{
				if (flag)
				{
					TypeCheckerVisitor.\u0002 u3 = sortedList.GetByIndex(j) as TypeCheckerVisitor.\u0002;
					if (u3.\u0001)
					{
						TypeCheckerVisitor.\u0002 u4 = sortedList.GetByIndex(j + 1) as TypeCheckerVisitor.\u0002;
						if (u4.\u0001 <= u3.\u0002)
						{
							if (u4.\u0001)
							{
								this.AddError(u4.\u0001 as _IExprement, MessageId.Err_CaseRangesOverlapping, new object[]
								{
									u3.\u0001,
									u3.\u0002,
									u4.\u0001,
									u4.\u0002
								});
							}
							else
							{
								this.AddError(u4.\u0001 as _IExprement, MessageId.Err_CaseLabelInCaseRange, new object[]
								{
									u4.\u0001,
									u3.\u0001,
									u3.\u0002
								});
							}
						}
					}
				}
				else
				{
					TypeCheckerVisitor.\u0003 u5 = sortedList.GetByIndex(j) as TypeCheckerVisitor.\u0003;
					if (u5.\u0001)
					{
						TypeCheckerVisitor.\u0003 u6 = sortedList.GetByIndex(j + 1) as TypeCheckerVisitor.\u0003;
						if (u6.\u0001 <= u5.\u0002)
						{
							if (u6.\u0001)
							{
								this.AddError(u6.\u0001 as _IExprement, MessageId.Err_CaseRangesOverlapping, new object[]
								{
									u5.\u0001,
									u5.\u0002,
									u6.\u0001,
									u6.\u0002
								});
							}
							else
							{
								this.AddError(u6.\u0001 as _IExprement, MessageId.Err_CaseLabelInCaseRange, new object[]
								{
									u6.\u0001,
									u5.\u0001,
									u5.\u0002
								});
							}
						}
					}
				}
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CD8 RID: 11480 RVA: 0x000A239C File Offset: 0x000A059C
		public void \u0001(_IErrorExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CD9 RID: 11481 RVA: 0x000A23A8 File Offset: 0x000A05A8
		public void \u0001(_IErrorStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CDA RID: 11482 RVA: 0x000A23B4 File Offset: 0x000A05B4
		public void \u0001(_INullExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CDB RID: 11483 RVA: 0x000A23C0 File Offset: 0x000A05C0
		public void \u0001(_INullStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CDC RID: 11484 RVA: 0x000A23CC File Offset: 0x000A05CC
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			\u0002._Number.Accept(this);
			_IExpression number = \u0002._Number;
			TypeCheckerVisitor.\u0001(this, \u0002._Number, \u0002._Number.Type, TypeClass.AnyInt, this.Scope, ref number);
			\u0002._Number = number;
			\u0002._Value.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06002CDD RID: 11485 RVA: 0x000A2428 File Offset: 0x000A0628
		private void \u0001(_IArrayType \u0002, _IArrayInitialization \u0003, int \u0004)
		{
			_IExpression iexpression = \u0003._InitValues[\u0004];
			if (this.CheckingInitialValue)
			{
				iexpression = (iexpression.Duplicate() as _IExpression);
			}
			iexpression.Accept(this);
			if (\u0002 == null)
			{
				return;
			}
			_IMultipleIndexInitialization imultipleIndexInitialization = iexpression as _IMultipleIndexInitialization;
			if (imultipleIndexInitialization != null)
			{
				iexpression = imultipleIndexInitialization._Value;
				TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, \u0002._Base.DeRefType, this.Scope, ref iexpression);
				if (this.CheckingInitialValue)
				{
					imultipleIndexInitialization._Value._CompiledType = iexpression._CompiledType;
					return;
				}
				imultipleIndexInitialization._Value = iexpression;
				return;
			}
			else
			{
				TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, \u0002._Base.DeRefType, this.Scope, ref iexpression);
				if (this.CheckingInitialValue)
				{
					\u0003[\u0004]._CompiledType = iexpression._CompiledType;
					return;
				}
				\u0003[\u0004] = iexpression;
				return;
			}
		}

		// Token: 0x06002CDE RID: 11486 RVA: 0x000A24FC File Offset: 0x000A06FC
		public void \u0001(_IArrayInitialization \u0002)
		{
			_IArrayType u = \u0002.Type as _IArrayType;
			for (int i = 0; i < \u0002._InitValues.Count; i++)
			{
				this.\u0001(u, \u0002, i);
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CDF RID: 11487 RVA: 0x000A253C File Offset: 0x000A073C
		public void \u0001(_IStructureInitialization \u0002)
		{
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				Operator kindOf = iassignmentExpression.KindOf;
				if (iassignmentExpression._LValue.Type != null && iassignmentExpression._LValue.Type.Class == TypeClass.Reference)
				{
					iassignmentExpression.KindOf = Operator.RefAssign;
				}
				iassignmentExpression.Accept(this);
				iassignmentExpression.KindOf = kindOf;
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CE0 RID: 11488 RVA: 0x000A25CC File Offset: 0x000A07CC
		public void \u0001(_IDefineReference \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CE1 RID: 11489 RVA: 0x000A25D8 File Offset: 0x000A07D8
		public void \u0001(_IVariableReference \u0002)
		{
			\u0002.InstancePath.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06002CE2 RID: 11490 RVA: 0x000A25F0 File Offset: 0x000A07F0
		public void \u0001(_ITypeReference \u0002)
		{
			\u0002.InstancePath.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06002CE3 RID: 11491 RVA: 0x000A2608 File Offset: 0x000A0808
		public void \u0001(_IPouReference \u0002)
		{
			this.\u0001(this.Scope, false);
			\u0002.InstancePath.Accept(this);
			this.\u0003();
			this.\u0001(\u0002);
		}

		// Token: 0x06002CE4 RID: 11492 RVA: 0x000A2630 File Offset: 0x000A0830
		public void \u0001(_ITaskReference \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CE5 RID: 11493 RVA: 0x000A263C File Offset: 0x000A083C
		public void \u0001(_IResourceReference \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CE6 RID: 11494 RVA: 0x000A2648 File Offset: 0x000A0848
		public void \u0001(_IDefinedExpression \u0002)
		{
			\u0002.ItemReference.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06002CE7 RID: 11495 RVA: 0x000A2660 File Offset: 0x000A0860
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			foreach (_IExpression iexpression in \u0002.Operands)
			{
				iexpression.Accept(this);
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CE8 RID: 11496 RVA: 0x000A26B4 File Offset: 0x000A08B4
		public void \u0001(_IPragmaAssertion \u0002)
		{
			\u0002.Condition.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06002CE9 RID: 11497 RVA: 0x000A26CC File Offset: 0x000A08CC
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CEA RID: 11498 RVA: 0x000A26D8 File Offset: 0x000A08D8
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CEB RID: 11499 RVA: 0x000A26E4 File Offset: 0x000A08E4
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			_IDefineReference defineReference = \u0002.DefineReference;
			if (defineReference != null)
			{
				defineReference.Accept(this);
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CEC RID: 11500 RVA: 0x000A2700 File Offset: 0x000A0900
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			IList<_IPragmaElseIf> elseIf = \u0002.ElseIf;
			_IPragmaExpression ipragmaExpression = \u0002.Condition as _IPragmaExpression;
			if (ipragmaExpression == null)
			{
				return;
			}
			if (ipragmaExpression.Value)
			{
				\u0002.IfThen.Accept(this);
				return;
			}
			if (elseIf != null)
			{
				foreach (_IPragmaElseIf ipragmaElseIf in elseIf)
				{
					_IPragmaExpression ipragmaExpression2 = ipragmaElseIf.Condition as _IPragmaExpression;
					if (ipragmaExpression2 != null && ipragmaExpression2.Value)
					{
						ipragmaElseIf.Controlled.Accept(this);
						return;
					}
				}
			}
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.Accept(this);
				return;
			}
		}

		// Token: 0x06002CED RID: 11501 RVA: 0x000A27B0 File Offset: 0x000A09B0
		public void \u0001(_IBreakPointStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CEE RID: 11502 RVA: 0x000A27BC File Offset: 0x000A09BC
		public void \u0001(_IDefineStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CEF RID: 11503 RVA: 0x000A27C8 File Offset: 0x000A09C8
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
			this.\u0001(\u0002);
		}

		// Token: 0x06002CF0 RID: 11504 RVA: 0x000A27FC File Offset: 0x000A09FC
		public void \u0001(_IHasTypeExpression \u0002)
		{
			if (\u0002.Variable != null)
			{
				\u0002.Variable.Accept(this);
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CF1 RID: 11505 RVA: 0x000A281C File Offset: 0x000A0A1C
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CF2 RID: 11506 RVA: 0x000A2828 File Offset: 0x000A0A28
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			if (\u0002.ItemReference != null)
			{
				\u0002.ItemReference.Accept(this);
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06002CF3 RID: 11507 RVA: 0x000A2848 File Offset: 0x000A0A48
		public void \u0001(_IHasValueExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CF4 RID: 11508 RVA: 0x000A2854 File Offset: 0x000A0A54
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CF5 RID: 11509 RVA: 0x000A2860 File Offset: 0x000A0A60
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002CF6 RID: 11510 RVA: 0x000A286C File Offset: 0x000A0A6C
		public void \u0001(_ITryCatchStatement \u0002)
		{
			if (this.\u0001 != null)
			{
				if (this.\u0001.GetTargetSettings() != null && Helper.\u0001(this.\u0001.GetTargetSettings()) < new Version(3, 5, 6, 0))
				{
					this.AddError(\u0002, MessageId.Err_TryCatchNotSupportedVersion, Array.Empty<object>());
				}
				else if (this.\u0001.Codegenerator != null)
				{
					ICodegenerator7 codegenerator = this.\u0001.Codegenerator as ICodegenerator7;
					bool flag = this.\u0001.Codegenerator is ICodegenerator6;
					ISubroutineCodegenerator subroutineCodegenerator = this.\u0001.Codegenerator as ISubroutineCodegenerator;
					if (!flag || (codegenerator != null && !codegenerator.SupportsTryCatch) || subroutineCodegenerator == null)
					{
						this.AddError(\u0002, MessageId.Err_TryCatchNotSupportedCodegenerator, Array.Empty<object>());
					}
				}
			}
			if (\u0002._Exception != null && \u0002._Exception.Type != null)
			{
				_IExpression exception = \u0002._Exception;
				TypeCheckerVisitor.\u0001(this, \u0002._Exception, \u0002._Exception.Type.DeRefType, TypeTable.UDInt, this.Scope, ref exception);
				\u0002._Exception = exception;
			}
			\u0002.DefaultTraverse(this);
		}

		// Token: 0x06002CF7 RID: 11511 RVA: 0x000A2984 File Offset: 0x000A0B84
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			this.\u0002();
			\u0002._Left.Accept(this);
			this.\u0003();
			if (\u0002._Left._CompiledType != null)
			{
				global::\u0082.\u0007.\u0001(\u0002, \u0002._Left._CompiledType, new \u0082.\u0007.\u0001(this.AddError), (ICommonScope)this.Scope);
			}
			if (\u0002.Left is ICallExpression || this.\u0002(\u0002._Left))
			{
				this.AddError(\u0002._Left, MessageId.Err_NoBitAccessOnFunctionCall, Array.Empty<object>());
			}
		}

		// Token: 0x06002CF8 RID: 11512 RVA: 0x000A2A10 File Offset: 0x000A0C10
		public void \u0002(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression = \u0002[i];
				_IExpression value = iexpression;
				TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, TypeTable.DWord, this.Scope, ref value);
				\u0002[i] = value;
			}
			this.\u0004(\u0002);
		}

		// Token: 0x06002CF9 RID: 11513 RVA: 0x000A2A6C File Offset: 0x000A0C6C
		public void \u0003(_IOperatorExpression \u0002)
		{
			this.\u0004(\u0002);
		}

		// Token: 0x06002CFA RID: 11514 RVA: 0x000A2A78 File Offset: 0x000A0C78
		public void \u0004(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (1 != operandsList.Count)
			{
				return;
			}
			_IExpression iexpression = operandsList[0];
			if (TypeCheckerVisitor.\u0001(iexpression))
			{
				return;
			}
			_IVariable ivariable = iexpression.GetVariable(this.Scope) as _IVariable;
			bool flag = iexpression is _IDeRefAccessExpression || iexpression is _IIndexAccessExpression || iexpression is _IPartialAccessExpression;
			if (ivariable != null && ivariable.IsProperty && !flag)
			{
				this.AddError(iexpression, MessageId.Err_WrongTypeForAdr, new object[]
				{
					iexpression
				});
				return;
			}
			if ((iexpression.Type != null && iexpression.Type.Class == TypeClass.Bit) || (ivariable != null && ivariable.Address != null && ivariable.Address.Size == DirectVariableSize.X))
			{
				if (!(iexpression is IAddressExpression))
				{
					this.\u0001(iexpression, MessageId.Wrn_NoPointerToBit, Array.Empty<object>());
					return;
				}
			}
			else
			{
				if (iexpression.Type != null && iexpression.Type is ISpecialSizeType && !(iexpression.Type is _IBool16Type))
				{
					this.AddError(iexpression, MessageId.Err_WrongTypeForAdr, new object[]
					{
						iexpression
					});
					return;
				}
				if (this.\u0003(iexpression))
				{
					this.AddError(iexpression, MessageId.Err_WrongTypeForAdr, new object[]
					{
						iexpression
					});
				}
			}
		}

		// Token: 0x06002CFB RID: 11515 RVA: 0x000A2B9C File Offset: 0x000A0D9C
		private bool \u0003(_IExpression \u0002)
		{
			return TypeCheckerVisitor.\u0001(\u0002, (global::\u0017.\u0006)this.Scope, false);
		}

		// Token: 0x06002CFC RID: 11516 RVA: 0x000A2BB8 File Offset: 0x000A0DB8
		private static bool \u0001(_IExpression \u0002)
		{
			if (\u0002 is _ILiteralExpression)
			{
				_ILiteralExpression iliteralExpression = \u0002 as _ILiteralExpression;
				if (iliteralExpression.ConstantType == TypeClass.String || iliteralExpression.ConstantType == TypeClass.WString)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002CFD RID: 11517 RVA: 0x000A2BEC File Offset: 0x000A0DEC
		internal static bool \u0001(_IExpression \u0002, global::\u0017.\u0006 \u0003, bool \u0004 = false)
		{
			if (\u0003 != null && \u0003.\u0002(\u0002))
			{
				return true;
			}
			if (!(\u0002 is _IVariableExpression) && !(\u0002 is ICompoAccessExpression) && !(\u0002 is INamespaceAccessExpression))
			{
				return !(\u0002 is _IIndexAccessExpression) && !(\u0002 is _ICopyScopeExpression) && !(\u0002 is _IGlobalScopeExpression) && !(\u0002 is _IAddressExpression) && !(\u0002 is _IDeRefAccessExpression) && !(\u0002 is _ISystemScopeExpression) && !(\u0002 is _IPoolScopeExpression) && !(\u0002 is _IPartialAccessExpression);
			}
			if (\u0003 != null)
			{
				bool flag = \u0003.\u0001(\u0002) != null;
				_IVariable ivariable = \u0003.\u0001(\u0002);
				return (!flag && ivariable == null) || (ivariable != null && ivariable.IsProperty && !\u0004);
			}
			return false;
		}

		// Token: 0x06002CFE RID: 11518 RVA: 0x000A2C94 File Offset: 0x000A0E94
		public void \u0005(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002CFF RID: 11519 RVA: 0x000A2C98 File Offset: 0x000A0E98
		public void \u0006(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Debug.\u0001(operandsList.Count == 1);
			if (operandsList[0].Type == null)
			{
				return;
			}
			if (\u0002.Type.Class == TypeClass.Userdef || \u0002.Type.Class == TypeClass.Array || \u0002.Type.Class == TypeClass.String)
			{
				this.AddError(\u0002, MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					\u0002.Code,
					\u0002.Type
				});
			}
		}

		// Token: 0x06002D00 RID: 11520 RVA: 0x000A2D1C File Offset: 0x000A0F1C
		public void \u0007(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression = \u0002[i];
				_IExpression value = iexpression;
				ITargetSettings targetSettings = this.\u0001.GetTargetSettings();
				if (global::\u0016.\u0004.LRealDataType.GetBoolValue(targetSettings))
				{
					ICompiledType u = TypeTable.LReal;
					ICompiledType type = iexpression.Type;
					if (type != null && type.Class == TypeClass.Real)
					{
						string empty = string.Empty;
						TypeClass typeClass = TypeClass.None;
						Operator code = \u0002.Code;
						if (\u0002.Code == Operator.TruncInt)
						{
							\u0002.Code = Operator.Trunc;
						}
						if (this.\u0001 != null && this.\u0001.Codegenerator != null && ImplicitFunctionCallsHandler.\u0001(this.\u0001, \u0002, \u0002.Type, ref empty, ref typeClass))
						{
							u = TypeTable.Real;
						}
						\u0002.Code = code;
					}
					TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, u, this.Scope, ref value);
				}
				else
				{
					TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, TypeTable.Real, this.Scope, ref value);
				}
				\u0002[i] = value;
			}
		}

		// Token: 0x06002D01 RID: 11521 RVA: 0x000A2E30 File Offset: 0x000A1030
		public void \u0008(_IOperatorExpression \u0002)
		{
			this.\u0007(\u0002);
		}

		// Token: 0x06002D02 RID: 11522 RVA: 0x000A2E3C File Offset: 0x000A103C
		public void \u000E(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Debug.\u0001(operandsList.Count == 2);
			if (operandsList[0].Type == null || operandsList[1].Type == null)
			{
				return;
			}
			ICompiledType deRefType = operandsList[0].Type.DeRefType;
			ICompiledType deRefType2 = operandsList[1].Type.DeRefType;
			_IUserdefType iuserdefType = deRefType as _IUserdefType;
			ISignature signature = (iuserdefType == null) ? null : iuserdefType.GetSignature(this.Scope);
			if (iuserdefType == null || signature == null || (signature.POUType != Operator.FunctionBlock && signature.POUType != Operator.Type))
			{
				this.AddError(operandsList[0], MessageId.Err_IniNeedsUserdefType, Array.Empty<object>());
			}
			_IExpression value = operandsList[1];
			TypeCheckerVisitor.\u0001(this, operandsList[1], deRefType2, TypeClass.Bool, this.Scope, ref value);
			\u0002[1] = value;
		}

		// Token: 0x06002D03 RID: 11523 RVA: 0x000A2F10 File Offset: 0x000A1110
		public void \u000F(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Debug.\u0001(operandsList.Count == 1);
			if (operandsList[0].Type != null)
			{
				_IExpression iexpression = operandsList[0];
				TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type.DeRefType, TypeTable.UDInt, this.Scope, ref iexpression);
				\u0002[0] = iexpression;
			}
			this.AddError(\u0002, MessageId.Err_FeatureNotImplemented, new object[]
			{
				Scanner.GetTextOfOperator(Operator.__Throw)
			});
		}

		// Token: 0x06002D04 RID: 11524 RVA: 0x000A2F90 File Offset: 0x000A1190
		public void \u0010(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D05 RID: 11525 RVA: 0x000A2F94 File Offset: 0x000A1194
		public void \u0011(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Debug.\u0001(operandsList.Count == 1);
			if (operandsList[0].Type == null)
			{
				return;
			}
			ICompiledType deRefType = operandsList[0].Type.DeRefType;
			IVariable variable = operandsList[0].GetVariable(this.Scope);
			if (variable != null && variable.Address != null && variable.Address.Size == DirectVariableSize.X)
			{
				return;
			}
			if (deRefType.Class != TypeClass.Bit)
			{
				this.AddError(operandsList[0], MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					\u0002.Code,
					deRefType
				});
			}
			if (operandsList[0] is _ICompoAccessExpression && (operandsList[0] as _ICompoAccessExpression)._Right.Literal(this.Scope) != null)
			{
				this.AddError(operandsList[0], MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					\u0002.Code,
					deRefType
				});
			}
		}

		// Token: 0x06002D06 RID: 11526 RVA: 0x000A3088 File Offset: 0x000A1288
		public void \u0012(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D07 RID: 11527 RVA: 0x000A308C File Offset: 0x000A128C
		public void \u0013(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D08 RID: 11528 RVA: 0x000A3090 File Offset: 0x000A1290
		public void \u0014(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D09 RID: 11529 RVA: 0x000A3094 File Offset: 0x000A1294
		public void \u0015(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D0A RID: 11530 RVA: 0x000A3098 File Offset: 0x000A1298
		public void \u0016(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D0B RID: 11531 RVA: 0x000A309C File Offset: 0x000A129C
		public void \u0017(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D0C RID: 11532 RVA: 0x000A30A0 File Offset: 0x000A12A0
		public void \u0018(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count == 1 && operandsList[0].Type.Class != TypeClass.Reference)
			{
				IVariable variable = operandsList[0].GetVariable(this.Scope);
				if (variable == null || !variable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
				{
					this.AddError(operandsList[0], MessageId.Err_IsValidRefNeedsReference, Array.Empty<object>());
				}
			}
		}

		// Token: 0x06002D0D RID: 11533 RVA: 0x000A310C File Offset: 0x000A130C
		public void \u0019(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (!this.\u0001.SupportDynamicMemory)
			{
				this.AddError(\u0002, MessageId.Err_DynamicMemoryNotSupported, new object[]
				{
					APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationNameByGuid(this.\u0001.ApplicationGuid)
				});
			}
			if (operandsList.Count == 1 && operandsList[0].Type.DeRefType.Class != TypeClass.Pointer)
			{
				this.AddError(operandsList[0], MessageId.Err_DeleteNeedsPointer, Array.Empty<object>());
			}
		}

		// Token: 0x06002D0E RID: 11534 RVA: 0x000A3198 File Offset: 0x000A1398
		public void \u001A(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			ICompiledType u = null;
			ICompiledType deRefType = operandsList[0].Type.DeRefType;
			if (2 == operandsList.Count)
			{
				u = operandsList[1].Type.DeRefType;
			}
			this.\u0001(operandsList, deRefType);
			this.\u0002(operandsList, u);
		}

		// Token: 0x06002D0F RID: 11535 RVA: 0x000A31EC File Offset: 0x000A13EC
		private void \u0001(IList<_IExpression> \u0002, ICompiledType \u0003)
		{
			_IUserdefType iuserdefType = null;
			if (\u0003 is _IUserdefType)
			{
				iuserdefType = (\u0003 as _IUserdefType);
			}
			ISignature signature = null;
			if (iuserdefType != null)
			{
				signature = iuserdefType.GetSignature(this.Scope);
			}
			if (signature == null || (signature.POUType != Operator.FunctionBlock && signature.POUType != Operator.Interface && !signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion)))
			{
				this.AddError(\u0002[0], MessageId.Err_QueryPointerErrorP1, Array.Empty<object>());
				return;
			}
			ISignature signature2 = signature;
			if (signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				_IUserdefType iuserdefType2 = (signature["__Interface"] as _IVariable)._Type.BaseType as _IUserdefType;
				Debug.\u0001(iuserdefType2 != null);
				signature2 = ((iuserdefType2 != null) ? iuserdefType2.GetSignature(this.Scope) : null);
			}
			LDictionary<int, int> ldictionary = new LDictionary<int, int>();
			Helper.\u0001(this.Scope, signature2, ldictionary, true);
			ldictionary[signature2.Id] = signature2.Id;
			ISignature[] array = this.Scope.SystemScope.FindSignature("IQueryInterface");
			if (array != null && array.Length == 1 && !ldictionary.ContainsKey(array[0].Id))
			{
				this.AddError(\u0002[0], MessageId.Err_QueryInterfaceP1NoIQuery, new object[]
				{
					signature.OrgName,
					"__System.IQueryInterface"
				});
			}
		}

		// Token: 0x06002D10 RID: 11536 RVA: 0x000A3328 File Offset: 0x000A1528
		private void \u0002(IList<_IExpression> \u0002, ICompiledType \u0003)
		{
			if (\u0003 == null)
			{
				return;
			}
			if (!(\u0003 is _IPointerType))
			{
				this.AddError(\u0002[1], MessageId.Err_QueryPointerErrorP2, Array.Empty<object>());
			}
			IList<ISignature> list = this.Scope.SystemScope["__CheckedPointerCast"];
			Debug.\u0001(list != null && list.Count == 1);
		}

		// Token: 0x06002D11 RID: 11537 RVA: 0x000A3384 File Offset: 0x000A1584
		public void \u001B(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			ICompiledType deRefType = operandsList[0].Type.DeRefType;
			ICompiledType deRefType2 = operandsList[1].Type.DeRefType;
			_IUserdefType iuserdefType = null;
			if (deRefType is _IUserdefType)
			{
				iuserdefType = (deRefType as _IUserdefType);
			}
			ISignature signature = null;
			if (iuserdefType != null)
			{
				signature = iuserdefType.GetSignature(this.Scope);
			}
			if (signature == null || (signature.POUType != Operator.FunctionBlock && signature.POUType != Operator.Interface && !signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion)))
			{
				this.AddError(operandsList[0], MessageId.Err_QueryInterfaceErrorP1, Array.Empty<object>());
			}
			else
			{
				ISignature signature2 = signature;
				if (signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
				{
					_IUserdefType iuserdefType2 = (signature["__Interface"] as _IVariable)._Type.BaseType as _IUserdefType;
					Debug.\u0001(iuserdefType2 != null);
					signature2 = iuserdefType2.GetSignature(this.Scope);
				}
				LDictionary<int, int> ldictionary = new LDictionary<int, int>();
				Helper.\u0001(this.Scope, signature2, ldictionary, true);
				ldictionary[signature2.Id] = signature2.Id;
				ISignature[] array = this.Scope.SystemScope.FindSignature("IQueryInterface");
				if (array != null && array.Length == 1 && !ldictionary.ContainsKey(array[0].Id))
				{
					this.AddError(operandsList[0], MessageId.Err_QueryInterfaceP1NoIQuery, new object[]
					{
						signature.OrgName,
						"__System.IQueryInterface"
					});
				}
			}
			_IUserdefType iuserdefType3 = null;
			if (deRefType2 is _IUserdefType)
			{
				iuserdefType3 = (deRefType2 as _IUserdefType);
			}
			ISignature signature3 = null;
			if (iuserdefType3 != null)
			{
				signature3 = iuserdefType3.GetSignature(this.Scope);
			}
			if (signature3 == null || (signature3.POUType != Operator.Interface && !signature3.GetFlag(SignatureFlag.ImplicitInterfaceUnion)))
			{
				this.AddError(operandsList[1], MessageId.Err_QueryInterfaceErrorP2, Array.Empty<object>());
			}
			if (this.\u0002(operandsList[1]))
			{
				this.AddError(operandsList[1], MessageId.Err_QueryInterfaceErrorP2, Array.Empty<object>());
			}
			IList<ISignature> list = this.Scope.SystemScope["__CheckedInterfaceCast"];
			Debug.\u0001(list != null && list.Count == 1);
		}

		// Token: 0x06002D12 RID: 11538 RVA: 0x000A35AC File Offset: 0x000A17AC
		public void \u001C(_IOperatorExpression \u0002)
		{
			_IExpression u = \u0002._OperandsList[0];
			\u0002._OperandsList[0].GetVariable(this.Scope);
			_IUserdefType iuserdefType = \u0002._OperandsList[1]._CompiledType as _IUserdefType;
			if (iuserdefType != null)
			{
				iuserdefType.GetSignature(this.Scope);
			}
			if (TypeCheckerVisitor.\u0001(u, (global::\u0017.\u0006)this.Scope, true))
			{
				this.AddError(\u0002._OperandsList[0], MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					\u0002.Code,
					\u0002._OperandsList[0]._CompiledType
				});
			}
		}

		// Token: 0x06002D13 RID: 11539 RVA: 0x000A3654 File Offset: 0x000A1854
		private bool \u0001(_IOperatorExpression \u0002, out IList<_IExpression> \u0003, out IList<IVariable> \u0004)
		{
			\u0003 = \u0002._OperandsList;
			\u0004 = null;
			if (!(\u0003[0]._CompiledType is _IUserdefType))
			{
				this.AddError(\u0003[0], MessageId.Err_FCallWrongCallPatternSignature, Array.Empty<object>());
				return false;
			}
			ISignature signature = (\u0003[0]._CompiledType as _IUserdefType).GetSignature(this.Scope);
			if (signature == null)
			{
				this.AddError(\u0003[0], MessageId.Err_FCallWrongCallPatternSignature, Array.Empty<object>());
				return false;
			}
			if (!(\u0003[1]._CompiledType is _IPointerType))
			{
				this.AddError(\u0003[0], MessageId.Err_FCallExpectedPointerAsSecondArgument, Array.Empty<object>());
				return false;
			}
			_ISignature isignature = this.\u0001[signature.ParentSignatureId];
			Operator @operator = (isignature != null) ? isignature.POUType : Operator.None;
			bool flag = @operator == Operator.Program || @operator == Operator.VarGlobal;
			\u0004 = signature.AllInputs.ToList<IVariable>();
			if (flag)
			{
				\u0004.Remove(signature[IdentifierConstants.InstancePointer]);
			}
			if (\u0004.Count != \u0003.Count - 2)
			{
				this.AddError(\u0003[0], MessageId.Err_FCallWrongNumberOfArguments, new object[]
				{
					\u0004.Count
				});
				return false;
			}
			return true;
		}

		// Token: 0x06002D14 RID: 11540 RVA: 0x000A3790 File Offset: 0x000A1990
		public void \u001D(_IOperatorExpression \u0002)
		{
			IList<_IExpression> list;
			IList<IVariable> list2;
			if (!this.\u0001(\u0002, out list, out list2))
			{
				return;
			}
			for (int i = 0; i < list2.Count; i++)
			{
				_IExpression iexpression = list[i + 2];
				if (iexpression.Type != null)
				{
					this.\u0001(\u0002, i, iexpression, list2[i]);
				}
			}
		}

		// Token: 0x06002D15 RID: 11541 RVA: 0x000A37E0 File Offset: 0x000A19E0
		private void \u0001(_IOperatorExpression \u0002, int \u0003, _IExpression \u0004, IVariable \u0005)
		{
			if (this.\u0001(\u0004, \u0005))
			{
				return;
			}
			if (\u0005.GetFlag(VarFlag.Inout) && \u0005.CompiledType.DeRefType.Class != TypeClass.String && \u0005.CompiledType.DeRefType.Class != TypeClass.WString)
			{
				bool u = true;
				if (!global::\u0006.\u0011.\u0001(\u0004.Type.DeRefType, \u0005.CompiledType.DeRefType, this.Scope, this.Scope, u))
				{
					if (!TypeTable.IsBlock(\u0004.Type.DeRefType.Class) && TypeTable.IsEquivalent(\u0004.Type.DeRefType.Class, \u0005.CompiledType.DeRefType.Class))
					{
						return;
					}
					this.AddError(\u0004, MessageId.Err_InOutParamNotEqual, new object[]
					{
						\u0004.Type.DeRefType,
						\u0005.CompiledType.DeRefType,
						\u0005.OrgName
					});
				}
				return;
			}
			_IExpression value = \u0004;
			bool flag = true;
			if (\u0004.Type != null && \u0005.CompiledType != null && \u0004.Type.Class == TypeClass.Pointer && \u0005.CompiledType.Class == TypeClass.Reference && this.TreatReferenceAsPointer)
			{
				flag = false;
			}
			if (flag && !TypeCheckerVisitor.\u0001(this, \u0004, \u0004.Type, \u0005.CompiledType, this.Scope, ref value) && \u0004.Type == null)
			{
				this.AddError(\u0004, MessageId.Err_TypeMismatch, new object[]
				{
					global::\u0003.\u0006.\u0001(MessageId.Err_UnknownType, new object[]
					{
						\u0004
					}),
					\u0005.CompiledType
				});
			}
			\u0002[\u0003 + 2] = value;
		}

		// Token: 0x06002D16 RID: 11542 RVA: 0x000A397C File Offset: 0x000A1B7C
		private bool \u0001(_IExpression \u0002, IVariable \u0003)
		{
			if (\u0003.GetFlag(VarFlag.Inout) && \u0003.CompiledType.DeRefType.Class == TypeClass.Userdef && \u0002.Type.DeRefType.Class == TypeClass.Userdef)
			{
				ISignature signature = (\u0003.CompiledType.DeRefType as _IUserdefType).GetSignature(this.Scope);
				if (!global::\u0006.\u0011.\u0001((\u0002.Type.DeRefType as _IUserdefType).GetSignature(this.Scope), signature, this.Scope, this.Scope))
				{
					this.AddError(\u0002, MessageId.Err_InOutParamNotEqual, new object[]
					{
						\u0002.Type.DeRefType,
						\u0003.CompiledType.DeRefType,
						\u0003.OrgName
					});
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002D17 RID: 11543 RVA: 0x000A3A4C File Offset: 0x000A1C4C
		public void \u001E(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D18 RID: 11544 RVA: 0x000A3A50 File Offset: 0x000A1C50
		public void \u001F(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D19 RID: 11545 RVA: 0x000A3A54 File Offset: 0x000A1C54
		public void \u007F(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D1A RID: 11546 RVA: 0x000A3A58 File Offset: 0x000A1C58
		public void \u0080(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D1B RID: 11547 RVA: 0x000A3A5C File Offset: 0x000A1C5C
		public void \u0081(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D1C RID: 11548 RVA: 0x000A3A60 File Offset: 0x000A1C60
		public void \u0082(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D1D RID: 11549 RVA: 0x000A3A64 File Offset: 0x000A1C64
		public void \u0083(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Debug.\u0001(operandsList.Count == 1);
			if (operandsList[0].Type == null)
			{
				return;
			}
			if (operandsList[0].GetVariable(this.Scope) == null && operandsList[0].GetSignature(this.Scope) == null)
			{
				this.AddError(operandsList[0], MessageId.Err_UnexpectedOperandForIndexOf, new object[]
				{
					operandsList[0],
					Scanner.GetTextOfOperator(\u0002.Code)
				});
			}
		}

		// Token: 0x06002D1E RID: 11550 RVA: 0x000A3AEC File Offset: 0x000A1CEC
		public void \u0084(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Debug.\u0001(operandsList.Count == 2);
			if (operandsList[0].Type == null)
			{
				return;
			}
			if (operandsList[0].GetSignature(this.Scope) == null)
			{
				this.AddError(operandsList[0], MessageId.Err_UnexpectedOperandForCallInitFunction, new object[]
				{
					operandsList[0],
					Scanner.GetTextOfOperator(\u0002.Code)
				});
			}
			_IExpression iexpression = \u0002[1];
			_IExpression value = iexpression;
			TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, TypeClass.Bool, this.Scope, ref value);
			\u0002[1] = value;
		}

		// Token: 0x06002D1F RID: 11551 RVA: 0x000A3B8C File Offset: 0x000A1D8C
		public void \u0086(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Debug.\u0001(operandsList.Count == 1);
			if (operandsList[0].Type == null)
			{
				return;
			}
			if (!(operandsList[0] is ILiteralExpression))
			{
				this.AddError(operandsList[0], MessageId.Err_LiteralExpected, new object[]
				{
					operandsList[0]
				});
			}
			if (operandsList[0].Type.Class != TypeClass.String && operandsList[0].Type.Class != TypeClass.WString && operandsList[0].Type.Class != TypeClass.WString)
			{
				this.AddError(operandsList[0], MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					\u0002.Code,
					\u0002.Type
				});
			}
		}

		// Token: 0x06002D20 RID: 11552 RVA: 0x000A3C58 File Offset: 0x000A1E58
		public void \u0087(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count != 2)
			{
				return;
			}
			if (operandsList[0].Type == null || operandsList[1].Type == null)
			{
				return;
			}
			if (operandsList[0].Type.Class == TypeClass.Pointer)
			{
				IVariable variable = operandsList[0].GetVariable(this.Scope);
				if (variable == null || !variable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
				{
					this.AddError(operandsList[0], MessageId.Err_LowerUpperBoundOnVariableLengthArrayOnly, Array.Empty<object>());
				}
				else
				{
					IVariable variable2 = operandsList[0].GetSignatureEx(this.Scope)[variable.Name + "__ARRAY__INFO"];
					IArrayType arrayType = ((variable2 != null) ? variable2.Type : null) as IArrayType;
					if (arrayType != null)
					{
						bool flag = false;
						IArrayDimension2 arrayDimension = arrayType.Dimensions[0] as IArrayDimension2;
						int? num = (arrayDimension != null) ? new int?(arrayDimension.Range(out flag, this.Scope)) : null;
						if (num != null && flag)
						{
							this.\u0001(operandsList[1], num.Value, \u0002.Code);
						}
					}
				}
			}
			else if (TypeClass.Array == operandsList[0].Type.DeRefType.Class)
			{
				_IExpression u = operandsList[1];
				IArrayType arrayType2 = operandsList[0].Type as IArrayType;
				if (arrayType2 != null)
				{
					this.\u0001(u, arrayType2.Dimensions.Length, \u0002.Code);
				}
			}
			else
			{
				this.AddError(operandsList[0], MessageId.Err_LowerUpperBoundOnVariableLengthArrayOnly, Array.Empty<object>());
			}
			if (!(operandsList[1] is ILiteralExpression) || !TypeTable.IsInteger(operandsList[1].Type.Class))
			{
				this.AddError(operandsList[1], MessageId.Err_IntegerLiteralExpected, new object[]
				{
					operandsList[1]
				});
			}
		}

		// Token: 0x06002D21 RID: 11553 RVA: 0x000A3E38 File Offset: 0x000A2038
		private void \u0001(_IExpression \u0002, int \u0003, Operator \u0004)
		{
			if (\u0002 is ILiteralExpression && TypeTable.IsInteger(\u0002.Type.Class))
			{
				ILiteralValue2 literalValue = (_ILiteralValue)((ILiteralExpression)\u0002).LiteralValue;
				bool flag = false;
				int num = (int)literalValue.GetAnyLong(out flag);
				if (num < 1 || \u0003 < num)
				{
					if (1 == \u0003)
					{
						this.AddError(\u0002, MessageId.Err_LowerUpperBoundOperandNotExactly, new object[]
						{
							Scanner.GetTextOfOperator(\u0004)
						});
						return;
					}
					this.AddError(\u0002, MessageId.Err_LowerUpperBoundOperandNotInRange, new object[]
					{
						Scanner.GetTextOfOperator(\u0004),
						\u0003
					});
				}
			}
		}

		// Token: 0x06002D22 RID: 11554 RVA: 0x000A3EC8 File Offset: 0x000A20C8
		public void \u0088(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D23 RID: 11555 RVA: 0x000A3ECC File Offset: 0x000A20CC
		public void \u0089(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D24 RID: 11556 RVA: 0x000A3ED0 File Offset: 0x000A20D0
		public void \u008A(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D25 RID: 11557 RVA: 0x000A3ED4 File Offset: 0x000A20D4
		public void \u008B(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Debug.\u0001(operandsList.Count == 2);
			this.\u008D(\u0002);
			this.\u008C(\u0002);
			this.\u0001(\u0002, operandsList);
		}

		// Token: 0x06002D26 RID: 11558 RVA: 0x000A3F0C File Offset: 0x000A210C
		private void \u0001(_IOperatorExpression \u0002, IList<_IExpression> \u0003)
		{
			if (\u0002.Code == Operator.Shl || \u0002.Code == Operator.Shr)
			{
				ILiteralValue literalValue = \u0003[1].Literal(this.Scope);
				int num = 0;
				if (literalValue != null && literalValue.GetInt(out num) && \u0003[0].Type != null)
				{
					int num2 = \u0003[0].Type.Size(this.Scope);
					if (num >= num2 * 8)
					{
						this.\u0001(\u0002, MessageId.Wrn_ShiftExceedsTypeSize, new object[]
						{
							num,
							\u0003[0].Type.ToString()
						});
					}
				}
			}
		}

		// Token: 0x06002D27 RID: 11559 RVA: 0x000A3FAC File Offset: 0x000A21AC
		private void \u008C(_IOperatorExpression \u0002)
		{
			_IExpression iexpression = \u0002[1];
			_IExpression value = iexpression;
			TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, TypeClass.AnyInt, this.Scope, ref value);
			\u0002[1] = value;
		}

		// Token: 0x06002D28 RID: 11560 RVA: 0x000A3FE4 File Offset: 0x000A21E4
		private void \u008D(_IOperatorExpression \u0002)
		{
			_IExpression iexpression = \u0002[0];
			if (iexpression is ILiteralExpression && iexpression._CompiledType != null && TypeTable.IsInteger(iexpression._CompiledType.Class) && !TypeTable.IsConcreteType((iexpression as ILiteralExpression).ConstantType))
			{
				(iexpression as _ILiteralExpression).ConstantType = iexpression._CompiledType.Class;
			}
			_IExpression value = iexpression;
			TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, \u0002.Type.Class, this.Scope, ref value);
			\u0002[0] = value;
		}

		// Token: 0x06002D29 RID: 11561 RVA: 0x000A4070 File Offset: 0x000A2270
		public void \u008E(_IOperatorExpression \u0002)
		{
			string empty = string.Empty;
			TypeClass typeClass = TypeClass.None;
			if (this.\u0001 != null && this.\u0001.Codegenerator != null && ImplicitFunctionCallsHandler.\u0001(this.\u0001, \u0002, \u0002.Type, ref empty, ref typeClass) && Helper.\u0001(this.\u0001.GetTargetSettings()) < new Version(3, 5, 14, 0))
			{
				this.AddError(\u0002, MessageId.Err_OperatorNotSupportedVersion, new object[]
				{
					"__XADD",
					"3.5.14.0"
				});
				return;
			}
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression = \u0002[i];
				if (i == 0)
				{
					bool flag = false;
					if (iexpression.Type.Class == TypeClass.Pointer && (iexpression.Type as _IPointerType).BaseType.Class == TypeClass.DInt)
					{
						flag = true;
					}
					if (!flag)
					{
						this.AddError(iexpression, MessageId.Err_TypeMismatch, new object[]
						{
							iexpression.Type.ToString(),
							"POINTER TO DINT"
						});
					}
				}
				else
				{
					_IExpression value = iexpression;
					TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, TypeClass.DInt, this.Scope, ref value);
					\u0002[i] = value;
				}
			}
		}

		// Token: 0x06002D2A RID: 11562 RVA: 0x000A41A8 File Offset: 0x000A23A8
		public void \u008F(_IOperatorExpression \u0002)
		{
			string empty = string.Empty;
			TypeClass typeClass = TypeClass.None;
			if (this.\u0001 != null && this.\u0001.Codegenerator != null && ImplicitFunctionCallsHandler.\u0001(this.\u0001, \u0002, \u0002.Type, ref empty, ref typeClass) && Helper.\u0001(this.\u0001.GetTargetSettings()) < new Version(3, 5, 14, 0))
			{
				this.AddError(\u0002, MessageId.Err_OperatorNotSupportedVersion, new object[]
				{
					"__COMPARE_AND_SWAP",
					"3.5.14.0"
				});
				return;
			}
			int size = TypeTable.GetSize(TypeClass.Pointer, this.Scope);
			_IPointerType ipointerType = global::\u0019.\u0003.\u0001(TypeTable.DWord);
			if (size > 4)
			{
				ipointerType._Base = TypeTable.LWord;
			}
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression = \u0002[i];
				if (i == 0)
				{
					if (!iexpression.Type.IsEqual(ipointerType))
					{
						string text = "POINTER TO DWORD";
						if (size > 4)
						{
							text = "POINTER TO LWORD";
						}
						this.AddError(iexpression, MessageId.Err_TypeMismatch, new object[]
						{
							iexpression.Type.ToString(),
							text
						});
						return;
					}
				}
				else
				{
					TypeClass u = TypeClass.DWord;
					if (size > 4)
					{
						u = TypeClass.LWord;
					}
					_IExpression value = iexpression;
					TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, u, this.Scope, ref value);
					\u0002[i] = value;
				}
			}
		}

		// Token: 0x06002D2B RID: 11563 RVA: 0x000A4304 File Offset: 0x000A2504
		public void \u0090(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002D2C RID: 11564 RVA: 0x000A4308 File Offset: 0x000A2508
		public void \u0091(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression = \u0002[i];
				_IExpression value = iexpression;
				TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, \u0002.Type, this.Scope, ref value);
				\u0002[i] = value;
			}
		}

		// Token: 0x06002D2D RID: 11565 RVA: 0x000A435C File Offset: 0x000A255C
		public void \u0092(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Debug.\u0001(operandsList.Count == 2);
			if (operandsList[0].Type == null || operandsList[1].Type == null)
			{
				return;
			}
			if (operandsList[0].Type.Class == TypeClass.Enum && operandsList[1].Type.Class == TypeClass.Enum && (operandsList[0].Type as _IEnumType).SignatureId != (operandsList[1].Type as _IEnumType).SignatureId)
			{
				this.\u0001(\u0002, MessageId.Wrn_EnumComparison, new object[]
				{
					operandsList[0].Type,
					operandsList[1].Type
				});
			}
			ICompiledType compiledType = operandsList[0].Type.DeRefType;
			ICompiledType compiledType2 = operandsList[1].Type.DeRefType;
			if (this.TreatReferenceAsPointer && (OperatorService.IsEqualOp(\u0002.Code) || OperatorService.IsNotEqualOp(\u0002.Code)))
			{
				if (operandsList[0].Type.Class == TypeClass.Reference)
				{
					compiledType = global::\u0019.\u0003.\u0001(operandsList[0].Type.DeRefType as _IType);
					operandsList[0].Type = compiledType;
				}
				if (operandsList[1].Type.Class == TypeClass.Reference)
				{
					compiledType2 = global::\u0019.\u0003.\u0001(operandsList[1].Type.DeRefType as _IType);
					operandsList[1].Type = compiledType2;
				}
			}
			_IType u;
			if (TypeTable.GetSize(TypeClass.Pointer, this.Scope) == 8)
			{
				u = TypeTable.ULInt;
			}
			else
			{
				u = TypeTable.UDInt;
			}
			if (OperatorService.IsEqualOp(\u0002.Code) || OperatorService.IsNotEqualOp(\u0002.Code))
			{
				if (global::\u0006.\u0011.\u0001(compiledType, this.Scope))
				{
					if (operandsList[0] is ICallExpression)
					{
						this.AddError(operandsList[0], MessageId.Err_NoCallInInterfaceComparison, Array.Empty<object>());
					}
					int num = -1;
					if (operandsList[1].Literal(this.Scope) != null && operandsList[1].Literal(this.Scope).GetInt(out num) && num == 0)
					{
						_IExpression iexpression = operandsList[1];
						TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, u, this.Scope, ref iexpression);
						operandsList[1] = iexpression;
						return;
					}
				}
				if (global::\u0006.\u0011.\u0001(compiledType2, this.Scope))
				{
					if (operandsList[1] is ICallExpression)
					{
						this.AddError(operandsList[1], MessageId.Err_NoCallInInterfaceComparison, Array.Empty<object>());
					}
					int num2 = -1;
					if (operandsList[0].Literal(this.Scope) != null && operandsList[0].Literal(this.Scope).GetInt(out num2) && num2 == 0)
					{
						_IExpression iexpression = operandsList[0];
						TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, u, this.Scope, ref iexpression);
						operandsList[0] = iexpression;
						return;
					}
				}
			}
			if (!OperatorService.IsMinMaxOp(\u0002.Code))
			{
				if (operandsList[0].Type.DeRefType.Class == TypeClass.Pointer)
				{
					int num3 = -1;
					if (operandsList[1].Literal(this.Scope) != null && operandsList[1].Literal(this.Scope).GetInt(out num3) && num3 == 0)
					{
						_IExpression iexpression = operandsList[1];
						TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, u, this.Scope, ref iexpression);
						operandsList[1] = iexpression;
						return;
					}
				}
				if (operandsList[1].Type.DeRefType.Class == TypeClass.Pointer)
				{
					int num4 = -1;
					if (operandsList[0].Literal(this.Scope) != null && operandsList[0].Literal(this.Scope).GetInt(out num4) && num4 == 0)
					{
						_IExpression iexpression = operandsList[0];
						TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, u, this.Scope, ref iexpression);
						operandsList[0] = iexpression;
						return;
					}
				}
			}
			if (!TypeTable.IsBoolean(compiledType.Class) || !TypeTable.IsBoolean(compiledType2.Class))
			{
				if (compiledType.Class == TypeClass.Pointer && compiledType2.Class == TypeClass.Pointer)
				{
					return;
				}
				if (compiledType.Class == TypeClass.Pointer)
				{
					bool flag = false;
					if (operandsList[1] is _ILiteralExpression)
					{
						if (!global::\u0006.\u0011.\u0001(operandsList[1] as _ILiteralExpression, compiledType2, compiledType, this.Scope, this.Scope))
						{
							this.AddError(\u0002, MessageId.Err_TypesNotComparable, new object[]
							{
								compiledType,
								compiledType2
							});
							flag = true;
						}
					}
					else if (!global::\u0006.\u0011.\u0002(compiledType2, compiledType, this.Scope))
					{
						this.AddError(\u0002, MessageId.Err_TypesNotComparable, new object[]
						{
							compiledType,
							compiledType2
						});
						flag = true;
					}
					if (flag)
					{
						return;
					}
				}
				else if (compiledType2.Class == TypeClass.Pointer)
				{
					bool flag2 = false;
					if (operandsList[0] is _ILiteralExpression)
					{
						if (!global::\u0006.\u0011.\u0001(operandsList[0] as _ILiteralExpression, compiledType, compiledType2, this.Scope, this.Scope))
						{
							this.AddError(\u0002, MessageId.Err_TypesNotComparable, new object[]
							{
								compiledType2,
								compiledType
							});
							flag2 = true;
						}
					}
					else if (!global::\u0006.\u0011.\u0002(compiledType, compiledType2, this.Scope))
					{
						this.AddError(\u0002, MessageId.Err_TypesNotComparable, new object[]
						{
							compiledType,
							compiledType2
						});
						flag2 = true;
					}
					if (flag2)
					{
						return;
					}
				}
				else
				{
					if (TypeTable.IsNumber(compiledType.Class) != TypeTable.IsNumber(compiledType2.Class) || compiledType.Class == TypeClass.Bool != (compiledType2.Class == TypeClass.Bool) || compiledType.Class == TypeClass.String != (compiledType2.Class == TypeClass.String) || compiledType.Class == TypeClass.WString != (compiledType2.Class == TypeClass.WString) || (TypeTable.IsTimeOrDateType(compiledType.Class) && TypeTable.IsTimeOrDateType(compiledType2.Class) && compiledType.Class != compiledType2.Class))
					{
						this.AddError(\u0002, MessageId.Err_TypesNotComparable, new object[]
						{
							compiledType,
							compiledType2
						});
						return;
					}
					if (!TypeTable.IsNumber(compiledType.Class) && compiledType.Class != TypeClass.Bool && compiledType.Class != TypeClass.String && compiledType.Class != TypeClass.WString && !TypeTable.IsTimeOrDateType(compiledType.Class) && compiledType.Class == TypeClass.Userdef && !global::\u0006.\u0011.\u0001(compiledType, this.Scope))
					{
						this.AddError(\u0002, MessageId.Err_TypesNotComparable, new object[]
						{
							compiledType,
							compiledType2
						});
						return;
					}
					if (!TypeTable.IsNumber(compiledType2.Class) && compiledType2.Class != TypeClass.Bool && compiledType2.Class != TypeClass.String && compiledType2.Class != TypeClass.WString && !TypeTable.IsTimeOrDateType(compiledType2.Class) && compiledType2.Class == TypeClass.Userdef && !global::\u0006.\u0011.\u0001(compiledType2, this.Scope))
					{
						this.AddError(\u0002, MessageId.Err_TypesNotComparable, new object[]
						{
							compiledType,
							compiledType2
						});
						return;
					}
				}
			}
			if (TypeTable.IsInteger(compiledType.Class) && TypeTable.IsInteger(compiledType2.Class) && TypeTable.IsSigned(compiledType.Class) != TypeTable.IsSigned(compiledType2.Class) && TypeTable.GetSize(compiledType.Class, this.Scope) < 4)
			{
				TypeTable.GetSize(compiledType2.Class, this.Scope);
			}
			if (TypeTable.IsNumber(compiledType.Class) || TypeTable.IsNumber(compiledType2.Class))
			{
				if (\u0002.Code == Operator.Min || \u0002.Code == Operator.Max)
				{
					for (int i = 0; i < operandsList.Count; i++)
					{
						_IExpression iexpression = operandsList[i];
						TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, \u0002.Type, this.Scope, ref iexpression);
						\u0002[i] = iexpression;
					}
					return;
				}
				_IType u2 = global::\u001D.\u0005.\u0001(\u0002.Code, false, 0, operandsList, null, this.Scope, this.\u0001);
				for (int j = 0; j < operandsList.Count; j++)
				{
					_IExpression iexpression = operandsList[j];
					TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, u2, this.Scope, ref iexpression);
					\u0002[j] = iexpression;
				}
				return;
			}
			else
			{
				if (compiledType.IsInteger || TypeTable.IsTimeOrDateType(compiledType.Class) || compiledType.Class == TypeClass.String || compiledType.Class == TypeClass.WString || compiledType.Class == TypeClass.Bool || compiledType.Class == TypeClass.Real || compiledType.Class == TypeClass.LReal)
				{
					return;
				}
				Operator code = \u0002.Code;
				if ((code - Operator.Eq <= 1 || code - Operator.Equal <= 1) && compiledType.Class != TypeClass.Array && compiledType2.Class != TypeClass.Array)
				{
					if (compiledType.Class == TypeClass.Userdef && compiledType2.Class == TypeClass.Userdef && !global::\u0006.\u0011.\u0001(compiledType, this.Scope) && !global::\u0006.\u0011.\u0001(compiledType2, this.Scope))
					{
						_IUserdefType iuserdefType = compiledType as _IUserdefType;
						_IUserdefType iuserdefType2 = compiledType2 as _IUserdefType;
						ISignature signature = iuserdefType.GetSignature(this.Scope);
						ISignature signature2 = iuserdefType2.GetSignature(this.Scope);
						if (signature != null && signature2 != null)
						{
							if (this.\u0001.TypeIsSupported(TypeClass.Byte))
							{
								if (signature.POUType == Operator.FunctionBlock)
								{
									goto IL_927;
								}
								if (signature2.POUType == Operator.FunctionBlock)
								{
									goto IL_927;
								}
							}
							else if (signature.POUType == Operator.FunctionBlock || signature2.POUType == Operator.FunctionBlock || signature.GetFlag(SignatureFlag.Structure) || signature2.GetFlag(SignatureFlag.Structure))
							{
								goto IL_927;
							}
						}
					}
					if (!global::\u0006.\u0011.\u0001(compiledType, compiledType2, this.Scope))
					{
						this.AddError(\u0002, MessageId.Err_TypesNotComparable, new object[]
						{
							compiledType,
							compiledType2
						});
						return;
					}
					return;
				}
				IL_927:
				if (global::\u0006.\u0011.\u0001(compiledType, compiledType2, this.Scope))
				{
					this.AddError(\u0002, MessageId.Err_CompareNotPossible1, new object[]
					{
						compiledType
					});
					return;
				}
				this.AddError(\u0002, MessageId.Err_CompareNotPossible2, new object[]
				{
					compiledType,
					compiledType2
				});
				return;
			}
		}

		// Token: 0x06002D2E RID: 11566 RVA: 0x000A4CCC File Offset: 0x000A2ECC
		public void \u0093(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			_IExpression value = global::\u0019.\u0003.\u0001(Token.Empty);
			if (\u0002.Code == Operator.And_Then || \u0002.Code == Operator.Or_Else)
			{
				TypeCheckerVisitor.\u0001(this, \u0002, \u0002.Type, TypeClass.Bool, this.Scope, ref value);
			}
			else
			{
				TypeCheckerVisitor.\u0001(this, \u0002, \u0002.Type, TypeClass.AnyBit, this.Scope, ref value);
			}
			for (int i = 0; i < operandsList.Count; i++)
			{
				value = \u0002[i];
				TypeCheckerVisitor.\u0001(this, \u0002[i], \u0002[i].Type, \u0002.Type, this.Scope, ref value);
				\u0002[i] = value;
			}
		}

		// Token: 0x06002D2F RID: 11567 RVA: 0x000A4D80 File Offset: 0x000A2F80
		private bool \u0001(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Operator code = \u0002.Code;
			if (code <= Operator.Sub)
			{
				if (code != Operator.Add)
				{
					if (code != Operator.Sub)
					{
						return false;
					}
					goto IL_E2;
				}
			}
			else if (code != Operator.Plus)
			{
				if (code != Operator.Minus)
				{
					return false;
				}
				goto IL_E2;
			}
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression = operandsList[i];
				if (iexpression.Type != null && iexpression.Type.DeRefType.Class != TypeClass.Pointer)
				{
					_IExpression value = iexpression;
					bool u = this.InImplicitCode;
					if (TypeTable.IsSigned(iexpression.Type.Class))
					{
						this.InImplicitCode = true;
					}
					TypeClass u2 = TypeClass.DWord;
					if (TypeTable.GetSize(TypeClass.Pointer, this.Scope) == 8)
					{
						u2 = TypeClass.LWord;
					}
					TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, u2, this.Scope, ref value);
					this.InImplicitCode = u;
					\u0002[i] = value;
				}
			}
			return true;
			IL_E2:
			if (operandsList.Count == 2)
			{
				_IExpression iexpression2 = operandsList[1];
				if (iexpression2.Type != null)
				{
					_IExpression value = iexpression2;
					bool u3 = this.InImplicitCode;
					if (TypeTable.IsSigned(iexpression2.Type.Class))
					{
						this.InImplicitCode = true;
					}
					TypeClass u4 = TypeClass.DWord;
					if (TypeTable.GetSize(TypeClass.Pointer, this.Scope) == 8)
					{
						u4 = TypeClass.LWord;
					}
					TypeCheckerVisitor.\u0001(this, iexpression2, iexpression2.Type, u4, this.Scope, ref value);
					this.InImplicitCode = u3;
					\u0002[1] = value;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002D30 RID: 11568 RVA: 0x000A4EF4 File Offset: 0x000A30F4
		private bool \u0002(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			ICompiledType deRefType = \u0002.Type.DeRefType;
			ICompiledType deRefType2 = operandsList[0].Type.DeRefType;
			ICompiledType deRefType3 = operandsList[1].Type.DeRefType;
			ICompiledType compiledType = null;
			if (deRefType.Class == TypeClass.LTime)
			{
				compiledType = TypeTable.LInt;
			}
			else if (deRefType.Class == TypeClass.Time)
			{
				compiledType = TypeTable.DInt;
			}
			Operator code = \u0002.Code;
			switch (code)
			{
			case Operator.Add:
				break;
			case Operator.Sub:
				goto IL_AE;
			case Operator.Mul:
				goto IL_B9;
			case Operator.Div:
				goto IL_C6;
			default:
				switch (code)
				{
				case Operator.Plus:
					goto IL_A3;
				case Operator.Minus:
					goto IL_AE;
				case Operator.Times:
					goto IL_B9;
				case Operator.Divide:
					goto IL_C6;
				}
				this.AddError(\u0002, MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					\u0002.Code,
					deRefType
				});
				return false;
			}
			IL_A3:
			this.\u0002(\u0002, operandsList, deRefType);
			return true;
			IL_AE:
			this.\u0001(\u0002, operandsList, deRefType);
			return true;
			IL_B9:
			this.\u0001(\u0002, operandsList, deRefType, compiledType);
			return true;
			IL_C6:
			this.\u0001(\u0002, deRefType, deRefType2, deRefType3, compiledType);
			return true;
		}

		// Token: 0x06002D31 RID: 11569 RVA: 0x000A4FF8 File Offset: 0x000A31F8
		private void \u0001(_IOperatorExpression \u0002, ICompiledType \u0003, ICompiledType \u0004, ICompiledType \u0005, ICompiledType \u0006)
		{
			_IExpression value = \u0002[0];
			TypeCheckerVisitor.\u0001(this, \u0002[0], \u0004, \u0003, this.Scope, ref value);
			\u0002[0] = value;
			value = \u0002[1];
			TypeCheckerVisitor.\u0001(this, \u0002[1], \u0005, \u0006, this.Scope, ref value);
			\u0002[1] = value;
		}

		// Token: 0x06002D32 RID: 11570 RVA: 0x000A5058 File Offset: 0x000A3258
		private void \u0001(_IOperatorExpression \u0002, IList<_IExpression> \u0003, ICompiledType \u0004, ICompiledType \u0005)
		{
			int num = 0;
			for (int i = 0; i < \u0003.Count; i++)
			{
				if (\u0002[i].Type != null)
				{
					if (\u0002[i].Type.DeRefType.IsInteger)
					{
						bool u = this.ConvertAllTypeMismatches;
						this.ConvertAllTypeMismatches = true;
						_IExpression value = \u0002[i];
						TypeCheckerVisitor.\u0001(this, \u0002[i], \u0002[i].Type, \u0005, this.Scope, ref value);
						\u0002[i] = value;
						this.ConvertAllTypeMismatches = u;
					}
					else
					{
						_IExpression value = \u0002[i];
						bool flag = TypeCheckerVisitor.\u0001(this, \u0002[i], \u0002[i].Type, \u0004, this.Scope, ref value);
						\u0002[i] = value;
						if (flag)
						{
							num++;
						}
					}
				}
			}
			if (num > 1)
			{
				this.AddError(\u0002, MessageId.Err_CannotMultiplyMultipleTime, new object[]
				{
					\u0004
				});
			}
		}

		// Token: 0x06002D33 RID: 11571 RVA: 0x000A5144 File Offset: 0x000A3344
		private void \u0001(_IOperatorExpression \u0002, IList<_IExpression> \u0003, ICompiledType \u0004)
		{
			for (int i = 0; i < \u0003.Count; i++)
			{
				if (\u0002[i].Type.DeRefType.Class != TypeClass.Date && \u0002[i].Type.DeRefType.Class != TypeClass.DateAndTime && \u0002[i].Type.DeRefType.Class != TypeClass.TimeOfDay && \u0002[i].Type.DeRefType.Class != TypeClass.LDate && \u0002[i].Type.DeRefType.Class != TypeClass.LDateAndTime && \u0002[i].Type.DeRefType.Class != TypeClass.LTimeOfDay)
				{
					_IExpression value = \u0002[i];
					TypeCheckerVisitor.\u0001(this, \u0002[i], \u0002[i].Type, \u0004, this.Scope, ref value);
					\u0002[i] = value;
				}
			}
		}

		// Token: 0x06002D34 RID: 11572 RVA: 0x000A5240 File Offset: 0x000A3440
		private void \u0002(_IOperatorExpression \u0002, IList<_IExpression> \u0003, ICompiledType \u0004)
		{
			for (int i = 0; i < \u0003.Count; i++)
			{
				_IExpression value = \u0002[i];
				TypeCheckerVisitor.\u0001(this, \u0002[i], \u0002[i].Type, \u0004, this.Scope, ref value);
				\u0002[i] = value;
			}
		}

		// Token: 0x06002D35 RID: 11573 RVA: 0x000A5294 File Offset: 0x000A3494
		public void \u0094(_IOperatorExpression \u0002)
		{
			if (this.CheckingInitialValue)
			{
				\u0002 = (\u0002.Duplicate() as _IOperatorExpression);
			}
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (\u0002.Code == Operator.Mod && \u0002.Type != null && TypeTable.IsReal(\u0002.Type.Class))
			{
				this.AddError(\u0002, MessageId.Err_ModNotReal, Array.Empty<object>());
			}
			if (\u0002.Type == null)
			{
				return;
			}
			Debug.\u0001(operandsList.Count >= 2);
			if (operandsList[0].Type == null || operandsList[1].Type == null)
			{
				return;
			}
			this.\u0002(\u0002, operandsList);
			ICompiledType deRefType = operandsList[0].Type.DeRefType;
			ICompiledType deRefType2 = operandsList[1].Type.DeRefType;
			ICompiledType deRefType3 = \u0002.Type.DeRefType;
			bool flag = false;
			TypeClass @class = deRefType3.Class;
			_IExpression value;
			if (@class <= TypeClass.Pointer)
			{
				if (@class != TypeClass.DWord)
				{
					switch (@class)
					{
					case TypeClass.Time:
						goto IL_32D;
					case TypeClass.Date:
					case TypeClass.DateAndTime:
					case TypeClass.TimeOfDay:
						break;
					case TypeClass.Pointer:
						if (this.\u0001(\u0002))
						{
							flag = true;
							goto IL_339;
						}
						goto IL_339;
					default:
						goto IL_339;
					}
				}
				else
				{
					if (!OperatorService.IsSubtractionOp(\u0002.Code) || operandsList.Count != 2)
					{
						goto IL_339;
					}
					ICompiledType deRefType4 = operandsList[0].Type.DeRefType;
					ICompiledType deRefType5 = operandsList[1].Type.DeRefType;
					if (deRefType4.Class == TypeClass.Pointer || deRefType5.Class == TypeClass.Pointer)
					{
						value = operandsList[0];
						TypeCheckerVisitor.\u0001(this, \u0002, deRefType4, deRefType5, this.Scope, ref value);
						flag = true;
						goto IL_339;
					}
					goto IL_339;
				}
			}
			else
			{
				if (@class == TypeClass.LTime)
				{
					goto IL_32D;
				}
				if (@class - TypeClass.LDate > 2)
				{
					goto IL_339;
				}
			}
			Operator code = \u0002.Code;
			if (code <= Operator.Sub)
			{
				if (code != Operator.Add)
				{
					if (code != Operator.Sub)
					{
						goto IL_309;
					}
					goto IL_289;
				}
			}
			else if (code != Operator.Plus)
			{
				if (code != Operator.Minus)
				{
					goto IL_309;
				}
				goto IL_289;
			}
			int num = 0;
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression = operandsList[i];
				if (iexpression.Type != null && iexpression.Type.DeRefType.Class != TypeClass.Time && iexpression.Type.DeRefType.Class != TypeClass.LTime)
				{
					value = iexpression;
					bool flag2 = TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, deRefType3, this.Scope, ref value);
					\u0002[i] = value;
					if (flag2)
					{
						num++;
					}
				}
			}
			if (num > 1)
			{
				this.AddError(\u0002, MessageId.Err_CannotAddMultipleTime, new object[]
				{
					deRefType3
				});
			}
			flag = true;
			goto IL_339;
			IL_289:
			value = operandsList[0];
			TypeCheckerVisitor.\u0001(this, operandsList[0], deRefType, deRefType3, this.Scope, ref value);
			\u0002[0] = value;
			value = operandsList[1];
			if (TypeTable.IsLType(deRefType3.Class))
			{
				TypeCheckerVisitor.\u0001(this, operandsList[1], deRefType2, TypeClass.LTime, this.Scope, ref value);
			}
			else
			{
				TypeCheckerVisitor.\u0001(this, operandsList[1], deRefType2, TypeClass.Time, this.Scope, ref value);
			}
			\u0002[1] = value;
			flag = true;
			goto IL_339;
			IL_309:
			this.AddError(\u0002, MessageId.Err_OperationNotPossibleOnType, new object[]
			{
				\u0002.Code,
				deRefType3
			});
			goto IL_339;
			IL_32D:
			if (this.\u0002(\u0002))
			{
				flag = true;
			}
			IL_339:
			if (flag)
			{
				return;
			}
			value = global::\u0019.\u0003.\u0001(Token.Empty);
			if (TypeCheckerVisitor.\u0001(this, \u0002, \u0002.Type, TypeClass.AnyNum, this.Scope, ref value))
			{
				for (int j = 0; j < operandsList.Count; j++)
				{
					if (\u0002[j].Type != null)
					{
						value = \u0002[j];
						TypeCheckerVisitor.\u0001(this, \u0002[j], \u0002[j].Type.DeRefType, deRefType3, this.Scope, ref value);
						\u0002[j] = value;
					}
				}
			}
			if (\u0002.Code == Operator.Div || \u0002.Code == Operator.Divide)
			{
				ILiteralValue literalValue = \u0002[1].Literal(this.Scope, true);
				if (literalValue != null)
				{
					bool flag3;
					if (literalValue.GetInt(out flag3) == 0 && flag3)
					{
						this.AddError(\u0002[1], MessageId.Err_DivisionByZero, Array.Empty<object>());
						return;
					}
					if (literalValue.GetFloat(out flag3) == 0.0 && flag3)
					{
						this.AddError(\u0002[1], MessageId.Err_DivisionByZero, Array.Empty<object>());
					}
				}
			}
		}

		// Token: 0x06002D36 RID: 11574 RVA: 0x000A56F0 File Offset: 0x000A38F0
		private void \u0002(_IOperatorExpression \u0002, IList<_IExpression> \u0003)
		{
			foreach (IType type in \u0003.Select(new Func<_IExpression, ICompiledType>(TypeCheckerVisitor.<>c.<>9.\u0001)))
			{
				if (TypeClass.Enum == type.Class)
				{
					ISignature signature = this.Scope.FindSignature(type as IEnumType);
					if (signature != null && signature.HasAttribute("strict") && (!this.CheckingInitialValue || this.InitialValueContextSignature == null || this.InitialValueContextSignature.Id != signature.Id))
					{
						this.AddError(\u0002, MessageId.Err_StrictEnumNoArithmeticAllowed, new object[]
						{
							signature.OrgName
						});
						break;
					}
				}
			}
		}

		// Token: 0x06002D37 RID: 11575 RVA: 0x000A57C4 File Offset: 0x000A39C4
		public void \u0095(_IOperatorExpression \u0002)
		{
			switch (\u0002.Code)
			{
			case Operator.__vcAdd:
			case Operator.__vcSub:
			case Operator.__vcMul:
			case Operator.__vcDiv:
			case Operator.__vcMin:
			case Operator.__vcMax:
				this.\u009A(\u0002);
				return;
			case Operator.__vcDot:
				this.\u009B(\u0002);
				return;
			case Operator.__vcSqrt:
				this.\u0097(\u0002);
				return;
			case Operator.__vcSetReal:
			case Operator.__vcSetLReal:
				this.\u0096(\u0002);
				return;
			case Operator.__vcLoadReal:
			case Operator.__vcLoadLReal:
				this.\u0099(\u0002);
				return;
			case Operator.__vcStore:
				this.\u0098(\u0002);
				return;
			default:
				return;
			}
		}

		// Token: 0x06002D38 RID: 11576 RVA: 0x000A5848 File Offset: 0x000A3A48
		private void \u0096(_IOperatorExpression \u0002)
		{
			_IType u = TypeTable.Get((\u0002.Code == Operator.__vcSetLReal) ? Operator.LReal : Operator.Real);
			for (int i = 0; i < \u0002._OperandsList.Count; i++)
			{
				_IExpression iexpression = \u0002[i];
				TypeCheckerVisitor.\u0001(this, iexpression, iexpression.Type, u, this.Scope, ref iexpression);
				\u0002[i] = iexpression;
			}
		}

		// Token: 0x06002D39 RID: 11577 RVA: 0x000A58AC File Offset: 0x000A3AAC
		private void \u0097(_IOperatorExpression \u0002)
		{
			if (\u0002[0].Type.DeRefType.Class != TypeClass.__Vector)
			{
				this.AddError(\u0002, MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					\u0002.Code,
					\u0002[0].Type
				});
			}
		}

		// Token: 0x06002D3A RID: 11578 RVA: 0x000A5900 File Offset: 0x000A3B00
		private void \u0098(_IOperatorExpression \u0002)
		{
			TypeClass tc = TypeClass.Any;
			ICompiledType type = \u0002[1].Type;
			if (type.Class != TypeClass.__Vector)
			{
				this.AddError(\u0002, MessageId.Err_TypeMismatch, new object[]
				{
					type,
					"__VECTOR"
				});
			}
			else
			{
				tc = type.BaseType.Class;
			}
			_IExpression value = \u0002[0];
			TypeCheckerVisitor.\u0001(this, \u0002[0], \u0002[0].Type, global::\u0019.\u0003.\u0001(TypeTable.Get(tc)), this.Scope, ref value);
			\u0002[0] = value;
		}

		// Token: 0x06002D3B RID: 11579 RVA: 0x000A5990 File Offset: 0x000A3B90
		private void \u0099(_IOperatorExpression \u0002)
		{
			_IType u = TypeTable.Get((\u0002.Code == Operator.__vcLoadLReal) ? Operator.LReal : Operator.Real);
			if (!TypeTable.IsInteger(\u0002[0].Type.Class))
			{
				this.AddError(\u0002[0], MessageId.Err_VectorSizeNotValid, new object[]
				{
					\u0002.Code
				});
			}
			else
			{
				bool flag;
				int num = global::\u0084.\u0004.\u0001(\u0002[0], this.Scope, out flag);
				if (!flag)
				{
					this.AddError(\u0002, MessageId.Err_VectorSizeIsNoConstant, new object[]
					{
						\u0002.Code
					});
				}
				else if (num <= 0 || num > 8)
				{
					this.AddError(\u0002, MessageId.Err_VectorSizeNotValid, new object[]
					{
						\u0002.Code
					});
				}
			}
			_IExpression value = \u0002[1];
			TypeCheckerVisitor.\u0001(this, \u0002[1], \u0002[1].Type, global::\u0019.\u0003.\u0001(u), this.Scope, ref value);
			\u0002[1] = value;
		}

		// Token: 0x06002D3C RID: 11580 RVA: 0x000A5A90 File Offset: 0x000A3C90
		private void \u009A(_IOperatorExpression \u0002)
		{
			ICompiledType deRefType = \u0002[0]._CompiledType.DeRefType;
			ICompiledType deRefType2 = \u0002[1]._CompiledType.DeRefType;
			if (\u0002.Code == Operator.__vcMul)
			{
				if (TypeTable.IsNumber(deRefType.Class) && deRefType2.Class == TypeClass.__Vector)
				{
					_IExpression value = \u0002[0];
					TypeCheckerVisitor.\u0001(this, \u0002[0], deRefType, deRefType2.BaseType, this.Scope, ref value);
					\u0002[0] = value;
					return;
				}
				if (deRefType.Class == TypeClass.__Vector && TypeTable.IsNumber(deRefType2.Class))
				{
					_IExpression value2 = \u0002[1];
					TypeCheckerVisitor.\u0001(this, \u0002[1], deRefType2, deRefType.BaseType, this.Scope, ref value2);
					\u0002[1] = value2;
					return;
				}
			}
			if (\u0002.Code == Operator.__vcDiv && deRefType.Class == TypeClass.__Vector && TypeTable.IsNumber(deRefType2.Class))
			{
				_IExpression value3 = \u0002[1];
				TypeCheckerVisitor.\u0001(this, \u0002[1], deRefType2, deRefType.BaseType, this.Scope, ref value3);
				\u0002[1] = value3;
				return;
			}
			if (!(deRefType as _IType).IsEqual(deRefType2, this.Scope))
			{
				this.AddError(\u0002[0], MessageId.Err_VectorTypesNotCompatible, new object[]
				{
					deRefType,
					deRefType2
				});
			}
		}

		// Token: 0x06002D3D RID: 11581 RVA: 0x000A5BE4 File Offset: 0x000A3DE4
		private void \u009B(_IOperatorExpression \u0002)
		{
			ICompiledType deRefType = \u0002[0].Type.DeRefType;
			ICompiledType deRefType2 = \u0002[1].Type.DeRefType;
			if (!(deRefType as _IType).IsEqual(deRefType2, this.Scope))
			{
				this.AddError(\u0002, MessageId.Err_VectorTypesNotCompatible, new object[]
				{
					deRefType,
					deRefType2
				});
			}
		}

		// Token: 0x06002D3E RID: 11582 RVA: 0x000A5C44 File Offset: 0x000A3E44
		public void \u009C(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			switch (\u0002.Code)
			{
			case Operator.Limit:
				Debug.\u0001(operandsList.Count == 3);
				if (\u0002.Type.Class == TypeClass.Userdef || \u0002.Type.Class == TypeClass.Array)
				{
					this.AddError(\u0002, MessageId.Err_OperationNotPossibleOnType, new object[]
					{
						\u0002.Code,
						\u0002.Type
					});
				}
				this.\u009D(\u0002);
				return;
			case Operator.Min:
			case Operator.Max:
				this.\u0092(\u0002);
				return;
			case Operator.Trunc:
				return;
			case Operator.Mux:
			{
				Debug.\u0001(operandsList.Count >= 3);
				_IExpression value = \u0002[0];
				TypeCheckerVisitor.\u0001(this, operandsList[0], operandsList[0].Type, TypeClass.AnyInt, this.Scope, ref value);
				\u0002[0] = value;
				for (int i = 1; i < operandsList.Count; i++)
				{
					value = \u0002[i];
					TypeCheckerVisitor.\u0001(this, \u0002[i], \u0002[i].Type, \u0002.Type, this.Scope, ref value);
					\u0002[i] = value;
				}
				return;
			}
			case Operator.Sel:
			{
				Debug.\u0001(operandsList.Count == 3);
				_IExpression value = \u0002[0];
				TypeCheckerVisitor.\u0001(this, \u0002[0], \u0002[0].Type, TypeClass.Bool, this.Scope, ref value);
				\u0002[0] = value;
				for (int j = 1; j < operandsList.Count; j++)
				{
					value = \u0002[j];
					TypeCheckerVisitor.\u0001(this, \u0002[j], \u0002[j].Type, \u0002.Type, this.Scope, ref value);
					\u0002[j] = value;
				}
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x06002D3F RID: 11583 RVA: 0x000A5E08 File Offset: 0x000A4008
		private void \u009D(_IOperatorExpression \u0002)
		{
			if (\u0002._OperandsList.Any(new Func<_IExpression, bool>(TypeCheckerVisitor.<>c.<>9.\u0001)))
			{
				return;
			}
			for (int i = 0; i < 3; i++)
			{
				if (\u0002[i].Type == null)
				{
					return;
				}
				ICompiledType u = \u0002[i].Type.DeRefType;
				if (\u0002[i].Type is IEnumType)
				{
					u = \u0002[i].Type;
				}
				_IExpression value = \u0002[i];
				TypeCheckerVisitor.\u0001(this, \u0002[i], u, \u0002.Type, this.Scope, ref value);
				\u0002[i] = value;
			}
		}

		// Token: 0x06002D40 RID: 11584 RVA: 0x000A5EBC File Offset: 0x000A40BC
		public void \u009E(_IOperatorExpression \u0002)
		{
			_IExpression value = \u0002[0];
			TypeCheckerVisitor.\u0001(this, \u0002[0], \u0002[0].Type, TypeClass.DWord, this.Scope, ref value);
			\u0002[0] = value;
			value = \u0002[1];
			TypeCheckerVisitor.\u0001(this, \u0002[1], \u0002[1].Type, TypeClass.Bool, this.Scope, ref value);
			\u0002[1] = value;
		}

		// Token: 0x06002D41 RID: 11585 RVA: 0x000A5F30 File Offset: 0x000A4130
		public void \u009F(_IOperatorExpression \u0002)
		{
			this.\u009E(\u0002);
			_IExpression value = \u0002[2];
			TypeCheckerVisitor.\u0001(this, \u0002[2], \u0002[2].Type, TypeClass.USInt, this.Scope, ref value);
			\u0002[2] = value;
		}

		// Token: 0x06002D43 RID: 11587 RVA: 0x000A5FE0 File Offset: 0x000A41E0
		[CompilerGenerated]
		private void \u0002(_ISignature \u0002)
		{
			_ICompiledPOU u = this.\u0001;
			if (u != null)
			{
				u.SetFlagInternal(InternalCompiledPOUFlags.ContainsCallWithOmittedOptionalInput, true);
			}
			_ISignature u2 = this.\u0001;
			if (u2 == null)
			{
				return;
			}
			u2.SetFlagInternal(SignatureFlagInternal.ContainsCallWithOmittedOptionalInput, true);
		}

		// Token: 0x04000864 RID: 2148
		private readonly Stack<TypeCheckerVisitor.ETypeStackContent> \u0001 = new Stack<TypeCheckerVisitor.ETypeStackContent>();

		// Token: 0x04000865 RID: 2149
		private readonly ObjectPool<TypeCheckerVisitor.ETypeStackContent> \u0001;

		// Token: 0x04000866 RID: 2150
		private _ICompileContext \u0001;

		// Token: 0x04000867 RID: 2151
		private bool \u0001;

		// Token: 0x04000868 RID: 2152
		private bool \u0002;

		// Token: 0x04000869 RID: 2153
		private _ISignature \u0001;

		// Token: 0x0400086A RID: 2154
		private _ICompiledPOU \u0001;

		// Token: 0x0400086B RID: 2155
		private HashSet<Operator> \u0001;

		// Token: 0x0400086C RID: 2156
		[CompilerGenerated]
		private \u001F.\u0007 \u0001;

		// Token: 0x0400086D RID: 2157
		[CompilerGenerated]
		private bool \u0003;

		// Token: 0x0400086E RID: 2158
		[CompilerGenerated]
		private bool \u0004;

		// Token: 0x0400086F RID: 2159
		[CompilerGenerated]
		private bool \u0005;

		// Token: 0x04000870 RID: 2160
		[CompilerGenerated]
		private ISignature \u0001;

		// Token: 0x04000871 RID: 2161
		private static Dictionary<MessageId, MessageId> \u0001 = new Dictionary<MessageId, MessageId>
		{
			{
				MessageId.Err_CallOfPrivateProperty,
				MessageId.Wrn_CallOfPrivateProperty
			},
			{
				MessageId.Err_AccessToInternalProperty,
				MessageId.Wrn_AccessToInternalProperty
			},
			{
				MessageId.Err_CallOfProtectedProperty,
				MessageId.Wrn_CallOfProtectedProperty
			},
			{
				MessageId.Err_AccessToInternalVariable,
				MessageId.Wrn_AccessToInternalVariable
			},
			{
				MessageId.Err_AccessToInternalObject,
				MessageId.Wrn_AccessToInternalObject
			}
		};

		// Token: 0x020002E2 RID: 738
		[Flags]
		private enum \u0001 : sbyte
		{
			// Token: 0x04000873 RID: 2163
			\u0001 = 0,
			// Token: 0x04000874 RID: 2164
			\u0002 = 1,
			// Token: 0x04000875 RID: 2165
			\u0003 = 2,
			// Token: 0x04000876 RID: 2166
			\u0004 = 4
		}

		// Token: 0x020002E3 RID: 739
		private sealed class ETypeStackContent : \u001F.\u0001
		{
			// Token: 0x06002D44 RID: 11588 RVA: 0x000A6010 File Offset: 0x000A4210
			internal ETypeStackContent()
			{
				this.\u0001();
			}

			// Token: 0x06002D45 RID: 11589 RVA: 0x000A6020 File Offset: 0x000A4220
			public void \u0001()
			{
				this.\u0001 = null;
				this.\u0001 = true;
				this.\u0002 = false;
				this.\u0003 = false;
				this.\u0004 = false;
				this.\u0006 = false;
				this.\u0001 = Operator.None;
			}

			// Token: 0x04000877 RID: 2167
			public IScope5 \u0001;

			// Token: 0x04000878 RID: 2168
			public bool \u0001;

			// Token: 0x04000879 RID: 2169
			public bool \u0002;

			// Token: 0x0400087A RID: 2170
			public bool \u0003;

			// Token: 0x0400087B RID: 2171
			public bool \u0004;

			// Token: 0x0400087C RID: 2172
			public bool \u0005;

			// Token: 0x0400087D RID: 2173
			public bool \u0006;

			// Token: 0x0400087E RID: 2174
			public Operator \u0001;

			// Token: 0x0400087F RID: 2175
			public AccessFlag \u0001;

			// Token: 0x04000880 RID: 2176
			public TypeCheckerVisitor.\u0001 \u0001;
		}

		// Token: 0x020002E4 RID: 740
		private sealed class \u0002
		{
			// Token: 0x04000881 RID: 2177
			public long \u0001;

			// Token: 0x04000882 RID: 2178
			public long \u0002;

			// Token: 0x04000883 RID: 2179
			public bool \u0001;

			// Token: 0x04000884 RID: 2180
			public IExpression \u0001;
		}

		// Token: 0x020002E5 RID: 741
		private sealed class \u0003
		{
			// Token: 0x04000885 RID: 2181
			public ulong \u0001;

			// Token: 0x04000886 RID: 2182
			public ulong \u0002;

			// Token: 0x04000887 RID: 2183
			public bool \u0001;

			// Token: 0x04000888 RID: 2184
			public IExpression \u0001;
		}

		// Token: 0x020002E7 RID: 743
		[CompilerGenerated]
		private sealed class \u0004
		{
			// Token: 0x06002D52 RID: 11602 RVA: 0x000A60D0 File Offset: 0x000A42D0
			internal bool \u0001(ISignature \u0002)
			{
				return \u0002.Id != this.\u0001.Id;
			}

			// Token: 0x04000891 RID: 2193
			public ISignature \u0001;
		}
	}
}
