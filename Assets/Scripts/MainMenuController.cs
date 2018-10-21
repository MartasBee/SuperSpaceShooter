using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
	// resolution of screen
	private int screenWidth;
	private int screenHeight;

	// array with rectangles of all buttons
	private Rect mainButtonsRect;
	//
	private Vector2 mainButtonsHeight;

	//
	private bool bestScoreShowed = false;
	//
	private string bestScoreTextTable = "";

	//
	private bool creditsShowed = false;
	//
	private string creditsText = "";

	//
	private bool helpShowed = false;
	//
	private string helpText = "";

	//
	private Vector2  textPageScrollVec = Vector2.zero;
	//
	private Rect 	 textPageScrollRect;
	//
	private GUIStyle textPageTextStyle;

	/**
	 * 
	 */
	void CalcButtonRects ()
	{
		screenWidth  		= Screen.width;
		screenHeight 		= Screen.height;

		// count of currently shown buttons
		// right now, we have 5 buttons
		int buttonsCount	= 6;

		// basic button properties
		int buttonWidth    	= (int)(screenWidth * 0.5f);
		int buttonHeight   	= 160;
		int betweenButtons 	= 40;
				
		// total height of all showed buttons including vert spaces between buttons
		int totalHeight 	= (int)(screenHeight * 0.6f);

		// is there enought vertical space for buttons ?!
		while (totalHeight < ((buttonsCount * buttonHeight) + ((buttonsCount - 1) * betweenButtons)))
		{
			// there is not enought vertical space, do smaller buttons
			buttonHeight   -= 20;
			betweenButtons -= 5;
		}

		// store height of button (mainButtonsHeight.x) and vertical space between buttons (mainButtonsHeight.y)
		mainButtonsHeight	= new Vector2 (buttonHeight, betweenButtons); 
		
		// position of first button rectangle
		int buttonPosX	   	= (int)(screenWidth  * ((1.0f - ((float)buttonWidth / screenWidth))  / 2.0f));
		int buttonPosY	   	= (int)(screenHeight * ((1.0f - ((float)totalHeight / screenHeight)) / 2.0f));

		// calculate final rectangles of all buttons
		mainButtonsRect     = new Rect (buttonPosX, buttonPosY, buttonWidth, totalHeight);
	}

	/**
	 * 
	 */
	void InitTextBestScore ()
	{
		Statistics.GetInst ().LoadStatistics ();
		
		bestScoreTextTable  = "<size=20><b>Total Score</b></size>\n";
		bestScoreTextTable += "\n\t<b>Score: </b>" + Statistics.GetInst ().nesTotScore.totalScore;
		bestScoreTextTable += "\n\t<b>Waves survived: </b>" + Statistics.GetInst ().nesTotScore.totalSurvivedWaves;
		bestScoreTextTable += "\n\t<b>Enemies destroyed: </b>" + Statistics.GetInst ().nesTotScore.totalDestroyedEnemies;
		bestScoreTextTable += "\n\t<b>Asteroids destroyed: </b>" + Statistics.GetInst ().nesTotScore.totalDestroyedAsteroids;
		bestScoreTextTable += "\n\n\n";
		bestScoreTextTable += "<size=20><b>Top 20 Rank</b></size>\n";
		
		int iScores = 0;
		for (; iScores < Statistics.GetInst ().nesBestScores.listBestScores.Count; iScores++)
		{
			bestScoreTextTable += "\n\t" + (iScores+1) + ".\t<b>";
			bestScoreTextTable += Statistics.GetInst ().nesBestScores.listBestScores[iScores].bestScore;
			bestScoreTextTable += "</b>\n\t\t<size=10><i>";
			bestScoreTextTable += Statistics.GetInst ().nesBestScores.listBestScores[iScores].bestScoreTime + "</i></size>";
		}
		for (; iScores < Statistics.GetInst ().nesBestScores.maxElements; iScores++)
		{
			bestScoreTextTable += "\n\t" + (iScores+1) + ".\t<b>--</b>\n";
		}
	}

	/**
	 * 
	 */
	void InitTextCredits ()
	{
		creditsText  = "<size=20><b>Credits</b></size>";
		creditsText += "\n\n";
		creditsText += "\tGame: <b>Invasion Shoooter</b>";
		creditsText += "\n\tVersion: <b>v0.3</b>";
		creditsText += "\n\n";
		creditsText += "\tAuthor: <b>Ing. Martin Balaz</b>";
		creditsText += "\n\tContact: <b>draconis.sigma@gmail.com</b>";
		creditsText += "\n\n";
		creditsText += "\tDeveloped in <b><i>Unity 3D</i></b>";
		creditsText += "\n\n\n";
		creditsText += "<size=20><b>Special Thanks</b></size>";
		creditsText += "\n\n";
		creditsText += "\t<b>CGPitbull</b>";
		creditsText += "\n\t<size=10><i>Fighter AK-5 (3D Model)</i></size>";
		creditsText += "\n\n";
		creditsText += "\t<b>Unity Technologies</b>";
		creditsText += "\n\t<size=10><i>Default Fighter, Enemy Fighters, Asteroids (3D Models)</i></size>";
		creditsText += "\n\n";
	}

	/**
	 * 
	 */
	void InitTextHelp ()
	{
		helpText  = "<size=20><b>Help</b></size>";
		helpText += "\n\n";
		helpText += "Short description about how to play the game <b>Invasion Shoooter</b>";
		helpText += "\n\n\n";
		helpText += "<size=15><b>PC Version</b></size>\n";
		helpText += "\n>> <b>Ship movement</b>\n\t\t<i>Keyboard arrows</i>";
		helpText += "\n>> <b>Ship fire</b>\n\t\t<i>Left Ctrl</i>\n\t\t<i>Left mouse button</i>\n";
		helpText += "\n>> <b>Menu navigation</b>\n\t\t<i>Esc key</i>";
		helpText += "\n\n\n";
		helpText += "<size=15><b>Android Version</b></size>\n";
		helpText += "\n>> <b>Ship movement</b>\n\t\t<i>Accelerometer</i>";
		helpText += "\n>> <b>Ship fire</b>\n\t\t<i>Touch the screen</i>\n";
		helpText += "\n>> <b>Menu navigation</b>\n\t\t<i>Back button</i>";
		helpText += "\n\n";
	}

	/**
	 * 
	 */
	void CalcBestScoreRect ()
	{
		screenWidth  	= Screen.width;
		screenHeight 	= Screen.height;

		int scoreWidth  = (int)(screenWidth  * 0.80f);
		int scoreHeight = (int)(screenHeight * 0.70f);

		int scorePosX	= (int)(screenWidth  * ((1.0f - ((float)scoreWidth  / screenWidth))  / 2.0f));
		int scorePosY	= (int)(screenHeight * ((1.0f - ((float)scoreHeight / screenHeight)) / 2.0f));

		textPageScrollRect = new Rect (scorePosX, scorePosY, scoreWidth, scoreHeight);
	}

	void OnGUI ()
	{
		if ((Screen.width != screenWidth) || (Screen.height != screenHeight))
		{
			CalcButtonRects ();
			CalcBestScoreRect ();

			textPageTextStyle = new GUIStyle ("box");
			textPageTextStyle.richText  = true;
			textPageTextStyle.alignment = TextAnchor.UpperLeft;
			textPageTextStyle.font		= new Font ("Times New Roman");
		}

		if (bestScoreShowed)
		{
			if (bestScoreTextTable.Length <= 0)
				InitTextBestScore ();

			GUILayout.BeginArea (textPageScrollRect);
			{
				textPageScrollVec = GUILayout.BeginScrollView (textPageScrollVec,
				                                                GUILayout.Width (textPageScrollRect.width),
				                                                GUILayout.Height (textPageScrollRect.height));
				{
					GUILayout.Label (bestScoreTextTable);
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndArea ();
		}
		else if (creditsShowed)
		{
			if (creditsText.Length <= 0)
				InitTextCredits ();
			
			GUILayout.BeginArea (textPageScrollRect);
			{
				textPageScrollVec = GUILayout.BeginScrollView (textPageScrollVec,
				                                               GUILayout.Width (textPageScrollRect.width),
				                                               GUILayout.Height (textPageScrollRect.height));
				{
					GUILayout.Label (creditsText);
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndArea ();
		}
		else if (helpShowed)
		{
			if (helpText.Length <= 0)
				InitTextHelp ();

			GUILayout.BeginArea (textPageScrollRect);
			{
				textPageScrollVec = GUILayout.BeginScrollView (textPageScrollVec,
				                                               GUILayout.Width (textPageScrollRect.width),
				                                               GUILayout.Height (textPageScrollRect.height));
				{
					GUILayout.Label (helpText);
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndArea ();
		}
		else
		{
			GUILayout.BeginArea (mainButtonsRect);
			{
				GUILayout.BeginVertical ();
				{
					//GUILayout.FlexibleSpace();
					if (GUILayout.Button ("New Game", GUILayout.ExpandHeight (true)))// GUILayout.Height(mainButtonsHeight.x)))
					{
						if (GamePlaySave.GetInst ().LoadNewGame ())
							Application.LoadLevel (RuntimeContext.GetInst ().nesActLevel.levelName);
					}
					GUILayout.Space(mainButtonsHeight.y);
					if (GUILayout.Button ("Load Game", GUILayout.ExpandHeight (true)))// GUILayout.Height(mainButtonsHeight.x)))
					{
						if (GamePlaySave.GetInst ().LoadProgress ())
							Application.LoadLevel (RuntimeContext.GetInst ().nesActLevel.levelName);
					}
					GUILayout.Space(mainButtonsHeight.y);
					if (GUILayout.Button ("Best Scores", GUILayout.ExpandHeight (true)))// GUILayout.Height(mainButtonsHeight.x)))
					{
						bestScoreShowed = true;
					}
					GUILayout.Space(mainButtonsHeight.y);
					if (GUILayout.Button ("Ships", GUILayout.ExpandHeight (true)))// GUILayout.Height(mainButtonsHeight.x)))
					{
						Application.LoadLevel (eGameLevels.Screen_ShipSelect.ToString ());
					}
					GUILayout.Space(mainButtonsHeight.y);
					if (GUILayout.Button ("Credits", GUILayout.ExpandHeight (true)))// GUILayout.Height(mainButtonsHeight.x)))
					{
						creditsShowed = true;
					}
					GUILayout.Space(mainButtonsHeight.y);
					if (GUILayout.Button ("Help", GUILayout.ExpandHeight (true)))// GUILayout.Height(mainButtonsHeight.x)))
					{
						helpShowed = true;
					}
					//GUILayout.FlexibleSpace();
				}
				GUILayout.EndVertical ();
			}
			GUILayout.EndArea ();
		}
	}

	void Update ()
	{
#if UNITY_ANDROID
		// it should work on android too...
		if (Input.GetKey (KeyCode.Escape))
#else
		if (Input.GetKeyDown (KeyCode.Escape))
#endif
		{
			if (bestScoreShowed)
				bestScoreShowed = false;
			else if (creditsShowed)
				creditsShowed = false;
			else if (helpShowed)
				helpShowed = false;
			else
				Application.Quit ();
		}
	}
}
