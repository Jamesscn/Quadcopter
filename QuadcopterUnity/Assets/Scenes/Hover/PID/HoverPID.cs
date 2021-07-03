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
    float Reward = 0.0F;
    bool PIDInitialized = false;

    PID ThrustController;
	PID YawController;
	PID PitchController;
	PID RollController;
    PID DisplacementPitchController;
    PID DisplacementRollController;

    public void Start() {
        if(!PIDInitialized) {
            SetPIDConstants(new double[]{0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D});
            if(!Training) {
                SetPIDConstants(new double[]{7.967D, 3.221D, 3.512D, 3.359D, 0.000D, 0.417D, 9.103D, 0.000D, 5.671D});
            }
            PIDInitialized = true;
            SendMessage("ResetSimulation");
        }
    }

    void SetPIDConstants(double[] chromosome) {
        ThrustController = new PID(chromosome[0], chromosome[1], chromosome[2]);
        YawController = new PID(chromosome[3], chromosome[4], chromosome[5]);
        PitchController = new PID(chromosome[3], chromosome[4], chromosome[5]);
        RollController = new PID(chromosome[3], chromosome[4], chromosome[5]);
        DisplacementPitchController = new PID(chromosome[6], chromosome[7], chromosome[8]);
        DisplacementRollController = new PID(chromosome[6], chromosome[7], chromosome[8]);
        PIDInitialized = true;
    }

    public void InitialisePID(Tuple<int, Genome> data) {
        TrainingIndex = data.Item1;
        SetPIDConstants(data.Item2.GetChromosome());
        SendMessage("ResetSimulation");
        Reward = 0.0F;
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
        double PositionControllerPitch = 0.2D * Math.Tanh(DisplacementPitchController.ComputeOutput(0.0D, pitchDisplacement));
        double PositionControllerRoll = 0.2D * Math.Tanh(DisplacementRollController.ComputeOutput(0.0D, rollDisplacement));
        double ThrustSignal = ThrustController.ComputeOutput(targetHeight, measuredHeight);
        double YawSignal = YawController.ComputeOutput(0.0D, measuredYaw);
        double PitchSignal = PitchController.ComputeOutput(PositionControllerPitch, measuredPitch);
        double RollSignal = RollController.ComputeOutput(PositionControllerRoll, measuredRoll);
        MotorSignals[0] = Math.Tanh(ThrustSignal - YawSignal + PitchSignal - RollSignal);
		MotorSignals[1] = Math.Tanh(ThrustSignal + YawSignal + PitchSignal + RollSignal);
		MotorSignals[2] = Math.Tanh(ThrustSignal - YawSignal - PitchSignal + RollSignal);
		MotorSignals[3] = Math.Tanh(ThrustSignal + YawSignal - PitchSignal - RollSignal);
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
		float maxDistance = 10.0F;
		float distancePower = 0.5F;
		float spinImportance = 0.02F;
		float distanceReward = Mathf.Max(0.0F, 1.0F - Mathf.Pow(distance / maxDistance, distancePower));
		float spinPenalty = -spinImportance * Body.angularVelocity.magnitude;
        Reward += Mathf.Max(0.0F, (distanceReward + spinPenalty)) / Mathf.Max(1.0F, MaxStep);
        if(CurrentStep > MaxStep && Training && !SentEndSignal) {
            float speed = Body.velocity.magnitude;
            float angularSpeed = Body.angularVelocity.magnitude;
            float yaw = Mathf.Atan2(Body.transform.right.z, Body.transform.right.x);
			float pitch = Mathf.Atan2(Body.transform.forward.y, Body.transform.forward.z);
			float roll = Mathf.Atan2(Body.transform.right.y, Body.transform.right.x);
			if(Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
				yaw = -Mathf.Atan2(Body.transform.forward.x, Body.transform.forward.z);
			}
            float[] endValues = {Reward, distance, speed, angularSpeed, yaw, pitch, roll};
            Tuple<int, float[]> endData = new Tuple<int, float[]>(TrainingIndex, endValues);
            SendMessageUpwards("EpisodeEnded", endData);
            SentEndSignal = true;
        }
    }

}
