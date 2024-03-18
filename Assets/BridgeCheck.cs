using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeCheck : MonoBehaviour
{
    private List<BridgeMaterialsSwitch> bridgesInProximity = new List<BridgeMaterialsSwitch>();
    private Coroutine checkCoroutine;

    void Start()
    {
        checkCoroutine = StartCoroutine(CheckForBridges());
    }

    private IEnumerator CheckForBridges()
    {
        while (true)
        {
            Collider[] hits = Physics.OverlapBox(transform.position, new Vector3(20,2,20));
            HashSet<BridgeMaterialsSwitch> currentBridges = new HashSet<BridgeMaterialsSwitch>();

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Bridge") || hit.CompareTag("Pillar"))
                {
                    BridgeMaterialsSwitch bridgeSwitch = hit.GetComponent<BridgeMaterialsSwitch>();
                    if (bridgeSwitch != null)
                    {
                        currentBridges.Add(bridgeSwitch);
                        if (!bridgesInProximity.Contains(bridgeSwitch))
                        {
                            bridgeSwitch.SwitchToTransparent();
                        }
                    }
                }
            }

            for (var i = 0; i < bridgesInProximity.Count; i++)
            {
                var bridge = bridgesInProximity[i];
                if (bridge == null)
                {
                    bridgesInProximity.RemoveAt(i);
                    i--;
                    continue;
                }
                
                if (!currentBridges.Contains(bridge))
                {
                    bridge.SwitchToOpaque();
                }
            }

            bridgesInProximity = new List<BridgeMaterialsSwitch>(currentBridges);

            yield return new WaitForSeconds(0.3f);
        }
    }

    void OnDestroy()
    {
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
        }
    }
}