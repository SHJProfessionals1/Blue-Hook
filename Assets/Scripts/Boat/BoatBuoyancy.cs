using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class BoatBuoyancy : MonoBehaviour
{
	[SerializeField] Rigidbody rb;
	[SerializeField] Transform waterTransform;

	[SerializeField] float floatStrength = 20f;
	[SerializeField] float floatDamping = 6f;
	[SerializeField] float waterDrag = 2f;

	[SerializeField] Vector3 buoyancyOffset = Vector3.zero;

	float waterHeight;

	void Awake()
	{
		if (!rb)
			rb = GetComponent<Rigidbody>();

		if (!waterTransform)
			AutoAssignWater();
	}

	void FixedUpdate()
	{
		UpdateWaterHeight();
		ApplyBuoyancy();
		ApplyWaterDrag();
	}

	void AutoAssignWater()
	{
		GameObject water = GameObject.FindGameObjectWithTag("Water");
		if (water)
			waterTransform = water.transform;
	}

	void UpdateWaterHeight()
	{
		if (waterTransform)
			waterHeight = waterTransform.position.y;
	}

	void ApplyBuoyancy()
	{
		Vector3 forcePoint =
			rb.worldCenterOfMass +
			transform.TransformVector(buoyancyOffset);

		float depth = waterHeight - forcePoint.y;
		if (depth <= 0f)
			return;

		float upwardForce =
			depth * floatStrength -
			rb.linearVelocity.y * floatDamping;

		rb.AddForceAtPosition(
			Vector3.up * upwardForce,
			forcePoint,
			ForceMode.Acceleration
		);
	}

	void ApplyWaterDrag()
	{
		rb.linearVelocity *= 1f - waterDrag * Time.fixedDeltaTime;
	}

	void OnDrawGizmosSelected()
	{
		if (!rb) return;

		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(
			rb.worldCenterOfMass + transform.TransformVector(buoyancyOffset),
			0.1f
		);
	}
}
