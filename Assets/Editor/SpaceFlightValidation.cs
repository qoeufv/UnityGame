using System;
using StrategyRPG.Flight;
using UnityEditor;
using UnityEngine;

/// <summary>Runs synchronously in Play Mode, with temporary objects and no save changes.</summary>
public static class SpaceFlightValidation
{
    private static readonly Vector3 Origin = new Vector3(0f, -100f, 0f);
    private const float Dt = 0.02f;

    [MenuItem("Tools/Validate Space Flight")]
    public static void Validate()
    {
        if (!Application.isPlaying)
            throw new InvalidOperationException("Start Play Mode before validating space flight.");
        GameObject root = new GameObject("SpaceFlightValidation_Temporary");
        try
        {
            GameObject ship = new GameObject("TestShip");
            ship.transform.SetParent(root.transform);
            ship.transform.position = Origin;
            var flight = ship.AddComponent<SpaceFlightController>();
            flight.enabled = false; // Only explicit ticks, never keyboard polling.
            flight.boundaryCenter = Origin;
            flight.BoundRadius = 450f;
            Reset(flight);
            Advance(flight, 5f, 1f);
            Check(Near(flight.Speed, 18f) && !flight.Boosting, "cruise speed must settle at 18");
            Check(ship.transform.position.z > 20f, "throttle must move the ship");
            Reset(flight);
            Advance(flight, 5f, 1f, boost: true);
            Check(Near(flight.Speed, 42f) && flight.Boosting, "boost must settle at 42");
            Advance(flight, 2f, 1f);
            Check(Near(flight.Speed, 18f) && !flight.Boosting, "releasing boost must return to cruise");
            Advance(flight, 2f, 0f, brake: true);
            Check(Near(flight.Speed, 0f), "brake must stop the ship");
            Vector3 stopped = ship.transform.position;
            Advance(flight, 1f, 0f, brake: true);
            Check(Vector3.Distance(stopped, ship.transform.position) < 0.001f, "braked ship must remain still");
            Reset(flight);
            Advance(flight, 3f, -1f, boost: true);
            Check(flight.Speed < 0f && Mathf.Abs(flight.Speed) <= 18f && !flight.Boosting && ship.transform.position.z < -1f,
                "reverse must be slower than forward and cannot boost");
            Reset(flight);
            Advance(flight, 1f, 0f, yaw: 1f);
            Check(Vector3.Dot(ship.transform.forward, Vector3.right) > 0.5f, "yaw must turn the ship right");
            Reset(flight);
            Advance(flight, 0.5f, 0f, pitch: -1f);
            Check(ship.transform.forward.y > 0.2f, "upward pitch must lift ship direction");
            Reset(flight);
            Advance(flight, 0.5f, 0f, roll: 1f);
            Check(Mathf.Abs(ship.transform.right.y) > 0.2f, "roll must rotate the ship");
            Advance(flight, 4f, 0f);
            Check(Vector3.Dot(ship.transform.up, Vector3.up) > 0.99f, "neutral controls must stabilize roll");
            Reset(flight);
            Advance(flight, 1f, 1f);
            stopped = ship.transform.position;
            Quaternion orientation = ship.transform.rotation;
            flight.ControlsEnabled = false;
            Advance(flight, 1f, 1f, yaw: 1f, boost: true);
            Check(Near(flight.Speed, 0f) && !flight.Boosting && Vector3.Distance(stopped, ship.transform.position) < 0.001f &&
                Quaternion.Angle(orientation, ship.transform.rotation) < 0.001f, "disabled controls must stop translation and rotation");
            flight.ControlsEnabled = true;
            flight.ResetFlight(Origin, Quaternion.Euler(0f, 90f, 0f));
            Check(Near(flight.Speed, 0f) && !flight.AtBoundary && !flight.Boosting && Vector3.Distance(ship.transform.position, Origin) < 0.001f &&
                Vector3.Dot(ship.transform.forward, Vector3.right) > 0.99f, "reset must restore pose and clear motion");
            Check(ship.GetComponent<CharacterController>().enabled, "reset must restore character controller");

            var wall = new GameObject("ThinCollisionWall");
            wall.transform.SetParent(root.transform);
            wall.transform.position = Origin + Vector3.forward * 10f;
            var box = wall.AddComponent<BoxCollider>();
            box.size = new Vector3(20f, 20f, 0.2f);
            Physics.SyncTransforms();
            Reset(flight);
            for (int i = 0; i < 12; i++) flight.TickFlight(0.25f, 1f, 0f, 0f, 0f, true, false);
            Check(ship.transform.position.z > 1f && ship.transform.position.z < 10f && Mathf.Abs(flight.Speed) < 1f,
                "boost under long frames must not tunnel through a thin wall");
            box.enabled = false;

            flight.ResetFlight(Origin + Vector3.forward * 449f, Quaternion.identity);
            Advance(flight, 5f, 1f, boost: true);
            Check(flight.AtBoundary && Vector3.Distance(ship.transform.position, Origin) <= flight.BoundRadius + 0.01f,
                "outward boost must remain inside the exploration boundary");
            flight.ResetFlight(ship.transform.position, Quaternion.Euler(0f, 180f, 0f));
            Advance(flight, 3f, 1f);
            Check(!flight.AtBoundary && Vector3.Distance(ship.transform.position, Origin) < 430f,
                "boundary must permit turning and flying back inward");

            Reset(flight);
            wall.transform.position = Origin + Vector3.back * 5f;
            box.enabled = true;
            Physics.SyncTransforms();
            var cameraObject = new GameObject("TestFollowCamera");
            cameraObject.transform.SetParent(root.transform);
            var rig = cameraObject.AddComponent<SpaceFollowCamera>();
            rig.enabled = false;
            cameraObject.GetComponent<Camera>().enabled = false;
            rig.target = ship.transform;
            rig.ResetView();
            Check(cameraObject.transform.position.z > -4.9f && cameraObject.transform.position.z < -1f,
                "follow camera must stop in front of scenery without colliding with its own ship");
            box.enabled = false;
            Physics.SyncTransforms();
            rig.ResetView();
            Check(cameraObject.transform.position.z < -10f, "camera must return to its normal follow distance after obstruction removal");
            Debug.Log("SPACE_FLIGHT_VALIDATION_PASSED: cruise, boost, brake, reverse, yaw/pitch/roll, stabilization, disabled input, reset, thin-wall collision, boundary return, camera occlusion. Temporary objects only.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static void Reset(SpaceFlightController flight)
    {
        flight.ResetFlight(Origin, Quaternion.identity);
        Physics.SyncTransforms();
    }

    private static void Advance(SpaceFlightController flight, float seconds, float throttle,
        float yaw = 0f, float pitch = 0f, float roll = 0f, bool boost = false, bool brake = false)
    {
        int count = Mathf.RoundToInt(seconds / Dt);
        for (int i = 0; i < count; i++) flight.TickFlight(Dt, throttle, yaw, pitch, roll, boost, brake);
    }

    private static bool Near(float a, float b) => Mathf.Abs(a - b) < 0.02f;
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Space flight validation failed: " + message);
    }
}
