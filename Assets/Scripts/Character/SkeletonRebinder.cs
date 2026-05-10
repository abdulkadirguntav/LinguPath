using System.Collections.Generic;
using UnityEngine;

// Add to the same GameObject that has the Animator.
// Assign the Root bone (e.g. "Root" or "Hips") to Skeleton Root in the Inspector.
public class SkeletonRebinder : MonoBehaviour
{
    [Tooltip("The top-most bone of the skeleton (Root or Hips)")]
    [SerializeField] private Transform skeletonRoot;

    void Awake()
    {
        if (skeletonRoot == null)
        {
            Debug.LogError("SkeletonRebinder: Skeleton Root not assigned!");
            return;
        }

        // Collect ONLY bones from the skeleton hierarchy (not mesh variation objects)
        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        boneMap[skeletonRoot.name] = skeletonRoot;
        foreach (Transform bone in skeletonRoot.GetComponentsInChildren<Transform>(true))
        {
            if (!boneMap.ContainsKey(bone.name))
                boneMap[bone.name] = bone;
        }

        int reboundCount = 0;

        foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            bool changed = false;

            Transform[] newBones = new Transform[smr.bones.Length];
            for (int i = 0; i < smr.bones.Length; i++)
            {
                if (smr.bones[i] != null && boneMap.TryGetValue(smr.bones[i].name, out Transform mapped))
                {
                    if (mapped != smr.bones[i]) changed = true;
                    newBones[i] = mapped;
                }
                else
                {
                    newBones[i] = smr.bones[i];
                }
            }
            smr.bones = newBones;

            if (smr.rootBone != null && boneMap.TryGetValue(smr.rootBone.name, out Transform newRoot))
            {
                if (newRoot != smr.rootBone) changed = true;
                smr.rootBone = newRoot;
            }

            if (changed) reboundCount++;
        }

        Debug.Log($"SkeletonRebinder: {reboundCount} mesh(es) rebound on '{name}'.");
    }
}
