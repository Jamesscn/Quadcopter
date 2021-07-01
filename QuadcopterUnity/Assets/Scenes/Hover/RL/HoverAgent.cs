using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class HoverAgent : RLAgent {

	public GameObject Target;

	public override void CollectObservations(VectorSensor sensor) {
		Vector3 differenceVector = Target.transform.position - Body.transform.position;
		float distance = differenceVector.magnitude;
		float maxDistance = 14.0F;
		float distancePower = 0.5F;
		float spinImportance = 0.02F;
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
		sensor.AddObservation(ComponentNormalize(Body.transform.up));
		sensor.AddObservation(ComponentNormalize(Body.velocity));
		sensor.AddObservation(ComponentNormalize(Body.angularVelocity));
		sensor.AddObservation(ComponentNormalize(differenceVector));
		sensor.AddObservation(Mathf.Max(0.0F, 1.0F - Mathf.Pow(distance / maxDistance, distancePower)));
		sensor.AddObservation(spinImportance * Body.angularVelocity.magnitude);
	}

	void FixedUpdate() {
		Vector3 differenceVector = Target.transform.position - Body.transform.position;
		float distance = differenceVector.magnitude;
		float maxDistance = 14.0F;
		float distancePower = 0.5F;
		float spinImportance = 0.02F;
		float distanceReward = Mathf.Max(0.0F, 1.0F - Mathf.Pow(distance / maxDistance, distancePower));
		float spinPenalty = -spinImportance * Body.angularVelocity.magnitude;
		AddReward((distanceReward - spinPenalty) / Mathf.Max(1, MaxStep));
		if(StepCount == MaxStep - 1 || distance > maxDistance) {
			Academy.Instance.StatsRecorder.Add("Final Distance", distance, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Speed", Body.velocity.magnitude, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Angular Speed", Body.angularVelocity.magnitude, StatAggregationMethod.Average);
			Academy.Instance.StatsRecorder.Add("Final Vertical Dot Product", Vector3.Dot(Body.transform.up, Vector3.up));
			EndEpisode();
		}
	}
	
}