using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter.Passes
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class LineCtx
	{
		private readonly List<int> _indentationScope;

		private readonly List<IWhiteExprement> _callScope;

		private int LineLength { get; set; }

		public int Indentation => _indentationScope.Sum();

		public LineCtx()
		{
			_indentationScope = new List<int> { 0 };
			_callScope = new List<IWhiteExprement>();
			LineLength = 0;
		}

		public void PushCallee(IWhiteExprement exprement)
		{
			_callScope.Add(exprement);
		}

		public void PopCallee()
		{
			_callScope.RemoveAt(_callScope.Count - 1);
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		public IWhiteExprement PeekCallee(int n = 1)
		{
			return _callScope[_callScope.Count - n];
		}

		public void Indent(int num = 1)
		{
			_indentationScope.Add(num);
		}

		public void UnIndent()
		{
			_indentationScope.RemoveAt(_indentationScope.Count - 1);
		}

		public int LengthWithIndent()
		{
			return LineLength + Indentation * 4;
		}

		public void ExtendLineLength(int len)
		{
			LineLength += len;
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		public void ExtendLineLength(IWhiteToken token)
		{
			if (token != null)
			{
				LineLength += TokenSerializer.GetTokenList(token).Sum((IWhiteToken t) => t.Text.Length);
			}
		}

		public void ResetLineLength()
		{
			LineLength = 0;
		}

		public bool IsInExpression()
		{
			return _callScope.Last() is IWhiteExpression;
		}

		public bool IsInStatement()
		{
			return !IsInExpression();
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		public IWhiteExpression GetTopExpressionInChain(out bool upperIsExpressionStatement)
		{
			IWhiteExpression result = null;
			upperIsExpressionStatement = false;
			int num = _callScope.Count - 1;
			while (num >= 0)
			{
				IWhiteExprement whiteExprement = _callScope[num];
				if (whiteExprement is IWhiteExpression whiteExpression)
				{
					result = whiteExpression;
					num--;
					continue;
				}
				if (whiteExprement is IWhiteExpressionStatement)
				{
					upperIsExpressionStatement = true;
				}
				break;
			}
			return result;
		}
	}
}
