using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0004;
using \u0007;
using \u0011;
using \u0014;
using \u0016;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u001C
{
	// Token: 0x020003DE RID: 990
	internal sealed class \u0015
	{
		// Token: 0x170008FE RID: 2302
		// (get) Token: 0x06003758 RID: 14168 RVA: 0x000E3894 File Offset: 0x000E1A94
		private _ICompileContext CompileContext { get; }

		// Token: 0x170008FF RID: 2303
		// (get) Token: 0x06003759 RID: 14169 RVA: 0x000E389C File Offset: 0x000E1A9C
		private Codegeneration Codegeneration { get; }

		// Token: 0x17000900 RID: 2304
		// (get) Token: 0x0600375A RID: 14170 RVA: 0x000E38A4 File Offset: 0x000E1AA4
		// (set) Token: 0x0600375B RID: 14171 RVA: 0x000E38AC File Offset: 0x000E1AAC
		public bool AllocationError { get; set; }

		// Token: 0x0600375C RID: 14172 RVA: 0x000E38B8 File Offset: 0x000E1AB8
		public \u0015(_ICompileContext \u001C\u0004, Codegeneration \u000E\u0002)
		{
			this.CompileContext = \u001C\u0004;
			this.Codegeneration = \u000E\u0002;
		}

		// Token: 0x0600375D RID: 14173 RVA: 0x000E38D0 File Offset: 0x000E1AD0
		internal _ICompiledPOU \u0001(global::\u0016.\u0016 \u0002, global::\u0016.\u0016 \u0003, _ISignature \u0004, _ISignature \u0005, _ICompiledPOU \u0006, _ICompiledPOU \u0007)
		{
			IList<\u001A> list = \u0002.Lists;
			_IVariable ivariable = \u0005["__RELOCATIONTABLE"] as _IVariable;
			ivariable.DataLocation = \u0006.CompiledCode.Location;
			ivariable.SetFlag(VarFlag.Absolut | VarFlag.NoInit, true);
			\u0019.\u0003.\u0001();
			_ICompiledPOU icompiledPOU = \u0019.\u0003.\u0001(IdentifierConstants.RelocateCodeName);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToRemoveAfterDownload, true);
			IScope5 scope = global::\u0007.\u0005.\u0001(this.CompileContext, \u0004.Id);
			scope.LocalSignature = \u0005;
			scope.MethodSignature = \u0004;
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendFormat("IF __dwVersion = 0 THEN\r\n                __PAreaOffsets := __PAreaOffsetsNew;\r\n              ELSE\r\n                __PAreaOffsets := __PAreaOffsetsIn;\r\n              END_IF", Array.Empty<object>());
			lstringBuilder.AppendLine("{__INTERNAL_POINTEROP}");
			lstringBuilder.AppendLine(string.Format("__PRELOCATIONTABLE := ADR(__RELOCATIONTABLE) + __PAreaOffsetsNew^[{0}];", ivariable.DataLocation.Area));
			for (int i = 0; i < list.Count; i++)
			{
				\u001A u001A = list[i];
				lstringBuilder.AppendLine(string.Format("FOR __i := 0 TO {0} DO", \u0002.\u0001[i]));
				lstringBuilder.Append("__WHIGH := __PRELOCATIONTABLE^[0];");
				lstringBuilder.Append("__PRELOCATIONTABLE := __PRELOCATIONTABLE + SIZEOF(__WHIGH);");
				lstringBuilder.Append("__WCOUNT := __PRELOCATIONTABLE^[0]-1;");
				lstringBuilder.Append("__PRELOCATIONTABLE := __PRELOCATIONTABLE + SIZEOF(__WCOUNT);");
				lstringBuilder.AppendLine("FOR __j := 0 TO __WCOUNT DO");
				lstringBuilder.Append("__WLOW := __PRELOCATIONTABLE^[0];");
				lstringBuilder.Append("__PRELOCATIONTABLE := __PRELOCATIONTABLE + SIZEOF(__WLOW);");
				lstringBuilder.Append("__DWOFFSET := SHL(WORD_TO_DWORD(__WHIGH), 16) OR WORD_TO_DWORD(__WLOW);");
				if (u001A.\u0001)
				{
					if (this.Codegeneration.PointerSize == 8)
					{
						lstringBuilder.AppendLine(string.Format("__pAdrHelp := __PAreaOffsetsNew^[{0}] + DWORD_TO_LWORD(__DWOFFSET);", u001A.\u0001));
					}
					else
					{
						lstringBuilder.AppendLine(string.Format("__pAdrHelp := __PAreaOffsetsNew^[{0}] + __DWOFFSET;", u001A.\u0001));
					}
					lstringBuilder.AppendLine(string.Format("__pAdrHelp^ := __pAdrHelp^ + __PAreaOffsets^[{0}];", u001A.\u0002));
				}
				else
				{
					lstringBuilder.AppendLine(string.Format("__RELOCATE__RESULT := __RELOC(__PAreaOffsetsNew^[{0}] + __DWOFFSET, __PAreaOffsets^[{1}]);", u001A.\u0001, u001A.\u0002));
				}
				lstringBuilder.AppendLine("END_FOR");
				lstringBuilder.AppendLine("END_FOR");
			}
			if (\u0003 != null)
			{
				IList<\u001A> list2 = \u0003.Lists;
				_IVariable ivariable2 = \u0005["__RELOCATIONTABLENEW"] as _IVariable;
				ivariable2.DataLocation = \u0007.CompiledCode.Location;
				ivariable2.SetFlag(VarFlag.Absolut | VarFlag.NoInit, true);
				lstringBuilder.AppendLine("{__INTERNAL_POINTEROP}");
				lstringBuilder.AppendLine(string.Format("__PRELOCATIONTABLE := ADR(__RELOCATIONTABLENEW) + __PAreaOffsetsNew^[{0}];", ivariable2.DataLocation.Area));
				for (int j = 0; j < list2.Count; j++)
				{
					\u001A u001A2 = list2[j];
					lstringBuilder.AppendLine(string.Format("FOR __i := 0 TO {0} DO", \u0003.\u0001[j]));
					lstringBuilder.Append("__WHIGH := __PRELOCATIONTABLE^[0];");
					lstringBuilder.Append("__PRELOCATIONTABLE := __PRELOCATIONTABLE + SIZEOF(__WHIGH);");
					lstringBuilder.Append("__WCOUNT := __PRELOCATIONTABLE^[0]-1;");
					lstringBuilder.Append("__PRELOCATIONTABLE := __PRELOCATIONTABLE + SIZEOF(__WCOUNT);");
					lstringBuilder.AppendLine("FOR __j := 0 TO __WCOUNT DO");
					lstringBuilder.Append("__WLOW := __PRELOCATIONTABLE^[0];");
					lstringBuilder.Append("__PRELOCATIONTABLE := __PRELOCATIONTABLE + SIZEOF(__WLOW);");
					lstringBuilder.Append("__DWOFFSET := SHL(WORD_TO_DWORD(__WHIGH), 16) OR WORD_TO_DWORD(__WLOW);");
					lstringBuilder.AppendLine(string.Format("__RELOCATE__RESULT := __RELOC(__PAreaOffsetsNew^[{0}] + __DWOFFSET, __PAreaOffsetsNew^[{1}]);", u001A2.\u0001, u001A2.\u0002));
					lstringBuilder.AppendLine("END_FOR");
					lstringBuilder.AppendLine("END_FOR");
				}
			}
			_IStatement istatement = new global::\u0011.\u0006(lstringBuilder.ToString(), true).\u0001();
			icompiledPOU.SetParseTree(istatement);
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
			this.CompileContext.AddCompiledPOU(icompiledPOU, \u0004, null);
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, this.CompileContext, false, icompiledPOU);
			istatement.Accept(ivisit);
			TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope, this.CompileContext);
			istatement.Accept(ivisit2);
			scope.LocalSignature = \u0005;
			scope.MethodSignature = \u0004;
			icompiledPOU.SetParseTree(istatement);
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			this.Codegeneration.\u0001(icompiledPOU, \u0005, \u0004);
			return icompiledPOU;
		}

		// Token: 0x0600375E RID: 14174 RVA: 0x000E3D00 File Offset: 0x000E1F00
		internal _ISignature \u0001()
		{
			_IDataManager dataManager = this.CompileContext.DataManager;
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("FUNCTION " + IdentifierConstants.RelocateCodeName + " : BOOL");
			lstringBuilder.AppendLine("VAR_INPUT");
			int num = dataManager.FirstArea + (int)dataManager.AreaCount - 1;
			if (this.CompileContext.PointerSize == 8)
			{
				lstringBuilder.AppendLine(string.Format("__PAreaOffsetsNew : POINTER TO ARRAY [0..{0}] OF LWORD;", num));
			}
			else
			{
				lstringBuilder.AppendLine(string.Format("__PAreaOffsetsNew : POINTER TO ARRAY [0..{0}] OF DWORD;", num));
			}
			lstringBuilder.AppendLine("__dwVersion : DWORD;");
			if (this.CompileContext.PointerSize == 8)
			{
				lstringBuilder.AppendLine(string.Format("__PAreaOffsetsIn : POINTER TO ARRAY [0..{0}] OF LWORD;", num));
			}
			else
			{
				lstringBuilder.AppendLine(string.Format("__PAreaOffsetsIn : POINTER TO ARRAY [0..{0}] OF DWORD;", num));
			}
			lstringBuilder.AppendLine("END_VAR");
			lstringBuilder.AppendLine("VAR");
			if (this.CompileContext.PointerSize == 8)
			{
				lstringBuilder.AppendLine("__pAdrHelp : POINTER TO LWORD := 0;");
			}
			else
			{
				lstringBuilder.AppendLine("__pAdrHelp : POINTER TO DWORD := 0;");
			}
			lstringBuilder.AppendLine("__i : INT := 0;");
			lstringBuilder.AppendLine("__j : UINT := 0;");
			lstringBuilder.AppendLine("__RELOCATE__RESULT : BOOL;");
			lstringBuilder.AppendLine("__dwOffset : DWORD;");
			lstringBuilder.AppendLine("__wHigh, __wLow, __wCount : WORD;");
			lstringBuilder.AppendLine("__PRELOCATIONTABLE : POINTER TO ARRAY [0..0] OF WORD;");
			lstringBuilder.AppendLine(string.Format("__PAreaOffsets : POINTER TO ARRAY [0..{0}] OF POINTER TO BYTE;", num));
			lstringBuilder.AppendLine("END_VAR");
			_ISignature isignature = ParserHelper.\u0001(lstringBuilder.ToString(), true);
			isignature.SetFlag(SignatureFlag.Generated, true);
			isignature = isignature.CreateCompiledSignature(null, this.CompileContext.HasByteSupport());
			isignature.SetFlag(SignatureFlag.ToRemoveAfterDownload | SignatureFlag.NoCompareWithNew, true);
			IScope5 scope = global::\u0007.\u0005.\u0001(this.CompileContext, isignature.Id);
			scope.LocalSignature = isignature;
			global::\u0014.\u0013.\u0002(isignature, scope, this.CompileContext);
			Locator.\u0001(isignature, null, this.CompileContext, null);
			Locator.\u0001(this.CompileContext, isignature, null);
			return isignature;
		}

		// Token: 0x04000ADB RID: 2779
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x04000ADC RID: 2780
		[CompilerGenerated]
		private readonly Codegeneration \u0001;

		// Token: 0x04000ADD RID: 2781
		[CompilerGenerated]
		private bool \u0001;
	}
}
