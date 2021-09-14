using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using System.Reflection;

public class NixieCodeScript : MonoBehaviour
{
	//Setting up strike count (Thanks Dark and River!)
	Type bombType = FindType("Bomb");
	int TotalStrikes;

	public KMBombInfo BombInfo;
	public KMBombModule ThisModule;

	public KMAudio BombAudio;

	public KMSelectable Light1, Light2, Light3;
	public KMSelectable SubmitButton;

	public TextMesh LightText1, LightText1Glow1, LightText1Glow2, LightText1Glow3, LightText1Glow4, LightText1Glow5, LightText1Glow6;
	public TextMesh LightText2, LightText2Glow1, LightText2Glow2, LightText2Glow3, LightText2Glow4, LightText2Glow5, LightText2Glow6;
	public TextMesh LightText3, LightText3Glow1, LightText3Glow2, LightText3Glow3, LightText3Glow4, LightText3Glow5, LightText3Glow6;
	public TextMesh StageCount;

	public KMGameCommands BombCommands;

	public GameObject Backlights;

	public List<int> Digits = new List<int>(new int[3]);
	List<int> DigitsInStageCount;
	int CurrentStage = 1;
	int PrimaryTube, SecondaryTube;
	int CorrectTube;
	int DigitOfLast = -999;
	int Digit1Previous = -1, Digit2Previous = -1, Digit3Previous = -1;

	static int moduleIdCounter = 1;
	int ModuleID;

	public List<string> StringCode;
	public string FullCode;

	List<string> CorrectCombinations = new List<string>
	{
		"415", "856", "064", "262", "347",
		"578", "904", "494", "119", "539",
		"647", "735", "110", "957", "420",
		"690", "372", "818", "672", "777",
		"872", "666", "582", "268", "492",
		"178", "215", "349", "431", "529",
		"671", "725", "835", "965", "055",
		"039", "147", "216", "362", "470",
		"513", "651", "796", "802", "987",
	};

	public static Type FindType(string qualifiedTypeName) //Used for finding total strike count.
	{
		Type t = Type.GetType(qualifiedTypeName);

		if (t != null) return t;
		else
		{
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				t = asm.GetType(qualifiedTypeName);
				if (t != null) return t;
			}
			return null;
		}
	}

	void Awake()
    {
		ModuleID = moduleIdCounter++;
	}

	// Use this for initialization
	void Start()
	{
		//Setting up strike count (Thanks Dark and River, again!)
		TotalStrikes = bombType == null ? 3 : (int)bombType.GetField("NumStrikesToLose").GetValue(transform.parent.parent.GetComponent(bombType));

		//Drawing random numbers for the lights
		Digits[0] = Random.Range(0, 10);
		Digits[1] = Random.Range(0, 10);
		Digits[2] = Random.Range(0, 10);
		
		while (Digits[0] == Digit1Previous)
        {
			Digits[0] = Random.Range(0, 10);
		}

		while (Digits[1] == Digit2Previous)
		{
			Digits[1] = Random.Range(0, 10);
		}

		while (Digits[2] == Digit3Previous)
		{
			Digits[2] = Random.Range(0, 10);
		}

		//Applying number to light
		LightText1.text = Digits[0].ToString();
		LightText1Glow1.text = Digits[0].ToString();
		LightText1Glow2.text = Digits[0].ToString();
		LightText1Glow3.text = Digits[0].ToString();
		LightText1Glow4.text = Digits[0].ToString();
		LightText1Glow5.text = Digits[0].ToString();
		LightText1Glow6.text = Digits[0].ToString();

		LightText2.text = Digits[1].ToString();
		LightText2Glow1.text = Digits[1].ToString();
		LightText2Glow2.text = Digits[1].ToString();
		LightText2Glow3.text = Digits[1].ToString();
		LightText2Glow4.text = Digits[1].ToString();
		LightText2Glow5.text = Digits[1].ToString();
		LightText2Glow6.text = Digits[1].ToString();

		LightText3.text = Digits[2].ToString();
		LightText3Glow1.text = Digits[2].ToString();
		LightText3Glow2.text = Digits[2].ToString();
		LightText3Glow3.text = Digits[2].ToString();
		LightText3Glow4.text = Digits[2].ToString();
		LightText3Glow5.text = Digits[2].ToString();
		LightText3Glow6.text = Digits[2].ToString();

		//Setting up KMSelectable actions
		Light1.OnInteract = HandleLight1;
		Light2.OnInteract = HandleLight2;
		Light3.OnInteract = HandleLight3;

		SubmitButton.OnInteract = HandleSubmit;

		FindReferenceTubes();
	}

	void FindReferenceTubes()
	{
		StringCode = Digits.ConvertAll(delegate (int i) { return i.ToString(); });
		FullCode = StringCode[0] + StringCode[1] + StringCode[2];
		Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) The values of the tubes are, from left to right: {2}, {3}, {4}", ModuleID, CurrentStage, Digits[0], Digits[1], Digits[2]);

		DigitsInStageCount = StageCount.text.Select(x => Convert.ToInt32(x.ToString())).ToList();

		if (DigitOfLast == -999)
        {
			DigitOfLast = BombInfo.GetSerialNumberNumbers().First();
        }

		//Primary tube
		if (CurrentStage == 1)
        {
			Debug.LogFormat("<Nixie Code #{0}> (Stage {1}) PRIMARY: Stage 1 rule applies.", ModuleID, CurrentStage);
			PrimaryTube = 0;
		}
		else if (Digits[0] == BombInfo.GetSerialNumberNumbers().Last() || Digits[0] > BombInfo.GetSerialNumberNumbers().First())
        {
			Debug.LogFormat("<Nixie Code #{0}> (Stage {1}) PRIMARY: Serial# first/last number match rule applies. (Serial#: {2} and {3}, digit: {4})", ModuleID, CurrentStage, BombInfo.GetSerialNumberNumbers().First(), BombInfo.GetSerialNumberNumbers().Last(), Digits[0]);
			PrimaryTube = 0;
		}
		else if (Digits[1] == BombInfo.GetBatteryCount() || Digits[1] == BombInfo.GetBatteryHolderCount())
        {
			Debug.LogFormat("<Nixie Code #{0}> (Stage {1}) PRIMARY: Battery/holder match rule applies. (Serial#: {2} and {3}, digit: {4})", ModuleID, CurrentStage, BombInfo.GetBatteryCount(), BombInfo.GetBatteryHolderCount(), Digits[0]);
			PrimaryTube = 1;
		}
		else
        {
			Debug.LogFormat("<Nixie Code #{0}> (Stage {1}) PRIMARY: None apply.", ModuleID, CurrentStage);
			PrimaryTube = 2;
		}


		//Secondary tube
		if (DigitOfLast == Digits[0])
		{
			Debug.LogFormat("<Nixie Code #{0}> (Stage {1}) SECONDARY: Previous tube matches this one rule applies. (Previous digit: {2}, current digit: {3})", ModuleID, CurrentStage, DigitOfLast, Digits[0]);
			SecondaryTube = 0;
		}
		else if (DigitOfLast == CurrentStage)
        {
			Debug.LogFormat("<Nixie Code #{0}> (Stage {1}) SECONDARY: Previous tube matches stage count rule applies. (Previous digit: {2}, stage count digits: {3} and {4})", ModuleID, CurrentStage, DigitOfLast, DigitsInStageCount[0], DigitsInStageCount[1]);
			SecondaryTube = 1;
		}
			
		else
        {
			Debug.LogFormat("<Nixie Code #{0}> (Stage {1}) SECONDARY: None apply.", ModuleID, CurrentStage);
			SecondaryTube = 2;
		}

		Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) The primary tube is {2}", ModuleID, CurrentStage, PrimaryTube + 1);
		Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) The secondary tube is {2}", ModuleID, CurrentStage, SecondaryTube + 1);

		DetermineTubePress(PrimaryTube, SecondaryTube);
	}

	void DetermineTubePress(int Primary, int Secondary)
	{
		List<int[,]> TubePressTable = new List<int[,]>
		{
			new int[3, 3]
			{
				{ 3, 2, 1 },
				{ 1, 3, 2 },
				{ 2, 3, 1 },
			}
		};
		CorrectTube = TubePressTable[0][Secondary, Primary];
		Debug.LogFormat("<Nixie Code #{0}> (Stage {1}) In the table, column {2} row {3} gives {4}", ModuleID, CurrentStage, Primary + 1, Secondary + 1, CorrectTube);
		Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) The correct tube is {2}", ModuleID, CurrentStage, CorrectTube);
		CheckForCorrectCode();
	}

	void CheckForCorrectCode()
    {
		if (CorrectCombinations.Contains(FullCode))
        {
			Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) However, the code \"{2}\" is a valid code, so submitting is allowed.", ModuleID, CurrentStage, FullCode);
		}
		else
        {
			Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) And the code for this stage, \"{2}\", is not a valid code, so submitting is NOT allowed.", ModuleID, CurrentStage, FullCode);
		}
	}

	protected bool HandleLight1()
	{
		GetComponent<KMSelectable>().AddInteractionPunch();
		Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Pressed tube 1...", ModuleID, CurrentStage);
		if (CorrectTube == 1)
		{
			if (CurrentStage != 10)
			{
				Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Tube 1 was correct. Advancing to next stage.", ModuleID, CurrentStage);
				HandleStageText();
				DigitOfLast = Digits[0];
				BombAudio.PlaySoundAtTransform("NixiePress", transform);
				int NewDigit = Random.Range(0, 10);
				while (NewDigit == Digits[0])
				{
					NewDigit = Random.Range(0, 10);
				}
				Digits[0] = NewDigit;

				LightText1.text = Digits[0].ToString();
				LightText1Glow1.text = Digits[0].ToString();
				LightText1Glow2.text = Digits[0].ToString();
				LightText1Glow3.text = Digits[0].ToString();
				LightText1Glow4.text = Digits[0].ToString();
				LightText1Glow5.text = Digits[0].ToString();
				LightText1Glow6.text = Digits[0].ToString();

				FindReferenceTubes();
			}
			else
            {
				Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Tube 1 was correct. 10 stages passed. Module solved.", ModuleID, CurrentStage);
				StartCoroutine(HandleAnimation(true));
			}
		}
		else
		{
			Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Tube 1 was incorrect. Strike given.", ModuleID, CurrentStage);
			if (BombInfo.GetStrikes() == TotalStrikes - 1)
				BombCommands.CauseStrike("Nixie Code (Pressed wrong tube)");
			else
				ThisModule.HandleStrike();
		}
		return false;
	}
	protected bool HandleLight2()
	{
		GetComponent<KMSelectable>().AddInteractionPunch();
		Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Pressed tube 2...", ModuleID, CurrentStage);
		if (CorrectTube == 2)
		{
			if (CurrentStage != 10)
			{
				Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Tube 2 was correct. Advancing to next stage.", ModuleID, CurrentStage);
				HandleStageText();
				DigitOfLast = Digits[1];
				BombAudio.PlaySoundAtTransform("NixiePress", transform);
				int NewDigit = Random.Range(0, 10);
				while (NewDigit == Digits[1])
				{
					NewDigit = Random.Range(0, 10);
				}
				Digits[1] = NewDigit;

				LightText2.text = Digits[1].ToString();
				LightText2Glow1.text = Digits[1].ToString();
				LightText2Glow2.text = Digits[1].ToString();
				LightText2Glow3.text = Digits[1].ToString();
				LightText2Glow4.text = Digits[1].ToString();
				LightText2Glow5.text = Digits[1].ToString();
				LightText2Glow6.text = Digits[1].ToString();

				FindReferenceTubes();
			}
			else
			{
				Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Tube 2 was correct. 10 stages passed. Module solved.", ModuleID, CurrentStage);
				StartCoroutine(HandleAnimation(true));
			}
		}
		else
		{
			Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Tube 2 was incorrect. Strike given.", ModuleID, CurrentStage);
			if (BombInfo.GetStrikes() == TotalStrikes - 1)
				BombCommands.CauseStrike("Nixie Code (Pressed wrong tube)");
			else
				ThisModule.HandleStrike();
		}
		return false;
	}
	protected bool HandleLight3()
	{
		GetComponent<KMSelectable>().AddInteractionPunch();
		Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Pressed tube 3...", ModuleID, CurrentStage);
		if (CorrectTube == 3)
		{
			if (CurrentStage != 10)
			{
				Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Tube 3 was correct. Advancing to next stage.", ModuleID, CurrentStage);
				HandleStageText();
				DigitOfLast = Digits[2];
				BombAudio.PlaySoundAtTransform("NixiePress", transform);
				int NewDigit = Random.Range(0, 10);
				while (NewDigit == Digits[2])
				{
					NewDigit = Random.Range(0, 10);
				}
				Digits[2] = NewDigit;

				LightText3.text = Digits[2].ToString();
				LightText3Glow1.text = Digits[2].ToString();
				LightText3Glow2.text = Digits[2].ToString();
				LightText3Glow3.text = Digits[2].ToString();
				LightText3Glow4.text = Digits[2].ToString();
				LightText3Glow5.text = Digits[2].ToString();
				LightText3Glow6.text = Digits[2].ToString();

				FindReferenceTubes();
			}
			else
            {
				Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Tube 2 was correct. 10 stages passed. Module solved.", ModuleID, CurrentStage);
				StartCoroutine(HandleAnimation(true));
			}
				
		}
		else
		{
			Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Tube 3 was incorrect. Strike given.", ModuleID, CurrentStage);
			if (BombInfo.GetStrikes() == TotalStrikes - 1)
				BombCommands.CauseStrike("Nixie Code (Pressed wrong tube)");
			else
				ThisModule.HandleStrike();
		}
		return false;
	}

	protected bool HandleSubmit()
	{
		Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Pressed the submit button... Code: {2}", ModuleID, CurrentStage, FullCode);
		GetComponent<KMSelectable>().AddInteractionPunch();
		if (CorrectCombinations.Contains(FullCode))
		{
			Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Code is valid. Module solved.", ModuleID, CurrentStage);
			StartCoroutine(HandleAnimation(true));
		}
		else
		{
			Debug.LogFormat("[Nixie Code #{0}] (Stage {1}) Code is invalid. Strike given.", ModuleID, CurrentStage);
			StartCoroutine(HandleAnimation(false));
		}
		return false;
	}

	IEnumerator HandleAnimation(bool Solved)
	{
		BombAudio.PlaySoundAtTransform("SubmitButton", transform);
		Light1.OnInteract = Empty;
		Light2.OnInteract = Empty;
		Light3.OnInteract = Empty;
		SubmitButton.OnInteract = Empty;
		int AnimDigitGen1, AnimDigitGen2, AnimDigitGen3;
		int frame = 0;
		while (frame != 15)
		{
			AnimDigitGen1 = Random.Range(0, 10);
			AnimDigitGen2 = Random.Range(0, 10);
			AnimDigitGen3 = Random.Range(0, 10);

			//Tube 1
			{
				LightText1.text = AnimDigitGen1.ToString();
				LightText1Glow1.text = AnimDigitGen1.ToString();
				LightText1Glow2.text = AnimDigitGen1.ToString();
				LightText1Glow3.text = AnimDigitGen1.ToString();
				LightText1Glow4.text = AnimDigitGen1.ToString();
			}
			//Tube 2
			{
				LightText2.text = AnimDigitGen2.ToString();
				LightText2Glow1.text = AnimDigitGen2.ToString();
				LightText2Glow2.text = AnimDigitGen2.ToString();
				LightText2Glow3.text = AnimDigitGen2.ToString();
				LightText2Glow4.text = AnimDigitGen2.ToString();
			}
			//Tube 3
			{
				LightText3.text = AnimDigitGen3.ToString();
				LightText3Glow1.text = AnimDigitGen3.ToString();
				LightText3Glow2.text = AnimDigitGen3.ToString();
				LightText3Glow3.text = AnimDigitGen3.ToString();
				LightText3Glow4.text = AnimDigitGen3.ToString();
			}
			frame++;
			yield return new WaitForSeconds(0.01f);
		}

		if (Solved)
		{
			ThisModule.HandlePass();
			StageCount.text = "GG";
			//Tube 1
			{
				LightText1.gameObject.SetActive(false);
				LightText1Glow1.gameObject.SetActive(false);
				LightText1Glow2.gameObject.SetActive(false);
				LightText1Glow3.gameObject.SetActive(false);
				LightText1Glow4.gameObject.SetActive(false);
			}
			//Tube 2
			{
				LightText2.gameObject.SetActive(false);
				LightText2Glow1.gameObject.SetActive(false);
				LightText2Glow2.gameObject.SetActive(false);
				LightText2Glow3.gameObject.SetActive(false);
				LightText2Glow4.gameObject.SetActive(false);
			}
			//Tube 3
			{
				LightText3.gameObject.SetActive(false);
				LightText3Glow1.gameObject.SetActive(false);
				LightText3Glow2.gameObject.SetActive(false);
				LightText3Glow3.gameObject.SetActive(false);
				LightText3Glow4.gameObject.SetActive(false);
			}
			//Backlight
			Backlights.SetActive(false);
		}
		else
		{
			if (BombInfo.GetStrikes() == TotalStrikes - 1)
				BombCommands.CauseStrike("Nixie Code (Submitted wrong code)");
			else
				ThisModule.HandleStrike();

			Start();
		}
	}

	void HandleStageText()
	{
		CurrentStage++;
		if (CurrentStage < 10)
		{
			StageCount.text = "0" + CurrentStage.ToString();
		}
		else
		{
			StageCount.text = CurrentStage.ToString();
		}
	}

	protected bool Empty()
	{
		return false;
	}

	//Twitch Plays stuff
	public string TwitchHelpMessage = "Use !{0} press (or simply p), followed by 1, 2, 3 (or l, m, r) to press the first, second or third tube from left to right. Use !{0} submit to submit the current code.";
	IEnumerator ProcessTwitchCommand(string command)
	{
		command = command.ToLower();
		String TrimmedCommand = command.Trim();
		String lastWord = TrimmedCommand.Substring(TrimmedCommand.LastIndexOf(" ") + 1);

		if (command.Equals("submit", StringComparison.InvariantCultureIgnoreCase)) //Submit code
		{
			yield return null;
			SubmitButton.OnInteract();
			yield break;
		}
		if (command.StartsWith("press") || command.StartsWith("p"))
        {
			if (command.EndsWith("1") || command.EndsWith("l")) //Press tube 1
			{
				yield return null;
				Light1.OnInteract();
				yield break;
			}
			else if (command.EndsWith("2") || command.EndsWith("m")) //Press tube 2
			{
				yield return null;
				Light2.OnInteract();
				yield break;
			}
			else if (command.EndsWith("3") || command.EndsWith("r")) //Press tube 3
			{
				yield return null;
				Light3.OnInteract();
				yield break;
			}
			else
            {
				yield return null;
				yield return "sendtochaterror I cannot press the \"" + lastWord + "\"th tube. The options are: 1, 2, 3 or l, m, r.";
				yield break;
			}
		}
		else
        {
			yield return null;
			yield return "sendtochaterror The command \"" + command + "\" does not exist in the current context.";
			yield break;
		}
	}
}
