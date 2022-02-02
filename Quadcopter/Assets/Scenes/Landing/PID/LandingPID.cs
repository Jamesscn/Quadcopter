using System;
using UnityEngine;
using static StaticFunctions;

public class LandingPID : MonoBehaviour {

    public Rigidbody Body;
    public GameObject Ground;
    public int MaxStep;
    public double MaxVoltage;
    public double NoiseStrength;
    public bool Training;
    public bool ShowTrajectory;

    bool SentEndSignal;
    int TrainingIndex;
    int StepCount = 0;
    float Reward = 0.0F;
    bool PIDInitialized = false;
    bool StopMovement = false;
	Vector3[] Locations = new Vector3[500];

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
                SetPIDConstants(new double[]{0.20378D, 0.00000D, 0.48254D, 0.02886D, 0.00000D, 0.03194D, 0.17312D, 0.00000D, 0.18296D});
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
        StepCount = 0;
        SentEndSignal = false;
    }

    double[] Controller() {
        double[] MotorSignals = {0.0D, 0.0D, 0.0D, 0.0D};
        Vector3 localTarget = Body.transform.InverseTransformPoint(Ground.transform.position);
        float targetHeight = Ground.transform.position[1];
        float measuredHeight = Body.transform.position[1];
        float measuredRollDisplacement = localTarget[0];
        float measuredPitchDisplacement = localTarget[2];
        float measuredYaw = Mathf.Atan2(Body.transform.right.z, Body.transform.right.x);
        float measuredPitch = Mathf.Atan2(Body.transform.forward.y, Body.transform.forward.z);
        float measuredRoll = Mathf.Atan2(Body.transform.right.y, Body.transform.right.x);
        if(Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
            measuredYaw = -Mathf.Atan2(Body.transform.forward.x, Body.transform.forward.z);
        }
        double PositionControllerPitch = Clamp(DisplacementPitchController.ComputeOutput(0.0D, AddNoise(measuredPitchDisplacement, NoiseStrength, 0)), -0.2D, 0.2D);
        double PositionControllerRoll = Clamp(DisplacementRollController.ComputeOutput(0.0D, AddNoise(measuredRollDisplacement, NoiseStrength, 1)), -0.2D, 0.2D);
        double ThrustSignal = ThrustController.ComputeOutput(targetHeight, AddNoise(measuredHeight, NoiseStrength, 2));
        double YawSignal = YawController.ComputeOutput(0.0D, AddNoise(measuredYaw, NoiseStrength, 3));
        double PitchSignal = PitchController.ComputeOutput(PositionControllerPitch, AddNoise(measuredPitch, NoiseStrength, 4));
        double RollSignal = RollController.ComputeOutput(PositionControllerRoll, AddNoise(measuredRoll, NoiseStrength, 5));
        MotorSignals[0] = Clamp(ThrustSignal - YawSignal + PitchSignal - RollSignal, 0.0D, 1.0D);
        MotorSignals[1] = Clamp(ThrustSignal + YawSignal + PitchSignal + RollSignal, 0.0D, 1.0D);
        MotorSignals[2] = Clamp(ThrustSignal - YawSignal - PitchSignal + RollSignal, 0.0D, 1.0D);
        MotorSignals[3] = Clamp(ThrustSignal + YawSignal - PitchSignal - RollSignal, 0.0D, 1.0D);
        return MotorSignals;
    }

    void SendActions(double[] vectorAction) {
		double[] voltages = new double[4];
		for(int i = 0; i < 4; i++) {
            if(StopMovement) {
                voltages[i] = 0.0D;
            } else {
			    voltages[i] = MaxVoltage * vectorAction[i];
            }
		}
		SendMessage("SetVoltages", voltages);
	}

    public void FixedUpdate() {
        SendActions(Controller());
        StepCount += 1;
        Vector3 differenceVector = Ground.transform.position - Body.transform.position;
        float yaw = Mathf.Atan2(Body.transform.right.z, Body.transform.right.x);
        float pitch = Mathf.Atan2(Body.transform.forward.y, Body.transform.forward.z);
        float roll = Mathf.Atan2(Body.transform.right.y, Body.transform.right.x);
        if(Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
            yaw = -Mathf.Atan2(Body.transform.forward.x, Body.transform.forward.z);
        }
		float distance = differenceVector.magnitude;
        float speed = Body.velocity.magnitude;
        float angularSpeed = Body.angularVelocity.magnitude;
		float DeltaReward = Mathf.Exp(- 0.4F * Mathf.Pow(distance, 0.8F)
                            - 1.0F * Mathf.Pow(Mathf.Abs(yaw), 1.4F)
                            - 0.8F * Mathf.Pow(angularSpeed, 1.2F)
                            - 1.1F * Mathf.Pow(speed, 0.7F)) / Mathf.Max(1.0F, MaxStep);
        Reward += DeltaReward;
        if((StepCount == MaxStep - 1 || differenceVector.y > 0) && Training && !SentEndSignal) {
            Reward += (MaxStep - StepCount - 1) * DeltaReward;
            float[] endValues = {Reward, distance, speed, angularSpeed, yaw, pitch, roll};
            Tuple<int, float[]> endData = new Tuple<int, float[]>(TrainingIndex, endValues);
            SendMessageUpwards("EpisodeEnded", endData);
            SentEndSignal = true;
        }
    }

    public void Collision() {
        if(!Training) {
            StopMovement = true;
        }
    }

    void OnDrawGizmos() {
		/*
        Locations[StepCount - 1] = Body.transform.position;
        if(ShowTrajectory && StepCount > 1) {
            Gizmos.color = Color.black;
            for(int i = 1; i < StepCount; i++) {
                Gizmos.DrawRay(Locations[i - 1], Locations[i] - Locations[i - 1]);
            }
        }
        */
	}

}
