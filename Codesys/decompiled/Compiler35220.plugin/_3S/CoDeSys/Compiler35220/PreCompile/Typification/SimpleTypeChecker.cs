using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0004;
using \u0006;
using \u000E;
using \u0010;
using \u0012;
using \u0013;
using \u0014;
using \u0015;
using \u0017;
using \u0019;
using \u001A;
using \u001D;
using \u001E;
using \u001F;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Compiler35220.TreeConversion;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u007F;
using \u0080;
using \u0081;
using \u0082;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification
{
	// Token: 0x020001AD RID: 429
	internal class SimpleTypeChecker : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2, IOperatorExpressionVisitor5, IOperatorExpressionVisitor4, IOperatorExpressionVisitor3, IOperatorExpressionVisitor2, IOperatorExpressionVisitor, IPrecompileChecker3, IPrecompileChecker2, IPrecompileChecker, global::\u0017.\u0010
	{
		// Token: 0x06001EC1 RID: 7873 RVA: 0x000632D0 File Offset: 0x000614D0
		internal SimpleTypeChecker(_ISignature signature, int nProjectHandle, _IPreCompileContext precomLocal, bool bTrackCrossReferences)
		{
			this.\u0001 = signature;
			this.\u0001 = ((signature != null) ? signature.ObjectGuid : Guid.Empty);
			this.\u0001 = nProjectHandle;
			this.\u0001 = precomLocal;
			this.\u0001(CheckerScope.\u0001(this.PointerSize, this.\u0001._GetLibraryTable(), this.\u0001, this.\u0001, APEnvironmentFacade.Instance.LanguageModelMgr.Pool), AccessFlag.Unknown);
			this.\u0001 = bTrackCrossReferences;
		}

		// Token: 0x170005E6 RID: 1510
		// (get) Token: 0x06001EC2 RID: 7874 RVA: 0x0006337C File Offset: 0x0006157C
		protected int PointerSize
		{
			get
			{
				if (this.\u0001 == null)
				{
					return 4;
				}
				return this.\u0001.PointerSize;
			}
		}

		// Token: 0x170005E7 RID: 1511
		// (get) Token: 0x06001EC3 RID: 7875 RVA: 0x00063394 File Offset: 0x00061594
		public IType DerivedType
		{
			get
			{
				if (this.\u0001.Count == 0)
				{
					return null;
				}
				return this.TopOfStack.typeResolved;
			}
		}

		// Token: 0x170005E8 RID: 1512
		// (get) Token: 0x06001EC4 RID: 7876 RVA: 0x000633B0 File Offset: 0x000615B0
		public IVariable DerivedVariable
		{
			get
			{
				if (this.\u0001.Count == 0)
				{
					return null;
				}
				return this.TopOfStack.\u0001;
			}
		}

		// Token: 0x170005E9 RID: 1513
		// (get) Token: 0x06001EC5 RID: 7877 RVA: 0x000633CC File Offset: 0x000615CC
		public IPrecompileScope SearchScope
		{
			get
			{
				if (this.\u0001.Count == 0)
				{
					return null;
				}
				if (this.TopOfStack.\u0005 != null)
				{
					return this.TopOfStack.\u0005;
				}
				return this.TopOfStack.\u0002;
			}
		}

		// Token: 0x170005EA RID: 1514
		// (get) Token: 0x06001EC6 RID: 7878 RVA: 0x00063404 File Offset: 0x00061604
		public ISignature DerivedSignature
		{
			get
			{
				if (this.\u0001.Count == 0)
				{
					return null;
				}
				return this.TopOfStack.\u0001;
			}
		}

		// Token: 0x170005EB RID: 1515
		// (get) Token: 0x06001EC7 RID: 7879 RVA: 0x00063420 File Offset: 0x00061620
		public IPrecompileScope DerivedScope
		{
			get
			{
				if (this.\u0001.Count == 0)
				{
					return null;
				}
				return this.TopOfStack.\u0004;
			}
		}

		// Token: 0x170005EC RID: 1516
		// (get) Token: 0x06001EC8 RID: 7880 RVA: 0x0006343C File Offset: 0x0006163C
		public IList<_ICompilerMessage> Messages
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x06001EC9 RID: 7881 RVA: 0x00063444 File Offset: 0x00061644
		private void \u0001(_ICompilerMessage \u0002)
		{
			global::\u0015.\u0004 u = new global::\u0015.\u0004(\u0002);
			if (!this.\u0001.Contains(u))
			{
				this.\u0001.Add(\u0002);
				this.\u0001.Add(u);
			}
		}

		// Token: 0x06001ECA RID: 7882 RVA: 0x00063480 File Offset: 0x00061680
		private static bool \u0001(string \u0002)
		{
			return SimpleTypeChecker.\u0001.Contains(\u0002);
		}

		// Token: 0x06001ECB RID: 7883 RVA: 0x00063490 File Offset: 0x00061690
		private static bool \u0001(IExpression \u0002)
		{
			return \u0002 != null && SimpleTypeChecker.\u0001(\u0002.ToString());
		}

		// Token: 0x06001ECC RID: 7884 RVA: 0x000634A4 File Offset: 0x000616A4
		private void \u0001(_ISignature \u0002)
		{
			IPreCompileContext preCompileContext;
			_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(\u0002.ParentObjectGuid, out preCompileContext) as _ISignature;
			if (isignature != null)
			{
				\u0002.PrecompileParentId = isignature.PrecompileId;
			}
		}

		// Token: 0x06001ECD RID: 7885 RVA: 0x000634E0 File Offset: 0x000616E0
		public void \u0002(_ISignature \u0002)
		{
			this.\u0001(CheckerScope.\u0001(this.PointerSize, this.\u0001._GetLibraryTable(), \u0002, this.\u0001, APEnvironmentFacade.Instance.LanguageModelMgr.Pool), AccessFlag.Read);
			if (\u0002.POUType == Operator.Method || \u0002.POUType == Operator.Action)
			{
				this.\u0001(\u0002);
			}
			this.\u0003(\u0002);
			foreach (_IExpression u in \u0002.InterfaceExpressions.OfType<_IExpression>())
			{
				this.\u0001(u);
				_ISignature u2 = this.TopOfStack.\u0001;
				if (u2 != null)
				{
					\u0002.AddPrecompileInterfaceId(u2.PrecompileId);
				}
			}
			foreach (_IVariable u3 in \u0002.AllVariables)
			{
				try
				{
					this.\u0001(u3);
					this.\u0003(u3);
				}
				finally
				{
					this.\u0002(u3);
				}
			}
			SimpleTypeChecker.\u0002(\u0002);
			SimpleTypeChecker.\u0001(\u0002);
			global::\u007F.\u0010.\u0001(\u0002, this.\u0001, \u0002, new global::\u000E.\u0016(this.\u0003), new global::\u0012.\u0013(this.\u0002));
			this.\u0002();
		}

		// Token: 0x06001ECE RID: 7886 RVA: 0x00063638 File Offset: 0x00061838
		private static void \u0001(_ISignature \u0002)
		{
			if (!\u0002.GetFlag(SignatureFlag.Abstract))
			{
				return;
			}
			_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0002.PrecompileParentId) as _ISignature;
			if (isignature == null)
			{
				return;
			}
			SimpleTypeChecker.\u0001(\u0002, isignature);
		}

		// Token: 0x06001ECF RID: 7887 RVA: 0x00063680 File Offset: 0x00061880
		private static void \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			if (\u0002.GetFlag(SignatureFlag.Abstract) && \u0003 != null && \u0003.POUType == Operator.FunctionBlock && !\u0003.GetFlag(SignatureFlag.Abstract))
			{
				\u0003.AddMessage(Severity.Error, MessageId.Err_AbstractMethodOnlyInAbstractFunctionblock, new object[]
				{
					\u0002.OrgName,
					\u0003.OrgName
				});
			}
		}

		// Token: 0x06001ED0 RID: 7888 RVA: 0x000636E0 File Offset: 0x000618E0
		private void \u0001(_IVariable \u0002)
		{
			foreach (string text in \u0002.Attributes)
			{
				if (text.StartsWith("suppress_warning_"))
				{
					this.\u0001(\u0002.GetAttributeValue(text));
				}
			}
		}

		// Token: 0x06001ED1 RID: 7889 RVA: 0x00063720 File Offset: 0x00061920
		private void \u0002(_IVariable \u0002)
		{
			foreach (string text in \u0002.Attributes)
			{
				if (text.StartsWith("suppress_warning_"))
				{
					this.\u0002(\u0002.GetAttributeValue(text));
				}
			}
		}

		// Token: 0x06001ED2 RID: 7890 RVA: 0x00063760 File Offset: 0x00061960
		private static void \u0002(_ISignature \u0002)
		{
			if (\u0002.GetFlag(SignatureFlag.Enum) && \u0002.All.Length != 0)
			{
				_IEnumType ienumType = \u0002.All[0].Type as _IEnumType;
				if (ienumType != null && ienumType._DefaultValue != null)
				{
					SimpleTypeChecker.\u0002 u = new SimpleTypeChecker.\u0002();
					u.\u0001 = ienumType._DefaultValue.ToString().ToUpperInvariant();
					_IVariable ivariable = \u0002.AllVariables.FirstOrDefault(new Func<_IVariable, bool>(u.\u0001));
					if (ivariable != null)
					{
						_IVariableExpression defaultValue = ienumType._DefaultValue;
						if (defaultValue != null)
						{
							defaultValue.PrecompileSignatureId = \u0002.PrecompileId;
							defaultValue.PrecompileVariableId = ivariable.PrecompileId;
						}
					}
				}
			}
		}

		// Token: 0x06001ED3 RID: 7891 RVA: 0x000637FC File Offset: 0x000619FC
		private void \u0003(_ISignature \u0002)
		{
			if (\u0002._BaseSignature != null)
			{
				_ISignature isignature = global::\u001E.\u0006.\u0001(this.Scope, \u0002._BaseSignature);
				if (isignature != null)
				{
					\u0002.PrecompileBaseSignatureId = isignature.PrecompileId;
					this.\u0001(\u0002._BaseSignature, isignature);
					ITypeExpression typeExpression = \u0002._BaseSignature as ITypeExpression;
					if (typeExpression != null)
					{
						this.\u0001(null, (_IType)typeExpression.ExpressionType, isignature);
						return;
					}
				}
				else
				{
					string text = (\u0002._BaseSignature == null) ? string.Empty : \u0002._BaseSignature.ToString();
					this.AddError(\u0002._BaseSignature, MessageId.Err_BaseClassNotFound, new object[]
					{
						text
					});
				}
			}
		}

		// Token: 0x06001ED4 RID: 7892 RVA: 0x00063898 File Offset: 0x00061A98
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Number.Accept(this);
			this.\u0001(\u0002._Number, \u0002._Number, this.TopOfStack.typeResolved, TypeTable.AnyInt);
			this.\u0002();
			\u0002._Value.Accept(this);
		}

		// Token: 0x06001ED5 RID: 7893 RVA: 0x000638F0 File Offset: 0x00061AF0
		public void \u0001(_IArrayInitialization \u0002)
		{
			_IArrayType iarrayType = this.TopOfStack.\u0001 as _IArrayType;
			foreach (_IExpression iexpression in \u0002._InitValues)
			{
				this.\u0001(AccessFlag.Read);
				if (iarrayType != null)
				{
					this.TopOfStack.\u0001 = iarrayType._Base;
				}
				iexpression.Accept(this);
				_IType u = this.TopOfStack.typeResolved;
				this.\u0002();
				if (iarrayType != null)
				{
					this.\u0001(iexpression, iexpression, u, iarrayType._Base);
				}
			}
		}

		// Token: 0x06001ED6 RID: 7894 RVA: 0x00063990 File Offset: 0x00061B90
		public void \u0001(_IStructureInitialization \u0002)
		{
			global::\u0015.\u0002 u = null;
			global::\u0015.\u0002 u2 = null;
			_IUserdefType iuserdefType = this.TopOfStack.\u0001 as _IUserdefType;
			if (iuserdefType != null)
			{
				_ISignature isignature;
				if (this.TopOfStack.\u0003 != null)
				{
					u2 = this.TopOfStack.\u0003.\u0001(iuserdefType.NameExpression);
					isignature = (this.TopOfStack.\u0003.FindSignature(iuserdefType) as _ISignature);
				}
				else
				{
					u2 = this.Scope.\u0001(iuserdefType.NameExpression);
					isignature = (this.Scope.FindSignature(iuserdefType) as _ISignature);
				}
				if (isignature != null && !string.IsNullOrEmpty(isignature.LibraryPath))
				{
					u = this.Scope.\u0002(isignature);
				}
			}
			_IType itype = null;
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				if (u2 != null)
				{
					this.\u0001(u2, AccessFlag.Write);
					iassignmentExpression._LValue.Accept(this);
					itype = this.TopOfStack.typeResolved;
					this.\u0002();
				}
				this.\u0001(AccessFlag.Read);
				this.TopOfStack.\u0003 = u;
				this.TopOfStack.\u0001 = itype;
				iassignmentExpression._RValue.Accept(this);
				_IType u3 = this.TopOfStack.typeResolved;
				this.\u0002();
				this.\u0001(iassignmentExpression, iassignmentExpression._RValue, iassignmentExpression._LValue, u3, itype);
			}
		}

		// Token: 0x06001ED7 RID: 7895 RVA: 0x00063B08 File Offset: 0x00061D08
		public void \u0003(_IVariable \u0002)
		{
			this.\u0001(\u0002, \u0002._Type);
			_IUserdefType udtype;
			if (SimpleTypeChecker.\u0001(\u0002, out udtype) && this.\u0001)
			{
				_ISignature isignature = this.Scope.FindSignature(udtype) as _ISignature;
				global::\u0015.\u0002 u = this.Scope.\u0001(isignature);
				_ISignature isignature2 = this.\u0001(\u0002, u, isignature);
				global::\u0015.\u0002 u2;
				int u3;
				if (isignature2 != null)
				{
					u2 = this.Scope.\u0001(isignature2);
					u3 = Helper.\u0001(isignature2).Count;
				}
				else
				{
					u2 = null;
					u3 = 0;
				}
				this.\u0001(\u0002, isignature2, u2, u3);
			}
			this.\u0004(\u0002);
		}

		// Token: 0x06001ED8 RID: 7896 RVA: 0x00063B98 File Offset: 0x00061D98
		private static bool \u0001(_IVariable \u0002, out _IUserdefType \u0003)
		{
			return \u0002._Type.HasUnderlyingUserdefType(out \u0003) && \u0003.SignatureId != -1 && \u0002.InputAssignments != null && \u0002.InputAssignments.Any<IAssignmentExpression>();
		}

		// Token: 0x06001ED9 RID: 7897 RVA: 0x00063BC8 File Offset: 0x00061DC8
		private _ISignature \u0001(_IVariable \u0002, global::\u0015.\u0002 \u0003, _ISignature \u0004)
		{
			_ISignature isignature = (_ISignature)((\u0003 != null) ? \u0003.FindSignatureLocal(IdentifierConstants.InitMethodName) : null);
			if (isignature != null)
			{
				IList<ICompiledType> list = new List<ICompiledType>();
				foreach (_IAssignmentExpression iassignmentExpression in \u0002.InputAssignments.OfType<_IAssignmentExpression>())
				{
					if (iassignmentExpression._RValue != null)
					{
						this.\u0001(AccessFlag.Read);
						this.TopOfStack.\u0001 = TypeTable.Any;
						iassignmentExpression._RValue.Accept(this);
						list.Add(this.TopOfStack.typeResolved);
						this.\u0002();
					}
				}
				IList<_ISignature> list2 = global::\u001F.\u0010.\u0001(this.Scope, this.\u0001, \u0002.InputAssignments, list, isignature, (_ISignature4)\u0004);
				global::\u001F.\u0010.\u0001(\u0002, isignature.Name, list2, new global::\u000E.\u0016(this.\u0001), new global::\u0012.\u0013(this.\u0001));
				isignature = list2.FirstOrDefault<_ISignature>();
			}
			return isignature;
		}

		// Token: 0x06001EDA RID: 7898 RVA: 0x00063CCC File Offset: 0x00061ECC
		private void \u0001(_IVariable \u0002, _ISignature \u0003, global::\u0015.\u0002 \u0004, int \u0005)
		{
			int u = 2;
			foreach (_IAssignmentExpression u2 in \u0002.InputAssignments.Cast<_IAssignmentExpression>())
			{
				u = this.\u0001(\u0003, \u0004, \u0005, u, u2);
			}
		}

		// Token: 0x06001EDB RID: 7899 RVA: 0x00063D28 File Offset: 0x00061F28
		private int \u0001(_ISignature \u0002, global::\u0015.\u0002 \u0003, int \u0004, int \u0005, _IAssignmentExpression \u0006)
		{
			ICompiledType compiledType = null;
			global::\u0015.\u0002 u = null;
			IVariable variable = null;
			if (\u0003 != null && \u0004 > 0)
			{
				variable = \u0006._LValue.GetVariable(\u0003);
				if (\u0006._LValue == null || \u0006._LValue is INullExpression)
				{
					compiledType = (\u0002.AllInputs[\u0005].Type as ICompiledType);
					u = this.Scope;
				}
				else
				{
					this.\u0001(\u0003, AccessFlag.Write);
					\u0006._LValue.Accept(this);
					compiledType = this.TopOfStack.typeResolved;
					u = (this.TopOfStack.\u0004 ?? this.Scope);
					this.\u0002();
				}
				\u0005++;
				\u0005 = (\u0005 + 1) % \u0004;
			}
			ICompiledType compiledType2 = null;
			global::\u0015.\u0002 u2 = null;
			if (\u0006._RValue != null)
			{
				this.\u0001(AccessFlag.Read);
				this.TopOfStack.\u0001 = TypeTable.Any;
				\u0006._RValue.Accept(this);
				compiledType2 = this.TopOfStack.typeResolved;
				u2 = (this.TopOfStack.\u0004 ?? this.Scope);
				this.\u0002();
			}
			if (variable != null && !variable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY) && compiledType != null && compiledType2 != null && u != null && u2 != null)
			{
				bool flag;
				this.\u0001(\u0006._RValue, \u0006._RValue, \u0006._LValue, compiledType2, compiledType, u2, u, false, out flag);
			}
			return \u0005;
		}

		// Token: 0x06001EDC RID: 7900 RVA: 0x00063E74 File Offset: 0x00062074
		private void \u0004(_IVariable \u0002)
		{
			if (\u0002._Initial == null)
			{
				return;
			}
			_IExpression initial = \u0002._Initial;
			this.\u0001(AccessFlag.Read);
			this.TopOfStack.\u0001 = \u0002._Type;
			this.\u0001(initial);
			_IType itype = this.TopOfStack.typeResolved;
			this.\u0002();
			if (itype != null && \u0002._Type != null && !TypeTable.IsBlock(\u0002._Type.Class) && !this.\u0001(\u0002))
			{
				this.\u0001(initial, initial, itype, \u0002._Type);
			}
			_IExpression u = global::\u0017.\u000E.\u0001(initial, this.\u0001);
			this.\u0001(\u0002, \u0002._Type, u);
			IVariableWithCompactedInitialValue variableWithCompactedInitialValue = \u0002 as IVariableWithCompactedInitialValue;
			if (((variableWithCompactedInitialValue != null) ? variableWithCompactedInitialValue.CompactedInitialValueInformation : null) != null)
			{
				ICompactedParseTreeInformation compactedParseTreeInformation = global::\u0019.\u0003.Builder.CreateCompactedParseTreeInformation();
				PrecompileParseTreeInformationCollector.CollectParseTreeInformation(initial, compactedParseTreeInformation);
				variableWithCompactedInitialValue.CompactedInitialValueInformation = compactedParseTreeInformation;
			}
		}

		// Token: 0x06001EDD RID: 7901 RVA: 0x00063F44 File Offset: 0x00062144
		private bool \u0001(_IVariable \u0002)
		{
			if (\u0002 != null && \u0002.Initial != null && \u0002.Type != null && \u0002.Type.Class == TypeClass.Reference)
			{
				ILiteralValue literalValue = this.Scope.GetLiteralValue(\u0002.Initial, true);
				int num;
				if (literalValue != null && literalValue.GetInt(out num) && num == 0)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001EDE RID: 7902 RVA: 0x00063F9C File Offset: 0x0006219C
		private void \u0001(_IVariable \u0002, _IType \u0003, _IExpression \u0004)
		{
			bool flag = global::\u0013.\u0004.\u0001((_IExprement3)\u0004, true);
			if (\u0002.HasFlag(VarFlag.ReplacedConstant) && !flag)
			{
				this.AddError(\u0004, MessageId.Err_NoConstantInitialisationForValue, new object[]
				{
					\u0002.OrgName
				});
				return;
			}
			if (\u0002.HasFlag(VarFlag.Constant) && !TypeTable.IsBlock(\u0003.EffectiveType.Class) && !flag)
			{
				this.AddError(\u0004, MessageId.Err_NoConstantInitialisationForValue, new object[]
				{
					\u0002.OrgName
				});
			}
		}

		// Token: 0x06001EDF RID: 7903 RVA: 0x0006401C File Offset: 0x0006221C
		private void \u0001(ISignature \u0002, _IVariable \u0003, _IType \u0004)
		{
			global::\u0015.\u0002 u;
			if (\u0003.HasFlag(VarFlag.Output) && !\u0002.Name.Equals(\u0003.Name, StringComparison.InvariantCultureIgnoreCase) && this.Scope.\u0001(\u0004, out u).Class == TypeClass.Reference)
			{
				this.\u0001(\u0003, MessageId.Err_NoReferenceToOutput, Array.Empty<object>());
			}
		}

		// Token: 0x06001EE0 RID: 7904 RVA: 0x00064070 File Offset: 0x00062270
		public void \u0001(_IVariable \u0002, _IType \u0003)
		{
			if (\u0003 == null)
			{
				return;
			}
			this.\u0001(this.\u0001, \u0002, \u0003);
			this.\u0005(\u0002);
			TypeClass @class = \u0003.Class;
			switch (@class)
			{
			case TypeClass.String:
			{
				_IStringType istringType = \u0003 as _IStringType;
				this.\u0001(istringType.Length);
				return;
			}
			case TypeClass.WString:
			{
				_IWStringType iwstringType = \u0003 as _IWStringType;
				this.\u0001(iwstringType.Length);
				return;
			}
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
			case TypeClass.Params:
				break;
			case TypeClass.Pointer:
			{
				_IPointerType ipointerType = \u0003 as _IPointerType;
				if (ipointerType._Base.Class == TypeClass.Userdef && this.\u0001(ipointerType._Base as _IUserdefType))
				{
					this.\u0001(\u0002, MessageId.Err_NoPointerToBit, Array.Empty<object>());
				}
				this.\u0001(\u0002, ipointerType._Base);
				return;
			}
			case TypeClass.Reference:
			{
				_IReferenceType ireferenceType = \u0003 as _IReferenceType;
				if (\u0002.GetFlag(VarFlag.Inout))
				{
					this.\u0001(\u0002, MessageId.Err_ReferenceNotAllowedForVarInOut, Array.Empty<object>());
				}
				if (this.\u0001(ireferenceType._Base as _IUserdefType))
				{
					this.\u0001(\u0002, MessageId.Err_NoReferenceToBits, Array.Empty<object>());
				}
				this.\u0001(\u0002, ireferenceType._Base);
				return;
			}
			case TypeClass.Subrange:
			{
				_ISubrangeType isubrangeType = \u0003 as _ISubrangeType;
				this.\u0001(isubrangeType._LowerBorder);
				this.\u0001(isubrangeType._UpperBorder);
				return;
			}
			case TypeClass.Enum:
			{
				_IEnumType ienumType = \u0003 as _IEnumType;
				_ISignature isignature = this.\u0001(ienumType);
				if (isignature == null)
				{
					this.\u0001(\u0002, MessageId.Err_IdentNotDefined, new object[]
					{
						ienumType.Name.ToString()
					});
					return;
				}
				ienumType.SignatureId = isignature.PrecompileId;
				this.\u0004(isignature);
				break;
			}
			case TypeClass.Array:
			{
				_IArrayType iarrayType = \u0003 as _IArrayType;
				foreach (_IArrayDimension iarrayDimension in iarrayType._Dimensions)
				{
					this.\u0001(iarrayDimension._LowerBorder);
					this.\u0001(iarrayDimension._UpperBorder);
				}
				if (iarrayType._Base.Class == TypeClass.Userdef && this.\u0001(iarrayType._Base as _IUserdefType))
				{
					this.\u0001(\u0002, MessageId.Err_NoArrayOfBit, Array.Empty<object>());
				}
				this.\u0001(\u0002, iarrayType._Base);
				return;
			}
			case TypeClass.Userdef:
			{
				_IUserdefType iuserdefType = (_IUserdefType)\u0003;
				_ISignature isignature2 = global::\u001E.\u0006.\u0001(this.Scope, iuserdefType);
				if (isignature2 == null)
				{
					this.\u0001(\u0002, MessageId.Err_IdentNotDefined, new object[]
					{
						iuserdefType.NameExpression.ToString()
					});
					return;
				}
				iuserdefType.SignatureId = isignature2.PrecompileId;
				this.\u0001(iuserdefType.NameExpression, isignature2);
				this.\u0001(\u0002, \u0003, isignature2);
				return;
			}
			default:
			{
				if (@class != TypeClass.__Vector)
				{
					return;
				}
				_IVectorType ivectorType = \u0003 as _IVectorType;
				this.\u0001((_IExpression)ivectorType.Dimension);
				return;
			}
			}
		}

		// Token: 0x06001EE1 RID: 7905 RVA: 0x00064338 File Offset: 0x00062538
		private void \u0001(_IVariable \u0002, _IType \u0003, _ISignature \u0004)
		{
			IGenericUserdefType genericUserdefType = \u0003 as IGenericUserdefType;
			if (genericUserdefType != null)
			{
				GenericTypeChecker genericTypeChecker = new GenericTypeChecker(new Action<_IExpression, string, MessageId>(this.\u0001));
				genericTypeChecker.\u0001(genericUserdefType, \u0004, \u0002);
				genericTypeChecker.\u0001(genericUserdefType);
				_IVariable[] array = \u0004.AllVariables.Where(new Func<_IVariable, bool>(SimpleTypeChecker.<>c.<>9.\u0001)).ToArray<_IVariable>();
				genericTypeChecker.\u0001(array.ToArray<_IVariable>(), genericUserdefType);
				int num = 0;
				foreach (_IExpression u in genericUserdefType.GenericConstantsInitializations)
				{
					this.\u0001(\u0004, u, array, num, genericTypeChecker);
					num++;
				}
				if (!\u0004.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants))
				{
					this.\u0001(\u0002, MessageId.Err_TypeIsNotGeneric, new object[]
					{
						\u0004.OrgName
					});
				}
			}
		}

		// Token: 0x06001EE2 RID: 7906 RVA: 0x00064430 File Offset: 0x00062630
		private void \u0001(_ISignature \u0002, _IExpression \u0003, _IVariable[] \u0004, int \u0005, GenericTypeChecker \u0006)
		{
			IVariable variable = null;
			_IExpression iexpression = \u0003;
			_IAssignmentExpression iassignmentExpression = \u0003 as _IAssignmentExpression;
			if (iassignmentExpression != null)
			{
				this.\u0001(this.Scope.\u0001(\u0002), AccessFlag.Write);
				iassignmentExpression._LValue.Accept(this);
				this.\u0002();
				iexpression = iassignmentExpression._RValue;
				variable = \u0002[iassignmentExpression._LValue.ToString()];
			}
			else if (\u0004.Length > \u0005)
			{
				variable = \u0004[\u0005];
			}
			this.\u0001(AccessFlag.Read);
			iexpression.Accept(this);
			_IType u = this.TopOfStack.typeResolved;
			this.\u0002();
			\u0006.\u0001(iexpression, variable, (global::\u0017.\u0006)this.Scope);
			if (variable != null)
			{
				this.\u0001(iexpression, iexpression, u, (ICompiledType)variable.Type);
			}
		}

		// Token: 0x06001EE3 RID: 7907 RVA: 0x000644E4 File Offset: 0x000626E4
		private bool \u0001(_IUserdefType \u0002)
		{
			_ISignature isignature = this.\u0001(\u0002);
			if (isignature != null && isignature.GetFlag(SignatureFlag.Alias) && isignature.AllVariables.Count == 1)
			{
				_IVariable ivariable = isignature.AllVariables[0];
				IType type = (ivariable != null) ? ivariable.Type : null;
				if (type != null && type.Class == TypeClass.Bit)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001EE4 RID: 7908 RVA: 0x0006453C File Offset: 0x0006273C
		protected bool \u0001(_IType \u0002, out _IArrayType \u0003)
		{
			\u0003 = null;
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			if (iuserdefType != null)
			{
				_ISignature isignature = this.\u0001(iuserdefType);
				if (isignature != null && isignature.GetFlag(SignatureFlag.Alias))
				{
					_IVariable ivariable = isignature.AllVariables[0];
					IType type = (ivariable != null) ? ivariable.Type : null;
					if (type != null && type.Class == TypeClass.Array)
					{
						\u0003 = (type as _IArrayType);
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001EE5 RID: 7909 RVA: 0x0006459C File Offset: 0x0006279C
		public void \u0001(_IExpression \u0002)
		{
			if (\u0002 != null)
			{
				\u0002.Accept(this);
			}
		}

		// Token: 0x06001EE6 RID: 7910 RVA: 0x000645A8 File Offset: 0x000627A8
		public void \u0001(_ICompiledPOU \u0002)
		{
			_ISignature isignature = this.\u0001[\u0002.ObjectGuid];
			_IStatement parseTree = \u0002.GetParseTree();
			bool flag;
			global::\u0084.\u0016.\u0001(parseTree as _ISequenceStatement, out flag);
			this.\u0001(CheckerScope.\u0001(this.PointerSize, this.\u0001._GetLibraryTable(), isignature, this.\u0001, APEnvironmentFacade.Instance.LanguageModelMgr.Pool), AccessFlag.Unknown);
			global::\u0014.\u0013.\u0001(this.\u0001, isignature, \u0002);
			parseTree.Accept(this);
			ICompiledPOUWithCompactedParseTree compiledPOUWithCompactedParseTree = \u0002 as ICompiledPOUWithCompactedParseTree;
			if (((compiledPOUWithCompactedParseTree != null) ? compiledPOUWithCompactedParseTree.CompactedParseTreeInformation : null) != null)
			{
				ICompactedParseTreeInformation compactedParseTreeInformation = global::\u0019.\u0003.Builder.CreateCompactedParseTreeInformation();
				PrecompileParseTreeInformationCollector.CollectParseTreeInformation(parseTree, compactedParseTreeInformation);
				compiledPOUWithCompactedParseTree.CompactedParseTreeInformation = compactedParseTreeInformation;
			}
			this.\u0002();
		}

		// Token: 0x06001EE7 RID: 7911 RVA: 0x0006465C File Offset: 0x0006285C
		public void \u0001(_IStatement \u0002, _ISignature \u0003, out IEnumerable<IMessage> \u0004)
		{
			bool flag;
			global::\u0084.\u0016.\u0001(\u0002 as _ISequenceStatement, out flag);
			this.\u0001(CheckerScope.\u0001(this.PointerSize, this.\u0001._GetLibraryTable(), \u0003, this.\u0001, APEnvironmentFacade.Instance.LanguageModelMgr.Pool), AccessFlag.Unknown);
			\u0002.Accept(this);
			this.\u0002();
			\u0004 = this.\u0001;
		}

		// Token: 0x170005ED RID: 1517
		// (get) Token: 0x06001EE8 RID: 7912 RVA: 0x000646C0 File Offset: 0x000628C0
		protected global::\u0010.\u0003 TopOfStack
		{
			get
			{
				if (this.\u0001.Count > 0)
				{
					return this.\u0001.Peek();
				}
				return new global::\u0010.\u0003();
			}
		}

		// Token: 0x170005EE RID: 1518
		// (get) Token: 0x06001EE9 RID: 7913 RVA: 0x000646E4 File Offset: 0x000628E4
		protected global::\u0015.\u0002 Scope
		{
			get
			{
				return this.TopOfStack.\u0002;
			}
		}

		// Token: 0x170005EF RID: 1519
		// (get) Token: 0x06001EEA RID: 7914 RVA: 0x000646F4 File Offset: 0x000628F4
		internal global::\u0004.\u0006 OperatorInfo
		{
			get
			{
				return this.TopOfStack.\u0001;
			}
		}

		// Token: 0x06001EEB RID: 7915 RVA: 0x00064704 File Offset: 0x00062904
		protected void \u0001(global::\u0015.\u0002 \u0002, AccessFlag \u0003)
		{
			this.\u0001.Push(new global::\u0010.\u0003
			{
				\u0002 = \u0002,
				\u0003 = this.TopOfStack.\u0003,
				\u0001 = \u0003,
				\u0001 = this.TopOfStack.\u0001,
				\u0001 = this.TopOfStack.\u0001,
				\u0002 = this.TopOfStack.\u0002,
				\u0003 = this.TopOfStack.\u0003,
				\u0004 = this.TopOfStack.\u0004
			});
		}

		// Token: 0x06001EEC RID: 7916 RVA: 0x00064798 File Offset: 0x00062998
		protected void \u0001(AccessFlag \u0002)
		{
			this.\u0001(this.Scope, \u0002);
		}

		// Token: 0x06001EED RID: 7917 RVA: 0x000647A8 File Offset: 0x000629A8
		protected void \u0001(IgnoreCheckerMessageFlags \u0002)
		{
			this.\u0001.Push(new global::\u0010.\u0003
			{
				\u0002 = this.TopOfStack.\u0002,
				\u0001 = this.TopOfStack.\u0001,
				\u0001 = this.TopOfStack.\u0001,
				\u0001 = \u0002,
				\u0002 = this.TopOfStack.\u0002,
				\u0003 = this.TopOfStack.\u0003,
				\u0004 = this.TopOfStack.\u0004
			});
		}

		// Token: 0x06001EEE RID: 7918 RVA: 0x00064834 File Offset: 0x00062A34
		protected void \u0001(bool \u0002)
		{
			this.\u0001.Push(new global::\u0010.\u0003
			{
				\u0002 = this.TopOfStack.\u0002,
				\u0001 = this.TopOfStack.\u0001,
				\u0001 = \u0002,
				\u0001 = this.TopOfStack.\u0001,
				\u0002 = this.TopOfStack.\u0002,
				\u0003 = this.TopOfStack.\u0003,
				\u0004 = this.TopOfStack.\u0004
			});
		}

		// Token: 0x06001EEF RID: 7919 RVA: 0x000648C0 File Offset: 0x00062AC0
		protected void \u0001()
		{
			this.\u0001(this.Scope, this.TopOfStack.\u0001);
		}

		// Token: 0x06001EF0 RID: 7920 RVA: 0x000648DC File Offset: 0x00062ADC
		protected void \u0002()
		{
			this.\u0001.Pop();
		}

		// Token: 0x06001EF1 RID: 7921 RVA: 0x000648EC File Offset: 0x00062AEC
		private _ISignature \u0001(IUserdefType \u0002)
		{
			return this.Scope.FindSignature(\u0002) as _ISignature;
		}

		// Token: 0x06001EF2 RID: 7922 RVA: 0x00064900 File Offset: 0x00062B00
		private _ISignature \u0001(IEnumType \u0002)
		{
			return this.Scope.FindSignature(\u0002) as _ISignature;
		}

		// Token: 0x06001EF3 RID: 7923 RVA: 0x00064914 File Offset: 0x00062B14
		public _ISignature \u0001(string \u0002)
		{
			ISignature[] array = this.Scope.\u0001(\u0002);
			if (array.Length == 1)
			{
				return array[0] as _ISignature;
			}
			return null;
		}

		// Token: 0x06001EF4 RID: 7924 RVA: 0x00064940 File Offset: 0x00062B40
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Condition.Accept(this);
			_IType u = this.TopOfStack.typeResolved;
			this.\u0002();
			this.\u0001(\u0002, \u0002._Condition, u, TypeTable.Bool);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06001EF5 RID: 7925 RVA: 0x00064994 File Offset: 0x00062B94
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
			this.\u0001(AccessFlag.Read);
			\u0002._Condition.Accept(this);
			_IType u = this.TopOfStack.typeResolved;
			this.\u0002();
			this.\u0001(\u0002, \u0002._Condition, u, TypeTable.Bool);
		}

		// Token: 0x06001EF6 RID: 7926 RVA: 0x000649E8 File Offset: 0x00062BE8
		public void \u0001(_IExpression \u0002, string \u0003, MessageId \u0004)
		{
			this.\u0001(global::\u0019.\u0003.\u0001(\u0002.GetPosition(), \u0003, Severity.Error, \u0004));
		}

		// Token: 0x06001EF7 RID: 7927 RVA: 0x00064A00 File Offset: 0x00062C00
		public void \u0001(object \u0002, MessageId \u0003, params object[] \u0004)
		{
			_IVariable ivariable = \u0002 as _IVariable;
			if (ivariable != null)
			{
				this.\u0001(ivariable, \u0003, \u0004);
			}
		}

		// Token: 0x06001EF8 RID: 7928 RVA: 0x00064A20 File Offset: 0x00062C20
		public void \u0001(object \u0002, _ICompilerMessage \u0003)
		{
			this.\u0001(\u0003);
		}

		// Token: 0x06001EF9 RID: 7929 RVA: 0x00064A2C File Offset: 0x00062C2C
		public void \u0001(_IVariable \u0002, MessageId \u0003, params object[] \u0004)
		{
			if (this.TopOfStack.\u0001.HasFlag(IgnoreCheckerMessageFlags.Errors))
			{
				return;
			}
			string format = global::\u000E.\u0018.\u0001(\u0003);
			this.\u0001(global::\u0019.\u0003.\u0001(\u0002.SourcePosition, string.Format(format, \u0004), Severity.Error, \u0003));
		}

		// Token: 0x06001EFA RID: 7930 RVA: 0x00064A78 File Offset: 0x00062C78
		private void \u0002(object \u0002, MessageId \u0003, params object[] \u0004)
		{
			_IExpression iexpression = \u0002 as _IExpression;
			if (iexpression != null)
			{
				this.AddError(iexpression, \u0003, \u0004);
			}
		}

		// Token: 0x06001EFB RID: 7931 RVA: 0x00064A98 File Offset: 0x00062C98
		public void AddError(_IExprement exp, MessageId mid, params object[] args)
		{
			if (this.TopOfStack.\u0001.HasFlag(IgnoreCheckerMessageFlags.Errors))
			{
				return;
			}
			string format = global::\u000E.\u0018.\u0001(mid);
			this.\u0001(global::\u0019.\u0003.\u0001(exp.GetPosition(), string.Format(format, args), Severity.Error, mid));
		}

		// Token: 0x06001EFC RID: 7932 RVA: 0x00064AE4 File Offset: 0x00062CE4
		public void \u0001(_ISignature \u0002, MessageId \u0003, params object[] \u0004)
		{
			if (this.TopOfStack.\u0001.HasFlag(IgnoreCheckerMessageFlags.Errors))
			{
				return;
			}
			_ISourcePosition u = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.PrimaryProjectHandle, \u0002.MessageGuid, 0L, 0, 0);
			string format = global::\u000E.\u0018.\u0001(\u0003);
			this.\u0001(global::\u0019.\u0003.\u0001(u, string.Format(format, \u0004), Severity.Error, \u0003));
		}

		// Token: 0x06001EFD RID: 7933 RVA: 0x00064B48 File Offset: 0x00062D48
		private void \u0003(object \u0002, MessageId \u0003, params object[] \u0004)
		{
			_ISignature isignature = \u0002 as _ISignature;
			if (isignature != null)
			{
				this.\u0001(isignature, \u0003, \u0004);
			}
		}

		// Token: 0x06001EFE RID: 7934 RVA: 0x00064B68 File Offset: 0x00062D68
		private void \u0002(object \u0002, _ICompilerMessage \u0003)
		{
			this.\u0001(\u0003);
		}

		// Token: 0x06001EFF RID: 7935 RVA: 0x00064B74 File Offset: 0x00062D74
		private void \u0003(object \u0002, _ICompilerMessage \u0003)
		{
			this.\u0002(\u0003);
		}

		// Token: 0x06001F00 RID: 7936 RVA: 0x00064B80 File Offset: 0x00062D80
		private void \u0002(_ICompilerMessage \u0002)
		{
			if (this.TopOfStack.\u0001.HasFlag(IgnoreCheckerMessageFlags.Information))
			{
				return;
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06001F01 RID: 7937 RVA: 0x00064BA8 File Offset: 0x00062DA8
		internal void \u0001(string \u0002)
		{
			if (!this.\u0001.ContainsKey(\u0002))
			{
				this.\u0001.Add(\u0002, \u0002);
			}
		}

		// Token: 0x06001F02 RID: 7938 RVA: 0x00064BC8 File Offset: 0x00062DC8
		private void \u0002(string \u0002)
		{
			if (this.\u0001.ContainsKey(\u0002))
			{
				this.\u0001.Remove(\u0002);
			}
		}

		// Token: 0x06001F03 RID: 7939 RVA: 0x00064BE8 File Offset: 0x00062DE8
		public void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			Severity u = Severity.Warning;
			if (this.TopOfStack.\u0001.HasFlag(IgnoreCheckerMessageFlags.Warnings))
			{
				return;
			}
			string format = global::\u000E.\u0018.\u0001(\u0003);
			_ICompilerMessage u2 = this.\u0001(\u0002.GetPosition(), string.Format(format, \u0004), u, \u0003);
			this.\u0001(u2);
		}

		// Token: 0x06001F04 RID: 7940 RVA: 0x00064C3C File Offset: 0x00062E3C
		private _ICompilerMessage \u0001(ISourcePosition \u0002, string \u0003, Severity \u0004, MessageId \u0005)
		{
			_ICompilerMessage icompilerMessage = global::\u0019.\u0003.\u0001(\u0002, \u0003, \u0004, \u0005);
			if (icompilerMessage.Severity == Severity.Warning && icompilerMessage.Number != null && this.\u0001.Count > 0)
			{
				string text = string.Format("{0}{1:d4}", icompilerMessage.Prefix, icompilerMessage.Number);
				if (this.\u0001.ContainsKey(text))
				{
					icompilerMessage.Severity = Severity.SuppressedWarning;
				}
			}
			if (APEnvironmentFacade.Instance.IsWarningMessageDisabled(icompilerMessage.MessageId))
			{
				icompilerMessage.Severity = Severity.SuppressedWarning;
			}
			return icompilerMessage;
		}

		// Token: 0x06001F05 RID: 7941 RVA: 0x00064CC8 File Offset: 0x00062EC8
		private void \u0001(_IForStatement \u0002, out bool \u0003, out bool \u0004)
		{
			\u0003 = false;
			\u0004 = false;
			checked
			{
				try
				{
					if (\u0002._By != null)
					{
						ILiteralValue literalValue = \u0002._By.Literal(this.Scope);
						if (literalValue == null)
						{
							return;
						}
						bool flag;
						int @int = literalValue.GetInt(out flag);
						if (!flag)
						{
							return;
						}
						\u0004 = (@int > 0);
					}
					ILiteralValue literalValue2 = \u0002._UpperBound.Literal(this.Scope);
					if (literalValue2 != null)
					{
						if (\u0002._Counter != null)
						{
							if (\u0002._Counter._CompiledType != null)
							{
								bool flag2 = false;
								bool flag3 = false;
								ulong unsignedLong = literalValue2.GetUnsignedLong(out flag2);
								long num = 0L;
								if (!flag2)
								{
									num = literalValue2.GetSignedLong(out flag3);
								}
								if (flag3 || flag2)
								{
									if (\u0004)
									{
										ulong typeRangeHigh = TypeTable.GetTypeRangeHigh(\u0002._Counter._CompiledType.Class);
										if (flag2 && unsignedLong == typeRangeHigh)
										{
											\u0003 = true;
										}
										if (flag3 && typeRangeHigh <= 9223372036854775807UL && num == (long)typeRangeHigh)
										{
											\u0003 = true;
										}
									}
									else
									{
										long typeRangeLow = TypeTable.GetTypeRangeLow(\u0002._Counter._CompiledType.Class);
										if (flag3 && num == typeRangeLow)
										{
											\u0003 = true;
										}
										if (flag2 && typeRangeLow >= 0L && unsignedLong == (ulong)typeRangeLow)
										{
											\u0003 = true;
										}
									}
								}
							}
						}
					}
				}
				catch
				{
					\u0003 = false;
				}
			}
		}

		// Token: 0x06001F06 RID: 7942 RVA: 0x00064E14 File Offset: 0x00063014
		public void \u0001(_IForStatement \u0002)
		{
			_IAssignmentExpression iassignmentExpression = \u0002._CounterStart as _IAssignmentExpression;
			_IType itype = null;
			_IType u = null;
			_IType u2 = null;
			string u3 = string.Format("C{0:D4}", 195);
			if (iassignmentExpression != null)
			{
				this.\u0001(AccessFlag.Write);
				iassignmentExpression._LValue.Accept(this);
				itype = this.TopOfStack.typeResolved;
				this.\u0002();
				this.\u0001(AccessFlag.Read);
				iassignmentExpression._RValue.Accept(this);
				u = this.TopOfStack.typeResolved;
				this.\u0002();
			}
			if (\u0002._Condition != null)
			{
				this.\u0001(AccessFlag.Read);
				\u0002._Condition.Accept(this);
				this.\u0002();
			}
			if (\u0002._UpperBound != null)
			{
				this.\u0001(AccessFlag.Read);
				\u0002._UpperBound.Accept(this);
				u2 = this.TopOfStack.typeResolved;
				this.\u0002();
			}
			if (\u0002._Counter != null)
			{
				this.\u0001(u3);
				this.\u0001(AccessFlag.Read);
				\u0002._Counter.Accept(this);
				this.\u0002();
				this.\u0002(u3);
			}
			_IType u4 = null;
			if (\u0002._By != null)
			{
				this.\u0001(AccessFlag.Read);
				\u0002._By.Accept(this);
				u4 = this.TopOfStack.typeResolved;
				this.\u0002();
			}
			\u0002._Controlled.Accept(this);
			bool flag;
			bool flag2;
			this.\u0001(\u0002, out flag, out flag2);
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
			if (iassignmentExpression != null)
			{
				this.\u0001(iassignmentExpression._LValue, iassignmentExpression._LValue, itype, TypeTable.AnyInt);
				this.\u0001(iassignmentExpression._RValue, iassignmentExpression._RValue, \u0002._CounterStart, u, itype);
			}
			if (\u0002.UpperBound != null)
			{
				this.\u0001(\u0002._UpperBound, \u0002._UpperBound, \u0002._CounterStart, u2, itype);
			}
			if (\u0002.By != null)
			{
				this.\u0001(u3);
				this.\u0001(\u0002._By, \u0002._By, \u0002._CounterStart, u4, itype);
				this.\u0002(u3);
			}
		}

		// Token: 0x06001F07 RID: 7943 RVA: 0x00065054 File Offset: 0x00063254
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x06001F08 RID: 7944 RVA: 0x00065058 File Offset: 0x00063258
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06001F09 RID: 7945 RVA: 0x0006505C File Offset: 0x0006325C
		public void \u0001(_ISequenceStatement \u0002)
		{
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				try
				{
					if (istatement is ICaseLabelStatement && !this.TopOfStack.\u0001)
					{
						this.AddError(istatement, MessageId.Err_CaseLabelOutsideOfCase, Array.Empty<object>());
					}
					this.\u0001(false);
					istatement.Accept(this);
					this.\u0002();
				}
				catch
				{
				}
			}
		}

		// Token: 0x06001F0A RID: 7946 RVA: 0x000650F0 File Offset: 0x000632F0
		public void \u0001(_ITryCatchStatement \u0002)
		{
			if (\u0002._Exception != null && \u0002._Exception.Type != null)
			{
				_IExpression exception = \u0002._Exception;
				this.\u0001(\u0002._Exception, \u0002._Exception, \u0002._Exception.Type.DeRefType, TypeTable.UDInt);
				\u0002._Exception = exception;
			}
			\u0002.DefaultTraverse(this);
		}

		// Token: 0x06001F0B RID: 7947 RVA: 0x00065150 File Offset: 0x00063350
		private TypeClass \u0001(ICompiledType \u0002)
		{
			if (\u0002.Class == TypeClass.Reference)
			{
				_IReferenceType ireferenceType = \u0002 as _IReferenceType;
				return this.\u0001(ireferenceType.BaseType);
			}
			if (TypeClass.Userdef == \u0002.Class)
			{
				_IUserdefType iuserdefType = \u0002 as _IUserdefType;
				if (iuserdefType != null)
				{
					_ISignature isignature = this.Scope.FindSignature(iuserdefType) as _ISignature;
					if (isignature != null && isignature.GetFlag(SignatureFlag.Alias) && !this.KeepAliases)
					{
						global::\u0015.\u0002 u;
						_IType itype = this.Scope.\u0001(isignature, this.TopOfStack.typeResolved, out u);
						if (itype != null)
						{
							return this.\u0001(itype);
						}
					}
				}
			}
			return \u0002.Class;
		}

		// Token: 0x06001F0C RID: 7948 RVA: 0x000651E4 File Offset: 0x000633E4
		public virtual void \u0001(_IAssignmentExpression \u0002)
		{
			this.\u0001(this.TopOfStack.\u0001);
			this.TopOfStack.\u0001 = AccessFlag.Write;
			this.TopOfStack.opAssignment = \u0002.KindOf;
			\u0002._LValue.Accept(this);
			_IType itype = this.TopOfStack.typeResolved;
			global::\u0015.\u0002 u = this.TopOfStack.\u0005 ?? this.Scope;
			this.\u0002();
			this.\u0001(AccessFlag.Read);
			this.TopOfStack.\u0001 = itype;
			this.TopOfStack.opAssignment = \u0002.KindOf;
			\u0002._RValue.Accept(this);
			_IType itype2 = this.TopOfStack.typeResolved;
			global::\u0015.\u0002 u2 = this.TopOfStack.\u0005 ?? this.Scope;
			this.\u0002();
			if (\u0002.KindOf == Operator.Assign || \u0002.KindOf == Operator.RefAssign)
			{
				bool flag;
				this.\u0001(\u0002._LValue, \u0002._RValue, \u0002._LValue, itype2, itype, u2, u, \u0002.KindOf == Operator.RefAssign, out flag);
			}
			if (itype != null && itype2 != null && \u0002.KindOf == Operator.RefAssign && !SimpleTypeChecker.\u0001(\u0002._RValue, this.Scope) && this.\u0001(itype) != this.\u0001(itype2))
			{
				this.AddError(\u0002._RValue, MessageId.Err_TypeMismatch, new object[]
				{
					itype2.ToString(),
					itype.ToString()
				});
			}
			if (\u0002._RValue is _INewExpression && itype2 != null && itype != null && itype2.Class == TypeClass.Pointer && itype2.BaseType.Class == TypeClass.Userdef)
			{
				if (itype.Class == TypeClass.Pointer)
				{
					if (!global::\u0006.\u0011.\u0001(itype.BaseType, itype2.BaseType, this.Scope, this.Scope, false))
					{
						this.AddError(\u0002._RValue, MessageId.Err_TypeMismatch, new object[]
						{
							itype2.ToString(),
							itype.ToString()
						});
						return;
					}
				}
				else
				{
					this.AddError(\u0002._RValue, MessageId.Err_TypeMismatch, new object[]
					{
						itype2.ToString(),
						itype.ToString()
					});
				}
			}
		}

		// Token: 0x06001F0D RID: 7949 RVA: 0x000653FC File Offset: 0x000635FC
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Condition.Accept(this);
			_IType u = this.TopOfStack.typeResolved;
			this.\u0002();
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				this.\u0001(AccessFlag.Read);
				ielseIf._Condition.Accept(this);
				_IType u2 = this.TopOfStack.typeResolved;
				this.\u0002();
				ielseIf._Controlled.Accept(this);
				this.\u0001(ielseIf._Condition, ielseIf._Condition, u2, TypeTable.Bool);
			}
			if (\u0002.IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
			this.\u0001(\u0002._Condition, \u0002._Condition, u, TypeTable.Bool);
		}

		// Token: 0x06001F0E RID: 7950 RVA: 0x000654EC File Offset: 0x000636EC
		public void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001(AccessFlag.Read);
				\u0002._Condition.Accept(this);
				_IType u = this.TopOfStack.typeResolved;
				this.\u0002();
				this.\u0001(\u0002._Condition, \u0002._Condition, u, TypeTable.Bool);
			}
		}

		// Token: 0x06001F0F RID: 7951 RVA: 0x00065540 File Offset: 0x00063740
		public void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001(AccessFlag.Read);
				\u0002._Condition.Accept(this);
				_IType u = this.TopOfStack.typeResolved;
				this.\u0002();
				this.\u0001(\u0002._Condition, \u0002._Condition, u, TypeTable.Bool);
			}
		}

		// Token: 0x06001F10 RID: 7952 RVA: 0x00065594 File Offset: 0x00063794
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06001F11 RID: 7953 RVA: 0x00065598 File Offset: 0x00063798
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06001F12 RID: 7954 RVA: 0x0006559C File Offset: 0x0006379C
		public void \u0001(_IPragmaStatement \u0002)
		{
			if (\u0002 is _IWarningDisableRestorePragmaStatement)
			{
				_IWarningDisableRestorePragmaStatement iwarningDisableRestorePragmaStatement = \u0002 as _IWarningDisableRestorePragmaStatement;
				if (iwarningDisableRestorePragmaStatement.Restore)
				{
					if (this.\u0001.ContainsKey(iwarningDisableRestorePragmaStatement.Id))
					{
						this.\u0001.Remove(iwarningDisableRestorePragmaStatement.Id);
						return;
					}
				}
				else if (!this.\u0001.ContainsKey(iwarningDisableRestorePragmaStatement.Id))
				{
					this.\u0001.Add(iwarningDisableRestorePragmaStatement.Id, iwarningDisableRestorePragmaStatement.Id);
				}
			}
		}

		// Token: 0x06001F13 RID: 7955 RVA: 0x00065610 File Offset: 0x00063810
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Expr.Accept(this);
			_IType itype = this.TopOfStack.typeResolved;
			this.\u0002();
			if ((\u0002._Expr is _IVariableExpression || \u0002._Expr is _ICompoAccessExpression || \u0002._Expr is _IDeRefAccessExpression || \u0002._Expr is _IIndexAccessExpression) && itype != null && itype.Class == TypeClass.Userdef)
			{
				_IUserdefType u = itype as _IUserdefType;
				ISignature signature = this.\u0001(u);
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

		// Token: 0x06001F14 RID: 7956 RVA: 0x00065728 File Offset: 0x00063928
		public virtual void \u0001(_ICallExpression \u0002, _ISignature \u0003, int \u0004, _IVariable \u0005, _IExpression \u0006, _IType \u0007, IVariable \u0008, global::\u0015.\u0002 \u000E, global::\u0015.\u0002 \u000F)
		{
			_IExpression iexpression = (_IExpression)\u0006.Duplicate();
			if (\u0005 == null || !\u0005.HasFlag(VarFlag.Input | VarFlag.Inout))
			{
				this.AddError(\u0002.Duplicate() as _ICallExpression, MessageId.Err_IsNoInput, new object[]
				{
					iexpression,
					\u0003.Name
				});
			}
			if (\u0005 == null || \u0007 == null)
			{
				return;
			}
			if (!this.\u0001(\u0006, \u0007, \u0005._Type, \u000E, \u000E))
			{
				return;
			}
			if (\u0007.Class == TypeClass.Userdef || \u0005._Type.DeRefType.Class == TypeClass.Userdef)
			{
				return;
			}
			if (\u0005.GetFlag(VarFlag.Inout) && \u0005._Type.DeRefType.Class != TypeClass.String && \u0005._Type.DeRefType.Class != TypeClass.WString)
			{
				if (\u0005.Type.Class == TypeClass.Lazy || \u0007.Class == TypeClass.Lazy)
				{
					return;
				}
				if ((\u0005.Type as _IType).DeRefType.Class == TypeClass.Bool && \u0008 != null && \u0008.Address != null && \u0008.Address.Size == DirectVariableSize.X)
				{
					this.AddError(iexpression, MessageId.Err_InOutParamNotEqual, new object[]
					{
						Scanner.GetTextOfOperator(Operator.Bit),
						\u0005.Type,
						\u0005.OrgName
					});
				}
				global::\u0015.\u0002 u = this.Scope.\u0001(\u0003);
				if (!global::\u0006.\u0011.\u0001(\u0007, \u0005._Type, \u000E, u, true))
				{
					if (!TypeTable.IsBlock(\u0007.Class) && TypeTable.IsEquivalentTypeIncludeXTypes(\u0007.Class, \u0005._Type.DeRefType.Class, this.PointerSize))
					{
						return;
					}
					this.AddError(iexpression, MessageId.Err_InOutParamNotEqual, new object[]
					{
						\u0007,
						\u0005.Type,
						\u0005.OrgName
					});
					return;
				}
			}
			else if (\u0005.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
			{
				_IArrayType iarrayType = \u0007 as _IArrayType;
				if (iarrayType != null)
				{
					_IPointerType ipointerType = \u0005.Type as _IPointerType;
					if (ipointerType != null)
					{
						_IType @base = iarrayType._Base;
						_IType base2 = ipointerType._Base;
						if (@base.Class == TypeClass.Userdef || base2.DeRefType.Class == TypeClass.Userdef)
						{
							return;
						}
						if (!@base.IsEqualPreCompile(base2, null))
						{
							string text = \u0007.ToString();
							string text2 = string.Format("ARRAY [*] OF {0}", base2);
							this.AddError(iexpression, MessageId.Err_TypeMismatch, new object[]
							{
								text,
								text2
							});
							return;
						}
					}
				}
			}
			else
			{
				bool flag;
				this.\u0001(\u0006, \u0006, null, \u0007, \u0005._Type, \u000E ?? this.Scope, \u000F ?? this.Scope, false, out flag);
			}
		}

		// Token: 0x06001F15 RID: 7957 RVA: 0x000659C8 File Offset: 0x00063BC8
		private bool \u0001(_IVariable \u0002, _IVariable \u0003)
		{
			return \u0003 == null || \u0002 == null || (!this.\u0003(\u0003) && !\u0003.HasFlag(VarFlag.ReplacedConstant) && (!\u0003.HasFlag(VarFlag.Constant) || \u0002.IsVarInoutConstant));
		}

		// Token: 0x170005F0 RID: 1520
		// (get) Token: 0x06001F16 RID: 7958 RVA: 0x00065A04 File Offset: 0x00063C04
		protected virtual bool KeepAliases
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001F17 RID: 7959 RVA: 0x00065A08 File Offset: 0x00063C08
		protected virtual void \u0001(global::\u0015.\u0002 \u0002, _IType \u0003)
		{
			bool u = false;
			if (\u0002 != null)
			{
				u = \u0002.IgnoreActions;
			}
			_ISignature u2 = null;
			if (\u0002 != null && TypeClass.Userdef == \u0003.Class)
			{
				_IUserdefType iuserdefType = \u0003 as _IUserdefType;
				if (iuserdefType != null)
				{
					if (AccessFlag.Call == (this.TopOfStack.\u0001 & AccessFlag.Call))
					{
						\u0002.IgnoreActions = false;
					}
					else if (this.TopOfStack.\u0004)
					{
						\u0002.IgnoreActions = true;
					}
					u2 = (\u0002.FindSignature(iuserdefType) as _ISignature);
				}
			}
			this.TopOfStack.\u0001(\u0002, this.KeepAliases, \u0003, u2);
			if (\u0002 != null)
			{
				\u0002.IgnoreActions = u;
			}
		}

		// Token: 0x06001F18 RID: 7960 RVA: 0x00065A94 File Offset: 0x00063C94
		private void \u0001(_IExprement \u0002, IList<_IVariable> \u0003)
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
							this.AddError(\u0002, MessageId.Err_MultipleAssignsToSameInputInCall, new object[]
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

		// Token: 0x06001F19 RID: 7961 RVA: 0x00065B18 File Offset: 0x00063D18
		private static _IVariable \u0001(_ISignature \u0002)
		{
			if (\u0002.POUType == Operator.Function || \u0002.POUType == Operator.Method)
			{
				foreach (_IVariable ivariable in \u0002.AllVariables)
				{
					if (ivariable.HasFlag(VarFlag.Output))
					{
						return ivariable;
					}
				}
			}
			return null;
		}

		// Token: 0x06001F1A RID: 7962 RVA: 0x00065B84 File Offset: 0x00063D84
		public virtual void \u0001(_ICallExpression \u0002)
		{
			this.\u0002(\u0002);
			this.\u0001(AccessFlag.Call);
			\u0002._Callee.Accept(this);
			_ISignature u = this.TopOfStack.\u0001;
			_IVariable u2 = this.TopOfStack.\u0001;
			_IType itype = this.TopOfStack.typeResolved;
			ICompiledType compiledType = (itype != null) ? itype.DeRefType : null;
			global::\u0015.\u0002 u3 = this.TopOfStack.\u0005 ?? this.Scope;
			this.\u0002();
			_ISignature isignature;
			this.\u0001(u3, u, compiledType, out isignature, out compiledType);
			if (isignature == null)
			{
				return;
			}
			this.\u0001(u3, \u0002, ref isignature);
			this.\u0001(\u0002, u, u2, compiledType);
			this.\u0001(\u0002, isignature);
			this.\u0001(\u0002, isignature);
			SimpleTypeChecker.\u0001 u4 = new SimpleTypeChecker.\u0001();
			IVariable[] inputs = isignature.Inputs;
			bool u5 = this.\u0001 && (isignature.POUType == Operator.Method || isignature.POUType == Operator.Function);
			IList<_IAssignmentExpression> inputAssigns = \u0002._InputAssigns;
			global::\u0015.\u0002 u6 = this.Scope.\u0001(isignature);
			for (int i = 0; i < inputAssigns.Count; i++)
			{
				_IAssignmentExpression u7 = inputAssigns[i];
				this.\u0001(isignature, u4, u5, u6, i, u7);
			}
			foreach (_IAssignmentExpression u8 in \u0002._OutputAssigns)
			{
				this.\u0001(isignature, u4, u6, u8);
			}
			_IVariable ivariable = SimpleTypeChecker.\u0001(isignature);
			_IType itype2 = (ivariable != null) ? ivariable._Type : null;
			if (itype2 != null)
			{
				if (isignature.LibraryPath != string.Empty)
				{
					global::\u0015.\u0002 u9 = u3.\u0002(isignature);
					if (u9 != null)
					{
						this.\u0001(u9, itype2);
					}
				}
				else
				{
					this.\u0001(u3, itype2);
				}
			}
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0004 = null;
			this.\u0005(isignature);
			this.\u0001(\u0002, u6, u4);
			this.\u0001(\u0002, isignature, u4, u6);
		}

		// Token: 0x06001F1B RID: 7963 RVA: 0x00065D90 File Offset: 0x00063F90
		private void \u0001(_ICallExpression \u0002, _ISignature \u0003)
		{
			if (\u0003 == null || !\u0003.GetFlag(SignatureFlag.Abstract))
			{
				return;
			}
			_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0003.PrecompileParentId) as _ISignature;
			if (isignature != null && (isignature.POUType == Operator.Program || isignature.POUType == Operator.VarGlobal))
			{
				this.AddError(\u0002._Callee, MessageId.Err_AbstractMethodStaticCall, new object[]
				{
					\u0003.OrgName
				});
			}
		}

		// Token: 0x06001F1C RID: 7964 RVA: 0x00065E04 File Offset: 0x00064004
		private void \u0001(_ICallExpression \u0002, global::\u0015.\u0002 \u0003, SimpleTypeChecker.\u0001 \u0004)
		{
			foreach (_IExpression iexpression in \u0002.Inputs)
			{
				if (iexpression != null)
				{
					this.\u0001(\u0003, AccessFlag.Write);
					iexpression.Accept(this);
					\u0004.\u0001(this.TopOfStack.typeResolved, this.TopOfStack.\u0001);
					this.\u0002();
				}
			}
			foreach (_IExpression iexpression2 in \u0002.Outputs)
			{
				this.\u0001(\u0003, AccessFlag.Read);
				if (iexpression2 != null)
				{
					iexpression2.Accept(this);
				}
				\u0004.\u0002(this.TopOfStack.typeResolved, this.TopOfStack.\u0001);
				this.\u0002();
			}
			foreach (_IExpression iexpression3 in \u0002.EmptyAssigns)
			{
				this.\u0001(\u0003, AccessFlag.Read);
				if (iexpression3 != null)
				{
					iexpression3.Accept(this);
				}
				this.\u0002();
			}
		}

		// Token: 0x06001F1D RID: 7965 RVA: 0x00065F38 File Offset: 0x00064138
		private void \u0002(_ICallExpression \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001(AccessFlag.Read);
				\u0002._Condition.Accept(this);
				this.\u0001(\u0002._Condition, \u0002._Condition, this.TopOfStack.typeResolved, TypeTable.Bool);
				this.\u0002();
			}
		}

		// Token: 0x06001F1E RID: 7966 RVA: 0x00065F8C File Offset: 0x0006418C
		private void \u0001(_ISignature \u0002, SimpleTypeChecker.\u0001 \u0003, global::\u0015.\u0002 \u0004, _IAssignmentExpression \u0005)
		{
			if (\u0005._LValue != null)
			{
				_IVariable ivariable = null;
				_IVariableExpression ivariableExpression = \u0005.RValue as _IVariableExpression;
				if (ivariableExpression != null)
				{
					ivariable = (\u0002[ivariableExpression.Name] as _IVariable);
				}
				this.\u0001(AccessFlag.Write);
				this.TopOfStack.\u0001 = ((ivariable != null) ? ivariable._Type : null);
				this.TopOfStack.\u0001 = \u0004;
				\u0005._LValue.Accept(this);
				\u0003.\u0001(this.TopOfStack.typeResolved, this.TopOfStack.\u0005);
				this.\u0002();
			}
		}

		// Token: 0x06001F1F RID: 7967 RVA: 0x00066020 File Offset: 0x00064220
		private void \u0001(_ISignature \u0002, SimpleTypeChecker.\u0001 \u0003, bool \u0004, global::\u0015.\u0002 \u0005, int \u0006, _IAssignmentExpression \u0007)
		{
			if (\u0007._RValue != null)
			{
				bool flag = \u0007.LValue is INullExpression;
				_IVariable ivariable = null;
				if (flag)
				{
					if (\u0006 < \u0002.AllInputs.Length)
					{
						ivariable = (\u0002.AllInputs[\u0006] as _IVariable);
					}
				}
				else
				{
					_IVariableExpression ivariableExpression = \u0007.LValue as _IVariableExpression;
					if (ivariableExpression != null)
					{
						ivariable = (\u0002[ivariableExpression.Name] as _IVariable);
					}
				}
				if (\u0004 && flag && ivariable != null)
				{
					ivariable.AddPrecompileCrossReference(this.\u0001.PrecompileId);
				}
				this.\u0001(AccessFlag.Read);
				this.TopOfStack.\u0001 = ((ivariable != null) ? ivariable._Type : null);
				IVarLenArrayTypeInfo2 varLenArrayTypeInfo = APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.TypeInfo as IVarLenArrayTypeInfo2;
				if (varLenArrayTypeInfo != null)
				{
					varLenArrayTypeInfo.IsVarLenArray(ivariable, out this.TopOfStack.\u0001);
				}
				this.TopOfStack.\u0001 = \u0005;
				\u0007._RValue.Accept(this);
				\u0003.\u0001(this.TopOfStack.typeResolved, this.TopOfStack.\u0001, this.TopOfStack.\u0005);
				if (flag)
				{
					\u0003.\u0001((ivariable != null) ? ivariable._Type : null, ivariable);
				}
				this.\u0002();
			}
		}

		// Token: 0x06001F20 RID: 7968 RVA: 0x00066154 File Offset: 0x00064354
		private void \u0001(global::\u0015.\u0002 \u0002, _ISignature \u0003, ICompiledType \u0004, out _ISignature \u0005, out ICompiledType \u0006)
		{
			if (\u0004 != null && \u0004.Class == TypeClass.Userdef)
			{
				\u0005 = (\u0002.FindSignature(\u0004 as _IUserdefType) as _ISignature2);
			}
			else
			{
				\u0005 = \u0003;
				if (\u0005 != null && \u0004 == null)
				{
					\u0004 = global::\u0019.\u0003.\u0001(\u0005.Name);
				}
			}
			\u0006 = \u0004;
		}

		// Token: 0x06001F21 RID: 7969 RVA: 0x000661A8 File Offset: 0x000643A8
		private void \u0001(global::\u0015.\u0002 \u0002, _ICallExpression \u0003, ref _ISignature \u0004)
		{
			IList<ICompiledType> list = new List<ICompiledType>();
			for (int i = 0; i < \u0003.ParamExpressions.Count; i++)
			{
				this.\u0001(AccessFlag.Read);
				\u0003.ParamExpressions[i].Accept(this);
				list.Add(this.TopOfStack.typeResolved);
				this.\u0002();
			}
			if (\u0004 == null || \u0004.POUType != Operator.Method)
			{
				return;
			}
			_ISignature4 isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(\u0004).GetSignature(\u0004.ParentObjectGuid) as _ISignature4;
			if (isignature == null)
			{
				return;
			}
			IList<_ISignature> list2 = global::\u001F.\u0010.\u0001(\u0002, this.\u0001, \u0003, list, \u0004, isignature);
			if (global::\u001F.\u0010.\u0001(\u0003, list2, new global::\u000E.\u0016(this.\u0002), new global::\u0012.\u0013(this.\u0003)))
			{
				return;
			}
			if (list2[0].PrecompileId == \u0004.PrecompileId)
			{
				return;
			}
			\u0004 = list2[0];
		}

		// Token: 0x06001F22 RID: 7970 RVA: 0x0006628C File Offset: 0x0006448C
		private void \u0001(_ICallExpression \u0002, ISignature \u0003, _IVariable \u0004, ICompiledType \u0005)
		{
			if (\u0005 == null)
			{
				return;
			}
			if (!(\u0005 is _IUserdefType))
			{
				this.AddError(\u0002._Callee, MessageId.Err_CalleeInvalidType, new object[]
				{
					\u0002._Callee
				});
				if (\u0004 != null && \u0003 != null)
				{
					string u = global::\u0081.\u0002.Inf_RelatedPosition;
					_ISourcePosition isourcePosition = \u0004.SourcePosition as _ISourcePosition;
					isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0003.LibraryPath), \u0003.ObjectGuid);
					this.\u0002(global::\u0019.\u0003.\u0001(isourcePosition, u, Severity.Information, MessageId.Inf_RelatedPosition));
					return;
				}
			}
			else if ((\u0002._Callee is ICallExpression || this.\u0003(\u0004)) && \u0005.Class != TypeClass.Reference)
			{
				this.AddError(\u0002._Callee, MessageId.Err_NoCallInInstancePath, new object[]
				{
					\u0002._Callee
				});
			}
		}

		// Token: 0x06001F23 RID: 7971 RVA: 0x00066360 File Offset: 0x00064560
		private void \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			if (\u0003.POUType != Operator.Function && \u0003.POUType != Operator.FunctionBlock && \u0003.POUType != Operator.Program && \u0003.POUType != Operator.Method && \u0003.POUType != Operator.Action)
			{
				this.AddError(\u0002, MessageId.Err_WrongObjectType, new object[]
				{
					Scanner.GetTextOfOperator(\u0003.POUType)
				});
			}
			string attributeValue = \u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBSOLETE);
			if (!string.IsNullOrEmpty(attributeValue))
			{
				this.\u0001(\u0002, MessageId.Wrn_Obsolete, new object[]
				{
					\u0003.OrgName,
					attributeValue
				});
			}
			string attributeValue2 = \u0003.GetAttributeValue("no_explicit_call");
			if (!string.IsNullOrEmpty(attributeValue2))
			{
				this.AddError(\u0002._Callee, MessageId.Err_NoExplicitCall, new object[]
				{
					\u0003.OrgName,
					attributeValue2
				});
			}
		}

		// Token: 0x06001F24 RID: 7972 RVA: 0x0006642C File Offset: 0x0006462C
		private void \u0001(_ICallExpression \u0002, _ISignature \u0003, SimpleTypeChecker.\u0001 \u0004, global::\u0015.\u0002 \u0005)
		{
			if (\u0003.GetFlag(SignatureFlag.Action))
			{
				_ISignature isignature = \u0003;
				_ISignature isignature2 = this.Scope[isignature.ParentObjectGuid] as _ISignature;
				if (isignature2 != null)
				{
					\u0003 = isignature2;
				}
			}
			IList<_IVariable> list = Helper.\u0001(\u0003);
			int count = \u0002.ParamExpressions.Count;
			int count2 = list.Count;
			Operator poutype = \u0003.POUType;
			if (poutype == Operator.Function || poutype == Operator.Method)
			{
				if (list.Count != count)
				{
					IVariable variable = null;
					if (list.Count > 0)
					{
						variable = list[list.Count - 1];
					}
					if (list.Count == 0 || (variable != null && !variable.GetFlag(VarFlag.ImplicitParamsStruct)))
					{
						global::\u0082.\u0007.\u0001(\u0002, count, count2, \u0003, new \u0082.\u0007.\u0001(this.AddError), null);
						return;
					}
				}
				this.\u0001(\u0002, \u0004.\u0002);
				this.\u0001(\u0002, \u0003, \u0004);
				this.\u0001(\u0002, \u0003, \u0004, list, \u0005);
				this.\u0001(\u0002, \u0003, \u0004);
				return;
			}
			this.\u0002(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06001F25 RID: 7973 RVA: 0x0006652C File Offset: 0x0006472C
		private void \u0001(_ICallExpression \u0002, _ISignature \u0003, SimpleTypeChecker.\u0001 \u0004, IList<_IVariable> \u0005, global::\u0015.\u0002 \u0006)
		{
			IList<_IExpression> inputs = \u0002.Inputs;
			if (inputs.Count == 0)
			{
				return;
			}
			bool flag = inputs.All(new Func<_IExpression, bool>(SimpleTypeChecker.<>c.<>9.\u0001));
			bool flag2 = inputs.All(new Func<_IExpression, bool>(SimpleTypeChecker.<>c.<>9.\u0002));
			if (!flag && !flag2)
			{
				return;
			}
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			if (flag2)
			{
				for (int i = 0; i < Math.Min(paramExpressions.Count, \u0005.Count); i++)
				{
					if (\u0004.\u0003[i] != null && \u0004.\u0001[i] != null)
					{
						this.\u0001(\u0002, \u0003, i, \u0005[i], paramExpressions[i], \u0004.\u0001[i], \u0004.\u0001[i], \u0004.\u0001[i], \u0006);
					}
				}
				return;
			}
			for (int j = 0; j < inputs.Count; j++)
			{
				_IVariable ivariable = \u0004.\u0002[j];
				if (inputs[j] is IVariableExpression && ivariable != null)
				{
					this.\u0001(\u0002, \u0003, j, ivariable, paramExpressions[j], \u0004.\u0001[j], \u0004.\u0001[j], \u0004.\u0001[j], \u0006);
				}
			}
		}

		// Token: 0x06001F26 RID: 7974 RVA: 0x00066690 File Offset: 0x00064890
		private void \u0001(_ICallExpression \u0002, ISignature \u0003, SimpleTypeChecker.\u0001 \u0004)
		{
			IList<_IExpression> inputs = \u0002.Inputs;
			if (inputs.Count == 0)
			{
				return;
			}
			bool flag = inputs.All(new Func<_IExpression, bool>(SimpleTypeChecker.<>c.<>9.\u0003));
			bool flag2 = inputs.All(new Func<_IExpression, bool>(SimpleTypeChecker.<>c.<>9.\u0004));
			if (!flag && !flag2)
			{
				return;
			}
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			for (int i = 0; i < \u0004.\u0002.Count; i++)
			{
				_IVariable ivariable = \u0004.\u0002[i];
				_IVariable ivariable2 = \u0004.\u0001[i];
				if (ivariable != null && ivariable.HasFlag(VarFlag.Input | VarFlag.Inout) && ivariable.Type.Class == TypeClass.Reference && !this.\u0001(ivariable, ivariable2) && !ivariable.IsVarInoutConstant && !\u0003.GetFlag(SignatureFlag.External))
				{
					ICompiledType deRefType = ivariable._Type.DeRefType;
					ICompiledType deRefType2 = \u0004.\u0001[i].DeRefType;
					TypeClass @class = deRefType.Class;
					TypeClass class2 = deRefType2.Class;
					if (@class == class2 && (@class == TypeClass.String || @class == TypeClass.WString) && this.Scope.GetSize(deRefType) > \u0004.\u0001[i].GetSize(deRefType2))
					{
						this.AddError(\u0002, MessageId.Err_StringTooShortForVarInOut, new object[]
						{
							paramExpressions[i],
							ivariable2.OrgName,
							\u0003.OrgName
						});
					}
				}
			}
		}

		// Token: 0x06001F27 RID: 7975 RVA: 0x00066828 File Offset: 0x00064A28
		private void \u0001(_ICallExpression \u0002, _ISignature \u0003, SimpleTypeChecker.\u0001 \u0004)
		{
			global::\u0015.\u0002 u = this.Scope.\u0001(\u0003);
			IList<_IExpression> outputs = \u0002.Outputs;
			IList<_IExpression> outputExpressions = \u0002.OutputExpressions;
			for (int i = 0; i < outputs.Count; i++)
			{
				_IVariableExpression ivariableExpression = outputs[i] as _IVariableExpression;
				if (ivariableExpression != null)
				{
					IVariable variable = \u0004.\u0003[i];
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
						_IExpression u2 = outputExpressions[i];
						bool flag;
						this.\u0001(outputs[i], u2, outputs[i], \u0004.\u0004[i], \u0004.\u0002[i], u, \u0004.\u0002[i], false, out flag);
					}
				}
			}
		}

		// Token: 0x06001F28 RID: 7976 RVA: 0x00066900 File Offset: 0x00064B00
		private void \u0002(_ICallExpression \u0002, _ISignature \u0003, SimpleTypeChecker.\u0001 \u0004, global::\u0015.\u0002 \u0005)
		{
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			for (int i = 0; i < \u0002.Inputs.Count; i++)
			{
				_IVariableExpression ivariableExpression = \u0002.Inputs[i] as _IVariableExpression;
				if (ivariableExpression == null)
				{
					if (!(\u0002.ParamExpressions[i] is _IErrorExpression))
					{
						this.AddError(\u0002, MessageId.Err_InputMissing, new object[]
						{
							\u0002.ParamExpressions[i],
							\u0003.Name
						});
					}
				}
				else if (\u0004.\u0002[i] == null)
				{
					this.AddError(\u0002, MessageId.Err_IsNoInput, new object[]
					{
						ivariableExpression,
						\u0003.Name
					});
				}
				else
				{
					this.\u0001(\u0002, \u0003, i, \u0004.\u0002[i], paramExpressions[i], \u0004.\u0001[i], \u0004.\u0001[i], \u0004.\u0001[i], \u0005);
				}
			}
			global::\u0015.\u0002 u = this.Scope;
			if (!string.IsNullOrEmpty(\u0003.LibraryPath))
			{
				u = this.Scope.\u0002(\u0003);
			}
			for (int j = 0; j < \u0002.Outputs.Count; j++)
			{
				_IVariableExpression ivariableExpression2 = \u0002.Outputs[j] as _IVariableExpression;
				if (ivariableExpression2 != null)
				{
					if (\u0004.\u0003[j] == null)
					{
						this.AddError(\u0002, MessageId.Err_IsNoOutput, new object[]
						{
							ivariableExpression2,
							\u0003.Name
						});
					}
					else
					{
						_IExpression u2 = \u0002.OutputExpressions[j];
						bool flag;
						this.\u0001(\u0002.Outputs[j], ivariableExpression2, u2, \u0004.\u0004[j], \u0004.\u0002[j], u, u, false, out flag);
					}
				}
			}
			_ISignature isignature = \u0003;
			LHashSet<_ISignature> lhashSet = new LHashSet<_ISignature>();
			while (isignature != null && !lhashSet.Contains(isignature))
			{
				lhashSet.Add(isignature);
				foreach (_IVariable ivariable in isignature.InOuts.Cast<_IVariable>())
				{
					bool flag2 = false;
					for (int k = 0; k < \u0002.Inputs.Count; k++)
					{
						_IVariableExpression ivariableExpression3 = \u0002.Inputs[k] as _IVariableExpression;
						flag2 = (ivariableExpression3 != null && ivariableExpression3.PrecompileSignatureId == isignature.PrecompileId && ivariableExpression3.PrecompileVariableId == ivariable.PrecompileId);
						if (flag2)
						{
							this.\u0001(ivariableExpression3, \u0004.\u0003[k], \u0004.\u0001[k], u, u);
							break;
						}
					}
					if (!flag2)
					{
						this.AddError(\u0002, MessageId.Err_InOutNotAssigned, new object[]
						{
							ivariable.OrgName,
							\u0003.OrgName
						});
					}
				}
				if (isignature.IsLibraryObject)
				{
					if (isignature.BaseExpression != null)
					{
						global::\u0015.\u0002 u3 = this.Scope.\u0002(isignature);
						if (u3 != null)
						{
							ISignature[] array = u3.FindSignature(isignature.BaseExpression);
							isignature = (((array != null) ? array.FirstOrDefault<ISignature>() : null) as _ISignature);
						}
					}
				}
				else
				{
					isignature = (APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(isignature.PrecompileBaseSignatureId) as _ISignature);
				}
			}
		}

		// Token: 0x06001F29 RID: 7977 RVA: 0x00066C4C File Offset: 0x00064E4C
		protected bool \u0001(_IOperatorExpression \u0002, IList<_IType> \u0003, out _IType \u0004)
		{
			\u0004 = null;
			switch (\u0002.Code)
			{
			case Operator.__vcAdd:
			case Operator.__vcSub:
			case Operator.__vcMul:
			case Operator.__vcDiv:
			case Operator.__vcMin:
			case Operator.__vcMax:
			{
				IType deRefType = this.TopOfStack.\u0001.\u0001[0].DeRefType;
				IType deRefType2 = this.TopOfStack.\u0001.\u0001[1].DeRefType;
				if (deRefType.Class == TypeClass.__Vector)
				{
					\u0004 = (deRefType as _IType);
				}
				else if (deRefType2.Class == TypeClass.__Vector)
				{
					\u0004 = (deRefType2 as _IType);
				}
				break;
			}
			case Operator.__vcDot:
				\u0004 = (this.TopOfStack.\u0001.\u0001[0].DeRefType.BaseType as _IType);
				break;
			case Operator.__vcSqrt:
				\u0004 = \u0003[0];
				break;
			case Operator.__vcSetReal:
			case Operator.__vcSetLReal:
			{
				_IType u = TypeTable.Get((\u0002.Code == Operator.__vcSetLReal) ? Operator.LReal : Operator.Real);
				\u0004 = global::\u0019.\u0003.\u0001(u, global::\u0019.\u0003.\u0001((long)\u0002._OperandsList.Count));
				break;
			}
			case Operator.__vcLoadReal:
			case Operator.__vcLoadLReal:
			{
				_IType u2 = TypeTable.Get((\u0002.Code == Operator.__vcLoadLReal) ? Operator.LReal : Operator.Real);
				if (((\u0003[1].Class == TypeClass.Reference) ? (\u0003[1].DeRefType as _IType) : \u0003[1]).Class == TypeClass.Pointer)
				{
					\u0004 = global::\u0019.\u0003.\u0001(u2, \u0002._OperandsList[0]);
				}
				break;
			}
			case Operator.__vcStore:
				\u0004 = \u0003[0];
				break;
			default:
				return false;
			}
			return true;
		}

		// Token: 0x06001F2A RID: 7978 RVA: 0x00066DF4 File Offset: 0x00064FF4
		protected _IType \u0001(_IOperatorExpression \u0002, IList<_IType> \u0003)
		{
			_IType itype;
			if (this.\u0001(\u0002, \u0003, out itype))
			{
				return itype;
			}
			switch (\u0002.Code)
			{
			case Operator.__XAdd:
				return TypeTable.DInt;
			case Operator.__MemoryBarrier:
				return TypeTable.Bool;
			case Operator.__CompareAndSwap:
				return TypeTable.Bool;
			}
			foreach (_IType itype2 in \u0003)
			{
				if (itype2 == null || itype2.Class == TypeClass.Lazy)
				{
					return null;
				}
			}
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count != \u0003.Count || operandsList.Count == 0)
			{
				return null;
			}
			bool u = false;
			itype = this.\u0002(\u0002);
			if (itype == null)
			{
				itype = this.\u0002(\u0002, \u0003);
				if (itype == null)
				{
					itype = this.\u0001(\u0002);
					if (itype == null)
					{
						itype = this.\u0001(\u0002, operandsList, \u0003);
					}
				}
			}
			if (itype != null)
			{
				return itype;
			}
			Operator code = \u0002.Code;
			if (code <= Operator.VerticalLine)
			{
				if (code <= Operator.Not)
				{
					switch (code)
					{
					case Operator.Time:
						return TypeTable.Time;
					case Operator.LTime:
						return TypeTable.LTime;
					case Operator.Date:
					case Operator.DateAndTime:
					case Operator.TimeOfDay:
					case Operator.BitAdr:
					case Operator.IndexOf:
					case Operator.SizeOf:
					case Operator.Ini:
					case Operator.Trunc:
						goto IL_357;
					case Operator.Adr:
						goto IL_2E7;
					case Operator.Abs:
						return \u0003[0];
					case Operator.Limit:
						if (operandsList.Count < 3)
						{
							return null;
						}
						if (TypeTable.IsInteger(\u0003[1].Class))
						{
							return this.\u0001(\u0002.Code, false, 0, operandsList, \u0003);
						}
						return \u0003[1];
					case Operator.Min:
					case Operator.Max:
					case Operator.Mux:
					case Operator.Sel:
						goto IL_26F;
					default:
						switch (code)
						{
						case Operator.Add:
						case Operator.Sub:
						case Operator.Mul:
						case Operator.Div:
						case Operator.Mod:
							goto IL_26F;
						case Operator.And:
						case Operator.AndN:
						case Operator.Or:
						case Operator.OrN:
						case Operator.Xor:
						case Operator.XorN:
							break;
						case Operator.Not:
							if (operandsList[0] is ILiteralExpression && !TypeTable.IsConcreteType((operandsList[0] as ILiteralExpression).ConstantType))
							{
								return null;
							}
							u = true;
							goto IL_26F;
						default:
							goto IL_357;
						}
						break;
					}
				}
				else
				{
					if (code == Operator.Move)
					{
						return \u0003[0];
					}
					if (code - Operator.Plus <= 4)
					{
						goto IL_26F;
					}
					if (code - Operator.Ampersand > 1)
					{
						goto IL_357;
					}
				}
			}
			else if (code <= Operator.__BitOffset)
			{
				if (code != Operator.__AdrInst)
				{
					if (code == Operator.__RefAdr)
					{
						goto IL_2E7;
					}
					if (code != Operator.__BitOffset)
					{
						goto IL_357;
					}
					if (operandsList.Count == 3)
					{
						return TypeTable.Bool;
					}
					return TypeTable.Int;
				}
				else
				{
					ISignature localSignature = this.Scope.LocalSignature;
					if (localSignature != null && (localSignature.POUType == Operator.FunctionBlock || localSignature.GetFlag(SignatureFlag.Structure)))
					{
						_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(localSignature.Name);
						iuserdefType.SignatureId = localSignature.Id;
						return global::\u0019.\u0003.\u0001(iuserdefType);
					}
					return null;
				}
			}
			else if (code != Operator.__FCall)
			{
				if (code == Operator.__MemorySet)
				{
					return global::\u0019.\u0003.\u0001(TypeTable.DWord);
				}
				if (code - Operator.And_Then > 1)
				{
					goto IL_357;
				}
			}
			else
			{
				if (!(\u0003[0] is _IUserdefType))
				{
					goto IL_357;
				}
				ISignature signature = this.\u0001(\u0003[0] as _IUserdefType);
				if (signature != null && signature.Outputs.Length != 0)
				{
					return signature.Outputs[0].CompiledType as _IType;
				}
				goto IL_357;
			}
			u = true;
			IL_26F:
			return this.\u0001(\u0002, \u0003, operandsList, u);
			IL_2E7:
			if (\u0003.Count == 1)
			{
				return global::\u0019.\u0003.\u0001(\u0003[0]);
			}
			return TypeTable.DInt;
			IL_357:
			return null;
		}

		// Token: 0x06001F2B RID: 7979 RVA: 0x0006716C File Offset: 0x0006536C
		private _IType \u0001(_IOperatorExpression \u0002)
		{
			Operator code = \u0002.Code;
			if (code <= Operator.TestAndSet)
			{
				switch (code)
				{
				case Operator.BitAdr:
					return TypeTable.DWord;
				case Operator.IndexOf:
					return TypeTable.Int;
				case Operator.SizeOf:
					return (_IType)UnknownIdentVisitor.\u0001(\u0002, this.Scope, null);
				default:
					if (code == Operator.Trunc)
					{
						return TypeTable.DInt;
					}
					if (code == Operator.TestAndSet)
					{
						return TypeTable.DWord;
					}
					break;
				}
			}
			else if (code <= Operator.__GetLTick)
			{
				switch (code)
				{
				case Operator.TruncInt:
					return TypeTable.Int;
				case Operator.FupAssign:
				case Operator.__VarInfo:
				case Operator.__SystemScope:
					break;
				case Operator.__LocalOffset:
					return TypeTable.DInt;
				case Operator.__TypeOf:
					return TypeTable.Int;
				case Operator.__CRC:
					return TypeTable.DWord;
				case Operator.__MaxOffset:
					return TypeTable.UDInt;
				default:
					if (code == Operator.__GetLTick)
					{
						return TypeTable.ULInt;
					}
					break;
				}
			}
			else
			{
				if (code - Operator.LowerBound <= 1)
				{
					return TypeTable.DInt;
				}
				if (code == Operator.XSizeOf)
				{
					if (8 == this.PointerSize)
					{
						return TypeTable.ULInt;
					}
					return TypeTable.UDInt;
				}
			}
			return null;
		}

		// Token: 0x06001F2C RID: 7980 RVA: 0x0006726C File Offset: 0x0006546C
		private _IType \u0001(_IOperatorExpression \u0002, IList<_IExpression> \u0003, IList<_IType> \u0004)
		{
			Operator code = \u0002.Code;
			if (code - Operator.Rol > 3)
			{
				return null;
			}
			if (\u0003.Count == 0)
			{
				return null;
			}
			if (\u0003[0] is ILiteralExpression && !TypeTable.IsConcreteType((\u0003[0] as ILiteralExpression).ConstantType))
			{
				return null;
			}
			return \u0004[0];
		}

		// Token: 0x06001F2D RID: 7981 RVA: 0x000672C4 File Offset: 0x000654C4
		private _IType \u0002(_IOperatorExpression \u0002, IList<_IType> \u0003)
		{
			Operator code = \u0002.Code;
			if (code - Operator.Exp <= 10)
			{
				_IType result = TypeTable.Real;
				using (IEnumerator<_IType> enumerator = \u0003.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.Class != TypeClass.Real)
						{
							result = TypeTable.LReal;
							break;
						}
					}
				}
				return result;
			}
			return null;
		}

		// Token: 0x06001F2E RID: 7982 RVA: 0x00067330 File Offset: 0x00065530
		private _IType \u0002(_IOperatorExpression \u0002)
		{
			Operator code = \u0002.Code;
			if (code <= Operator.Lt)
			{
				if (code == Operator.__Reloc)
				{
					return TypeTable.Bool;
				}
				if (code != Operator.Ini && code - Operator.Eq > 5)
				{
					goto IL_C3;
				}
			}
			else if (code <= Operator.__Delete)
			{
				if (code - Operator.Less > 5)
				{
					switch (code)
					{
					case Operator.__VarInfo:
						return TypeTable.Bool;
					case Operator.__SystemScope:
					case Operator.__TypeOf:
					case Operator.__CRC:
					case Operator.__MaxOffset:
					case Operator.__New:
						goto IL_C3;
					case Operator.__Init:
						return TypeTable.Bool;
					case Operator.__IsValidRef:
						return TypeTable.Bool;
					case Operator.__QueryInterface:
						return TypeTable.Bool;
					case Operator.__QueryPointer:
						return TypeTable.Bool;
					case Operator.__Delete:
						return TypeTable.Bool;
					default:
						goto IL_C3;
					}
				}
			}
			else
			{
				if (code == Operator.__PropertyInfo)
				{
					return TypeTable.Bool;
				}
				if (code - Operator.__CallInitFunction > 1)
				{
					goto IL_C3;
				}
				return TypeTable.Bool;
			}
			return TypeTable.Bool;
			IL_C3:
			return null;
		}

		// Token: 0x06001F2F RID: 7983 RVA: 0x00067404 File Offset: 0x00065604
		private _IType \u0001(IList<_IType> \u0002)
		{
			if (\u0002.Count >= 2)
			{
				TypeClass typeClass = TypeClass.None;
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				_IPointerType ipointerType = null;
				int num = 0;
				foreach (_IType itype in \u0002)
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
			}
			return null;
		}

		// Token: 0x06001F30 RID: 7984 RVA: 0x00067528 File Offset: 0x00065728
		private _IType \u0002(IList<_IType> \u0002)
		{
			if (\u0002.Count == 2)
			{
				TypeClass @class = \u0002[0].Class;
				TypeClass class2 = \u0002[1].Class;
				if (@class == TypeClass.Date || @class == TypeClass.DateAndTime || @class == TypeClass.TimeOfDay || @class == TypeClass.Time)
				{
					if (class2 == TypeClass.Time)
					{
						return TypeTable.Get(@class);
					}
					if (class2 == @class)
					{
						return TypeTable.Time;
					}
				}
				else
				{
					if (@class == TypeClass.LTime && class2 == TypeClass.LTime)
					{
						return TypeTable.LTime;
					}
					if (@class == TypeClass.Pointer && TypeTable.IsInteger(class2))
					{
						return \u0002[0];
					}
					if (@class == TypeClass.Pointer && class2 == TypeClass.Pointer)
					{
						return TypeTable.DWord;
					}
				}
			}
			return null;
		}

		// Token: 0x06001F31 RID: 7985 RVA: 0x000675B8 File Offset: 0x000657B8
		private _IType \u0003(IList<_IType> \u0002)
		{
			if (\u0002.Count >= 2)
			{
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				int num = 0;
				foreach (_IType itype in \u0002)
				{
					TypeClass @class = itype.Class;
					if (@class != TypeClass.Time)
					{
						if (@class == TypeClass.LTime)
						{
							flag2 = true;
							num++;
						}
						else
						{
							if (!TypeTable.IsInteger(itype.Class))
							{
								flag3 = true;
							}
							if (TypeTable.IsLInteger2(itype.Class, this.Scope))
							{
								flag4 = true;
							}
						}
					}
					else
					{
						flag = true;
						num++;
					}
				}
				if (num == 1 && flag2 && !flag3)
				{
					return TypeTable.LTime;
				}
				if (num == 1 && flag && !flag3 && !flag4)
				{
					return TypeTable.Time;
				}
			}
			return null;
		}

		// Token: 0x06001F32 RID: 7986 RVA: 0x00067690 File Offset: 0x00065890
		private _IType \u0004(IList<_IType> \u0002)
		{
			bool flag = true;
			bool flag2 = false;
			_IType itype = null;
			for (int i = 0; i < \u0002.Count; i++)
			{
				_IType itype2 = \u0002[i];
				if (itype2.Class == TypeClass.Bool || itype2.Class == TypeClass.Bit || itype2.Class == TypeClass.BitConst)
				{
					flag2 = true;
					if (itype == null || itype2.Class == TypeClass.Bool)
					{
						itype = itype2;
					}
				}
				if (itype2.Class != TypeClass.Bool && itype2.Class != TypeClass.Bit && itype2.Class != TypeClass.BitConst)
				{
					flag = false;
					break;
				}
			}
			if (!flag || !flag2)
			{
				return null;
			}
			if (itype == null)
			{
				return TypeTable.Get(TypeClass.Bool);
			}
			return itype;
		}

		// Token: 0x06001F33 RID: 7987 RVA: 0x00067724 File Offset: 0x00065924
		private _IType \u0001(_IOperatorExpression \u0002, IList<_IType> \u0003, IList<_IExpression> \u0004, bool \u0005)
		{
			_IType itype = UnknownIdentVisitor.\u0001(\u0002, \u0003.Select(new Func<_IType, IType>(SimpleTypeChecker.<>c.<>9.\u0001)).ToArray<IType>());
			if (itype != null)
			{
				return itype;
			}
			Operator code = \u0002.Code;
			if (code <= Operator.Sel)
			{
				if (code - Operator.Min > 1)
				{
					if (code - Operator.Mux > 1)
					{
						goto IL_276;
					}
				}
				else
				{
					using (IEnumerator<_IType> enumerator = \u0003.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							_IType itype2 = enumerator.Current;
							TypeClass @class = itype2.Class;
							if (@class <= TypeClass.TimeOfDay)
							{
								if (@class != TypeClass.Bool && @class - TypeClass.String > 5)
								{
									continue;
								}
							}
							else if (@class != TypeClass.LTime && @class - TypeClass.LDate > 2)
							{
								continue;
							}
							return TypeTable.Get(itype2.Class);
						}
						goto IL_276;
					}
				}
				int i = 1;
				while (i < \u0003.Count)
				{
					_IType itype3 = \u0003[i];
					TypeClass @class = itype3.Class;
					if (@class <= TypeClass.Userdef)
					{
						if (@class == TypeClass.Bool)
						{
							goto IL_241;
						}
						switch (@class)
						{
						case TypeClass.String:
						case TypeClass.WString:
						case TypeClass.Time:
						case TypeClass.Date:
						case TypeClass.DateAndTime:
						case TypeClass.TimeOfDay:
							goto IL_241;
						case TypeClass.Pointer:
						case TypeClass.Array:
						case TypeClass.Userdef:
							return itype3;
						}
					}
					else if (@class == TypeClass.LTime || @class - TypeClass.LDate <= 2)
					{
						goto IL_241;
					}
					i++;
					continue;
					IL_241:
					return TypeTable.Get(itype3.Class);
				}
			}
			else
			{
				switch (code)
				{
				case Operator.Add:
					break;
				case Operator.Sub:
					goto IL_D6;
				case Operator.Mul:
					goto IL_E9;
				case Operator.Div:
					goto IL_FC;
				case Operator.Mod:
					goto IL_276;
				case Operator.And:
				case Operator.AndN:
				case Operator.Or:
				case Operator.OrN:
				case Operator.Xor:
				case Operator.XorN:
				case Operator.Not:
					goto IL_266;
				default:
					switch (code)
					{
					case Operator.Plus:
						break;
					case Operator.Minus:
						goto IL_D6;
					case Operator.Times:
						goto IL_E9;
					case Operator.Power:
						goto IL_276;
					case Operator.Divide:
						goto IL_FC;
					default:
						if (code - Operator.And_Then > 1)
						{
							goto IL_276;
						}
						goto IL_266;
					}
					break;
				}
				_IType itype4 = this.\u0001(\u0003);
				if (itype4 != null)
				{
					return itype4;
				}
				goto IL_276;
				IL_D6:
				_IType itype5 = this.\u0002(\u0003);
				if (itype5 != null)
				{
					return itype5;
				}
				goto IL_276;
				IL_E9:
				_IType itype6 = this.\u0003(\u0003);
				if (itype6 != null)
				{
					return itype6;
				}
				goto IL_276;
				IL_FC:
				if (\u0004.Count != 2)
				{
					goto IL_276;
				}
				TypeClass class2 = \u0003[0].Class;
				TypeClass class3 = \u0003[1].Class;
				if (class2 == TypeClass.Time && TypeTable.IsInteger(class3) && !TypeTable.IsLInteger2(class3, this.Scope))
				{
					return TypeTable.Time;
				}
				if (class2 == TypeClass.LTime && TypeTable.IsInteger(class3))
				{
					return TypeTable.LTime;
				}
				goto IL_276;
				IL_266:
				_IType itype7 = this.\u0004(\u0003);
				if (itype7 != null)
				{
					return itype7;
				}
			}
			IL_276:
			int u = 0;
			if (\u0002.Code == Operator.Mux || \u0002.Code == Operator.Sel)
			{
				u = 1;
			}
			return this.\u0001(\u0002.Code, \u0005, u, \u0004, \u0003);
		}

		// Token: 0x06001F34 RID: 7988 RVA: 0x000679E4 File Offset: 0x00065BE4
		public _IType \u0001(Operator \u0002, bool \u0003, int \u0004, IList<_IExpression> \u0005, IList<_IType> \u0006)
		{
			_IType result = null;
			bool flag = true;
			try
			{
				for (int i = 0; i < \u0005.Count; i++)
				{
					\u0005[i]._CompiledType = \u0006[i];
					if (\u0005[i].Type == null)
					{
						flag = false;
						break;
					}
					_IEnumType ienumType = \u0005[i].Type as _IEnumType;
					if (ienumType != null && ienumType.SignatureId < 0)
					{
						ISignature signature = this.Scope.FindSignature(ienumType);
						if (signature != null && signature.HasAttribute("strict"))
						{
							flag = false;
							break;
						}
					}
					if (\u0005[i]._CompiledType.Class == TypeClass.Userdef || \u0005[i]._CompiledType.Class == TypeClass.String || \u0005[i]._CompiledType.Class == TypeClass.WString)
					{
						return \u0005[i]._CompiledType as _IType;
					}
				}
				if (flag)
				{
					result = global::\u001D.\u0005.\u0001(\u0002, \u0003, \u0004, \u0005, null, this.Scope, null);
				}
			}
			finally
			{
				for (int j = 0; j < \u0005.Count; j++)
				{
					\u0005[j]._CompiledType = null;
				}
			}
			return result;
		}

		// Token: 0x06001F35 RID: 7989 RVA: 0x00067B28 File Offset: 0x00065D28
		public bool \u0001(ICompiledType \u0002)
		{
			if (\u0002.Class != TypeClass.Userdef)
			{
				return false;
			}
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			if (iuserdefType == null)
			{
				return false;
			}
			ISignature signature = this.\u0001(iuserdefType);
			return signature != null && signature.POUType == Operator.Interface;
		}

		// Token: 0x06001F36 RID: 7990 RVA: 0x00067B64 File Offset: 0x00065D64
		public bool \u0002(ICompiledType \u0002)
		{
			if (\u0002.Class != TypeClass.Userdef)
			{
				return false;
			}
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			if (iuserdefType == null)
			{
				return false;
			}
			ISignature signature = this.\u0001(iuserdefType);
			return signature != null && (signature.POUType == Operator.Type && signature.GetFlag(SignatureFlag.Structure));
		}

		// Token: 0x06001F37 RID: 7991 RVA: 0x00067BB0 File Offset: 0x00065DB0
		private void \u0002(_IExpression \u0002)
		{
			if (\u0002.MessagesList != null && 0 < \u0002.MessagesList.Count)
			{
				foreach (_ICompilerMessage icompilerMessage in \u0002.MessagesList)
				{
					if (Severity.Error == icompilerMessage.Severity)
					{
						this.\u0001(icompilerMessage);
					}
				}
			}
		}

		// Token: 0x06001F38 RID: 7992 RVA: 0x00067C1C File Offset: 0x00065E1C
		public virtual void \u0001(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			LList<_IType> llist = new LList<_IType>();
			LList<_IVariable> llist2 = new LList<_IVariable>();
			LList<_ISignature> llist3 = new LList<_ISignature>();
			this.TopOfStack.\u0001 = new global::\u0004.\u0006
			{
				\u0001 = operandsList,
				\u0001 = llist3,
				\u0001 = llist,
				\u0001 = llist2
			};
			this.\u0002(\u0002);
			bool flag = false;
			_IType u = this.TopOfStack.\u0001;
			flag = this.\u0001(operandsList, llist, llist2, llist3, flag, u);
			if (flag)
			{
				return;
			}
			_IType itype = this.\u0001(\u0002, llist);
			this.\u0001(null, itype);
			if (itype == null)
			{
				return;
			}
			this.\u0001(\u0002, llist, llist2, llist3);
			this.TopOfStack.\u0001.\u0001 = itype;
			\u0002.AcceptOperatorVisitor(this);
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0004 = null;
		}

		// Token: 0x06001F39 RID: 7993 RVA: 0x00067CFC File Offset: 0x00065EFC
		private void \u0001(_IOperatorExpression \u0002, LList<_IType> \u0003, LList<_IVariable> \u0004, LList<_ISignature> \u0005)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression = operandsList[i];
				_IType u = \u0003[i];
				IVariable variable = \u0004[i];
				_ISignature isignature = \u0005[i];
				if (\u0002.Code == Operator.__Delete && (iexpression is _IThisExpression || iexpression is _IBaseExpression))
				{
					this.AddError(iexpression, MessageId.Err_OperationNotPossibleOnType, new object[]
					{
						\u0002.Code,
						iexpression.ToString()
					});
					return;
				}
				bool flag = iexpression is _IThisExpression;
				bool flag2 = isignature == null || variable != null;
				bool flag3 = isignature != null && isignature.POUType == Operator.Interface && (\u0002.Code == Operator.Eq || \u0002.Code == Operator.Ne || \u0002.Code == Operator.Equal || \u0002.Code == Operator.NotEqual);
				bool flag4 = iexpression is _ICallExpression;
				if (!flag && !flag2 && !flag3 && !flag4 && this.\u0001(\u0002, iexpression, u, isignature))
				{
					break;
				}
			}
		}

		// Token: 0x06001F3A RID: 7994 RVA: 0x00067E14 File Offset: 0x00066014
		private bool \u0001(_IOperatorExpression \u0002, _IExpression \u0003, _IType \u0004, _ISignature \u0005)
		{
			bool result = false;
			if (\u0005.GetFlag(SignatureFlag.Alias) && Helper.\u0001(\u0002))
			{
				global::\u0015.\u0002 u;
				\u0005 = this.Scope.\u0001(\u0005, out u);
			}
			if (\u0005 != null)
			{
				string text;
				bool flag;
				this.\u0001(\u0002, \u0005, out text, out flag);
				if (!flag)
				{
					if (text == null)
					{
						text = \u0004.ToString();
					}
					this.AddError(\u0003, MessageId.Err_OperationNotPossibleOnType, new object[]
					{
						\u0002.Code,
						text
					});
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06001F3B RID: 7995 RVA: 0x00067E8C File Offset: 0x0006608C
		private void \u0001(_IOperatorExpression \u0002, _ISignature \u0003, out string \u0004, out bool \u0005)
		{
			\u0004 = null;
			\u0005 = false;
			Operator code = \u0002.Code;
			if (code <= Operator.__MaxOffset)
			{
				switch (code)
				{
				case Operator.Adr:
				case Operator.IndexOf:
				{
					Operator poutype = \u0003.POUType;
					if (poutype - Operator.Function <= 1 || poutype == Operator.Program)
					{
						\u0005 = true;
						return;
					}
					if (poutype != Operator.Method)
					{
						return;
					}
					ISignature signature = this.Scope[\u0003.ParentObjectGuid];
					if (signature == null || signature.POUType != Operator.Interface)
					{
						\u0005 = true;
						return;
					}
					\u0004 = signature.OrgName + "." + \u0003.OrgName;
					return;
				}
				case Operator.BitAdr:
					return;
				case Operator.SizeOf:
					goto IL_50;
				default:
					if (code - Operator.__CRC > 1)
					{
						return;
					}
					break;
				}
			}
			else if (code != Operator.__FCall)
			{
				if (code != Operator.XSizeOf)
				{
					return;
				}
				goto IL_50;
			}
			\u0005 = true;
			return;
			IL_50:
			if ((\u0003.POUType == Operator.FunctionBlock && !\u0003.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants)) || \u0003.GetFlag(SignatureFlag.Structure) || \u0003.HasAttribute("subsequent"))
			{
				\u0005 = true;
				return;
			}
			if (\u0003.GetFlag(SignatureFlag.Enum) && \u0003.AllVariables.Count != 0)
			{
				\u0005 = true;
				return;
			}
		}

		// Token: 0x06001F3C RID: 7996 RVA: 0x00067F9C File Offset: 0x0006619C
		private bool \u0001(IList<_IExpression> \u0002, LList<_IType> \u0003, LList<_IVariable> \u0004, LList<_ISignature> \u0005, bool \u0006, _IType \u0007)
		{
			foreach (_IExprement iexprement in \u0002)
			{
				this.\u0001(AccessFlag.Read);
				this.TopOfStack.\u0001 = \u0007;
				iexprement.Accept(this);
				_IType itype = null;
				if (this.TopOfStack.typeResolved == null)
				{
					\u0006 = true;
				}
				else if (!this.\u0001(ref itype))
				{
					\u0006 = true;
				}
				else
				{
					\u0004.Add(this.TopOfStack.\u0001);
					\u0005.Add(this.TopOfStack.\u0001);
				}
				\u0003.Add(itype);
				this.\u0002();
			}
			return \u0006;
		}

		// Token: 0x06001F3D RID: 7997 RVA: 0x00068050 File Offset: 0x00066250
		protected bool \u0001(ref _IType \u0002)
		{
			bool flag = false;
			if (this.TopOfStack.typeResolved != null && this.TopOfStack.typeResolved.Class == TypeClass.Userdef)
			{
				_ISignature isignature = this.Scope.FindSignature(this.TopOfStack.typeResolved as _IUserdefType) as _ISignature;
				if (isignature == null)
				{
					return false;
				}
				IList<_IVariable> allVariables = isignature.AllVariables;
				if (isignature.GetFlag(SignatureFlag.Enum) && allVariables.Any<_IVariable>() && allVariables[0].Type is _IEnumType)
				{
					\u0002 = (allVariables[0].Type as _IEnumType);
					flag = true;
				}
				if (isignature.GetFlag(SignatureFlag.Alias) && !this.KeepAliases)
				{
					global::\u0015.\u0002 u;
					\u0002 = this.Scope.\u0001(isignature, this.TopOfStack.typeResolved, out u);
					flag = true;
				}
			}
			if (!flag)
			{
				if (this.TopOfStack.typeResolved != null && this.TopOfStack.typeResolved.Class == TypeClass.Enum)
				{
					_IEnumType etype = this.TopOfStack.typeResolved as _IEnumType;
					ISignature signature = this.Scope.FindSignature(etype);
					if (signature != null && signature.HasAttribute("strict"))
					{
						\u0002 = this.TopOfStack.typeResolved;
					}
					else
					{
						\u0002 = (this.TopOfStack.typeResolved.DeRefType as _IType);
					}
				}
				else
				{
					_IType itype = this.TopOfStack.typeResolved;
					\u0002 = (((itype != null) ? itype.DeRefType : null) as _IType);
				}
			}
			return true;
		}

		// Token: 0x06001F3E RID: 7998 RVA: 0x000681C0 File Offset: 0x000663C0
		public void \u0001(_IConversionExpression \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Exp.Accept(this);
			this.\u0002();
			this.\u0001(null, TypeTable.Get(\u0002.To));
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0004 = null;
		}

		// Token: 0x06001F3F RID: 7999 RVA: 0x0006821C File Offset: 0x0006641C
		public void \u0001(_IThisExpression \u0002)
		{
			_ISignature isignature = null;
			if (this.Scope.LocalSignature != null)
			{
				isignature = (this.Scope.LocalSignature as _ISignature);
				global::\u0019.\u0003.\u0001(isignature.Name).SignatureId = isignature.PrecompileId;
				this.\u0001(null, global::\u0019.\u0003.\u0001());
				this.\u0005(isignature);
			}
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0001 = isignature;
			this.TopOfStack.\u0004 = null;
		}

		// Token: 0x06001F40 RID: 8000 RVA: 0x00068298 File Offset: 0x00066498
		public void \u0001(_IBaseExpression \u0002)
		{
			_ISignature isignature = null;
			if (this.Scope.LocalSignature != null)
			{
				_ISignature isignature2 = this.Scope.LocalSignature as _ISignature;
				if (isignature2.PrecompileBaseSignatureId != Helper.InvalidId)
				{
					isignature = (APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(isignature2.PrecompileBaseSignatureId) as _ISignature);
				}
				else
				{
					ISignature[] array = this.Scope.FindSignature(isignature2.BaseExpression);
					if (array != null && array.Length == 1 && array[0] != null)
					{
						isignature = (array[0] as _ISignature);
						isignature2.PrecompileBaseSignatureId = isignature.PrecompileId;
					}
				}
			}
			if (isignature != null)
			{
				this.\u0005(isignature);
				global::\u0019.\u0003.\u0001(isignature.Name).SignatureId = isignature.PrecompileId;
				this.\u0001(null, global::\u0019.\u0003.\u0001());
			}
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0001 = isignature;
			this.TopOfStack.\u0004 = null;
		}

		// Token: 0x06001F41 RID: 8001 RVA: 0x00068374 File Offset: 0x00066574
		public virtual void visit(_ILiteralExpression literal)
		{
			this.\u0001(null, (_IType)Helper.\u0001(literal, true, false, true, false, this.\u0001.IsDefined("NO_UNICODE_SUPPORT")));
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0004 = null;
		}

		// Token: 0x06001F42 RID: 8002 RVA: 0x000683CC File Offset: 0x000665CC
		public void \u0001(_IAddressExpression \u0002)
		{
			_IType directVariableSizeType = TypeTable.GetDirectVariableSizeType(\u0002.DirectAddress.Size);
			this.\u0001(null, directVariableSizeType);
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0001 = null;
			this.TopOfStack.\u0004 = null;
		}

		// Token: 0x06001F43 RID: 8003 RVA: 0x00068418 File Offset: 0x00066618
		public virtual void \u0001(_IVariableExpression \u0002)
		{
			_IVariable ivariable = null;
			IType u = null;
			_ISignature isignature = null;
			IVariable[] array = null;
			ISignature[] array2 = null;
			global::\u0015.\u0002 u2 = null;
			this.\u0001(\u0002, out array, out array2, out u2);
			if (array != null && array.Length != 0)
			{
				this.\u0001(\u0002, array, array2);
				if (array.Length != 0)
				{
					ivariable = (_IVariable)array[0];
					u = this.\u0001(ivariable);
					isignature = (array2[0] as _ISignature);
					this.\u0001(\u0002, ivariable, u, isignature);
				}
			}
			else if (array2 != null && array2.Length != 0)
			{
				this.\u0001(\u0002, array2);
				if (array2.Length != 0)
				{
					isignature = (array2[0] as _ISignature);
					this.\u0002(\u0002, isignature);
				}
			}
			else if (u2 == null)
			{
				if (SimpleTypeChecker.\u0001(\u0002.Name))
				{
					\u0002.SignatureId = Helper.InvalidId;
					\u0002.VariableId = Helper.InvalidId;
				}
				else
				{
					\u0002.SignatureId = Helper.InvalidId;
					\u0002.VariableId = Helper.InvalidId;
					this.AddError(\u0002, MessageId.Err_IdentNotDefined, new object[]
					{
						\u0002.Name
					});
				}
			}
			this.\u0001(\u0002, ivariable, u, isignature, u2);
			this.TopOfStack.\u0001 = ivariable;
			this.TopOfStack.\u0001 = isignature;
			this.TopOfStack.\u0004 = u2;
		}

		// Token: 0x06001F44 RID: 8004 RVA: 0x00068538 File Offset: 0x00066738
		private void \u0001(_IVariableExpression \u0002, out IVariable[] \u0003, out ISignature[] \u0004, out global::\u0015.\u0002 \u0005)
		{
			bool u = this.Scope.IgnoreImplicitEnumMembers;
			if (this.TopOfStack.\u0003)
			{
				this.Scope.IgnoreImplicitEnumMembers = true;
			}
			this.Scope.\u0001(\u0002.Name, out \u0003, out \u0004, out \u0005);
			this.Scope.IgnoreImplicitEnumMembers = u;
		}

		// Token: 0x06001F45 RID: 8005 RVA: 0x0006858C File Offset: 0x0006678C
		private void \u0001(_IVariableExpression \u0002, _IVariable \u0003, IType \u0004, _ISignature \u0005, global::\u0015.\u0002 \u0006)
		{
			if (\u0003 != null)
			{
				_IType u = this.\u0001(\u0004, \u0005, this.TopOfStack.opAssignment == Operator.RefAssign && this.TopOfStack.\u0001 == AccessFlag.Read);
				this.\u0001(\u0002, \u0005);
				global::\u0015.\u0002 u2 = this.Scope;
				if (\u0005 != null)
				{
					u2 = this.Scope.\u0001(\u0005);
				}
				this.\u0001(u2, u);
				return;
			}
			if (\u0005 != null)
			{
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(\u0005.Name);
				iuserdefType.SignatureId = \u0005.PrecompileId;
				this.\u0001(this.Scope, iuserdefType);
				return;
			}
		}

		// Token: 0x06001F46 RID: 8006 RVA: 0x00068624 File Offset: 0x00066824
		private _IType \u0001(IType \u0002, _ISignature \u0003, bool \u0004)
		{
			if (\u0002 != null && \u0002.Class == TypeClass.Reference && (this.TopOfStack.opAssignment != Operator.RefAssign || (this.TopOfStack.opAssignment == Operator.RefAssign && this.TopOfStack.\u0001 == AccessFlag.Write)))
			{
				return (\u0002 as _IReferenceType)._Base;
			}
			if (\u0002 != null && \u0004)
			{
				_IArrayType u;
				if (this.\u0001(\u0002 as _IType, out u))
				{
					return global::\u0019.\u0003.\u0001(u);
				}
				if (\u0002.Class == TypeClass.Reference)
				{
					\u0002 = this.\u0001((\u0002 as _IReferenceType)._Base, \u0003, false);
				}
				return global::\u0019.\u0003.\u0001(\u0002 as _IType);
			}
			else
			{
				_IEnumType ienumType = \u0002 as _IEnumType;
				if (ienumType != null)
				{
					_IType result = ienumType;
					ienumType.SignatureId = \u0003.PrecompileId;
					return result;
				}
				return \u0002 as _IType;
			}
		}

		// Token: 0x06001F47 RID: 8007 RVA: 0x000686E8 File Offset: 0x000668E8
		private void \u0001(_IVariableExpression \u0002, _ISignature \u0003)
		{
			if (\u0003 != null && this.\u0001 != null && \u0003.PrecompileId != this.\u0001.PrecompileId && \u0003.POUType == Operator.VarGlobal && \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_OBSOLETE))
			{
				string attributeValue = \u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBSOLETE);
				if (!string.IsNullOrEmpty(attributeValue))
				{
					this.\u0001(\u0002, MessageId.Wrn_Obsolete, new object[]
					{
						\u0003.OrgName,
						attributeValue
					});
				}
			}
		}

		// Token: 0x06001F48 RID: 8008 RVA: 0x00068760 File Offset: 0x00066960
		private void \u0001(IExpression \u0002, _ISignature \u0003)
		{
			global::\u001E.\u0006.\u0001(\u0002, \u0003);
			if (this.\u0001)
			{
				global::\u001E.\u0006.\u0001(\u0002, this.TopOfStack.\u0002, this.\u0001, \u0003);
			}
		}

		// Token: 0x06001F49 RID: 8009 RVA: 0x0006878C File Offset: 0x0006698C
		private void \u0002(IExpression \u0002, _ISignature \u0003)
		{
			global::\u001E.\u0006.\u0001(\u0002, \u0003);
			if (this.\u0001)
			{
				global::\u001E.\u0006.\u0002(\u0002, this.TopOfStack.\u0002, this.\u0001, \u0003);
			}
		}

		// Token: 0x06001F4A RID: 8010 RVA: 0x000687B8 File Offset: 0x000669B8
		private void \u0004(_ISignature \u0002)
		{
			if (this.\u0001)
			{
				global::\u001E.\u0006.\u0002(this.\u0001, \u0002);
			}
		}

		// Token: 0x06001F4B RID: 8011 RVA: 0x000687D0 File Offset: 0x000669D0
		private void \u0005(_ISignature \u0002)
		{
			if (this.\u0001)
			{
				global::\u001E.\u0006.\u0001(this.\u0001, \u0002);
			}
		}

		// Token: 0x06001F4C RID: 8012 RVA: 0x000687E8 File Offset: 0x000669E8
		private void \u0001(_IVariableExpression \u0002, _IVariable \u0003, IType \u0004, _ISignature \u0005)
		{
			\u0002.PrecompileVariableId = \u0003.PrecompileId;
			\u0002.PrecompileSignatureId = \u0005.PrecompileId;
			if (this.\u0001)
			{
				global::\u001E.\u0006.\u0001(\u0002, \u0003, \u0004, this.Scope, this.\u0001, \u0005);
			}
		}

		// Token: 0x06001F4D RID: 8013 RVA: 0x00068824 File Offset: 0x00066A24
		private void \u0001(_IUserdefType \u0002)
		{
			if (this.\u0001 && \u0002 != null)
			{
				global::\u001E.\u0006.\u0001(\u0002, this.\u0001, this.Scope);
			}
		}

		// Token: 0x06001F4E RID: 8014 RVA: 0x00068844 File Offset: 0x00066A44
		private void \u0005(_IVariable \u0002)
		{
			if (this.\u0001)
			{
				\u0002.AddPrecompileCrossReference(this.\u0001.PrecompileId);
				if (\u0002.HasAttribute("map_to"))
				{
					string attributeValue = \u0002.GetAttributeValue("map_to");
					IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(attributeValue, false, false, false, false);
					_IExpression iexpression = ((IParser4)APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner)).ParseExpression() as _IExpression;
					if (iexpression != null)
					{
						iexpression.Accept(this);
					}
				}
			}
		}

		// Token: 0x06001F4F RID: 8015 RVA: 0x000688C4 File Offset: 0x00066AC4
		private IType \u0001(_IVariable \u0002)
		{
			IType type = \u0002.Type;
			if (type != null && type.Class == TypeClass.Lazy)
			{
				string attributeValue = \u0002.GetAttributeValue("inferredtype");
				if (attributeValue != null)
				{
					this.\u0001 = (this.\u0001 ?? new global::\u0004.\u0005());
					IType type2 = this.\u0001.\u0001(attributeValue);
					if (type2 != null)
					{
						type = type2;
					}
				}
			}
			return type;
		}

		// Token: 0x06001F50 RID: 8016 RVA: 0x0006891C File Offset: 0x00066B1C
		private void \u0001(_IVariableExpression \u0002, ISignature[] \u0003)
		{
			if (\u0003.Length > 1)
			{
				this.AddError(\u0002, MessageId.Err_Ambiguity, new object[]
				{
					\u0002.Name
				});
				for (int i = 0; i < \u0003.Length; i++)
				{
					_ISourcePosition isourcePosition = global::\u0019.\u0003.\u0001();
					isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0003[i].LibraryPath), \u0003[i].ObjectGuid);
					string u = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
					this.\u0002(global::\u0019.\u0003.\u0001(isourcePosition, u, Severity.Information, MessageId.Inf_RelatedPosition));
				}
			}
		}

		// Token: 0x06001F51 RID: 8017 RVA: 0x000689AC File Offset: 0x00066BAC
		private void \u0001(_IVariableExpression \u0002, IVariable[] \u0003, ISignature[] \u0004)
		{
			if (\u0003.Length > 1)
			{
				this.AddError(\u0002, MessageId.Err_Ambiguity, new object[]
				{
					\u0002.Name
				});
				for (int i = 0; i < \u0003.Length; i++)
				{
					_ISourcePosition isourcePosition = \u0003[i].SourcePosition as _ISourcePosition;
					if (isourcePosition.ObjectGuid != Guid.Empty)
					{
						isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0004[i].LibraryPath), \u0004[i].ObjectGuid);
					}
					string u = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
					this.\u0002(global::\u0019.\u0003.\u0001(isourcePosition, u, Severity.Information, MessageId.Inf_RelatedPosition));
				}
			}
		}

		// Token: 0x06001F52 RID: 8018 RVA: 0x00068A58 File Offset: 0x00066C58
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			this.\u0001();
			\u0002._Var.Accept(this);
			_IType itype = this.TopOfStack.typeResolved;
			global::\u0015.\u0002 u = this.TopOfStack.\u0005;
			_IVariable u2 = this.TopOfStack.\u0001;
			_ISignature u3 = this.TopOfStack.\u0001;
			this.\u0002();
			if (itype == null)
			{
				return;
			}
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				this.\u0001(this.Scope, AccessFlag.Read);
				\u0002.GetAccess(i).Accept(this);
				this.\u0002();
			}
			global::\u0015.\u0002 u4 = u ?? this.Scope;
			_IArrayType iarrayType;
			if (this.\u0001(itype, out iarrayType))
			{
				itype = iarrayType;
			}
			_IPointerType ipointerType = itype as _IPointerType;
			if (ipointerType != null)
			{
				this.\u0001(u4, ipointerType._Base);
			}
			else
			{
				_IVectorType ivectorType = itype as _IVectorType;
				if (ivectorType != null)
				{
					this.\u0001(u4, ivectorType._Base);
				}
				else
				{
					_IArrayType iarrayType2 = itype as _IArrayType;
					if (iarrayType2 != null)
					{
						this.\u0001(u4, iarrayType2._Base);
					}
				}
			}
			if (u != null)
			{
				u = this.TopOfStack.\u0005;
			}
			this.TopOfStack.\u0001 = u2;
			this.TopOfStack.\u0001 = u3;
			this.TopOfStack.\u0005 = u;
		}

		// Token: 0x06001F53 RID: 8019 RVA: 0x00068B8C File Offset: 0x00066D8C
		internal bool \u0002(_IVariable \u0002)
		{
			return \u0002 != null && (\u0002.HasAttribute(CompileAttributes.GET_BITACCESS) || \u0002.HasAttribute(CompileAttributes.SET_BITACCESS) || \u0002.HasAttribute(CompileAttributes.DEVICE_PARAMETER));
		}

		// Token: 0x06001F54 RID: 8020 RVA: 0x00068BBC File Offset: 0x00066DBC
		internal bool \u0003(_IVariable \u0002)
		{
			return \u0002 != null && \u0002.IsProperty;
		}

		// Token: 0x06001F55 RID: 8021 RVA: 0x00068BCC File Offset: 0x00066DCC
		internal bool \u0001(global::\u0015.\u0002 \u0002, _IVariable \u0003)
		{
			if (\u0003 != null && \u0003.IsProperty)
			{
				string u = "__get" + \u0003.OrgName;
				ISignature[] array = \u0002.\u0001(u);
				if (array != null && array.Length == 1 && array[0] != null)
				{
					return array[0].GetFlag(SignatureFlag.Abstract);
				}
			}
			return false;
		}

		// Token: 0x06001F56 RID: 8022 RVA: 0x00068C20 File Offset: 0x00066E20
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			this.\u0001();
			this.TopOfStack.\u0003 = true;
			this.TopOfStack.\u0004 = true;
			\u0002._Left.Accept(this);
			_IType itype = this.TopOfStack.typeResolved;
			_IVariable u = this.TopOfStack.\u0001;
			_ISignature u2 = this.TopOfStack.\u0001;
			global::\u0015.\u0002 u3 = this.TopOfStack.\u0004;
			global::\u0015.\u0002 u4 = this.TopOfStack.\u0005;
			this.\u0002();
			if (itype == null && u2 == null && u3 == null)
			{
				return;
			}
			this.\u0001();
			this.TopOfStack.\u0003 = true;
			_ISignature isignature;
			global::\u0015.\u0002 u5 = this.\u0001(itype, u2, u3, u4, out isignature);
			\u0002._Right.Accept(this);
			_IType itype2 = this.TopOfStack.typeResolved;
			_IVariable u6 = this.TopOfStack.\u0001;
			_ISignature u7 = this.TopOfStack.\u0001;
			global::\u0015.\u0002 u8 = this.TopOfStack.\u0004;
			global::\u0015.\u0002 u9 = this.TopOfStack.\u0005;
			this.\u0002();
			this.\u0001(null, itype2);
			this.TopOfStack.\u0001 = u6;
			this.TopOfStack.\u0001 = u7;
			if (u8 != null)
			{
				this.TopOfStack.\u0004 = u8;
			}
			else if (u9 != null)
			{
				this.TopOfStack.\u0005 = u9;
			}
			else
			{
				this.TopOfStack.\u0005 = u5;
			}
			if (itype == null)
			{
				return;
			}
			if (\u0002._Left is _IVariableExpression && u == null && isignature != null && u7 == null)
			{
				if (isignature.POUType == Operator.FunctionBlock)
				{
					this.AddError(\u0002._Left, MessageId.Err_FunctionBlockNeedsInstance, new object[]
					{
						isignature.OrgName
					});
				}
				if (isignature.POUType == Operator.Interface)
				{
					this.AddError(\u0002._Left, MessageId.Err_InterfaceNeedsInstance, new object[]
					{
						isignature.OrgName
					});
				}
				if (isignature.POUType == Operator.Type)
				{
					this.AddError(\u0002._Left, MessageId.Err_UnexpectedTypeName, new object[]
					{
						isignature.OrgName
					});
				}
			}
			if (itype2 == null || itype2.IsInteger)
			{
				ILiteralValue literalValue = \u0002._Right.Literal(this.Scope);
				bool flag = literalValue != null;
				if (isignature != null && \u0002._Right.GetVariable(this.Scope.\u0001(isignature)) != null)
				{
					flag = false;
				}
				if (flag && this.\u0001(\u0002, u, itype, isignature, literalValue))
				{
					return;
				}
			}
			_IUserdefType iuserdefType = itype.DeRefType as _IUserdefType;
			if (iuserdefType == null)
			{
				this.AddError(\u0002._Left, MessageId.Err_CompoAccessNoStruct, new object[]
				{
					\u0002._Left
				});
				if (u != null && u2 != null)
				{
					string u10 = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
					_ISourcePosition isourcePosition = u.SourcePosition as _ISourcePosition;
					isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(u2.LibraryPath), u2.ObjectGuid);
					this.\u0002(global::\u0019.\u0003.\u0001(isourcePosition, u10, Severity.Information, MessageId.Inf_RelatedPosition));
					return;
				}
			}
			else
			{
				if (\u0002._Left is ICallExpression || (this.\u0003(u) && !(\u0002._Left is IDeRefAccessExpression)))
				{
					this.\u0001(\u0002, u, u6, itype, iuserdefType, isignature);
					return;
				}
				if (u7 != null && u6 != null && this.TopOfStack.\u0005 != null && (u7.POUType == Operator.VarGlobal || u7.POUType == Operator.Program) && this.\u0001(this.TopOfStack.\u0005, u6))
				{
					this.AddError(\u0002._Right, MessageId.Err_AbstractPropertyStaticCall, new object[]
					{
						u6.OrgName
					});
					return;
				}
				if (itype2 == null)
				{
					_IVariableExpression ivariableExpression = \u0002._Right as _IVariableExpression;
					if ((ivariableExpression == null || !ivariableExpression.Name.Equals("FB_INIT", StringComparison.OrdinalIgnoreCase)) && !SimpleTypeChecker.\u0001(\u0002._Right))
					{
						this.AddError(\u0002._Right, MessageId.Err_NoComponentOf, new object[]
						{
							\u0002._Right,
							\u0002._Left
						});
					}
				}
			}
		}

		// Token: 0x06001F57 RID: 8023 RVA: 0x00069000 File Offset: 0x00067200
		private void \u0001(_ICompoAccessExpression \u0002, _IVariable \u0003, IVariable \u0004, IType \u0005, IUserdefType \u0006, _ISignature \u0007)
		{
			if (\u0003 != null && \u0003.IsProperty && \u0003.Type.Class == TypeClass.Reference)
			{
				return;
			}
			if (\u0005.Class != TypeClass.Reference && (\u0006 == null || (\u0007 != null && \u0007.POUType != Operator.Interface)))
			{
				this.AddError(\u0002._Left, MessageId.Err_NoCallInInstancePath, new object[]
				{
					\u0002._Left
				});
			}
		}

		// Token: 0x06001F58 RID: 8024 RVA: 0x00069068 File Offset: 0x00067268
		private global::\u0015.\u0002 \u0001(_IType \u0002, _ISignature \u0003, global::\u0015.\u0002 \u0004, global::\u0015.\u0002 \u0005, out _ISignature \u0006)
		{
			if (this.KeepAliases)
			{
				global::\u0015.\u0002 u;
				\u0002 = this.Scope.\u0001(\u0002, out u);
				IPrecompileScope7 precompileScope;
				\u0003 = (this.Scope.DetermineAliasBaseSignature(\u0003, out precompileScope) as _ISignature);
			}
			bool flag = \u0002 != null && \u0002.DeRefType.IsInteger;
			global::\u0015.\u0002 u2 = \u0005 ?? this.Scope;
			\u0006 = null;
			if (\u0002 != null && \u0002.DeRefType.Class == TypeClass.Userdef)
			{
				global::\u0015.\u0002 u3 = u2;
				\u0006 = global::\u001E.\u0006.\u0001(u3, (IUserdefType)\u0002.DeRefType);
				if (\u0006 == null && \u0003 != null && !string.IsNullOrEmpty(\u0003.LibraryPath))
				{
					global::\u0015.\u0002 u4 = u2.\u0002(\u0003);
					if (u4 != null)
					{
						u3 = u4;
					}
					\u0006 = (u3.FindSignature((IUserdefType)\u0002.DeRefType) as _ISignature);
				}
				this.TopOfStack.\u0002 = u3.\u0001(\u0006);
			}
			else if (!flag)
			{
				if (\u0003 != null)
				{
					this.TopOfStack.\u0002 = u2.\u0001(\u0003);
				}
				else if (\u0004 != null)
				{
					this.TopOfStack.\u0002 = \u0004;
				}
			}
			if (this.TopOfStack.\u0002 == null)
			{
				this.TopOfStack.\u0002 = u2;
			}
			else
			{
				u2 = this.TopOfStack.\u0002;
			}
			this.\u0001(\u0003, flag);
			return u2;
		}

		// Token: 0x06001F59 RID: 8025 RVA: 0x000691A0 File Offset: 0x000673A0
		private void \u0001(_ISignature \u0002, bool \u0003)
		{
			if (\u0002 != null && !\u0003)
			{
				CheckerScope checkerScope = this.TopOfStack.\u0002 as CheckerScope;
				if (checkerScope != null)
				{
					checkerScope.LocalScope = true;
				}
			}
		}

		// Token: 0x06001F5A RID: 8026 RVA: 0x000691D0 File Offset: 0x000673D0
		private bool \u0001(_ICompoAccessExpression \u0002, _IVariable \u0003, _IType \u0004, ISignature \u0005, ILiteralValue \u0006)
		{
			if (\u0004 == null)
			{
				return true;
			}
			bool flag = \u0004.DeRefType.IsInteger;
			if (\u0005 != null && \u0005.GetFlag(SignatureFlag.Enum))
			{
				flag = true;
			}
			if (\u0005 != null && \u0005.GetFlag(SignatureFlag.Alias))
			{
				flag = true;
			}
			if (\u0002.Right is ILiteralExpression && !flag)
			{
				this.AddError(\u0002._Right, MessageId.Err_BitAccessOnlyOnInt, Array.Empty<object>());
			}
			if (!flag || \u0006 == null)
			{
				return false;
			}
			this.\u0001(null, TypeTable.Bool);
			bool flag2;
			int @int = \u0006.GetInt(out flag2);
			if (!flag2 || (\u0006.KindOf != KindOfLiteral.SignedInteger && \u0006.KindOf != KindOfLiteral.UnsignedInteger))
			{
				this.AddError(\u0002._Right, MessageId.Err_Bitaccessnoconst, Array.Empty<object>());
			}
			if ((\u0002.Left is ICallExpression || this.\u0003(\u0003)) && !this.\u0002(\u0003))
			{
				this.AddError(\u0002._Left, MessageId.Err_NoBitAccessOnFunctionCall, Array.Empty<object>());
				return true;
			}
			int num = this.Scope.GetSize(\u0004) * 8;
			if (@int >= num && num > 0)
			{
				this.AddError(\u0002._Right, MessageId.Err_BitNrOverflow, new object[]
				{
					\u0002._Right.ToString(),
					\u0002.Left.ToString()
				});
			}
			this.\u0001(this.Scope, TypeTable.Bit);
			return true;
		}

		// Token: 0x06001F5B RID: 8027 RVA: 0x00069310 File Offset: 0x00067510
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			this.\u0001();
			\u0002._Base.Accept(this);
			_IType itype = this.TopOfStack.typeResolved;
			global::\u0015.\u0002 u = this.TopOfStack.\u0005;
			_IVariable u2 = this.TopOfStack.\u0001;
			_ISignature u3 = this.TopOfStack.\u0001;
			this.\u0002();
			if (itype is _IPointerType)
			{
				this.TopOfStack.\u0005 = (u ?? this.Scope);
				this.\u0001(u, (itype as _IPointerType)._Base);
			}
			else if (itype != null)
			{
				this.AddError(\u0002._Base, MessageId.Err_DerefNoPointer, Array.Empty<object>());
			}
			this.TopOfStack.\u0001 = u2;
			this.TopOfStack.\u0001 = u3;
		}

		// Token: 0x06001F5C RID: 8028 RVA: 0x000693C8 File Offset: 0x000675C8
		public void \u0001(_ICopyScopeExpression \u0002)
		{
		}

		// Token: 0x06001F5D RID: 8029 RVA: 0x000693CC File Offset: 0x000675CC
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			this.\u0001();
			this.TopOfStack.\u0002 = this.Scope.\u0001();
			this.TopOfStack.\u0001 = VarFlag.Global;
			\u0002._Base.Accept(this);
			_IType u = this.TopOfStack.typeResolved;
			_ISignature u2 = this.TopOfStack.\u0001;
			_IVariable u3 = this.TopOfStack.\u0001;
			global::\u0015.\u0002 u4 = this.TopOfStack.\u0004;
			VarFlag u5 = this.TopOfStack.\u0001;
			this.\u0002();
			this.\u0001(null, u);
			this.TopOfStack.\u0001 = u2;
			this.TopOfStack.\u0001 = u3;
			this.TopOfStack.\u0004 = u4;
			this.TopOfStack.\u0001 = u5;
		}

		// Token: 0x06001F5E RID: 8030 RVA: 0x00069490 File Offset: 0x00067690
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			CheckerScope u = new CheckerScope(null, APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext, null);
			this.\u0001(u, this.TopOfStack.\u0001);
			\u0002._Base.Accept(this);
			_IType u2 = this.TopOfStack.typeResolved;
			_ISignature u3 = this.TopOfStack.\u0001;
			_IVariable u4 = this.TopOfStack.\u0001;
			global::\u0015.\u0002 u5 = this.TopOfStack.\u0004;
			this.\u0002();
			this.\u0001(null, u2);
			this.TopOfStack.\u0001 = u3;
			this.TopOfStack.\u0001 = u4;
			this.TopOfStack.\u0004 = u5;
		}

		// Token: 0x06001F5F RID: 8031 RVA: 0x00069538 File Offset: 0x00067738
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			CheckerScope u = new CheckerScope(null, APEnvironmentFacade.Instance.LanguageModelMgr.Pool, null);
			this.\u0001(u, this.TopOfStack.\u0001);
			\u0002._Base.Accept(this);
			_IType u2 = this.TopOfStack.typeResolved;
			_ISignature u3 = this.TopOfStack.\u0001;
			_IVariable u4 = this.TopOfStack.\u0001;
			global::\u0015.\u0002 u5 = this.TopOfStack.\u0004;
			global::\u0015.\u0002 u6 = this.TopOfStack.\u0005;
			this.\u0002();
			this.\u0001(null, u2);
			this.TopOfStack.\u0001 = u3;
			this.TopOfStack.\u0001 = u4;
			this.TopOfStack.\u0004 = u5;
			this.TopOfStack.\u0005 = u6;
		}

		// Token: 0x06001F60 RID: 8032 RVA: 0x000695F8 File Offset: 0x000677F8
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			global::\u0015.\u0002 u = this.Scope.\u0002(\u0002._Namespace);
			if (u != null)
			{
				this.\u0001(u, this.TopOfStack.\u0001);
				\u0002._Access.Accept(this);
				_IType u2 = this.TopOfStack.typeResolved;
				_ISignature u3 = this.TopOfStack.\u0001;
				_IVariable u4 = this.TopOfStack.\u0001;
				global::\u0015.\u0002 u5 = this.TopOfStack.\u0004;
				this.\u0002();
				this.\u0001(null, u2);
				this.TopOfStack.\u0001 = u3;
				this.TopOfStack.\u0001 = u4;
				this.TopOfStack.\u0004 = u5;
				this.TopOfStack.\u0005 = u;
			}
		}

		// Token: 0x06001F61 RID: 8033 RVA: 0x000696AC File Offset: 0x000678AC
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06001F62 RID: 8034 RVA: 0x000696B0 File Offset: 0x000678B0
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			\u0002._Low.Accept(this);
			\u0002._High.Accept(this);
		}

		// Token: 0x06001F63 RID: 8035 RVA: 0x000696CC File Offset: 0x000678CC
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06001F64 RID: 8036 RVA: 0x00069718 File Offset: 0x00067918
		public void \u0001(_ICaseStatement \u0002)
		{
			\u0002._Switch.Accept(this);
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				this.\u0001(true);
				icase._Controlled.Accept(this);
				this.\u0002();
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
			ICompiledType type = \u0002._Switch.Type;
		}

		// Token: 0x06001F65 RID: 8037 RVA: 0x000697B0 File Offset: 0x000679B0
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x06001F66 RID: 8038 RVA: 0x000697B4 File Offset: 0x000679B4
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06001F67 RID: 8039 RVA: 0x000697B8 File Offset: 0x000679B8
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x06001F68 RID: 8040 RVA: 0x000697BC File Offset: 0x000679BC
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06001F69 RID: 8041 RVA: 0x000697C0 File Offset: 0x000679C0
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06001F6A RID: 8042 RVA: 0x000697C4 File Offset: 0x000679C4
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001F6B RID: 8043 RVA: 0x000697C8 File Offset: 0x000679C8
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06001F6C RID: 8044 RVA: 0x000697CC File Offset: 0x000679CC
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001F6D RID: 8045 RVA: 0x000697D0 File Offset: 0x000679D0
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001F6E RID: 8046 RVA: 0x000697D4 File Offset: 0x000679D4
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001F6F RID: 8047 RVA: 0x000697D8 File Offset: 0x000679D8
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06001F70 RID: 8048 RVA: 0x000697DC File Offset: 0x000679DC
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x06001F71 RID: 8049 RVA: 0x000697E0 File Offset: 0x000679E0
		public void \u0001(_IVariableReference \u0002)
		{
			this.\u0001(this.TopOfStack.\u0001);
			this.TopOfStack.\u0001 = AccessFlag.Read;
			\u0002.InstancePath.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001F72 RID: 8050 RVA: 0x00069814 File Offset: 0x00067A14
		public void \u0001(_ITypeReference \u0002)
		{
			ISignature[] array = this.Scope.FindSignature(\u0002.InstancePath);
			if (array != null && array.Length == 1 && array[0] != null)
			{
				\u0002.InstancePath.PrecompileSignatureId = (array[0] as _ISignature).PrecompileId;
			}
		}

		// Token: 0x06001F73 RID: 8051 RVA: 0x0006985C File Offset: 0x00067A5C
		public void \u0001(_IPouReference \u0002)
		{
			ISignature[] array = this.Scope.FindSignature(\u0002.InstancePath);
			if (array != null && array.Length == 1)
			{
				if (array[0] != null)
				{
					\u0002.InstancePath.PrecompileSignatureId = (array[0] as _ISignature).PrecompileId;
				}
			}
			else if (\u0002.InstancePath is ICompoAccessExpression)
			{
				_ICompoAccessExpression icompoAccessExpression = (_ICompoAccessExpression)\u0002.InstancePath;
				ISignature[] array2 = this.Scope.FindSignature(icompoAccessExpression.Left);
				if (array2 != null && array2.Length != 0)
				{
					ISignature signature = array2[0];
					if (signature != null)
					{
						ISignature signature2 = CheckerScope.\u0001(this.PointerSize, this.\u0001._GetLibraryTable(), signature as _ISignature, this.\u0001, APEnvironmentFacade.Instance.LanguageModelMgr.Pool).FindSignatureLocal(icompoAccessExpression.Right.ToString()) as _ISignature;
						if (signature2 != null)
						{
							\u0002.InstancePath.PrecompileSignatureId = (signature2 as _ISignature).PrecompileId;
						}
					}
				}
			}
			this.\u0001(this.TopOfStack.\u0001);
			this.TopOfStack.\u0001 = AccessFlag.Read;
			this.TopOfStack.\u0002 = true;
			\u0002.InstancePath.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001F74 RID: 8052 RVA: 0x00069984 File Offset: 0x00067B84
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x06001F75 RID: 8053 RVA: 0x00069988 File Offset: 0x00067B88
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06001F76 RID: 8054 RVA: 0x0006998C File Offset: 0x00067B8C
		public void \u0001(_IDefinedExpression \u0002)
		{
			this.\u0001(this.TopOfStack.\u0001);
			this.TopOfStack.\u0001 = AccessFlag.Read;
			\u0002.ItemReference.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001F77 RID: 8055 RVA: 0x000699C0 File Offset: 0x00067BC0
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x06001F78 RID: 8056 RVA: 0x000699C4 File Offset: 0x00067BC4
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			this.\u0001(IgnoreCheckerMessageFlags.Information);
			if (\u0002.Condition != null)
			{
				\u0002.Condition.Accept(this);
			}
			if (\u0002.IfThen != null)
			{
				\u0002.IfThen.Accept(this);
			}
			if (\u0002.ElseIf != null)
			{
				foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
				{
					if (ipragmaElseIf.Condition != null)
					{
						ipragmaElseIf.Condition.Accept(this);
					}
					if (ipragmaElseIf.Controlled != null)
					{
						ipragmaElseIf.Controlled.Accept(this);
					}
				}
			}
			if (\u0002.IfElseStatement != null)
			{
				((_IStatement)\u0002.IfElseStatement).Accept(this);
			}
			this.\u0002();
		}

		// Token: 0x06001F79 RID: 8057 RVA: 0x00069A88 File Offset: 0x00067C88
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06001F7A RID: 8058 RVA: 0x00069A8C File Offset: 0x00067C8C
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06001F7B RID: 8059 RVA: 0x00069A90 File Offset: 0x00067C90
		public void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x06001F7C RID: 8060 RVA: 0x00069A94 File Offset: 0x00067C94
		public void \u0001(_IHasTypeExpression \u0002)
		{
			this.\u0001(this.TopOfStack.\u0001);
			this.TopOfStack.\u0001 = AccessFlag.Read;
			\u0002.Variable.Accept(this);
			this.\u0002();
			this.\u0001(\u0002.ReferencedType as _IUserdefType);
		}

		// Token: 0x06001F7D RID: 8061 RVA: 0x00069AE4 File Offset: 0x00067CE4
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x06001F7E RID: 8062 RVA: 0x00069AE8 File Offset: 0x00067CE8
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			this.\u0001(this.TopOfStack.\u0001);
			this.TopOfStack.\u0001 = AccessFlag.Read;
			\u0002.ItemReference.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001F7F RID: 8063 RVA: 0x00069B1C File Offset: 0x00067D1C
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x06001F80 RID: 8064 RVA: 0x00069B20 File Offset: 0x00067D20
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
			this.\u0001(this.TopOfStack.\u0001);
			this.TopOfStack.\u0001 = AccessFlag.Read;
			\u0002._Constant.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001F81 RID: 8065 RVA: 0x00069B54 File Offset: 0x00067D54
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
			this.\u0001(this.TopOfStack.\u0001);
			this.TopOfStack.\u0001 = AccessFlag.Read;
			\u0002._Constant.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001F82 RID: 8066 RVA: 0x00069B88 File Offset: 0x00067D88
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x06001F83 RID: 8067 RVA: 0x00069B8C File Offset: 0x00067D8C
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x06001F84 RID: 8068 RVA: 0x00069B90 File Offset: 0x00067D90
		public void \u0001(_ICastExpression \u0002)
		{
			\u0002.BaseExpression.Accept(this);
			this.\u0001();
			_IType u = null;
			if (\u0002.ExprWithType != null)
			{
				\u0002.ExpWithType.Accept(this);
				u = this.TopOfStack.typeResolved;
			}
			if (\u0002.ExplicitelySpecifiedType != null)
			{
				u = (\u0002.ExplicitelySpecifiedType as _IType);
			}
			this.\u0002();
			this.\u0001(this.Scope, u);
		}

		// Token: 0x06001F85 RID: 8069 RVA: 0x00069BF8 File Offset: 0x00067DF8
		public void \u0001(_INewExpression \u0002)
		{
			this.\u0001();
			\u0002._Count.Accept(this);
			this.\u0001(\u0002._Count, \u0002._Count, this.TopOfStack.typeResolved, TypeTable.AnyInt);
			this.\u0001(\u0002.TypeToCreate as _IUserdefType);
			if (\u0002._FBInitParams != null)
			{
				_IUserdefType iuserdefType = \u0002.TypeToCreate as _IUserdefType;
				if (iuserdefType != null)
				{
					_ISignature u = this.\u0001(iuserdefType);
					_ISignature u2 = this.Scope.\u0001(u).FindSignatureLocal(IdentifierConstants.InitMethodName) as _ISignature;
					global::\u0015.\u0002 u3 = this.Scope.\u0001(u2);
					foreach (_IAssignmentExpression iassignmentExpression in \u0002._FBInitParams.OfType<_IAssignmentExpression>())
					{
						_IType u4 = null;
						if (iassignmentExpression._LValue != null)
						{
							this.\u0001(AccessFlag.Write);
							this.TopOfStack.\u0002 = u3;
							iassignmentExpression._LValue.Accept(this);
							u4 = this.TopOfStack.typeResolved;
							this.\u0002();
						}
						this.\u0001(AccessFlag.Read);
						this.TopOfStack.\u0001 = u4;
						if (iassignmentExpression._RValue != null)
						{
							iassignmentExpression._RValue.Accept(this);
						}
						this.\u0002();
					}
				}
			}
			this.\u0002();
			this.\u0001(this.Scope, global::\u0019.\u0003.\u0001(\u0002._TypeToCast));
		}

		// Token: 0x06001F86 RID: 8070 RVA: 0x00069D6C File Offset: 0x00067F6C
		public void \u0001(_ITypeExpression \u0002)
		{
			IGenericUserdefType genericUserdefType = \u0002.Type as IGenericUserdefType;
			if (genericUserdefType != null)
			{
				this.\u0001((_IExpression)genericUserdefType.NameExpression);
				foreach (_IExpression u in genericUserdefType.GenericConstantsInitializations)
				{
					this.\u0001(u);
				}
			}
		}

		// Token: 0x06001F87 RID: 8071 RVA: 0x00069DDC File Offset: 0x00067FDC
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			this.\u0001();
			\u0002._Left.Accept(this);
			_IType itype = this.TopOfStack.typeResolved;
			_IVariable u = this.TopOfStack.\u0001;
			_ISignature u2 = this.TopOfStack.\u0001;
			this.\u0002();
			if (itype != null)
			{
				_IType u3 = (_IType)global::\u0082.\u0007.\u0001(\u0002, itype, new \u0082.\u0007.\u0001(this.AddError), this.Scope);
				this.\u0001(this.Scope, u3);
				this.TopOfStack.\u0001 = u;
				this.TopOfStack.\u0001 = u2;
			}
		}

		// Token: 0x06001F88 RID: 8072 RVA: 0x00069E70 File Offset: 0x00068070
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
		}

		// Token: 0x170005F1 RID: 1521
		// (get) Token: 0x06001F89 RID: 8073 RVA: 0x00069E74 File Offset: 0x00068074
		public bool ConvertAllTypeMismatches
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005F2 RID: 1522
		// (get) Token: 0x06001F8A RID: 8074 RVA: 0x00069E78 File Offset: 0x00068078
		public bool InImplicitCode
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005F3 RID: 1523
		// (get) Token: 0x06001F8B RID: 8075 RVA: 0x00069E7C File Offset: 0x0006807C
		public Guid ApplicationGuid
		{
			get
			{
				return this.\u0001.ApplicationGuid;
			}
		}

		// Token: 0x170005F4 RID: 1524
		// (get) Token: 0x06001F8C RID: 8076 RVA: 0x00069E8C File Offset: 0x0006808C
		public bool TreatLRealAsReal
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005F5 RID: 1525
		// (get) Token: 0x06001F8D RID: 8077 RVA: 0x00069E90 File Offset: 0x00068090
		public bool TreatInt64AsInt32
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005F6 RID: 1526
		// (get) Token: 0x06001F8E RID: 8078 RVA: 0x00069E94 File Offset: 0x00068094
		public bool NoConversionChecks
		{
			get
			{
				return this.\u0001.IsDefined("NO_3_0_CONVERSION_CHECKS");
			}
		}

		// Token: 0x06001F8F RID: 8079 RVA: 0x00069EA8 File Offset: 0x000680A8
		public bool \u0001(_IImplicitConversionExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001F90 RID: 8080 RVA: 0x00069EAC File Offset: 0x000680AC
		private bool \u0001(_IExpression \u0002, ICompiledType \u0003, ICompiledType \u0004, global::\u0015.\u0002 \u0005, global::\u0015.\u0002 \u0006)
		{
			if (\u0004 != null && \u0004.DeRefType.Class == TypeClass.Userdef)
			{
				_ISignature isignature = ((\u0006 != null) ? \u0006.FindSignature(\u0004 as IUserdefType) : null) as _ISignature;
				if (isignature != null && isignature.GetFlag(SignatureFlag.Enum))
				{
					bool flag = global::\u000E.\u000F.\u0001(\u0005, \u0003, \u0004);
					if (!(\u0003 is _IReferenceType) && !flag && !global::\u000E.\u000F.\u0001(\u0002, isignature, \u0006))
					{
						this.AddError(\u0002, MessageId.Err_StrictEnumNotAMember, new object[]
						{
							\u0002.ToString(),
							isignature.OrgName
						});
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06001F91 RID: 8081 RVA: 0x00069F3C File Offset: 0x0006813C
		public virtual bool \u0001(_IExprement \u0002, _IExpression \u0003, _IExpression \u0004, ICompiledType \u0005, ICompiledType \u0006, global::\u0015.\u0002 \u0007, global::\u0015.\u0002 \u0008, bool \u000E, out bool \u000F)
		{
			\u000F = false;
			if (\u0005 == null || \u0006 == null)
			{
				return true;
			}
			if (\u0005.Class == TypeClass.Lazy || \u0006.Class == TypeClass.Lazy)
			{
				return true;
			}
			if (!this.\u0001(\u0003, \u0005, \u0006, \u0007, \u0008))
			{
				return false;
			}
			if (\u0005.DeRefType.Class == TypeClass.Userdef && \u0006.DeRefType.Class == TypeClass.Userdef)
			{
				_IUserdefType iuserdefType = \u0005.DeRefType as _IUserdefType;
				_IUserdefType iuserdefType2 = \u0006.DeRefType as _IUserdefType;
				if (iuserdefType != null && iuserdefType.SignatureId >= 0 && iuserdefType2 != null && iuserdefType.SignatureId == iuserdefType2.SignatureId)
				{
					return true;
				}
			}
			if (\u0005.DeRefType.Class == TypeClass.Userdef || \u0006.DeRefType.Class == TypeClass.Userdef)
			{
				return global::\u0006.\u0011.\u0008(\u0005, \u0006, \u0007, \u0008);
			}
			if (\u0005.DeRefType.Class == TypeClass.XWord || \u0006.DeRefType.Class == TypeClass.XWord)
			{
				return true;
			}
			if (\u0006.DeRefType.Class == TypeClass.XString || \u0005.DeRefType.Class == TypeClass.XString)
			{
				return true;
			}
			if (\u0005.DeRefType.Class == TypeClass.Array && (\u0005.DeRefType as _IArrayType)._Base.Class == TypeClass.Userdef)
			{
				return true;
			}
			if (\u0006.DeRefType.Class == TypeClass.Array && (\u0006.DeRefType as _IArrayType)._Base.Class == TypeClass.Userdef)
			{
				return true;
			}
			if (\u0005.DeRefType.Class == TypeClass.Lazy)
			{
				return true;
			}
			if (\u0006.DeRefType.Class == TypeClass.Lazy)
			{
				return true;
			}
			if (\u000E && SimpleTypeChecker.\u0001(\u0003, this.Scope))
			{
				return true;
			}
			_IExpression iexpression = \u0003.Duplicate() as _IExpression;
			return global::\u000E.\u000F.\u0001(this, \u0002, \u0005, \u0006, \u0007, \u0008, ref iexpression, out \u000F);
		}

		// Token: 0x06001F92 RID: 8082 RVA: 0x0006A0FC File Offset: 0x000682FC
		private static bool \u0001(_IExpression \u0002, global::\u0015.\u0002 \u0003)
		{
			if (\u0002.IsLiteral)
			{
				bool flag;
				int @int = \u0002.Literal(\u0003).GetInt(out flag);
				if (flag && @int == 0)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001F93 RID: 8083 RVA: 0x0006A12C File Offset: 0x0006832C
		private static bool \u0002(_IExpression \u0002, global::\u0015.\u0002 \u0003)
		{
			ILiteralValue literalValue = \u0003.GetLiteralValue(\u0002, true);
			int num;
			return literalValue != null && literalValue.GetInt(out num) && num == 0;
		}

		// Token: 0x06001F94 RID: 8084 RVA: 0x0006A158 File Offset: 0x00068358
		public bool \u0001(_IExprement \u0002, _IExpression \u0003, ICompiledType \u0004, ICompiledType \u0005)
		{
			return this.\u0001(\u0002, \u0003, null, \u0004, \u0005);
		}

		// Token: 0x06001F95 RID: 8085 RVA: 0x0006A168 File Offset: 0x00068368
		public bool \u0001(_IExprement \u0002, _IExpression \u0003, _IExpression \u0004, ICompiledType \u0005, ICompiledType \u0006)
		{
			bool flag;
			return this.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, this.Scope, this.Scope, false, out flag);
		}

		// Token: 0x06001F96 RID: 8086 RVA: 0x0006A194 File Offset: 0x00068394
		public void \u0002(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001F97 RID: 8087 RVA: 0x0006A198 File Offset: 0x00068398
		public void \u0003(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001F98 RID: 8088 RVA: 0x0006A19C File Offset: 0x0006839C
		public void \u0004(_IOperatorExpression \u0002)
		{
			if (this.TopOfStack.\u0001.\u0001.Class == TypeClass.Userdef || this.TopOfStack.\u0001.\u0001.Class == TypeClass.Array || this.TopOfStack.\u0001.\u0001.Class == TypeClass.String)
			{
				this.AddError(\u0002, MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					\u0002.Code,
					this.TopOfStack.\u0001.\u0001
				});
			}
		}

		// Token: 0x06001F99 RID: 8089 RVA: 0x0006A224 File Offset: 0x00068424
		public void \u0005(_IOperatorExpression \u0002)
		{
			for (int i = 0; i < this.TopOfStack.\u0001.\u0001.Count; i++)
			{
				_IExpression iexpression = this.TopOfStack.\u0001.\u0001[i];
				_IType u = this.TopOfStack.\u0001.\u0001[i];
				this.\u0001(iexpression, iexpression, u, TypeTable.LReal);
			}
		}

		// Token: 0x06001F9A RID: 8090 RVA: 0x0006A290 File Offset: 0x00068490
		public void \u0006(_IOperatorExpression \u0002)
		{
			this.\u0005(\u0002);
		}

		// Token: 0x06001F9B RID: 8091 RVA: 0x0006A29C File Offset: 0x0006849C
		public void \u0007(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001F9C RID: 8092 RVA: 0x0006A2A0 File Offset: 0x000684A0
		public void \u0008(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001F9D RID: 8093 RVA: 0x0006A2A4 File Offset: 0x000684A4
		public void \u000E(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001F9E RID: 8094 RVA: 0x0006A2A8 File Offset: 0x000684A8
		public void \u000F(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001F9F RID: 8095 RVA: 0x0006A2AC File Offset: 0x000684AC
		public void \u0010(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FA0 RID: 8096 RVA: 0x0006A2B0 File Offset: 0x000684B0
		public void \u0011(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FA1 RID: 8097 RVA: 0x0006A2B4 File Offset: 0x000684B4
		public void \u0012(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FA2 RID: 8098 RVA: 0x0006A2B8 File Offset: 0x000684B8
		public void \u0013(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FA3 RID: 8099 RVA: 0x0006A2BC File Offset: 0x000684BC
		public void \u0014(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FA4 RID: 8100 RVA: 0x0006A2C0 File Offset: 0x000684C0
		public void \u0015(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FA5 RID: 8101 RVA: 0x0006A2C4 File Offset: 0x000684C4
		public void \u0016(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FA6 RID: 8102 RVA: 0x0006A2C8 File Offset: 0x000684C8
		public void \u0017(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FA7 RID: 8103 RVA: 0x0006A2CC File Offset: 0x000684CC
		public void \u0018(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FA8 RID: 8104 RVA: 0x0006A2D0 File Offset: 0x000684D0
		public void \u0019(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FA9 RID: 8105 RVA: 0x0006A2D4 File Offset: 0x000684D4
		public void \u001A(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FAA RID: 8106 RVA: 0x0006A2D8 File Offset: 0x000684D8
		public void \u001B(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FAB RID: 8107 RVA: 0x0006A2DC File Offset: 0x000684DC
		public void \u001C(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FAC RID: 8108 RVA: 0x0006A2E0 File Offset: 0x000684E0
		public void \u001D(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FAD RID: 8109 RVA: 0x0006A2E4 File Offset: 0x000684E4
		public void \u001E(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FAE RID: 8110 RVA: 0x0006A2E8 File Offset: 0x000684E8
		public void \u001F(_IOperatorExpression \u0002)
		{
			this.\u0001(\u0002[0], \u0002[0], this.TopOfStack.\u0001.\u0001[0], TypeTable.DWord);
			this.\u0001(\u0002[1], \u0002[1], this.TopOfStack.\u0001.\u0001[1], TypeTable.Bool);
		}

		// Token: 0x06001FAF RID: 8111 RVA: 0x0006A358 File Offset: 0x00068558
		public void \u007F(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FB0 RID: 8112 RVA: 0x0006A35C File Offset: 0x0006855C
		public void \u0080(_IOperatorExpression \u0002)
		{
			this.\u0081(\u0002);
		}

		// Token: 0x06001FB1 RID: 8113 RVA: 0x0006A368 File Offset: 0x00068568
		public void \u0081(_IOperatorExpression \u0002)
		{
			IList<_IExpression> u = this.TopOfStack.\u0001.\u0001;
			IList<_IType> u2 = this.TopOfStack.\u0001.\u0001;
			if (1 != u.Count)
			{
				return;
			}
			if (u[0] is _ILiteralExpression)
			{
				_ILiteralExpression iliteralExpression = u[0] as _ILiteralExpression;
				if (iliteralExpression.ConstantType == TypeClass.String || iliteralExpression.ConstantType == TypeClass.WString)
				{
					return;
				}
			}
			_IVariable ivariable = null;
			if (this.TopOfStack.\u0001.\u0001.Count > 0)
			{
				ivariable = this.TopOfStack.\u0001.\u0001[0];
			}
			if (ivariable != null && this.\u0003(ivariable))
			{
				this.AddError(u[0], MessageId.Err_WrongTypeForAdr, new object[]
				{
					u[0]
				});
			}
			if (u2[0].Class != TypeClass.Bit)
			{
				if (u2[0] != null && u2[0] is ISpecialSizeType && !(u2[0] is _IBool16Type))
				{
					this.AddError(u[0], MessageId.Err_WrongTypeForAdr, new object[]
					{
						u[0]
					});
					return;
				}
				if (TypeCheckerVisitor.\u0001(u[0], (global::\u0017.\u0006)this.Scope, false))
				{
					this.AddError(u[0], MessageId.Err_WrongTypeForAdr, new object[]
					{
						u[0]
					});
				}
			}
		}

		// Token: 0x06001FB2 RID: 8114 RVA: 0x0006A4C8 File Offset: 0x000686C8
		public void \u0082(_IOperatorExpression \u0002)
		{
			this.TopOfStack.\u0002 = true;
		}

		// Token: 0x06001FB3 RID: 8115 RVA: 0x0006A4D8 File Offset: 0x000686D8
		public void \u0083(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FB4 RID: 8116 RVA: 0x0006A4DC File Offset: 0x000686DC
		public void \u0084(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FB5 RID: 8117 RVA: 0x0006A4E0 File Offset: 0x000686E0
		public void \u0086(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FB6 RID: 8118 RVA: 0x0006A4E4 File Offset: 0x000686E4
		public void \u0087(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FB7 RID: 8119 RVA: 0x0006A4E8 File Offset: 0x000686E8
		public void \u0088(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FB8 RID: 8120 RVA: 0x0006A4EC File Offset: 0x000686EC
		public void \u0089(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FB9 RID: 8121 RVA: 0x0006A4F0 File Offset: 0x000686F0
		public void \u008A(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FBA RID: 8122 RVA: 0x0006A4F4 File Offset: 0x000686F4
		public void \u008B(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FBB RID: 8123 RVA: 0x0006A4F8 File Offset: 0x000686F8
		public void \u008C(_IOperatorExpression \u0002)
		{
			for (int i = 0; i < this.TopOfStack.\u0001.\u0001.Count; i++)
			{
				_IExpression iexpression = \u0002[i];
				_IType u = this.TopOfStack.\u0001.\u0001[i];
				if (i == 0 && iexpression is ILiteralExpression && !TypeTable.IsConcreteType((iexpression as ILiteralExpression).ConstantType))
				{
					this.\u0001(iexpression, iexpression, u, TypeTable.UDInt);
				}
				else
				{
					this.\u0001(iexpression, iexpression, u, TypeTable.AnyInt);
				}
			}
			if ((\u0002.Code == Operator.Shl || \u0002.Code == Operator.Shr) && this.TopOfStack.\u0001.\u0001.Count == 2)
			{
				ILiteralValue literalValue = this.TopOfStack.\u0001.\u0001[1].Literal(this.Scope);
				int num;
				if (literalValue != null && literalValue.GetInt(out num))
				{
					int size = this.Scope.GetSize(this.TopOfStack.\u0001.\u0001[0]);
					if (size > 0 && num >= size * 8)
					{
						this.\u0001(\u0002, MessageId.Wrn_ShiftExceedsTypeSize, new object[]
						{
							num,
							this.TopOfStack.\u0001.\u0001[0].ToString()
						});
					}
				}
			}
		}

		// Token: 0x06001FBC RID: 8124 RVA: 0x0006A650 File Offset: 0x00068850
		public void \u008D(_IOperatorExpression \u0002)
		{
			for (int i = 0; i < this.TopOfStack.\u0001.\u0001.Count; i++)
			{
				_IExpression iexpression = \u0002[i];
				_IType itype = this.TopOfStack.\u0001.\u0001[i];
				if (i == 0)
				{
					bool flag = false;
					if (itype.Class == TypeClass.Pointer && (itype as _IPointerType).BaseType.Class == TypeClass.DInt)
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
					this.\u0001(iexpression, iexpression, itype, TypeTable.AnyInt);
				}
			}
		}

		// Token: 0x06001FBD RID: 8125 RVA: 0x0006A700 File Offset: 0x00068900
		public void \u008E(_IOperatorExpression \u0002)
		{
			int size = TypeTable.GetSize(TypeClass.Pointer, this.Scope as IScope);
			_IPointerType ipointerType = global::\u0019.\u0003.\u0001(TypeTable.DWord);
			if (size > 4)
			{
				ipointerType._Base = TypeTable.LWord;
			}
			_IPointerType type = global::\u0019.\u0003.\u0001(TypeTable.XWord);
			for (int i = 0; i < this.TopOfStack.\u0001.\u0001.Count; i++)
			{
				_IExpression iexpression = \u0002[i];
				_IType itype = this.TopOfStack.\u0001.\u0001[i];
				if (i == 0)
				{
					if (!itype.IsEqual(ipointerType) && !itype.IsEqual(type))
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
					_IExpression value = iexpression;
					this.\u0001(iexpression, iexpression, itype, TypeTable.XWord);
					\u0002[i] = value;
				}
			}
		}

		// Token: 0x06001FBE RID: 8126 RVA: 0x0006A7FC File Offset: 0x000689FC
		public void \u008F(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FBF RID: 8127 RVA: 0x0006A800 File Offset: 0x00068A00
		public void \u0090(_IOperatorExpression \u0002)
		{
			for (int i = 0; i < this.TopOfStack.\u0001.\u0001.Count; i++)
			{
				_IExpression iexpression = this.TopOfStack.\u0001.\u0001[i];
				_IType u = this.TopOfStack.\u0001.\u0001[i];
				this.\u0001(iexpression, iexpression, u, TypeTable.AnyNum);
			}
		}

		// Token: 0x06001FC0 RID: 8128 RVA: 0x0006A86C File Offset: 0x00068A6C
		private bool \u0001(_IType \u0002)
		{
			IList<_IExpression> u = this.TopOfStack.\u0001.\u0001;
			IList<_IType> u2 = this.TopOfStack.\u0001.\u0001;
			if (u.Count != 2)
			{
				return false;
			}
			ICompiledType u3 = u2[0];
			ICompiledType u4 = u2[1];
			if (this.\u0001(u3))
			{
				if (u[0] is ICallExpression)
				{
					this.AddError(u[0], MessageId.Err_NoCallInInterfaceComparison, Array.Empty<object>());
				}
				if (SimpleTypeChecker.\u0001(u[1], this.Scope) || SimpleTypeChecker.\u0002(u[1], this.Scope))
				{
					this.\u0001(u[1], u[1], u2[1], \u0002);
					return true;
				}
			}
			if (this.\u0001(u4))
			{
				if (u[1] is ICallExpression)
				{
					this.AddError(u[1], MessageId.Err_NoCallInInterfaceComparison, Array.Empty<object>());
				}
				if (SimpleTypeChecker.\u0001(u[0], this.Scope) || SimpleTypeChecker.\u0002(u[0], this.Scope))
				{
					this.\u0001(u[0], u[0], u2[0], \u0002);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001FC1 RID: 8129 RVA: 0x0006A9A4 File Offset: 0x00068BA4
		private bool \u0002(_IType \u0002)
		{
			IList<_IExpression> u = this.TopOfStack.\u0001.\u0001;
			IList<_IType> u2 = this.TopOfStack.\u0001.\u0001;
			if (u.Count != 2)
			{
				return false;
			}
			if (u2[0].Class == TypeClass.Pointer && SimpleTypeChecker.\u0001(u[1], this.Scope))
			{
				this.\u0001(u[1], u[1], u2[1], \u0002);
				return true;
			}
			if (u2[1].Class == TypeClass.Pointer && SimpleTypeChecker.\u0001(u[0], this.Scope))
			{
				this.\u0001(u[0], u[0], u2[0], \u0002);
				return true;
			}
			return false;
		}

		// Token: 0x06001FC2 RID: 8130 RVA: 0x0006AA68 File Offset: 0x00068C68
		public void \u0091(_IOperatorExpression \u0002)
		{
			IList<_IExpression> u = this.TopOfStack.\u0001.\u0001;
			IList<_IType> u2 = this.TopOfStack.\u0001.\u0001;
			if (u.Count != 2)
			{
				return;
			}
			ICompiledType compiledType = u2[0];
			ICompiledType compiledType2 = u2[1];
			_IType u3 = this.Scope.\u0001();
			if ((OperatorService.IsEqualOp(\u0002.Code) || OperatorService.IsNotEqualOp(\u0002.Code)) && this.\u0001(u3))
			{
				return;
			}
			if (!OperatorService.IsMinMaxOp(\u0002.Code) && this.\u0002(u3))
			{
				return;
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
					if (u[1] is _ILiteralExpression)
					{
						if (!global::\u0006.\u0011.\u0001(u[1] as _ILiteralExpression, compiledType2, compiledType, this.Scope, this.Scope))
						{
							this.AddError(\u0002, MessageId.Err_TypesNotComparable, new object[]
							{
								compiledType,
								compiledType2
							});
							flag = true;
						}
					}
					else if (!global::\u0006.\u0011.\u0002(compiledType2, compiledType, this.Scope) && !SimpleTypeChecker.\u0001(compiledType, compiledType2, u[0], u[1], this.Scope))
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
					if (u[0] is _ILiteralExpression)
					{
						if (!global::\u0006.\u0011.\u0001(u[0] as _ILiteralExpression, compiledType, compiledType2, this.Scope, this.Scope))
						{
							this.AddError(\u0002, MessageId.Err_TypesNotComparable, new object[]
							{
								compiledType2,
								compiledType
							});
							flag2 = true;
						}
					}
					else if (!global::\u0006.\u0011.\u0002(compiledType, compiledType2, this.Scope) && !SimpleTypeChecker.\u0001(compiledType, compiledType2, u[0], u[1], this.Scope))
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
					if (!TypeTable.IsNumber(compiledType.Class) && compiledType.Class != TypeClass.Bool && compiledType.Class != TypeClass.String && compiledType.Class != TypeClass.WString && compiledType.Class != TypeClass.Time && compiledType.Class != TypeClass.LTime && compiledType.Class != TypeClass.Date && compiledType.Class != TypeClass.DateAndTime && compiledType.Class != TypeClass.TimeOfDay && compiledType.Class != TypeClass.LDate && compiledType.Class != TypeClass.LDateAndTime && compiledType.Class != TypeClass.LTimeOfDay && compiledType.Class == TypeClass.Userdef && !this.\u0001(compiledType))
					{
						this.AddError(\u0002, MessageId.Err_TypesNotComparable, new object[]
						{
							compiledType,
							compiledType2
						});
						return;
					}
					if (!TypeTable.IsNumber(compiledType2.Class) && compiledType2.Class != TypeClass.Bool && compiledType2.Class != TypeClass.String && compiledType2.Class != TypeClass.WString && compiledType2.Class != TypeClass.Time && compiledType2.Class != TypeClass.LTime && compiledType2.Class != TypeClass.Date && compiledType2.Class != TypeClass.DateAndTime && compiledType2.Class != TypeClass.TimeOfDay && compiledType2.Class != TypeClass.LDate && compiledType2.Class != TypeClass.LDateAndTime && compiledType2.Class != TypeClass.LTimeOfDay && compiledType2.Class == TypeClass.Userdef && !this.\u0001(compiledType2))
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
			if (TypeTable.IsNumber(compiledType.Class) || TypeTable.IsNumber(compiledType2.Class))
			{
				_IType u4 = this.\u0001(\u0002.Code, false, 0, u, u2);
				for (int i = 0; i < u.Count; i++)
				{
					this.\u0001(\u0002[i], u[i], u2[i], u4);
				}
			}
		}

		// Token: 0x06001FC3 RID: 8131 RVA: 0x0006AED4 File Offset: 0x000690D4
		private static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, _IExpression \u0004, _IExpression \u0005, ICommonScope \u0006)
		{
			return SimpleTypeChecker.\u0001(\u0002, \u0005, \u0006) || SimpleTypeChecker.\u0001(\u0003, \u0004, \u0006);
		}

		// Token: 0x06001FC4 RID: 8132 RVA: 0x0006AEEC File Offset: 0x000690EC
		private static bool \u0001(ICompiledType \u0002, _IExpression \u0003, ICommonScope \u0004)
		{
			ISignature signature = null;
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			if (iuserdefType != null && Helper.InvalidId != iuserdefType.SignatureId)
			{
				signature = \u0004.FindSignature(iuserdefType);
			}
			if (signature == null || Operator.Interface != signature.POUType)
			{
				return false;
			}
			IPrecompileScope precompileScope = \u0004 as IPrecompileScope;
			if (precompileScope == null)
			{
				return false;
			}
			ILiteralValue literalValue = \u0003.Literal(precompileScope);
			return literalValue != null && ((literalValue.KindOf == KindOfLiteral.SignedInteger && literalValue.SignedLong == 0L) || (KindOfLiteral.UnsignedInteger == literalValue.KindOf && literalValue.UnsignedLong == 0UL));
		}

		// Token: 0x06001FC5 RID: 8133 RVA: 0x0006AF6C File Offset: 0x0006916C
		public virtual void \u0092(_IOperatorExpression \u0002)
		{
			if (\u0002.Code == Operator.And_Then || \u0002.Code == Operator.Or_Else)
			{
				this.\u0001(\u0002, \u0002, this.TopOfStack.\u0001.\u0001, TypeTable.Bool);
			}
			else
			{
				this.\u0001(\u0002, \u0002, this.TopOfStack.\u0001.\u0001, TypeTable.AnyBit);
			}
			for (int i = 0; i < this.TopOfStack.\u0001.\u0001.Count; i++)
			{
				this.\u0001(this.TopOfStack.\u0001.\u0001[i], this.TopOfStack.\u0001.\u0001[i], this.TopOfStack.\u0001.\u0001[i], this.TopOfStack.\u0001.\u0001);
			}
		}

		// Token: 0x06001FC6 RID: 8134 RVA: 0x0006B04C File Offset: 0x0006924C
		private bool \u0001(_IOperatorExpression \u0002)
		{
			IList<_IExpression> u = this.TopOfStack.\u0001.\u0001;
			IList<_IType> u2 = this.TopOfStack.\u0001.\u0001;
			if (u.Count < 2)
			{
				return false;
			}
			Operator code = \u0002.Code;
			if (code <= Operator.Sub)
			{
				if (code != Operator.Add)
				{
					if (code != Operator.Sub)
					{
						return false;
					}
					goto IL_BB;
				}
			}
			else if (code != Operator.Plus)
			{
				if (code != Operator.Minus)
				{
					return false;
				}
				goto IL_BB;
			}
			for (int i = 0; i < u2.Count; i++)
			{
				_IType itype = u2[i];
				_IExpression iexpression = u[i];
				if (itype != null && itype.Class != TypeClass.Pointer)
				{
					TypeClass tc = TypeClass.DWord;
					if (TypeTable.GetSize2(TypeClass.Pointer, this.Scope) == 8)
					{
						tc = TypeClass.LWord;
					}
					this.\u0001(iexpression, iexpression, itype, TypeTable.Get(tc));
				}
			}
			return true;
			IL_BB:
			if (u.Count == 2)
			{
				_IExpression iexpression2 = u[1];
				_IType u3 = u2[1];
				TypeClass tc2 = TypeClass.DWord;
				if (TypeTable.GetSize2(TypeClass.Pointer, this.Scope) == 8)
				{
					tc2 = TypeClass.LWord;
				}
				this.\u0001(iexpression2, iexpression2, u3, TypeTable.Get(tc2));
				return true;
			}
			return false;
		}

		// Token: 0x06001FC7 RID: 8135 RVA: 0x0006B15C File Offset: 0x0006935C
		private bool \u0001(_IOperatorExpression \u0002, ICompiledType \u0003)
		{
			IList<_IExpression> u = this.TopOfStack.\u0001.\u0001;
			IList<_IType> u2 = this.TopOfStack.\u0001.\u0001;
			if (u.Count < 2)
			{
				return false;
			}
			ICompiledType u3 = u2[0];
			ICompiledType u4 = u2[1];
			Operator code = \u0002.Code;
			switch (code)
			{
			case Operator.Add:
				break;
			case Operator.Sub:
				goto IL_BC;
			case Operator.Mul:
				goto IL_15F;
			case Operator.Div:
				goto IL_1E5;
			default:
				switch (code)
				{
				case Operator.Plus:
					goto IL_85;
				case Operator.Minus:
					goto IL_BC;
				case Operator.Times:
					goto IL_15F;
				case Operator.Divide:
					goto IL_1E5;
				}
				this.AddError(\u0002, MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					\u0002.Code,
					\u0003
				});
				return false;
			}
			IL_85:
			for (int i = 0; i < u.Count; i++)
			{
				this.\u0001(\u0002[i], \u0002[i], u2[i], \u0003);
			}
			return true;
			IL_BC:
			for (int j = 0; j < u.Count; j++)
			{
				if (u2[j].Class != TypeClass.Date && u2[j].Class != TypeClass.DateAndTime && u2[j].Class != TypeClass.TimeOfDay && u2[j].Class != TypeClass.LDate && u2[j].Class != TypeClass.LDateAndTime && u2[j].Class != TypeClass.LTimeOfDay)
				{
					this.\u0001(\u0002[j], \u0002[j], u2[j], \u0003);
				}
			}
			return true;
			IL_15F:
			int num = 0;
			for (int k = 0; k < u.Count; k++)
			{
				if (u2[k] != null && !u2[k].IsInteger && !TypeTable.IsReal(u2[k].Class) && this.\u0001(\u0002[k], \u0002[k], u2[k], \u0003))
				{
					num++;
				}
			}
			if (num > 1)
			{
				this.AddError(\u0002, MessageId.Err_CannotMultiplyMultipleTime, new object[]
				{
					\u0003
				});
			}
			return true;
			IL_1E5:
			this.\u0001(\u0002[0], \u0002[0], u3, \u0003);
			this.\u0001(\u0002[1], \u0002[1], u4, TypeTable.AnyInt);
			return true;
		}

		// Token: 0x06001FC8 RID: 8136 RVA: 0x0006B3A4 File Offset: 0x000695A4
		public virtual void \u0093(_IOperatorExpression \u0002)
		{
			IList<_IExpression> u = this.TopOfStack.\u0001.\u0001;
			IList<_IType> u2 = this.TopOfStack.\u0001.\u0001;
			_IType u3 = this.TopOfStack.\u0001.\u0001;
			if (\u0002.Code == Operator.Mod && TypeTable.IsReal(u3.Class))
			{
				this.AddError(\u0002, MessageId.Err_ModNotReal, Array.Empty<object>());
			}
			if (u.Count < 2)
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				_IType itype = u2[i];
				if (itype.Class == TypeClass.Enum)
				{
					ISignature signature = this.Scope.FindSignature(itype as IEnumType);
					if (signature != null && signature.HasAttribute("strict"))
					{
						this.AddError(\u0002, MessageId.Err_StrictEnumNoArithmeticAllowed, new object[]
						{
							signature.OrgName
						});
						break;
					}
				}
			}
			ICompiledType compiledType = u2[0];
			ICompiledType compiledType2 = u2[1];
			ICompiledType compiledType3 = u3;
			bool flag = false;
			TypeClass @class = compiledType3.Class;
			if (@class <= TypeClass.Pointer)
			{
				if (@class != TypeClass.DWord)
				{
					switch (@class)
					{
					case TypeClass.Time:
						goto IL_2C1;
					case TypeClass.Date:
					case TypeClass.DateAndTime:
					case TypeClass.TimeOfDay:
						break;
					case TypeClass.Pointer:
						flag = this.\u0001(\u0002);
						goto IL_2CC;
					default:
						goto IL_2CC;
					}
				}
				else
				{
					if ((\u0002.Code == Operator.Sub || \u0002.Code == Operator.Minus) && (compiledType.Class == TypeClass.Pointer || compiledType2.Class == TypeClass.Pointer))
					{
						this.\u0001(\u0002, \u0002, compiledType, compiledType2);
						flag = true;
						goto IL_2CC;
					}
					goto IL_2CC;
				}
			}
			else
			{
				if (@class == TypeClass.LTime)
				{
					goto IL_2C1;
				}
				if (@class - TypeClass.LDate > 2)
				{
					goto IL_2CC;
				}
			}
			Operator code = \u0002.Code;
			if (code <= Operator.Sub)
			{
				if (code != Operator.Add)
				{
					if (code != Operator.Sub)
					{
						goto IL_29D;
					}
					goto IL_238;
				}
			}
			else if (code != Operator.Plus)
			{
				if (code != Operator.Minus)
				{
					goto IL_29D;
				}
				goto IL_238;
			}
			int num = 0;
			for (int j = 0; j < u.Count; j++)
			{
				_IExpression iexpression = u[j];
				_IType itype2 = u2[j];
				if (itype2.Class != TypeClass.Time && itype2.Class != TypeClass.LTime && this.\u0001(iexpression, iexpression, itype2, compiledType3))
				{
					num++;
				}
			}
			if (num > 1)
			{
				this.AddError(\u0002, MessageId.Err_CannotAddMultipleTime, new object[]
				{
					compiledType3
				});
			}
			flag = true;
			goto IL_2CC;
			IL_238:
			this.\u0001(u[0], u[0], compiledType, compiledType3);
			if (TypeTable.IsLType(compiledType3.Class))
			{
				this.\u0001(u[1], u[1], compiledType2, TypeTable.LTime);
			}
			else
			{
				this.\u0001(u[1], u[1], compiledType2, TypeTable.Time);
			}
			flag = true;
			goto IL_2CC;
			IL_29D:
			this.AddError(\u0002, MessageId.Err_OperationNotPossibleOnType, new object[]
			{
				\u0002.Code,
				compiledType3
			});
			goto IL_2CC;
			IL_2C1:
			flag = this.\u0001(\u0002, compiledType3);
			IL_2CC:
			if (\u0002.Code == Operator.Div || \u0002.Code == Operator.Divide)
			{
				_IExpression iexpression2 = (_IExpression)this.TopOfStack.\u0001.\u0001[1].Duplicate();
				iexpression2._CompiledType = this.TopOfStack.\u0001.\u0001;
				_IVariableExpression ivariableExpression = iexpression2 as _IVariableExpression;
				if (ivariableExpression != null)
				{
					ivariableExpression.SetFlag(VarExprFlag.PrecompileTypified, true);
				}
				ILiteralValue literalValue = iexpression2.Literal(this.Scope);
				if (literalValue != null)
				{
					bool flag2;
					if (literalValue.GetInt(out flag2) == 0 && flag2)
					{
						this.AddError(\u0002[1], MessageId.Err_DivisionByZero, Array.Empty<object>());
					}
					else if (literalValue.GetFloat(out flag2) == 0.0 && flag2)
					{
						this.AddError(\u0002[1], MessageId.Err_DivisionByZero, Array.Empty<object>());
					}
				}
			}
			if (flag)
			{
				return;
			}
			if (this.\u0001(\u0002, \u0002, u3, TypeTable.AnyNum))
			{
				for (int k = 0; k < u.Count; k++)
				{
					this.\u0001(\u0002[k], \u0002[k], u2[k], compiledType3);
				}
			}
		}

		// Token: 0x06001FC9 RID: 8137 RVA: 0x0006B79C File Offset: 0x0006999C
		public void \u0094(_IOperatorExpression \u0002)
		{
			IList<_IExpression> u = this.TopOfStack.\u0001.\u0001;
			IList<_IType> u2 = this.TopOfStack.\u0001.\u0001;
			switch (\u0002.Code)
			{
			case Operator.__vcAdd:
			case Operator.__vcSub:
			case Operator.__vcMul:
			case Operator.__vcDiv:
			case Operator.__vcMin:
			case Operator.__vcMax:
			{
				ICompiledType deRefType = this.TopOfStack.\u0001.\u0001[0].DeRefType;
				ICompiledType deRefType2 = this.TopOfStack.\u0001.\u0001[1].DeRefType;
				if (\u0002.Code == Operator.__vcMul)
				{
					if (TypeTable.IsNumber(deRefType.Class) && deRefType2.Class == TypeClass.__Vector)
					{
						this.\u0001(\u0002[0], u[0], deRefType, deRefType2.BaseType);
						return;
					}
					if (deRefType.Class == TypeClass.__Vector && TypeTable.IsNumber(deRefType2.Class))
					{
						this.\u0001(\u0002[1], u[1], deRefType2, deRefType.BaseType);
						return;
					}
				}
				if (\u0002.Code == Operator.__vcDiv && deRefType.Class == TypeClass.__Vector && TypeTable.IsNumber(deRefType2.Class))
				{
					this.\u0001(\u0002[1], u[1], deRefType2, deRefType.BaseType);
					return;
				}
				if (!global::\u001A.\u0001.\u0001(deRefType, deRefType2, this.Scope))
				{
					this.AddError(\u0002[0], MessageId.Err_VectorTypesNotCompatible, new object[]
					{
						deRefType,
						deRefType2
					});
					return;
				}
				return;
			}
			case Operator.__vcDot:
			{
				ICompiledType deRefType3 = this.TopOfStack.\u0001.\u0001[0].DeRefType;
				ICompiledType deRefType4 = this.TopOfStack.\u0001.\u0001[1].DeRefType;
				if (!global::\u001A.\u0001.\u0001(deRefType3, deRefType4, this.Scope))
				{
					this.AddError(\u0002, MessageId.Err_VectorTypesNotCompatible, new object[]
					{
						deRefType3,
						deRefType4
					});
					return;
				}
				return;
			}
			case Operator.__vcSqrt:
				break;
			case Operator.__vcSetReal:
			case Operator.__vcSetLReal:
			{
				_IType u3 = TypeTable.Get((\u0002.Code == Operator.__vcSetLReal) ? Operator.LReal : Operator.Real);
				using (IEnumerator<_IExpression> enumerator = \u0002._OperandsList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_IExpression iexpression = enumerator.Current;
						this.\u0001(iexpression, iexpression, iexpression.Type, u3);
					}
					return;
				}
				break;
			}
			case Operator.__vcLoadReal:
			case Operator.__vcLoadLReal:
			{
				_IType u4 = TypeTable.Get((\u0002.Code == Operator.__vcLoadLReal) ? Operator.LReal : Operator.Real);
				if (!TypeTable.IsInteger(u2[0].Class))
				{
					this.AddError(\u0002[0], MessageId.Err_VectorSizeNotValid, new object[]
					{
						\u0002.Code
					});
				}
				this.\u0001(\u0002[1], \u0002[1], u2[1], global::\u0019.\u0003.\u0001(u4));
				return;
			}
			case Operator.__vcStore:
			{
				TypeClass tc = TypeClass.Any;
				if (u2[1].Class != TypeClass.__Vector)
				{
					this.AddError(\u0002, MessageId.Err_TypeMismatch, new object[]
					{
						u2[1],
						"__VECTOR"
					});
				}
				else
				{
					tc = u2[1].BaseType.Class;
				}
				this.\u0001(\u0002[0], \u0002[0], u2[0], global::\u0019.\u0003.\u0001(TypeTable.Get(tc)));
				return;
			}
			default:
				return;
			}
			ICompiledType deRefType5 = this.TopOfStack.\u0001.\u0001[0].DeRefType;
			if (deRefType5.Class != TypeClass.__Vector)
			{
				this.AddError(\u0002, MessageId.Err_OperationNotPossibleOnType, new object[]
				{
					\u0002.Code,
					deRefType5
				});
				return;
			}
		}

		// Token: 0x06001FCA RID: 8138 RVA: 0x0006BB54 File Offset: 0x00069D54
		public void \u0095(_IOperatorExpression \u0002)
		{
			IList<_IExpression> u = this.TopOfStack.\u0001.\u0001;
			IList<_IType> u2 = this.TopOfStack.\u0001.\u0001;
			_IType u3 = this.TopOfStack.\u0001.\u0001;
			switch (\u0002.Code)
			{
			case Operator.Limit:
				if (u.Count == 3)
				{
					ICompiledType u4 = u2[0];
					ICompiledType u5 = u2[1];
					ICompiledType u6 = u2[2];
					if (u3.Class == TypeClass.Userdef || u3.Class == TypeClass.Array)
					{
						this.AddError(\u0002, MessageId.Err_OperationNotPossibleOnType, new object[]
						{
							\u0002.Code,
							u3
						});
					}
					this.\u0001(\u0002[0], \u0002[0], u4, u3);
					this.\u0001(\u0002[1], \u0002[1], u5, u3);
					this.\u0001(\u0002[2], \u0002[2], u6, u3);
					return;
				}
				break;
			case Operator.Min:
			case Operator.Max:
				this.\u0091(\u0002);
				return;
			case Operator.Trunc:
				break;
			case Operator.Mux:
				if (u.Count < 3)
				{
					return;
				}
				this.\u0001(u[0], u[0], u2[0], TypeTable.AnyInt);
				for (int i = 1; i < u.Count; i++)
				{
					this.\u0001(\u0002[i], \u0002[i], u2[i], u3);
				}
				return;
			case Operator.Sel:
				if (u.Count != 3)
				{
					return;
				}
				this.\u0001(\u0002[0], \u0002[0], u2[0], TypeTable.Bool);
				for (int j = 1; j < u.Count; j++)
				{
					this.\u0001(\u0002[j], \u0002[j], u2[j], u3);
				}
				break;
			default:
				return;
			}
		}

		// Token: 0x06001FCB RID: 8139 RVA: 0x0006BD2C File Offset: 0x00069F2C
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x06001FCC RID: 8140 RVA: 0x0006BD30 File Offset: 0x00069F30
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
		}

		// Token: 0x06001FCD RID: 8141 RVA: 0x0006BD34 File Offset: 0x00069F34
		// Note: this type is marked as 'beforefieldinit'.
		static SimpleTypeChecker()
		{
			LHashSet<string> lhashSet = new LHashSet<string>(StringComparer.OrdinalIgnoreCase);
			lhashSet.Add("__vfinit");
			lhashSet.Add("FB_INIT");
			SimpleTypeChecker.\u0001 = lhashSet;
		}

		// Token: 0x04000514 RID: 1300
		protected readonly int \u0001;

		// Token: 0x04000515 RID: 1301
		protected readonly Guid \u0001;

		// Token: 0x04000516 RID: 1302
		protected readonly _ISignature \u0001;

		// Token: 0x04000517 RID: 1303
		protected readonly _IPreCompileContext \u0001;

		// Token: 0x04000518 RID: 1304
		private readonly LList<_ICompilerMessage> \u0001 = new LList<_ICompilerMessage>();

		// Token: 0x04000519 RID: 1305
		private readonly LHashSet<global::\u0015.\u0004> \u0001 = new LHashSet<global::\u0015.\u0004>();

		// Token: 0x0400051A RID: 1306
		private readonly LDictionary<string, string> \u0001 = new LDictionary<string, string>();

		// Token: 0x0400051B RID: 1307
		private readonly bool \u0001;

		// Token: 0x0400051C RID: 1308
		private readonly Stack<global::\u0010.\u0003> \u0001 = new Stack<global::\u0010.\u0003>();

		// Token: 0x0400051D RID: 1309
		private global::\u0004.\u0005 \u0001;

		// Token: 0x0400051E RID: 1310
		private static readonly LHashSet<string> \u0001;

		// Token: 0x0400051F RID: 1311
		private const int \u0002 = 4;

		// Token: 0x020001AE RID: 430
		private sealed class \u0001
		{
			// Token: 0x06001FCE RID: 8142 RVA: 0x0006BD60 File Offset: 0x00069F60
			public void \u0001(_IType \u0002, _IVariable \u0003, global::\u0015.\u0002 \u0004)
			{
				this.\u0001.Add(\u0002);
				this.\u0001.Add(\u0003);
				this.\u0001.Add(\u0004);
			}

			// Token: 0x06001FCF RID: 8143 RVA: 0x0006BD88 File Offset: 0x00069F88
			public void \u0001(_IType \u0002, global::\u0015.\u0002 \u0003)
			{
				this.\u0002.Add(\u0002);
				this.\u0002.Add(\u0003);
			}

			// Token: 0x06001FD0 RID: 8144 RVA: 0x0006BDA4 File Offset: 0x00069FA4
			public void \u0001(_IType \u0002, _IVariable \u0003)
			{
				this.\u0003.Add(\u0002);
				this.\u0002.Add(\u0003);
			}

			// Token: 0x06001FD1 RID: 8145 RVA: 0x0006BDC0 File Offset: 0x00069FC0
			public void \u0002(_IType \u0002, _IVariable \u0003)
			{
				this.\u0004.Add(\u0002);
				this.\u0003.Add(\u0003);
			}

			// Token: 0x04000520 RID: 1312
			public LList<_IType> \u0001 = new LList<_IType>();

			// Token: 0x04000521 RID: 1313
			public LList<_IVariable> \u0001 = new LList<_IVariable>();

			// Token: 0x04000522 RID: 1314
			public LList<global::\u0015.\u0002> \u0001 = new LList<global::\u0015.\u0002>();

			// Token: 0x04000523 RID: 1315
			public LList<_IType> \u0002 = new LList<_IType>();

			// Token: 0x04000524 RID: 1316
			public LList<global::\u0015.\u0002> \u0002 = new LList<global::\u0015.\u0002>();

			// Token: 0x04000525 RID: 1317
			public LList<_IType> \u0003 = new LList<_IType>();

			// Token: 0x04000526 RID: 1318
			public LList<_IVariable> \u0002 = new LList<_IVariable>();

			// Token: 0x04000527 RID: 1319
			public LList<_IType> \u0004 = new LList<_IType>();

			// Token: 0x04000528 RID: 1320
			public LList<_IVariable> \u0003 = new LList<_IVariable>();
		}

		// Token: 0x020001AF RID: 431
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x06001FD4 RID: 8148 RVA: 0x0006BE5C File Offset: 0x0006A05C
			internal bool \u0001(_IVariable \u0002)
			{
				return \u0002.Name == this.\u0001;
			}

			// Token: 0x04000529 RID: 1321
			public string \u0001;
		}
	}
}
