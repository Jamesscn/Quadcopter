using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class PathAgent : RLAgent {

	public GameObject Target;
    public bool ShowPath;
	public bool ShowTrajectory;
	
	Vector3[] Locations = new Vector3[500];
	float a = 1.0F;
    float b = 0.0F;
    float c = 0.0F;

	//Overrides the episode begin in RLAgent.cs
	public override void OnEpisodeBegin() {
		a = UnityEngine.Random.Range(0.0F, Mathf.PI / 2.0F);
		b = UnityEngine.Random.Range(Mathf.PI / 4.0F, 3.0F * Mathf.PI / 4.0F);
		c = UnityEngine.Random.Range(-1.0F, 1.0F);
		Body.transform.localRotation = Quaternion.identity;
        Body.transform.localPosition = PathFunction(0.0F);
		SendMessage("ResetSimulation");
	}

	public override void CollectObservations(VectorSensor sensor) {
		Vector3 differenceVector = Target.transform.position - Body.transform.position;
		float distance = differenceVector.magnitude;
		float maxDistance = 10.0F;
		float measuredYaw = Mathf.Atan2(Body.transform.right.z, Body.transform.right.x);
        float measuredPitch = Mathf.Atan2(Body.transform.forward.y, Body.transform.forward.z);
        float measuredRoll = Mathf.Atan2(Body.transform.right.y, Body.transform.right.x);
        if(Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
            measuredYaw = -Mathf.Atan2(Body.transform.forward.x, Body.transform.forward.z);
        }
		measuredYaw /= Mathf.PI;
		measuredPitch /= Mathf.PI;
		measuredRoll /= Mathf.PI;

        float firstTime = (float)StepCount / MaxStep;
        float secondTime = (float)(StepCount + 1) / MaxStep;
		float thirdTime = (float)(StepCount + 2) / MaxStep;
        Vector3 firstPos = PathFunction(firstTime);
        Vector3 secondPos = PathFunction(secondTime);
		Vector3 thirdPos = PathFunction(thirdTime);
        Vector3 targetVelocity = secondPos - firstPos;
		Vector3 targetAcceleration = thirdPos - 2 * secondPos + firstPos;

		sensor.AddObservation(measuredYaw);
		sensor.AddObservation(measuredPitch);
		sensor.AddObservation(measuredRoll);
		sensor.AddObservation(distance / maxDistance);
		sensor.AddObservation(Body.velocity.magnitude / maxDistance);
		sensor.AddObservation(Body.angularVelocity.magnitude / maxDistance);
		sensor.AddObservation(ComponentNormalise(differenceVector));
		sensor.AddObservation(ComponentNormalise(Body.velocity));
		sensor.AddObservation(ComponentNormalise(Body.angularVelocity));
        sensor.AddObservation(ComponentNormalise(targetVelocity));
		sensor.AddObservation(ComponentNormalise(targetAcceleration));
	}

	void FixedUpdate() {
        Target.transform.localPosition = PathFunction((float)(StepCount % 500) / MaxStep);
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
		AddReward(Mathf.Exp(- 0.4F * Mathf.Pow(distance, 0.8F)
                            - 1.0F * Mathf.Pow(Mathf.Abs(yaw), 1.4F)
                            - 0.8F * Mathf.Pow(angularSpeed, 1.2F)) / Mathf.Max(1.0F, MaxStep));
		if(StepCount == MaxStep - 1 || Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
			Academy.Instance.StatsRecorder.Add("Final Distance", distance, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Speed", speed, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Angular Speed", angularSpeed, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Yaw", yaw);
			Academy.Instance.StatsRecorder.Add("Final Pitch", pitch);
			Academy.Instance.StatsRecorder.Add("Final Roll", roll);
			EndEpisode();
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