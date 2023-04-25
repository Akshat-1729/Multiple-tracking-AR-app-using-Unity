using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultipleImageTracking : MonoBehaviour
{
    [SerializeField]
    private GameObject[] placeablePrefabs;

    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();

    private ARTrackedImageManager trackedImageManager;

    private void Awake()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();

        foreach (GameObject prefab in placeablePrefabs)
        {
            if (prefab == null)
            {
                continue;
            }

            GameObject newPrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            newPrefab.name = prefab.name;
            spawnedPrefabs.Add(prefab.name, newPrefab);
        }
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            spawnedPrefabs[trackedImage.referenceImage.name].SetActive(false);
        }
    }

    private void UpdateImage(ARTrackedImage trackedImage)
{
    string name = trackedImage.referenceImage.name;

    if (spawnedPrefabs.TryGetValue(name, out GameObject prefab))
    {
        Vector3 position = trackedImage.transform.position;
        prefab.transform.position = position;
        prefab.SetActive(true);

        // Check for user input
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                // Rotate the object based on touch delta position
                float rotationSpeed = 0.5f;
                float horizontalRotation = -touch.deltaPosition.x * rotationSpeed;
                float verticalRotation = touch.deltaPosition.y * rotationSpeed;
                prefab.transform.Rotate(verticalRotation, horizontalRotation, 0, Space.World);
            }
            else if (touch.phase == TouchPhase.Moved && touch.tapCount == 2)
            {
                // Zoom in/out based on touch delta position
                Vector3 scale = prefab.transform.localScale;
                scale.x += touch.deltaPosition.x * 0.01f;
                scale.y += touch.deltaPosition.y * 0.01f;
                scale.z += touch.deltaPosition.y * 0.01f;
                prefab.transform.localScale = scale;
            }
        }
    }

    foreach (var item in spawnedPrefabs)
    {
        if (item.Key != name)
        {
            item.Value.SetActive(false);
        }
    }
}
}