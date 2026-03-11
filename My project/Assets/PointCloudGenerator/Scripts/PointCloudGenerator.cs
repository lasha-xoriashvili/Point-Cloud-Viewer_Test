using UnityEngine;
using UnityEngine.VFX;

public class PointCloudGenerator : MonoBehaviour
{
    public Material renderMaterial;
    public ComputeShader computeShader;
    public int pointCount = 100000;
    public float TransitionSpeed;
    public float Size;
    public Color col;
    public float Scale;

    private ComputeBuffer pointBuffer;
    private int kernel;
    struct PointData
    {
        public Vector3 pos;
        public Color color;
    }

    void Start()
    {
        // 1. ვქმნით ბუფერს (3 float = 12 ბაიტი თითო წერტილზე)
        int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(PointData));
        pointBuffer = new ComputeBuffer(pointCount, stride);
        kernel = computeShader.FindKernel("CSMain");
        // 2. ვავსებთ საწყისი მონაცემებით
        PointData[] initialPositions = new PointData[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            initialPositions[i].pos = Random.insideUnitSphere * Scale;
            initialPositions[i].color = col;
        }
        pointBuffer.SetData(initialPositions);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            DrawSpero();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            DrawCube();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            DrawPiramide();
        }

        //computeShader.SetFloat("TransitionSpeed", TransitionSpeed);
       // computeShader.SetBuffer(kernel, "targetBuffer", targetBuffer);
        computeShader.SetBuffer(kernel, "_PointBuffer", pointBuffer);

        // ჩვენ ვიყენებთ Math.Ceil, რომ დავრწმუნდეთ, რომ ყველა წერტილს მივწვდებით
        int groups = Mathf.CeilToInt(pointCount / 64f);

        // თუ groups მაინც დიდია, დავყოთ ის X და Y ღერძებზე
        if (groups > 65535)
        {
            int groupsX = 1024;
            int groupsY = Mathf.CeilToInt(groups / 1024f);
            computeShader.Dispatch(kernel, groupsX, groupsY, 1);
        }
        else
        {
            computeShader.Dispatch(kernel, groups, 1, 1);
        }
    }

    void DrawSpero()
    {
        PointData[] targets = new PointData[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            targets[i].pos = Random.insideUnitSphere * Scale;
            targets[i].color = col;
        }
        pointBuffer.SetData(targets);
    }

    void DrawCube()
    {
        PointData[] targets = new PointData[pointCount];
        float side = Mathf.Pow(pointCount, 1f / 3f);
        for (int i = 0; i < pointCount; i++)
        {
            float x = (i % side) / side - 0.5f;
            float y = ((i / side) % side) / side - 0.5f;
            float z = (i / (side * side)) / side - 0.5f;
            targets[i].pos = new Vector3(x, y, z) * Scale;
            targets[i].color = col;
        }

        pointBuffer.SetData(targets);
    }

    void DrawPiramide()
    {
        PointData[] targets = new PointData[pointCount];
        float side = Mathf.Pow(pointCount, 1f / 3f);
        float spacing = 0.5f;


        for (int i = 0; i < pointCount; i++)
        {
            float x = (i % side) / side - spacing;
            float y = ((i / side) % side) / side - spacing;
            float z = (i / (side * side)) / side - spacing;

            float shrink = (1.0f - spacing) - y;

            float finalyX = x * shrink;
            float finalyY = y;
            float finalyZ = z * shrink;
            targets[i].pos = new Vector3(finalyX, finalyY, finalyZ) * Scale;
            targets[i].color = col;
        }
        pointBuffer.SetData(targets);
    }

    void OnRenderObject()
    {
        if (pointBuffer == null) return;

        renderMaterial.SetBuffer("_PointBuffer", pointBuffer);
        renderMaterial.SetFloat("_PointSize", Size);

        renderMaterial.SetPass(0);

        renderMaterial.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);

        Graphics.DrawProceduralNow(MeshTopology.Points, pointCount);
    }

    void OnDestroy()
    {
     //   targetBuffer?.Release();
        pointBuffer?.Release();
    }
}
