using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeConversion
{
    /// <summary>
    /// Writes code based on a abstract syntax tree.
    /// </summary>
    public abstract class CodeWriter : NodeVisitor
    {
        // protected IntentVisitor IntentVisitor { get; set; }

        /// <summary>
        /// Builder containing the code to write.
        /// </summary>
        protected StringBuilder Builder { get; private set; }
        /// <summary>
        /// The current indentation depth.
        /// </summary>
        protected int IndentationDepth { get; private set; }
        /// <summary>
        /// The character used for indentation. Usually spaces vs tabs.
        /// </summary>
        public string IndentationCharacter { get; set; } = "    ";

        /// <summary>
        /// Writes an abstract syntax tree to a string.
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public string Write(Node ast)
        {
            Builder = new StringBuilder();
            ast.Accept(this);
            return Builder.ToString();
        }

        /// <summary>
        /// The language supported by this code writer.
        /// </summary>
        public abstract Language Language { get; }

        /// <summary>
        /// Appends a string to the output code.
        /// </summary>
        /// <param name="str"></param>
        protected void Append(string str)
        {
            Builder.Append(str);
        }

        /// <summary>
        /// Appends a new line of text to the output code.
        /// </summary>
        /// <param name="str"></param>
        protected void AppendLine(string str)
        {
            Builder.AppendLine(str);
        }

        /// <summary>
        /// Inserts a new line character
        /// </summary>
        protected void NewLine()
        {
            Append(Environment.NewLine);
            for (int i = 0; i < IndentationDepth; i++)
                Append(IndentationCharacter);
        }
        /// <summary>
        /// Indents the code one level.
        /// </summary>
        protected void Indent()
        {
            IndentationDepth++;
        }
        /// <summary>
        /// Outdents the code one level.
        /// </summary>
        protected void Outdent()
        {
            Builder = Builder.Remove(Builder.Length - IndentationCharacter.Length, IndentationCharacter.Length);
            IndentationDepth--;
        }
    }

    public abstract class CStyleCodeWriter : CodeWriter
    {
        private static Dictionary<BinaryOperator, string> _operatorMap;

        protected bool TerminateStatementWithSemiColon { get; set; }
        protected virtual Dictionary<BinaryOperator, string> OperatorMap => _operatorMap;

        static CStyleCodeWriter()
        {
            _operatorMap = new Dictionary<BinaryOperator, string>
            {
                { BinaryOperator.Equal, " == " },
                { BinaryOperator.NotEqual, " != " },
                { BinaryOperator.GreaterThan, " > " },
                { BinaryOperator.LessThan, " < " },
                { BinaryOperator.LessThanEqualTo, " <= " },
                { BinaryOperator.GreaterThanEqualTo, " >= " },
                { BinaryOperator.And, " && " },
                { BinaryOperator.Or, " || " },
                { BinaryOperator.Bor, " | " },
                { BinaryOperator.Minus, " - " },
                { BinaryOperator.Plus, " + " },
                { BinaryOperator.Not, " ! " }
            };
        }

        public override void VisitAssignment(Assignment node)
        {
            node.Left.Accept(this);
            Append(" = ");
            node.Right.Accept(this);
        }

        public override void VisitArgument(Argument node)
        {
            node?.Expression?.Accept(this);
        }

        public override void VisitArgumentList(ArgumentList node)
        {
            foreach (var argument in node.Arguments)
            {
                argument.Accept(this);

                Append(",");
            }

            //Remove trailing comma
            Builder.Remove(Builder.Length - 1, 1);
        }

        public override void VisitBinaryExpression(BinaryExpression node)
        {
            node.Left.Accept(this);

            if (OperatorMap.ContainsKey(node.Operator))
            {
                Append(OperatorMap[node.Operator]);
            }

            node.Right.Accept(this);
        }

        public override void VisitBlock(Block node)
        {
            if (node == null)
            {
                return;
            }

            foreach (var statement in node.Statements)
            {
                if (statement == null)
                {
                    continue;
                }

                statement.Accept(this);

                if (TerminateStatementWithSemiColon && Builder.Length > 0 && Builder[Builder.Length - 1] != '}' && Builder[Builder.Length - 1] != ';')
                {
                    Append(";");
                }

                NewLine();
            }
        }

        public override void VisitBreak(Break node)
        {
            Append("break");
        }

        public override void VisitCast(Cast node)
        {
            Append("(");
            Append(node.Type);
            Append(")");
            node.Expression.Accept(this);
        }

        public override void VisitCatch(Catch node)
        {
            Append("catch");
            if (node.Declaration != null)
            {
                Append(" ");
                node.Declaration.Accept(this);
            }
            NewLine();
            Append("{");
            Indent();
            NewLine();

            node.Block.Accept(this);

            Outdent();
            Append("}");
        }

        public override void VisitCatchDeclaration(CatchDeclaration node)
        {
            Append("(");
            Append(node.Type);
            Append(")");
        }

        public override void VisitContinue(Continue node)
        {
            Append("continue");
        }

        public override void VisitBracketedArgumentList(BracketedArgumentList node)
        {
            Append("[");

            foreach (var argument in node.Arguments)
            {
                argument.Accept(this);

                Append(", ");
            }

            //Remove trailing comma
            Builder.Remove(Builder.Length - 1, 1);

            Append("]");
        }

        public override void VisitElseClause(ElseClause node)
        {
            NewLine();
            Append("else");

            var isIf = node.Body is IfStatement;
            if (!isIf)
            {
                NewLine();
                Append("{");
                Indent();
                NewLine();
            }
            else
            {
                Append(" ");
            }

            node.Body.Accept(this);

            if (!isIf)
            {
                Outdent();
                Append("}");
            }
        }

        public override void VisitFinally(Finally node)
        {
            Append("finally");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            node.Body.Accept(this);

            Outdent();
            Append("}");
        }

        public override void VisitForStatement(ForStatement node)
        {
            Append("for(");

            if (node.Declaration != null)
            {
                node.Declaration.Accept(this);
            }
            else
            {
                foreach (var initializer in node.Initializers)
                {
                    initializer.Accept(this);
                }
            }

            Append("; ");

            node.Condition.Accept(this);

            Append("; ");

            foreach (var incrementor in node.Incrementors)
            {
                incrementor.Accept(this);
            }

            Append(")");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            if (node.Statement is Block)
            {
                node.Statement.Accept(this);
            }
            else
            {
                var block = new Block(node.Statement);
                block.Accept(this);
            }

            Outdent();
            Append("}");
        }

        public override void VisitForEachStatement(ForEachStatement node)
        {
            Append("foreach (");
            node.Identifier.Accept(this);
            Append(" in ");
            node.Expression.Accept(this);
            Append(")");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            if (node.Statement is Block)
            {
                node.Statement.Accept(this);
            }
            else
            {
                var block = new Block(node.Statement);
                block.Accept(this);
            }

            Outdent();
            Append("}");
        }

        public override void VisitIdentifierName(IdentifierName node)
        {
            Append(node.Name);
        }

        public override void VisitIfStatement(IfStatement node)
        {
            Append("if (");
            node.Condition.Accept(this);
            Append(")");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            node.Body.Accept(this);

            Outdent();
            Append("}");

            node.ElseClause?.Accept(this);
        }

        public override void VisitInvocation(Invocation node)
        {
            node.Expression.Accept(this);

            if (!node.Arguments.Arguments.Any())
            {
                Append("()");
            }
            else
            {
                Append("(");
                node.Arguments.Accept(this);
                Append(")");
            }
        }

        public override void VisitLiteral(Literal node)
        {
            Append(node.Token);
        }

        public override void VisitMemberAccess(MemberAccess node)
        {
            if (node == null || node.Expression == null)
            {
                return;
            }

            node.Expression.Accept(this);
            Append(".");
            Append(node.Identifier);
        }

        public override void VisitParameter(Parameter node)
        {
            Append(node.Type);
            Append(" ");
            Append(node.Name);
        }

        public override void VisitParenthesizedExpression(ParenthesizedExpression node)
        {
            Append("(");
            node.Expression.Accept(this);
            Append(")");
        }

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpression node)
        {
            node.Operand.Accept(this);
            Append("++");
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpression node)
        {
            Append("++");
            node.Operand.Accept(this);
        }

        public override void VisitStringConstant(StringConstant node)
        {
            var escapedString = node.Value.Replace("\\", "\\\\");

            Append("\"" + escapedString + "\"");
        }

        public override void VisitSwitchStatement(SwitchStatement node)
        {
            Append("switch (");
            node.Expression.Accept(this);
            Append(")"); NewLine();
            Append("{"); Indent(); NewLine();

            foreach (var section in node.Sections)
            {
                section.Accept(this);
            }

            Outdent();
            Append("}");
        }

        public override void VisitSwitchSection(SwitchSection node)
        {
            foreach (var label in node.Labels)
            {
                var idName = label as IdentifierName;
                if (idName?.Name.Equals("default") == true)
                {
                    Append("default:");
                }
                else
                {
                    Append("case ");
                    label.Accept(this);
                    Append(":");
                }

                Indent();
                foreach (var statement in node.Statements)
                {
                    NewLine();
                    statement.Accept(this);
                }
                Outdent();
            }
        }

        public override void VisitTemplateStringConstant(TemplateStringConstant node)
        {
            Append("\"" + node.Value + "\"");
        }

        public override void VisitThrow(Throw node)
        {
            Append("throw ");
            node.Statement.Accept(this);
        }

        public override void VisitTry(Try node)
        {
            Append("try");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            node.Block.Accept(this);

            Outdent();
            Append("}");
            foreach (var @catch in node.Catches)
            {
                NewLine();
                @catch.Accept(this);
            }

            if (node.Finally != null)
            {
                NewLine();
                node.Finally.Accept(this);
            }
        }

        public override void VisitTypeExpression(TypeExpression node)
        {
            Append(node.TypeName);
        }

        public override void VisitUnknown(Unknown unknown)
        {
            Append(unknown.Message);
        }

        public override void VisitRawCode(RawCode node)
        {
            Append(node.Code);
        }

        public override void VisitReturnStatement(ReturnStatement node)
        {
            Append("return");

            if (node.Expression != null)
            {
                Append(" ");
                node.Expression.Accept(this);
            }

        }

        public override void VisitWhile(While node)
        {
            Append("while (");
            node.Condition.Accept(this);
            Append(")");
            NewLine();
            Append("{");
            Indent();
            NewLine();
            node.Statement.Accept(this);
            Outdent();
            Append("}");
        }

        public override void VisitVariableDeclaration(VariableDeclaration node)
        {
            if (!string.IsNullOrEmpty(node.Type))
            {
                Append(node.Type);
                Append(" ");
            }

            foreach (var variable in node.Variables)
            {
                VisitVariableDeclarator(variable);
            }
        }

        public override void VisitVariableDeclarator(VariableDeclarator node)
        {
            Append(node.Name);
            if (node.Initializer != null)
            {
                Append(" = ");
                node.Initializer.Accept(this);
            }
        }
    }

    public class PowerShellCodeWriter : CStyleCodeWriter
    {
        private bool _inSwitch;

        private static Dictionary<BinaryOperator, string> _operatorMap;
        protected override Dictionary<BinaryOperator, string> OperatorMap => _operatorMap;

        static PowerShellCodeWriter()
        {
            _operatorMap = new Dictionary<BinaryOperator, string>
            {
                { BinaryOperator.Equal, " -eq " },
                { BinaryOperator.NotEqual, " -ne " },
                { BinaryOperator.GreaterThan, " -gt " },
                { BinaryOperator.LessThan, " -lt " },
                { BinaryOperator.LessThanEqualTo, " -le " },
                { BinaryOperator.GreaterThanEqualTo, " -ge " },
                { BinaryOperator.And, " -and " },
                { BinaryOperator.Or, " -or " },
                { BinaryOperator.Bor, " -bor " },
                { BinaryOperator.Minus, " - " },
                { BinaryOperator.Plus, " + " },
                { BinaryOperator.Not, " -not " }
            };
        }

        public override Language Language => Language.PowerShell;

        public override void VisitArrayCreation(ArrayCreation node)
        {
            Append("@(");
            foreach(var item in node.Initializer)
            {
                item.Accept(this);
                Append(", ");
            }

            // Remove last ,
            Builder.Remove(Builder.Length - 2, 2);
            Append(")");
        }

        public override void VisitBreak(Break node)
        {
            if (_inSwitch) return;

            base.VisitBreak(node);
        }

        public override void VisitCast(Cast node)
        {
            var type = node.Type.Replace("<", "[").Replace(">", "]");
            Append("[");
            Append(type);
            Append("]");
            node.Expression.Accept(this);
        }

        public override void VisitCatchDeclaration(CatchDeclaration node)
        {
            var type = node.Type.Replace("<", "[").Replace(">", "]");
            Append("[");
            Append(type);
            Append("]");
        }

        public override void VisitIdentifierName(IdentifierName node)
        {
            if (string.IsNullOrEmpty(node.Name))
                return;
       
            Append("$");

            var firstChar = node.Name.ToCharArray()[0].ToString();
            if (firstChar.ToUpper() == firstChar || firstChar == "_")
            {
                Append("this.");
            }

            Append(node.Name.Replace("@", string.Empty));
        }

        public override void VisitIfStatement(IfStatement node)
        {
            Append("if (");
            node.Condition.Accept(this);
            Append(")");
            NewLine();

            Append("{");
            Indent();

            NewLine();
            node.Body.Accept(this);
            NewLine();

            Outdent();
            Append("}");

            node.ElseClause?.Accept(this);

            NewLine();
        }

		public override void VisitElseClause(ElseClause node)
		{
			NewLine();
			Append("else");

			var isIf = node.Body is IfStatement;
			if (!isIf)
			{
				NewLine();
				Append("{");
				Indent();
				NewLine();
			}

			node.Body.Accept(this);

			if (!isIf)
			{
				Outdent();
				Append("}");
			}
		}

		public override void VisitLiteral(Literal node)
        {
            if (node.Token == "true" || node.Token == "false" || node.Token == "null")
                Append("$");

            Append(node.Token);
        }

        public override void VisitMemberAccess(MemberAccess node)
        {
            if (node == null || node.Expression == null)
            {
                return;
            }

            var typeExpression = node.Expression as TypeExpression;
            if (typeExpression != null)
            {
                var type = typeExpression.TypeName.Replace("<", "[").Replace(">", "]");
                Append("[");
                Append(type);
                Append("]::");
                Append(node.Identifier);
            }
            else
            {
                base.VisitMemberAccess(node);
            }
        }

        public override void VisitMethodDeclaration(MethodDeclaration node)
        {
            Append("function ");
            Append(node.Name);
            NewLine();
            Append("{");
            Indent();
            NewLine();

            if (node.Parameters.Any())
            {
                Append("param(");
                foreach (var parameter in node.Parameters)
                {
                    parameter.Accept(this);
                    Append(", ");
                }
                Builder.Remove(Builder.Length - 2, 2);
                Append(")");
                NewLine();
            }

            if (node.Modifiers.Contains("extern") && node.Attributes.Any(m => m.Name == "DllImport"))
            {
                Append("Add-Type -TypeDefinition '"); Indent(); NewLine();
                Append("using System;"); NewLine();
                Append("using System.Runtime.InteropServices;"); NewLine();
                Append("public static class PInvoke {"); Indent(); NewLine();

                foreach(var line in node.OriginalSource.Split('\r'))
                {
                    var trimmedLine = line.Trim();
                    Append(trimmedLine);
                    NewLine();
                }

                Outdent();

                Append("}"); 
                NewLine(); 
                Outdent();
                
                Append("'");
                NewLine(); 
                Append("[PInvoke]::"); Append(node.Name); Append("(");

                foreach (var parameter in node.Parameters)
                {
                    if (parameter.Modifiers.Any(m => m == "ref" || m == "out"))
                    {
                        Append("[ref] ");
                    }
                    Append("$"); Append(parameter.Name); Append(", ");
                }

                Builder.Remove(Builder.Length - 2, 2);
                Append(")"); NewLine();
            }
            else
            {
                node.Body?.Accept(this);
            }

            Outdent();
            Append("}");
            NewLine();
        }

        public override void VisitObjectCreation(ObjectCreation node)
        {
            var typeName = node.Type;

            Append("(New-Object -TypeName ");
            Append(typeName);

            if (!node.Arguments.Arguments.Any())
            {
                Append(")");
                return;
            };

            Append(" -ArgumentList ");

            VisitArgumentList(node.Arguments);

            Append(")");
        }

        public override void VisitParameter(Parameter node)
        {
            if (node.Modifiers.Any(m => m == "ref" || m == "out"))
            {
                Append("[ref] ");
            }

            if (!string.IsNullOrEmpty(node.Type))
            {
                var type = node.Type.Replace("<", "[").Replace(">", "]");
                Append("[");
                Append(type);
                Append("]");
            }

            Append("$");
            Append(node.Name.Replace("@", string.Empty));
        }

        public override void VisitStringConstant(StringConstant node)
        {
            Append("\'" + node.Value + "\'");
        }

        public override void VisitSwitchStatement(SwitchStatement node)
        {
            _inSwitch = true;
            
            Append("switch (");
            node.Expression.Accept(this);
            Append(")"); 
            NewLine();
            
            Append("{"); 
            Indent(); 
            NewLine();

            foreach(var section in node.Sections)
            {
                section.Accept(this);
            }

            Outdent();
            Append("}");

            _inSwitch = false;
        }

        public override void VisitSwitchSection(SwitchSection node)
        {
            foreach(var label in node.Labels)
            {
                label.Accept(this);
                Append(" {"); Indent(); NewLine();
                foreach(var statement in node.Statements)
                {
                    statement.Accept(this);
                    NewLine();
                }
                Outdent();
                Append("}");
                NewLine();
            }
        }

        public override void VisitUsing(Using node)
        {
            string variableName = null;
            Node initializer = null;
            var variableDeclaration = node.Declaration as VariableDeclaration;
            if (variableDeclaration != null && variableDeclaration.Variables.Any())
            {
                var variableDeclartor = variableDeclaration.Variables.First();
                if (!string.IsNullOrEmpty(variableDeclaration.Type))
                {
                    var type = variableDeclaration.Type.Replace("<", "[").Replace(">", "]");
                    Append("[");
                    Append(type);
                    Append("]");
                }

                variableName = variableDeclartor.Name;
                initializer = variableDeclartor.Initializer;

                Append("$");
                Append(variableDeclartor.Name);
                Append(" = $null");
                NewLine();
            }

            var identifierName = node.Declaration as IdentifierName;
            if (identifierName != null)
            {
                variableName = identifierName.Name;
            }

            Append("try");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            if (initializer != null)
            {
                Append("$");
                Append(variableName);
                Append(" = ");
                initializer.Accept(this);
                NewLine();
            }

            node.Expression.Accept(this);
            Outdent();
            Append("}");
            NewLine();
            Append("finally");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            if (variableName != null)
            {
                Append("$");
                Append(variableName);
                Append(".Dispose()");
                NewLine();
            }

            Outdent();
            Append("}");
        }

        public override void VisitVariableDeclaration(VariableDeclaration node)
        {
            if (!string.IsNullOrEmpty(node.Type))
            {
                var type = node.Type.Replace("<", "[").Replace(">", "]");

                Append("[");
                Append(type);
                Append("]");
            }
            
            foreach(var variable in node.Variables)
            {
                VisitVariableDeclarator(variable);
            }
        }

        public override void VisitVariableDeclarator(VariableDeclarator node)
        {
            Append("$");
            Append(node.Name.Replace("@", string.Empty));
            if (node.Initializer != null)
            {
                Append(" = ");
                node.Initializer.Accept(this);
            }
        }
    }

    public class PowerShell5CodeWriter : PowerShellCodeWriter
    {
        public override Language Language => Language.PowerShell5;

        public override void VisitUsingDirective(UsingDirective node)
        {
            Append($"using namespace {node.Name}");
            NewLine();

        }

        public override void VisitNamespace(Namespace node)
        {

            Append($"# module {node.Name}");
            NewLine();

            foreach (var usin in node.Usings)
            {
                usin.Accept(this);
            }

            foreach (var member in node.Members)
            {
                member.Accept(this);
            }
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclaration node)
        {
            if (node.Modifiers.Any())
            {
                Append($"# Interface Modifiers: ");
                Append(node.Modifiers.Aggregate((x, y) => x + ", " + y));
                NewLine();
            }

            if (node.Attributes.Any())
            {
                foreach (var attribute in node.Attributes)
                {
                    Append($"[{attribute.Name}(");

                    if (attribute.Arguments != null)
                    {
                        int index = 0;
                        foreach (var argument in attribute.Arguments)
                        {
                            argument.Expression.Accept(this);
                            if (index < attribute.Arguments.Count() - 1)
                            {
                                Append(", ");
                            }
                            index++;
                        }
                    }
                    Append($")]");
                    NewLine();
                }
            }

            Append($"class {node.Name}");
            if (node.Bases.Any())
            {
                Append(" : ");
                Append(node.Bases.Aggregate((x, y) => x + ", " + y));
            }
            NewLine();

            Append("{");
            Indent();
            foreach (var member in node.Members)
            {
                NewLine();
                member.Accept(this);
            }
            NewLine();
            Outdent();
            Append("}");

            NewLine();
            NewLine();
        }

        public override void VisitClassDeclaration(ClassDeclaration node)
        {
            if (node.Modifiers.Any())
            {
                Append($"# Class Modifiers: ");
                Append(node.Modifiers.Aggregate((x, y) => x + ", " + y));
                NewLine();
            }

            if (!node.Modifiers.Contains("public"))
            {
                // add an attribute
            }

            if (node.Attributes.Any())
            {
                foreach (var attribute in node.Attributes)
                {
                    Append($"[{attribute.Name}(");

                    if (attribute.Arguments != null)
                    {
                        int index = 0;
                        foreach (var argument in attribute.Arguments)
                        {
                            argument.Expression.Accept(this);
                            if (index < attribute.Arguments.Count() - 1)
                            {
                                Append(", ");
                            }
                            index++;
                        }
                    }
                    Append($")]");
                    NewLine();
                }
            }

            Append($"class {node.Name}");
            if (node.Bases.Any())
            {
                Append(" : ");
                Append(node.Bases.Aggregate((x, y) => x + ", " + y));
            }
            NewLine();

            Append("{");
            Indent();
            foreach (var member in node.Members)
            {
                NewLine();
                member.Accept(this);
            }
            NewLine();
            Outdent();
            Append("}");

            NewLine();
            NewLine();
        }

        public override void VisitMethodDeclaration(MethodDeclaration node)
        {
            if (node.Modifiers.Any())
            {
                var modifiers = string.Join(", ", node.Modifiers);
                if (modifiers != "public" && modifiers != "public, static")
                {
                    Append($"# Modifiers: ");
                    Append(modifiers);
                    NewLine();
                }
            }

            var hidden = string.Empty;

            if (!node.Modifiers.Contains("public"))
            {
                hidden = "hidden ";
            }

            var staticMod = string.Empty;
            if (node.Modifiers.Contains("static"))
            {
                staticMod = "static ";
            }

            var returnType = string.Empty;
            if (node.ReturnType != "void")
            {
                returnType = $"[{node.ReturnType}] ";
            }
            returnType = returnType.Replace("<", "[").Replace(">", "]");

            Append($"{hidden}{staticMod}{returnType}{node.Name}(");

            int index = 0;
            foreach (var parameter in node.Parameters)
            {
                parameter.Accept(this);
                if (index < node.Parameters.Count() - 1)
                {
                    Append(", ");
                }
            }
            Append(")");
            NewLine();

            Append("{");
            Indent();
            NewLine();

            if (node.Body == null)
            {
                Append($"throw [NotImplementedException]\"The Method {node.Name} need an Implementation.\"");
                NewLine();
            }
            else
            {
                node.Body.Accept(this);
            }

            Outdent();
            Append("}");
            NewLine();

        }

        public override void VisitPropertyDeclaration(PropertyDeclaration node)
        {
            if (node.Modifiers.Any())
            {
                var modifiers = string.Join(", ", node.Modifiers);
                if (modifiers != "public")
                {
                    Append($"# Property Modifiers: ");
                    Append(modifiers);
                    NewLine();
                }
            }

            var hidden = string.Empty;
            // if (node.Modifiers.Contains("private"))
            if (!node.Modifiers.Contains("public"))
            {
                hidden = "hidden ";
            }

            var staticMod = string.Empty;
            if (node.Modifiers.Contains("static"))
            {
                staticMod = "static ";
            }

            var type = node.Type.Replace("<", "[").Replace(">", "]");
            Append($"{hidden}{staticMod}[{type}] ${node.Name}");
            NewLine();
        }

        public override void VisitFieldDeclaration(FieldDeclaration node)
        {
            Append($"# Field: ");
            if (node.Modifiers.Any())
            {
                Append(string.Join(", ", node.Modifiers));
            }
            NewLine();

            var hidden = string.Empty;
            // if (node.Modifiers.Contains("private"))
            if (!node.Modifiers.Contains("public"))
                if (!node.Modifiers.Contains("public"))
                {
                    hidden = "hidden ";
                }

            var staticMod = string.Empty;
            if (node.Modifiers.Contains("static"))
            {
                staticMod = "static ";
            }

            var type = node.Type.Replace("<", "[").Replace(">", "]");
            Append($"{hidden}{staticMod}[{type}] ${node.Name}");
            NewLine();
        }

        public override void VisitThisExpression(ThisExpression node)
        {
            Append("$this");
        }

        public override void VisitAttributeArgument(AttributeArgument node)
        {
            var text = node.Expression.ToString();
            Append(text);
        }

        public override void VisitParameter(Parameter node)
        {
            if (node.Modifiers.Any(m => m == "ref" || m == "out"))
            {
                Append("[ref] ");
            }

            if (!string.IsNullOrEmpty(node.Type))
            {
                var type = node.Type.Replace("<", "[").Replace(">", "]");
                Append("[");
                Append(type);
                Append("] ");
            }

            Append("$");

            var name = node.Name.Replace("@", string.Empty);
            Append(name);
        }

        public override void VisitConstructor(Constructor node)
        {
            Append("# Constructor");
            NewLine();

            Append($"{node.Identifier}(");
            int index = 0;
            foreach (var parameter in node.ArgumentList.Arguments)
            {
                parameter.Accept(this);
                if (index < node.ArgumentList.Arguments.Count() - 1)
                {
                    Append(", ");
                }
                index++;
            }
            Append(")");
            NewLine();

            Append("{");
            Indent();
            NewLine();
            if (node.Body == null)
            {
                Append($"throw [NotImplementedException]\"The Constructor need an Implementation.\"");
                NewLine();
            }
            else
            {
                node.Body.Accept(this);
            }
            Outdent();
            Append("}");
            NewLine();
        }

        public override void VisitObjectCreation(ObjectCreation node)
        {
            var typeName = node.Type.Replace("<", "[").Replace(">", "]");

            Append("[");
            Append(typeName);
            Append("]::new(");
            if (node.Arguments != null && node.Arguments.Arguments.Any())
            {
                VisitArgumentList(node.Arguments);
            };
            Append(")");
        }

    }

}
