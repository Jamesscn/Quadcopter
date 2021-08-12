using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class PathAgent : RLAgent {

	public GameObject Target;
    public double LoopsPerIteration;
    public bool ShowPath;

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

        float currTime = 2 * Mathf.PI * (float)LoopsPerIteration * StepCount / MaxStep;
        float nextTime = 2 * Mathf.PI * (float)LoopsPerIteration * (StepCount + 1) / MaxStep;
        Vector3 currPos = 5.0F * new Vector3(Mathf.Sin(currTime), Mathf.Sin(currTime), Mathf.Sin(currTime) * Mathf.Cos(currTime));
        Vector3 nextPos = 5.0F * new Vector3(Mathf.Sin(nextTime), Mathf.Sin(nextTime), Mathf.Sin(nextTime) * Mathf.Cos(nextTime));
        Vector3 targetVelocity = nextPos - currPos;

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
	}

	void FixedUpdate() {
        float time = 2 * Mathf.PI * (float)LoopsPerIteration * StepCount / MaxStep;
        Target.transform.localPosition = 5.0F * new Vector3(Mathf.Sin(time), Mathf.Sin(time), Mathf.Sin(time) * Mathf.Cos(time));

		Vector3 differenceVector = Target.transform.position - Body.transform.position;
		float yaw = Mathf.Atan2(Body.transform.right.z, Body.transform.right.x);
		float pitch = Mathf.Atan2(Body.transform.forward.y, Body.transform.forward.z);
		float roll = Mathf.Atan2(Body.transform.right.y, Body.transform.right.x);
		if(Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
			yaw = -Mathf.Atan2(Body.transform.forward.x, Body.transform.forward.z);
		}
		float distance = differenceVector.magnitude;
		float maxDistance = 10.0F;
		float distancePower = 0.5F;
		float spinImportance = 0.02F;
		float angleImportance = 0.02F;
		float distanceReward = Mathf.Max(0.0F, 1.0F - Mathf.Pow(distance / maxDistance, distancePower));
		float spinPenalty = -spinImportance * Body.angularVelocity.magnitude;
		float anglePenalty = -angleImportance * Mathf.Abs(yaw);
		AddReward(Mathf.Max(0.0F, (distanceReward + spinPenalty + anglePenalty)) / Mathf.Max(1.0F, MaxStep));
		if(StepCount == MaxStep - 1 || distance > maxDistance || Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
			Academy.Instance.StatsRecorder.Add("Final Distance", distance, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Speed", Body.velocity.magnitude, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Angular Speed", Body.angularVelocity.magnitude, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Yaw", yaw);
			Academy.Instance.StatsRecorder.Add("Final Pitch", pitch);
			Academy.Instance.StatsRecorder.Add("Final Roll", roll);
			EndEpisode();
		}
	}

    void OnDrawGizmos() {
        if(ShowPath) {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Body.transform.position, Target.transform.position - Body.transform.position);
            Gizmos.color = Color.black;
            int DrawSteps = 100;
            for(int i = 0; i < DrawSteps; i++) {
                float currTime = 2 * Mathf.PI * (float)LoopsPerIteration * i / DrawSteps;
                float nextTime = 2 * Mathf.PI * (float)LoopsPerIteration * (i + 1) / DrawSteps;
                Vector3 currPos = 5.0F * new Vector3(Mathf.Sin(currTime), Mathf.Sin(currTime), Mathf.Sin(currTime) * Mathf.Cos(currTime));
                Vector3 nextPos = 5.0F * new Vector3(Mathf.Sin(nextTime), Mathf.Sin(nextTime), Mathf.Sin(nextTime) * Mathf.Cos(nextTime));
                Gizmos.DrawRay(currPos, nextPos - currPos);
            }
        }
	}
	
}