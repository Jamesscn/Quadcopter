using System;
using UnityEngine;
using static StaticFunctions;

public class PathPID : MonoBehaviour {

    public Rigidbody Body;
    public GameObject Target;
    public int MaxStep;
    public double MaxVoltage;
    public double NoiseStrength;
    public bool ShowPath;
    public bool ShowTrajectory;
    public bool Training;

    bool SentEndSignal;
    int TrainingIndex;
    int StepCount = 0;
    float Reward = 0.0F;
    bool PIDInitialized = false;
    float a = 1.0F;
    float b = 0.0F;
    float c = 0.0F;
	Vector3[] Locations = new Vector3[500];

    PID ThrustController;
    PID YawController;
    PID PitchController;
    PID RollController;
    PID DisplacementPitchController;
    PID DisplacementRollController;

    public void Start() {
        if(!PIDInitialized) {
            a = UnityEngine.Random.Range(0.0F, Mathf.PI / 2.0F);
            b = UnityEngine.Random.Range(Mathf.PI / 4.0F, 3.0F * Mathf.PI / 4.0F);
            c = UnityEngine.Random.Range(-1.0F, 1.0F);
            SetPIDConstants(new double[]{0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D});
            if(!Training) {
                SetPIDConstants(new double[]{0.42898D, 0.27482D, 0.20639D, 0.11335D, 0.01156D, 0.04488D, 0.05784D, 0.01122D, 0.28169D});
            }
            PIDInitialized = true;
            Body.transform.localRotation = Quaternion.identity;
            Body.transform.localPosition = PathFunction(0.0F);
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
        Body.transform.localRotation = Quaternion.identity;
        Body.transform.localPosition = PathFunction(0.0F);
        SendMessage("ResetSimulation");
        Reward = 0.0F;
        StepCount = 0;
        SentEndSignal = false;
        a = UnityEngine.Random.Range(0.0F, Mathf.PI / 2.0F);
        b = UnityEngine.Random.Range(-1.0F, 1.0F);
        c = UnityEngine.Random.Range(Mathf.PI / 4.0F, 3.0F * Mathf.PI / 4.0F);
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
			voltages[i] = MaxVoltage * vectorAction[i];
		}
		SendMessage("SetVoltages", voltages);
	}

    public void FixedUpdate() {
        StepCount += 1;
        Target.transform.localPosition = PathFunction((float)(StepCount % 500) / MaxStep);
        SendActions(Controller());
        Vector3 differenceVector = Target.transform.position - Body.transform.position;
        float yaw = Mathf.Atan2(Body.transform.right.z, Body.transform.right.x);
        float pitch = Mathf.Atan2(Body.transform.forward.y, Body.transform.forward.z);
        float roll = Mathf.Atan2(Body.transform.right.y, Body.transform.right.x);
        if(Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
            yaw = -Mathf.Atan2(Body.transform.forward.x, Body.transform.forward.z);
        }
		float distance = differenceVector.magnitude;
        float speed = Body.velocity.magnitude;
        float angularSpeed = Body.angularVelocity.magnitude;
		Reward += Mathf.Exp(- 0.4F * Mathf.Pow(distance, 0.8F)
                            - 1.0F * Mathf.Pow(Mathf.Abs(yaw), 1.4F)
                            - 0.8F * Mathf.Pow(angularSpeed, 1.2F)) / Mathf.Max(1.0F, MaxStep);
        if(StepCount == MaxStep - 1 && Training && !SentEndSignal) {
            float[] endValues = {Reward, distance, speed, angularSpeed, yaw, pitch, roll};
            Tuple<int, float[]> endData = new Tuple<int, float[]>(TrainingIndex, endValues);
            SendMessageUpwards("EpisodeEnded", endData);
            SentEndSignal = true;
        }
    }

    void OnDrawGizmos() {
        /*
        Locations[StepCount - 1] = Body.transform.position;
        if(ShowTrajectory && StepCount > 1) {
            Gizmos.color = Color.black;
            for(int i = 1; i < StepCount; i++) {
                Gizmos.DrawRay(Locations[i - 1], Locations[i] - Locations[i - 1]);
                if(i % 10 == 0) {
                    float time = (float)i / MaxStep;
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(Locations[i], transform.position + PathFunction(time) - Locations[i]);
                    Gizmos.color = Color.black;
                }
            }
        }
        */
        if(ShowPath) {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(Body.transform.position, Target.transform.position - Body.transform.position);
            Gizmos.color = Color.red;
            int DrawSteps = 100;
            for(int i = 0; i < DrawSteps; i++) {
                float currTime = (float)i / DrawSteps;
                float nextTime = (float)(i + 1) / DrawSteps;
                Vector3 currPos = transform.position + PathFunction(currTime);
                Vector3 nextPos = transform.position + PathFunction(nextTime);
                Gizmos.DrawRay(currPos, nextPos - currPos);
            }
        }
	}

    public Vector3 PathFunction(float time) {
        time = time - (int)time;
        return 5.0F * new Vector3(
            Mathf.Cos(2 * Mathf.PI * time + a),
            b * Mathf.Min(time, 1 - time),
            Mathf.Cos(2 * Mathf.PI * time) * Mathf.Cos(2 * Mathf.PI * time + c));
    }

}
