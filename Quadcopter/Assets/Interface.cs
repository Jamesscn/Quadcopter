using UnityEngine;


/**
The Interface class is used to draw a graphical interface which allows the Thrust, Yaw, Pitch and Roll to be adjusted while debugging.
*/
public class Interface : MonoBehaviour {
    
    public GUISkin InterfaceSkin;
    float UserThrust = -1.0F;
	float UserYaw = 0.0F;
	float UserPitch = 0.0F;
	float UserRoll = 0.0F;
    float[] actionsOut = {-1.0F, -1.0F, -1.0F, -1.0F};

    void OnGUI() {
        GUI.skin = InterfaceSkin;

		GUI.Box(new Rect(0, 0, 190, 110), GUIContent.none);
		GUI.Label(new Rect(130, 10, 100, 20), "Thrust");
		GUI.Label(new Rect(130, 30, 100, 20), "Yaw");
		GUI.Label(new Rect(130, 50, 100, 20), "Pitch");
		GUI.Label(new Rect(130, 70, 100, 20), "Roll");

		UserThrust = GUI.HorizontalSlider(new Rect(20, 20, 100, 20), UserThrust, -1.0F, 1.0F);
		UserYaw = GUI.HorizontalSlider(new Rect(20, 40, 100, 20), UserYaw, -0.1F, 0.1F);
		UserPitch = GUI.HorizontalSlider(new Rect(20, 60, 100, 20), UserPitch, -0.1F, 0.1F);
		UserRoll = GUI.HorizontalSlider(new Rect(20, 80, 100, 20), UserRoll, -0.1F, 0.1F);
		actionsOut[0] = Mathf.Clamp(UserThrust + UserYaw - UserPitch - UserRoll, -1.0F, 1.0F);
		actionsOut[1] = Mathf.Clamp(UserThrust - UserYaw - UserPitch + UserRoll, -1.0F, 1.0F);
		actionsOut[2] = Mathf.Clamp(UserThrust + UserYaw + UserPitch + UserRoll, -1.0F, 1.0F);
		actionsOut[3] = Mathf.Clamp(UserThrust - UserYaw + UserPitch - UserRoll, -1.0F, 1.0F);
        SendMessage("GetHeuristic", actionsOut);
    }

}
