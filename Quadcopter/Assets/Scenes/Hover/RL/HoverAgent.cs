using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class HoverAgent : RLAgent {

	public GameObject Target;
	public bool ShowTrajectory;

	Vector3[] Locations = new Vector3[500];

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
		sensor.AddObservation(ComponentNormalise(differenceVector));
		sensor.AddObservation(ComponentNormalise(Body.velocity));
		sensor.AddObservation(ComponentNormalise(Body.angularVelocity));
	}

	void FixedUpdate() {
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
            }
        }
        */
	}
	
}