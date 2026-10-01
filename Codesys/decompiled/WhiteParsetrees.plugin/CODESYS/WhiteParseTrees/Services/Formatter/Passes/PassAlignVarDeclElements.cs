using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter.Passes
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class PassAlignVarDeclElements : StatementFormatterVisitor
	{
		public static void Execute(IWhiteSequenceStatement sequenceStatement, IFormatterSettings settings)
		{
			new PassAlignVarDeclElements(settings).Perform(sequenceStatement);
		}

		public override void Perform(IWhiteSequenceStatement sequenceStatement)
		{
			if (_settings.AlignVariableDeclarationInitializations || _settings.AlignVariableDeclarationsToLongestName || _settings.AlignVariableDeclarationTrailingComments)
			{
				sequenceStatement.Accept(new PassAlignVarDeclElements(_settings), 0);
			}
		}

		private PassAlignVarDeclElements(IFormatterSettings settings)
			: base(settings, null)
		{
		}

		public override bool visit(IWhiteEnumDeclarationStatement statement, int indentation)
		{
			List<IWhiteExpression> nodes = statement.EnumerationTypeExpression.Definitions.ToList();
			if (_settings.AlignVariableDeclarationInitializations)
			{
				AlignEnumDeclInitValues(nodes);
			}
			return base.visit(statement, indentation);
		}

		public override bool visit(IWhiteVariableDeclarationListStatement statement, int indentation)
		{
			List<INode> nodes = statement.Declarations.GetChildren().ToList();
			if (_settings.AlignVariableDeclarationsToLongestName)
			{
				AlignVarDeclsName(nodes);
			}
			if (_settings.AlignVariableDeclarationInitializations)
			{
				AlignVarDeclsInitValues(nodes);
			}
			if (_settings.AlignVariableDeclarationTrailingComments)
			{
				AlignVarDeclsTrailingComments(nodes);
			}
			return base.visit(statement, indentation);
		}

		private void AlignVarDeclsName(IEnumerable<INode> nodes)
		{
			Dictionary<IWhiteVariableDeclarationStatement, int> dictionary = new Dictionary<IWhiteVariableDeclarationStatement, int>();
			List<INode> list = nodes.ToList();
			if (!list.Any())
			{
				return;
			}
			foreach (INode item in list)
			{
				if (item is IWhiteVariableDeclarationStatement whiteVariableDeclarationStatement)
				{
					string text = StringConverter.ConvertToString(whiteVariableDeclarationStatement).Split(':').FirstOrDefault();
					if (text != null)
					{
						int length = text.Length;
						dictionary.Add(whiteVariableDeclarationStatement, length);
					}
				}
			}
			int num = dictionary.Values.Max();
			foreach (KeyValuePair<IWhiteVariableDeclarationStatement, int> item2 in dictionary)
			{
				int num2 = num - item2.Value;
				for (int i = 0; i < num2; i++)
				{
					AddSpace(item2.Key.Colon);
				}
			}
		}

		private void AlignEnumDeclInitValues(IEnumerable<INode> nodes)
		{
			Dictionary<IWhiteAssignmentExpression, int> dictionary = new Dictionary<IWhiteAssignmentExpression, int>();
			List<INode> list = nodes.ToList();
			if (!list.Any())
			{
				return;
			}
			foreach (INode item in list)
			{
				if (item is IWhiteAssignmentExpression whiteAssignmentExpression)
				{
					int value = StringConverter.ConvertToString(whiteAssignmentExpression).LastIndexOf(':');
					dictionary.Add(whiteAssignmentExpression, value);
				}
				else if (item is ILeadByCommaExpression leadByCommaExpression && leadByCommaExpression.Expression is IWhiteAssignmentExpression whiteAssignmentExpression2)
				{
					int value2 = StringConverter.ConvertToString(whiteAssignmentExpression2).LastIndexOf(':');
					dictionary.Add(whiteAssignmentExpression2, value2);
				}
			}
			if (dictionary.Count < 2)
			{
				return;
			}
			int num = dictionary.Values.Max();
			foreach (KeyValuePair<IWhiteAssignmentExpression, int> item2 in dictionary)
			{
				int num2 = num - item2.Value;
				for (int i = 0; i < num2; i++)
				{
					AddSpace(item2.Key.AssignmentToken);
				}
			}
		}

		private void AlignVarDeclsInitValues(IEnumerable<INode> nodes)
		{
			Dictionary<IWhiteVariableDeclarationStatement, int> dictionary = new Dictionary<IWhiteVariableDeclarationStatement, int>();
			List<INode> list = nodes.ToList();
			if (!list.Any())
			{
				return;
			}
			foreach (INode item in list)
			{
				if (item is IWhiteVariableDeclarationStatement whiteVariableDeclarationStatement && whiteVariableDeclarationStatement.InitializationExpression != null)
				{
					int value = StringConverter.ConvertToString(whiteVariableDeclarationStatement).LastIndexOf(':');
					dictionary.Add(whiteVariableDeclarationStatement, value);
				}
			}
			if (dictionary.Count < 2)
			{
				return;
			}
			int num = dictionary.Values.Max();
			foreach (KeyValuePair<IWhiteVariableDeclarationStatement, int> item2 in dictionary)
			{
				int num2 = num - item2.Value;
				for (int i = 0; i < num2; i++)
				{
					AddSpace(item2.Key.Assignment);
				}
			}
		}

		private void AlignVarDeclsTrailingComments(IEnumerable<INode> nodes)
		{
			Dictionary<IWhiteStatement, int> dictionary = new Dictionary<IWhiteStatement, int>();
			List<INode> list = nodes.ToList();
			if (!list.Any())
			{
				return;
			}
			IWhiteStatement whiteStatement = null;
			foreach (INode item in list)
			{
				if (item is IWhiteVariableDeclarationStatement whiteVariableDeclarationStatement)
				{
					whiteStatement = whiteVariableDeclarationStatement;
				}
				else if (item is IWhiteCommentStatement key && whiteStatement != null)
				{
					int length = StringConverter.ConvertToString(whiteStatement).Length;
					dictionary.Add(key, length);
					whiteStatement = null;
				}
				else if (item is IWhiteDocuCommentStatement key2 && whiteStatement != null)
				{
					int length2 = StringConverter.ConvertToString(whiteStatement).Length;
					dictionary.Add(key2, length2);
					whiteStatement = null;
				}
			}
			if (dictionary.Count < 2)
			{
				return;
			}
			int num = dictionary.Values.Max();
			foreach (KeyValuePair<IWhiteStatement, int> item2 in dictionary)
			{
				int num2 = num - item2.Value;
				for (int i = 0; i < num2; i++)
				{
					AddSpace(item2.Key);
				}
			}
		}
	}
}
