using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class LandingAgent : RLAgent {

	public GameObject Floor;

    public override void CollectObservations(VectorSensor sensor) {
		
    }

    void FixedUpdate() {

    }

    void OnCollisionEnter(Collision collision) {
        Vector3 differenceVector = Floor.transform.position - Body.transform.position;
        float distance = differenceVector.magnitude;
		float maxDistance = 14.0F;
		float distancePower = 0.5F;
        float crashImportance = 0.2F;
        float angleImportance = 0.1F;
        float distanceReward = Mathf.Max(0.0F, 1.0F - Mathf.Pow(distance / maxDistance, distancePower));
        //Test it squared
        float crashPenalty = -crashImportance * collision.relativeVelocity.magnitude;
        float anglePenalty = angleImportance * Vector3.Dot(Body.transform.up, Vector3.up);
        SetReward(distanceReward - crashPenalty - anglePenalty);
        Academy.Instance.StatsRecorder.Add("Final Distance", distance, StatAggregationMethod.Average);
        Academy.Instance.StatsRecorder.Add("Final Speed", collision.relativeVelocity.magnitude, StatAggregationMethod.Average);
        Academy.Instance.StatsRecorder.Add("Final Vertical Dot Product", Vector3.Dot(Body.transform.up, Vector3.up), StatAggregationMethod.Average);
        EndEpisode();
    }
}
