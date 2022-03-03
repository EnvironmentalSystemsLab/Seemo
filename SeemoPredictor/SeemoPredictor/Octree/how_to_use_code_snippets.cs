

/*

// make octree
PointOctree<Face> octree = new PointOctree<Face>((float)maxNodeSize, allGeometry[0].Center, (float)minNodeSize);

for (int i = 0; i < allGeometry.Count; i++)
{
    var f = allGeometry[i];
    octree.Add(f, f.Center);
}



....




internal static bool IsObstructed(PointOctree<Face> octree, Face f, Point3 rayDir, double maxNodeSize)
{
    var ray = new Ray(f.Center + (f.Normal * 0.01f), rayDir);
    var testGeo = octree.GetNearby(ray, (float)maxNodeSize);

    foreach (var g in testGeo)
    {

        Point3 ipt1;
        Point3 ipt2;
        bool i1 = false;
        bool i2 = false;

        i1 = Intersect.RayTriangle_MollerTrumbore(ray, g.VertexList[0], g.VertexList[1], g.VertexList[2], out ipt1);

        if (i1) { return i1; }

        if (g.IsQuad)
        {
            i2 = Intersect.RayTriangle_MollerTrumbore(ray, g.VertexList[0], g.VertexList[2], g.VertexList[3], out ipt2);
            if (i2) { return i2; }
        }
    }
    return false;
}
*/