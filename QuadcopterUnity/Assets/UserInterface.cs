using UnityEngine;

public class UserInterface : MonoBehaviour {

	//The following variable changes the appearance of the user interface.
	public GUISkin InterfaceSkin;

	//Four values for thrust, yaw, pitch and roll are initialised. These values have ranges between -1 and +1.
	//These values are static so that they can be easily accessed by other scripts.
	public static float Thrust = -1.0F;
	public static float Yaw = 0.0F;
	public static float Pitch = 0.0F;
	public static float Roll = 0.0F;

	//This function is called periodically, and is used to update the user interface.
    void OnGUI() {
		//The appearance of the interface is changed.
		GUI.skin = InterfaceSkin;

		//A box and some labels are added to help the user understand which control does what.
		GUI.Box(new Rect(0, 0, 190, 110), GUIContent.none);
		GUI.Label(new Rect(130, 10, 100, 20), "Thrust");
		GUI.Label(new Rect(130, 30, 100, 20), "Yaw");
		GUI.Label(new Rect(130, 50, 100, 20), "Pitch");
		GUI.Label(new Rect(130, 70, 100, 20), "Roll");

		//The four signals are given a slider on the screen.
		Thrust = GUI.HorizontalSlider(new Rect(20, 20, 100, 20), Thrust, -1.0F, 1.0F);
		Yaw = GUI.HorizontalSlider(new Rect(20, 40, 100, 20), Yaw, -1.0F, 1.0F);
		Pitch = GUI.HorizontalSlider(new Rect(20, 60, 100, 20), Pitch, -0.1F, 0.1F);
		Roll = GUI.HorizontalSlider(new Rect(20, 80, 100, 20), Roll, -0.1F, 0.1F);
		
	}
}
