using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace error_reporting_csharp_dotnet_tool
{

    partial class Program
    {
        class ErrorCodeCrawler : CSharpSyntaxWalker
        {
            //use base ctor to specify depth if necessary
            public ErrorCodeCrawler(SemanticModel semanticModel)
            {
                SemanticModel = semanticModel;

                if (ErrorCodeCollection==null)
                {
                    ErrorCodeCollection = new ErrorCodeCollection();
                }
            }
            public ErrorCodeCrawler(SemanticModel semanticModel, ErrorCodeCollection errorCodeCollection) : this(semanticModel)
            {
                ErrorCodeCollection = errorCodeCollection;
            }

            public SemanticModel SemanticModel {get;set;}
            public ErrorCodeCollection ErrorCodeCollection;

            static bool IsExasolErrorCodeRelated(SemanticModel semanticModel,CSharpSyntaxNode node)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(node);
                var symbolContainingTypeStr = symbolInfo.Symbol.ContainingType.ToString();
                if (symbolContainingTypeStr == "Exasol.ErrorReporting.ExaError" || symbolContainingTypeStr == "Exasol.ErrorReporting.ErrorMessageBuilder")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private ErrorCodeEntry currentEntry;
            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                base.VisitInvocationExpression(node);

                var nodeExpression = node.Expression.ToString();
                if (nodeExpression.EndsWith("MessageBuilder") || nodeExpression.EndsWith("Message") ||
                    nodeExpression.EndsWith("TicketMitigation") || nodeExpression.EndsWith("Mitigation"))
                {
                    //if the method's called similar but isn't related do nothing further -> this semantic check is 'expensive' hence we do the syntax check first.
                    if (!IsExasolErrorCodeRelated(SemanticModel, node))
                    {
                        return;
                    }
                    if (node.Expression.ToString().EndsWith("MessageBuilder"))
                    {
                        var argList = node.ArgumentList;
                        Console.WriteLine($@"found a construction helper function: {argList.Arguments[0]}");
                        var argument = argList.Arguments[0].ToString();
                        currentEntry = ErrorCodeCollection.AddEntry(argument);
                    }
                    else if (nodeExpression.EndsWith("Message"))
                    {
                        var argList = node.ArgumentList;
                        Console.WriteLine($@"found a message function: {argList.Arguments[0]}");
                        var argument = argList.Arguments[0].ToString();
                        currentEntry.Messages.Add(argument);
                    }
                    else if (nodeExpression.EndsWith("TicketMitigation"))
                    {
                        var argList = node.ArgumentList;
                        Console.WriteLine($@"found a ticket mitigation function");
                        var argument = argList.Arguments[0].ToString(); //TODO: eventually get this from the errorhandling package so it updates alongside it.
                        currentEntry.Mitigations.Add("Ticket mitigation");
                    }
                    else if (nodeExpression.EndsWith("Mitigation"))
                    {
                        var argList = node.ArgumentList;
                        Console.WriteLine($@"found a mitigation function: {argList.Arguments[0]}");
                        var argument = argList.Arguments[0].ToString();
                        currentEntry.Mitigations.Add(argument);
                    }
                }

            }

        }
    }
}

