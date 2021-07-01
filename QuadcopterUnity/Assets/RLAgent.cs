using UnityEngine;
using Unity.MLAgents;

/**
The RLAgent class is used mainly as a base class with helpful functions for the reinforcement learning agent scripts used for each of the tasks.
*/
public class RLAgent : Agent {

	public Rigidbody Body;
	public float MaxVoltage;

	float[] heuristicActions = {-1.0F, -1.0F, -1.0F, -1.0F};

	//The ComponentNormalize function takes in a 3D vector and returns a vector pointing in the same direction but with the largest component of that vector equal to one. This is a suggestion in the ML-Agents documentation which is supposed to increase the efficiency of training while using vectors.
	public Vector3 ComponentNormalize(Vector3 vec) {
		if(vec.magnitude == 0) {
			return vec;
		}
		return vec / Mathf.Max(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
	}

	//This function tells the Dynamics class to reset the simulation when a new episode begins.
	public override void OnEpisodeBegin() {
		SendMessage("ResetSimulation");
	}

	//This function receives four output values from -1 to 1 from the reinforcement learning agent. This is rescaled to be between 0 and the largest voltage allowed, then sent to the Dynamics class.
	public override void OnActionReceived(float[] vectorAction) {
		double[] voltages = new double[4];
		for(int i = 0; i < 4; i++) {
			voltages[i] = ScaleAction(vectorAction[i], 0, MaxVoltage);
		}
		SendMessage("SetVoltages", voltages);
	}

	//This function is used to control the quadcopter whenever the reinforcement learning agent is not active.
	public override void Heuristic(float[] actionsOut) {
		for(int i = 0; i < 4; i++) {
			actionsOut[i] = heuristicActions[i];
		}
	}

	//This function receives values from -1 to 1 from the Interface class and stores them so that they can be used by the Heuristic function.
	public void GetHeuristic(float[] actionsOut) {
		heuristicActions = actionsOut;
	}
	
}