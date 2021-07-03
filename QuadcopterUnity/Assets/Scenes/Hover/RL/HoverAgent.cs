using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class HoverAgent : RLAgent {

	public GameObject Target;

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
		sensor.AddObservation(measuredYaw);
		sensor.AddObservation(measuredPitch);
		sensor.AddObservation(measuredRoll);
		sensor.AddObservation(distance / maxDistance);
		sensor.AddObservation(Body.velocity.magnitude / maxDistance);
		sensor.AddObservation(Body.angularVelocity.magnitude / maxDistance);
		sensor.AddObservation(ComponentNormalize(differenceVector));
		sensor.AddObservation(ComponentNormalize(Body.velocity));
		sensor.AddObservation(ComponentNormalize(Body.angularVelocity));
	}

	void FixedUpdate() {
		Vector3 differenceVector = Target.transform.position - Body.transform.position;
		float distance = differenceVector.magnitude;
		float maxDistance = 10.0F;
		float distancePower = 0.5F;
		float spinImportance = 0.02F;
		float distanceReward = Mathf.Max(0.0F, 1.0F - Mathf.Pow(distance / maxDistance, distancePower));
		float spinPenalty = -spinImportance * Body.angularVelocity.magnitude;
		AddReward(Mathf.Max(0.0F, (distanceReward + spinPenalty)) / Mathf.Max(1.0F, MaxStep));
		if(StepCount == MaxStep - 1 || distance > maxDistance || Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
			float yaw = Mathf.Atan2(Body.transform.right.z, Body.transform.right.x);
			float pitch = Mathf.Atan2(Body.transform.forward.y, Body.transform.forward.z);
			float roll = Mathf.Atan2(Body.transform.right.y, Body.transform.right.x);
			if(Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
				yaw = -Mathf.Atan2(Body.transform.forward.x, Body.transform.forward.z);
			}
			Academy.Instance.StatsRecorder.Add("Final Distance", distance, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Speed", Body.velocity.magnitude, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Angular Speed", Body.angularVelocity.magnitude, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Yaw", yaw);
			Academy.Instance.StatsRecorder.Add("Final Pitch", pitch);
			Academy.Instance.StatsRecorder.Add("Final Roll", roll);
			EndEpisode();
		}
	}
	
}