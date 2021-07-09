using System;
using UnityEngine;

public class HoverPID : MonoBehaviour {

    public Rigidbody Body;
    public GameObject Target;
    public int MaxStep;
    public double MaxVoltage;
    public double NoiseStrength;
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

    double Clamp(double value, double min, double max) {
        if(value < min) {
            value = min;
        }
        if(value > max) {
            value = max;
        }
        return value;
    }

    double ScaleAction(double value, double min, double max) {
        return (value + 1.0D) * (max - min) / 2.0D + min;
    }

    double AddNoise(double value, double seed) {
        value += 2.0D * NoiseStrength * (Mathf.PerlinNoise(Time.fixedTime, (float)seed) - 0.5D);
        return value;
    }

    public void Start() {
        if(!PIDInitialized) {
            SetPIDConstants(new double[]{0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D});
            if(!Training) {
                SetPIDConstants(new double[]{0.10000D, 0.09540D, 0.07289D, 0.05738D, 0.00000D, 0.01660D, 0.03429D, 0.00000D, 0.09955D});
            }
            PIDInitialized = true;
            SendMessage("ResetSimulation");
        }
    }

    void SetPIDConstants(double[] chromosome) {
        ThrustController = new PID(chromosome[0], chromosome[1], chromosome[2], 0.0D, 1.0D);
        YawController = new PID(chromosome[3], chromosome[4], chromosome[5], -0.5D, 0.5D);
        PitchController = new PID(chromosome[3], chromosome[4], chromosome[5], -0.5D, 0.5D);
        RollController = new PID(chromosome[3], chromosome[4], chromosome[5], -0.5D, 0.5D);
        DisplacementPitchController = new PID(chromosome[6], chromosome[7], chromosome[8], -0.2D, 0.2D);
        DisplacementRollController = new PID(chromosome[6], chromosome[7], chromosome[8], -0.2D, 0.2D);
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

    double[] Controller() {
        double[] MotorSignals = {0.0D, 0.0D, 0.0D, 0.0D};
        Vector3 localTarget = Body.transform.InverseTransformPoint(Target.transform.position);
        float targetHeight = Target.transform.position[1];
        float measuredHeight = Body.transform.position[1];
        float measuredRollDisplacement = localTarget[0];
        float measuredPitchDisplacement = localTarget[2];
        float measuredYaw = Mathf.Atan2(Body.transform.right.z, Body.transform.right.x);
        float measuredPitch = Mathf.Atan2(Body.transform.forward.y, Body.transform.forward.z);
        float measuredRoll = Mathf.Atan2(Body.transform.right.y, Body.transform.right.x);
        if(Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
            measuredYaw = -Mathf.Atan2(Body.transform.forward.x, Body.transform.forward.z);
        }
        double PositionControllerPitch = Clamp(DisplacementPitchController.ComputeOutput(0.0D, AddNoise(measuredPitchDisplacement, 0)), -0.2D, 0.2D);
        double PositionControllerRoll = Clamp(DisplacementRollController.ComputeOutput(0.0D, AddNoise(measuredRollDisplacement, 1)), -0.2D, 0.2D);
        double ThrustSignal = ThrustController.ComputeOutput(targetHeight, AddNoise(measuredHeight, 2));
        double YawSignal = YawController.ComputeOutput(0.0D, AddNoise(measuredYaw, 3));
        double PitchSignal = PitchController.ComputeOutput(PositionControllerPitch, AddNoise(measuredPitch, 4));
        double RollSignal = RollController.ComputeOutput(PositionControllerRoll, AddNoise(measuredRoll, 5));
        MotorSignals[0] = Clamp(ThrustSignal - YawSignal + PitchSignal - RollSignal, 0.0D, 1.0D);
        MotorSignals[1] = Clamp(ThrustSignal + YawSignal + PitchSignal + RollSignal, 0.0D, 1.0D);
        MotorSignals[2] = Clamp(ThrustSignal - YawSignal - PitchSignal + RollSignal, 0.0D, 1.0D);
        MotorSignals[3] = Clamp(ThrustSignal + YawSignal - PitchSignal - RollSignal, 0.0D, 1.0D);
        return MotorSignals;
    }

    void SendActions(double[] vectorAction) {
		double[] voltages = new double[4];
		for(int i = 0; i < 4; i++) {
			voltages[i] = MaxVoltage * vectorAction[i];
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
