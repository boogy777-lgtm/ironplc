using System;
using System.IO;
using \u0007;
using \u0014;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0082
{
	// Token: 0x02000213 RID: 531
	internal sealed class \u000F
	{
		// Token: 0x0600231D RID: 8989 RVA: 0x000789D8 File Offset: 0x00076BD8
		internal \u000F(_ICompileContext \u0001\u0002, IScope5 \u009B\u0002, ICodegenerator \u0007\u0004)
		{
			this.\u0001 = \u009B\u0002;
			this.\u0001 = \u0001\u0002;
			this.\u0001 = \u0007\u0004;
		}

		// Token: 0x0600231E RID: 8990 RVA: 0x000789F8 File Offset: 0x00076BF8
		internal void \u0001(_IVariable \u0002, _ISignature \u0003, _ISignature \u0004, bool \u0005)
		{
			ChunkedMemoryStream chunkedMemoryStream = new ChunkedMemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(chunkedMemoryStream);
			string u = string.Format("__BlobInit__{0}__{1}", \u0004.Id, \u0002.Id);
			byte[] array = new byte[\u0002.CompiledType.Size(this.\u0001)];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = 0;
			}
			binaryWriter.Write(array);
			binaryWriter.Seek(0, SeekOrigin.Begin);
			\u0018.\u0008 u2 = new \u0018.\u0008(binaryWriter, this.\u0001, this.\u0001.MotorolaByteOrder, \u0002);
			try
			{
				((_IExpression)\u0002.Initial).Accept(u2);
			}
			catch (BlobInitException ex)
			{
				IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
				_ICompilerMessage icompilerMessage = \u0019.\u0003.\u0001(ex._IExprement.Position, ex.Message, Severity.Error, ex.MessageId);
				icompilerMessage.ObjectGuid = \u0003.ObjectGuid;
				icompilerMessage.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0003.LibraryPath);
				APEnvironmentFacade.Instance.AddMessage(messageCategory, icompilerMessage);
			}
			catch (Exception ex2)
			{
				IMessageCategory messageCategory2 = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
				_ICompilerMessage message = \u0019.\u0003.\u0001(null, ex2.ToString(), Severity.Error, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(messageCategory2, message);
			}
			binaryWriter.Flush();
			binaryWriter.BaseStream.Flush();
			_ICompiledPOU icompiledPOU = \u0019.\u0003.\u0001(u);
			icompiledPOU.SetFlag(CompiledPOUFlags.DataRelocations, true);
			_ICompiledCodeData icompiledCodeData = \u0019.\u0003.\u0001(chunkedMemoryStream, \u0004.Id);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			icompiledCodeData.RelocationList = u2.Relocationlist;
			icompiledPOU.CompiledCode = icompiledCodeData;
			icompiledPOU.CompiledCode.Location = \u0002.DataLocation;
			icompiledPOU.SetFlag(CompiledPOUFlags.ConstBlob, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToRemoveAfterDownload, \u0005);
			icompiledPOU.SetParseTree(\u0019.\u0003.\u0001());
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("VAR_GLOBAL");
			lstringBuilder.AppendLine("END_VAR");
			_ISignature isignature = ParserHelper.\u0001(u, lstringBuilder.ToString(), true);
			isignature = isignature.CreateCompiledSignature(null, this.\u0001.HasByteSupport());
			isignature.SetFlag(SignatureFlag.Generated, true);
			isignature.SetFlag(SignatureFlag.ToRemoveAfterDownload, \u0005);
			IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001, isignature.Id);
			scope.SetLocalSignature(isignature);
			global::\u0014.\u0013.\u0002(isignature, scope, this.\u0001);
			this.\u0001.AddSignature(isignature, null, null, true);
			this.\u0001.AddCompiledPOU(icompiledPOU, isignature, null);
		}

		// Token: 0x0400062B RID: 1579
		private readonly IScope5 \u0001;

		// Token: 0x0400062C RID: 1580
		private readonly _ICompileContext \u0001;

		// Token: 0x0400062D RID: 1581
		private readonly ICodegenerator \u0001;
	}
}
