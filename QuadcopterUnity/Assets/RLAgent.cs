using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RLAgent : Agent {

	public Rigidbody Body;
	public GameObject Target;

	float lastEpisodeReward = 0.0F;

	Vector3 ComponentNormalize(Vector3 vec) {
		if(vec.magnitude == 0) {
			return vec;
		}
		return vec / Mathf.Max(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
	}

	public override void OnEpisodeBegin() {
		if(lastEpisodeReward != 0.0F) {
			Debug.Log("Episode Reward: " + lastEpisodeReward);
		}
		gameObject.SendMessage("ResetSimulation");
		lastEpisodeReward = 0.0F;
	}

	public override void Heuristic(float[] actionsOut) {
		gameObject.SendMessage("UpdateController");
		for(int i = 0; i < 4; i++) {
			actionsOut[i] = Controller.MotorSignals[i];
		}
	}

	public override void OnActionReceived(float[] vectorAction) {
		double[] voltages = new double[4];
		double maxVoltage = 45.0D;
		for(int i = 0; i < 4; i++) {
			voltages[i] = vectorAction[i] * maxVoltage;
		}
		gameObject.SendMessage("SetVoltages", voltages);
	}

	public override void CollectObservations(VectorSensor sensor) {
		Vector3 differenceVector = Target.transform.position - Body.transform.position;
		float distance = differenceVector.magnitude;
		float maxDistance = 10.5F;
		float distancePower = 0.5F;
		float measuredPitch = Body.transform.rotation.eulerAngles[0];
		float measuredYaw = Body.transform.rotation.eulerAngles[1];
		float measuredRoll = Body.transform.rotation.eulerAngles[2];
		if(measuredPitch > 180.0F) {
			measuredPitch -= 360.0F;
		}
		if(measuredYaw > 180.0F) {
			measuredYaw -= 360.0F;
		}
		if(measuredRoll > 180.0F) {
			measuredRoll -= 360.0F;
		}
		measuredPitch /= 180.0F;
		measuredYaw /= 180.0F;
		measuredRoll /= 180.0F;
		sensor.AddObservation(measuredPitch);
		sensor.AddObservation(measuredYaw);
		sensor.AddObservation(measuredRoll);
		sensor.AddObservation(ComponentNormalize(Body.transform.up));
		sensor.AddObservation(ComponentNormalize(Body.velocity));
		sensor.AddObservation(ComponentNormalize(Body.angularVelocity));
		sensor.AddObservation(ComponentNormalize(differenceVector));
		sensor.AddObservation(Mathf.Max(0.0F, 1.0F - Mathf.Pow(distance / maxDistance, distancePower)));
	}

	void FixedUpdate() {
		Vector3 differenceVector = Target.transform.position - Body.transform.position;
		float distance = differenceVector.magnitude;
		float maxDistance = 10.5F;
		float distancePower = 0.5F;
		float reward = Mathf.Max(0.0F, 1.0F - Mathf.Pow(distance / maxDistance, distancePower));

		if(distance > 2 * maxDistance) {
			EndEpisode();
		} else {
			AddReward(0.01F * reward);
		}
		lastEpisodeReward = GetCumulativeReward();
	}
	
}
