using System;
using UnityEngine;

public class HoverPID : MonoBehaviour {

    public Rigidbody Body;
    public GameObject Target;
    public int MaxStep;
    public double MaxVoltage;
    public bool Training;

    bool SentEndSignal;
    int TrainingIndex;
    int CurrentStep = 0;
    double reward = 0.0D;

    PID ThrustController;
	PID YawController;
	PID PitchController;
	PID RollController;
    PID DisplacementPitchController;
    PID DisplacementRollController;

    public void Start() {
        SetPIDConstants(new double[]{0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D});
        if(!Training) {
            SetPIDConstants(new double[]{6.2686D, 4.195D, 4.4783D, 6.56D, 6.8595D, 1.5698D, 4.23247D, 1.2628D, 5.33989D});
        }
        SendMessage("ResetSimulation");
    }

    void SetPIDConstants(double[] chromosome) {
        ThrustController = new PID(chromosome[0], chromosome[1], chromosome[2]);
        YawController = new PID(chromosome[3], chromosome[4], chromosome[5]);
        PitchController = new PID(chromosome[3], chromosome[4], chromosome[5]);
        RollController = new PID(chromosome[3], chromosome[4], chromosome[5]);
        DisplacementPitchController = new PID(chromosome[6], chromosome[7], chromosome[8]);
        DisplacementRollController = new PID(chromosome[6], chromosome[7], chromosome[8]);
    }

    public void InitialisePID(Tuple<int, Genome> data) {
        TrainingIndex = data.Item1;
        SetPIDConstants(data.Item2.GetChromosome());
        SendMessage("ResetSimulation");
        reward = 0.0D;
        CurrentStep = 0;
        SentEndSignal = false;
    }

    double ScaleAction(double value, double min, double max) {
        return (value + 1.0D) * (max - min) / 2.0D + min;
    }

    double[] Controller() {
        double[] MotorSignals = {0.0D, 0.0D, 0.0D, 0.0D};
        Vector3 localTarget = Body.transform.InverseTransformPoint(Target.transform.position);
        float pitchDisplacement = localTarget[2];
        float rollDisplacement = localTarget[0];
        float targetHeight = Target.transform.position[1];
        float measuredHeight = Body.transform.position[1];
        float measuredYaw = Mathf.Atan2(Body.transform.right.z, Body.transform.right.x);
        float measuredPitch = Mathf.Atan2(Body.transform.forward.y, Body.transform.forward.z);
        float measuredRoll = Mathf.Atan2(Body.transform.right.y, Body.transform.right.x);
        if(Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
            measuredYaw = -Mathf.Atan2(Body.transform.forward.x, Body.transform.forward.z);
        }
        double PositionControllerPitch = 0.1D * System.Math.Atan(DisplacementPitchController.ComputeOutput(0.0D, pitchDisplacement));//0.02D * System.Math.Atan(pitchDisplacement);
        double PositionControllerRoll = 0.1D * System.Math.Atan(DisplacementRollController.ComputeOutput(0.0D, rollDisplacement));//0.02D * System.Math.Atan(rollDisplacement);
        double ThrustSignal = ThrustController.ComputeOutput(targetHeight, measuredHeight);
        double YawSignal = YawController.ComputeOutput(0.0D, measuredYaw);
        double PitchSignal = PitchController.ComputeOutput(PositionControllerPitch, measuredPitch);
        double RollSignal = RollController.ComputeOutput(PositionControllerRoll, measuredRoll);
        MotorSignals[0] = ThrustSignal - YawSignal + PitchSignal - RollSignal;
		MotorSignals[1] = ThrustSignal + YawSignal + PitchSignal + RollSignal;
		MotorSignals[2] = ThrustSignal - YawSignal - PitchSignal + RollSignal;
		MotorSignals[3] = ThrustSignal + YawSignal - PitchSignal - RollSignal;
        for(int i = 0; i < 4; i++) {
            if(MotorSignals[i] < -1.0D) {
                MotorSignals[i] = -1.0D;
            }
            if(MotorSignals[i] > 1.0D) {
                MotorSignals[i] = 1.0D;
            }
        }
        return MotorSignals;
    }

    void SendActions(double[] vectorAction) {
		double[] voltages = new double[4];
		for(int i = 0; i < 4; i++) {
			voltages[i] = ScaleAction(vectorAction[i], 0, MaxVoltage);
		}
		SendMessage("SetVoltages", voltages);
	}

    public void FixedUpdate() {
        SendActions(Controller());
        CurrentStep += 1;
        Vector3 differenceVector = Target.transform.position - Body.transform.position;
		float distance = differenceVector.magnitude;
		float maxDistance = 14.0F;
		float distancePower = 0.5F;
		float spinImportance = 0.02F;
		float distanceReward = Mathf.Max(0.0F, 1.0F - Mathf.Pow(distance / maxDistance, distancePower));
		float spinPenalty = -spinImportance * Body.angularVelocity.magnitude;
        reward += (distanceReward - spinPenalty) / Mathf.Max(1, MaxStep);
        if(CurrentStep > MaxStep && Training && !SentEndSignal) {
            //measure distance, Body.velocity.magnitude, Body.angularVelocity.magnitude and Vector3.Dot(Body.transform.up, Vector3.up)
            Tuple<int, double> rewardData = new Tuple<int, double>(TrainingIndex, reward);
            SendMessageUpwards("EpisodeEnded", rewardData);
            SentEndSignal = true;
        }
    }

}
