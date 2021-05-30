using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RLAgent : Agent {

	public Rigidbody Body;
	public GameObject Target;

	public override void OnEpisodeBegin() {
		gameObject.SendMessage("ResetSimulation");
	}

	public override void Heuristic(float[] actionsOut) {
		actionsOut[0] = UserInterface.Thrust - UserInterface.Yaw + UserInterface.Pitch - UserInterface.Roll;
		actionsOut[1] = UserInterface.Thrust + UserInterface.Yaw + UserInterface.Pitch + UserInterface.Roll;
		actionsOut[2] = UserInterface.Thrust - UserInterface.Yaw - UserInterface.Pitch + UserInterface.Roll;
		actionsOut[3] = UserInterface.Thrust + UserInterface.Yaw - UserInterface.Pitch - UserInterface.Roll;
	}

	public override void OnActionReceived(float[] vectorAction) {
		double[] voltages = new double[4];
		for(int i = 0; i < 4; i++) {
			voltages[i] = (vectorAction[i] + 1.0F) * 45.0F / 2.0F;
		}
		gameObject.SendMessage("SetVoltages", voltages);
	}

	public override void CollectObservations(VectorSensor sensor) {
		Vector3 differenceVector = Target.transform.position - Body.transform.position;
		sensor.AddObservation(differenceVector.normalized);
		sensor.AddObservation(Body.transform.up.normalized);
		sensor.AddObservation(Body.velocity.normalized);
		sensor.AddObservation(Vector3.Dot(Body.transform.up, Vector3.up));

		float distancePower = 0.9F;
		float distance = Vector3.Distance(Body.transform.position, Target.transform.position);
		float maxDistance = Mathf.Sqrt(6 * 6 + 5 * 5 + 5 * 5);
		float distanceReward = 1.0F - Mathf.Pow(distance / maxDistance, distancePower);
		sensor.AddObservation(distanceReward * 0.01F);
	}

	void FixedUpdate() {
		float distancePower = 0.9F;
		float distance = Vector3.Distance(Body.transform.position, Target.transform.position);
		float maxDistance = Mathf.Sqrt(6 * 6 + 5 * 5 + 5 * 5);
		float distanceReward = 1.0F - Mathf.Pow(distance / maxDistance, distancePower);
		float leanAngle = Vector3.Dot(Body.transform.up, Vector3.up);
		if(leanAngle < 0) {
			SetReward(-1.0F);
			EndEpisode();
		} else {
			SetReward(distanceReward * 0.01F);
		}
	}
	
}
