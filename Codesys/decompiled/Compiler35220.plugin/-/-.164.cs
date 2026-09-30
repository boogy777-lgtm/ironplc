using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u001F
{
	// Token: 0x020001C7 RID: 455
	internal static class \u0008
	{
		// Token: 0x0600209E RID: 8350 RVA: 0x0006F120 File Offset: 0x0006D320
		private static bool \u0001(\u0018.\u0011 \u0002, _ISignature \u0003, out _ISignature \u0004, out _ISignature \u0005)
		{
			\u0004 = null;
			\u0005 = \u0003;
			if (\u0003 == null)
			{
				return false;
			}
			if (\u0018.\u0003.\u0001(\u0003))
			{
				\u0004 = \u0018.\u0003.\u0001(\u0002.ComCon, \u0003);
				return \u0004 != null;
			}
			if (\u0003.ParentSignatureId != Helper.InvalidId)
			{
				_ISignature u = \u0002.ComCon.GetSignatureById(\u0003.ParentSignatureId) as _ISignature;
				_ISignature isignature;
				if (\u001F.\u0008.\u0001(\u0002, u, out isignature, out \u0005))
				{
					\u0004 = (isignature.GetSubSignature(\u0003.Name) as _ISignature);
				}
			}
			return \u0004 != null && \u0005 != null;
		}

		// Token: 0x0600209F RID: 8351 RVA: 0x0006F1A4 File Offset: 0x0006D3A4
		internal static bool \u0001(\u0018.\u0011 \u0002, _ICompiledPOU \u0003, _ISignature \u0004)
		{
			if (\u0004 == null)
			{
				return false;
			}
			_ISignature isignature;
			_ISignature u;
			if (!\u001F.\u0008.\u0001(\u0002, \u0004, out isignature, out u))
			{
				return false;
			}
			_ICompiledPOU u2 = \u0002.ComCon.GetCompiledPOUById(isignature.Id) as _ICompiledPOU;
			List<IMessage> list = new List<IMessage>();
			\u001F.\u0008.\u0001(\u0002, u2, isignature, list);
			if (list.Any<IMessage>())
			{
				return true;
			}
			List<IMessage> list2 = new List<IMessage>();
			\u001F.\u0008.\u0001(\u0002, \u0003, \u0004, list2);
			return \u001F.\u0008.\u0001(\u0002, u, list2);
		}

		// Token: 0x060020A0 RID: 8352 RVA: 0x0006F210 File Offset: 0x0006D410
		private static void \u0001(\u0018.\u0011 \u0002, _ISignature \u0003, LHashSet<\u001F.\u0008.\u0001> \u0004)
		{
			\u001F.\u0008.\u0002 u = new \u001F.\u0008.\u0002();
			u.\u0001 = \u0003;
			foreach (int nId in u.\u0001.DeclarerIds)
			{
				_ISignature isignature = \u0002.ComCon[nId];
				if (isignature != null)
				{
					IEnumerable<_IVariable> source = isignature.All.OfType<_IVariable>();
					Func<_IVariable, bool> predicate;
					if ((predicate = u.\u0001) == null)
					{
						predicate = (u.\u0001 = new Func<_IVariable, bool>(u.\u0001));
					}
					foreach (_IVariable u001A_u in source.Where(predicate))
					{
						\u0004.Add(new \u001F.\u0008.\u0001(isignature, u001A_u));
					}
				}
			}
		}

		// Token: 0x060020A1 RID: 8353 RVA: 0x0006F2DC File Offset: 0x0006D4DC
		private static bool \u0001(\u0018.\u0011 \u0002, _ISignature \u0003, List<IMessage> \u0004)
		{
			if (!\u0004.Any<IMessage>())
			{
				return true;
			}
			LHashSet<\u001F.\u0008.\u0001> lhashSet = new LHashSet<\u001F.\u0008.\u0001>();
			\u001F.\u0008.\u0001(\u0002, \u0003, lhashSet);
			if (!lhashSet.Any<\u001F.\u0008.\u0001>())
			{
				return false;
			}
			foreach (\u001F.\u0008.\u0001 u in lhashSet)
			{
				string u2 = string.Format(\u0081.\u0002.Err_GenericDeclarationProducesErrorInGeneratedCode, u.Variable.CompiledType);
				_ICompilerMessage u3 = \u0019.\u0003.\u0001(u.Variable._SourcePosition, u2, Severity.Error, MessageId.Err_GenericDeclarationProducesErrorInGeneratedCode);
				\u0002.\u0001(u3, u.Signature);
				\u0002.\u0001(u3, u.Signature, u.Signature.LibraryId);
				foreach (_ICompilerMessage icompilerMessage in \u0004.OfType<_ICompilerMessage>())
				{
					_ICompilerMessage u4 = \u0019.\u0003.\u0001(u.Variable._SourcePosition, icompilerMessage.Text, icompilerMessage.Severity, icompilerMessage.MessageId);
					\u0002.\u0001(u4, u.Signature);
					\u0002.\u0001(u4, u.Signature, u.Signature.LibraryId);
				}
			}
			return true;
		}

		// Token: 0x060020A2 RID: 8354 RVA: 0x0006F42C File Offset: 0x0006D62C
		private static void \u0001(\u0018.\u0011 \u0002, _ICompiledPOU \u0003, _ISignature \u0004, List<IMessage> \u0005)
		{
			\u001F.\u0008.\u0001(\u0002, \u0003, \u0005);
			\u001F.\u0008.\u0001(\u0002, \u0004, \u0005);
		}

		// Token: 0x060020A3 RID: 8355 RVA: 0x0006F440 File Offset: 0x0006D640
		private static void \u0001(\u0018.\u0011 \u0002, _ISignature \u0003, List<IMessage> \u0004)
		{
			if (\u0003 != null)
			{
				foreach (_ICompilerMessage icompilerMessage in \u0003.Messages.OfType<_ICompilerMessage>())
				{
					if (icompilerMessage.Severity == Severity.Error || icompilerMessage.Severity == Severity.FatalError)
					{
						\u0002.\u0001(icompilerMessage, \u0003);
						\u0004.Add(icompilerMessage);
					}
				}
			}
		}

		// Token: 0x060020A4 RID: 8356 RVA: 0x0006F4B0 File Offset: 0x0006D6B0
		private static void \u0001(\u0018.\u0011 \u0002, _ICompiledPOU \u0003, List<IMessage> \u0004)
		{
			if (\u0003 == null)
			{
				return;
			}
			_ICompilerMessage[] array = \u0003.GetFlagInternal(InternalCompiledPOUFlags.TypeCheckDone) ? \u0003.GetMessages(true) : \u0003.Messages;
			if (array == null)
			{
				return;
			}
			foreach (_ICompilerMessage icompilerMessage in array)
			{
				if (icompilerMessage.Severity == Severity.Error || icompilerMessage.Severity == Severity.FatalError)
				{
					\u0002.\u0001(icompilerMessage, \u0003);
					\u0004.Add(icompilerMessage);
				}
			}
		}

		// Token: 0x020001C8 RID: 456
		private sealed class \u0001
		{
			// Token: 0x060020A5 RID: 8357 RVA: 0x0006F518 File Offset: 0x0006D718
			public \u0001(_ISignature \u001C\u0002, _IVariable \u001A\u0002)
			{
				this.Signature = \u001C\u0002;
				this.Variable = \u001A\u0002;
			}

			// Token: 0x17000615 RID: 1557
			// (get) Token: 0x060020A6 RID: 8358 RVA: 0x0006F530 File Offset: 0x0006D730
			public _ISignature Signature { get; }

			// Token: 0x17000616 RID: 1558
			// (get) Token: 0x060020A7 RID: 8359 RVA: 0x0006F538 File Offset: 0x0006D738
			public _IVariable Variable { get; }

			// Token: 0x060020A8 RID: 8360 RVA: 0x0006F540 File Offset: 0x0006D740
			public bool \u0001(object \u0002)
			{
				\u001F.\u0008.\u0001 u = \u0002 as \u001F.\u0008.\u0001;
				return u != null && u.Signature.Id == this.Signature.Id && u.Variable.Id == this.Variable.Id;
			}

			// Token: 0x060020A9 RID: 8361 RVA: 0x0006F58C File Offset: 0x0006D78C
			public int \u0001()
			{
				return this.Signature.Id.GetHashCode() ^ this.Variable.Id.GetHashCode();
			}

			// Token: 0x04000558 RID: 1368
			[CompilerGenerated]
			private readonly _ISignature \u0001;

			// Token: 0x04000559 RID: 1369
			[CompilerGenerated]
			private readonly _IVariable \u0001;
		}

		// Token: 0x020001C9 RID: 457
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x060020AB RID: 8363 RVA: 0x0006F5C8 File Offset: 0x0006D7C8
			internal bool \u0001(_IVariable \u0002)
			{
				IUserdefType userdefType = \u0002.Type as IUserdefType;
				int? num = (userdefType != null) ? new int?(userdefType.SignatureId) : null;
				int id = this.\u0001.Id;
				return num.GetValueOrDefault() == id & num != null;
			}

			// Token: 0x0400055A RID: 1370
			public _ISignature \u0001;

			// Token: 0x0400055B RID: 1371
			public Func<_IVariable, bool> \u0001;
		}
	}
}
