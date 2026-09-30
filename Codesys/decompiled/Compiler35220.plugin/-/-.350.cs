using System;
using System.Collections.Generic;
using \u0003;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u0019
{
	// Token: 0x02000393 RID: 915
	internal sealed class \u0014 : _ICompilerMessageCreator
	{
		// Token: 0x06003528 RID: 13608 RVA: 0x000D1CCC File Offset: 0x000CFECC
		private \u0014()
		{
		}

		// Token: 0x170008B9 RID: 2233
		// (get) Token: 0x06003529 RID: 13609 RVA: 0x000D1CD4 File Offset: 0x000CFED4
		internal static \u0019.\u0014 Instance
		{
			get
			{
				if (\u0019.\u0014.\u0001 == null)
				{
					\u0019.\u0014.\u0001 = new \u0019.\u0014();
				}
				return \u0019.\u0014.\u0001;
			}
		}

		// Token: 0x0600352A RID: 13610 RVA: 0x000D1CEC File Offset: 0x000CFEEC
		public void \u0001(LList<_ICompilerMessage> \u0002, IEnumerable<IContainerLibIssue> \u0003)
		{
			if (\u0002 != null && \u0003 != null)
			{
				foreach (IContainerLibIssue containerLibIssue in \u0003)
				{
					string text = null;
					INotAllowedSignatureInContainerLibIssue notAllowedSignatureInContainerLibIssue = containerLibIssue as INotAllowedSignatureInContainerLibIssue;
					if (notAllowedSignatureInContainerLibIssue == null)
					{
						IMissingPublishSymbolsInContainerIssue missingPublishSymbolsInContainerIssue = containerLibIssue as IMissingPublishSymbolsInContainerIssue;
						if (missingPublishSymbolsInContainerIssue != null)
						{
							text = global::\u0003.\u0006.\u0001(MessageId.Err_NotAllowedInContainerLib, new object[]
							{
								missingPublishSymbolsInContainerIssue.Library
							});
							text += ": ";
							text += \u0081.\u0002.PublishSymbolsMustBeSet;
						}
					}
					else
					{
						text = global::\u0003.\u0006.\u0001(MessageId.Err_NotAllowedInContainerLib, new object[]
						{
							Scanner.GetTextOfOperator(notAllowedSignatureInContainerLibIssue.Signature.POUType, false)
						});
					}
					if (text != null)
					{
						_ICompilerMessage icompilerMessage = \u0019.\u0003.\u0001(containerLibIssue.Position, text, containerLibIssue.Severity, MessageId.Err_NotAllowedInContainerLib);
						\u0002.Add(icompilerMessage);
					}
				}
			}
		}

		// Token: 0x0600352B RID: 13611 RVA: 0x000D1DD8 File Offset: 0x000CFFD8
		public void \u0001(LList<_ICompilerMessage> \u0002, IEnumerable<INamespaceConflictIssue> \u0003)
		{
			if (\u0002 != null && \u0003 != null)
			{
				foreach (INamespaceConflictIssue namespaceConflictIssue in \u0003)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_LibraryConflict, new object[]
					{
						namespaceConflictIssue.Namespace,
						namespaceConflictIssue.Library
					});
					_ICompilerMessage icompilerMessage = \u0019.\u0003.\u0001(namespaceConflictIssue.Position, u, namespaceConflictIssue.Severity, MessageId.Err_LibraryConflict);
					\u0002.Add(icompilerMessage);
				}
			}
		}

		// Token: 0x0600352C RID: 13612 RVA: 0x000D1E64 File Offset: 0x000D0064
		public void \u0001(List<_ICompilerMessage> \u0002, IEnumerable<INamespaceConflictIssue> \u0003)
		{
			if (\u0002 != null && \u0003 != null)
			{
				foreach (INamespaceConflictIssue namespaceConflictIssue in \u0003)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_LibraryConflict, new object[]
					{
						namespaceConflictIssue.Namespace,
						namespaceConflictIssue.Library
					});
					_ICompilerMessage item = \u0019.\u0003.\u0001(namespaceConflictIssue.Position, u, namespaceConflictIssue.Severity, MessageId.Err_LibraryConflict);
					\u0002.Add(item);
				}
			}
		}

		// Token: 0x04000A54 RID: 2644
		private static \u0019.\u0014 \u0001;
	}
}
