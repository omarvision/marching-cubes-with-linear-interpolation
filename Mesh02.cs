using System.Collections.Generic;
using UnityEngine;

public class Mesh02 : MonoBehaviour
{
    public Vector3 GridSize = new Vector3(3, 3, 3);
    public float Zoom = 1f;
    public float SurfaceLevel = 0.5f;
    public Material material = null;    
    private GridPoint[,,] p = null;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uv = new List<Vector2>();
    private GridCell cell = new GridCell();

    private void Start()
    {
        InitGrid();
        BuildMesh();
    }
    private void InitGrid()
    {
        p = new GridPoint[(int)GridSize.x + 1, (int)GridSize.y + 1, (int)GridSize.z + 1];

        for (int z = 0; z <= GridSize.z; z++)
        {
            for (int y = 0; y <= GridSize.y; y++)
            {
                for (int x = 0; x <= GridSize.x; x++)
                {
                    float nx = Zoom * (x / GridSize.x);
                    float ny = Zoom * (y / GridSize.y);
                    float nz = Zoom * (z / GridSize.z);
                    p[x, y, z] = new GridPoint();
                    p[x, y, z].Position = new Vector3(x, y, z);
                    p[x, y, z].Value = MarchingCube.Perlin3D(nx, ny, nz);
                }
            }
        }
    }
    private void BuildMesh()
    {
        GameObject go = this.gameObject;
        MarchingCube.GetMesh(ref go, ref material, true);

        /*  vertex 8 (0-7)
              E4-------------F5         7654-3210
              |               |         HGFE-DCBA
              |               |
        H7-------------G6     |
        |     |         |     |
        |     |         |     |
        |     A0--------|----B1  
        |               |
        |               |
        D3-------------C2               */

        vertices.Clear();
        triangles.Clear();
        uv.Clear();

        for (int z = 0; z < GridSize.z; z++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int x = 0; x < GridSize.x; x++)
                {
                    cell.p[0] = p[x, y, z + 1];         //A0
                    cell.p[1] = p[x + 1, y, z + 1];     //B1
                    cell.p[2] = p[x + 1, y, z];         //C2
                    cell.p[3] = p[x, y, z];             //D3
                    cell.p[4] = p[x, y + 1, z + 1];     //E4
                    cell.p[5] = p[x + 1, y + 1, z + 1]; //F5
                    cell.p[6] = p[x + 1, y + 1, z];     //G6
                    cell.p[7] = p[x, y + 1, z];         //H7
                    MarchingCube.IsoFaces(ref cell, SurfaceLevel);
                    BuildMeshCellData(ref cell);
                }
            }
        }

        Vector3[] av = vertices.ToArray();
        int[] at = triangles.ToArray();
        Vector2[] au = uv.ToArray();
        MarchingCube.SetMesh(ref go, ref av, ref at, ref au);
    }
    private void BuildMeshCellData(ref GridCell cell)
    {
        bool uvAlternate = false;
        for (int i = 0; i < cell.numtriangles; i++)
        {
            vertices.Add(cell.triangle[i].p[0]);
            vertices.Add(cell.triangle[i].p[1]);
            vertices.Add(cell.triangle[i].p[2]);

            //triangles.Add(vertices.Count - 1);  //this order changes side rendered
            //triangles.Add(vertices.Count - 2);
            //triangles.Add(vertices.Count - 3);

            triangles.Add(vertices.Count - 3);  
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);

            if (uvAlternate == true)
            {
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.D);
            }
            else
            {
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.C);
            }
            uvAlternate = !uvAlternate;
        }
    }
}
