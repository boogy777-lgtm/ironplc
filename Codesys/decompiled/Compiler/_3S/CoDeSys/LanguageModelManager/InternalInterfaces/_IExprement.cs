using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IExprement : IExprement3, IExprement2, IExprement
	{
		IList<_ICompilerMessage> MessagesList { get; set; }

		IMinimalPosition _Position { get; set; }

		short PositionLength { get; set; }

		IMinimalPosition PositionIntern { get; set; }

		short LengthIntern { get; set; }

		IExprInfo Info { get; set; }

		void Accept(IExprementVisitor ivisit);

		void AddError(string stError, MessageId mid);

		void AddError(string stError, MessageId mid, Guid messageGuid);

		void AddError(string stError, IToken tokenPos, MessageId mid);

		_IExprement Duplicate();

		IBreakpoint CreateBreakpoint(int nOffset, byte bySize);

		_ICompilerMessage AddMessage(_ICompilerMessage cm);

		_ICompilerMessage AddMessage(_ICompilerMessage cm, bool bAddAlways);

		_ICompilerMessage AddMessage(_ICompilerMessage cm, bool bAddAlways, bool bCompareObjectGuid);

		_ICompilerMessage AddMessage(string stMessage, IMinimalPosition sourcepos, Severity severity, short sLength, MessageId nid);

		void AddWarning(string stError, IToken tokenPos, MessageId mid);

		void AddWarning(string stWarning, MessageId mid);

		void ClearMessages();

		ISourcePosition GetPosition();

		short MergePosition(IMinimalPosition pos1, short sLen1, IMinimalPosition pos2, short sLen2, out bool bMergeDone);

		bool MergePosition(IToken tokenIn);

		void SetPositionIntern(IMinimalPosition minpos);

		void SetPosition(IToken token);

		bool IsEqual(_IExprement exprementRight);
	}
}
