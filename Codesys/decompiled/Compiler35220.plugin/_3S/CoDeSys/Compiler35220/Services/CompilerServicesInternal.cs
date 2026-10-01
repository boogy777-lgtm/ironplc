using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using \u0003;
using \u0004;
using \u0005;
using \u0007;
using \u000E;
using \u000F;
using \u0010;
using \u0011;
using \u0012;
using \u0014;
using \u0016;
using \u0019;
using \u001A;
using \u001B;
using \u001D;
using \u001E;
using \u001F;
using _3S.CoDeSys.Compiler35220.CompilerPhases;
using _3S.CoDeSys.Compiler35220.Features;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u007F;
using \u0080;
using \u0081;
using \u0083;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000D8 RID: 216
	internal sealed class CompilerServicesInternal : ICompiler4, ICompiler3, ICompiler2, ICompiler, ICompilerHandleInstanceVariables
	{
		// Token: 0x06000F30 RID: 3888 RVA: 0x00029808 File Offset: 0x00027A08
		public void \u0001(bool \u0002)
		{
		}

		// Token: 0x06000F31 RID: 3889 RVA: 0x0002980C File Offset: 0x00027A0C
		public bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ref IExpression \u0004)
		{
			return global::\u000E.\u000F.\u0001(\u0002, \u0003, ref \u0004);
		}

		// Token: 0x06000F32 RID: 3890 RVA: 0x00029818 File Offset: 0x00027A18
		public void \u0001(_ICompiledPOU \u0002)
		{
			global::\u0004.\u0004 visitor = new global::\u0004.\u0004(\u0002.ObjectGuid, -1, null, null, null, false);
			\u0002.Accept(visitor);
		}

		// Token: 0x06000F33 RID: 3891 RVA: 0x00029840 File Offset: 0x00027A40
		public bool \u0001(_ISignature \u0002, IScope5 \u0003, _ICompileContext \u0004)
		{
			return global::\u0014.\u0013.\u0005(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000F34 RID: 3892 RVA: 0x0002984C File Offset: 0x00027A4C
		public bool \u0002(_ISignature \u0002, IScope5 \u0003, _ICompileContext \u0004)
		{
			return global::\u0014.\u0013.\u0006(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000F35 RID: 3893 RVA: 0x00029858 File Offset: 0x00027A58
		public IList<ICodePosition> \u0001(_IStatement \u0002, int \u0003, int \u0004)
		{
			return global::\u001A.\u0004.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000F36 RID: 3894 RVA: 0x00029864 File Offset: 0x00027A64
		public IList<ICodePosition> \u0001(_IExprement \u0002, int \u0003, int \u0004, AccessFlag \u0005)
		{
			return global::\u001A.\u0004.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06000F37 RID: 3895 RVA: 0x00029870 File Offset: 0x00027A70
		public IList<IPrecompilePositionInfo> \u0001(int \u0002, int \u0003, int \u0004)
		{
			return global::\u0007.\u0003.\u0001(\u0002, \u0003, \u0004, false);
		}

		// Token: 0x06000F38 RID: 3896 RVA: 0x0002987C File Offset: 0x00027A7C
		public IList<IPrecompilePositionInfo> \u0001(_IStatement \u0002, int \u0003, int \u0004, int \u0005, bool \u0006)
		{
			return global::\u0007.\u0003.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06000F39 RID: 3897 RVA: 0x0002988C File Offset: 0x00027A8C
		public IList<IPrecompilePositionInfo> \u0001(int \u0002, int \u0003, bool \u0004)
		{
			return global::\u0007.\u0003.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000F3A RID: 3898 RVA: 0x00029898 File Offset: 0x00027A98
		public void \u0001(IList<IAccessInfo> \u0002, _ICompiledPOU \u0003, string \u0004, RefType \u0005, int \u0006, IPreCompileContext \u0007)
		{
			global::\u000E.\u0006.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007);
		}

		// Token: 0x06000F3B RID: 3899 RVA: 0x000298A8 File Offset: 0x00027AA8
		public void \u0001(IList<IAccessInfo> \u0002, ISignature \u0003, string \u0004, RefType \u0005, int \u0006, IPreCompileContext \u0007)
		{
			global::\u000E.\u0006.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007);
		}

		// Token: 0x06000F3C RID: 3900 RVA: 0x000298B8 File Offset: 0x00027AB8
		public void \u0001(IDictionary<string, IList<IAccessInfo>> \u0002, _ICompiledPOU \u0003, Regex \u0004, RefType \u0005, int \u0006, IPreCompileContext \u0007)
		{
			global::\u000E.\u0006.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007);
		}

		// Token: 0x06000F3D RID: 3901 RVA: 0x000298C8 File Offset: 0x00027AC8
		public void \u0001(IDictionary<string, IList<IAccessInfo>> \u0002, ISignature \u0003, Regex \u0004, RefType \u0005, int \u0006, IPreCompileContext \u0007)
		{
			global::\u000E.\u0006.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007);
		}

		// Token: 0x06000F3E RID: 3902 RVA: 0x000298D8 File Offset: 0x00027AD8
		public string \u0001(MessageId \u0002, params object[] \u0003)
		{
			return global::\u0003.\u0006.\u0001(\u0002, \u0003);
		}

		// Token: 0x06000F3F RID: 3903 RVA: 0x000298E4 File Offset: 0x00027AE4
		public void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			global::\u0003.\u0006.\u0002(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000F40 RID: 3904 RVA: 0x000298F0 File Offset: 0x00027AF0
		public void \u0001(_IExprement \u0002, Severity \u0003, MessageId \u0004, params object[] \u0005)
		{
			global::\u0003.\u0006.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06000F41 RID: 3905 RVA: 0x000298FC File Offset: 0x00027AFC
		public void \u0001(_IExprement \u0002, IToken \u0003, MessageId \u0004, params object[] \u0005)
		{
			global::\u0003.\u0006.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06000F42 RID: 3906 RVA: 0x00029908 File Offset: 0x00027B08
		public void \u0001(_IExprement \u0002, IToken \u0003, Severity \u0004, MessageId \u0005, params object[] \u0006)
		{
			global::\u0003.\u0006.\u0001(\u0002, \u0003, \u0005, \u0006);
		}

		// Token: 0x06000F43 RID: 3907 RVA: 0x00029918 File Offset: 0x00027B18
		public void \u0001(_IStatement \u0002)
		{
			global::\u0012.\u0004.\u0001(\u0002);
		}

		// Token: 0x06000F44 RID: 3908 RVA: 0x00029920 File Offset: 0x00027B20
		public void \u0002(_IStatement \u0002)
		{
			global::\u000F.\u0002.\u0001(\u0002);
		}

		// Token: 0x06000F45 RID: 3909 RVA: 0x00029928 File Offset: 0x00027B28
		public void \u0003(_IStatement \u0002)
		{
			global::\u000F.\u0002.\u0001(\u0002);
		}

		// Token: 0x06000F46 RID: 3910 RVA: 0x00029930 File Offset: 0x00027B30
		public IExprementVisitor \u0001(IFlowPosVisitor \u0002, _IBreakpointList \u0003)
		{
			return new global::\u0005.\u0005(\u0002 as IFlowPosVisitor351300, \u0003);
		}

		// Token: 0x06000F47 RID: 3911 RVA: 0x00029940 File Offset: 0x00027B40
		public IStandardTraverser \u0001()
		{
			return new StandardTraverser();
		}

		// Token: 0x06000F48 RID: 3912 RVA: 0x00029948 File Offset: 0x00027B48
		public void \u0001(_ISignature \u0002, _ICompileContext \u0003, _ICompileContext \u0004)
		{
			_ISignature isignature = null;
			_ISignature isignature2 = \u0003.GetSignatureById(\u0002.ParentSignatureId) as _ISignature;
			Debug.\u0001(isignature2 != null);
			if (\u0004 != null)
			{
				isignature = (\u0004.GetSignatureById(\u0002.ParentSignatureId) as _ISignature);
			}
			foreach (_IVariable ivariable in \u0002.All)
			{
				if (ivariable.HasAttribute("instancevar"))
				{
					ivariable.SetFlag(VarFlag.AllocateInInstance, true);
				}
			}
			if (isignature2.POUType == Operator.Program)
			{
				foreach (_IVariable ivariable2 in \u0002.InstanceLocals)
				{
					ivariable2.SetFlag(VarFlag.Static, true);
					ivariable2.SetFlag(VarFlag.Absolut, true);
					ivariable2.SetFlag(VarFlag.AllocateInInstance, false);
					ivariable2.RemoveAttribute(CompileAttributes.ATTRIBUTE_USELOCATION);
				}
				\u0002.SetFlagInternal(SignatureFlagInternal.ContainsInstanceVars, false);
				return;
			}
			foreach (_IVariable ivariable3 in \u0002.InstanceLocals)
			{
				int num = -1;
				string implicitMethodInstVarName = IdentifierConstants.GetImplicitMethodInstVarName352000(isignature2, \u0002, ivariable3);
				if (isignature != null)
				{
					IVariable variable = isignature[implicitMethodInstVarName];
					if (variable != null)
					{
						num = variable.Id;
					}
				}
				if (num == -1)
				{
					num = isignature2.NextId;
				}
				_IVariable ivariable4 = ivariable3.Duplicate() as _IVariable;
				ivariable4.Id = num;
				ivariable4.Name = implicitMethodInstVarName;
				ivariable4.SetFlag(VarFlag.AllocateInInstance, false);
				ivariable4.SetFlag(VarFlag.Local | VarFlag.Implicit, true);
				if (isignature2.POUType == Operator.Program)
				{
					ivariable4.SetFlag(VarFlag.Absolut, true);
				}
				isignature2.AddVariable(ivariable4);
				ivariable3.AddCrossReference(isignature2.Id, null);
				ivariable3.AddAttribute(CompileAttributes.ATTRIBUTE_USELOCATION, ivariable4.Name);
				ivariable4.AddAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INST_VAR, "");
			}
		}

		// Token: 0x06000F49 RID: 3913 RVA: 0x00029B18 File Offset: 0x00027D18
		public IScope5 \u0001(_ICompileContext \u0002)
		{
			return global::\u0007.\u0005.\u0001(\u0002);
		}

		// Token: 0x06000F4A RID: 3914 RVA: 0x00029B20 File Offset: 0x00027D20
		public IScope5 \u0001(_ICompileContext \u0002, int \u0003, int \u0004)
		{
			return global::\u0007.\u0005.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000F4B RID: 3915 RVA: 0x00029B2C File Offset: 0x00027D2C
		public _IParser \u0001(string \u0002, bool \u0003)
		{
			return new global::\u0011.\u0006(\u0002, \u0003);
		}

		// Token: 0x06000F4C RID: 3916 RVA: 0x00029B38 File Offset: 0x00027D38
		public _IParser \u0001(string \u0002)
		{
			return new global::\u0011.\u0006(\u0002);
		}

		// Token: 0x06000F4D RID: 3917 RVA: 0x00029B40 File Offset: 0x00027D40
		public _IParser \u0001(IList<string> \u0002)
		{
			return new global::\u0011.\u0006(\u0002);
		}

		// Token: 0x06000F4E RID: 3918 RVA: 0x00029B48 File Offset: 0x00027D48
		public _IParser \u0001(IScanner \u0002, bool \u0003)
		{
			return new global::\u0011.\u0006(\u0002, \u0003);
		}

		// Token: 0x06000F4F RID: 3919 RVA: 0x00029B54 File Offset: 0x00027D54
		public _IParser \u0001(IScanner \u0002)
		{
			return new global::\u0011.\u0006(\u0002);
		}

		// Token: 0x06000F50 RID: 3920 RVA: 0x00029B5C File Offset: 0x00027D5C
		public _IParser \u0001(IScanner \u0002, bool \u0003, Version \u0004, ILMCompileOptions3 \u0005)
		{
			return new global::\u0011.\u0006(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06000F51 RID: 3921 RVA: 0x00029B68 File Offset: 0x00027D68
		public _IScanner \u0001(IList<string> \u0002, bool \u0003, bool \u0004, bool \u0005, bool \u0006)
		{
			return Scanner.CreateMultiStringScanner(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06000F52 RID: 3922 RVA: 0x00029B78 File Offset: 0x00027D78
		public _IScanner \u0001()
		{
			return Scanner.\u0001();
		}

		// Token: 0x06000F53 RID: 3923 RVA: 0x00029B80 File Offset: 0x00027D80
		public _IScanner \u0001(bool \u0002)
		{
			return Scanner.\u0001();
		}

		// Token: 0x06000F54 RID: 3924 RVA: 0x00029B88 File Offset: 0x00027D88
		public _IScanner \u0001(Version \u0002)
		{
			return Scanner.\u0001(\u0002);
		}

		// Token: 0x17000473 RID: 1139
		// (get) Token: 0x06000F55 RID: 3925 RVA: 0x00029B90 File Offset: 0x00027D90
		public ITypeTable TypeTable
		{
			get
			{
				return TypeTableClass.Singleton;
			}
		}

		// Token: 0x06000F56 RID: 3926 RVA: 0x00029B98 File Offset: 0x00027D98
		public IScope5 \u0001(_ICompileContext \u0002, int \u0003)
		{
			return global::\u0007.\u0005.\u0001(\u0002, \u0003);
		}

		// Token: 0x06000F57 RID: 3927 RVA: 0x00029BA4 File Offset: 0x00027DA4
		public IScope5 \u0001(_ICompileContext \u0002, int \u0003, bool \u0004)
		{
			return global::\u0007.\u0005.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000F58 RID: 3928 RVA: 0x00029BB0 File Offset: 0x00027DB0
		public IScope5 \u0002(_ICompileContext \u0002, int \u0003)
		{
			return new \u0080.\u0008(\u0002, \u0003, true);
		}

		// Token: 0x06000F59 RID: 3929 RVA: 0x00029BBC File Offset: 0x00027DBC
		public string \u0001(_IExprement \u0002)
		{
			\u0084.\u0002 u = new \u0084.\u0002(false, false, false);
			\u0002.Accept(u);
			return u.Output;
		}

		// Token: 0x06000F5A RID: 3930 RVA: 0x00029BE0 File Offset: 0x00027DE0
		public _ICompilerMessage[] \u0001(_IExprement \u0002)
		{
			return CompilerServicesInternal.\u0001(\u0002);
		}

		// Token: 0x06000F5B RID: 3931 RVA: 0x00029BE8 File Offset: 0x00027DE8
		internal static _ICompilerMessage[] \u0001(_IExprement \u0002)
		{
			ErrorVisitor errorVisitor = new ErrorVisitor();
			\u0002.Accept(errorVisitor);
			return errorVisitor._Messages;
		}

		// Token: 0x06000F5C RID: 3932 RVA: 0x00029C08 File Offset: 0x00027E08
		public _ICompilerMessage[] \u0001(_ICompiledPOU \u0002)
		{
			return CompilerServicesInternal.\u0001(\u0002);
		}

		// Token: 0x06000F5D RID: 3933 RVA: 0x00029C10 File Offset: 0x00027E10
		internal static _ICompilerMessage[] \u0001(_ICompiledPOU \u0002)
		{
			ErrorVisitor errorVisitor = new ErrorVisitor();
			\u0002.Accept(errorVisitor);
			return errorVisitor._Messages;
		}

		// Token: 0x06000F5E RID: 3934 RVA: 0x00029C30 File Offset: 0x00027E30
		public _ICompilerMessage[] \u0001(_ICompiledPOU \u0002, bool \u0003)
		{
			return this.\u0001(\u0002, \u0003, \u0003).ToArray<_ICompilerMessage>();
		}

		// Token: 0x06000F5F RID: 3935 RVA: 0x00029C40 File Offset: 0x00027E40
		public IEnumerable<_ICompilerMessage> \u0001(_ICompiledPOU \u0002, bool \u0003, bool \u0004)
		{
			ErrorVisitor errorVisitor = new ErrorVisitor
			{
				VisitErrorStatements = \u0003,
				VisitErrorStatementsInConditionalPragmas = \u0004
			};
			\u0002.Accept(errorVisitor);
			return errorVisitor._Messages;
		}

		// Token: 0x06000F60 RID: 3936 RVA: 0x00029C70 File Offset: 0x00027E70
		internal static void \u0001(IExprement \u0002, IScope \u0003, _ICompileContext \u0004, ICompiledType \u0005, bool \u0006, bool \u0007, _ICompiledPOU \u0008)
		{
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(\u0003 as IScope5, \u0004, \u0005, \u0006, \u0007, \u0008);
			(\u0002 as _IExprement).Accept(ivisit);
		}

		// Token: 0x06000F61 RID: 3937 RVA: 0x00029CA0 File Offset: 0x00027EA0
		public void \u0001(IExprement \u0002, IScope \u0003, _ICompileContext \u0004, ICompiledType \u0005, bool \u0006, bool \u0007, _ICompiledPOU \u0008)
		{
			CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008);
		}

		// Token: 0x06000F62 RID: 3938 RVA: 0x00029CB4 File Offset: 0x00027EB4
		internal static void \u0001(IExprement \u0002, IScope \u0003, _ICompileContext \u0004, ICompiledType \u0005, bool \u0006, bool \u0007, bool \u0008, _ICompiledPOU \u000E)
		{
			ExpressionTypifierWithSpecialTasks expressionTypifierWithSpecialTasks = new ExpressionTypifierWithSpecialTasks(\u0003 as IScope5, \u0004, \u0005, \u0006, \u0007, \u000E);
			expressionTypifierWithSpecialTasks.TreatReferenceAsPointer = \u0008;
			(\u0002 as _IExprement).Accept(expressionTypifierWithSpecialTasks);
		}

		// Token: 0x06000F63 RID: 3939 RVA: 0x00029CEC File Offset: 0x00027EEC
		public void \u0001(IExprement \u0002, IScope \u0003, _ICompileContext \u0004, ICompiledType \u0005, bool \u0006, bool \u0007, bool \u0008, _ICompiledPOU \u000E)
		{
			CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008, \u000E);
		}

		// Token: 0x06000F64 RID: 3940 RVA: 0x00029D00 File Offset: 0x00027F00
		public void \u0001(ICompiledPOU \u0002, IScope \u0003, _ICompileContext \u0004, ICompiledType \u0005, bool \u0006, bool \u0007)
		{
			ExpressionTypifierWithSpecialTasks visitor = new ExpressionTypifierWithSpecialTasks(\u0003 as IScope5, \u0004, \u0005, \u0006, \u0007, \u0002 as _ICompiledPOU);
			(\u0002 as _ICompiledPOU).Accept(visitor);
		}

		// Token: 0x06000F65 RID: 3941 RVA: 0x00029D34 File Offset: 0x00027F34
		public void \u0001(IExprement \u0002, IScope \u0003, _ICompileContext \u0004)
		{
			TypeCheckerVisitor ivisit = new TypeCheckerVisitor(\u0003 as IScope5, \u0004, false, null, false);
			(\u0002 as _IExprement).Accept(ivisit);
		}

		// Token: 0x06000F66 RID: 3942 RVA: 0x00029D60 File Offset: 0x00027F60
		internal static void \u0001(IExprement \u0002, IScope \u0003, _ICompileContext \u0004, bool \u0005)
		{
			TypeCheckerVisitor ivisit = new TypeCheckerVisitor(\u0003 as IScope5, \u0004, \u0005);
			(\u0002 as _IExprement).Accept(ivisit);
		}

		// Token: 0x06000F67 RID: 3943 RVA: 0x00029D88 File Offset: 0x00027F88
		public void \u0001(IExprement \u0002, IScope \u0003, _ICompileContext \u0004, bool \u0005)
		{
			CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06000F68 RID: 3944 RVA: 0x00029D94 File Offset: 0x00027F94
		public void \u0001(ICompiledPOU \u0002, IScope \u0003, _ICompileContext \u0004)
		{
			TypeCheckerVisitor visitor = new TypeCheckerVisitor(\u0003 as IScope5, \u0004, false, null, false);
			(\u0002 as _ICompiledPOU).Accept(visitor);
		}

		// Token: 0x06000F69 RID: 3945 RVA: 0x00029DC0 File Offset: 0x00027FC0
		public _ICompilerMessage[] \u0001(_IExprement \u0002, IScope \u0003, _ICompileContext \u0004)
		{
			this.\u0001(\u0002, \u0003, \u0004, null, true, false, null);
			this.\u0001(\u0002, \u0003, \u0004);
			return this.\u0001(\u0002);
		}

		// Token: 0x06000F6A RID: 3946 RVA: 0x00029DE0 File Offset: 0x00027FE0
		public bool \u0001(Guid \u0002, IProgressCallback \u0003, bool \u0004, bool \u0005)
		{
			return CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06000F6B RID: 3947 RVA: 0x00029DEC File Offset: 0x00027FEC
		public bool \u0001(_IPreCompileContext \u0002, Guid \u0003, bool \u0004, bool \u0005, bool \u0006, out bool \u0007, out _ICompileContext \u0008, IProgressCallback \u000E, bool \u000F, bool \u0010)
		{
			_ICompileContext icompileContext = null;
			return CompilerServicesInternal.\u0001(\u0003, \u0004, \u0005, \u0006, out \u0007, out \u0008, out icompileContext, \u000E, \u000F, \u0010, true);
		}

		// Token: 0x06000F6C RID: 3948 RVA: 0x00029E14 File Offset: 0x00028014
		public bool \u0001(Guid \u0002, bool \u0003, bool \u0004, bool \u0005, out IOnlineChangeDetails \u0006, out IMessage[] \u0007, out IMessage[] \u0008)
		{
			return CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005, out \u0006, out \u0007, out \u0008);
		}

		// Token: 0x06000F6D RID: 3949 RVA: 0x00029E28 File Offset: 0x00028028
		public ICodegenerator \u0001(Guid \u0002, Guid \u0003, bool \u0004, bool \u0005)
		{
			return CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06000F6E RID: 3950 RVA: 0x00029E34 File Offset: 0x00028034
		public bool \u0001(Guid \u0002, string \u0003)
		{
			return false;
		}

		// Token: 0x06000F6F RID: 3951 RVA: 0x00029E38 File Offset: 0x00028038
		public IDownloadInfo \u0001(Guid \u0002, bool \u0003, bool \u0004, bool \u0005)
		{
			return CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06000F70 RID: 3952 RVA: 0x00029E44 File Offset: 0x00028044
		public IDownloadInfo \u0001(Guid \u0002, bool \u0003, bool \u0004, bool \u0005, int[] \u0006)
		{
			return CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06000F71 RID: 3953 RVA: 0x00029E54 File Offset: 0x00028054
		public IExternalReference[] \u0001(_ICompileContext \u0002, bool \u0003)
		{
			return \u001D.\u0002.\u0001(\u0002, \u0003);
		}

		// Token: 0x06000F72 RID: 3954 RVA: 0x00029E60 File Offset: 0x00028060
		public void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			global::\u0014.\u0016.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06000F73 RID: 3955 RVA: 0x00029E6C File Offset: 0x0002806C
		public void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ICompileContext \u0004)
		{
			global::\u0005.\u0008.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000F74 RID: 3956 RVA: 0x00029E78 File Offset: 0x00028078
		public void \u0002(_ICompileContext \u0002, _ISignature \u0003, _ICompileContext \u0004)
		{
			ImplicitToStringFunctions.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000F75 RID: 3957 RVA: 0x00029E84 File Offset: 0x00028084
		public void \u0001(_ICompileContext \u0002, ILMPOU \u0003, _ICompileContext \u0004, ref bool \u0005, bool \u0006, bool \u0007)
		{
			\u0083.\u0007.\u0001(\u0002, \u0003, \u0004, ref \u0005, \u0006, \u0007);
		}

		// Token: 0x06000F76 RID: 3958 RVA: 0x00029E94 File Offset: 0x00028094
		public void \u0001(_ICompileContext \u0002, ILMGlobVarlist \u0003, _ICompileContext \u0004, ref bool \u0005, bool \u0006, bool \u0007)
		{
			\u0083.\u0007.\u0001(\u0002, \u0003, \u0004, ref \u0005, \u0006, \u0007);
		}

		// Token: 0x06000F77 RID: 3959 RVA: 0x00029EA4 File Offset: 0x000280A4
		public void \u0001(_ICompileContext \u0002, ILMDataType \u0003, _ICompileContext \u0004, ref bool \u0005, bool \u0006, bool \u0007)
		{
			\u0083.\u0007.\u0001(\u0002, \u0003, \u0004, ref \u0005, \u0006, \u0007);
		}

		// Token: 0x06000F78 RID: 3960 RVA: 0x00029EB4 File Offset: 0x000280B4
		public IApplicationContent \u0001(_ICompileContext \u0002)
		{
			global::\u001B.\u0005 u = new global::\u001B.\u0005();
			u.\u0001(\u0002);
			return u;
		}

		// Token: 0x06000F79 RID: 3961 RVA: 0x00029EC4 File Offset: 0x000280C4
		public IApplicationContent \u0001(byte[] \u0002, bool \u0003, bool \u0004)
		{
			global::\u001B.\u0005 u = new global::\u001B.\u0005();
			u.\u0001(\u0002, \u0003, \u0004);
			return u;
		}

		// Token: 0x06000F7A RID: 3962 RVA: 0x00029ED4 File Offset: 0x000280D4
		public bool \u0001(_ICompileContext \u0002, IMessageStorage \u0003, IMessageCategory \u0004)
		{
			return Messages.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000F7B RID: 3963 RVA: 0x00029EE0 File Offset: 0x000280E0
		public IList<_ICompilerMessage> \u0001(_ICompileContext \u0002)
		{
			return Messages.\u0001(\u0002);
		}

		// Token: 0x06000F7C RID: 3964 RVA: 0x00029EE8 File Offset: 0x000280E8
		public string \u0001(MessageId \u0002)
		{
			return global::\u000E.\u0018.\u0001(\u0002);
		}

		// Token: 0x06000F7D RID: 3965 RVA: 0x00029EF0 File Offset: 0x000280F0
		public bool \u0001(_ICompileContext \u0002, ISignature \u0003, IExprementPosition \u0004, string \u0005, ICompiledType \u0006)
		{
			return CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06000F7E RID: 3966 RVA: 0x00029F00 File Offset: 0x00028100
		public IExpressionTypifier \u0001(IScope \u0002, _ICompileContext \u0003, ICompiledType \u0004, bool \u0005, bool \u0006, bool \u0007, _ICompiledPOU \u0008)
		{
			return new ExpressionTypifierWithSpecialTasks(\u0002 as IScope5, \u0003, \u0004, \u0005, \u0006, \u0008)
			{
				TreatReferenceAsPointer = \u0007
			};
		}

		// Token: 0x06000F7F RID: 3967 RVA: 0x00029F20 File Offset: 0x00028120
		public IExpressionTypifier \u0001(int \u0002, _ICompileContext \u0003, ICompiledType \u0004, bool \u0005, bool \u0006, bool \u0007, _ICompiledPOU \u0008)
		{
			IScope u = null;
			if (\u0003 != null)
			{
				ISignature signatureById = \u0003.GetSignatureById(\u0002);
				if (\u0006)
				{
					u = \u0081.\u0008.\u0001(\u0003, signatureById as _ISignature, true, APEnvironmentFacade.Instance.LanguageModelMgr.Pool, APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(\u0003.ApplicationGuid));
				}
				else
				{
					u = global::\u0007.\u0005.\u0001(\u0003, \u0002);
				}
			}
			return this.\u0001(u, \u0003, null, \u0005, \u0006, false, \u0008);
		}

		// Token: 0x06000F80 RID: 3968 RVA: 0x00029F8C File Offset: 0x0002818C
		public void \u0001(ICodegenerator \u0002, _ICompileContext \u0003, ITargetSettings \u0004)
		{
			\u0002.Setup(\u0004);
			\u0002.Initialize(new \u001E.\u000E(global::\u0007.\u0005.\u0001(\u0003), false, \u0003), global::\u0007.\u0005.\u0001(\u0003));
		}

		// Token: 0x17000474 RID: 1140
		// (get) Token: 0x06000F81 RID: 3969 RVA: 0x00029FB0 File Offset: 0x000281B0
		internal static int SizeNoLimit
		{
			get
			{
				return CompilerServicesInternal.InvalidSignatureOffset;
			}
		}

		// Token: 0x17000475 RID: 1141
		// (get) Token: 0x06000F82 RID: 3970 RVA: 0x00029FB8 File Offset: 0x000281B8
		internal static int NoSegmentation
		{
			get
			{
				return CompilerServicesInternal.InvalidSignatureOffset;
			}
		}

		// Token: 0x17000476 RID: 1142
		// (get) Token: 0x06000F83 RID: 3971 RVA: 0x00029FC0 File Offset: 0x000281C0
		internal static ushort InvalidRefId
		{
			get
			{
				return ushort.MaxValue;
			}
		}

		// Token: 0x17000477 RID: 1143
		// (get) Token: 0x06000F84 RID: 3972 RVA: 0x00029FC8 File Offset: 0x000281C8
		internal static int InvalidSignatureOffset
		{
			get
			{
				return int.MaxValue;
			}
		}

		// Token: 0x06000F85 RID: 3973 RVA: 0x00029FD0 File Offset: 0x000281D0
		public void \u0001()
		{
			\u0081.\u0008.\u0001();
		}

		// Token: 0x06000F86 RID: 3974 RVA: 0x00029FD8 File Offset: 0x000281D8
		private static void \u0001()
		{
			CompilerServicesInternal.LMM.StartCompilation();
		}

		// Token: 0x06000F87 RID: 3975 RVA: 0x00029FE4 File Offset: 0x000281E4
		public ISymbolTables \u0001(_ICompileContext \u0002)
		{
			return new \u007F.\u0004(\u0002);
		}

		// Token: 0x06000F88 RID: 3976 RVA: 0x00029FEC File Offset: 0x000281EC
		private static bool \u0001(Guid \u0002, IProgressCallback \u0003, bool \u0004, bool \u0005)
		{
			CompilerServicesInternal.\u0001();
			bool result;
			try
			{
				if (CompilerServicesInternal.LMM._GetPrecompileContext(\u0002) == null)
				{
					throw new ArgumentException("No application found", "stApplication");
				}
				bool flag = false;
				foreach (IPreCompileContext preCompileContext in CompilerServicesInternal.LMM._AllPreCompileContexts(true, true))
				{
					_IPreCompileContext ipreCompileContext = (_IPreCompileContext)preCompileContext;
					ipreCompileContext.RemoveTimeStampOnlyObjects();
					ipreCompileContext.CalculateLinkIds();
				}
				_ICompileContext icompileContext;
				_ICompileContext icompileContext2;
				bool flag2 = CompilerServicesInternal.\u0001(\u0002, true, false, false, out flag, out icompileContext, out icompileContext2, \u0003, \u0004, \u0005, true);
				if (flag2 && \u0002 != Guid.Empty && !flag)
				{
					_IMemorySettings memorySettings = APEnvironmentFacade.Instance.LanguageModelMgr.MemorySettingsHelper.GetMemorySettings(\u0002, icompileContext.SimulationMode);
					LList<_IArea> llist = new LList<_IArea>(1);
					llist.AddRange(memorySettings._Areas);
					_IDataManager dataManager = icompileContext.DataManager;
					_IDataManager dataManager2 = MemoryCompiler.\u0001(icompileContext.DataManager);
					icompileContext.DataManager = dataManager2;
					icompileContext.ConfigureMemory(memorySettings, llist, 0);
					Locator.\u0001(icompileContext, CompilerServicesInternal.LMM.GetReferenceContext(\u0002));
					global::\u0004.\u0010.\u0001(icompileContext, true);
					icompileContext.DataManager = dataManager;
				}
				if (icompileContext != null)
				{
					flag2 = CompilerServicesInternal.LMM.DoOutput(icompileContext);
				}
				if (!flag2)
				{
					CompilerServicesInternal.LMM.RemoveCompileContext(\u0002);
				}
				result = flag2;
			}
			finally
			{
				CompilerServicesInternal.LMM.EndCompilation();
			}
			return result;
		}

		// Token: 0x06000F89 RID: 3977 RVA: 0x0002A16C File Offset: 0x0002836C
		internal static bool \u0001(ITargetSettings \u0002)
		{
			return global::\u0016.\u0004.SimpleCycle.GetBoolValue(\u0002);
		}

		// Token: 0x06000F8A RID: 3978 RVA: 0x0002A17C File Offset: 0x0002837C
		private static bool \u0001(Guid \u0002, bool \u0003, bool \u0004, bool \u0005, out bool \u0006, out _ICompileContext \u0007, out _ICompileContext \u0008, IProgressCallback \u000E, bool \u000F, bool \u0010, bool \u0011)
		{
			return global::\u0004.\u001B.\u0001(\u0002, \u0003, \u0004, \u0005, out \u0006, out \u0007, out \u0008, \u000E, \u000F, \u0010, \u0011);
		}

		// Token: 0x06000F8B RID: 3979 RVA: 0x0002A1A0 File Offset: 0x000283A0
		private static bool \u0001(Guid \u0002, bool \u0003, bool \u0004, bool \u0005, out IOnlineChangeDetails \u0006, out IMessage[] \u0007, out IMessage[] \u0008)
		{
			return CompilerPhaseControllerGenerateCode.\u0001(\u0002, \u0003, \u0004, \u0005, out \u0006, out \u0007, out \u0008);
		}

		// Token: 0x06000F8C RID: 3980 RVA: 0x0002A1B4 File Offset: 0x000283B4
		internal static ICodegenerator \u0001(Guid \u0002, Guid \u0003, bool \u0004, bool \u0005)
		{
			if (\u0002 == Guid.Empty)
			{
				\u0002 = \u0003;
			}
			ICodegenerator result;
			try
			{
				IDeviceIdentification targetIdOfDevice = CompilerServicesInternal.LMM.ApplicationDeviceTable.GetTargetIdOfDevice(\u0002);
				ITargetSettings targetSettings = APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
				if (targetSettings == null)
				{
					result = null;
				}
				else
				{
					if (\u0004)
					{
						targetSettings = APEnvironmentFacade.Instance.GetSimulationTargetSettings(targetIdOfDevice, \u0002);
						if (targetSettings == null)
						{
							return null;
						}
					}
					string stringValue = global::\u0016.\u0004.CodegeneratorGuid.GetStringValue(targetSettings);
					if (string.IsNullOrEmpty(stringValue))
					{
						result = null;
					}
					else
					{
						Guid typeGuid = new Guid(stringValue);
						ICodegenerator codegenerator = APEnvironmentFacade.Instance.CreateCodegenerator(typeGuid);
						codegenerator.Setup(targetSettings);
						if (codegenerator is IDisassembler2)
						{
							(codegenerator as IDisassembler2).GenerateDisassembleCode = \u0005;
						}
						result = codegenerator;
					}
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000F8D RID: 3981 RVA: 0x0002A27C File Offset: 0x0002847C
		public static ITargetSettings \u0001(Guid \u0002)
		{
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(\u0002);
			if (guid == Guid.Empty)
			{
				guid = \u0002;
			}
			IDeviceIdentification targetIdOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
			return APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
		}

		// Token: 0x06000F8E RID: 3982 RVA: 0x0002A2D0 File Offset: 0x000284D0
		public static IDeviceIdentification \u0001(Guid \u0002)
		{
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(\u0002);
			if (guid == Guid.Empty)
			{
				guid = \u0002;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
		}

		// Token: 0x06000F8F RID: 3983 RVA: 0x0002A318 File Offset: 0x00028518
		private static _ICompileContext \u0001(Guid \u0002, ref DownloadInfoFlags \u0003)
		{
			_ICompileContext icompileContext = null;
			if (\u0003.CompactDownload)
			{
				\u0003.OfflineBootProject = true;
			}
			if (\u0003.BootProject)
			{
				bool flag = false;
				icompileContext = CompilerServicesInternal.\u0001(\u0002, out flag, false, \u0003.OfflineBootProject);
				if (icompileContext == null)
				{
					if (flag)
					{
						throw new Exception(\u0081.\u0001.Except_Boot_Project_Error);
					}
					throw new Exception(\u0081.\u0001.Except_Boot_Project_Error_Error_In_Compile);
				}
			}
			\u0002 = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetOriginalApplication(\u0002);
			_ICompileContext icompileContext2 = APEnvironmentFacade.Instance.LanguageModelMgr[\u0002];
			if (\u0003.BootProject)
			{
				icompileContext2 = icompileContext;
			}
			if (icompileContext2 == null)
			{
				icompileContext2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(\u0002);
				if (icompileContext2 == null)
				{
					return null;
				}
			}
			bool flag2 = !\u0003.BootProject;
			if (icompileContext2.ContainsOnlineChangeCode != \u0003.OnlineChange && flag2)
			{
				CompilerServicesInternal.\u0001(\u0002, \u0003.OnlineChange, false);
				icompileContext2 = APEnvironmentFacade.Instance.LanguageModelMgr[\u0002];
				if (icompileContext2 == null)
				{
					return null;
				}
			}
			if (!icompileContext2.ContainsCode)
			{
				CompilerServicesInternal.\u0001(\u0002, \u0003.OnlineChange, false);
				icompileContext2 = APEnvironmentFacade.Instance.LanguageModelMgr[\u0002];
			}
			if (icompileContext2 == null)
			{
				return null;
			}
			return icompileContext2;
		}

		// Token: 0x06000F90 RID: 3984 RVA: 0x0002A428 File Offset: 0x00028628
		private static IDownloadInfo \u0001(Guid \u0002, bool \u0003, bool \u0004, bool \u0005)
		{
			return CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005, null);
		}

		// Token: 0x06000F91 RID: 3985 RVA: 0x0002A434 File Offset: 0x00028634
		private static IDownloadInfo \u0001(Guid \u0002, bool \u0003, bool \u0004, bool \u0005, int[] \u0006)
		{
			_ICompileContext value = APEnvironmentFacade.Instance.LanguageModelMgr[\u0002];
			ITargetSettings deviceSettings = CompilerServicesInternal.\u0001(\u0002);
			bool boolValue = global::\u0016.\u0004.CompactDownload.GetBoolValue(deviceSettings);
			DownloadInfoFlags u = new DownloadInfoFlags
			{
				BootProject = \u0004,
				OnlineChange = \u0003,
				OfflineBootProject = \u0005,
				CompactDownload = boolValue
			};
			_ICompileContext icompileContext = CompilerServicesInternal.\u0001(\u0002, ref u);
			if (icompileContext == null)
			{
				return null;
			}
			IDownloadInfo result = \u001D.\u0002.\u0001(icompileContext, u, \u0006);
			if (\u0004)
			{
				APEnvironmentFacade.Instance.LanguageModelMgr[\u0002] = value;
			}
			if (\u0005)
			{
				string applicationFileNameNew = APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationFileNameNew(\u0002, false, icompileContext.SimulationMode);
				if (!APEnvironmentFacade.Instance.ProjectSideCarService.ExistsEntry(APEnvironmentFacade.Instance.PrimaryProjectHandle, Path.GetFileName(applicationFileNameNew)))
				{
					APEnvironmentFacade.Instance.LanguageModelMgr.UpdateDownloadContext(\u0002);
					return result;
				}
				APEnvironmentFacade.Instance.LanguageModelMgr.CreateBootDuplicate(\u0002);
			}
			return result;
		}

		// Token: 0x06000F92 RID: 3986 RVA: 0x0002A520 File Offset: 0x00028720
		internal static void \u0001(int \u0002, bool \u0003, IDictionary<int, _ICompiledPOU[]> \u0004, _ICompileContext \u0005)
		{
			_ICompiledPOU[] array;
			if (!\u0004.TryGetValue(\u0002, out array))
			{
				return;
			}
			foreach (_ICompiledPOU icompiledPOU in array)
			{
				_ICompiledPOU icompiledPOU2 = global::\u0019.\u0003.\u0001(icompiledPOU.Name);
				icompiledPOU2.SetFlag(CompiledPOUFlags.ToCompile, \u0003);
				icompiledPOU2.SetFlag(CompiledPOUFlags.Generated, true);
				icompiledPOU2.SetFlag(CompiledPOUFlags.DataRelocations, icompiledPOU.GetFlag(CompiledPOUFlags.DataRelocations));
				icompiledPOU2.CompiledCode = icompiledPOU.CompiledCode;
				_ISignature isignature = global::\u0019.\u0003.\u0001();
				isignature.Name = icompiledPOU.Name;
				isignature.POUType = Operator.VarGlobal;
				isignature = isignature.CreateCompiledSignature(null, \u0005.HasByteSupport());
				if (icompiledPOU.GetFlag(CompiledPOUFlags.Blob) && !\u0005.DataManager._MemorySettings.OnlineChangeInOwnSegment)
				{
					if (!MemoryCompiler.\u0002(\u0005.DataManager, icompiledPOU2.CompiledCode.Location.Area, icompiledPOU2.CompiledCode.Location.Offset, icompiledPOU2.CompiledCode.CodeSize, DataSegmentFlags.None))
					{
						string u = string.Format(\u0081.\u0002.Err_InternalErrorProhibitingOnlineChange, 1);
						IMessage cm = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.Err_InternalErrorProhibitingOnlineChange);
						isignature.AddError(cm);
					}
					icompiledPOU2.SetFlag(CompiledPOUFlags.Blob, true);
				}
				else
				{
					icompiledPOU2.SetFlag(CompiledPOUFlags.ConstBlob, true);
				}
				icompiledPOU2.SetFlag(CompiledPOUFlags.Typified, true);
				icompiledPOU2.SetParseTree(global::\u0019.\u0003.\u0001());
				IScope5 scope = global::\u0007.\u0005.\u0001(\u0005, isignature.Id);
				scope.LocalSignature = isignature;
				global::\u0014.\u0013.\u0002(isignature, scope, \u0005);
				\u0005.AddSignature(isignature, null, null, true);
				\u0005.AddCompiledPOU(icompiledPOU2, isignature, null);
			}
		}

		// Token: 0x06000F93 RID: 3987 RVA: 0x0002A6BC File Offset: 0x000288BC
		private static _ICompileContext \u0001(Guid \u0002, _ICompileContext \u0003, IMessageCategory \u0004, IProgressCallback \u0005, out bool \u0006, bool \u0007)
		{
			\u0006 = false;
			_ICompileContext icompileContext = null;
			try
			{
				icompileContext = global::\u0010.\u0013.\u0001(\u0002, \u0003, \u0004, \u0005, out \u0006, \u0007);
			}
			catch
			{
				return null;
			}
			if (icompileContext == null)
			{
				return null;
			}
			icompileContext.DataId = \u0003.DataId;
			icompileContext.LastDataId = \u0003.DataId;
			icompileContext.CodeId = \u0003.CodeId;
			icompileContext.LastCodeId = \u0003.CodeId;
			return icompileContext;
		}

		// Token: 0x06000F94 RID: 3988 RVA: 0x0002A72C File Offset: 0x0002892C
		private static uint \u0001(_ICompileContext \u0002)
		{
			ICRCSum icrcsum = APEnvironmentFacade.Instance.LanguageModelMgr.CreateCheckSumComputer();
			IList<ICompiledPOU4> allCompiledPOUsEx = \u0002.GetAllCompiledPOUsEx();
			LDictionary<IDataLocation, _ICompiledPOU> ldictionary = new LDictionary<IDataLocation, _ICompiledPOU>(allCompiledPOUsEx.Count);
			foreach (ICompiledPOU4 compiledPOU in allCompiledPOUsEx)
			{
				_ICompiledPOU icompiledPOU = (_ICompiledPOU)compiledPOU;
				if (icompiledPOU.CompiledCode != null && icompiledPOU.CompiledCode.Location != null && !icompiledPOU.GetFlag(CompiledPOUFlags.ToRemoveAfterDownload) && !icompiledPOU.GetFlag(CompiledPOUFlags.IgnoreForChecksum))
				{
					ldictionary[icompiledPOU.CompiledCode.Location] = icompiledPOU;
				}
			}
			LList<IDataLocation> llist = Enumerable.ToLList<IDataLocation>(ldictionary.Keys);
			llist.Sort();
			foreach (IDataLocation dataLocation in llist)
			{
				_ICompiledPOU icompiledPOU2 = ldictionary[dataLocation];
				IntegerUnion integerUnion = default(IntegerUnion);
				integerUnion.m_short0 = (short)icompiledPOU2.CompiledCode.Location.Area;
				integerUnion.m_uint1 = (uint)icompiledPOU2.CompiledCode.Location.Offset;
				byte[] array = new byte[12];
				BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(array));
				binaryWriter.Write(integerUnion.m_ulong);
				binaryWriter.Write(Helper.\u0001(icompiledPOU2));
				binaryWriter.Flush();
				icrcsum.CRC32Update(array, array.Length);
			}
			return icrcsum.CRC32Finish(Array.Empty<byte>(), 0);
		}

		// Token: 0x06000F95 RID: 3989 RVA: 0x0002A8AC File Offset: 0x00028AAC
		internal static bool \u0001(_ISignature \u0002, LStack<_ISignature> \u0003, bool \u0004)
		{
			bool result = true;
			int num = 0;
			foreach (_ISignature isignature in \u0003)
			{
				if (isignature.Id == \u0002.Id)
				{
					string text = " -> " + \u0002.Name;
					object[] array = \u0003.ToArray();
					object[] array2 = array;
					for (int i = 0; i <= num; i++)
					{
						_ISignature isignature2 = array2[i] as _ISignature;
						if (num == i)
						{
							text = isignature2.Name + text;
						}
						else
						{
							text = " -> " + isignature2.Name + text;
						}
					}
					if (\u0004)
					{
						isignature.AddMessage(Severity.Error, MessageId.Err_CallRecursion, new object[]
						{
							text
						});
					}
					else
					{
						isignature.AddMessage(Severity.Error, MessageId.Err_DataRecursion, new object[]
						{
							text
						});
					}
					result = false;
					break;
				}
				num++;
			}
			return result;
		}

		// Token: 0x06000F96 RID: 3990 RVA: 0x0002A9B0 File Offset: 0x00028BB0
		private static bool \u0001(_ICompileContext \u0002, _ISignature \u0003, LStack<_ISignature> \u0004, LDictionary<int, int> \u0005)
		{
			if (!CompilerServicesInternal.\u0001(\u0003, \u0004, true))
			{
				return false;
			}
			if (\u0005.ContainsKey(\u0003.Id))
			{
				return true;
			}
			if (\u0003.POUType == Operator.Method)
			{
				return true;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002);
			\u0004.Push(\u0003);
			foreach (int nId in \u0003.CalleeIds)
			{
				_ISignature u = scope[nId] as _ISignature;
				if (!CompilerServicesInternal.\u0001(\u0002, u, \u0004, \u0005))
				{
					return false;
				}
			}
			\u0004.Pop();
			\u0005[\u0003.Id] = \u0003.Id;
			return true;
		}

		// Token: 0x06000F97 RID: 3991 RVA: 0x0002AA44 File Offset: 0x00028C44
		private static bool \u0001(IScope5 \u0002, _ISignature \u0003)
		{
			foreach (int nId in \u0003.CallerIds)
			{
				_ISignature isignature = \u0002[nId] as _ISignature;
				if (!(isignature.Name == IdentifierConstants.InitMethodName))
				{
					return false;
				}
				if (!CompilerServicesInternal.\u0001(\u0002, isignature))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000F98 RID: 3992 RVA: 0x0002AA98 File Offset: 0x00028C98
		private static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			uint num = CompilerServicesInternal.\u0002(\u0002);
			uint num2 = CompilerServicesInternal.\u0001(\u0002);
			\u0002.CheckSumCode = num2;
			\u0002.CheckSumData = num;
			if (\u0003 != null)
			{
				\u0002.CheckSumCodeLast = \u0003.CheckSumCode;
				\u0002.CheckSumDataLast = \u0003.CheckSumData;
			}
			else
			{
				\u0002.CheckSumCodeLast = num2;
				\u0002.CheckSumDataLast = num;
			}
			byte[] array = new byte[16];
			IntegerUnion integerUnion = new IntegerUnion
			{
				m_int0 = (int)\u0002.CheckSumCode
			};
			array[0] = integerUnion.m_byte0;
			array[1] = integerUnion.m_byte1;
			array[2] = integerUnion.m_byte2;
			array[3] = integerUnion.m_byte3;
			\u0002.CodeId = new Guid(array);
			integerUnion.m_int0 = (int)\u0002.CheckSumData;
			array[0] = integerUnion.m_byte0;
			array[1] = integerUnion.m_byte1;
			array[2] = integerUnion.m_byte2;
			array[3] = integerUnion.m_byte3;
			\u0002.DataId = new Guid(array);
		}

		// Token: 0x17000478 RID: 1144
		// (get) Token: 0x06000F99 RID: 3993 RVA: 0x0002AB78 File Offset: 0x00028D78
		private static _ILanguageModelManagerConsolidated LMM
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr;
			}
		}

		// Token: 0x06000F9A RID: 3994 RVA: 0x0002AB84 File Offset: 0x00028D84
		private static bool \u0001(Guid \u0002, bool \u0003, bool \u0004)
		{
			IMessage[] array = null;
			IMessage[] array2 = null;
			IOnlineChangeDetails onlineChangeDetails;
			return CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, out onlineChangeDetails, out array, out array2);
		}

		// Token: 0x06000F9B RID: 3995 RVA: 0x0002ABA4 File Offset: 0x00028DA4
		private static bool \u0001(Guid \u0002, bool \u0003, bool \u0004, out IOnlineChangeDetails \u0005, out IMessage[] \u0006, out IMessage[] \u0007)
		{
			if (CompilerServicesInternal.LMM.GetPrecompileContext(\u0002) == null)
			{
				throw new ArgumentException(\u0081.\u0001.ErrNoApplication);
			}
			CompilerServicesInternal.\u0001();
			try
			{
				foreach (IPreCompileContext preCompileContext in CompilerServicesInternal.LMM._AllPreCompileContexts(true, true))
				{
					_IPreCompileContext ipreCompileContext = (_IPreCompileContext)preCompileContext;
					ipreCompileContext.RemoveTimeStampOnlyObjects();
					ipreCompileContext.CalculateLinkIds();
				}
				\u0002 = CompilerServicesInternal.LMM.ApplicationDeviceTable.GetOriginalApplication(\u0002);
				if (\u0002 == Guid.Empty)
				{
					throw new ArgumentException("guidApplication");
				}
				if (!CompilerServicesInternal.\u0001(\u0002, \u0003, false, \u0004, out \u0005, out \u0006, out \u0007))
				{
					return false;
				}
				_ICompileContext icompileContext = CompilerServicesInternal.LMM[\u0002];
				CompilerServicesInternal.LMM.OnCodeChanged(new CodeChangeEventArgs(\u0002, \u0005 as IOnlineChangeDetails2, icompileContext));
				if (icompileContext == null)
				{
					if (CompilerServicesInternal.LMM.GetReferenceContext(\u0002) == null)
					{
						return false;
					}
					return true;
				}
				else
				{
					icompileContext.ContainsOnlineChangeCode = \u0003;
				}
			}
			finally
			{
				CompilerServicesInternal.LMM.EndCompilation();
			}
			return true;
		}

		// Token: 0x06000F9C RID: 3996 RVA: 0x0002ACBC File Offset: 0x00028EBC
		internal static _ICompileContext \u0001(Guid \u0002, out bool \u0003, bool \u0004, bool \u0005)
		{
			\u0003 = false;
			\u0002 = CompilerServicesInternal.LMM.ApplicationDeviceTable.GetOriginalApplication(\u0002);
			_ICompileContext icompileContext = CompilerServicesInternal.LMM.GetReferenceContext(\u0002);
			if (icompileContext == null || \u0005)
			{
				icompileContext = CompilerServicesInternal.LMM[\u0002];
			}
			if (icompileContext.ContainsOnlineChangeCode)
			{
				IProgressCallback progressCallback = APEnvironmentFacade.Instance.StartLengthyOperation();
				APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyNextTask(progressCallback, true, \u0081.\u0001.GenerateCodeProgress, 0, null);
				IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
				APEnvironmentFacade.Instance.ClearMessages(messageCategory);
				string u = string.Format(\u0081.\u0001.BuildStarted, CompilerServicesInternal.LMM.GetApplicationNameByGuid(\u0002));
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
				try
				{
					_ICompileContext icompileContext2 = CompilerServicesInternal.\u0001(\u0002, icompileContext, messageCategory, progressCallback, out \u0003, \u0004);
					if (icompileContext2 != null)
					{
						IMessage[] messages = APEnvironmentFacade.Instance.GetMessages(messageCategory, Severity.Error);
						int num = 0;
						if (messages != null)
						{
							num = messages.Length;
						}
						string format;
						if (num > 0)
						{
							format = \u0081.\u0001.BuildCompleteErrors;
						}
						else
						{
							format = \u0081.\u0001.BuildCompleteOK;
						}
						u = string.Format(format, 0, 0);
						message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
						APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
						if (num > 0)
						{
							return null;
						}
						CompilerServicesInternal.\u0001(icompileContext2, null);
						return icompileContext2;
					}
				}
				finally
				{
					progressCallback.Finish();
				}
				if (CompilerServicesInternal.\u0001(\u0002, true, true))
				{
					return CompilerServicesInternal.LMM[\u0002];
				}
				return null;
			}
			return icompileContext;
		}

		// Token: 0x06000F9D RID: 3997 RVA: 0x0002AE38 File Offset: 0x00029038
		internal static bool \u0001(_ICompileContext \u0002)
		{
			return CompilerServicesInternal.TaskLocalVariablesHelper.\u0001(\u0002);
		}

		// Token: 0x06000F9E RID: 3998 RVA: 0x0002AE40 File Offset: 0x00029040
		internal static string \u0001(_ISignature \u0002, _ICompileContext \u0003)
		{
			string text = IdentifierConstants.InterfaceUnion(\u0002.OrgName);
			if (\u0002.GetFlagInternal(SignatureFlagInternal.VersionFreeLibrary) || (\u0002.IsLibraryObject && \u0003.LibraryIsUnique(\u0002.LibraryPath)))
			{
				return global::\u0014.\u0002.\u0001(\u0002.LibraryPath) + "." + text;
			}
			if (\u0002.GetFlag(SignatureFlag.PoolSignature))
			{
				return "@pool." + text;
			}
			if (string.IsNullOrEmpty(\u0002.LibraryPath))
			{
				return text;
			}
			return \u0002.LibraryPath + "." + text;
		}

		// Token: 0x06000F9F RID: 3999 RVA: 0x0002AED4 File Offset: 0x000290D4
		private static bool \u0001(_ISignature \u0002, out uint \u0003)
		{
			\u0003 = 0U;
			if (!\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC))
			{
				return false;
			}
			string attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC);
			try
			{
				\u0003 = uint.Parse(attributeValue);
			}
			catch
			{
				return false;
			}
			return true;
		}

		// Token: 0x06000FA0 RID: 4000 RVA: 0x0002AF24 File Offset: 0x00029124
		private static uint \u0002(_ICompileContext \u0002)
		{
			MyChecksumStream myChecksumStream = new MyChecksumStream(false);
			BinaryWriter binaryWriter = new BinaryWriter(myChecksumStream);
			LList<ISignature> llist = new LList<ISignature>();
			IList<ISignature4> allSignaturesFlatEx = \u0002.GetAllSignaturesFlatEx();
			\u001F.\u0003 u = new \u001F.\u0003();
			LDictionary<int, bool> ldictionary = new LDictionary<int, bool>();
			for (int i = 0; i < (int)\u0002.DataManager.AreaCount; i++)
			{
				_IArea area = \u0002.DataManager.GetArea(i);
				ldictionary[area.Index] = (area.GetDataSegmentFlag(DataSegmentFlags.Retain) || area.GetDataSegmentFlag(DataSegmentFlags.Persistent));
			}
			LDictionary<int, LList<Tuple<ISignature, _IVariable>>> ldictionary2 = new LDictionary<int, LList<Tuple<ISignature, _IVariable>>>();
			foreach (ISignature4 signature in allSignaturesFlatEx)
			{
				_ISignature isignature = (_ISignature)signature;
				llist.Add(isignature);
				if (isignature.GetFlag(SignatureFlag.ContainsPersistent) || isignature.GetFlag(SignatureFlag.ContainsRetain))
				{
					foreach (_IVariable ivariable in isignature.AllVariables)
					{
						if (ivariable.DataLocation != null && ivariable.DataLocation.Area != 65535 && ldictionary.ContainsKey((int)ivariable.DataLocation.Area) && ldictionary[(int)ivariable.DataLocation.Area])
						{
							LList<Tuple<ISignature, _IVariable>> llist2;
							if (ldictionary2.ContainsKey((int)ivariable.DataLocation.Area))
							{
								llist2 = ldictionary2[(int)ivariable.DataLocation.Area];
							}
							else
							{
								llist2 = new LList<Tuple<ISignature, _IVariable>>();
								ldictionary2.Add((int)ivariable.DataLocation.Area, llist2);
							}
							llist2.Add(new Tuple<ISignature, _IVariable>(isignature, ivariable));
						}
					}
				}
			}
			llist.Sort(u);
			\u0084.\u0003 u2 = new \u0084.\u0003();
			foreach (LList<Tuple<ISignature, _IVariable>> llist3 in ldictionary2.Values)
			{
				llist3.Sort(u2);
			}
			foreach (ISignature signature2 in llist)
			{
				_ISignature u3 = (_ISignature)signature2;
				uint value = 0U;
				if (CompilerServicesInternal.\u0001(u3, out value))
				{
					binaryWriter.Write(value);
				}
			}
			IScope2 u4 = \u0002.CreateGlobalIScope() as IScope2;
			foreach (int num in ldictionary2.Keys)
			{
				LList<Tuple<ISignature, _IVariable>> llist4 = ldictionary2[num];
				MyChecksumStream myChecksumStream2 = new MyChecksumStream(false);
				BinaryWriter binaryWriter2 = new BinaryWriter(myChecksumStream2);
				binaryWriter2.Write(APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationNameByGuid(\u0002.ApplicationGuid));
				foreach (Tuple<ISignature, _IVariable> tuple in llist4)
				{
					int offset = tuple.Item2.DataLocation.Offset;
					binaryWriter2.Write(tuple.Item1.Name);
					if (tuple.Item2.GetFlag(VarFlag.Implicit))
					{
						binaryWriter2.Write("IMPLICIT");
					}
					else
					{
						binaryWriter2.Write(tuple.Item2.Name);
					}
					binaryWriter2.Write(offset);
					Debug.\u0001(global::\u0003.\u0002.\u0001(binaryWriter2, tuple.Item2.CompiledType, \u0002, u4));
				}
				binaryWriter2.Flush();
				myChecksumStream2.Close();
				\u0002.DataManager.GetAreaByIndex(num).Checksum = myChecksumStream2.Checksum;
			}
			binaryWriter.Flush();
			myChecksumStream.Close();
			return myChecksumStream.Checksum;
		}

		// Token: 0x06000FA1 RID: 4001 RVA: 0x0002B370 File Offset: 0x00029570
		private static bool \u0001(ICompiledType \u0002, int \u0003)
		{
			switch (\u0002.Class)
			{
			case TypeClass.Pointer:
			case TypeClass.Reference:
			case TypeClass.Array:
				return CompilerServicesInternal.\u0001(\u0002.BaseType, \u0003);
			case TypeClass.Userdef:
				return (\u0002 as _IUserdefType).SignatureId == \u0003;
			}
			return false;
		}

		// Token: 0x06000FA2 RID: 4002 RVA: 0x0002B3C8 File Offset: 0x000295C8
		internal static bool \u0001(_ICompileContext \u0002, ISignature \u0003, IExprementPosition \u0004, string \u0005, ICompiledType \u0006)
		{
			_ISourcePosition u = global::\u0019.\u0003.\u0001();
			if (\u0004 != null)
			{
				u = global::\u0019.\u0003.\u0001(-1, Guid.Empty, \u0004.Position, \u0004.PositionOffset, (short)\u0005.Length);
			}
			_IVariable ivariable = global::\u0019.\u0003.\u0001(u);
			ivariable.Name = \u0005;
			ivariable._Type = (\u0006 as _IType);
			bool flag = \u0003.POUType == Operator.Function || \u0003.POUType == Operator.Method;
			if (flag)
			{
				ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.Implicit, true);
				ivariable.AddAttribute("added-after-compile", "");
			}
			else
			{
				ivariable.SetFlag(VarFlag.IsCompiled | VarFlag.Implicit | VarFlag.Temp, true);
			}
			ivariable.Id = (\u0003 as _ISignature).NextId;
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0003.Id);
			global::\u0014.\u0012 u2 = new global::\u0014.\u0012(scope as _IScope2, \u0002, null);
			ExpressionTypifierWithSpecialTasks u3 = new ExpressionTypifierWithSpecialTasks(scope, \u0002, null, true, false, null);
			ivariable._Type = TypeCompiler.\u0001(ivariable._Type, scope, \u0002, u2, u3, ivariable, \u0003 as _ISignature);
			if (flag)
			{
				return ((_ISignature)\u0003).InsertVariable(ivariable, 0);
			}
			return ((_ISignature)\u0003).AddVariable(ivariable);
		}

		// Token: 0x06000FA3 RID: 4003 RVA: 0x0002B4D0 File Offset: 0x000296D0
		public _IDataManager \u0001(_IDataManager \u0002)
		{
			return MemoryCompiler.\u0001(\u0002);
		}

		// Token: 0x06000FA4 RID: 4004 RVA: 0x0002B4D8 File Offset: 0x000296D8
		public bool \u0001(_IDataManager \u0002, _IMemorySettings \u0003, IList<_IArea> \u0004, int \u0005)
		{
			return MemoryCompiler.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06000FA5 RID: 4005 RVA: 0x0002B4E4 File Offset: 0x000296E4
		public bool \u0001(_IDataManager \u0002, int \u0003)
		{
			return MemoryCompiler.\u0001(\u0002, \u0003);
		}

		// Token: 0x06000FA6 RID: 4006 RVA: 0x0002B4F0 File Offset: 0x000296F0
		public bool \u0001(_IDataManager \u0002, IDataLocation \u0003, int \u0004, DataSegmentFlags \u0005)
		{
			return MemoryCompiler.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06000FA7 RID: 4007 RVA: 0x0002B4FC File Offset: 0x000296FC
		public bool \u0001(_IDataManager \u0002, ushort \u0003, int \u0004, int \u0005, DataSegmentFlags \u0006)
		{
			return MemoryCompiler.\u0002(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06000FA8 RID: 4008 RVA: 0x0002B50C File Offset: 0x0002970C
		public bool \u0001(_IDataManager \u0002, ref ushort \u0003, ref int \u0004, int \u0005, int \u0006, int \u0007, DataSegmentFlags \u0008)
		{
			return MemoryCompiler.\u0003(\u0002, ref \u0003, ref \u0004, \u0005, \u0006, \u0007, \u0008);
		}

		// Token: 0x040002A2 RID: 674
		internal const string \u0001 = "LanguageModelManager";

		// Token: 0x040002A3 RID: 675
		internal static readonly string \u0002 = "UnicodeIdentifiers";

		// Token: 0x040002A4 RID: 676
		internal static readonly string \u0003 = "{E709B08B-B6E4-4966-8EED-D793A13114C6}";

		// Token: 0x020000D9 RID: 217
		private static class TaskLocalVariablesHelper
		{
			// Token: 0x06000FAA RID: 4010 RVA: 0x0002B538 File Offset: 0x00029738
			public static bool \u0001(_ICompileContext \u0002)
			{
				IEnumerable<ISignature> enumerable = \u0002.GVLSignatures.Where(new Func<ISignature, bool>(CompilerServicesInternal.TaskLocalVariablesHelper.<>c.<>9.\u0001));
				if (enumerable.Count<ISignature>() == 0)
				{
					return true;
				}
				foreach (ISignature signature in enumerable)
				{
					_ISignature isignature = (_ISignature)signature;
					using (IEnumerator<_IVariable> enumerator2 = isignature.AllVariables.Where(new Func<_IVariable, bool>(CompilerServicesInternal.TaskLocalVariablesHelper.<>c.<>9.\u0001)).GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							_IVariable ivariable = enumerator2.Current;
							string u = global::\u0003.\u0006.\u0001(MessageId.Err_TaskLocalVariablesNoOnlineChangePossible, new object[]
							{
								isignature.OrgName
							});
							isignature.AddError(global::\u0019.\u0003.\u0001(ivariable._SourcePosition, u, Severity.Error, MessageId.Err_TaskLocalVariablesNoOnlineChangePossible));
							return false;
						}
					}
				}
				return true;
			}
		}

		// Token: 0x020000DB RID: 219
		internal enum \u0001
		{
			// Token: 0x040002A9 RID: 681
			\u0001,
			// Token: 0x040002AA RID: 682
			\u0002,
			// Token: 0x040002AB RID: 683
			\u0003
		}

		// Token: 0x020000DC RID: 220
		// (Invoke) Token: 0x06000FB0 RID: 4016
		public delegate void CallersCompiledPOUProcessor(_ICompiledPOU cpou);
	}
}
