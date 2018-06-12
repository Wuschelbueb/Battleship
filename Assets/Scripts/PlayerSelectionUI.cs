using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class PlayerSelectionUI : MonoBehaviour {

	[SerializeField] Button FactionButton;

	[SerializeField] List<InputKeys> keysToAssign;
	[SerializeField] List<Button> buttons;

	[SerializeField] Button Close;
	[SerializeField] Text FactionText;
	[SerializeField] InputField PlayerNameInput;

	public int FactionNumber = 0;

	private List<Text> texts;
	private List<KeyCode> definedKeyCodes;
	private bool waitingForInput = false;
	private InputKeys whichKey;

	void Awake()
	{
		Debug.Assert(keysToAssign.Count == buttons.Count);

		texts = new List<Text>();

		for (int i = 0; i < buttons.Count; i++)
		{
			buttons[i].onClick.AddListener(CreateClickHandler(keysToAssign[i]));
			texts.Add(buttons[i].GetComponentInChildren<Text>());
		}

		Debug.Assert(texts.Count == keysToAssign.Count);

		setDefaultCodes(KeyCode.A, KeyCode.S, KeyCode.D);

		FactionButton.image.sprite = Factions.Get(FactionNumber).UIFlag;
		FactionButton.onClick.AddListener(OnFactionButtonClick);
		FactionText.text = Factions.Get (FactionNumber).Name;

		Close.onClick.AddListener (() => {
			Menu.Instance.DestroyMe(this);
			GameObject.Destroy(gameObject);
		});
	}

	private void OnFactionButtonClick () {
		FactionNumber = (FactionNumber + 1) % Factions.Count();
		FactionButton.image.sprite = Factions.Get(FactionNumber).UIFlag;
		FactionText.text = Factions.Get (FactionNumber).Name;
	}

	public void setDefaultCodes (params KeyCode[] codes) {
		Debug.Assert(codes.Length == buttons.Count);
		definedKeyCodes = new List<KeyCode>();
		definedKeyCodes.AddRange(codes);
		UpdateTexts();
	}
    
	private void UpdateTexts () {
		for (int i = 0; i < buttons.Count; i++)
			texts[i].text = "Press " + definedKeyCodes[i] + " for " + keysToAssign[i] + " (Change keybinding)";
	}

	private UnityEngine.Events.UnityAction CreateClickHandler (InputKeys key) {
		return () =>
		{
			waitingForInput = true;
			whichKey = key;
			//Debug.Log("listening for keys to assign to " + key.ToString());
		};      
	}

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		if (waitingForInput && Input.anyKey) 
		{
			foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
			{
				if (Input.GetKey(key)) {
                  
					if (!definedKeyCodes.Contains(key)) {
						
						definedKeyCodes[keysToAssign.IndexOf(whichKey)] = key;                    
                        //Debug.Log("got defintion " + key.ToString() + " for " + whichKey);
                        UpdateTexts();
					}
                    
					waitingForInput = false;
					return;
				}
			}

		}
	}
}
