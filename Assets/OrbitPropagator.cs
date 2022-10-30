using UnityEngine;

public enum StepMode
{
    FixedStepSize,
    AdaptiveStepSize
}

public enum Integrator
{
    None,
    RK4,
    RKDP45
}

[ExecuteInEditMode]
public class OrbitPropagator : MonoBehaviour
{
    public double time;
    // [Tooltip("Whether the propagator is being run in fixed-step mode or in adaptive-step mode")]
    // public StepMode stepMode;
    [Tooltip("Intermediate integration steps are taken to ensure that the error never exceeds this value")]
    public double tolerance;
    [Tooltip("The number of seconds propagated")]
    public float stepSize;
    [Tooltip("The number of steps propagated")]
    public int steps;
    [SerializeField] private float timeScale;
    public Integrator integrator;
    public OrbitalBody referenceFrame;

    private OrbitalBody[] orbitalBodies;
    private OrbitData[] orbitData;
    private OrbitData[] virtualOrbitData;

    private void Start()
    {
        orbitalBodies = FindObjectsOfType<OrbitalBody>();

        orbitData = new OrbitData[orbitalBodies.Length];

        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            orbitData[i] = new OrbitData(orbitalBodies[i].cartesian, /* orbitalBodies[i].keplerian, */ i);
        }
    }

    private void Update()
    {
        DrawOrbit();

        if (!Application.isPlaying) { return; }

        time += timeScale * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!Application.isPlaying) { return; }

        orbitData = Propagate(orbitData, Time.fixedDeltaTime * timeScale);

        for (int j = 0; j < orbitData.Length; j++)
        {
            orbitalBodies[j].cartesian = orbitData[j].cartesian;
            // orbitalBodies[j].keplerian = orbitData[j].keplerian;
        }
    }

    private OrbitData[] Propagate(OrbitData[] orbitData, float time)
    {
        for (int j = 0; j < orbitData.Length; j++)
        {
            OrbitData orbitalBody = orbitData[j];
            orbitalBody.CalculateGravitationalAcceleration(orbitData);
            orbitalBody.AddForce(time, integrator);
            // if (timeScale < 2)
            // {
            //     orbitalBody.CartesianToKeplerian(orbitData[0].cartesian, orbitData[1].cartesian);
            // }
            orbitData[j] = orbitalBody;
        }
        return orbitData;
    }

    private void DrawOrbit()
    {
        orbitalBodies = FindObjectsOfType<OrbitalBody>();

        virtualOrbitData = new OrbitData[orbitalBodies.Length];
        Vector3[][] drawPoints = new Vector3[orbitalBodies.Length][];

        int referenceFrameIndex = 0;
        Vector3d referenceBodyInitialPosition = Vector3d.zero;

        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            virtualOrbitData[i] = new OrbitData(orbitalBodies[i].cartesian, /* orbitalBodies[i].keplerian, */ i);
            drawPoints[i] = new Vector3[steps];

            if (referenceFrame != null && orbitalBodies[i] == referenceFrame)
            {
                referenceFrameIndex = i;
                referenceBodyInitialPosition = virtualOrbitData[i].cartesian.position;
            }
        }

        for (int step = 0; step < steps; step++)
        {
            Vector3d referenceBodyPosition = (referenceFrame != null) ? virtualOrbitData[referenceFrameIndex].cartesian.position : Vector3d.zero;

            for (int i = 0; i < virtualOrbitData.Length; i++)
            {
                virtualOrbitData = Propagate(virtualOrbitData, stepSize);

                // virtualOrbitData[0].cartesian.velocity += virtualOrbitData[0].ApplyManeuver(maneuverData, timeScale);

                Vector3d nextPosition = virtualOrbitData[i].cartesian.position;
                if (referenceFrame != null)
                {
                    var referenceFrameOffset = referenceBodyPosition - referenceBodyInitialPosition;
                    nextPosition -= referenceFrameOffset;
                }
                if (referenceFrame != null && i == referenceFrameIndex)
                {
                    nextPosition = referenceBodyInitialPosition;
                }

                // drawPoints[i][step] = (Vector3)(nextPosition) / Constant.Scale;
                drawPoints[i][step] = (Vector3)(nextPosition);
            }
        }

        for (int bodyIndex = 0; bodyIndex < virtualOrbitData.Length; bodyIndex++)
        {
            var pathColour = Color.white;

            for (int i = 0; i < drawPoints[bodyIndex].Length - 1; i++)
            {
                Debug.DrawLine(drawPoints[bodyIndex][i], drawPoints[bodyIndex][i + 1], pathColour);
            }
        }
    }
}
