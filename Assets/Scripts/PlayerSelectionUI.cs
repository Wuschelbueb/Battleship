using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class PlayerMenu : MonoBehaviour {

	[SerializeField] Button FactionButton;

	// TODO this was a dumb, idea: just hard code all the buttons!
	[SerializeField] List<InputKeys> keysToAssign;
	[SerializeField] List<Button> buttons;

	[SerializeField] Button Close;
	[SerializeField] Text FactionText;
	[SerializeField] InputField PlayerNameInput;

	public int FactionNumber = 0;

	public RectTransform rectTransform { get { return GetComponentInChildren<RectTransform>(); } }
	public string playerName { get { return PlayerNameInput.text; } }

	private List<Text> texts;
	private List<KeyCode> definedKeyCodes;
	private bool waitingForInput = false;
	private InputKeys whichKey;

	public int myIndex { get { return Menu.Instance.playerUIList.IndexOf (this); } }
	public Ship myShip { get { return GameManager.Instance.Ships [myIndex]; } }



	/* ----- METHODS ------ */


	/*  Public  */

	public void setDefaultCodes (params KeyCode[] codes) {
		Debug.Assert(codes.Length == buttons.Count);
		definedKeyCodes = new List<KeyCode>();
		definedKeyCodes.AddRange(codes);
		UpdateTexts();

		myShip.FireKey = codes [0];
		myShip.LeftKey = codes [1];
		myShip.RightKey = codes [2];
	}

	/*  Private  */

	private void Start()
	{
		// I use the Start() here as opposed to Awake() as this depends on other Objects (Factions)
		//   having completed the Awake() function.

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

	// TODO this is public, as the color has to be set on spawn, rather ugly.... 
	public void OnFactionButtonClick () {
		FactionNumber = (FactionNumber + 1) % Factions.Count();
		FactionButton.image.sprite = Factions.Get(FactionNumber).UIFlag;
		FactionText.text = Factions.Get (FactionNumber).Name;

		GameManager.Instance.UpdateAllShips ();

		Ship myShip = GameManager.Instance.Ships [Menu.Instance.playerUIList.IndexOf (this)];
		myShip.SetCompassColor (Factions.Get (FactionNumber).color);

	}
    
	private void UpdateTexts () {
		for (int i = 0; i < buttons.Count; i++)
			texts[i].text = "Press " + definedKeyCodes[i] + " for " + keysToAssign[i] + " (Change)";
	}

	private UnityEngine.Events.UnityAction CreateClickHandler (InputKeys key) {
		return () =>
		{
			waitingForInput = true;
			whichKey = key;
		};      
	}

	// Update is called once per frame
	private void Update () {
		if (waitingForInput && Input.anyKey) 
		{
			foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
			{
				if (Input.GetKey(key)) {
                  
					if (!definedKeyCodes.Contains(key)) {
						
						definedKeyCodes[keysToAssign.IndexOf(whichKey)] = key;    

						Ship myShip = GameManager.Instance.Ships [Menu.Instance.playerUIList.IndexOf (this)];

						switch (whichKey) {
						case InputKeys.Fire:
							myShip.FireKey = key;
							break;
						case InputKeys.Left:
							myShip.LeftKey = key;
							break;
						case InputKeys.Right:
							myShip.RightKey = key;
							break;
						default:
							break;
						}
							
                        UpdateTexts();
					}
                    
					waitingForInput = false;
					return;
				}
			}

		}
	}
}
