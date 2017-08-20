// Tristan

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Purpose:	Animates dialogue pop-ups in a variety of ways, then reads in and displays dialogue through
//			text-reader animation.

// Placement:	Place script on a docked canvas text-box object that is within a filled object 
//				that should be parented to a panel.  Canvas -> Panel -> Background -> [Text Object]

public class Dialogue_Popup : MonoBehaviour { 

	// Events
	public delegate void TextboxTurnOff_Action();
	public static event TextboxTurnOff_Action OnTextboxTurnOff;

	List<GameObject> nearbyNPCs;

	GameObject 
		dialogueText,
		currentDialogueObj,
		totalDialogueObj,
		nameTag;

	RectTransform nameTagRT;

	Rect tagRect;

	public float scaleSpeed = 240;

	Vector2 scale, tagScale;

	IEnumerator coroutine;

	bool dialogueFinished = true;
	bool resetLock = false;	// makes sure that the reset function is called once.
	bool dialogueBoxOpen = false;

	// When talking to an NPC, prevent interruption from other NPCs.  
	// We cannot hit 'Interact' and start OTHER dialogue while in dialogue until it ends.
	bool dialogueLock = false;	
	GameObject dialogueTarget = null;	// Only this NPC can be used to continue locked dialogue

	int 
		currentDialogue,
		totalDialogue;

	GameObject playerObj;


	void Start () {

		nearbyNPCs = new List<GameObject> ();
		playerObj = GameObject.FindWithTag ("Player");

		nameTag = GameObject.Find ("nameTag");
		dialogueText = GameObject.Find ("DialogueText");
		currentDialogueObj = GameObject.Find ("currentDialogue");
		totalDialogueObj = GameObject.Find ("totalDialogue");

		TurnOffTextbox ();

	}

	void Update () {

		// update the dialogue displays
		currentDialogueObj.GetComponent<Text>().text = currentDialogue.ToString();
		totalDialogueObj.GetComponent<Text>().text = totalDialogue.ToString();

		if (IsNPCListEmpty ()) {
			TurnOffTextbox ();
			ResetDialogue ();
		} else {
			resetLock = false;
		}

		// Instant-fill dialogue box
		if(Input.GetButtonDown ("Interact") && readStarted){

			readStarted = false;

		}

	}

	public IEnumerator GetCoroutine{ get { return coroutine; } }
	public bool DialogueFinished{ get { return dialogueFinished; } }

	public void TurnOffTextbox(){

		// Stop typing
		if(coroutine != null)
			StopCoroutine (GetCoroutine);

		StopAllCoroutines ();

		// Make sure you can see dialogue
		dialogueFinished = true;

		nameTag.gameObject.SetActive (false);
		dialogueText.SetActive (false);
		currentDialogueObj.SetActive (false);
		totalDialogueObj.SetActive (false);

		// Make sure the font is all normal-like
		dialogueText.GetComponent<Text> ().fontStyle = FontStyle.Normal;

		// Make sure event isn't null, then cast.
		if(OnTextboxTurnOff != null)
			OnTextboxTurnOff ();
	}

	public GameObject GetFirstNPC(){
		
		return nearbyNPCs [0];

	}

	// Sort through all NPCs in list; return shortest
	public GameObject GetNearestNPC(){

		GameObject selectedNPC = null;
		float shortest = 999;

		foreach(GameObject npc in nearbyNPCs){
			float dist = Vector3.Distance (npc.transform.position, playerObj.transform.position);
			if(dist < shortest){
				shortest = dist;
				selectedNPC = npc;
			}
		}

		return selectedNPC;
	}

	// Are there any NPCs near player?
	public bool IsNPCListEmpty(){

		if (nearbyNPCs.Count <= 0)
			return true;
		else
			return false;

	}

	public int GetNPCListCount(){

		return nearbyNPCs.Count;

	}

	public void AddNPC(GameObject NPC){

		if(!nearbyNPCs.Contains(NPC))
			nearbyNPCs.Add (NPC);

	}

	public void RemoveNPC(GameObject NPC){

		if (nearbyNPCs.Contains (NPC))
			nearbyNPCs.Remove (NPC);
	}

	public void LockDialogue(bool value){
		dialogueLock = value;
		if (!value) {
			dialogueTarget = null;
		}
	}
	public bool GetLockStatus(){
		return dialogueLock;
	}
	public void SetLockTarget(GameObject NPC){
		dialogueTarget = NPC;
	}
	public GameObject GetLockTarget(){
		return dialogueTarget;
	}

	public void ResetDialogue(){

		if (!resetLock) {
			
			resetLock = true;

			dialogueBoxOpen = false;

			LockDialogue (false);	// unlock all dialogue

			GameObject.Find ("DialogueCanvas/DialoguePanel/DialogueBG").GetComponent<Animator> ().SetTrigger ("Disappear");

			if (Camera.main.GetComponent<CameraControlDeluxe> ().GetLookTarget () != Vector3.zero)
				Camera.main.GetComponent<CameraControlDeluxe> ().CancelLookTarget ();
		}
	}

	public void DialogueBoxAnimation(float speed, string text, int currentDialogue, int totalDialogue, string npcName){
		if (!GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("PaintDialogue")
		   && !GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("PaintDialogue_Done")) {

			GetComponent<Animator> ().SetTrigger ("Appear");

		}

		fullTextStorage = text;

		textLength = 0;
		dialogueText.GetComponent<Text> ().text = "";

		this.totalDialogue = totalDialogue;
		this.currentDialogue = currentDialogue;

		if (!dialogueBoxOpen) {
			
			dialogueBoxOpen = true;
			StartCoroutine (TextAppear (2.0f, text, npcName));

		} else {
			
			StartCoroutine (TextAppear (0.2f, text, npcName));

		}
			

		dialogueFinished = false;

	}

	IEnumerator TextAppear(float wait, string text, string npcName){

		yield return new WaitForSeconds (wait);

		if (!readStarted) {
			nameTag.gameObject.SetActive (true);
			dialogueText.SetActive (true);
			currentDialogueObj.SetActive (true);
			totalDialogueObj.SetActive (true);

			// Start typing
			coroutine = DialogueTypeFiller_Letter (0.008f, 0.0f, 0.16f, 0.2f, text);
			nameTag.GetComponent<Text> ().text = npcName;

			StartCoroutine (coroutine);
		}

	}

	// Word + Letter
	string 
	storedString = "",
	textCheck = "",
	fullTextStorage = "";

	int 
	textLength = 0;

	bool
	fontStyleItalics = false,	// (_) does font have a style?
	fontStyleBold = false,		// (*) does font have a style?
	fontSlowdown = false;		// (%) should the font be slowed down?

	// Letter
	bool 
	storeCheck = false,
	writeable = false,
	readStarted = false;

	int 
	storedTextLength = 0;

	IEnumerator DialogueTypeFiller_Letter(float defaultLetterSpeed, float defaultReadSpeed, float defaultEffectPause, float sentenceBreak, string text){

		// init
		float letterSpeed, readSpeed, effectPause;

		// set
		letterSpeed = defaultLetterSpeed;
		readSpeed = defaultReadSpeed;
		effectPause = defaultEffectPause;

		readStarted = true;

		if (text.Length < 1)
			TurnOffTextbox ();

		float storedSpeed = readSpeed;

		// Loop - stops when body of text has been completely read
		while (textLength < text.Length && readStarted) {

			// set
			letterSpeed = defaultLetterSpeed;
			readSpeed = defaultReadSpeed;
			effectPause = defaultEffectPause;

			// Stores current progress in case the sentence breaks the border
			if (!storeCheck) {
				storedString = dialogueText.GetComponent<Text> ().text;
				storedTextLength = textLength;
				storeCheck = true;
			}

			// Reads in full words and checks them with the boundaries.  
			//	If word doesn't work out, shifts line down.
			char storedChar = 'a';
			if (!writeable) {
				while (storedChar != ' ' && textLength < text.Length && readStarted) {

					textCheck += text [textLength];

					storedChar = text [textLength];

					dialogueText.GetComponent<Text> ().text += text [textLength];

					textLength++;
				}

				storedChar = 'a';
				writeable = true;

				// If the text body breaks the border, skip it to the next line before continuing
				if (LayoutUtility.GetPreferredWidth (dialogueText.GetComponent<RectTransform> ()) > dialogueText.GetComponent<RectTransform> ().rect.width) {

					if (readStarted) {
						dialogueText.GetComponent<Text> ().text = storedString;
						dialogueText.GetComponent<Text> ().text += "\n";
					}

				} else{

					if (readStarted)
						dialogueText.GetComponent<Text> ().text = storedString;
					
				}

				textLength = storedTextLength;

			} else {	// !writable

				while ((storedChar != ' ' && storedChar != '.' && storedChar != '?' && storedChar != '!')
					&& textLength < text.Length) {

					textCheck += text [textLength];

					storedChar = text [textLength];

					// If the font needs to be changed and hasn't BEEN changed, change it.
					if (storedChar == '_') {
						if (!fontStyleItalics) {
							dialogueText.GetComponent<Text> ().fontStyle = FontStyle.Italic;
							fontStyleItalics = true;
							letterSpeed += effectPause;
						} else if (fontStyleItalics) {
							dialogueText.GetComponent<Text> ().fontStyle = FontStyle.Normal;
							fontStyleItalics = false;
							letterSpeed -= effectPause;
						}
					}

					if (storedChar == '*') {
						if (!fontStyleBold) {
							dialogueText.GetComponent<Text> ().fontStyle = FontStyle.Bold;
							fontStyleBold = true;
							letterSpeed += effectPause;
						} else if (fontStyleBold) {
							dialogueText.GetComponent<Text> ().fontStyle = FontStyle.Normal;
							fontStyleBold = false;
							letterSpeed -= effectPause;
						}
					}

					if (storedChar == '%') {
						if (!fontSlowdown) {
							fontSlowdown = true;
							letterSpeed += effectPause;
						} else if (fontSlowdown) {
							fontSlowdown = false;
							letterSpeed -= effectPause;
						}
					}


					// Don't draw those key variable characters
					if( storedChar != '_' && 
						storedChar != '*' && 
						storedChar != '%')
					if(readStarted)
						dialogueText.GetComponent<Text> ().text += text [textLength];

					textLength++;

					yield return new WaitForSeconds (letterSpeed);
				}

				if (storedChar == '.' || storedChar == '?' || storedChar == '!') {
					readSpeed = sentenceBreak;
					storedChar = 'a';
				} else {
					readSpeed = storedSpeed;
				}

				writeable = false;
				storeCheck = false;

				// If the text body breaks the border, skip it to the next line before continuing
				if (LayoutUtility.GetPreferredWidth (dialogueText.GetComponent<RectTransform> ()) > dialogueText.GetComponent<RectTransform> ().rect.width) {

					dialogueText.GetComponent<Text> ().text = storedString;
					dialogueText.GetComponent<Text> ().text += "\n";
					dialogueText.GetComponent<Text> ().text += textCheck;

				}

				// Reset stored values
				textCheck = "";
				storedString = "";
			}

			yield return new WaitForSeconds (readSpeed);
		}

		string newTextStorage = "";
		foreach (char c in fullTextStorage) {
			if (c == '_' || c == '*' || c == '%') { }
			else  newTextStorage += c;
		}

		dialogueText.GetComponent<Text> ().fontStyle = FontStyle.Normal;
		// Fill in the entire text portion
		dialogueText.GetComponent<Text> ().text = newTextStorage;

		// When finished, let everyone know.
		dialogueFinished = true;

	}
}
