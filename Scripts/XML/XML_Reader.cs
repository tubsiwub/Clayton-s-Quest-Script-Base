using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.IO;

public class XML_Reader {

	#region SINGLETON
	private static XML_Reader instance;
	private XML_Reader(){
		Initialize ();
	}
	public static XML_Reader getInstance(){
		if (instance == null)
			instance = new XML_Reader ();
		return instance;
	}
	#endregion

	Dictionary<string, string[]> dialogueSet;

	// Find dialogue text by NPC name
	public string[] GetDialogueText(string name){

		if(dialogueSet.ContainsKey(name))
			return dialogueSet [name];

		string[] newString = new string[1];
		newString[0] = "ERROR: NO DIALOGUE FOUND";
		return newString;
	}

	// Runs this when created, once.
	void Initialize (){

		TextAsset xmlAsset = Resources.Load("JamrootDialogue", typeof(TextAsset)) as TextAsset;

		MemoryStream memStream = new MemoryStream (xmlAsset.bytes);

		XmlTextReader CReader = new XmlTextReader(memStream);

		// Displays confirmation --
//		if (xmlAsset != null) {
//			GameObject.Find ("XMLPATH").GetComponent<Text> ().text = "[XML_READER.cs]   XML File loaded correctly:  ";
//			for(int i = 0; i < 40; i++)
//			GameObject.Find ("XMLPATH").GetComponent<Text> ().text += xmlAsset.ToString ()[i];
//		}
		// -- XML format isn't useful to know.

		dialogueSet = new Dictionary<string,string[]> ();

		while (CReader.Read ()) {

			// While data is relevant...
			if (CReader.Name != "" &&
				CReader.NodeType != XmlNodeType.EndElement &&
				CReader.Name != "Dialogue" &&
				CReader.Name != "xml") {

				string[] storedStringArray = new string[10];

				// Cycle through 10 reads and store the ones that are valid in the string array
				//	that gets passed into our dialogueSet dictionary.
				for (int i = 0; i < 10; i++) {
					if (CReader.GetAttribute ("Text" + (i+1).ToString()) != "@") {
						storedStringArray [i] = CReader.GetAttribute ("Text" + (i+1).ToString());
					}
				}

				dialogueSet.Add (CReader.Name, storedStringArray);
			}
		}

		CReader.Close ();
	}
}
