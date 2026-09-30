using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using \u0003;
using \u0007;
using \u000E;
using \u0014;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u001B
{
	// Token: 0x02000296 RID: 662
	internal sealed class \u0007
	{
		// Token: 0x06002A0A RID: 10762 RVA: 0x00092184 File Offset: 0x00090384
		private \u0007(_IAssignmentExpression \u001C\u0006, global::\u000E.\u0011 \u001D\u0006)
		{
			this.\u0001 = \u001D\u0006;
			this.\u0001 = \u001C\u0006;
			this.\u0001 = ((this.\u0001._Scope.MethodSignature as _ISignature) ?? (this.\u0001._Scope.LocalSignature as _ISignature));
			this.\u0001 = (_IVariable)this.\u0001._LValue.GetVariable(this.\u0001._Scope);
			this.\u0001 = false;
			if (this.\u0001 != null)
			{
				this.\u0001 = this.\u0001.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST);
			}
		}

		// Token: 0x06002A0B RID: 10763 RVA: 0x00092224 File Offset: 0x00090424
		private _IStatement \u0001()
		{
			if (!this.\u0003())
			{
				return null;
			}
			if (this.\u0001._RValue is _IStructureInitialization || this.\u0001._RValue is _IArrayInitialization || this.\u0001() || this.\u0002())
			{
				return this.\u0002();
			}
			return null;
		}

		// Token: 0x06002A0C RID: 10764 RVA: 0x00092278 File Offset: 0x00090478
		public static _IStatement \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			return new \u001B.\u0007(\u0002, \u0003).\u0001();
		}

		// Token: 0x06002A0D RID: 10765 RVA: 0x00092288 File Offset: 0x00090488
		public static bool \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			return new \u001B.\u0007(\u0002, \u0003).\u0003();
		}

		// Token: 0x06002A0E RID: 10766 RVA: 0x00092298 File Offset: 0x00090498
		private bool \u0001()
		{
			return this.\u0001._LValue.Type != null && TypeTable.IsString(this.\u0001._LValue.Type.Class) && this.\u0001.GetFlag(VarFlag.Constant) && !this.\u0001.GetFlag(VarFlag.ReplacedConstant);
		}

		// Token: 0x06002A0F RID: 10767 RVA: 0x000922F8 File Offset: 0x000904F8
		private bool \u0002()
		{
			_IUserdefType iuserdefType = this.\u0001._LValue.Type as _IUserdefType;
			if (iuserdefType != null && this.\u0001.GetFlag(VarFlag.Constant) && !this.\u0001.GetFlag(VarFlag.ReplacedConstant))
			{
				this.\u0001 = (_ISignature)this.\u0001._Scope[iuserdefType.SignatureId];
				return this.\u0001 != null;
			}
			return false;
		}

		// Token: 0x06002A10 RID: 10768 RVA: 0x00092370 File Offset: 0x00090570
		private _IStatement \u0002()
		{
			ChunkedMemoryStream chunkedMemoryStream = new ChunkedMemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(chunkedMemoryStream);
			_ISignature isignature = (_ISignature)this.\u0001._Scope[this.\u0001._LValue.SignatureId];
			string text = string.Format("__BlobInit__{0}__{1}", isignature.Id, this.\u0001.Id);
			_IArrayType iarrayType = global::\u0019.\u0003.\u0001(TypeTable.Byte);
			iarrayType.AddDimension(global::\u0019.\u0003.\u0001(0L), global::\u0019.\u0003.\u0001((long)(this.\u0001.CompiledType.Size(this.\u0001._Scope) - 1)));
			bool flag = this.\u0001.GetFlag(VarFlag.OnlChangeInit);
			if (this.\u0001.Comcon[text] != null && flag)
			{
				return \u001B.\u0007.\u0001(this.\u0001, this.\u0001, text, this.\u0001, iarrayType, isignature);
			}
			byte[] array = new byte[this.\u0001.CompiledType.Size(this.\u0001._Scope)];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = 0;
			}
			binaryWriter.Write(array);
			binaryWriter.Seek(0, SeekOrigin.Begin);
			global::\u0018.\u0008 u = new global::\u0018.\u0008(binaryWriter, this.\u0001._Scope, this.\u0001.CodeGen.MotorolaByteOrder, this.\u0001);
			if (!\u001B.\u0007.\u0001(this.\u0001, u, isignature))
			{
				return null;
			}
			binaryWriter.Flush();
			binaryWriter.BaseStream.Flush();
			_ICompiledPOU icompiledPOU = this.\u0001(text, chunkedMemoryStream, u, isignature);
			if (this.\u0001)
			{
				return global::\u0019.\u0003.\u0001();
			}
			IDataLocation location = icompiledPOU.CompiledCode.Location;
			text = ArrayInitialisationCodeGenerator.GetPOUUniqueLabel("__LeftForBlob__{0}");
			_IVariable ivariable = this.\u0001(text, iarrayType, isignature);
			text = ArrayInitialisationCodeGenerator.GetPOUUniqueLabel("__Right__{0}");
			_IVariable ivariable2 = global::\u0019.\u0003.\u0001(null);
			this.\u0001(ivariable2, text, location, iarrayType);
			string u2 = ivariable.VersionedName + " := " + ivariable2.VersionedName + ";";
			return this.\u0001.Generator.\u0001(u2, this.\u0001._Scope, this.\u0001.CompiledPOU);
		}

		// Token: 0x06002A11 RID: 10769 RVA: 0x000925BC File Offset: 0x000907BC
		private void \u0001(_IVariable \u0002, string \u0003, IDataLocation \u0004, _IArrayType \u0005)
		{
			\u0002.Name = \u0003;
			\u0002.DataLocation = \u0004;
			\u0002._Type = \u0005;
			\u0002.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit | VarFlag.Implicit, true);
			\u0002.Id = this.\u0001.NextId;
			this.\u0001.AddVariable(\u0002);
		}

		// Token: 0x06002A12 RID: 10770 RVA: 0x0009260C File Offset: 0x0009080C
		private _IVariable \u0001(string \u0002, _IArrayType \u0003, _ISignature \u0004)
		{
			_IVariable ivariable = global::\u0019.\u0003.\u0001(null);
			ivariable.Name = \u0002;
			ivariable.DataLocation = this.\u0001.DataLocation;
			ivariable._Type = \u0003;
			if (this.\u0001.DataLocation.IsRelativ)
			{
				if (this.\u0001.GetFlag(VarFlag.RelativeInstance))
				{
					ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.RelativeInstance | VarFlag.NoInit | VarFlag.Implicit, true);
				}
				else
				{
					ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.RelativeStack | VarFlag.NoInit | VarFlag.Implicit, true);
				}
			}
			else
			{
				ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit | VarFlag.Implicit, true);
			}
			ivariable.Id = \u0004.NextId;
			\u0004.AddVariable(ivariable);
			return ivariable;
		}

		// Token: 0x06002A13 RID: 10771 RVA: 0x000926A4 File Offset: 0x000908A4
		private _ICompiledPOU \u0001(string \u0002, ChunkedMemoryStream \u0003, global::\u0018.\u0008 \u0004, _ISignature \u0005)
		{
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(\u0002);
			icompiledPOU.SetFlag(CompiledPOUFlags.DataRelocations, true);
			_ICompiledCodeData icompiledCodeData = global::\u0019.\u0003.\u0001(\u0003, this.\u0001.Id);
			_ICompiledPOU icompiledPOU2;
			bool flag = this.\u0001(\u0002, out icompiledPOU2, icompiledCodeData);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, !flag);
			icompiledCodeData.RelocationList = \u0004.Relocationlist;
			icompiledPOU.CompiledCode = icompiledCodeData;
			if (this.\u0001)
			{
				icompiledPOU.CompiledCode.Location = this.\u0001.DataLocation;
				icompiledPOU.SetFlag(CompiledPOUFlags.ConstBlob, true);
			}
			else
			{
				icompiledPOU.SetFlag(CompiledPOUFlags.Blob, true);
				if (icompiledPOU2 != null && flag && !this.\u0001.Comcon.DataManager._MemorySettings.OnlineChangeInOwnSegment)
				{
					this.\u0001(icompiledPOU, icompiledPOU2);
				}
				else
				{
					this.\u0001(\u0005, icompiledPOU, icompiledCodeData);
				}
			}
			icompiledPOU.SetParseTree(global::\u0019.\u0003.\u0001());
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("VAR_GLOBAL");
			lstringBuilder.AppendLine("END_VAR");
			_ISignature isignature = ParserHelper.\u0001(\u0002, lstringBuilder.ToString(), true);
			isignature = isignature.CreateCompiledSignature(null, this.\u0001.Comcon.HasByteSupport());
			isignature.SetFlag(SignatureFlag.Generated, true);
			IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001.Comcon, isignature.Id);
			scope.LocalSignature = isignature;
			global::\u0014.\u0013.\u0002(isignature, scope, this.\u0001.Comcon);
			this.\u0001.Comcon.AddSignature(isignature, null, null, true);
			this.\u0001.Comcon.AddCompiledPOU(icompiledPOU, isignature, null);
			return icompiledPOU;
		}

		// Token: 0x06002A14 RID: 10772 RVA: 0x0009284C File Offset: 0x00090A4C
		private void \u0001(_ISignature \u0002, _ICompiledPOU \u0003, _ICompiledCodeData \u0004)
		{
			ushort u = 0;
			int u2 = 0;
			DataSegmentFlags u3 = DataSegmentFlags.Code;
			if (this.\u0001.Comcon.DataManager.HasConstantSegment)
			{
				u3 = DataSegmentFlags.Constant;
			}
			if (!MemoryCompiler.\u0003(this.\u0001.Comcon.DataManager, ref u, ref u2, this.\u0001.Comcon.DataManager._MemorySettings.PackMode, \u0003.CompiledCode.CodeSize, this.\u0001.Comcon.DataManager._MemorySettings.DataSegmentSize, u3))
			{
				string u4 = global::\u0003.\u0006.\u0001(MessageId.Err_NotEnoughMemoryForVariableInit, new object[]
				{
					this.\u0001.VersionedName,
					\u0004.CodeSize
				});
				IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
				_ICompilerMessage icompilerMessage = global::\u0019.\u0003.\u0001(this.\u0001.SourcePosition, u4, Severity.Error, MessageId.Err_NotEnoughMemoryForVariableInit);
				icompilerMessage.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0002.LibraryPath);
				icompilerMessage.ObjectGuid = \u0002.ObjectGuid;
				APEnvironmentFacade.Instance.AddMessage(messageCategory, icompilerMessage);
			}
			\u0003.CompiledCode.Location = global::\u0019.\u0003.\u0001(u, u2);
		}

		// Token: 0x06002A15 RID: 10773 RVA: 0x0009298C File Offset: 0x00090B8C
		private void \u0001(_ICompiledPOU \u0002, _ICompiledPOU \u0003)
		{
			DataSegmentFlags u = DataSegmentFlags.Code;
			if (this.\u0001.Comcon.DataManager.HasConstantSegment)
			{
				u = DataSegmentFlags.Constant;
			}
			\u0002.CompiledCode = \u0003.CompiledCode;
			if (!MemoryCompiler.\u0002(this.\u0001.Comcon.DataManager, \u0002.CompiledCode.Location.Area, \u0002.CompiledCode.Location.Offset, \u0002.CompiledCode.CodeSize, u))
			{
				string u2 = string.Format(\u0081.\u0002.Err_InternalErrorProhibitingOnlineChange, 1);
				IMessage cm = global::\u0019.\u0003.\u0001(null, u2, Severity.Error, MessageId.Err_InternalErrorProhibitingOnlineChange);
				this.\u0001.AddError(cm);
			}
		}

		// Token: 0x06002A16 RID: 10774 RVA: 0x00092A38 File Offset: 0x00090C38
		private bool \u0001(string \u0002, out _ICompiledPOU \u0003, _ICompiledCodeData \u0004)
		{
			bool result = false;
			\u0003 = null;
			if (this.\u0001.Codegeneration.RefContext != null && this.\u0001.Codegeneration.OnlineChange)
			{
				_ISignature isignature = this.\u0001.Codegeneration.RefContext[\u0002];
				if (isignature != null)
				{
					\u0003 = (_ICompiledPOU)this.\u0001.Codegeneration.RefContext.GetCompiledPOUById(isignature.Id);
					_ICompiledCodeData icompiledCodeData = (_ICompiledCodeData)\u0003.CompiledCode;
					if (icompiledCodeData != null && icompiledCodeData.Equals(\u0004))
					{
						result = true;
					}
				}
			}
			return result;
		}

		// Token: 0x06002A17 RID: 10775 RVA: 0x00092AD4 File Offset: 0x00090CD4
		[ExcludeFromCodeCoverage]
		private static bool \u0001(_IAssignmentExpression \u0002, global::\u0018.\u0008 \u0003, _ISignature \u0004)
		{
			try
			{
				\u0002._RValue.Accept(\u0003);
			}
			catch (BlobInitException ex)
			{
				IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
				_ICompilerMessage icompilerMessage = global::\u0019.\u0003.\u0001(ex._IExprement.Position, ex.Message, Severity.Error, ex.MessageId);
				icompilerMessage.ObjectGuid = \u0004.ObjectGuid;
				icompilerMessage.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0004.LibraryPath);
				APEnvironmentFacade.Instance.AddMessage(messageCategory, icompilerMessage);
				return false;
			}
			catch (Exception ex2)
			{
				IMessageCategory messageCategory2 = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, ex2.ToString(), Severity.Error, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(messageCategory2, message);
				return false;
			}
			return true;
		}

		// Token: 0x06002A18 RID: 10776 RVA: 0x00092BB4 File Offset: 0x00090DB4
		private static _IStatement \u0001(_ISignature \u0002, global::\u000E.\u0011 \u0003, string \u0004, _IVariable \u0005, _IArrayType \u0006, _ISignature \u0007)
		{
			ISignature signature = \u0003.Comcon[\u0004];
			_ICompiledPOU icompiledPOU = \u0003.Comcon._GetCompiledPOUById(signature.Id);
			string pouuniqueLabel = ArrayInitialisationCodeGenerator.GetPOUUniqueLabel("__LeftForBlob__{0}");
			_IVariable ivariable = global::\u0019.\u0003.\u0001(null);
			ivariable.Name = pouuniqueLabel;
			ivariable.DataLocation = \u0005.DataLocation;
			ivariable._Type = \u0006;
			if (\u0005.DataLocation.IsRelativ)
			{
				if (\u0005.GetFlag(VarFlag.RelativeInstance))
				{
					ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.RelativeInstance | VarFlag.NoInit | VarFlag.Implicit, true);
				}
				else
				{
					ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.RelativeStack | VarFlag.NoInit | VarFlag.Implicit, true);
				}
			}
			else
			{
				ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit | VarFlag.Implicit, true);
			}
			ivariable.Id = \u0007.NextId;
			\u0007.AddVariable(ivariable);
			string pouuniqueLabel2 = ArrayInitialisationCodeGenerator.GetPOUUniqueLabel("__Right__{0}");
			_IVariable ivariable2 = global::\u0019.\u0003.\u0001(null);
			ivariable2.Name = pouuniqueLabel2;
			ivariable2.DataLocation = icompiledPOU.CompiledCode.Location;
			ivariable2._Type = \u0006;
			ivariable2.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit | VarFlag.Implicit, true);
			ivariable2.Id = \u0002.NextId;
			\u0002.AddVariable(ivariable2);
			string u = ivariable.VersionedName + " := " + ivariable2.VersionedName + ";";
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0003.Comcon, \u0007.Id);
			scope.MethodSignature = \u0002;
			return \u0003.Generator.\u0001(u, scope, \u0003.CompiledPOU);
		}

		// Token: 0x06002A19 RID: 10777 RVA: 0x00092D1C File Offset: 0x00090F1C
		private bool \u0003()
		{
			return this.\u0001 != null && (this.\u0001.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINIT) || this.\u0001.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST)) && !this.\u0001.Comcon.IsDefined("noblobinit");
		}

		// Token: 0x040007AD RID: 1965
		private readonly _IVariable \u0001;

		// Token: 0x040007AE RID: 1966
		private _ISignature \u0001;

		// Token: 0x040007AF RID: 1967
		private readonly _IAssignmentExpression \u0001;

		// Token: 0x040007B0 RID: 1968
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x040007B1 RID: 1969
		private readonly bool \u0001;
	}
}
