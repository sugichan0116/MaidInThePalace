using UnityEngine;
using System.Collections;
using UnityEngine.UI;	// uGUIの機能を使うお約束
using System.Linq;
using System.Collections.Generic;

public class TextController : MonoBehaviour {

	private List<LineScript> linescripts;
	private List<LineScript> indentHeads;
	public Dictionary<string, string> variable;
	public Text uiText;	// uiTextへの参照を保つ
	public Text speaker;

	int currentLine = 0; // 現在の行番号

	void Start()
	{
		linescripts = new List<LineScript> ();
		indentHeads = new List<LineScript> ();
		variable = new Dictionary<string, string>();

		TextAsset t = new TextAsset ();
		t = Resources.Load ("test", typeof(TextAsset)) as TextAsset;

		string rowText = t.text;
		Debug.Log (rowText);
		//\r\n \n
		string[] lineScripts = rowText.Split ('\n');

		foreach (string line in lineScripts) {
			linescripts.Add(LineScriptFactory.interpret(line));
		}

		TextUpdate();
	}

	void Update () 
	{
		// 現在の行番号がラストまで行ってない状態でクリックすると、テキストを更新する
		if(currentLine < linescripts.Count && Input.GetMouseButtonUp(0))
		{
			TextUpdate();
		}
	}

	// テキストを更新する
	void TextUpdate()
	{
		bool isReadText = false;
		// 現在の行のテキストをuiTextに流し込み、現在の行番号を一つ追加する
		while(isReadText == false) {
			LineScript line = linescripts[currentLine];
			isReadText = isReadText || line.isText();
			
			uiText.text = line.text; //debug
			//Debug.Log (line.indent);

			//indent down & up
			// if(line.indent < indentHeads.Count) {
			// 	indentHeads.RemoveAt(indentHeads.Count - 1);
			// }
			while(line.indent < indentHeads.Count) {
				indentHeads.RemoveAt(indentHeads.Count - 1);
			}
			if(line.isIndent) {
				indentHeads.Add(line);
			}
			
			if(line is Assignment) {
				Debug.Log("Execute Assignment from now");
				Assignment a = line as Assignment;
				a.execute(variable);
				if(variable.ContainsKey(a.name) == false) {
					variable.Add(a.name, "0");
				}
				variable[a.name] = a.value;
				Debug.Log("*[$]completed calculation :: " + a.name + " : " + variable[a.name]);
			}

			if(line is Label) {
				Label label = line as Label;
				if(label.isExecutable(variable)) {
					Debug.Log("label[" + label.name + "] was executed");
				} else {
					Debug.Log("label[" + label.name + "] was failed...");
					while(line.indent < linescripts[currentLine + 1].indent) {
						//out of range array を考えてね
						currentLine++;
					}
				}
			}

			if(line is Order) {
				Order order = line as Order;
				Debug.Log("Order [" + order.name + "]");
				if(order.name == "go") {
					
				} else if(order.name == "select"){

				}
			}

			//speaker update
			string name = "";
			for(int i = 0, n = indentHeads.Count; i < n; i++) {
				if(indentHeads[i] is Speaker) {
					name = (indentHeads[i] as Speaker).name;
				}
			}
			speaker.text = name;
			//speaker.text += line.indent + "/" + indentHeads.Count;

			currentLine ++;
		}

	}
}