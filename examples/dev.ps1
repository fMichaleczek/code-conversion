<#
class in class
enum + flags
global scope
annotations/docs
attribute
using namespace

#>

$code = @'
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Test
{
    /// <summary>
    /// Abstract node in a syntax tree.
    /// </summary>
    [Cmdlet(VerbsCommunications.Send, "Greeting", SupportPaging = true)]
    [Cmdlet("Greeting")]
    [TCmdlet()]
    [TCmdlet()]
    public abstract class Node
    {
        [Parameter(Position = 0, Mandatory = true)]
		public Node Parent { get; set; }
        
        /// <summary>
        /// Binary expression. 
        /// </summary>
        public static CodeWriter DefaultCodeWriter { get; set; }
        public CodeWriter CodeWriter { get; set; }
        public Language SourceLanguage { get; }
        

        [Alias("UN","Writer","Editor")]
        public string OriginalSource { get; set; }
		public Intent Intent { get; set; }
        public abstract void Accept(NodeVisitor visitor);


		protected void SetParent(Node node)
		{
			if (node != null)
				node.Parent = this;
		}
    }
}
'@

$ErrorView = 'DetailedView'

Invoke-CSharpConversion $code -As Type