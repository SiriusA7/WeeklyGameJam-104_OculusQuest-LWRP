using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshCombiner : MonoBehaviour
{
    public GameObject sourceParent;
    public MeshFilter destinationMeshFilter;

    public bool advancedMerge;
    public bool basicMerge;

    void Update()
    {
        if (advancedMerge)
        {
            destinationMeshFilter.sharedMesh = AdvancedMerge(sourceParent);
            sourceParent.SetActive(false);
        }
        advancedMerge = false;
        if (basicMerge)
        {
            BasicMerge();
        }
        basicMerge = false;
    }


    public void BasicMerge()
    {
        Quaternion oldRotation = sourceParent.transform.rotation;
        Vector3 oldPosition = sourceParent.transform.position;

        sourceParent.transform.rotation = Quaternion.identity;
        sourceParent.transform.position = Vector3.zero;

        MeshFilter[] filters = sourceParent.GetComponentsInChildren<MeshFilter>();

        Debug.Log(name + " is combining " + filters.Length + " meshes.");

        Mesh finalMesh = new Mesh();

        CombineInstance[] combiners = new CombineInstance[filters.Length];

        for (int i = 0; i < filters.Length; i++)
        {
            if (filters[i].transform == sourceParent.transform)
            {
                continue;
            }
            combiners[i].subMeshIndex = 0;
            combiners[i].mesh = filters[i].sharedMesh;
            combiners[i].transform = filters[i].transform.localToWorldMatrix;
        }

        finalMesh.CombineMeshes(combiners);

        destinationMeshFilter.GetComponent<MeshFilter>().sharedMesh = finalMesh;

        sourceParent.transform.rotation = oldRotation;
        sourceParent.transform.position = oldPosition;

        for (int i = 0; i < transform.childCount; i++)
        {
            sourceParent.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public Mesh AdvancedMerge(GameObject sourceParent)
    {
        //get alll the children and ourselves
        MeshFilter[] filters = sourceParent.GetComponentsInChildren<MeshFilter>(false);

        //a big list of all the meshes in our children
        List<Material> materials = new List<Material>();
        MeshRenderer[] renderers = sourceParent.GetComponentsInChildren<MeshRenderer>(false);
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.transform == sourceParent.transform)
            {
                //if it matches the transform of ourself, skip to prevent duplicate
                continue;
            }
            Material[] localMats = renderer.sharedMaterials;
            foreach (Material localMat in localMats)
            {
                if (!materials.Contains(localMat))
                {
                    materials.Add(localMat);
                }
            }
        }

        //a mesh for each material
        List<Mesh> submeshes = new List<Mesh>();
        foreach (Material material in materials)
        {
            //a combiner for each submesh mapped correctly
            List<CombineInstance> combiners = new List<CombineInstance>();
            foreach (MeshFilter filter in filters)
            {
                if (filter.transform == sourceParent.transform)
                {
                    continue;
                }
                //the filter doesnt know what materials are involved, get the renderer
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null)
                {
                    Debug.LogError(filter.name + " has no MeshRenderer");
                    continue;
                }

                //see if their materials are the ones we want right now
                Material[] localMaterials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < localMaterials.Length; materialIndex++)
                {
                    if (localMaterials [materialIndex] != material)
                    {
                        continue;
                    }
                    //this submesh is the material we're looking for right now
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = filter.sharedMesh;
                    ci.subMeshIndex = materialIndex;
                    ci.transform = filter.transform.localToWorldMatrix;
                    combiners.Add(ci);
                }
            }

            //flatten into one mesh
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combiners.ToArray(), true);
            submeshes.Add(mesh);
        }

        // combine all material specific meshes as independent submeshes
        List<CombineInstance> finalCombiners = new List<CombineInstance>();
        foreach (Mesh mesh in submeshes)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = mesh;
            ci.subMeshIndex = 0;
            ci.transform = Matrix4x4.identity;
            finalCombiners.Add(ci);
        }
        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(finalCombiners.ToArray(), false);
        Debug.Log("Final mesh has " + submeshes.Count + " materials.");
        return finalMesh;
    }
}
