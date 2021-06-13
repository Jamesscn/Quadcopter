using UnityEngine;

//coefficients:
//thrust: 0.2, 0.001, 0.7
//rotation: 0.01, 1e-6, 0.05

//generalise this later

public class GeneticAlgorithm : MonoBehaviour {

    public Rigidbody Body;
    public GameObject Target;
    public int MaxStep;
    public double MaxVoltage;
    public bool AllowReverseFlight;

    int CurrentStep = 0;

    PID ThrustController;
	PID YawController;
	PID PitchController;
	PID RollController;
	PID CorrectivePitchController;
	PID CorrectiveRollController;

    public void Start() {
        ThrustController = new PID(0.2D, 0.001D, 0.7D);
        YawController = new PID(0.01D, 0.000001D, 0.05D);
        PitchController = new PID(0.01D, 0.000001D, 0.05D);
        RollController = new PID(0.01D, 0.000001D, 0.05D);
        CorrectivePitchController = new PID(0.0D, 0.0D, 0.0D);
        CorrectiveRollController = new PID(0.0D, 0.0D, 0.0D);
    }

    public double ScaleAction(double value, double min, double max) {
        return (value + 1.0D) * (max - min) / 2.0D + min;
    }

    public double[] Controller() {
        double[] MotorSignals = {0.0D, 0.0D, 0.0D, 0.0D};
        Vector3 localTarget = Body.transform.InverseTransformPoint(Target.transform.position);
        float pitchDisplacement = -localTarget[2];
        float rollDisplacement = localTarget[0];
        float targetHeight = Target.transform.position[1];
        float measuredHeight = Body.transform.position[1];
        float measuredYaw = Body.transform.rotation.eulerAngles[1];
        float measuredPitch = Body.transform.rotation.eulerAngles[0];
        float measuredRoll = Body.transform.rotation.eulerAngles[2];
        if(measuredYaw > 180.0F) {
            measuredYaw -= 360.0F;
        }
        if(measuredPitch > 180.0F) {
            measuredPitch -= 360.0F;
        }
        if(measuredRoll > 180.0F) {
            measuredRoll -= 360.0F;
        }
        double CorrectivePitchAngle = CorrectivePitchController.ComputeOutput(0.0D, pitchDisplacement);
        double CorrectiveRollAngle = CorrectiveRollController.ComputeOutput(0.0D, rollDisplacement);
        double Thrust = ThrustController.ComputeOutput(targetHeight, measuredHeight);
        double Yaw = YawController.ComputeOutput(0.0D, measuredYaw);
        double Pitch = PitchController.ComputeOutput(CorrectivePitchAngle, measuredPitch);
        double Roll = RollController.ComputeOutput(CorrectiveRollAngle, measuredRoll);
        MotorSignals[0] = Thrust + Yaw - Pitch - Roll;
		MotorSignals[1] = Thrust - Yaw - Pitch + Roll;
		MotorSignals[2] = Thrust + Yaw + Pitch + Roll;
		MotorSignals[3] = Thrust - Yaw + Pitch - Roll;
        return MotorSignals;
    }

    public void SendActions(double[] vectorAction) {
		double[] voltages = new double[4];
		for(int i = 0; i < 4; i++) {
			if(AllowReverseFlight) {
				voltages[i] = ScaleAction(vectorAction[i], -MaxVoltage, MaxVoltage);
			} else {
				voltages[i] = ScaleAction(vectorAction[i], 0, MaxVoltage);
			}
		}
		gameObject.SendMessage("SetVoltages", voltages);
	}

    public void FixedUpdate() {
        //send and receive actions
        SendActions(Controller());
        CurrentStep += 1;
        if(CurrentStep > MaxStep) {
            gameObject.SendMessage("ResetSimulation");
            CurrentStep = 0;
        }
    }

}
