using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[DebuggerDisplay("({Text}, {Expression}")]
	internal class ExpressionAttributedString : AttributedString, IExpressionAttributedString, IAttributedString
	{
		public IExpression Expression { get; private set; }

		public bool Available { get; private set; }

		internal ExpressionAttributedString(IExpression expr, bool bAvailable)
			: base(expr.ToString())
		{
			Expression = expr;
			Available = bAvailable;
		}
	}
}
