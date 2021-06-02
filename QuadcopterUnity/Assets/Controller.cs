using UnityEngine;

public class Controller : MonoBehaviour {

	public Rigidbody Body;
	public GameObject Target;
	public GUISkin InterfaceSkin;
	public Vector3 ThrustCoefficients;
	public Vector3 RotationCoefficients;
	public Vector3 CorrectiveCoefficients;

	public static float[] MotorSignals = {0.0F, 0.0F, 0.0F, 0.0F};

	public bool ManualMode;

	double Thrust = 0.0D;
	double Yaw = 0.0D;
	double Pitch = 0.0D;
	double Roll = 0.0D;

	PID ThrustController;
	PID YawController;
	PID PitchController;
	PID RollController;

	PID CorrectivePitchController;
	PID CorrectiveRollController;

	public void ResetSimulation() {
		ThrustController = new PID(ThrustCoefficients[0], ThrustCoefficients[1], ThrustCoefficients[2]);
		YawController = new PID(RotationCoefficients[0], RotationCoefficients[1], RotationCoefficients[2]);
		PitchController = new PID(RotationCoefficients[0], RotationCoefficients[1], RotationCoefficients[2]);
		RollController = new PID(RotationCoefficients[0], RotationCoefficients[1], RotationCoefficients[2]);
		CorrectivePitchController = new PID(CorrectiveCoefficients[0], CorrectiveCoefficients[1], CorrectiveCoefficients[2]);
		CorrectiveRollController = new PID(CorrectiveCoefficients[0], CorrectiveCoefficients[1], CorrectiveCoefficients[2]);
	}

	public void UpdateController() {
		if(!ManualMode) {
			Vector3 localTarget = Body.transform.InverseTransformPoint(Target.transform.position);
			float pitchDisplacement = -localTarget[2];
			float rollDisplacement = localTarget[0];
			float targetHeight = Target.transform.position[1];
			float measuredHeight = Body.transform.position[1];
			float measuredPitch = Body.transform.rotation.eulerAngles[0];
			float measuredYaw = Body.transform.rotation.eulerAngles[1];
			float measuredRoll = Body.transform.rotation.eulerAngles[2];
			if(measuredPitch > 180.0F) {
				measuredPitch -= 360.0F;
			}
			if(measuredYaw > 180.0F) {
				measuredYaw -= 360.0F;
			}
			if(measuredRoll > 180.0F) {
				measuredRoll -= 360.0F;
			}
			double CorrectivePitchAngle = CorrectivePitchController.ComputeOutput(0.0D, pitchDisplacement);
			double CorrectiveRollAngle = CorrectiveRollController.ComputeOutput(0.0D, rollDisplacement);
			Thrust = ThrustController.ComputeOutput(targetHeight, measuredHeight);
			Yaw = YawController.ComputeOutput(0.0D, measuredYaw);
			Pitch = PitchController.ComputeOutput(CorrectivePitchAngle, measuredPitch);
			Roll = RollController.ComputeOutput(CorrectiveRollAngle, measuredRoll);
		}
		MotorSignals[0] = (float)(Thrust - Pitch + Yaw - Roll);
		MotorSignals[1] = (float)(Thrust - Pitch - Yaw + Roll);
		MotorSignals[2] = (float)(Thrust + Pitch + Yaw + Roll);
		MotorSignals[3] = (float)(Thrust + Pitch - Yaw - Roll);
		for(int i = 0; i < 4; i++) {
			MotorSignals[i] = Mathf.Clamp(MotorSignals[i], -1.0F, 1.0F);
		}
	}

    void OnGUI() {
		if(ManualMode) {
			GUI.skin = InterfaceSkin;

			GUI.Box(new Rect(0, 0, 190, 110), GUIContent.none);
			GUI.Label(new Rect(130, 10, 100, 20), "Thrust");
			GUI.Label(new Rect(130, 30, 100, 20), "Yaw");
			GUI.Label(new Rect(130, 50, 100, 20), "Pitch");
			GUI.Label(new Rect(130, 70, 100, 20), "Roll");

			Thrust = GUI.HorizontalSlider(new Rect(20, 20, 100, 20), (float)Thrust, -1.0F, 1.0F);
			Yaw = GUI.HorizontalSlider(new Rect(20, 40, 100, 20), (float)Yaw, -0.1F, 0.1F);
			Pitch = GUI.HorizontalSlider(new Rect(20, 60, 100, 20), (float)Pitch, -0.1F, 0.1F);
			Roll = GUI.HorizontalSlider(new Rect(20, 80, 100, 20), (float)Roll, -0.1F, 0.1F);
		}
	}
}
