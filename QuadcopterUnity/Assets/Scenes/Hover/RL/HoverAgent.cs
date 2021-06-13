using UnityEngine;
using Unity.MLAgents.Sensors;

public class HoverAgent : RLAgent {

	public override void CollectObservations(VectorSensor sensor) {
		Vector3 differenceVector = Target.transform.position - Body.transform.position;
		float distance = differenceVector.magnitude;
		float maxDistance = 8.0F;
		float distancePower = 0.5F;
		float spinImportance = 0.02F;
		float measuredYaw = Body.transform.rotation.eulerAngles[1];
        float measuredPitch = Body.transform.rotation.eulerAngles[0];
        float measuredRoll = Body.transform.rotation.eulerAngles[2];
        if(measuredYaw > 180.0F) {
            measuredYaw -= 360.0F;
        }
        if(measuredPitch > 180.0F) {
            measuredPitch -= 360.0F;
        }
        if(measuredRoll > 180.0F) {
            measuredRoll -= 360.0F;
        }
		measuredYaw /= 180.0F;
		measuredPitch /= 180.0F;
		measuredRoll /= 180.0F;
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
		float maxDistance = 8.0F;
		float distancePower = 0.5F;
		float spinImportance = 0.02F;
		float reward = Mathf.Max(0.0F, 1.0F - Mathf.Pow(distance / maxDistance, distancePower)) - spinImportance * Body.angularVelocity.magnitude;
		if(distance > 1.5F * maxDistance) {
			EndEpisode();
		} else {
			AddReward(reward / Mathf.Max(1, MaxStep));
		}
		//add useful metrics such as distance, speed, orientation and angular velocity
		//probably best to ad these at the end
		//Academy.Instance.StatsRecorder.Add();
	}
	
}
