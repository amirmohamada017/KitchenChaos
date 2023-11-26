using System.Collections.Generic;
using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour
{
    [SerializeField] private PlatesCounter platesCounter;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;

    private List<GameObject> _plateVisualGameObjectList;

    private void Awake()
    {
        _plateVisualGameObjectList = new List<GameObject>();
    }

    private void Start()
    {
        platesCounter.OnPlateSpawned += PlatesCounter_OnPlatesSpawned;
        platesCounter.OnPlateRemoved += PlatesCounter_OnPlatesRemoved;
    }

    private void PlatesCounter_OnPlatesSpawned(object sender, System.EventArgs e)
    {
        var plateVisualTransform = Instantiate(plateVisualPrefab, counterTopPoint);

        const float plateOffsetY = .1f;
        plateVisualTransform.localPosition = new Vector3(0, plateOffsetY * _plateVisualGameObjectList.Count, 0);
        
        _plateVisualGameObjectList.Add(plateVisualTransform.gameObject);
    }

    private void PlatesCounter_OnPlatesRemoved(object sender, System.EventArgs e)
    {
        var plateGameObject = _plateVisualGameObjectList[^1];
        _plateVisualGameObjectList.Remove(plateGameObject);
        Destroy(plateGameObject);
    }
}
