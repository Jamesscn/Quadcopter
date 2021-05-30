using UnityEngine;

public class UserInterface : MonoBehaviour {

	public GUISkin InterfaceSkin;

	public static float Thrust = -1.0F;
	public static float Yaw = 0.0F;
	public static float Pitch = 0.0F;
	public static float Roll = 0.0F;

    void OnGUI() {
		GUI.skin = InterfaceSkin;

		GUI.Box(new Rect(0, 0, 190, 110), GUIContent.none);
		GUI.Label(new Rect(130, 10, 100, 20), "Thrust");
		GUI.Label(new Rect(130, 30, 100, 20), "Yaw");
		GUI.Label(new Rect(130, 50, 100, 20), "Pitch");
		GUI.Label(new Rect(130, 70, 100, 20), "Roll");

		Thrust = GUI.HorizontalSlider(new Rect(20, 20, 100, 20), Thrust, -1.0F, 1.0F);
		Yaw = GUI.HorizontalSlider(new Rect(20, 40, 100, 20), Yaw, -0.1F, 0.1F);
		Pitch = GUI.HorizontalSlider(new Rect(20, 60, 100, 20), Pitch, -0.1F, 0.1F);
		Roll = GUI.HorizontalSlider(new Rect(20, 80, 100, 20), Roll, -0.1F, 0.1F);
	}
}
