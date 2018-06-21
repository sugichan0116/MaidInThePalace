using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineScriptFactory {
	public static LineScript interpret(string rowText) {
		string mainText;
		int indentLevel = 0;

		mainText = rowText;
		while(mainText.StartsWith("\t")) {
			indentLevel++;
			mainText = mainText.Remove (0, 1);
		}
		while(mainText.StartsWith("  ")) {
			indentLevel++;
			mainText = mainText.Remove (0, 2);
		}
		mainText.Trim ();

		if (Label.isMatch(mainText)) {
			Debug.Log("la " + mainText);
			return new Label (rowText, "", indentLevel, mainText);
		} else if (Speaker.isMatch(mainText)) {
			Debug.Log("spe " + mainText);
			return new Speaker (rowText, "", indentLevel, mainText);
		} else if (Material.isMatch(mainText)) {
			Debug.Log("mat " + mainText);
			return new Material (rowText, "", indentLevel, mainText);
		} else if (Assignment.isMatch(mainText)) {
			Debug.Log("ass " + mainText);
			return new Assignment (rowText, "", indentLevel, mainText);
		} else if (Order.isMatch(mainText)) {
			Debug.Log("ode " + mainText);
			return new Order (rowText, "", indentLevel, mainText);
		} else if (Selection.isMatch(mainText)) {
			Debug.Log("sel " + mainText);
			return new Selection (rowText, "", indentLevel, mainText);
		}

		return new LineScript (rowText, mainText, indentLevel, "");
	}
}

public class LineScript {

	protected string rowText, 
				mainText;
	protected int indentLevel;
	protected bool isOrder;
	protected bool isIndentBlock;

	public LineScript(string text, string mainText, int indent, string option) {
		this.rowText = text;
		this.mainText = mainText;
		this.indentLevel = indent;
		isOrder = false;
		isIndentBlock = false;
		setOption (option);
	}

	protected virtual void setOption (string option) {

	}

	public string text {
		get {
			return mainText;
		}
	}

	public int indent {
		get {
			return indentLevel;
		}
	}

	public bool isIndent {
		get {
			return isIndentBlock;
		}
	}

	public bool isText() {
		return isOrder == false;
	}
}

public class Label : LineScript {
	public static string MARK = "@";
	public string name {private set; get;}
	private string condition;
	
	public Label(string text, string mainText, int indent, string option) 
		: base(text, mainText, indent, option){
		isOrder = true;
		isIndentBlock = true;
	}

	protected override void setOption (string option) {
		name = condition = "";
		string[] arg = option.Split(' ');

		for (int i = 0, n = arg.Length; i < n; i++) {
			if (Label.isMatch (arg [i]) && name == "") {
				name = arg [i].Replace (Label.MARK, "");
				arg [i] = "";
			}
		}

		condition = string.Join (" ", arg).Trim();
		Debug.Log("[Label] " + name + "/" + condition + "/" + option);
	}

	public bool isExecutable(Dictionary<string, string> variable) {
		if(condition == null || condition == "") return true;
		
		return Comparison.judge(variable, condition);
	}

	public static bool isMatch(string text) {
		return text.StartsWith (Label.MARK);
	}
}

public class Speaker : LineScript {
	public static string MARK = "#";
	public string name {private set; get;}
	
	public Speaker(string text, string mainText, int indent, string option) :
		base(text, mainText, indent, option){
		isOrder = true;
		isIndentBlock = true;
	}

	protected override void setOption (string option) {
		name = option.Replace(Speaker.MARK, "").Trim();
	}

	public static bool isMatch(string text) {
		return text.StartsWith (Speaker.MARK);
	}

}

public class Material : LineScript {
	public static string BEGIN = "[", END = "]";
	public string attributeText {private set; get;}
	
	public Material(string text, string mainText, int indent, string option) :
		base(text, mainText, indent, option){
		isOrder = true;
	}

	protected override void setOption (string option) {
		attributeText = option
			.Replace(Material.BEGIN, "")
			.Replace(Material.END, "")
			.Trim()
			;
	}

	public static bool isMatch(string text) {
		return text.StartsWith (Material.BEGIN) && text.Contains(Material.END);
	}
}

public class Selection : LineScript {
	public static string MARK = ":";
	public int value {private set; get;}
	private string selectText;
	private string condition;
	
	public Selection(string text, string mainText, int indent, string option) 
		: base(text, mainText, indent, option){
		isOrder = true;
	}

	protected override void setOption (string option) {
		string[] arg = option
			.Replace(" ", "")
			.Split(Selection.MARK.ToCharArray());
			;
		
		//Debug.Log(arg[0]);
		value = int.Parse(arg[0]);

		if(arg.Length == 2) {
			selectText = arg[1];
		} else if(arg.Length == 3) {
			condition = arg[1];
			selectText = arg[2];
		}
	}

	public static bool isMatch(string text) {
		return text.Contains (Selection.MARK);
	}
}

public class Order : LineScript {
	public static string MARK = "*";
	public string name {private set; get;}
	private string argument; 
	
	public Order(string text, string mainText, int indent, string option) 
		: base(text, mainText, indent, option){
		isOrder = true;
	}

	protected override void setOption (string option) {
		if(option.StartsWith(Order.MARK)) {
			option = option.Remove(0, 1).Trim();
		}
		string[] arg = option.Split(' ');
		
		try {
			name = arg[0];
			arg[0] = "";
			argument = string.Join(" ", arg).Trim();
		} catch {
			Debug.Log("* error : failed to interpret Order => " + option);
		}
	}

	public static bool isMatch(string text) {
		return text.StartsWith (Order.MARK);
	}
}

public sealed class Node<TValue>
{
    private TValue value;
    private Node<TValue> left;
    private Node<TValue> right;
    public Node(TValue value)
    {
        this.value = value;
        this.left = null;
        this.right = null;
    }
    public TValue Value
    {
        get { return value; }
        set { this.value = value; }
    }
    public Node<TValue> Left
    {
        get { return left; }
        set { left = value; }
    }
    public Node<TValue> Right
    {
        get { return right; }
        set { right = value; }
    }
}

public class Calculation {
	private static int DEFAULT_PRIORITY = 8;
	private static string BRACKET = "()";
	private static Dictionary<string, int> OPERATORS = new Dictionary<string, int>()
		{
			{"*", 7}, {"/", 7}, {"%", 7}, 
			{"+", 6}, {"-", 6},
			{"<", 5}, {"<=", 5}, {">", 5}, {">=", 5}, 
			{"==", 4}, {"!=", 4},
			{"&&", 3},
			{"||", 2},
			{"=", 1}, {"+=", 1}, {"-=", 1}, {"*=", 1}, {"/=", 1}, {"%=", 1}
		};

	private static bool isLeaf(Node<string> node) {
		return node != null && node.Left == null && node.Right == null;
	}

	private static bool isTerminalNode(Node<string> node) {
		return node != null &&
			isLeaf(node) == false &&
			isLeaf(node.Left) && 
			isLeaf(node.Right);
	}

	private static int operatorPriority(string text) {
		foreach(string key in Calculation.OPERATORS.Keys) {
			if(text == key) return Calculation.OPERATORS[key];
		}
		return Calculation.DEFAULT_PRIORITY;
	}

	private static string operatorGroup(string text) {
		int priority = operatorPriority(text);
		if(priority == 7 || priority == 6) return "calculate";
		if(priority == 5 || priority == 4) return "compare";
		if(priority == 3 || priority == 2) return "bitwise";
		if(priority == 1) return "assign";

		return "NaO";
	}

	private static string typeOf(string text) {
		int i;
		if(int.TryParse(text, out i)) {
			return "int";
		}
		
		float f;
		if(float.TryParse(text, out f)) {
			return "float";
		}
		
		if(string.Compare(text, "true", true) == 0 || string.Compare(text, "false", true) == 0) {
			return "bool";
		}

		if(text == "infinity" || text == "-infinity") {
			return "infinity";
		}

		if(operatorPriority(text) != Calculation.DEFAULT_PRIORITY) {
			return "operator";
		}

		if(text.Contains(Assignment.VARIABLE)) {
			bool isOperator = false;
			foreach(string key in Calculation.OPERATORS.Keys) {
				if(text.Contains(key)) {
					isOperator = true;
				}
			}
			if(isOperator == false) {
				return "variable";
			}
		}
		
		return "NaN";
	}

	private static bool isNumber(string text) {
		string resultType = typeOf(text);
		return resultType == "int" || resultType == "float";
	}

	private static bool canBuild(Node<string> node) {
		return typeOf(node.Value) == "NaN";
	}

	private static void build(Node<string> node) {
		if(canBuild(node)) {
			string left = "", right = "";
			string splitOperator = "";
			string origin = node.Value;

			int minIndex = origin.Length, nest = 0;
			int priority = Calculation.DEFAULT_PRIORITY;
			string process = origin;
			while(process.Length > 0) {

				if(process[0] == Calculation.BRACKET[1]) nest--;
				if(nest == 0) {
					string longerKey = "";
					int longerPriority = Calculation.DEFAULT_PRIORITY;
					foreach(string key in Calculation.OPERATORS.Keys) {
						if(process.StartsWith(key)) {
							//Debug.Log("[@] :" + longerKey + "[" + (origin.Length - process.Length) + "]" + key);
							if(longerKey.Length < key.Length) {
								longerKey = key;
								longerPriority = Calculation.OPERATORS[key];
							}
						}
					}
					if(priority > longerPriority) {
						priority = longerPriority;
						splitOperator = longerKey;
						minIndex = origin.Length - process.Length;
						Debug.Log("[search] : " + minIndex + "; " + splitOperator + "; " + priority + "; " + process);
					}
					if(longerPriority != Calculation.DEFAULT_PRIORITY) {
						//Debug.Log("[key] : " + longerKey);
						process = process.Remove(0, longerKey.Length - 1);
					}
				}
				if(process[0] == Calculation.BRACKET[0]) nest++;
				
				if(process.Length != 0) process = process.Remove(0, 1);
			}

			Debug.Log("[*] " + origin);
			try {
				left = origin.Substring(0, minIndex - 1);
				left = left.Trim();
				right = origin.Substring(splitOperator.Length + minIndex, origin.Length - minIndex - splitOperator.Length);
				right = right.Trim();
				Debug.Log("left & right :" + left + "/" + right);

				string assignOperator = "=";
				if(operatorPriority(splitOperator) == operatorPriority(assignOperator) &&
					splitOperator.Length > assignOperator.Length) {
					string newOperator = splitOperator.Replace(assignOperator, "");
					splitOperator = assignOperator;
					right = left + " " + newOperator + " (" + right + ")"; //頑張って直す
					Debug.Log("*extended Assignment Operator : " + left + "/" + splitOperator + "/" + right);
				}

				uncoverBracket(ref left);
				uncoverBracket(ref right);
				
				left = left.Trim();
				right = right.Trim();
				Debug.Log("left & right :" + left + "/" + right);
			} catch {
				Debug.Log("ERROR : invalid data, can not split into node. => " + origin);
				node.Value = "NaN";
				return;
			}
			node.Value = splitOperator;
			node.Left = new Node<string>(left);
			node.Right = new Node<string>(right);
		}
	}

	private static void uncoverBracket(ref string s) {
		s = s.Trim();
		if(s.Contains(Calculation.BRACKET[0].ToString()) == false) {
			return;
		}

		int i = 0, nest = 0, outChar = 0;
		while(s.Length > i) {
			if(s[i] == Calculation.BRACKET[1]) nest--;
			if(nest == 0) {
				outChar++;
			}
			if(s[i] == Calculation.BRACKET[0]) nest++;
			i++;
		}

		if(outChar == 2) {
			s = s.Remove(0, 1);
			s = s.Remove(s.Length - 1, 1);
			uncoverBracket(ref s);
		}
	}

	private static int countChar(string s, string c) {
		return (s.Length - s.Replace(c, "").Length) / c.Length;
	}

	private static void analyse(Node<string> node) {
		build(node); //参照的にイケてるのか？　いけてるかも？

		if(node.Left != null) analyse(node.Left);
		if(node.Right != null) analyse(node.Right);
	}

	private static void calculate(Dictionary<string, string> variable, Node<string> node) {
		if(isTerminalNode(node) == false) return;

		string operatorText = node.Value,
				left = node.Left.Value,
				right = node.Right.Value,
				leftType = typeOf(left),
				rightType = typeOf(right);

		string operatorType = operatorGroup(node.Value);

		if(leftType == "variable") {
			left = left.Replace(Assignment.VARIABLE, "");
			Debug.Log("left: " + left + "," + leftType);
			if(variable.ContainsKey(left)) {
				left = variable[left]; //例外と＄
				leftType = typeOf(left);
				Debug.Log("Value: " + left + "," + leftType);
			}
		} //infinity 例外 nan例外

		if(rightType == "variable") {
			right = right.Replace(Assignment.VARIABLE, "");
			Debug.Log("right: " + right + "," + rightType);
			if(variable.ContainsKey(right)) {
				right = variable[right]; //例外と＄
				rightType = typeOf(right);
				Debug.Log("Value: " + right + "," + rightType);
			}
		}

		Debug.Log("operator: " + operatorText + "," + operatorType);
		if(operatorType == "calculate") {
			var leftValue = (leftType == "int") ? int.Parse(left) : float.Parse(left);
			var rightValue = (rightType == "int") ? int.Parse(right) : float.Parse(right);
			
			if(operatorText == "+") {
				node.Value = "" + (leftValue + rightValue);
			} else if(operatorText == "-") {
				node.Value = "" + (leftValue - rightValue);
			} else if(operatorText == "*") {
				node.Value = "" + (leftValue * rightValue);
			} else if(operatorText == "/") {
				node.Value = "" + (leftValue / rightValue);
			} else if(operatorText == "%") {
				node.Value = "" + (leftValue % rightValue);
			}
		} else if(operatorType == "compare") {
			var leftValue = (leftType == "int") ? int.Parse(left) : float.Parse(left);
			var rightValue = (rightType == "int") ? int.Parse(right) : float.Parse(right);

			if(operatorText == "==") {
				node.Value = "" + (leftValue == rightValue);
			} else if(operatorText == "!=") {
				node.Value = "" + (leftValue != rightValue);
			} else if(operatorText == "<") {
				node.Value = "" + (leftValue < rightValue);
			} else if(operatorText == "<=") {
				node.Value = "" + (leftValue <= rightValue);
			} else if(operatorText == ">") {
				node.Value = "" + (leftValue > rightValue);
			} else if(operatorText == ">=") {
				node.Value = "" + (leftValue >= rightValue);
			}
		} else if(operatorType == "bitwise") {
			var leftValue = (leftType == "bool") ? bool.Parse(left) : false;
			var rightValue = (rightType == "bool") ? bool.Parse(right) : false;

			if(operatorText == "&&") {
				node.Value = "" + (leftValue && rightValue);
			} else if(operatorText == "||") {
				node.Value = "" + (leftValue || rightValue);
			}
		} else if(operatorType == "assign") {
			node.Value = node.Left.Value + operatorText + right;
		} else {
			node.Value = "NaN";
		}

		node.Left = node.Right = null;
	}

	private static void traverse(Dictionary<string, string> variable, Node<string> node) {
		if(node.Left != null) traverse(variable, node.Left);
		if(node.Right != null) traverse(variable, node.Right);

		calculate(variable, node);
	}

	public static string execute(Dictionary<string, string> variable, string text) {
		var root = new Node<string>(text);

		analyse(root);
		traverse(variable, root);

		return root.Value;
	}
}

public class Comparison {
	public static string EQUAL = "==", NOT_EQUAL = "!=", GREATER = ">", LESSER = "<";

	public static bool judge(Dictionary<string, string> variable, string text) {
		string result = Calculation.execute(variable, text);
		Debug.Log("Coparison executed, result in " + result);
		return string.Compare(result, "true", true) == 0;
	}

	public static bool isMatch(string text) {
		return text.Contains (Comparison.EQUAL) ||
			   text.Contains (Comparison.NOT_EQUAL) ||
			   text.Contains (Comparison.GREATER) ||
			   text.Contains (Comparison.LESSER);
	}
}

public class Assignment : LineScript {
	public static string MARK = "=", VARIABLE = "$";
	private string plainText;
	public string name {private set; get;}
	public string value {private set; get;}
	
	public Assignment(string text, string mainText, int indent, string option)
		: base(text, mainText, indent, option){
		isOrder = true;
	}

	protected override void setOption (string option) {
		plainText = option;
	}

	public void execute(Dictionary<string, string> variable) {
		Debug.Log("//rowText = " + plainText + "," + plainText.StartsWith(Order.MARK));
		if(plainText.StartsWith(Order.MARK)) {
			plainText = plainText.Remove(0, 1).Trim();
		}
		//Debug.Log("//rowText = " + plainText + "," + plainText.StartsWith(Order.MARK));
		string[] arg = 
			Calculation.execute(
				variable,
				plainText
				.Trim()
			)
			.Split(Assignment.MARK.ToCharArray())
		;

		name = arg[0].Replace(Assignment.VARIABLE, "");
		value = arg[1];
	}

	public static bool isMatch(string text) {
		return text.StartsWith (Order.MARK) && 
			text.Contains(Assignment.MARK) && 
			text.Contains(Assignment.VARIABLE) &&
			Comparison.isMatch(text) == false;
	}
}