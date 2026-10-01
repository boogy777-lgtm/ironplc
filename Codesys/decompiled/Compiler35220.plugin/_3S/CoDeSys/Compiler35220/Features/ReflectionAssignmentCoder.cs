using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using \u0007;
using \u0011;
using \u0014;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Features
{
	// Token: 0x020001CF RID: 463
	internal static class ReflectionAssignmentCoder
	{
		// Token: 0x060020BB RID: 8379 RVA: 0x0006FBE4 File Offset: 0x0006DDE4
		internal static void \u0001(LStringBuilder \u0002, _ICompileContext \u0003)
		{
			if (\u0003["__ReflectionInitialisation"] != null)
			{
				\u0002.AppendLine("__ReflectionInitialisation();");
			}
		}

		// Token: 0x060020BC RID: 8380 RVA: 0x0006FC00 File Offset: 0x0006DE00
		internal static void \u0001(_ISequenceStatement \u0002, _ICompileContext \u0003, _ICompiledPOU \u0004)
		{
			if (\u0003["__ReflectionInitialisationForOnlineChange"] != null)
			{
				_IStatement sm = ReflectionAssignmentCoder.\u0001("__ReflectionInitialisationForOnlineChange();", \u0003, \u0004);
				\u0002.Add(sm);
			}
		}

		// Token: 0x060020BD RID: 8381 RVA: 0x0006FC30 File Offset: 0x0006DE30
		internal static void \u0001(_IStatement \u0002, _ICompileContext \u0003, _ICompiledPOU \u0004)
		{
			_IScope iscope = \u0003.CreateIScope(\u0004.SignatureId) as _IScope;
			\u0002.Accept(new ExpressionTypifierWithSpecialTasks(iscope, \u0003, false, \u0004)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true,
				ContributeToCompile = false
			});
			\u0018.\u000E.\u0001(\u0002, \u0003);
			\u0002.Accept(new TypeCheckerVisitor(iscope, \u0003, true)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
		}

		// Token: 0x060020BE RID: 8382 RVA: 0x0006FC98 File Offset: 0x0006DE98
		internal static _IStatement \u0001(string \u0002, _ICompileContext \u0003, _ICompiledPOU \u0004)
		{
			_IStatement istatement = new global::\u0011.\u0006(\u0002, true).\u0001();
			ReflectionAssignmentCoder.\u0001(istatement, \u0003, \u0004);
			return istatement;
		}

		// Token: 0x060020BF RID: 8383 RVA: 0x0006FCB0 File Offset: 0x0006DEB0
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, int \u0004, int \u0005)
		{
			if (\u0004 > 0)
			{
				_IStringType istringType = \u0019.\u0003.Builder.CreateStringType();
				istringType.Length = (\u0019.\u0003.Builder.CreateLiteralExpression(null, (long)(\u0004 + 10)) as _IExpression);
				\u0019.\u0003.Builder.AddLocalStackVariableToCompiledSignature("ConcatenationBuffer", VarFlag.None, istringType, null, \u0003);
				_IPointerType ctype = \u0019.\u0003.Builder.CreatePointerType(istringType);
				\u0019.\u0003.Builder.AddLocalStackVariableToCompiledSignature("pConcatenationBuffer", VarFlag.None, ctype, null, \u0003);
				_IDIntType ctype2 = \u0019.\u0003.Builder.CreateDIntType();
				\u0019.\u0003.Builder.AddLocalStackVariableToCompiledSignature("MAXBUFFER", VarFlag.None, ctype2, null, \u0003);
			}
			if (\u0005 > 0)
			{
				_IWStringType iwstringType = \u0019.\u0003.Builder.CreateWStringType();
				iwstringType.Length = (\u0019.\u0003.Builder.CreateLiteralExpression(null, (long)(\u0005 + 10)) as _IExpression);
				\u0019.\u0003.Builder.AddLocalStackVariableToCompiledSignature("WConcatenationBuffer", VarFlag.None, iwstringType, null, \u0003);
				_IPointerType ctype3 = \u0019.\u0003.Builder.CreatePointerType(iwstringType);
				\u0019.\u0003.Builder.AddLocalStackVariableToCompiledSignature("pWConcatenationBuffer", VarFlag.None, ctype3, null, \u0003);
				_IDIntType ctype4 = \u0019.\u0003.Builder.CreateDIntType();
				\u0019.\u0003.Builder.AddLocalStackVariableToCompiledSignature("MAXWBUFFER", VarFlag.None, ctype4, null, \u0003);
			}
		}

		// Token: 0x060020C0 RID: 8384 RVA: 0x0006FDC4 File Offset: 0x0006DFC4
		private static string \u0001(_ICompileContext \u0002, int \u0003, int \u0004)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (\u0003 > 0)
			{
				stringBuilder.AppendLine(string.Format("MAXBUFFER := {0};", \u0003 + 10));
			}
			if (\u0004 > 0)
			{
				stringBuilder.AppendLine(string.Format("MAXWBUFFER := {0};", \u0004 + 10));
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060020C1 RID: 8385 RVA: 0x0006FE1C File Offset: 0x0006E01C
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, int \u0004)
		{
			for (int i = 0; i < \u0004; i++)
			{
				string stVariableName = string.Format("Index_{0}", i);
				\u0019.\u0003.Builder.AddLocalStackVariableToCompiledSignature(stVariableName, VarFlag.None, \u0019.\u0003.Builder.CreateDIntType(), null, \u0003);
			}
			Locator.\u0001(\u0002.DataManager, \u0002, null, \u0003, null);
		}

		// Token: 0x060020C2 RID: 8386 RVA: 0x0006FE70 File Offset: 0x0006E070
		internal static bool \u0001(_ISignature \u0002)
		{
			return \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_REFLECTION) && (\u0002.POUType == Operator.FunctionBlock || \u0002.POUType != Operator.Struct);
		}

		// Token: 0x060020C3 RID: 8387 RVA: 0x0006FE9C File Offset: 0x0006E09C
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, string \u0004, IEnumerable<_ISignature> \u0005)
		{
			if (\u0002[\u0004] != null)
			{
				return;
			}
			_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext[\u0004];
			Debug.\u0001(isignature != null);
			_ISignature isignature2 = null;
			if (\u0003 != null)
			{
				isignature2 = (\u0003.GetSignature(isignature.ObjectGuid) as _ISignature);
			}
			_ISignature isignature3 = \u0002.CreateCompiledSignature(isignature, isignature2, APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext, \u0003);
			_ICompiledPOU icompiledPOU = APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext.GetCompiledPOU(isignature.ObjectGuid) as _ICompiledPOU;
			\u0002.AddSignature(isignature3, isignature2, \u0003, true);
			IScope5 u = global::\u0007.\u0005.\u0001(\u0002, isignature3.Id);
			global::\u0014.\u0013.\u0002(isignature3, u, \u0002);
			Locator.\u0001(isignature3, isignature2, \u0002, \u0003);
			Locator.\u0001(\u0002, isignature3, isignature2);
			_ICompiledPOU icompiledPOU2 = \u0019.\u0003.\u0001(\u0004);
			icompiledPOU2.SignatureId = isignature3.Id;
			_ISequenceStatement isequenceStatement = icompiledPOU.GetParseTree().Duplicate() as _ISequenceStatement;
			ReflectionAssignmentCoder.\u0001(isequenceStatement, \u0002, icompiledPOU2);
			icompiledPOU2.SetParseTree(isequenceStatement);
			icompiledPOU2.SetFlag(CompiledPOUFlags.Typified | CompiledPOUFlags.ToCompile, true);
			icompiledPOU2.SetFlagInternal(InternalCompiledPOUFlags.ImplicitInitialisationCodeAdded, true);
			\u0002.AddSignature(isignature3, isignature2, \u0003);
			icompiledPOU2.SignatureId = isignature3.Id;
			icompiledPOU2.ObjectGuid = icompiledPOU.ObjectGuid;
			icompiledPOU2.Checksum = icompiledPOU.Checksum;
			icompiledPOU2.SetFlag(CompiledPOUFlags.Generated, true);
			\u0002.AddCompiledPOUSimple(icompiledPOU2);
		}

		// Token: 0x060020C4 RID: 8388 RVA: 0x0006FFEC File Offset: 0x0006E1EC
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004, bool \u0005, IEnumerable<_ISignature> \u0006)
		{
			ReflectionAssignmentCoder.\u0001(\u0002, \u0003, "__ReflectionInitialisation", \u0006);
			ReflectionAssignmentCoder.\u0001(\u0002, \u0003, "__ReflectionInitialisationForOnlineChange", \u0006);
			if (\u0004)
			{
				ReflectionAssignmentCoder.\u0001(\u0002, \u0003, "__StringAppend", \u0006);
				ReflectionAssignmentCoder.\u0001(\u0002, \u0003, "__APPEND_INT_TO_STRING", \u0006);
				ReflectionAssignmentCoder.\u0001(\u0002, \u0003, "__StringCopyChecked", \u0006);
			}
			if (\u0005)
			{
				ReflectionAssignmentCoder.\u0001(\u0002, \u0003, "__WStringAppend", \u0006);
				ReflectionAssignmentCoder.\u0001(\u0002, \u0003, "__APPEND_INT_TO_WSTRING", \u0006);
				ReflectionAssignmentCoder.\u0001(\u0002, \u0003, "__WStringCopyChecked", \u0006);
			}
		}

		// Token: 0x060020C5 RID: 8389 RVA: 0x00070070 File Offset: 0x0006E270
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, IList<_ICompiledPOU> \u0004)
		{
			ReflectionAssignmentCoder.\u0002(\u0002, \u0003);
			if (ReflectionAssignmentCoder.\u0001(\u0002, \u0003))
			{
				_ICompiledPOU icompiledPOU = \u0002.GetCompiledPOU(new Guid("7A8C8F68-910B-4A57-8487-30DA25617DE6")) as _ICompiledPOU;
				_ICompiledPOU icompiledPOU2 = \u0002.GetCompiledPOU(new Guid("C585A820-2459-45A4-B0C9-6B24AE575D30")) as _ICompiledPOU;
				icompiledPOU2.SetFlag(CompiledPOUFlags.ToCompile, true);
				icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
				\u0004.Add(icompiledPOU2);
				\u0004.Add(icompiledPOU);
			}
		}

		// Token: 0x060020C6 RID: 8390 RVA: 0x000700D8 File Offset: 0x0006E2D8
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			ReflectionAssignmentCoder.\u0002(\u0002, \u0003);
			ReflectionAssignmentCoder.\u0001(\u0002, \u0003);
		}

		// Token: 0x060020C7 RID: 8391 RVA: 0x000700EC File Offset: 0x0006E2EC
		private static bool \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (_ISignature u in \u0002.AllFlat.Where(new Func<_ISignature, bool>(ReflectionAssignmentCoder.<>c.<>9.\u0001)))
			{
				ReflectionCodinglistGenerator reflectionCodinglistGenerator = new ReflectionCodinglistGenerator();
				reflectionCodinglistGenerator.\u0001(\u0002, u, lstringBuilder, true);
				num = Math.Max(num, reflectionCodinglistGenerator.NumOfArrayIndexVariables);
				num2 = Math.Max(num2, reflectionCodinglistGenerator.NumOfCharactersNeeded);
				num3 = Math.Max(num3, reflectionCodinglistGenerator.NumOfWCharactersNeeded);
			}
			if (lstringBuilder.Length > 0)
			{
				_ISignature isignature = \u0002[new Guid("7A8C8F68-910B-4A57-8487-30DA25617DE6")];
				Debug.\u0001(isignature != null);
				_ICompiledPOU2 icompiledPOU = \u0002.GetCompiledPOUById(isignature.Id) as _ICompiledPOU2;
				Debug.\u0001(icompiledPOU != null);
				ISignatureSerializable signatureSerializable = isignature as ISignatureSerializable;
				if (signatureSerializable != null)
				{
					signatureSerializable.SerializableVariableIdManagement = 0;
				}
				ReflectionAssignmentCoder.\u0001(\u0002, isignature, num2, num3);
				ReflectionAssignmentCoder.\u0001(\u0002, isignature, num);
				string str = ReflectionAssignmentCoder.\u0001(\u0002, num2, num3);
				string str2 = lstringBuilder.ToString();
				_IStatement parseTree = ReflectionAssignmentCoder.\u0001(str + str2, \u0002, icompiledPOU);
				icompiledPOU.SetParseTree(parseTree);
				return true;
			}
			return false;
		}

		// Token: 0x060020C8 RID: 8392 RVA: 0x00070240 File Offset: 0x0006E440
		private static IEnumerable<_ISignature> \u0001(_ICompileContext \u0002)
		{
			IEnumerable<_ISignature> enumerable = \u0002.AllFlat.Where(new Func<_ISignature, bool>(ReflectionAssignmentCoder.\u0001));
			if (\u0002.ParentContext == null)
			{
				return enumerable;
			}
			return enumerable.Concat(ReflectionAssignmentCoder.\u0001(\u0002.ParentContext));
		}

		// Token: 0x060020C9 RID: 8393 RVA: 0x00070280 File Offset: 0x0006E480
		public static void \u0002(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			List<_ISignature> list = ReflectionAssignmentCoder.\u0001(\u0002).ToList<_ISignature>();
			foreach (_ISignature u in list)
			{
				ReflectionCodinglistGenerator reflectionCodinglistGenerator = new ReflectionCodinglistGenerator();
				reflectionCodinglistGenerator.\u0001(\u0002, u, lstringBuilder, false);
				num = Math.Max(num, reflectionCodinglistGenerator.NumOfArrayIndexVariables);
				num2 = Math.Max(num2, reflectionCodinglistGenerator.NumOfCharactersNeeded);
				num3 = Math.Max(num3, reflectionCodinglistGenerator.NumOfWCharactersNeeded);
			}
			if (lstringBuilder.Length > 0)
			{
				ReflectionAssignmentCoder.\u0001(\u0002, \u0003, num2 > 0, num3 > 0, list);
				_ISignature isignature = \u0002[new Guid("C585A820-2459-45A4-B0C9-6B24AE575D30")];
				Debug.\u0001(isignature != null);
				_ICompiledPOU2 icompiledPOU = \u0002.GetCompiledPOUById(isignature.Id) as _ICompiledPOU2;
				Debug.\u0001(icompiledPOU != null);
				ISignatureSerializable signatureSerializable = isignature as ISignatureSerializable;
				if (signatureSerializable != null)
				{
					signatureSerializable.SerializableVariableIdManagement = 0;
				}
				ReflectionAssignmentCoder.\u0001(\u0002, isignature, num2, num3);
				ReflectionAssignmentCoder.\u0001(\u0002, isignature, num);
				string str = ReflectionAssignmentCoder.\u0001(\u0002, num2, num3);
				string str2 = lstringBuilder.ToString();
				_IStatement parseTree = ReflectionAssignmentCoder.\u0001(str + str2, \u0002, icompiledPOU);
				icompiledPOU.SetParseTree(parseTree);
			}
		}
	}
}
