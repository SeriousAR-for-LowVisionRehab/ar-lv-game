using Microsoft.MixedReality.WorldLocking.Core;
using UnityEngine;

/// <summary>
/// MyWorldLockingHandler class deals with persistence of anchors in the game, between sessions.
/// source: inspired by the WLT examples and samples.
/// </summary>
public class MyWorldLockingHandler : MonoBehaviour
{
    // Use current instance of WorldLockingManager.
    private WorldLockingManager worldLockingManager { get { return WorldLockingManager.GetInstance(); } }


}
