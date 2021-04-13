using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RLAgent : Agent {

	//The following variable holds the body of the quadcopter.
	public GameObject Body;

	//The following variable holds the target position to be reached by the agent.
	public GameObject Target;

	//This function is called whenever a new episode begins. It resets the simulation.
	public override void OnEpisodeBegin() {
		gameObject.SendMessage("ResetSimulation");
	}

	//The following function allows the user to control the quadcopter when not being trained.
	//The four voltages across the motors are determined by the thrust, yaw, pitch and roll sliders.
	public override void Heuristic(float[] actionsOut) {
		actionsOut[0] = UserInterface.Thrust - UserInterface.Yaw + UserInterface.Pitch - UserInterface.Roll;
		actionsOut[1] = UserInterface.Thrust + UserInterface.Yaw + UserInterface.Pitch + UserInterface.Roll;
		actionsOut[2] = UserInterface.Thrust - UserInterface.Yaw - UserInterface.Pitch + UserInterface.Roll;
		actionsOut[3] = UserInterface.Thrust + UserInterface.Yaw - UserInterface.Pitch - UserInterface.Roll;
	}

	//The following function processes the outputs of the reinforcement learning agent.
	//In this case, it sends the output voltages to the quadcopter code.
	public override void OnActionReceived(float[] vectorAction) {
		float[] voltages = vectorAction;
		for(int i = 0; i < 4; i++) {
			voltages[i] = (voltages[i] + 1.0F) * 3.0F;
		}
		gameObject.SendMessage("SetVoltages", voltages);
	}

	//This function handles the inputs to the reinforcement learning agent.
	//In this case, it is the position of the quadcopter and the position of the target.
	public override void CollectObservations(VectorSensor sensor) {
		sensor.AddObservation(Body.transform.position);
		sensor.AddObservation(Body.transform.rotation);
		sensor.AddObservation(Body.GetComponent<Rigidbody>().velocity);
		sensor.AddObservation(Body.GetComponent<Rigidbody>().angularVelocity);
		sensor.AddObservation(Target.transform.position);
	}

	//The following function is called every time the physics engine updates.
	//The function sets a reward of -1 when the quadcopter is upside down, and +1 when it gets close to the target, then ends the episode.
	void FixedUpdate() {
		if(Vector3.Dot(Body.transform.up, Vector3.up) < 0) {
			SetReward(-1.0F);
			EndEpisode();
		}
		if(Vector3.Distance(Body.transform.position, Target.transform.position) < 1.5F) {
			SetReward(1.0F);
			EndEpisode();
		}
	}
	
}
