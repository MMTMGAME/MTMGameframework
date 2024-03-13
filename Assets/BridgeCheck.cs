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
            Collider[] hits = Physics.OverlapSphere(transform.position, 20f);
            HashSet<BridgeMaterialsSwitch> currentBridges = new HashSet<BridgeMaterialsSwitch>();

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Bridge"))
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

            foreach (var bridge in bridgesInProximity)
            {
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